namespace iM3Helpdesk.Application.DTOs.Integrations;

public class SlackWebhookDto
{
  public string? Command { get; set; }
  public string? Text { get; set; }
  public string? EventType { get; set; }
  public string? Channel { get; set; }
  public string? UserId { get; set; }
}

public class SlackNotifyDto
{
  public string OrgId { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public string TicketTitle { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
}
