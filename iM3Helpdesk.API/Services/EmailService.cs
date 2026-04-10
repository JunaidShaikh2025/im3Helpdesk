namespace iM3Helpdesk.API.Services;

public interface IEmailService
{
  Task SendVerificationEmailAsync(string toEmail, string fullName, string token);
  Task SendForgotPasswordEmailAsync(string toEmail, string fullName, string token);
  Task SendWelcomeEmailAsync(string toEmail, string fullName, string companyName);
  Task SendTicketCreatedEmailAsync(string toEmail, string fullName,
      string ticketTitle, string ticketId);
  Task SendTicketStatusChangedEmailAsync(string toEmail, string fullName,
      string ticketTitle, string newStatus, string ticketId);
  Task SendTicketAssignedEmailAsync(string toEmail, string agentName,
      string ticketTitle, string ticketId);
  Task SendAgentInviteEmailAsync(string toEmail, string agentName,
      string companyName, string tempPassword);
}

public class EmailService : IEmailService
{
  private readonly IConfiguration _config;

  public EmailService(IConfiguration config)
  {
    _config = config;
  }

  public async Task SendVerificationEmailAsync(string toEmail,
      string fullName, string token)
  {
    var url = $"http://localhost:4200/verify-email?token={token}";
    var body = GetEmailTemplate(
        "Verify Your Email",
        $"Hello {fullName}!",
        "Thank you for registering with iM3 Helpdesk. Please verify your email to get started.",
        "Verify Email",
        url,
        "#1976d2"
    );
    await SendAsync(toEmail, "Verify your iM3 Helpdesk account", body);
  }

  public async Task SendForgotPasswordEmailAsync(string toEmail,
      string fullName, string token)
  {
    var url = $"http://localhost:4200/reset-password?token={token}";
    var body = GetEmailTemplate(
        "Reset Your Password",
        $"Hello {fullName}!",
        "We received a request to reset your password. Click below to continue. If you did not request this, please ignore this email.",
        "Reset Password",
        url,
        "#f44336"
    );
    await SendAsync(toEmail, "Reset your iM3 Helpdesk password", body);
  }

  public async Task SendWelcomeEmailAsync(string toEmail,
      string fullName, string companyName)
  {
    var url = "http://localhost:4200/login";
    var body = GetEmailTemplate(
        "Welcome to iM3 Helpdesk!",
        $"Welcome {fullName}!",
        $"Your company <strong>{companyName}</strong> has been successfully set up on iM3 Helpdesk. You have a 30-day free trial. Login to get started!",
        "Go to Dashboard",
        url,
        "#4caf50"
    );
    await SendAsync(toEmail, $"Welcome to iM3 Helpdesk — {companyName}", body);
  }

  public async Task SendTicketCreatedEmailAsync(string toEmail,
      string fullName, string ticketTitle, string ticketId)
  {
    var url = $"http://localhost:4200/tickets/{ticketId}";
    var body = GetEmailTemplate(
        "New Ticket Created",
        $"Hello {fullName}!",
        $"Your support ticket has been created successfully.<br><br><strong>Ticket:</strong> {ticketTitle}<br><br>Our team will get back to you shortly.",
        "View Ticket",
        url,
        "#1976d2"
    );
    await SendAsync(toEmail, $"Ticket Created: {ticketTitle}", body);
  }

  public async Task SendTicketStatusChangedEmailAsync(string toEmail,
      string fullName, string ticketTitle, string newStatus, string ticketId)
  {
    var url = $"http://localhost:4200/tickets/{ticketId}";
    var statusColor = newStatus switch
    {
      "Resolved" => "#4caf50",
      "InProgress" => "#ff9800",
      "Closed" => "#9e9e9e",
      _ => "#1976d2"
    };
    var body = GetEmailTemplate(
        "Ticket Status Updated",
        $"Hello {fullName}!",
        $"The status of your ticket has been updated.<br><br><strong>Ticket:</strong> {ticketTitle}<br><strong>New Status:</strong> <span style='color:{statusColor}'>{newStatus}</span>",
        "View Ticket",
        url,
        statusColor
    );
    await SendAsync(toEmail, $"Ticket Status: {newStatus} — {ticketTitle}", body);
  }

