namespace iM3Helpdesk.Application.Contracts.Services;

public interface IAuthEmailService
{
    Task SendEmailVerificationAsync(
        string to,
        string fullName,
        string verificationToken,
        string orgName = "DeskMate",
        Guid? organizationId = null);

    Task SendWelcomeEmailAsync(
        string to,
        string fullName,
        string companyName,
        Guid? organizationId = null);

    Task SendForgotPasswordAsync(
        string to,
        string fullName,
        string resetToken,
        string orgName = "DeskMate",
        Guid? organizationId = null);

    Task SendAgentInviteAsync(
        string to,
        string agentName,
        string orgName,
        string tempPassword,
        Guid? organizationId = null);

    Task SendOtpEmailAsync(
        string to,
        string fullName,
        string otp,
        Guid? organizationId = null);
}
