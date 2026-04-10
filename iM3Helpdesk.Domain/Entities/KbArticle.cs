using iM3Helpdesk.Domain.Interfaces;

namespace iM3Helpdesk.Domain.Entities;

public class KbArticle : IMustHaveTenant
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Title { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
  public string Category { get; set; } = string.Empty;
  public string Tags { get; set; } = string.Empty;
  public bool IsPublished { get; set; } = false;
  public int ViewCount { get; set; } = 0;
  public Guid OrganizationId { get; set; }
  public Guid CreatedByUserId { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; set; }

  public User? CreatedBy { get; set; }
}
