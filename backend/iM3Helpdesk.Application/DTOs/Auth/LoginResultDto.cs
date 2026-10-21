namespace iM3Helpdesk.Application.DTOs.Auth;

public class LoginResultDto
{
    public bool RequiresOtp { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public bool IsFirstLogin { get; set; }
    public UserSummaryDto? User { get; set; }
}

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsTwoFactorEnabled { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public string? PhotoUrl { get; set; }
}

public class RefreshTokenResultDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserSummaryDto? User { get; set; }
}
