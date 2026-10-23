using System;
using System.Linq;
using System.Reflection;
using iM3Helpdesk.Application.Contracts.Services;
using Xunit;

namespace iM3Helpdesk.API.Tests.Architecture;

public class EmailServiceSegregationTests
{
    [Fact]
    public void EmailService_MustInheritFromSegregatedInterfaces()
    {
        var emailServiceType = typeof(IEmailService);

        Assert.True(typeof(IEmailDispatcher).IsAssignableFrom(emailServiceType),
            "IEmailService must implement IEmailDispatcher");
        Assert.True(typeof(ITicketNotificationService).IsAssignableFrom(emailServiceType),
            "IEmailService must implement ITicketNotificationService");
        Assert.True(typeof(IAuthEmailService).IsAssignableFrom(emailServiceType),
            "IEmailService must implement IAuthEmailService");
        Assert.True(typeof(ICalendarEmailService).IsAssignableFrom(emailServiceType),
            "IEmailService must implement ICalendarEmailService");
    }

    [Fact]
    public void SegregatedInterfaces_MustHaveFocusedContracts()
    {
        var dispatcherMethods = typeof(IEmailDispatcher).GetMethods();
        var ticketMethods = typeof(ITicketNotificationService).GetMethods();
        var authMethods = typeof(IAuthEmailService).GetMethods();
        var calendarMethods = typeof(ICalendarEmailService).GetMethods();

        Assert.Contains(dispatcherMethods, m => m.Name == "SendAsync");
        Assert.Contains(dispatcherMethods, m => m.Name == "SendReplyAsync");
        Assert.Contains(dispatcherMethods, m => m.Name == "SendForwardAsync");

        Assert.Contains(ticketMethods, m => m.Name == "SendTicketCreatedAsync");
        Assert.Contains(ticketMethods, m => m.Name == "SendTicketStatusChangedAsync");
        Assert.Contains(ticketMethods, m => m.Name == "SendTicketAssignedAsync");
        Assert.Contains(ticketMethods, m => m.Name == "SendTicketMergedAsync");

        Assert.Contains(authMethods, m => m.Name == "SendEmailVerificationAsync");
        Assert.Contains(authMethods, m => m.Name == "SendWelcomeEmailAsync");
        Assert.Contains(authMethods, m => m.Name == "SendForgotPasswordAsync");
        Assert.Contains(authMethods, m => m.Name == "SendAgentInviteAsync");
        Assert.Contains(authMethods, m => m.Name == "SendOtpEmailAsync");

        Assert.Contains(calendarMethods, m => m.Name == "SendCalendarReminderAsync");
        Assert.Contains(calendarMethods, m => m.Name == "SendCalendarInviteAsync");
        Assert.Contains(calendarMethods, m => m.Name == "SendCalendarEventUpdatedAsync");
    }
}
