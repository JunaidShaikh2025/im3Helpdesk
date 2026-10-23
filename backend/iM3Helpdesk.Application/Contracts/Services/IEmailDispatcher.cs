namespace iM3Helpdesk.Application.Contracts.Services;

public interface IEmailDispatcher
{
    Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        string? replyTo = null,
        Guid? organizationId = null,
        bool wrapInMasterTemplate = true,
        string? ticketNumberTag = null);

    Task<string?> SendReplyAsync(
        string to,
        string subject,
        string htmlBody,
        string ticketNumber,
        string agentName,
        string agentSignature,
        Guid? organizationId = null,
        IEnumerable<string>? cc = null,
        IEnumerable<string>? bcc = null,
        string? inReplyTo = null,
        IEnumerable<string>? references = null);

    Task<string?> SendForwardAsync(
        string to,
        string subject,
        string htmlBody,
        Guid? organizationId = null,
        IEnumerable<string>? cc = null,
        IEnumerable<string>? bcc = null,
        string? inReplyTo = null,
        IEnumerable<string>? references = null,
        string? fromDisplayName = null);
}
