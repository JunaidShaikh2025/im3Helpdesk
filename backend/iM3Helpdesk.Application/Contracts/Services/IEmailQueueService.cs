namespace iM3Helpdesk.Application.Contracts.Services;

public interface IEmailQueueService
{
    Task<EmailQueueEnqueueResult> QueueEmailAsync(QueuedEmailDispatch dispatch);

    Task ProcessQueuedEmailAsync(Guid emailQueueId);

    Task RetryEmailAsync(Guid emailQueueId);

    Task CancelQueuedEmailAsync(Guid emailQueueId);
}
