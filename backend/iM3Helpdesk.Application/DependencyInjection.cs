using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.Contracts.Sla;
using iM3Helpdesk.Application.Contracts.Subscriptions;
using iM3Helpdesk.Application.Contracts.Webhooks;
using iM3Helpdesk.Application.Services;
using iM3Helpdesk.Application.Services.Sla;
using iM3Helpdesk.Application.Services.Subscriptions;
using iM3Helpdesk.Application.Services.Webhooks;
using Microsoft.Extensions.DependencyInjection;

namespace iM3Helpdesk.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAgentService, AgentService>();
        services.AddScoped<IAIFeaturesService, AIFeaturesService>();

        // Open/Closed Principle: Webhook Strategies
        services.AddScoped<IEmailWebhookEventHandler, DeliveredWebhookEventHandler>();
        services.AddScoped<IEmailWebhookEventHandler, BouncedWebhookEventHandler>();
        services.AddScoped<IEmailWebhookEventHandler, OpenedWebhookEventHandler>();
        services.AddScoped<IEmailWebhookEventHandler, SentWebhookEventHandler>();
        services.AddScoped<IEmailWebhookEventHandler, DeferredWebhookEventHandler>();
        services.AddScoped<IEmailWebhookEventHandler, ClickedWebhookEventHandler>();

        // Open/Closed Principle: Subscription Tier Policies
        services.AddSingleton<ISubscriptionTierPolicy, GrowthTierPolicy>();
        services.AddSingleton<ISubscriptionTierPolicy, ProTierPolicy>();
        services.AddSingleton<ISubscriptionTierPolicy, EnterpriseTierPolicy>();
        services.AddSingleton<ISubscriptionTierPolicyProvider, SubscriptionTierPolicyProvider>();

        // Open/Closed Principle: SLA Priority Deadline Strategies
        services.AddSingleton<ISlaPriorityDeadlineStrategy, CriticalPrioritySlaStrategy>();
        services.AddSingleton<ISlaPriorityDeadlineStrategy, HighPrioritySlaStrategy>();
        services.AddSingleton<ISlaPriorityDeadlineStrategy, MediumPrioritySlaStrategy>();
        services.AddSingleton<ISlaPriorityDeadlineStrategy, LowPrioritySlaStrategy>();

        return services;
    }
}
