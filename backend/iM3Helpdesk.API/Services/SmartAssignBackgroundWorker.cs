using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.API.Services;

public class SmartAssignBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SmartAssignBackgroundWorker> _logger;

    public SmartAssignBackgroundWorker(
        IServiceProvider serviceProvider,
        IWebHostEnvironment env,
        ILogger<SmartAssignBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _env = env;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SmartAssignBackgroundWorker started.");

        // Initial delay on startup
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var smartAssign = scope.ServiceProvider.GetRequiredService<ISmartAssignService>();

                var organizations = await db.Organizations
                    .IgnoreQueryFilters()
                    .Select(o => o.Id)
                    .ToListAsync(stoppingToken);

                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                foreach (var orgId in organizations)
                {
                    try
                    {
                        var config = await smartAssign.GetOrCreateConfigAsync(orgId);

                        // Check when model was last trained
                        var activeModel = await db.AIModelVersions
                            .IgnoreQueryFilters()
                            .Where(m => m.OrganizationId == orgId && m.IsActive)
                            .OrderByDescending(m => m.TrainedAt)
                            .FirstOrDefaultAsync(stoppingToken);

                        bool shouldRetrain = false;
                        if (activeModel == null)
                        {
                            // Initial training attempt
                            shouldRetrain = true;
                        }
                        else
                        {
                            var daysSinceLastTrain = (DateTime.UtcNow - activeModel.TrainedAt).TotalDays;
                            if (daysSinceLastTrain >= config.AutoRetrainFrequencyDays)
                            {
                                shouldRetrain = true;
                            }
                        }

                        if (shouldRetrain)
                        {
                            _logger.LogInformation("Background retraining triggered for Organization {OrgId}", orgId);
                            await smartAssign.RetrainModelAsync(orgId, webRoot);
                        }
                        else
                        {
                            // Always keep skill statistics up to date
                            await smartAssign.RefreshAgentSkillStatsAsync(orgId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "SmartAssignBackgroundWorker error for Org {OrgId}", orgId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in SmartAssignBackgroundWorker loop");
            }

            // Check every 6 hours
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}

