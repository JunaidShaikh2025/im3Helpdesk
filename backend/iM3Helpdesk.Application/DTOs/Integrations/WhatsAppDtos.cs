namespace iM3Helpdesk.Application.DTOs.Integrations;

public class WhatsAppWebhookDto
{
  public string? From { get; set; }
  public string? To { get; set; }
  public string? Body { get; set; }
  public string? ProfileName { get; set; }
  public string? MediaUrl0 { get; set; }
}

public class SendWhatsAppDto
{
  public string To { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
}
