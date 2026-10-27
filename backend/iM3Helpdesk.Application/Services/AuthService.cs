using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using iM3Helpdesk.Application.Common;
using iM3Helpdesk.Application.Contracts.Persistence;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.DTOs.Auth;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace iM3Helpdesk.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _configuration;
    private readonly IAuthEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork uow,
        IConfiguration configuration,
        IAuthEmailService emailService,
        IOtpService otpService,
        ILogger<AuthService> logger)
    {
        _uow = uow;
        _configuration = configuration;
        _emailService = emailService;
        _otpService = otpService;
        _logger = logger;
    }

    public async Task<ServiceResult<LoginResultDto>> LoginAsync(
        LoginDto dto, string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByEmailAsync(dto.Email, includeOrganization: true, ct: ct);
        if (user == null)
            return ServiceResult<LoginResultDto>.Unauthorized("Invalid email or password");

        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow.AddYears(50))
        {
            return ServiceResult<LoginResultDto>.Unauthorized("Your account has been deactivated. Please contact your administrator.");
        }

        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
        {
            var mins = (int)(user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes + 1;
            return ServiceResult<LoginResultDto>.Unauthorized($"Account locked. Try again in {mins} minutes.");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            user.RecordFailedLogin(5, TimeSpan.FromMinutes(30));
            await _uow.SaveChangesAsync(ct);
            if (user.IsLockedOut())
            {
                return ServiceResult<LoginResultDto>.Unauthorized("Account locked for 30 minutes due to multiple failed attempts.");
            }
            return ServiceResult<LoginResultDto>.Unauthorized($"Invalid email or password. {5 - user.FailedLoginAttempts} attempts remaining.");
        }

        if (!user.IsEmailVerified)
            return ServiceResult<LoginResultDto>.Unauthorized("Please verify your email first");

        if (user.OrganizationId.HasValue)
        {
            var org = await _uow.Organizations.GetByIdAsync(user.OrganizationId.Value, asNoTracking: true, ct: ct);
            if (org != null && !org.IsActive)
                return ServiceResult<LoginResultDto>.Unauthorized("Your organization has been deactivated. Please contact support.");
        }

        user.RecordSuccessfulLogin();

        if (dto.LoginWithOtp || user.IsTwoFactorEnabled)
        {
            await _uow.SaveChangesAsync(ct);
            await _otpService.SendOtpAsync(dto.Email);
            return ServiceResult<LoginResultDto>.Success(new LoginResultDto
            {
                RequiresOtp = true,
                IsTwoFactorEnabled = user.IsTwoFactorEnabled,
                Message = "OTP sent to your email. Please verify."
            });
        }

        var isFirstLogin = user.LastLoginAt == null;
        var (token, refreshToken) = await CreateUserSessionAsync(user, ipAddress, userAgent, ct);

        return ServiceResult<LoginResultDto>.Success(new LoginResultDto
        {
            RequiresOtp = false,
            Token = token,
            RefreshToken = refreshToken,
            IsFirstLogin = isFirstLogin,
            User = MapToSummary(user)
        });
    }

    public async Task<ServiceResult<LoginResultDto>> VerifyOtpAsync(
        string email, string otp, string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
            return ServiceResult<LoginResultDto>.BadRequest("Email and OTP are required");

        var isValid = await _otpService.VerifyOtpAsync(email, otp);
        if (!isValid)
            return ServiceResult<LoginResultDto>.Unauthorized("Invalid or expired OTP. Please try again.");

        var user = await _uow.Users.GetByEmailAsync(email, includeOrganization: true, ct: ct);
        if (user == null)
            return ServiceResult<LoginResultDto>.Unauthorized("User not found");

        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow.AddYears(50))
            return ServiceResult<LoginResultDto>.Unauthorized("Your account has been deactivated. Please contact your administrator.");

        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            return ServiceResult<LoginResultDto>.Unauthorized("Account is temporarily locked. Please try again later.");

        var isFirstLogin = user.LastLoginAt == null;
        var (token, refreshToken) = await CreateUserSessionAsync(user, ipAddress, userAgent, ct);

        return ServiceResult<LoginResultDto>.Success(new LoginResultDto
        {
            RequiresOtp = false,
            Token = token,
            RefreshToken = refreshToken,
            IsFirstLogin = isFirstLogin,
            User = MapToSummary(user)
        });
    }

    public async Task<ServiceResult<string>> RegisterSuperAdminAsync(
        RegisterDto dto, CancellationToken ct = default)
    {
        if (dto.Password != dto.ConfirmPassword)
            return ServiceResult<string>.BadRequest("Passwords do not match");

        if (await _uow.Users.IsEmailTakenAsync(dto.Email, ct: ct))
            return ServiceResult<string>.BadRequest("Email already registered");

        var organization = new Organization
        {
            Name = dto.CompanyName,
            Slug = dto.CompanyName.ToLower().Replace(" ", "-").Replace(".", "") + "-" + Guid.NewGuid().ToString()[..6],
            TrialEndsAt = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };
        await _uow.Organizations.AddAsync(organization, ct);

        var verificationToken = Guid.NewGuid().ToString();
        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.CompanyAdmin,
            OrganizationId = organization.Id,
            IsEmailVerified = false,
            EmailVerificationToken = verificationToken
        };
        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        try
        {
            await _emailService.SendEmailVerificationAsync(
                user.Email, user.FullName, verificationToken, dto.CompanyName, organization.Id);
            await _emailService.SendWelcomeEmailAsync(
                user.Email, user.FullName, dto.CompanyName, organization.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send registration emails to {Email}", user.Email);
        }

        return ServiceResult<string>.Success("Account created! Please check your email to verify.");
    }

    public async Task<ServiceResult<string>> VerifyEmailAsync(string token, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByVerificationTokenAsync(token, ct);
        if (user == null)
            return ServiceResult<string>.BadRequest("Invalid or expired token");

        user.VerifyEmail();
        await _uow.SaveChangesAsync(ct);
        return ServiceResult<string>.Success("Email verified successfully");
    }

    public async Task<ServiceResult<string>> ResendVerificationAsync(string email, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByEmailAsync(email, includeOrganization: true, ct: ct);
        if (user == null)
            return ServiceResult<string>.BadRequest("User not found");

        if (user.IsEmailVerified)
            return ServiceResult<string>.BadRequest("Email is already verified");

        user.EmailVerificationToken = Guid.NewGuid().ToString();
        await _uow.SaveChangesAsync(ct);

        try
        {
            await _emailService.SendEmailVerificationAsync(
                user.Email, user.FullName, user.EmailVerificationToken, user.Organization?.Name ?? "iM3Helpdesk", user.OrganizationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend verification email to {Email}", user.Email);
        }

        return ServiceResult<string>.Success("Verification email resent.");
    }

    public async Task<ServiceResult<RefreshTokenResultDto>> RefreshTokenAsync(
        string? incomingRefreshToken, string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(incomingRefreshToken))
            return ServiceResult<RefreshTokenResultDto>.Unauthorized("Invalid or expired refresh token");

        var presentedHash = HashRefreshToken(incomingRefreshToken);
        var existingSession = await _uow.Users.GetSessionByTokenHashAsync(presentedHash, ct);

        if (existingSession == null)
        {
            var legacyUser = await _uow.Users.GetByRefreshTokenHashAsync(presentedHash, ct);
            if (legacyUser == null)
                return ServiceResult<RefreshTokenResultDto>.Unauthorized("Invalid or expired refresh token");

            var (legacyToken, legacyRefreshToken) = await CreateUserSessionAsync(legacyUser, ipAddress, userAgent, ct);
            return ServiceResult<RefreshTokenResultDto>.Success(new RefreshTokenResultDto
            {
                Token = legacyToken,
                RefreshToken = legacyRefreshToken,
                User = MapToSummary(legacyUser)
            });
        }

        if (existingSession.IsRevoked)
        {
            var familySessions = await _uow.Users.GetFamilySessionsAsync(existingSession.FamilyId, ct);
            foreach (var s in familySessions)
            {
                s.IsRevoked = true;
                s.RevokedAt = DateTime.UtcNow;
            }
            await _uow.SaveChangesAsync(ct);
            return ServiceResult<RefreshTokenResultDto>.Unauthorized(
                "Security breach detected: Replay of revoked refresh token. All active sessions in this family have been terminated.");
        }

        if (existingSession.ExpiresAt <= DateTime.UtcNow)
        {
            existingSession.IsRevoked = true;
            existingSession.RevokedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync(ct);
            return ServiceResult<RefreshTokenResultDto>.Unauthorized("Refresh token has expired. Please log in again.");
        }

        var user = existingSession.User;
        if (user == null)
            return ServiceResult<RefreshTokenResultDto>.Unauthorized("User not found");

        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            return ServiceResult<RefreshTokenResultDto>.Unauthorized("Account is locked.");

        var newRefreshToken = GenerateRefreshToken();
        var newRefreshTokenHash = HashRefreshToken(newRefreshToken);

        existingSession.IsRevoked = true;
        existingSession.RevokedAt = DateTime.UtcNow;
        existingSession.ReplacedByTokenHash = newRefreshTokenHash;

        var newSession = new UserSession
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            FamilyId = existingSession.FamilyId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceInfo = existingSession.DeviceInfo,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false
        };

        await _uow.Users.AddSessionAsync(newSession, ct);
        user.RefreshToken = newRefreshTokenHash;
        user.RefreshTokenExpiresAt = newSession.ExpiresAt;
        await _uow.SaveChangesAsync(ct);

        var newToken = GenerateJwtToken(user);
        return ServiceResult<RefreshTokenResultDto>.Success(new RefreshTokenResultDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            User = MapToSummary(user)
        });
    }

    public async Task<ServiceResult> LogoutAsync(string? refreshToken, Guid? userId, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var tokenHash = HashRefreshToken(refreshToken);
            var session = await _uow.Users.GetSessionByTokenHashAsync(tokenHash, ct);
            if (session != null)
            {
                session.IsRevoked = true;
                session.RevokedAt = DateTime.UtcNow;
                await _uow.SaveChangesAsync(ct);
            }
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<string>> ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByEmailAsync(email, includeOrganization: true, ct: ct);
        if (user != null)
        {
            user.EmailVerificationToken = Guid.NewGuid().ToString();
            await _uow.SaveChangesAsync(ct);
            try
            {
                await _emailService.SendForgotPasswordAsync(
                    user.Email, user.FullName, user.EmailVerificationToken, organizationId: user.OrganizationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send forgot password email to {Email}", user.Email);
            }
        }

        return ServiceResult<string>.Success("If email exists, reset link has been sent.");
    }

    public async Task<ServiceResult<string>> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return ServiceResult<string>.BadRequest("Token and new password are required.");

        var user = await _uow.Users.GetByVerificationTokenAsync(dto.Token, ct);
        if (user == null)
            return ServiceResult<string>.BadRequest("Invalid or expired reset link. Please request a new one.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.EmailVerificationToken = null;
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        var activeSessions = await _uow.Users.GetActiveSessionsByUserIdAsync(user.Id, ct);
        foreach (var s in activeSessions)
        {
            s.IsRevoked = true;
            s.RevokedAt = DateTime.UtcNow;
        }

        await _uow.SaveChangesAsync(ct);
        return ServiceResult<string>.Success("Password reset successfully.");
    }

    public async Task<ServiceResult<UserSummaryDto>> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdWithOrgAsync(userId, ct);
        if (user == null)
            return ServiceResult<UserSummaryDto>.NotFound("User not found");

        return ServiceResult<UserSummaryDto>.Success(MapToSummary(user));
    }

    private async Task<(string AccessToken, string RefreshToken)> CreateUserSessionAsync(
        User user, string? ipAddress, string? userAgent, CancellationToken ct)
    {
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        var familyId = Guid.NewGuid();

        var session = new UserSession
        {
            UserId = user.Id,
            TokenHash = HashRefreshToken(refreshToken),
            FamilyId = familyId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceInfo = ResolveDeviceInfo(userAgent),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false
        };

        await _uow.Users.AddSessionAsync(session, ct);
        user.RefreshToken = HashRefreshToken(refreshToken);
        user.RefreshTokenExpiresAt = session.ExpiresAt;
        user.LastLoginAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);

        return (token, refreshToken);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("organizationId", user.OrganizationId?.ToString() ?? ""),
            new Claim("fullName", user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    private static string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }

    private static string ResolveDeviceInfo(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent)) return "Unknown";
        if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase)) return "Windows Desktop";
        if (userAgent.Contains("Macintosh", StringComparison.OrdinalIgnoreCase) || userAgent.Contains("Mac OS", StringComparison.OrdinalIgnoreCase)) return "macOS Desktop";
        if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase)) return "Android Device";
        if (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) || userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase)) return "iOS Device";
        if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase)) return "Linux Desktop";
        return "Web Browser";
    }

    private static UserSummaryDto MapToSummary(User user)
    {
        return new UserSummaryDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsTwoFactorEnabled = user.IsTwoFactorEnabled,
            OrganizationId = user.OrganizationId,
            OrganizationName = user.Organization?.Name,
            PhotoUrl = user.PhotoUrl
        };
    }
}
