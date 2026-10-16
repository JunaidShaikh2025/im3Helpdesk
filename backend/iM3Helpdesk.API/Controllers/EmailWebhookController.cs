using System.Text.Json;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/email-webhooks")]
public class EmailWebhookController : ControllerBase
{
  private readonly ApplicationDbContext _context;
  private readonly IConfiguration _configuration;

  public EmailWebhookController(
      ApplicationDbContext context,
      IConfiguration configuration)
  {
    _context = context;
    _configuration = configuration;
  }

  [HttpPost("events")]
  [AllowAnonymous]
  public async Task<IActionResult> ReceiveEvents([FromBody] JsonElement payload)
  {
    var configuredSecret = _configuration["EmailWebhook:Secret"];
    if (!string.IsNullOrWhiteSpace(configuredSecret))
    {
      var suppliedSecret = Request.Headers["X-Webhook-Secret"].FirstOrDefault();
      if (!string.Equals(configuredSecret, suppliedSecret, StringComparison.Ordinal))
        return Unauthorized();
    }

    var provider = Request.Headers["X-Email-Provider"].FirstOrDefault() ?? "unknown";
    var events = payload.ValueKind == JsonValueKind.Array
        ? payload.EnumerateArray().ToList()
        : new List<JsonElement> { payload };

    foreach (var emailEvent in events)
    {
      var rawJson = emailEvent.GetRawText();
      var eventType = ExtractString(emailEvent, "event", "type", "notificationType")
          ?.Trim()
          ?.ToLowerInvariant() ?? "unknown";
      var recipient = ExtractString(emailEvent, "email", "recipient", "to");
      var providerMessageId = NormalizeMessageId(
          ExtractString(emailEvent, "sg_message_id", "message_id", "messageId", "smtp-id", "mail.messageId"));

      var log = new EmailWebhookLog
      {
        Provider = provider,
        EventType = eventType,
        Recipient = recipient,
        ProviderMessageId = providerMessageId,
        PayloadJson = rawJson,
        ReceivedAt = DateTime.UtcNow
      };

      try
      {
        var queueIdText = ExtractString(emailEvent, "emailQueueId", "email_queue_id", "custom_args.emailQueueId");
        Guid? queueId = Guid.TryParse(queueIdText, out var parsedQueueId)
            ? parsedQueueId
            : null;

        var queueRecord = queueId.HasValue
            ? await _context.EmailQueues.FirstOrDefaultAsync(e => e.Id == queueId.Value)
            : await _context.EmailQueues
                .Where(e =>
                    (providerMessageId != null && (e.ProviderMessageId == providerMessageId || e.MessageId == providerMessageId)) ||
                    (!string.IsNullOrWhiteSpace(recipient) && e.ToEmail == recipient))
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();

        if (queueRecord != null)
        {
          ApplyStatusUpdate(queueRecord, eventType);
          queueRecord.LastWebhookAt = DateTime.UtcNow;
          queueRecord.LastWebhookEvent = eventType;
          if (!string.IsNullOrWhiteSpace(providerMessageId))
            queueRecord.ProviderMessageId = providerMessageId;

          log.EmailQueueId = queueRecord.Id;
          log.Processed = true;
          log.ProcessedAt = DateTime.UtcNow;
        }
        else
        {
          log.Processed = false;
          log.ProcessingError = "No matching email queue record found";
        }
      }
      catch (Exception ex)
      {
        log.Processed = false;
        log.ProcessingError = ex.Message;
        log.ProcessedAt = DateTime.UtcNow;
      }

      _context.EmailWebhookLogs.Add(log);
    }

    await _context.SaveChangesAsync();
    return Ok(new { message = "Webhook events processed" });
  }

  private static void ApplyStatusUpdate(EmailQueue queueRecord, string eventType)
  {
    switch (eventType)
    {
      case "delivered":
        queueRecord.Status = "Delivered";
        queueRecord.DeliveredAt = DateTime.UtcNow;
        queueRecord.IsSent = true;
        break;

      case "bounce":
      case "bounced":
      case "dropped":
      case "blocked":
      case "spamreport":
        queueRecord.Status = "Bounced";
        queueRecord.FailedAt = DateTime.UtcNow;
        break;

      case "open":
      case "opened":
        queueRecord.Status = "Opened";
        break;

      case "processed":
      case "sent":
        queueRecord.Status = "Sent";
        queueRecord.IsSent = true;
        queueRecord.SentAt ??= DateTime.UtcNow;
        break;

      case "deferred":
        queueRecord.Status = "Queued";
        break;
    }
  }

  private static string? ExtractString(JsonElement source, params string[] paths)
  {
    foreach (var path in paths)
    {
      if (TryGetValue(source, path, out var value) && value.ValueKind is JsonValueKind.String or JsonValueKind.Number)
        return value.ToString();
    }

    return null;
  }

  private static bool TryGetValue(JsonElement source, string path, out JsonElement value)
  {
    value = source;
    var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    foreach (var part in parts)
    {
      if (value.ValueKind != JsonValueKind.Object || !value.TryGetProperty(part, out value))
      {
        value = default;
        return false;
      }
    }

    return true;
  }

  private static string? NormalizeMessageId(string? raw)
  {
    if (string.IsNullOrWhiteSpace(raw))
      return null;

    var cleaned = raw.Trim();
    var semiColon = cleaned.IndexOf(';');
    if (semiColon > 0)
      cleaned = cleaned.Substring(0, semiColon);

    cleaned = cleaned.Trim('<', '>');
    return cleaned;
  }
}
