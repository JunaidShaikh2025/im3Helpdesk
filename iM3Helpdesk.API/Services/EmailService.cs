using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace iM3Helpdesk.Infrastructure.Services;

// ✅ Interface — all methods here
public interface IEmailService
{
  Task SendAsync(
      string to,
      string subject,
      string htmlBody,
      string? replyTo = null);

  Task SendReplyAsync(
      string to,
      string subject,
      string htmlBody,
      string ticketNumber,
      string agentName,
      string agentSignature);
}

public class EmailService : IEmailService
{
  private readonly IConfiguration _config;

  public EmailService(IConfiguration config)
  {
    _config = config;
  }

  // ✅ Generic send
  public async Task SendAsync(
      string to,
      string subject,
      string htmlBody,
      string? replyTo = null)
  {
    var smtp = _config.GetSection("SmtpSettings");
    var fromEmail = smtp["FromEmail"] ?? "";
    var fromName = smtp["FromName"]
        ?? "iM3 Helpdesk";
    var password = smtp["Password"] ?? "";
    var host = smtp["Host"] ?? "smtp.gmail.com";
    var port = smtp.GetValue<int>("Port", 587);

    if (string.IsNullOrEmpty(fromEmail) ||
        string.IsNullOrEmpty(password))
      return;

    var msg = new MimeMessage();
    msg.From.Add(new MailboxAddress(
        fromName, fromEmail));
    msg.To.Add(MailboxAddress.Parse(to));
    msg.Subject = subject;

    if (replyTo != null)
      msg.ReplyTo.Add(
          MailboxAddress.Parse(replyTo));

    var body = new BodyBuilder
    {
      HtmlBody = htmlBody
    };
    msg.Body = body.ToMessageBody();

    using var client = new SmtpClient();
    await client.ConnectAsync(
        host, port,
        MailKit.Security.SecureSocketOptions
            .StartTls);
    await client.AuthenticateAsync(
        fromEmail, password);
    await client.SendAsync(msg);
    await client.DisconnectAsync(true);
  }

  // ✅ Agent reply to customer
  public async Task SendReplyAsync(
      string to,
      string subject,
      string htmlBody,
      string ticketNumber,
      string agentName,
      string agentSignature)
  {
    var fullSubject =
        $"Re: {subject} [{ticketNumber}]";

    var sig = string.IsNullOrEmpty(agentSignature)
        ? $"<p>{agentName}<br/>iM3 Support</p>"
        : agentSignature;

    var fullHtml = $@"
<div style='font-family:Arial,sans-serif;
  max-width:600px;margin:0 auto;
  color:#333;font-size:14px;line-height:1.6'>

  <div style='padding:16px 0'>
    {htmlBody}
  </div>

  <hr style='border:none;border-top:
    1px solid #e0e0e0;margin:16px 0'/>

  <div style='font-size:12px;color:#666'>
    {sig}
  </div>

  <div style='margin-top:16px;padding:12px;
    background:#f9fafb;border-radius:6px;
    font-size:11px;color:#9ca3af;
    border:1px solid #e8e8e8'>
    This is a reply to your support ticket
    <strong>{ticketNumber}</strong>.<br/>
    Please reply to this email to continue
    the conversation.
  </div>
</div>";

    await SendAsync(to, fullSubject, fullHtml);
  }
}
