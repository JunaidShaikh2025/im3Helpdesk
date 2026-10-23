using iM3Helpdesk.Domain.Enums;

namespace iM3Helpdesk.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public Guid? OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public Organization? Organization { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }
    public string? Signature { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Department { get; set; }
    public string? Location { get; set; }
    public string? Designation { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly? DateOfJoining { get; set; }
    public string? Gender { get; set; }
    public bool IsTwoFactorEnabled { get; set; } = false;

    // ── Domain Methods & Invariant Rules ────────────────────────────────
    public void RecordFailedLogin(int maxFailedAttempts = 5, TimeSpan? lockoutDuration = null)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxFailedAttempts)
        {
            LockedUntil = DateTime.UtcNow.Add(lockoutDuration ?? TimeSpan.FromMinutes(15));
            FailedLoginAttempts = 0;
        }
    }

    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
        LastLoginAt = DateTime.UtcNow;
    }

    public bool IsLockedOut(DateTime? now = null)
    {
        return LockedUntil.HasValue && LockedUntil.Value > (now ?? DateTime.UtcNow);
    }

    public void Unlock()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public void SetRefreshToken(string refreshToken, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token cannot be empty.", nameof(refreshToken));

        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = expiresAt;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        EmailVerificationToken = null;
    }

    public void UpdateProfile(string fullName, string? phoneNumber = null, string? department = null, string? designation = null, string? location = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));

        FullName = fullName.Trim();
        PhoneNumber = phoneNumber?.Trim();
        Department = department?.Trim();
        Designation = designation?.Trim();
        Location = location?.Trim();
    }
}

