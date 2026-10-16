namespace iM3Helpdesk.Application.Contracts.Services;

public interface IEmailDispatchService
{
  Task<string?> SendDirectAsync(QueuedEmailDispatch dispatch);
}
