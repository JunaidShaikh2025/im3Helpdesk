using System;
using iM3Helpdesk.Domain.Enums;

namespace iM3Helpdesk.Application.Contracts.Sla;

public interface ISlaPriorityDeadlineStrategy
{
    bool CanHandle(TicketPriority priority);
    DateTime CalculateDeadline(DateTime createdAt);
}
