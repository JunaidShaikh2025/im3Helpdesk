using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace iM3Helpdesk.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
