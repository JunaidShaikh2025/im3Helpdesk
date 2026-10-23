namespace iM3Helpdesk.Application.Contracts.Services;

/// <summary>
/// Composite email service interface adhering to the Interface Segregation Principle (ISP).
/// Composed of focused, role-specific contracts:
/// <see cref="IEmailDispatcher"/>, <see cref="ITicketNotificationService"/>,
/// <see cref="IAuthEmailService"/>, and <see cref="ICalendarEmailService"/>.
/// </summary>
public interface IEmailService :
    IEmailDispatcher,
    ITicketNotificationService,
    IAuthEmailService,
    ICalendarEmailService
{
}
