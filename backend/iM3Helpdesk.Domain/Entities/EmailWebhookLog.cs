namespace iM3Helpdesk.Domain.Entities;

public class EmailWebhookLog
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid? EmailQueueId { get; set; }
  public string Provider { get; set; } = "unknown";
  public string EventType { get; set; } = string.Empty;
  public string? Recipient { get; set; }
  public string? ProviderMessageId { get; set; }
  public string PayloadJson { get; set; } = string.Empty;
  public bool Processed { get; set; }
  public string? ProcessingError { get; set; }
  public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
  public DateTime? ProcessedAt { get; set; }
}
