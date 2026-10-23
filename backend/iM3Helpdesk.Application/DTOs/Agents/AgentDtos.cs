using System.Text.Json.Serialization;

namespace iM3Helpdesk.Application.DTOs.Agents;

public class AgentListItemDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
}

public class AgentDetailDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Signature { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public string? Location { get; set; }
    public DateOnly? DateOfJoining { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InviteAgentDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = "Agent";
    public string? Signature { get; set; }
    public string? PhotoUrl { get; set; }
    public List<Guid>? GroupIds { get; set; }
}

public class InviteAgentResultDto
{
    public string Message { get; set; } = string.Empty;
    public string TempPassword { get; set; } = string.Empty;
    public Guid AgentId { get; set; }
}

public class UpdateAgentDto
{
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public string? Signature { get; set; }
    public string? PhotoUrl { get; set; }
}

public class ToggleActiveResultDto
{
    public string Message { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class BulkImportUsersResponse
{
    public int TotalRows { get; set; }
    public int CreatedCount { get; set; }
    public int FailedCount { get; set; }
    public List<BulkImportRowResult> Results { get; set; } = new();
}

public class BulkImportRawRow
{
    public int RowNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class BulkImportRowResult
{
    public int RowNumber { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TempPassword { get; set; }
    public bool? InviteEmailSent { get; set; }

    public static BulkImportRowResult Fail(int rowNumber, string email, string role, string message)
    {
        return new BulkImportRowResult
        {
            RowNumber = rowNumber,
            Email = email,
            Role = role,
            Success = false,
            Message = message
        };
    }

    public static BulkImportRowResult Ok(int rowNumber, string email, string role, string tempPassword, bool emailSent)
    {
        return new BulkImportRowResult
        {
            RowNumber = rowNumber,
            Email = email,
            Role = role,
            Success = true,
            Message = "Created",
            TempPassword = tempPassword,
            InviteEmailSent = emailSent
        };
    }
}
