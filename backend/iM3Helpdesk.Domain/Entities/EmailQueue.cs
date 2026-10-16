namespace iM3Helpdesk.Domain.Entities;

public class EmailQueue
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid? OrganizationId { get; set; }
  public string ToEmail { get; set; } = string.Empty;
  public string Subject { get; set; } = string.Empty;
  public string Body { get; set; } = string.Empty;
  public string Status { get; set; } = "Queued";
  public string? PayloadJson { get; set; }
  public string? MessageId { get; set; }
  public string? ProviderMessageId { get; set; }
  public string? HangfireJobId { get; set; }
  public bool IsSent { get; set; } = false;
  public int RetryCount { get; set; } = 0;
  public string? ErrorMessage { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? LastAttemptAt { get; set; }
  public DateTime? SentAt { get; set; }
  public DateTime? DeliveredAt { get; set; }
  public DateTime? FailedAt { get; set; }
  public DateTime? CancelledAt { get; set; }
  public DateTime? LastWebhookAt { get; set; }
  public string? LastWebhookEvent { get; set; }
  public DateTime? NextRetryAt { get; set; }
}
