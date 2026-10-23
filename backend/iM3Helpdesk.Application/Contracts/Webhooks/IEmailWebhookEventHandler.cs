using iM3Helpdesk.Domain.Entities;

namespace iM3Helpdesk.Application.Contracts.Webhooks;

public interface IEmailWebhookEventHandler
{
    bool CanHandle(string eventType);
    void Handle(EmailQueue queueRecord, string eventType);
}
