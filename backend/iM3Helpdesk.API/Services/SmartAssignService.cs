using System.Text.Json;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.DTOs.SmartAssign;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.ML;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace iM3Helpdesk.API.Services;

public class SmartAssignService : ISmartAssignService
{
    private readonly ApplicationDbContext _context;
    private readonly ISmartAssignMlTrainer _mlTrainer;
    private readonly ISmartAssignNotifier? _notifier;
    private readonly INotificationService? _notificationService;
    private readonly IEmailService? _emailService;
    private readonly ILogger<SmartAssignService> _logger;

    public SmartAssignService(
        ApplicationDbContext context,
        ISmartAssignMlTrainer mlTrainer,
        ILogger<SmartAssignService> logger,
        ISmartAssignNotifier? notifier = null,
        INotificationService? notificationService = null,
        IEmailService? emailService = null)
    {
        _context = context;
        _mlTrainer = mlTrainer;
        _logger = logger;
        _notifier = notifier;
        _notificationService = notificationService;
        _emailService = emailService;
    }

    public async Task<TicketSmartAssignResultDto?> EvaluateTicketAsync(Guid ticketId, Guid organizationId, bool allowAutoAssign = true)
    {
        var ticket = await _context.Tickets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.OrganizationId == organizationId && !t.IsDeleted);

        if (ticket == null) return null;

        var config = await GetOrCreateConfigAsync(organizationId);

