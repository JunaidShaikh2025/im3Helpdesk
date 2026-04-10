using iM3Helpdesk.Domain.Interfaces;

namespace iM3Helpdesk.Domain.Entities;

public class TicketTemplate : IMustHaveTenant
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Name { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Category { get; set; } = string.Empty;
  public string Priority { get; set; } = "Medium";
  public Guid OrganizationId { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
