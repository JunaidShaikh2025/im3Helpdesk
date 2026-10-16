namespace iM3Helpdesk.Domain.Entities;

public class UserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string TokenHash { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceInfo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
}