  public async Task SendTicketAssignedEmailAsync(string toEmail,
      string agentName, string ticketTitle, string ticketId)
  {
    var url = $"http://localhost:4200/tickets/{ticketId}";
    var body = GetEmailTemplate(
        "Ticket Assigned to You",
        $"Hello {agentName}!",
        $"A new ticket has been assigned to you.<br><br><strong>Ticket:</strong> {ticketTitle}<br><br>Please review and take action.",
        "View Ticket",
        url,
        "#ff9800"
    );
    await SendAsync(toEmail, $"New Ticket Assigned: {ticketTitle}", body);
  }

  public async Task SendAgentInviteEmailAsync(string toEmail,
      string agentName, string companyName, string tempPassword)
  {
    var url = "http://localhost:4200/login";
    var body = GetEmailTemplate(
        "You have been invited!",
        $"Hello {agentName}!",
        $"You have been invited to join <strong>{companyName}</strong> on iM3 Helpdesk as an Agent.<br><br><strong>Your login credentials:</strong><br>Email: {toEmail}<br>Temporary Password: <code style='background:#f5f5f5;padding:2px 8px;border-radius:4px'>{tempPassword}</code><br><br>Please login and change your password immediately.",
        "Login Now",
        url,
        "#9c27b0"
    );
    await SendAsync(toEmail, $"Invitation to join {companyName} on iM3 Helpdesk", body);
  }

  private string GetEmailTemplate(string title, string heading,
      string content, string buttonText, string buttonUrl, string color)
  {
    return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>{title}</title>
</head>
<body style='margin:0;padding:0;background:#f5f5f5;font-family:Roboto,Arial,sans-serif'>
  <table width='100%' cellpadding='0' cellspacing='0'>
    <tr>
      <td align='center' style='padding:40px 0'>
        <table width='600' cellpadding='0' cellspacing='0'
          style='background:white;border-radius:12px;
          box-shadow:0 4px 20px rgba(0,0,0,0.08);overflow:hidden'>

          <!-- Header -->
          <tr>
            <td style='background:{color};padding:32px;text-align:center'>
              <h1 style='color:white;margin:0;font-size:24px;font-weight:500'>
                iM3 Helpdesk
              </h1>
              <p style='color:rgba(255,255,255,0.85);margin:8px 0 0;font-size:14px'>
                {title}
              </p>
            </td>
          </tr>

          <!-- Body -->
          <tr>
            <td style='padding:40px'>
              <h2 style='color:#333;margin:0 0 16px;font-size:20px;font-weight:500'>
                {heading}
              </h2>
              <p style='color:#555;line-height:1.7;font-size:15px;margin:0 0 28px'>
                {content}
              </p>
              <table cellpadding='0' cellspacing='0'>
                <tr>
                  <td style='border-radius:8px;background:{color}'>
                    <a href='{buttonUrl}'
                      style='display:inline-block;padding:14px 32px;
                      color:white;text-decoration:none;font-size:15px;
                      font-weight:500;border-radius:8px'>
                      {buttonText}
                    </a>
                  </td>
                </tr>
              </table>
              <p style='color:#999;font-size:13px;margin:24px 0 0'>
                Or copy this link: <a href='{buttonUrl}'
                  style='color:{color}'>{buttonUrl}</a>
              </p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='background:#fafafa;padding:24px;
              border-top:1px solid #f0f0f0;text-align:center'>
              <p style='color:#bbb;font-size:12px;margin:0'>
                iM3 Helpdesk &copy; {DateTime.Now.Year} — All rights reserved
              </p>
              <p style='color:#bbb;font-size:12px;margin:6px 0 0'>
                This is an automated email. Please do not reply.
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
  }

  private async Task SendAsync(string toEmail, string subject, string body)
  {
    var smtpSettings = _config.GetSection("SmtpSettings");
    var host = smtpSettings["Host"]!;
    var port = int.Parse(smtpSettings["Port"]!);
    var fromEmail = smtpSettings["FromEmail"]!;
    var fromName = smtpSettings["FromName"]!;
    var password = smtpSettings["Password"]!;

    using var client = new System.Net.Mail.SmtpClient(host, port)
    {
      EnableSsl = true,
      Credentials = new System.Net.NetworkCredential(fromEmail, password)
    };

    var message = new System.Net.Mail.MailMessage
    {
      From = new System.Net.Mail.MailAddress(fromEmail, fromName),
      Subject = subject,
      Body = body,
      IsBodyHtml = true
    };
    message.To.Add(toEmail);

    await client.SendMailAsync(message);
  }
}
