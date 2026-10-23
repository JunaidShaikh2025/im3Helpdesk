using System;
using System.Collections.Generic;
using System.Linq;
using iM3Helpdesk.Application.Contracts.Sla;
using iM3Helpdesk.Application.Contracts.Subscriptions;
using iM3Helpdesk.Application.Contracts.Webhooks;
using iM3Helpdesk.Application.Services.Sla;
using iM3Helpdesk.Application.Services.Subscriptions;
using iM3Helpdesk.Application.Services.Webhooks;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using Xunit;

namespace iM3Helpdesk.API.Tests.Architecture;

public class OpenClosedPrincipleStrategyTests
{
    [Theory]
    [InlineData("delivered", "Delivered", true)]
    [InlineData("bounce", "Bounced", false)]
    [InlineData("opened", "Opened", false)]
    [InlineData("sent", "Sent", true)]
    public void EmailWebhookStrategies_ShouldProcessEventsPolymorphically(string eventType, string expectedStatus, bool expectedSent)
    {
        var handlers = new List<IEmailWebhookEventHandler>
        {
            new DeliveredWebhookEventHandler(),
            new BouncedWebhookEventHandler(),
            new OpenedWebhookEventHandler(),
            new SentWebhookEventHandler(),
            new DeferredWebhookEventHandler(),
            new ClickedWebhookEventHandler()
        };

        var handler = handlers.FirstOrDefault(h => h.CanHandle(eventType));
        Assert.NotNull(handler);

        var queue = new EmailQueue { ToEmail = "test@example.com", Subject = "Test" };
        handler.Handle(queue, eventType);

        Assert.Equal(expectedStatus, queue.Status);
        if (expectedSent)
        {
            Assert.True(queue.IsSent);
        }
    }

    [Theory]
    [InlineData("growth", 0)]
    [InlineData("pro", 1)]
    [InlineData("enterprise", 2)]
    [InlineData("unknown", 0)]
    public void SubscriptionTierPolicyProvider_ShouldResolveRanksWithoutRigidSwitch(string tierKey, int expectedRank)
    {
        var policies = new ISubscriptionTierPolicy[]
        {
            new GrowthTierPolicy(),
            new ProTierPolicy(),
            new EnterpriseTierPolicy()
        };
        var provider = new SubscriptionTierPolicyProvider(policies);

        var rank = provider.GetRank(tierKey);
        Assert.Equal(expectedRank, rank);
    }

    [Fact]
    public void SlaPriorityDeadlineStrategies_ShouldCalculateDeadlinesPolymorphically()
    {
        var strategies = new ISlaPriorityDeadlineStrategy[]
        {
            new CriticalPrioritySlaStrategy(),
            new HighPrioritySlaStrategy(),
            new MediumPrioritySlaStrategy(),
            new LowPrioritySlaStrategy()
        };

        var createdAt = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

        var critical = strategies.First(s => s.CanHandle(TicketPriority.Critical)).CalculateDeadline(createdAt);
        var high = strategies.First(s => s.CanHandle(TicketPriority.High)).CalculateDeadline(createdAt);
        var medium = strategies.First(s => s.CanHandle(TicketPriority.Medium)).CalculateDeadline(createdAt);
        var low = strategies.First(s => s.CanHandle(TicketPriority.Low)).CalculateDeadline(createdAt);

        Assert.Equal(createdAt.AddHours(4), critical);
        Assert.Equal(createdAt.AddHours(8), high);
        Assert.Equal(createdAt.AddHours(24), medium);
        Assert.Equal(createdAt.AddHours(72), low);
    }
}
