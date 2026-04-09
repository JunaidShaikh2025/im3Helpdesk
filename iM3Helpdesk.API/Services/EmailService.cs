namespace iM3Helpdesk.API.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string fullName, string token);
    Task SendForgotPasswordEmailAsync(string toEmail, string fullName, string token);
    Task SendWelcomeEmailAsync(string toEmail, string fullName, string companyName);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string fullName, string token)
    {
        var verifyUrl = $"http://localhost:4200/verify-email?token={token}";
        var subject = "Verify your iM3 Helpdesk account";
        var body = $@"
            <h2>Hello {fullName}!</h2>
            <p>Thank you for registering with iM3 Helpdesk.</p>
            <p>Please click the button below to verify your email:</p>
            <a href='{verifyUrl}' style='background:#1976d2;color:white;padding:12px 24px;
               border-radius:6px;text-decoration:none;display:inline-block;margin:16px 0;'>
               Verify Email
            </a>
            <p>Or copy this link: {verifyUrl}</p>
            <p>This link expires in 24 hours.</p>
        ";
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendForgotPasswordEmailAsync(string toEmail, string fullName, string token)
    {
        var resetUrl = $"http://localhost:4200/reset-password?token={token}";
        var subject = "Reset your iM3 Helpdesk password";
        var body = $@"
            <h2>Hello {fullName}!</h2>
            <p>We received a request to reset your password.</p>
            <a href='{resetUrl}' style='background:#1976d2;color:white;padding:12px 24px;
               border-radius:6px;text-decoration:none;display:inline-block;margin:16px 0;'>
               Reset Password
            </a>
            <p>If you did not request this, please ignore this email.</p>
        ";
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string fullName, string companyName)
    {
        var subject = "Welcome to iM3 Helpdesk!";
        var body = $@"
            <h2>Welcome {fullName}!</h2>
            <p>Your company <strong>{companyName}</strong> has been successfully set up.</p>
            <p>You have a 30-day free trial. Login to get started!</p>
            <a href='http://localhost:4200/login' style='background:#1976d2;color:white;
               padding:12px 24px;border-radius:6px;text-decoration:none;
               display:inline-block;margin:16px 0;'>
               Go to Dashboard
            </a>
        ";
        await SendEmailAsync(toEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
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