using System;

namespace iM3Helpdesk.Application.Contracts.Services;

public interface ICalendarEmailService
{
    Task SendCalendarReminderAsync(
        string to,
        string attendeeName,
        string eventTitle,
        string eventType,
        string eventDescription,
        DateTime startDate,
        int minutesBefore,
        string? ticketNumber,
        string orgName,
        Guid? organizationId = null);

    Task SendCalendarInviteAsync(
        string to,
        string attendeeName,
        string eventTitle,
        string eventType,
        string eventDescription,
        DateTime startDate,
        DateTime? endDate,
        string organizerName,
        string orgName,
        Guid? organizationId = null);

    Task SendCalendarEventUpdatedAsync(
        string to,
        string attendeeName,
        string eventTitle,
        DateTime startDate,
        string changeType,  // "updated" | "cancelled"
        string orgName,
        Guid? organizationId = null);
}