        // Active agents in this organization
        var agents = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.OrganizationId == organizationId && (u.Role == UserRole.Agent || u.Role == UserRole.CompanyAdmin))
            .ToListAsync();

        if (agents.Count == 0)
        {
            _logger.LogWarning("No active agents found in organization {OrgId} for ticket {TicketId}", organizationId, ticketId);
            return null;
        }

        // Active open workload per agent
        var activeTickets = await _context.Tickets
            .IgnoreQueryFilters()
            .Where(t => t.OrganizationId == organizationId && !t.IsDeleted && t.AssignedToUserId != null &&
                        (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress || t.Status == TicketStatus.Pending))
            .GroupBy(t => t.AssignedToUserId!.Value)
            .Select(g => new
            {
                AgentId = g.Key,
                TotalActive = g.Count(),
                HighPriority = g.Count(x => x.Priority == TicketPriority.High || x.Priority == TicketPriority.Critical)
            })
            .ToDictionaryAsync(x => x.AgentId, x => (x.TotalActive, x.HighPriority));

        // Online status of agents
        var agentIds = agents.Select(a => a.Id).ToList();
        var onlineMap = await _context.UserOnlineStatuses
            .Where(s => agentIds.Contains(s.UserId))
            .ToDictionaryAsync(s => s.UserId, s => s.IsOnline);

        // Precomputed Category skill stats for agents
        var category = string.IsNullOrWhiteSpace(ticket.Category) ? "General" : ticket.Category.Trim();
        var skillStats = await _context.AIAgentSkillStats
            .IgnoreQueryFilters()
            .Where(s => s.OrganizationId == organizationId && s.Category.ToLower() == category.ToLower())
            .ToDictionaryAsync(s => s.AgentId, s => s);

        // Customer continuity history: how many times each agent handled this requester
        Dictionary<Guid, int> continuityMap = new();
        if (ticket.CreatedByUserId.HasValue)
        {
            continuityMap = await _context.Tickets
                .IgnoreQueryFilters()
                .Where(t => t.OrganizationId == organizationId && !t.IsDeleted && t.CreatedByUserId == ticket.CreatedByUserId && t.AssignedToUserId != null)
                .GroupBy(t => t.AssignedToUserId!.Value)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }
        else if (!string.IsNullOrWhiteSpace(ticket.FromEmail))
        {
            continuityMap = await _context.Tickets
                .IgnoreQueryFilters()
                .Where(t => t.OrganizationId == organizationId && !t.IsDeleted && t.FromEmail == ticket.FromEmail && t.AssignedToUserId != null)
                .GroupBy(t => t.AssignedToUserId!.Value)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        // Active ML model
        var activeModel = await _context.AIModelVersions
            .IgnoreQueryFilters()
            .Where(m => m.OrganizationId == organizationId && m.IsActive)
            .OrderByDescending(m => m.TrainedAt)
            .FirstOrDefaultAsync();

        var mlInput = new TicketTrainingInput
        {
            Title = ticket.Title ?? string.Empty,
            Description = ticket.Description ?? string.Empty,
            Category = category,
            Priority = ticket.Priority.ToString(),
            TicketType = ticket.TicketType ?? "Support",
            Tags = ticket.Tags ?? string.Empty
        };

        List<AgentClassProbability> mlPredictions = new();
        if (activeModel != null && !string.IsNullOrWhiteSpace(activeModel.ModelFilePath) && File.Exists(activeModel.ModelFilePath))
        {
            mlPredictions = _mlTrainer.PredictTopAgents(organizationId, mlInput, activeModel.ModelFilePath, topN: agents.Count);
        }

        var mlScoresMap = mlPredictions.ToDictionary(p => p.AgentId, p => p.Probability);

        // Max open workload among agents for normalization
        int maxWorkload = activeTickets.Values.Select(v => v.TotalActive).DefaultIfEmpty(0).Max();
        if (maxWorkload < 1) maxWorkload = 1;

        // Max resolved in this category for heuristic fallback
        int maxResolvedInCat = skillStats.Values.Select(s => s.TotalResolved).DefaultIfEmpty(0).Max();
        if (maxResolvedInCat < 1) maxResolvedInCat = 1;

        var recommendations = new List<AgentRecommendationDto>();

        foreach (var agent in agents)
        {
            activeTickets.TryGetValue(agent.Id, out var workload);
            bool isOnline = onlineMap.TryGetValue(agent.Id, out var on) && on;
            skillStats.TryGetValue(agent.Id, out var skill);
            continuityMap.TryGetValue(agent.Id, out var customerTicketsCount);

            // 1. Skill / ML Score (0.0 to 1.0)
            double mlScore = 0.0;
            if (mlScoresMap.TryGetValue(agent.Id.ToString(), out var predProb))
            {
                mlScore = predProb;
            }
            else
            {
                // Cold-start fallback: category resolution ratio
                double catRatio = skill != null ? (double)skill.TotalResolved / maxResolvedInCat : 0.0;
                mlScore = Math.Max(0.15, catRatio * 0.9);
            }

            // 2. Workload Balance Score (0.0 to 1.0)
            // Agent with fewer tickets gets higher score
            double workloadScore = Math.Max(0.1, 1.0 - ((double)workload.TotalActive / (maxWorkload + 2.0)));
            if (workload.HighPriority > 2)
            {
                workloadScore = Math.Max(0.05, workloadScore - 0.25);
            }

            // 3. Availability & Continuity & SLA Urgency (0.0 to 1.0)
            double availabilityScore = 0.2; // base
            if (isOnline) availabilityScore += 0.35;
            if (customerTicketsCount > 0)
            {
                availabilityScore += Math.Min(0.35, customerTicketsCount * 0.1);
            }
            if (skill != null && skill.SlaComplianceRate > 80.0)
            {
                availabilityScore += (skill.SlaComplianceRate / 100.0) * 0.15;
            }
            availabilityScore = Math.Min(1.0, availabilityScore);

            // Weighted composite score (default 60% ML, 20% Workload, 20% Availability)
            double composite = (mlScore * config.WeightMl) +
                               (workloadScore * config.WeightWorkload) +
                               (availabilityScore * config.WeightAvailability);

            composite = Math.Round(Math.Clamp(composite, 0.05, 0.99) * 100.0, 1);

            // Construct explanations ("Why?")
            var reasons = new List<string>();

            if (skill != null && skill.TotalResolved > 0)
            {
                reasons.Add($"Resolved {skill.TotalResolved} '{category}' tickets (avg {skill.AvgResolutionTimeHours:0.#}h)");
                if (skill.SlaComplianceRate >= 85.0)
                {
                    reasons.Add($"{skill.SlaComplianceRate:0.#}% SLA compliance in {category}");
                }
            }
            else
            {
                reasons.Add($"Assigned to handle {category} & support inquiries");
            }

            if (workload.TotalActive <= 2)
            {
                reasons.Add($"Low active workload ({workload.TotalActive} open tickets)");
            }
            else
            {
                reasons.Add($"Current workload: {workload.TotalActive} active tickets");
            }

            if (isOnline)
            {
                reasons.Add("Agent is currently Online");
            }

            if (customerTicketsCount > 0)
            {
                reasons.Add($"Previously resolved {customerTicketsCount} ticket{(customerTicketsCount == 1 ? "" : "s")} for this customer");
            }

            if (ticket.Priority == TicketPriority.Critical || ticket.Priority == TicketPriority.High)
            {
                if (skill != null && skill.AvgResolutionTimeHours > 0 && skill.AvgResolutionTimeHours < 4.0)
                {
                    reasons.Add($"High-speed resolution track record ({skill.AvgResolutionTimeHours:0.#}h)");
                }
            }

            recommendations.Add(new AgentRecommendationDto
            {
                AgentId = agent.Id,
                AgentName = agent.FullName,
                AgentEmail = agent.Email,
                AgentPhotoUrl = agent.PhotoUrl,
                CompositeScore = composite,
                MlConfidenceScore = Math.Round(mlScore * 100.0, 1),
                WorkloadScore = Math.Round(workloadScore * 100.0, 1),
                AvailabilityScore = Math.Round(availabilityScore * 100.0, 1),
                Reasons = reasons,
                ActiveTicketsCount = workload.TotalActive,
                IsOnline = isOnline,
                CategoryResolvedCount = skill?.TotalResolved ?? 0,
                CategoryAvgHours = skill?.AvgResolutionTimeHours ?? 0.0,
                CategorySlaRate = skill?.SlaComplianceRate ?? 0.0
            });
        }

        // Rank top 3
        var ranked = recommendations
            .OrderByDescending(r => r.CompositeScore)
            .Take(3)
            .ToList();

        for (int i = 0; i < ranked.Count; i++)
        {
            ranked[i].MatchRank = i + 1;
        }

        var topRecommendation = ranked.FirstOrDefault();
        bool isAutoAssigned = false;

        // Auto-assign execution
        if (allowAutoAssign && config.Mode == "Auto" && topRecommendation != null &&
            (topRecommendation.CompositeScore / 100.0) >= config.ConfidenceThreshold &&
            ticket.AssignedToUserId == null)
        {
            ticket.AssignedToUserId = topRecommendation.AgentId;
            ticket.UpdatedAt = DateTime.UtcNow;

            var log = new AIAssignmentLog
            {
                TicketId = ticket.Id,
                SuggestedAgentId = topRecommendation.AgentId,
                FinalAgentId = topRecommendation.AgentId,
                ConfidenceScore = topRecommendation.CompositeScore,
                MlScore = topRecommendation.MlConfidenceScore,
                WorkloadScore = topRecommendation.WorkloadScore,
                AvailabilityScore = topRecommendation.AvailabilityScore,
                ReasonJson = JsonSerializer.Serialize(topRecommendation.Reasons),
                Mode = "Auto",
                WasAutoAssigned = true,
                IsOverridden = false,
                OrganizationId = organizationId
            };

            _context.AIAssignmentLogs.Add(log);
            await _context.SaveChangesAsync();

            isAutoAssigned = true;

            // Notify assigned agent & broadcast
            if (_notificationService != null)
            {
                await _notificationService.CreateActivityAsync(
                    topRecommendation.AgentId, organizationId,
                    "Smart Assigned",
                    $"AI auto-assigned ticket #{ticket.TicketNumber} ({topRecommendation.CompositeScore}% confidence): {ticket.Title}",
                    "Ticket", ticket.Id);

                await _notificationService.CreateAsync(
                    topRecommendation.AgentId, organizationId,
                    "Smart Assign: Ticket Assigned",
                    $"Ticket #{ticket.TicketNumber} has been automatically assigned to you by DeskMate AI.",
                    "info", ticket.Id);
            }

            if (_emailService != null)
            {
                try
                {
                    var html = $@"
<div style='font-family:Arial,sans-serif;max-width:600px;color:#1e293b'>
  <div style='background:#2563eb;color:#fff;padding:12px 18px;border-radius:6px 6px 0 0;'>
    <h3 style='margin:0;'>DeskMate Smart Assign</h3>
  </div>
  <div style='padding:18px;border:1px solid #e2e8f0;border-top:none;border-radius:0 0 6px 6px;'>
    <p>Hi {topRecommendation.AgentName},</p>
    <p>Ticket <strong>#{ticket.TicketNumber} - {ticket.Title}</strong> has been auto-assigned to you based on your skill matching ({topRecommendation.CompositeScore}% confidence).</p>
    <p style='color:#64748b;font-size:13px;'>Category: {category} | Priority: {ticket.Priority}</p>
  </div>
</div>";
                    await _emailService.SendAsync(topRecommendation.AgentEmail, $"Smart Assigned: #{ticket.TicketNumber} - {ticket.Title}", html, organizationId: organizationId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send auto-assign email to {Email}", topRecommendation.AgentEmail);
                }
            }

            if (_notifier != null)
            {
                await _notifier.NotifySmartAssignedAsync(
                    organizationId, ticket.Id, ticket.TicketNumber, ticket.Title,
                    topRecommendation.AgentId, topRecommendation.AgentName,
                    topRecommendation.CompositeScore / 100.0, isAuto: true);
                await _notifier.NotifyStatsUpdatedAsync(organizationId);
            }
        }
        else if (topRecommendation != null && ticket.AssignedToUserId == null)
        {
            // Log suggestion without applying
            var log = new AIAssignmentLog
            {
                TicketId = ticket.Id,
                SuggestedAgentId = topRecommendation.AgentId,
                ConfidenceScore = topRecommendation.CompositeScore,
                MlScore = topRecommendation.MlConfidenceScore,
                WorkloadScore = topRecommendation.WorkloadScore,
                AvailabilityScore = topRecommendation.AvailabilityScore,
                ReasonJson = JsonSerializer.Serialize(topRecommendation.Reasons),
                Mode = "Suggest",
                WasAutoAssigned = false,
                IsOverridden = false,
                OrganizationId = organizationId
            };

            _context.AIAssignmentLogs.Add(log);
            await _context.SaveChangesAsync();

            if (_notifier != null)
            {
                await _notifier.NotifySuggestionGeneratedAsync(
                    organizationId, ticket.Id, ticket.TicketNumber, ticket.Title, ranked);
            }
        }

        string? currentAgentName = null;
        if (ticket.AssignedToUserId.HasValue)
        {
            currentAgentName = agents.FirstOrDefault(a => a.Id == ticket.AssignedToUserId.Value)?.FullName;
        }

        return new TicketSmartAssignResultDto
        {
            TicketId = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Title = ticket.Title,
            Category = category,
            Priority = ticket.Priority.ToString(),
            TicketType = ticket.TicketType ?? "Support",
            CustomerName = ticket.FromName,
            CustomerEmail = ticket.FromEmail,
            CurrentAssignedAgentId = ticket.AssignedToUserId,
            CurrentAssignedAgentName = currentAgentName,
            Mode = config.Mode,
            IsAutoAssigned = isAutoAssigned,
            Recommendations = ranked,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    public async Task<bool> ApplyAssignmentAsync(Guid ticketId, Guid agentId, Guid currentUserId, Guid organizationId, string? overrideReason = null)
    {
        var ticket = await _context.Tickets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.OrganizationId == organizationId && !t.IsDeleted);

        if (ticket == null) return false;

        var agent = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == agentId && u.OrganizationId == organizationId);

        if (agent == null) return false;

        var existingLog = await _context.AIAssignmentLogs
            .IgnoreQueryFilters()
            .Where(l => l.TicketId == ticketId && l.OrganizationId == organizationId)
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync();

        bool isOverride = false;
        if (existingLog != null)
        {
            isOverride = existingLog.SuggestedAgentId != agentId;
            existingLog.FinalAgentId = agentId;
            existingLog.IsOverridden = isOverride;
            if (isOverride)
            {
                existingLog.OverriddenByUserId = currentUserId;
                existingLog.OverriddenAt = DateTime.UtcNow;
            }
        }
        else
        {
            var newLog = new AIAssignmentLog
            {
                TicketId = ticket.Id,
                SuggestedAgentId = agentId,
                FinalAgentId = agentId,
                ConfidenceScore = 100.0,
                MlScore = 100.0,
                WorkloadScore = 100.0,
                AvailabilityScore = 100.0,
                ReasonJson = JsonSerializer.Serialize(new[] { "Manual assignment via Smart Assign queue" }),
                Mode = "Suggest",
                WasAutoAssigned = false,
                IsOverridden = false,
                OrganizationId = organizationId
            };
            _context.AIAssignmentLogs.Add(newLog);
        }

        ticket.AssignedToUserId = agentId;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (_notificationService != null)
        {
            await _notificationService.CreateActivityAsync(
                currentUserId, organizationId,
                isOverride ? "Assignment Overridden" : "Assigned via AI",
                $"Ticket #{ticket.TicketNumber} assigned to {agent.FullName} {(isOverride ? "(Override)" : "(Approved)")}",
                "Ticket", ticket.Id);

            await _notificationService.CreateAsync(
                agentId, organizationId,
                "Ticket Assigned",
                $"Ticket #{ticket.TicketNumber} has been assigned to you.",
                "info", ticket.Id);
        }

        if (_notifier != null)
        {
            await _notifier.NotifySmartAssignedAsync(
                organizationId, ticket.Id, ticket.TicketNumber, ticket.Title,
                agent.Id, agent.FullName, 1.0, isAuto: false);
            await _notifier.NotifyStatsUpdatedAsync(organizationId);
        }

        return true;
    }

    public async Task<RetrainResultDto> RetrainModelAsync(Guid organizationId, string baseStoragePath)
    {
        _logger.LogInformation("Starting manual retraining of Smart Assign model for Org {OrgId}", organizationId);

        // Fetch resolved tickets
        var resolvedTickets = await _context.Tickets
            .IgnoreQueryFilters()
            .Where(t => t.OrganizationId == organizationId && !t.IsDeleted && t.AssignedToUserId != null &&
                        (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed || t.Status == TicketStatus.ResolvedOnBeta))
            .OrderByDescending(t => t.ResolvedAt ?? t.CreatedAt)
            .Take(10000)
            .Select(t => new TicketTrainingInput
            {
                Title = t.Title ?? string.Empty,
                Description = t.Description ?? string.Empty,
                Category = string.IsNullOrWhiteSpace(t.Category) ? "General" : t.Category,
                Priority = t.Priority.ToString(),
                TicketType = t.TicketType ?? "Support",
                Tags = t.Tags ?? string.Empty,
                AssignedToUserId = t.AssignedToUserId!.Value.ToString()
            })
            .ToListAsync();

        if (resolvedTickets.Count < 5)
        {
            return new RetrainResultDto
            {
                Success = false,
                Message = $"Insufficient historical data: {resolvedTickets.Count} resolved tickets found (minimum 5 required). Model will use cold-start heuristics.",
                Version = "None",
                Accuracy = 0.0,
                Top3Accuracy = 0.0,
                SamplesCount = resolvedTickets.Count,
                TrainedAt = DateTime.UtcNow
            };
        }

        var (success, accuracy, top3Accuracy, samplesCount, modelPath, metricsJson) =
            await _mlTrainer.TrainAndSaveModelAsync(organizationId, resolvedTickets, baseStoragePath);

        if (!success)
        {
            return new RetrainResultDto
            {
                Success = false,
                Message = "Model training failed. Please check logs for detailed diagnostics.",
                Accuracy = 0.0,
                SamplesCount = samplesCount
            };
        }

        // Deactivate previous active models for this tenant
        var oldModels = await _context.AIModelVersions
            .IgnoreQueryFilters()
            .Where(m => m.OrganizationId == organizationId && m.IsActive)
            .ToListAsync();

        foreach (var old in oldModels)
        {
            old.IsActive = false;
        }

        var newVersionString = $"v{DateTime.UtcNow:yyyy.MM.dd.HHmm}";
        var modelRecord = new AIModelVersion
        {
            OrganizationId = organizationId,
            Version = newVersionString,
            Algorithm = "SdcaMaximumEntropy + TF-IDF",
            Accuracy = accuracy,
            Top3Accuracy = top3Accuracy,
            TrainingSamplesCount = samplesCount,
            ModelFilePath = modelPath,
            MetricsJson = metricsJson,
            IsActive = true,
            TrainedAt = DateTime.UtcNow
        };

        _context.AIModelVersions.Add(modelRecord);
        await _context.SaveChangesAsync();

        // Also refresh agent category skill metrics
        await RefreshAgentSkillStatsAsync(organizationId);

        if (_notifier != null)
        {
            await _notifier.NotifyModelRetrainedAsync(organizationId, newVersionString, accuracy, DateTime.UtcNow);
            await _notifier.NotifyStatsUpdatedAsync(organizationId);
        }

        return new RetrainResultDto
        {
            Success = true,
            Message = $"Model successfully trained and deployed with {accuracy}% accuracy ({top3Accuracy}% Top-3) across {samplesCount} resolved tickets.",
            Version = newVersionString,
            Accuracy = accuracy,
            Top3Accuracy = top3Accuracy,
            SamplesCount = samplesCount,
            TrainedAt = DateTime.UtcNow
        };
    }

    public async Task<SmartAssignStatsDto> GetStatsAsync(Guid organizationId)
    {
        var config = await GetOrCreateConfigAsync(organizationId);

        int unassignedCount = await _context.Tickets
            .IgnoreQueryFilters()
            .CountAsync(t => t.OrganizationId == organizationId && !t.IsDeleted && t.AssignedToUserId == null &&
                             (t.Status == TicketStatus.Open || t.Status == TicketStatus.Pending));

        var today = DateTime.UtcNow.Date;
        int autoAssignedToday = await _context.AIAssignmentLogs
            .IgnoreQueryFilters()
            .CountAsync(l => l.OrganizationId == organizationId && l.WasAutoAssigned && l.CreatedAt >= today);

        int autoAssignedTotal = await _context.AIAssignmentLogs
            .IgnoreQueryFilters()
            .CountAsync(l => l.OrganizationId == organizationId && l.WasAutoAssigned);

        int totalDecisions = await _context.AIAssignmentLogs
            .IgnoreQueryFilters()
            .CountAsync(l => l.OrganizationId == organizationId);

        int overrideCount = await _context.AIAssignmentLogs
            .IgnoreQueryFilters()
            .CountAsync(l => l.OrganizationId == organizationId && l.IsOverridden);

        double overrideRate = totalDecisions > 0 
            ? Math.Round(((double)overrideCount / totalDecisions) * 100.0, 1) 
            : 0.0;

        var activeModel = await _context.AIModelVersions
            .IgnoreQueryFilters()
            .Where(m => m.OrganizationId == organizationId && m.IsActive)
            .OrderByDescending(m => m.TrainedAt)
            .FirstOrDefaultAsync();

        // Agents workload
        var agents = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.OrganizationId == organizationId && (u.Role == UserRole.Agent || u.Role == UserRole.CompanyAdmin))
            .ToListAsync();

        var agentIds = agents.Select(a => a.Id).ToList();

        var onlineMap = await _context.UserOnlineStatuses
            .Where(s => agentIds.Contains(s.UserId))
            .ToDictionaryAsync(s => s.UserId, s => s.IsOnline);

        var activeTicketsMap = await _context.Tickets
            .IgnoreQueryFilters()
            .Where(t => t.OrganizationId == organizationId && !t.IsDeleted && t.AssignedToUserId != null &&
                        (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress || t.Status == TicketStatus.Pending))
            .GroupBy(t => t.AssignedToUserId!.Value)
            .Select(g => new
            {
                AgentId = g.Key,
                Total = g.Count(),
                High = g.Count(x => x.Priority == TicketPriority.High || x.Priority == TicketPriority.Critical)
            })
            .ToDictionaryAsync(x => x.AgentId, x => (x.Total, x.High));

        var assignedTodayMap = await _context.Tickets
            .IgnoreQueryFilters()
            .Where(t => t.OrganizationId == organizationId && !t.IsDeleted && t.AssignedToUserId != null && t.CreatedAt >= today)
            .GroupBy(t => t.AssignedToUserId!.Value)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        // Top category per agent
        var topCategoryMap = await _context.AIAgentSkillStats
            .IgnoreQueryFilters()
            .Where(s => s.OrganizationId == organizationId)
            .GroupBy(s => s.AgentId)
            .Select(g => g.OrderByDescending(x => x.TotalResolved).FirstOrDefault())
            .ToDictionaryAsync(x => x!.AgentId, x => x!.Category);

        var workloads = agents.Select(a =>
        {
            activeTicketsMap.TryGetValue(a.Id, out var counts);
            assignedTodayMap.TryGetValue(a.Id, out var todayAssigned);
            onlineMap.TryGetValue(a.Id, out var isOnline);
            topCategoryMap.TryGetValue(a.Id, out var topCat);

            return new AgentWorkloadSummaryDto
            {
                AgentId = a.Id,
                AgentName = a.FullName,
                AgentEmail = a.Email,
                PhotoUrl = a.PhotoUrl,
                ActiveTicketCount = counts.Total,
                HighPriorityTicketCount = counts.High,
                AssignedTodayCount = todayAssigned,
                IsOnline = isOnline,
                TopCategory = topCat ?? "General"
            };
        })
        .OrderBy(w => w.ActiveTicketCount)
        .ToList();

        return new SmartAssignStatsDto
        {
            UnassignedTicketsCount = unassignedCount,
            AutoAssignedToday = autoAssignedToday,
            AutoAssignedTotal = autoAssignedTotal,
            ModelAccuracy = activeModel?.Accuracy ?? 0.0,
            ModelTop3Accuracy = activeModel?.Top3Accuracy ?? 0.0,
            OverrideRate = overrideRate,
            TotalDecisions = totalDecisions,
            OverrideCount = overrideCount,
            ActiveModelVersion = activeModel?.Version ?? "Cold-Start (Heuristics)",
            LastTrainedAt = activeModel?.TrainedAt,
            TrainingSamplesCount = activeModel?.TrainingSamplesCount ?? 0,
            Mode = config.Mode,
            ConfidenceThreshold = config.ConfidenceThreshold,
            WeightMl = config.WeightMl,
            WeightWorkload = config.WeightWorkload,
            WeightAvailability = config.WeightAvailability,
            AgentWorkloads = workloads
        };
    }

    public async Task<List<TicketSmartAssignResultDto>> GetUnassignedQueueAsync(Guid organizationId, int limit = 20)
    {
        var unassignedTicketIds = await _context.Tickets
            .IgnoreQueryFilters()
            .Where(t => t.OrganizationId == organizationId && !t.IsDeleted && t.AssignedToUserId == null &&
                        (t.Status == TicketStatus.Open || t.Status == TicketStatus.Pending))
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Take(limit)
            .Select(t => t.Id)
            .ToListAsync();

        var results = new List<TicketSmartAssignResultDto>();
        foreach (var id in unassignedTicketIds)
        {
            var eval = await EvaluateTicketAsync(id, organizationId, allowAutoAssign: false);
            if (eval != null)
            {
                results.Add(eval);
            }
        }

        return results;
    }

    public async Task<AISmartAssignConfig> GetOrCreateConfigAsync(Guid organizationId)
    {
        var config = await _context.AISmartAssignConfigs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.OrganizationId == organizationId);

        if (config == null)
        {
            config = new AISmartAssignConfig
            {
                OrganizationId = organizationId,
                Mode = "Suggest",
                ConfidenceThreshold = 0.75,
                WeightMl = 0.60,
                WeightWorkload = 0.20,
                WeightAvailability = 0.20,
                AutoRetrainFrequencyDays = 7,
                MinTrainingSamples = 50,
                UpdatedAt = DateTime.UtcNow
            };
            _context.AISmartAssignConfigs.Add(config);
            await _context.SaveChangesAsync();
        }

        return config;
    }

    public async Task<AISmartAssignConfig> UpdateConfigAsync(Guid organizationId, UpdateSmartAssignConfigDto dto)
    {
        var config = await GetOrCreateConfigAsync(organizationId);

        if (!string.IsNullOrWhiteSpace(dto.Mode))
        {
            var normalized = dto.Mode.Trim();
            if (normalized.Equals("Auto", StringComparison.OrdinalIgnoreCase)) config.Mode = "Auto";
            else if (normalized.Equals("Suggest", StringComparison.OrdinalIgnoreCase)) config.Mode = "Suggest";
        }

        if (dto.ConfidenceThreshold.HasValue && dto.ConfidenceThreshold.Value is >= 0.1 and <= 1.0)
        {
            config.ConfidenceThreshold = dto.ConfidenceThreshold.Value;
        }

        if (dto.WeightMl.HasValue) config.WeightMl = Math.Clamp(dto.WeightMl.Value, 0.0, 1.0);
        if (dto.WeightWorkload.HasValue) config.WeightWorkload = Math.Clamp(dto.WeightWorkload.Value, 0.0, 1.0);
        if (dto.WeightAvailability.HasValue) config.WeightAvailability = Math.Clamp(dto.WeightAvailability.Value, 0.0, 1.0);

        // Normalize weights sum to 1.0
        double sum = config.WeightMl + config.WeightWorkload + config.WeightAvailability;
        if (sum > 0)
        {
            config.WeightMl = Math.Round(config.WeightMl / sum, 2);
            config.WeightWorkload = Math.Round(config.WeightWorkload / sum, 2);
            config.WeightAvailability = Math.Round(config.WeightAvailability / sum, 2);
        }

        config.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        if (_notifier != null)
        {
            await _notifier.NotifyStatsUpdatedAsync(organizationId);
        }

        return config;
    }

    public async Task RefreshAgentSkillStatsAsync(Guid organizationId)
    {
        _logger.LogInformation("Refreshing agent category skill statistics for Org {OrgId}", organizationId);

        var resolvedData = await _context.Tickets
            .IgnoreQueryFilters()
            .Where(t => t.OrganizationId == organizationId && !t.IsDeleted && t.AssignedToUserId != null &&
                        (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed || t.Status == TicketStatus.ResolvedOnBeta))
            .GroupBy(t => new { AgentId = t.AssignedToUserId!.Value, Category = string.IsNullOrWhiteSpace(t.Category) ? "General" : t.Category })
            .Select(g => new
            {
                g.Key.AgentId,
                g.Key.Category,
                TotalResolved = g.Count(),
                AvgResolutionHours = g.Average(x => EF.Functions.DateDiffMinute(x.CreatedAt, x.ResolvedAt ?? x.UpdatedAt ?? DateTime.UtcNow)) / 60.0,
                SlaComplianceRate = g.Average(x => x.IsSlaBreached ? 0.0 : 1.0) * 100.0,
                LastResolved = g.Max(x => x.ResolvedAt)
            })
            .ToListAsync();

        var existingStats = await _context.AIAgentSkillStats
            .IgnoreQueryFilters()
            .Where(s => s.OrganizationId == organizationId)
            .ToListAsync();

        var existingDict = existingStats.ToDictionary(s => (s.AgentId, s.Category.ToLower()), s => s);

        foreach (var row in resolvedData)
        {
            var key = (row.AgentId, row.Category.ToLower());
            if (existingDict.TryGetValue(key, out var stat))
            {
                stat.TotalResolved = row.TotalResolved;
                stat.AvgResolutionTimeHours = Math.Round(row.AvgResolutionHours, 1);
                stat.SlaComplianceRate = Math.Round(row.SlaComplianceRate, 1);
                stat.LastResolvedAt = row.LastResolved;
                stat.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.AIAgentSkillStats.Add(new AIAgentSkillStat
                {
                    OrganizationId = organizationId,
                    AgentId = row.AgentId,
                    Category = row.Category,
                    TotalResolved = row.TotalResolved,
                    AvgResolutionTimeHours = Math.Round(row.AvgResolutionHours, 1),
                    SlaComplianceRate = Math.Round(row.SlaComplianceRate, 1),
                    LastResolvedAt = row.LastResolved,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
    }
}
