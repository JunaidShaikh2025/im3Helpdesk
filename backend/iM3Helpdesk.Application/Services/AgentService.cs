using System.Net.Mail;
using System.Text;
using ClosedXML.Excel;
using iM3Helpdesk.Application.Common;
using iM3Helpdesk.Application.Contracts.Persistence;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.DTOs.Agents;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace iM3Helpdesk.Application.Services;

public class AgentService : IAgentService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenantService;
    private readonly IAuthEmailService _emailService;
    private readonly ILogger<AgentService> _logger;

    public AgentService(
        IUnitOfWork uow,
        ITenantService tenantService,
        IAuthEmailService emailService,
        ILogger<AgentService> logger)
    {
        _uow = uow;
        _tenantService = tenantService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AgentListItemDto>> GetAllAsync(Guid? organizationIdOverride, bool isSuperAdmin, CancellationToken ct = default)
    {
        var users = await _uow.Users.GetAgentsAsync(organizationIdOverride, isSuperAdmin, ct);
        return users.Select(u => new AgentListItemDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Role = u.Role.ToString(),
            IsEmailVerified = u.IsEmailVerified,
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt,
            IsActive = !u.LockedUntil.HasValue || u.LockedUntil < DateTime.UtcNow
        }).ToList();
    }

    public async Task<ServiceResult<AgentDetailDto>> GetByIdAsync(Guid id, Guid? organizationIdOverride, bool isSuperAdmin, CancellationToken ct = default)
    {
        var agent = await _uow.Users.GetAgentByIdAsync(id, organizationIdOverride, isSuperAdmin, ct);
        if (agent == null)
            return ServiceResult<AgentDetailDto>.NotFound("Agent not found");

        return ServiceResult<AgentDetailDto>.Success(new AgentDetailDto
        {
            Id = agent.Id,
            FullName = agent.FullName,
            Email = agent.Email,
            PhoneNumber = agent.PhoneNumber,
            Role = agent.Role.ToString(),
            Signature = agent.Signature,
            PhotoUrl = agent.PhotoUrl,
            Department = agent.Department,
            Designation = agent.Designation,
            Location = agent.Location,
            DateOfJoining = agent.DateOfJoining,
            IsEmailVerified = agent.IsEmailVerified,
            LastLoginAt = agent.LastLoginAt,
            CreatedAt = agent.CreatedAt
        });
    }

    public async Task<Guid?> ResolveOrganizationIdAsync(Guid? contextOrgId, string? claimOrgId, Guid? userId, CancellationToken ct = default)
    {
        if (contextOrgId.HasValue)
            return contextOrgId;

        if (!string.IsNullOrWhiteSpace(claimOrgId) && Guid.TryParse(claimOrgId, out var parsedClaimId))
            return parsedClaimId;

        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            var userOrgId = await _uow.Users.GetOrganizationIdByUserIdAsync(userId.Value, ct);
            if (userOrgId.HasValue)
                return userOrgId.Value;
        }

        return null;
    }

    public async Task<ServiceResult<InviteAgentResultDto>> InviteAgentAsync(InviteAgentDto dto, CancellationToken ct = default)
    {
        var isTaken = await _uow.Users.IsEmailTakenAsync(dto.Email, null, ct);
        if (isTaken)
            return ServiceResult<InviteAgentResultDto>.BadRequest("Email already registered");

        var orgId = _tenantService.OrganizationId;
        if (!orgId.HasValue)
            return ServiceResult<InviteAgentResultDto>.BadRequest("Organization context is missing");

        var tempPassword = GenerateTempPassword();
        var agent = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber ?? "",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
            Role = ParseRole(dto.Role),
            OrganizationId = orgId.Value,
            IsEmailVerified = true,
            Signature = dto.Signature ?? "",
            PhotoUrl = dto.PhotoUrl ?? ""
        };

        await _uow.Users.AddAsync(agent, ct);

        if (dto.GroupIds?.Any() == true)
        {
            var members = dto.GroupIds.Select(groupId => new AgentGroupMember
            {
                AgentGroupId = groupId,
                UserId = agent.Id
            }).ToList();

            await _uow.Users.AddAgentGroupMembersAsync(members, ct);
        }

        await _uow.SaveChangesAsync(ct);

        var org = await _uow.Organizations.GetByIdAsync(orgId.Value, asNoTracking: true, ct);

        try
        {
            await _emailService.SendAgentInviteAsync(
                agent.Email,
                agent.FullName,
                org?.Name ?? "Your Company",
                tempPassword,
                org?.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send agent invite email to {Email}", agent.Email);
        }

        return ServiceResult<InviteAgentResultDto>.Success(new InviteAgentResultDto
        {
            Message = "Agent invited successfully",
            TempPassword = tempPassword,
            AgentId = agent.Id
        });
    }

    public async Task<ServiceResult> UpdateAgentAsync(Guid id, UpdateAgentDto dto, CancellationToken ct = default)
    {
        var agent = await _uow.Users.GetByIdAsync(id, asNoTracking: false, ct);
        if (agent == null || (_tenantService.OrganizationId.HasValue && agent.OrganizationId != _tenantService.OrganizationId.Value))
            return ServiceResult.NotFound("Agent not found");

        if (!string.IsNullOrEmpty(dto.FullName))
            agent.FullName = dto.FullName;

        if (!string.IsNullOrEmpty(dto.Role))
            agent.Role = ParseRole(dto.Role);

        if (dto.Signature != null)
            agent.Signature = dto.Signature;

        if (dto.PhotoUrl != null)
            agent.PhotoUrl = dto.PhotoUrl;

        await _uow.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<ToggleActiveResultDto>> ToggleActiveAsync(Guid id, CancellationToken ct = default)
    {
        var agent = await _uow.Users.GetByIdAsync(id, asNoTracking: false, ct);
        if (agent == null)
            return ServiceResult<ToggleActiveResultDto>.NotFound("Agent not found");

        if (agent.LockedUntil.HasValue && agent.LockedUntil > DateTime.UtcNow)
        {
            agent.LockedUntil = null;
            agent.FailedLoginAttempts = 0;
        }
        else
        {
            agent.LockedUntil = DateTime.UtcNow.AddYears(100);
        }

        await _uow.SaveChangesAsync(ct);

        var isActive = !agent.LockedUntil.HasValue || agent.LockedUntil < DateTime.UtcNow;
        return ServiceResult<ToggleActiveResultDto>.Success(new ToggleActiveResultDto
        {
            Message = "Agent status updated",
            IsActive = isActive
        });
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var agent = await _uow.Users.GetByIdAsync(id, asNoTracking: false, ct);
        if (agent == null || agent.Role == UserRole.SuperAdmin)
            return ServiceResult.NotFound("Agent not found");

        _uow.Users.Remove(agent);
        await _uow.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<BulkImportUsersResponse>> BulkImportAsync(Stream fileStream, string fileExtension, bool sendInviteEmail, CancellationToken ct = default)
    {
        if (!_tenantService.OrganizationId.HasValue)
            return ServiceResult<BulkImportUsersResponse>.BadRequest("Organization context is missing.");

        var ext = fileExtension.ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".csv")
            return ServiceResult<BulkImportUsersResponse>.BadRequest("Only .xlsx and .csv files are supported.");

        List<BulkImportRawRow> rawRows;
        try
        {
            rawRows = ext == ".xlsx"
                ? ReadRowsFromExcel(fileStream)
                : ReadRowsFromCsv(fileStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse import file");
            return ServiceResult<BulkImportUsersResponse>.BadRequest("Unable to parse uploaded file. Please check template format.");
        }

        if (rawRows.Count == 0)
            return ServiceResult<BulkImportUsersResponse>.BadRequest("No data rows found in uploaded file.");

        var normalizedEmails = rawRows
            .Select(r => (r.Email ?? string.Empty).Trim().ToLowerInvariant())
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Distinct()
            .ToList();

        var existingEmailList = await _uow.Users.GetExistingEmailsAsync(normalizedEmails, ct);
        var existingEmails = new HashSet<string>(existingEmailList);

        var orgId = _tenantService.OrganizationId.Value;
        var org = await _uow.Organizations.GetByIdAsync(orgId, asNoTracking: true, ct);

        var fileSeenEmails = new HashSet<string>();
        var newUsers = new List<(User user, string tempPassword, int row)>();
        var results = new List<BulkImportRowResult>();

        foreach (var row in rawRows)
        {
            var fullName = (row.FullName ?? string.Empty).Trim();
            var email = (row.Email ?? string.Empty).Trim();
            var roleText = (row.Role ?? string.Empty).Trim();
            var phone = (row.PhoneNumber ?? string.Empty).Trim();
            var emailNorm = email.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                results.Add(BulkImportRowResult.Fail(row.RowNumber, email, roleText, "FullName is required."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                results.Add(BulkImportRowResult.Fail(row.RowNumber, email, roleText, "Email is required."));
                continue;
            }

            if (!IsValidEmail(email))
            {
                results.Add(BulkImportRowResult.Fail(row.RowNumber, email, roleText, "Invalid email format."));
                continue;
            }

            if (fileSeenEmails.Contains(emailNorm))
            {
                results.Add(BulkImportRowResult.Fail(row.RowNumber, email, roleText, "Duplicate email in upload file."));
                continue;
            }
            fileSeenEmails.Add(emailNorm);

            if (existingEmails.Contains(emailNorm))
            {
                results.Add(BulkImportRowResult.Fail(row.RowNumber, email, roleText, "Email already exists."));
                continue;
            }

            var parsedRole = ParseBulkRole(roleText);
            if (!parsedRole.HasValue)
            {
                results.Add(BulkImportRowResult.Fail(row.RowNumber, email, roleText, "Role must be Agent or Administrator."));
                continue;
            }

            var tempPassword = GenerateTempPassword();
            var user = new User
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
                Role = parsedRole.Value,
                OrganizationId = orgId,
                IsEmailVerified = true,
                Signature = string.Empty,
                PhotoUrl = string.Empty
            };

            newUsers.Add((user, tempPassword, row.RowNumber));
            await _uow.Users.AddAsync(user, ct);
            existingEmails.Add(emailNorm);
        }

        if (newUsers.Count > 0)
            await _uow.SaveChangesAsync(ct);

        foreach (var created in newUsers)
        {
            var roleLabel = created.user.Role.ToString();
            var emailSent = false;
            if (sendInviteEmail)
            {
                try
                {
                    var html = $@"<p>Hello {created.user.FullName},</p>
<p>Your {roleLabel} account has been created for {(org?.Name ?? "DeskMate")}.</p>
<p><strong>Email:</strong> {created.user.Email}<br/>
<strong>Temporary Password:</strong> {created.tempPassword}</p>
<p>Please login and change your password immediately.</p>";

                    await _emailService.SendAgentInviteAsync(
                        created.user.Email,
                        created.user.FullName,
                        org?.Name ?? "DeskMate",
                        created.tempPassword,
                        orgId);
                    emailSent = true;
                }
                catch
                {
                    emailSent = false;
                }
            }

            results.Add(BulkImportRowResult.Ok(
                created.row,
                created.user.Email,
                roleLabel,
                created.tempPassword,
                emailSent));
        }

        var createdCount = results.Count(x => x.Success);
        var failedCount = results.Count(x => !x.Success);

        return ServiceResult<BulkImportUsersResponse>.Success(new BulkImportUsersResponse
        {
            TotalRows = rawRows.Count,
            CreatedCount = createdCount,
            FailedCount = failedCount,
            Results = results.OrderBy(r => r.RowNumber).ToList()
        });
    }

    private static string GenerateTempPassword()
    {
        return "Agent@" + Guid.NewGuid().ToString()[..6];
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static UserRole? ParseBulkRole(string? role)
    {
        var normalized = (role ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            "administrator" => UserRole.CompanyAdmin,
            "companyadmin" => UserRole.CompanyAdmin,
            "company admin" => UserRole.CompanyAdmin,
            "agent" => UserRole.Agent,
            _ => null
        };
    }

    private static UserRole ParseRole(string? role)
    {
        return role switch
        {
            "Administrator" => UserRole.CompanyAdmin,
            "Agent" => UserRole.Agent,
            _ => UserRole.Agent
        };
    }

    private static List<BulkImportRawRow> ReadRowsFromExcel(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.First();
        var rows = new List<BulkImportRawRow>();

        var header = sheet.Row(1);
        var headers = new Dictionary<string, int>();
        for (var c = 1; c <= header.LastCellUsed()?.Address.ColumnNumber; c++)
        {
            var key = NormalizeHeader(header.Cell(c).GetString());
            if (!string.IsNullOrWhiteSpace(key) && !headers.ContainsKey(key))
                headers[key] = c;
        }

        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var r = 2; r <= lastRow; r++)
        {
            var row = sheet.Row(r);
            var fullName = GetCellByHeader(row, headers, "fullname", "name");
            var email = GetCellByHeader(row, headers, "email", "emailaddress");
            var role = GetCellByHeader(row, headers, "role", "usertype");
            var phone = GetCellByHeader(row, headers, "phone", "phonenumber", "mobile");

            if (string.IsNullOrWhiteSpace(fullName) && string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(role) && string.IsNullOrWhiteSpace(phone))
                continue;

            rows.Add(new BulkImportRawRow
            {
                RowNumber = r,
                FullName = fullName,
                Email = email,
                Role = role,
                PhoneNumber = phone
            });
        }

        return rows;
    }

    private static List<BulkImportRawRow> ReadRowsFromCsv(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, true);
        var lines = new List<string>();
        while (!reader.EndOfStream)
            lines.Add(reader.ReadLine() ?? string.Empty);

        if (lines.Count == 0)
            return new List<BulkImportRawRow>();

        var headersRaw = ParseCsvLine(lines[0]);
        var headers = headersRaw
            .Select((h, i) => new { Key = NormalizeHeader(h), Index = i })
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .GroupBy(x => x.Key)
            .ToDictionary(g => g.Key, g => g.First().Index);

        var rows = new List<BulkImportRawRow>();
        for (var i = 1; i < lines.Count; i++)
        {
            var cols = ParseCsvLine(lines[i]);
            var fullName = GetCsvByHeader(cols, headers, "fullname", "name");
            var email = GetCsvByHeader(cols, headers, "email", "emailaddress");
            var role = GetCsvByHeader(cols, headers, "role", "usertype");
            var phone = GetCsvByHeader(cols, headers, "phone", "phonenumber", "mobile");

            if (string.IsNullOrWhiteSpace(fullName) && string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(role) && string.IsNullOrWhiteSpace(phone))
                continue;

            rows.Add(new BulkImportRawRow
            {
                RowNumber = i + 1,
                FullName = fullName,
                Email = email,
                Role = role,
                PhoneNumber = phone
            });
        }

        return rows;
    }

    private static string NormalizeHeader(string raw)
    {
        return (raw ?? string.Empty)
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty);
    }

    private static string GetCellByHeader(IXLRow row, Dictionary<string, int> headers, params string[] names)
    {
        foreach (var name in names)
        {
            if (headers.TryGetValue(name, out var col))
                return row.Cell(col).GetString().Trim();
        }
        return string.Empty;
    }

    private static string GetCsvByHeader(List<string> cols, Dictionary<string, int> headers, params string[] names)
    {
        foreach (var name in names)
        {
            if (headers.TryGetValue(name, out var index) && index < cols.Count)
                return (cols[index] ?? string.Empty).Trim();
        }
        return string.Empty;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        if (line == null)
        {
            result.Add(string.Empty);
            return result;
        }

        var sb = new StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (ch == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(ch);
            }
        }

        result.Add(sb.ToString());
        return result;
    }
}
