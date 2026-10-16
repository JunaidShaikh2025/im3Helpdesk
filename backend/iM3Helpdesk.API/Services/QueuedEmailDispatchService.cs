using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Infrastructure.Persistence;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace iM3Helpdesk.Infrastructure.Services;

public class QueuedEmailDispatchService : IEmailDispatchService
{
  private sealed record SmtpProfile(
      string Host,
      int Port,
      string Username,
      string Password,
      string FromEmail,
      string FromName);

  private readonly IConfiguration _config;
  private readonly IServiceScopeFactory _scopeFactory;
  private readonly ILogger<QueuedEmailDispatchService> _logger;
  private readonly IEmailTemplateRenderer _templates;

  public QueuedEmailDispatchService(
      IConfiguration config,
      IServiceScopeFactory scopeFactory,
      ILogger<QueuedEmailDispatchService> logger,
      IEmailTemplateRenderer templates)
  {
    _config = config;
    _scopeFactory = scopeFactory;
    _logger = logger;
    _templates = templates;
  }

  public async Task<string?> SendDirectAsync(QueuedEmailDispatch dispatch)
  {
    var smtpProfile = await ResolveSmtpProfileAsync(dispatch.OrganizationId);
    if (smtpProfile == null)
      return null;

    var msg = new MimeMessage();
    msg.From.Add(new MailboxAddress(
        string.IsNullOrWhiteSpace(dispatch.FromDisplayName)
            ? smtpProfile.FromName
            : dispatch.FromDisplayName,
        smtpProfile.FromEmail));
    msg.To.Add(MailboxAddress.Parse(dispatch.To));
    AddRecipients(msg.Cc, dispatch.Cc);
    AddRecipients(msg.Bcc, dispatch.Bcc);
    msg.Subject = dispatch.Subject;

    if (!string.IsNullOrWhiteSpace(dispatch.ReplyTo))
      msg.ReplyTo.Add(MailboxAddress.Parse(dispatch.ReplyTo));

    if (!string.IsNullOrWhiteSpace(dispatch.MessageId))
      msg.MessageId = dispatch.MessageId;

    if (!string.IsNullOrWhiteSpace(dispatch.TicketNumberTag))
      msg.Headers.Add("X-iM3-Ticket", dispatch.TicketNumberTag);

    if (!string.IsNullOrWhiteSpace(dispatch.InReplyTo))
      msg.InReplyTo = NormalizeMessageId(dispatch.InReplyTo);

    foreach (var reference in dispatch.References)
    {
      if (string.IsNullOrWhiteSpace(reference))
        continue;
      msg.References.Add(NormalizeMessageId(reference));
    }

    var finalHtml = dispatch.WrapInMasterTemplate
      ? WrapInMasterTemplate(dispatch.HtmlBody, smtpProfile.FromName)
      : dispatch.HtmlBody;

    var body = new BodyBuilder { HtmlBody = finalHtml };
    msg.Body = body.ToMessageBody();

    using var client = new SmtpClient();
    await client.ConnectAsync(
        smtpProfile.Host,
        smtpProfile.Port,
        SecureSocketOptions.StartTls);
    await client.AuthenticateAsync(
        smtpProfile.Username,
        smtpProfile.Password);
    await client.SendAsync(msg);
    await client.DisconnectAsync(true);

    _logger.LogInformation(
        "Queued email dispatched to {To} subject {Subject} messageId {MessageId}",
        dispatch.To,
        dispatch.Subject,
        msg.MessageId);

    return msg.MessageId;
  }

  private async Task<SmtpProfile?> ResolveSmtpProfileAsync(Guid? organizationId)
  {
    if (organizationId.HasValue)
    {
      using var scope = _scopeFactory.CreateScope();
      var context = scope.ServiceProvider
          .GetRequiredService<ApplicationDbContext>();

      var organization = await context.Organizations
          .IgnoreQueryFilters()
          .AsNoTracking()
          .FirstOrDefaultAsync(o => o.Id == organizationId.Value);

      var organizationProfile = BuildOrgSmtpProfile(organization);
      if (organizationProfile != null)
        return organizationProfile;

      if (organization != null)
      {
        _logger.LogWarning(
            "Organization {OrganizationId} SMTP settings are incomplete; falling back to appsettings SMTP.",
            organizationId.Value);
      }
    }

    var fallbackProfile = BuildFallbackSmtpProfile();
    if (fallbackProfile == null)
    {
      _logger.LogWarning(
          "No usable SMTP profile found for organization {OrganizationId}.",
          organizationId);
    }

    return fallbackProfile;
  }

  private static SmtpProfile? BuildOrgSmtpProfile(Organization? organization)
  {
    if (organization == null)
      return null;

    var fromEmail = FirstNonEmpty(
        organization.SmtpFromEmail,
        organization.SupportEmail,
        organization.SmtpUsername);
    var username = FirstNonEmpty(
        organization.SmtpUsername,
        fromEmail);
    var host = organization.SmtpHost?.Trim();
    var password = organization.SmtpPassword?.Trim();

    if (string.IsNullOrWhiteSpace(host) ||
        string.IsNullOrWhiteSpace(fromEmail) ||
        string.IsNullOrWhiteSpace(username) ||
        string.IsNullOrWhiteSpace(password))
    {
      return null;
    }

    return new SmtpProfile(
        host,
        organization.SmtpPort ?? 587,
        username,
        password,
        fromEmail,
        FirstNonEmpty(
            organization.SmtpFromName,
            organization.Name,
            "DeskMate")!);
  }

  private SmtpProfile? BuildFallbackSmtpProfile()
  {
    var smtp = _config.GetSection("SmtpSettings");
    var fromEmail = FirstNonEmpty(
        smtp["FromEmail"],
        smtp["Username"]);
    var username = FirstNonEmpty(
        smtp["Username"],
        fromEmail);
    var host = smtp["Host"]?.Trim();
    var password = smtp["Password"]?.Trim();

    if (string.IsNullOrWhiteSpace(host) ||
        string.IsNullOrWhiteSpace(fromEmail) ||
        string.IsNullOrWhiteSpace(username) ||
        string.IsNullOrWhiteSpace(password))
    {
      return null;
    }

    return new SmtpProfile(
        host,
        smtp.GetValue<int>("Port", 587),
        username,
        password,
        fromEmail,
        FirstNonEmpty(
            smtp["FromName"],
            "DeskMate")!);
  }

  private static string? FirstNonEmpty(params string?[] values)
  {
    return values.FirstOrDefault(value =>
        !string.IsNullOrWhiteSpace(value))?.Trim();
  }

  private static void AddRecipients(InternetAddressList list, IEnumerable<string>? addresses)
  {
    if (addresses == null)
      return;

    foreach (var address in addresses)
    {
      if (string.IsNullOrWhiteSpace(address))
        continue;

      if (MailboxAddress.TryParse(address.Trim(), out var mailbox))
        list.Add(mailbox);
    }
  }

  private static string NormalizeMessageId(string id)
  {
    id = id.Trim();
    if (id.StartsWith("<") && id.EndsWith(">"))
      return id.Substring(1, id.Length - 2);
    return id;
  }

  private string WrapInMasterTemplate(string content, string orgName)
  {
    return _templates.Render("_master",
        new Dictionary<string, string?>
        {
          ["content"] = content,
          ["org_name"] = orgName,
        });
  }
}
