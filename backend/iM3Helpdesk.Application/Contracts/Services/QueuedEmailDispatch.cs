namespace iM3Helpdesk.Application.Contracts.Services;

public sealed class QueuedEmailDispatch
{
  public string To { get; set; } = string.Empty;
  public string Subject { get; set; } = string.Empty;
  public string HtmlBody { get; set; } = string.Empty;
  public string? ReplyTo { get; set; }
  public Guid? OrganizationId { get; set; }
  public bool WrapInMasterTemplate { get; set; } = true;
  public List<string> Cc { get; set; } = new();
  public List<string> Bcc { get; set; } = new();
  public string? InReplyTo { get; set; }
  public List<string> References { get; set; } = new();
  public string? TicketNumberTag { get; set; }
  public string? FromDisplayName { get; set; }
  public string? MessageId { get; set; }
}

public sealed record EmailQueueEnqueueResult(Guid EmailQueueId, string MessageId);
