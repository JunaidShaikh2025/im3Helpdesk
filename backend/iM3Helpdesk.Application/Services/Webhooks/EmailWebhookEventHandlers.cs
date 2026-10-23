using System;
using System.Linq;
using iM3Helpdesk.Application.Contracts.Webhooks;
using iM3Helpdesk.Domain.Entities;

namespace iM3Helpdesk.Application.Services.Webhooks;

public class DeliveredWebhookEventHandler : IEmailWebhookEventHandler
{
    public bool CanHandle(string eventType) =>
        string.Equals(eventType, "delivered", StringComparison.OrdinalIgnoreCase);

    public void Handle(EmailQueue queueRecord, string eventType)
    {
        queueRecord.Status = "Delivered";
        queueRecord.DeliveredAt = DateTime.UtcNow;
        queueRecord.IsSent = true;
    }
}

public class BouncedWebhookEventHandler : IEmailWebhookEventHandler
{
    private static readonly string[] HandledEvents = { "bounce", "bounced", "dropped", "blocked", "spamreport" };

    public bool CanHandle(string eventType) =>
        HandledEvents.Contains(eventType, StringComparer.OrdinalIgnoreCase);

    public void Handle(EmailQueue queueRecord, string eventType)
    {
        queueRecord.Status = "Bounced";
        queueRecord.FailedAt = DateTime.UtcNow;
    }
}

public class OpenedWebhookEventHandler : IEmailWebhookEventHandler
{
    private static readonly string[] HandledEvents = { "open", "opened" };

    public bool CanHandle(string eventType) =>
        HandledEvents.Contains(eventType, StringComparer.OrdinalIgnoreCase);

    public void Handle(EmailQueue queueRecord, string eventType)
    {
        queueRecord.Status = "Opened";
    }
}

public class SentWebhookEventHandler : IEmailWebhookEventHandler
{
    private static readonly string[] HandledEvents = { "processed", "sent" };

    public bool CanHandle(string eventType) =>
        HandledEvents.Contains(eventType, StringComparer.OrdinalIgnoreCase);

    public void Handle(EmailQueue queueRecord, string eventType)
    {
        queueRecord.Status = "Sent";
        queueRecord.IsSent = true;
        queueRecord.SentAt ??= DateTime.UtcNow;
    }
}

public class DeferredWebhookEventHandler : IEmailWebhookEventHandler
{
    public bool CanHandle(string eventType) =>
        string.Equals(eventType, "deferred", StringComparison.OrdinalIgnoreCase);

    public void Handle(EmailQueue queueRecord, string eventType)
    {
        queueRecord.Status = "Queued";
    }
}

public class ClickedWebhookEventHandler : IEmailWebhookEventHandler
{
    private static readonly string[] HandledEvents = { "click", "clicked" };

    public bool CanHandle(string eventType) =>
        HandledEvents.Contains(eventType, StringComparer.OrdinalIgnoreCase);

    public void Handle(EmailQueue queueRecord, string eventType)
    {
        // Preserves current delivery status while recording interaction
        if (queueRecord.Status != "Delivered")
        {
            queueRecord.Status = "Opened";
        }
    }
}
