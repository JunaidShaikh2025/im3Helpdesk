using System.Text.RegularExpressions;
using iM3Helpdesk.Application.Common;
using iM3Helpdesk.Application.Contracts.Persistence;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.DTOs.AI;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace iM3Helpdesk.Application.Services;

public class AIFeaturesService : IAIFeaturesService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenantService;
    private readonly ILogger<AIFeaturesService> _logger;

    public AIFeaturesService(
        IUnitOfWork uow,
        ITenantService tenantService,
        ILogger<AIFeaturesService> logger)
    {
        _uow = uow;
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<ServiceResult<object>> GetTideForecastAsync(int days = 7, CancellationToken ct = default)
    {
        if (!_tenantService.OrganizationId.HasValue)
            return ServiceResult<object>.BadRequest("Tenant context missing");

        var orgId = _tenantService.OrganizationId.Value;
        var cutoff = DateTime.UtcNow.AddDays(-90);

        var tickets = await _uow.Tickets.GetHistoricalTicketsAsync(orgId, cutoff, false, ct);

        var ticketData = tickets.Select(t => new
        {
            t.CreatedAt,
            DayOfWeek = (int)t.CreatedAt.DayOfWeek,
            Hour = t.CreatedAt.Hour
        }).ToList();

        var hourlyByDay = ticketData
            .GroupBy(t => new { t.DayOfWeek, t.Hour })
            .Select(g => new
            {
                g.Key.DayOfWeek,
                g.Key.Hour,
                AvgCount = g.Count() / 13.0
            })
            .ToList();

        var forecast = new List<object>();
        var now = DateTime.UtcNow.Date;

        for (int d = 0; d < days; d++)
        {
            var date = now.AddDays(d + 1);
            var dow = (int)date.DayOfWeek;

            var hourly = Enumerable.Range(0, 24).Select(h =>
            {
                var hist = hourlyByDay.FirstOrDefault(x => x.DayOfWeek == dow && x.Hour == h);
                var avg = hist?.AvgCount ?? 0;

                var jitter = avg > 0
                    ? avg * (0.85 + new Random(d * 24 + h).NextDouble() * 0.3)
                    : 0;

                return new
                {
                    Hour = h,
                    Predicted = Math.Round(jitter, 1),
                    Label = h == 0 ? "12am" : h < 12 ? $"{h}am" : h == 12 ? "12pm" : $"{h - 12}pm"
                };
            }).ToList();

            var totalPredicted = hourly.Sum(h => h.Predicted);
            var level = totalPredicted > 20 ? "High" : totalPredicted > 10 ? "Medium" : "Low";

            forecast.Add(new
            {
                Date = date.ToString("yyyy-MM-dd"),
                DayName = date.ToString("dddd"),
                ShortDay = date.ToString("ddd dd MMM"),
                TotalPredicted = Math.Round(totalPredicted, 0),
                TideLevel = level,
                Hourly = hourly,
                PeakHour = hourly.OrderByDescending(h => h.Predicted).First().Hour,
                PeakLabel = hourly.OrderByDescending(h => h.Predicted).First().Label
            });
        }

        var last7 = ticketData
            .Where(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key.ToString("ddd dd"),
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToList();

        var result = new
        {
            forecast,
            historical = last7,
            avgDailyActual = tickets.Any() ? Math.Round(tickets.Count / 90.0, 1) : 0,
            totalLast90Days = tickets.Count
        };

        return ServiceResult<object>.Success(result);
    }

    public async Task<ServiceResult<object>> GetInsightsAsync(CancellationToken ct = default)
    {
        if (!_tenantService.OrganizationId.HasValue)
            return ServiceResult<object>.BadRequest("Tenant context missing");

        var orgId = _tenantService.OrganizationId.Value;
        var now = DateTime.UtcNow;
        var last30 = now.AddDays(-30);
        var prev30 = now.AddDays(-60);

        var tickets = await _uow.Tickets.GetHistoricalTicketsAsync(orgId, prev30, true, ct);

        var current = tickets.Where(t => t.CreatedAt >= last30).ToList();
        var previous = tickets.Where(t => t.CreatedAt >= prev30 && t.CreatedAt < last30).ToList();

        var resolved = current.Where(t => t.ResolvedAt.HasValue).ToList();
        var avgResHours = resolved.Any()
            ? resolved.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
            : 0;

        var breachRate = current.Any()
            ? current.Count(t => t.IsSlaBreached) / (double)current.Count * 100
            : 0;

        var volChange = previous.Count > 0
            ? (current.Count - previous.Count) / (double)previous.Count * 100
            : 0;

        var topCat = current
            .GroupBy(t => t.Category)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        var busiestDay = current
            .GroupBy(t => t.CreatedAt.DayOfWeek)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        var agentStats = current
            .Where(t => t.AssignedTo != null)
            .GroupBy(t => t.AssignedTo!.FullName)
            .Select(g => new
            {
                Agent = g.Key,
                Total = g.Count(),
                Resolved = g.Count(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed),
                ResRate = g.Count() > 0
                    ? Math.Round(g.Count(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed) / (double)g.Count() * 100, 1)
                    : 0
            })
            .OrderByDescending(a => a.ResRate)
            .ToList();

        var unassigned = current.Count(t => t.AssignedToUserId == null);

        var insights = new List<object>();

        insights.Add(new
        {
            type = volChange > 10 ? "warning" : volChange < -10 ? "positive" : "neutral",
            icon = volChange > 0 ? "📈" : "📉",
            title = "Ticket Volume",
            text = volChange > 0
                ? $"Ticket volume is up {Math.Abs(volChange):F0}% vs last month. Consider adding more support staff."
                : volChange < 0
                ? $"Ticket volume dropped {Math.Abs(volChange):F0}% vs last month. Team performance is improving!"
                : "Ticket volume is stable compared to last month.",
            metric = $"{(volChange > 0 ? "+" : "")}{volChange:F0}%",
            value = current.Count
        });

        insights.Add(new
        {
            type = avgResHours > 48 ? "warning" : avgResHours < 24 ? "positive" : "neutral",
            icon = "⏱",
            title = "Resolution Time",
            text = avgResHours > 48
                ? $"Average resolution is {avgResHours:F0}h — above 48h target. Review backlog."
                : $"Average resolution is {avgResHours:F1}h — " + (avgResHours < 24 ? "excellent performance!" : "within acceptable range."),
            metric = $"{avgResHours:F1}h avg",
            value = resolved.Count
        });

        insights.Add(new
        {
            type = breachRate > 20 ? "critical" : breachRate > 10 ? "warning" : "positive",
            icon = "🎯",
            title = "SLA Compliance",
            text = breachRate > 20
                ? $"{breachRate:F0}% of tickets breached SLA. Urgent attention needed!"
                : breachRate > 10
                ? $"{breachRate:F0}% SLA breach rate. Monitor closely."
                : $"Only {breachRate:F0}% SLA breaches this month. Great job!",
            metric = $"{100 - breachRate:F0}% compliant",
            value = current.Count(t => t.IsSlaBreached)
        });

        if (topCat != null)
        {
            insights.Add(new
            {
                type = "info",
                icon = "📂",
                title = "Top Category",
                text = $"'{topCat.Key}' is your most common category with {topCat.Count()} tickets. Consider creating FAQ or automation for it.",
                metric = $"{topCat.Count()} tickets",
                value = topCat.Count()
            });
        }

        if (unassigned > 0)
        {
            insights.Add(new
            {
                type = unassigned > 5 ? "warning" : "neutral",
                icon = "👤",
                title = "Unassigned Tickets",
                text = $"{unassigned} tickets are unassigned. " + (unassigned > 5 ? "Assign them immediately to avoid SLA breaches." : "Review and assign soon."),
                metric = $"{unassigned} pending",
                value = unassigned
            });
        }

        if (agentStats.Any())
        {
            var top = agentStats.First();
            insights.Add(new
            {
                type = "positive",
                icon = "🏆",
                title = "Top Performer",
                text = $"{top.Agent} has the best resolution rate at {top.ResRate}% ({top.Resolved}/{top.Total} tickets). Star of the month!",
                metric = $"{top.ResRate}% resolved",
                value = top.Total
            });
        }

        if (busiestDay != null)
        {
            insights.Add(new
            {
                type = "info",
                icon = "📅",
                title = "Busiest Day",
                text = $"{busiestDay.Key} gets the most tickets ({busiestDay.Count()} avg). Ensure full staffing on this day.",
                metric = $"{busiestDay.Key}",
                value = busiestDay.Count()
            });
        }

        var result = new
        {
            insights,
            summary = new
            {
                totalTickets = current.Count,
                resolved = resolved.Count,
                avgResHours = Math.Round(avgResHours, 1),
                slaCompliance = Math.Round(100 - breachRate, 1),
                unassigned,
                topCategory = topCat?.Key,
                topAgent = agentStats.FirstOrDefault()?.Agent,
                agentStats
            }
        };

        return ServiceResult<object>.Success(result);
    }

    public async Task<ServiceResult<object>> DetectDuplicatesAsync(CancellationToken ct = default)
    {
        if (!_tenantService.OrganizationId.HasValue)
            return ServiceResult<object>.BadRequest("Tenant context missing");

        var orgId = _tenantService.OrganizationId.Value;
        var cutoff = DateTime.UtcNow.AddDays(-30);

        var tickets = await _uow.Tickets.GetOpenTicketsAsync(orgId, cutoff, ct);

        var duplicateGroups = new List<object>();
        var processed = new HashSet<Guid>();

        for (int i = 0; i < tickets.Count; i++)
        {
            if (processed.Contains(tickets[i].Id))
                continue;

            var group = new List<object>();
            var t1 = tickets[i];
            var t1Words = GetWords(t1.Title);

            for (int j = i + 1; j < tickets.Count; j++)
            {
                var t2 = tickets[j];
                if (processed.Contains(t2.Id))
                    continue;

                var t2Words = GetWords(t2.Title);
                var sim = GetSimilarity(t1Words, t2Words);
                var sameUser = t1.CreatedByUserId == t2.CreatedByUserId;
                var sameCategory = t1.Category == t2.Category;

                var score = sim + (sameUser ? 0.2 : 0) + (sameCategory ? 0.1 : 0);

                if (score >= 0.6)
                {
                    if (!group.Any())
                    {
                        group.Add(new
                        {
                            id = t1.Id,
                            ticketNumber = t1.TicketNumber,
                            title = t1.Title,
                            status = t1.Status.ToString(),
                            createdAt = t1.CreatedAt,
                            isOriginal = true,
                            similarity = 100
                        });
                    }

                    group.Add(new
                    {
                        id = t2.Id,
                        ticketNumber = t2.TicketNumber,
                        title = t2.Title,
                        status = t2.Status.ToString(),
                        createdAt = t2.CreatedAt,
                        isOriginal = false,
                        similarity = (int)(score * 100)
                    });

                    processed.Add(t2.Id);
                }
            }

            if (group.Any())
            {
                processed.Add(t1.Id);
                duplicateGroups.Add(new
                {
                    groupId = Guid.NewGuid(),
                    count = group.Count,
                    tickets = group,
                    confidence = group.Count > 0 ? "High" : "Medium"
                });
            }
        }

        var result = new
        {
            groups = duplicateGroups,
            totalDuplicates = duplicateGroups.Count,
            potentialSavings = duplicateGroups.Sum(g => ((dynamic)g).count - 1)
        };

        return ServiceResult<object>.Success(result);
    }

    public async Task<ServiceResult<MergeTicketsResultDto>> MergeTicketsAsync(MergeTicketsDto dto, Guid userId, CancellationToken ct = default)
    {
        if (dto.TicketIdsToClose == null || !dto.TicketIdsToClose.Any())
            return ServiceResult<MergeTicketsResultDto>.BadRequest("No tickets to merge");

        if (!_tenantService.OrganizationId.HasValue)
            return ServiceResult<MergeTicketsResultDto>.BadRequest("Tenant context missing");

        var orgId = _tenantService.OrganizationId.Value;

        var merged = await _uow.Tickets.MergeDuplicateTicketsAsync(dto.OriginalTicketId, dto.TicketIdsToClose, userId, orgId, ct);
        if (merged < 0)
            return ServiceResult<MergeTicketsResultDto>.NotFound("Original ticket not found");

        var original = await _uow.Tickets.GetByIdAsync(dto.OriginalTicketId, true, ct);

        return ServiceResult<MergeTicketsResultDto>.Success(new MergeTicketsResultDto
        {
            Message = $"{merged} ticket(s) merged into #TN{original?.TicketNumber}",
            OriginalId = dto.OriginalTicketId,
            MergedCount = merged
        });
    }

    public async Task<ServiceResult<object>> GetTicketSummaryAsync(Guid ticketId, CancellationToken ct = default)
    {
        var ticket = await _uow.Tickets.GetTicketSummaryDataAsync(ticketId, ct);
        if (ticket == null)
            return ServiceResult<object>.NotFound("Ticket not found");

        var commentCount = ticket.Comments.Count;
        var lastComment = ticket.Comments.OrderByDescending(c => c.CreatedAt).FirstOrDefault();

        var ageHours = (int)(DateTime.UtcNow - ticket.CreatedAt).TotalHours;
        var ageDays = ageHours / 24;

        var summaryParts = new List<string>();

        var desc = StripHtml(ticket.Description ?? ticket.Title);
        if (desc.Length > 150)
            desc = desc.Substring(0, 147) + "...";

        summaryParts.Add($"Customer reporting: {desc}");

        if (ageDays > 0)
            summaryParts.Add($"Open for {ageDays}d");
        else
            summaryParts.Add($"Opened {ageHours}h ago");

        if (commentCount > 0)
            summaryParts.Add($"{commentCount} response(s)");

        if (ticket.IsSlaBreached)
        {
            summaryParts.Add("⚠️ SLA breached");
        }
        else if (ticket.SlaDeadline.HasValue)
        {
            var remaining = ticket.SlaDeadline.Value - DateTime.UtcNow;
            if (remaining.TotalHours < 2)
                summaryParts.Add($"⚡ SLA due in {(int)remaining.TotalMinutes}min");
            else if (remaining.TotalHours < 24)
                summaryParts.Add($"SLA due in {(int)remaining.TotalHours}h");
        }

        if (lastComment != null)
        {
            var lastAge = (DateTime.UtcNow - lastComment.CreatedAt).TotalHours;
            summaryParts.Add($"Last reply {(lastAge < 1 ? "just now" : $"{(int)lastAge}h ago")}");
        }

        if (ticket.Priority == TicketPriority.Critical)
            summaryParts.Add("🔴 CRITICAL priority");
        else if (ticket.Priority == TicketPriority.High)
            summaryParts.Add("🟠 High priority");

        var oneLiner = string.Join(" · ", summaryParts);

        string action;
        if (ticket.IsSlaBreached)
            action = "Escalate immediately — SLA already breached.";
        else if (!ticket.AssignedToUserId.HasValue)
            action = "Assign to an agent right away.";
        else if (commentCount == 0)
            action = "No response yet — reply to customer.";
        else if (ticket.Priority == TicketPriority.Critical)
            action = "Critical issue — follow up immediately.";
        else
            action = "Review latest comment and update status.";

        var sentiment = "Neutral";
        var lowerDesc = StripHtml(ticket.Description ?? "").ToLower();
        if (lowerDesc.Contains("urgent") || lowerDesc.Contains("asap") || lowerDesc.Contains("critical") || lowerDesc.Contains("immediately"))
            sentiment = "Frustrated";
        else if (lowerDesc.Contains("thank") || lowerDesc.Contains("great") || lowerDesc.Contains("appreciate"))
            sentiment = "Positive";

        var tags = (ticket.Tags ?? "")
            .Split(',')
            .Where(t => !string.IsNullOrEmpty(t.Trim()))
            .Select(t => t.Trim())
            .ToList();

        var result = new
        {
            ticketId,
            oneLiner,
            suggestedAction = action,
            sentiment,
            keyFacts = summaryParts,
            stats = new
            {
                ageHours,
                ageDays,
                commentCount,
                isBreached = ticket.IsSlaBreached,
                priority = ticket.Priority.ToString(),
                status = ticket.Status.ToString(),
                tags
            },
            assignedTo = ticket.AssignedTo?.FullName,
            createdBy = ticket.CreatedBy?.FullName,
            generatedAt = DateTime.UtcNow
        };

        return ServiceResult<object>.Success(result);
    }

    public async Task<ServiceResult<object>> GetHotTopicsAsync(CancellationToken ct = default)
    {
        if (!_tenantService.OrganizationId.HasValue)
            return ServiceResult<object>.BadRequest("Tenant context missing");

        var orgId = _tenantService.OrganizationId.Value;
        var now = DateTime.UtcNow;
        var last30 = now.AddDays(-30);
        var prev30 = now.AddDays(-60);

        var tickets = await _uow.Tickets.GetHistoricalTicketsAsync(orgId, prev30, false, ct);

        var current = tickets.Where(t => t.CreatedAt >= last30).ToList();
        var previous = tickets.Where(t => t.CreatedAt >= prev30 && t.CreatedAt < last30).ToList();

        List<object> TopWithTrend<TKey>(Func<Ticket, TKey> selector, int take = 7) where TKey : notnull
        {
            var curMap = current.GroupBy(selector).ToDictionary(g => g.Key, g => g.Count());
            var prevMap = previous.GroupBy(selector).ToDictionary(g => g.Key, g => g.Count());

            return curMap
                .OrderByDescending(kv => kv.Value)
                .Take(take)
                .Select(kv =>
                {
                    var prevCount = prevMap.TryGetValue(kv.Key, out var p) ? p : 0;
                    double change = prevCount > 0
                        ? (kv.Value - prevCount) / (double)prevCount * 100
                        : (kv.Value > 0 ? 100 : 0);
                    string trend = change > 15 ? "up" : change < -15 ? "down" : "flat";
                    double sharePct = current.Count > 0
                        ? Math.Round(kv.Value / (double)current.Count * 100, 1)
                        : 0;

                    return (object)new
                    {
                        label = kv.Key?.ToString() ?? "(none)",
                        count = kv.Value,
                        previousCount = prevCount,
                        changePct = Math.Round(change, 1),
                        trend,
                        sharePct
                    };
                })
                .ToList();
        }

        var topCategories = TopWithTrend(t => string.IsNullOrWhiteSpace(t.Category) ? "Uncategorized" : t.Category);
        var topTypes = TopWithTrend(t => t.TicketType ?? "Unknown");
        var topPriorities = TopWithTrend(t => t.Priority.ToString());

        var tagCur = current
            .SelectMany(t => (t.Tags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().ToLower()).Where(x => x.Length > 0))
            .GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count());

        var tagPrev = previous
            .SelectMany(t => (t.Tags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().ToLower()).Where(x => x.Length > 0))
            .GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count());

        var topTags = tagCur
            .OrderByDescending(kv => kv.Value)
            .Take(10)
            .Select(kv =>
            {
                var pc = tagPrev.TryGetValue(kv.Key, out var p) ? p : 0;
                double change = pc > 0 ? (kv.Value - pc) / (double)pc * 100 : (kv.Value > 0 ? 100 : 0);
                return new
                {
                    label = kv.Key,
                    count = kv.Value,
                    previousCount = pc,
                    changePct = Math.Round(change, 1),
                    trend = change > 15 ? "up" : change < -15 ? "down" : "flat"
                };
            })
            .ToList();

        var stopWords = new HashSet<string>(new[]
        {
            "the","and","for","with","from","this","that","have","has","not","but","you","your","our","are","was","were","will","can","cannot","cant","please","help","need","issue","problem","support","ticket","tickets","new","old","get","got","when","how","what","why","who","about","into","onto","over","under","still","just","also","very","much","more","less","than","then","there","their","they","them","its","it's","doesnt","doesn't","didnt","didn't","isnt","isn't","wont","won't","regarding","kindly"
        });

        var wordCounts = current
            .SelectMany(t => (t.Title ?? "").ToLowerInvariant().Split(new[] { ' ', ',', '.', '!', '?', '-', '_', '/', ':', ';', '(', ')', '[', ']', '"', '\'', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Where(w => w.Length >= 4 && !stopWords.Contains(w) && !w.All(char.IsDigit)))
            .GroupBy(w => w)
            .Select(g => new { word = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .Take(20)
            .ToList();

        var resByCategory = current
            .Where(t => t.ResolvedAt.HasValue)
            .GroupBy(t => string.IsNullOrWhiteSpace(t.Category) ? "Uncategorized" : t.Category)
            .Where(g => g.Count() >= 2)
            .Select(g => new
            {
                label = g.Key,
                count = g.Count(),
                avgHours = Math.Round(g.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours), 1)
            })
            .OrderByDescending(x => x.avgHours)
            .Take(7)
            .ToList();

        var aiInsights = new List<object>();

        if (topCategories.Any())
        {
            dynamic c = topCategories.First();
            aiInsights.Add(new
            {
                type = "info",
                icon = "🏷️",
                text = $"\"{c.label}\" is the most common category ({c.count} tickets, {c.sharePct}% of total)."
            });
        }

        var surging = topCategories
            .Cast<dynamic>()
            .Where(c => c.trend == "up" && c.changePct >= 30)
            .OrderByDescending(c => c.changePct)
            .FirstOrDefault();
        if (surging != null)
        {
            aiInsights.Add(new
            {
                type = "warning",
                icon = "📈",
                text = $"\"{surging.label}\" tickets are surging — up {surging.changePct}% vs previous month. Investigate root cause."
            });
        }

        var dropping = topCategories
            .Cast<dynamic>()
            .Where(c => c.trend == "down" && c.changePct <= -30)
            .OrderBy(c => c.changePct)
            .FirstOrDefault();
        if (dropping != null)
        {
            aiInsights.Add(new
            {
                type = "positive",
                icon = "📉",
                text = $"\"{dropping.label}\" tickets dropped {Math.Abs(dropping.changePct)}% — recent fixes working."
            });
        }

        if (resByCategory.Any())
        {
            var slowest = resByCategory.First();
            aiInsights.Add(new
            {
                type = "critical",
                icon = "⏰",
                text = $"\"{slowest.label}\" tickets take longest to resolve (avg {slowest.avgHours}h). Consider a KB article or automation."
            });
        }

        if (wordCounts.Any())
        {
            var topKw = wordCounts.First();
            aiInsights.Add(new
            {
                type = "info",
                icon = "🔑",
                text = $"Keyword \"{topKw.word}\" appears in {topKw.count} ticket titles — a likely recurring theme."
            });
        }

        var totalChange = previous.Count > 0
            ? Math.Round((current.Count - previous.Count) / (double)previous.Count * 100, 1)
            : 0;

        var result = new
        {
            period = new
            {
                currentStart = last30,
                currentEnd = now,
                previousStart = prev30,
                previousEnd = last30
            },
            totals = new
            {
                currentCount = current.Count,
                previousCount = previous.Count,
                changePct = totalChange,
                trend = totalChange > 15 ? "up" : totalChange < -15 ? "down" : "flat"
            },
            topCategories,
            topTicketTypes = topTypes,
            topPriorities,
            topTags,
            topKeywords = wordCounts,
            slowestCategories = resByCategory,
            aiInsights
        };

        return ServiceResult<object>.Success(result);
    }

    private static string StripHtml(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        var noTags = Regex.Replace(input, "<[^>]*>", " ");
        noTags = noTags
            .Replace("&nbsp;", " ")
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&#39;", "'")
            .Replace("&apos;", "'");

        return Regex.Replace(noTags, @"\s+", " ").Trim();
    }

    private static List<string> GetWords(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new List<string>();

        return text.ToLower()
            .Split(new[] { ' ', ',', '.', '!', '?', '-', '_', '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .ToList();
    }

    private static double GetSimilarity(List<string> words1, List<string> words2)
    {
        if (!words1.Any() || !words2.Any())
            return 0;

        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();

        return union > 0 ? (double)intersection / union : 0;
    }
}
