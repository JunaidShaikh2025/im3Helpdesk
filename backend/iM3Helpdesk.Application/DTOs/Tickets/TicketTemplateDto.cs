namespace iM3Helpdesk.Application.DTOs.Tickets;

public class TicketTemplateDto
{
  public string Name { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Category { get; set; } = "General";
  public string Priority { get; set; } = "Medium";
  public string TicketType { get; set; } = "Support";
  public string Status { get; set; } = "Open";
  public string? Tags { get; set; }
}
