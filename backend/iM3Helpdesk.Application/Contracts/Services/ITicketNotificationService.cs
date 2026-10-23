namespace iM3Helpdesk.Application.Contracts.Services;

public interface ITicketNotificationService
{
    Task SendTicketCreatedAsync(
        string to,
        string customerName,
        string ticketTitle,
        string ticketNumber,
        string category,
        string priority,
        string orgName,
        Guid? organizationId = null);

    Task SendTicketStatusChangedAsync(
        string to,
        string customerName,
        string ticketTitle,
        string ticketNumber,
        string oldStatus,
        string newStatus,
        string orgName,
        Guid? organizationId = null);

    Task SendTicketAssignedAsync(
        string agentEmail,
        string agentName,
        string ticketTitle,
        string ticketNumber,
        string customerName,
        string priority,
        string orgName,
        Guid? organizationId = null);

    Task SendTicketMergedAsync(
        string to,
        string customerName,
        string mergedTicketNumber,
        string originalTicketNumber,
        string originalTicketTitle,
        string orgName,
        Guid? organizationId = null);
}
