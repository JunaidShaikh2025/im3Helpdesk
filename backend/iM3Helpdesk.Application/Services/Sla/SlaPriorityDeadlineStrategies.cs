using System;
using iM3Helpdesk.Application.Contracts.Sla;
using iM3Helpdesk.Domain.Enums;

namespace iM3Helpdesk.Application.Services.Sla;

public class CriticalPrioritySlaStrategy : ISlaPriorityDeadlineStrategy
{
    public bool CanHandle(TicketPriority priority) => priority == TicketPriority.Critical;
    public DateTime CalculateDeadline(DateTime createdAt) => createdAt.AddHours(4);
}

public class HighPrioritySlaStrategy : ISlaPriorityDeadlineStrategy
{
    public bool CanHandle(TicketPriority priority) => priority == TicketPriority.High;
    public DateTime CalculateDeadline(DateTime createdAt) => createdAt.AddHours(8);
}

public class MediumPrioritySlaStrategy : ISlaPriorityDeadlineStrategy
{
    public bool CanHandle(TicketPriority priority) => priority == TicketPriority.Medium;
    public DateTime CalculateDeadline(DateTime createdAt) => createdAt.AddHours(24);
}

public class LowPrioritySlaStrategy : ISlaPriorityDeadlineStrategy
{
    public bool CanHandle(TicketPriority priority) => priority == TicketPriority.Low;
    public DateTime CalculateDeadline(DateTime createdAt) => createdAt.AddHours(72);
}
