using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.API.Services;

/// <summary>
/// Runs at app startup. Ensures the 3 default plans exist and that every
/// organization has at least one OrganizationSubscription row (Trial Growth
/// for 30 days for pre-existing orgs without a plan).
/// </summary>
public static class SubscriptionSeeder
{
    // ── Default tier definitions ──────────────────────────────────────────
    // Feature keys match the frontend module/feature keys used by permissions
    // and the plans.data.ts catalogue.
    // ── Shared Growth feature keys ──────────────────────────────────────────
    private static readonly string[] GrowthFeatures = new[]
    {
        // Core modules (all plans)
        "dashboard", "tickets", "contacts", "todo", "notes", "calendar",
        "knowledge-base", "chat", "search", "profile", "notifications",
        "agents", "settings", "email-integration", "email-notifications",
        "canned-responses", "ticket-masters",
        // Advanced features available in Growth
        "analytics-heatmap", "ai-insights", "sla-policies",
        "reports", "audit-log", "whatsapp", "custom-fields", "business-hours",
    };

    private static readonly string[] ProOnlyFeatures = new[]
    {
        // Features added in Pro (Growth + these)
        "ticket-templates", "agent-groups", "holiday-setup", "mateboard",
        "recycle-bin", "role-rights", "organization-profile",
        "call-logs", "slack", "customer-portal",
    };

    private static readonly string[] EnterpriseOnlyFeatures = new[]
    {
        // Features added in Enterprise (Pro + these)
        "sso", "multi-org", "leads",
    };

    private static readonly (string Tier, string Name, string Tagline, string Accent, int Sort, decimal Price, string Features)[] Defaults =
    new[]
    {
        ("growth", "Growth",
         "Chhote teams ke liye — basic helpdesk",
         "#2563eb", 0, 500m,
         string.Join(',', GrowthFeatures)
        ),
        ("pro", "Pro",
         "Growing teams — best value, aapke liye best",
         "#7c3aed", 1, 1200m,
         string.Join(',', GrowthFeatures.Concat(ProOnlyFeatures))
        ),
        ("enterprise", "Enterprise",
         "Large orgs — full control + priority support",
         "#0f766e", 2, 2000m,
         string.Join(',', GrowthFeatures.Concat(ProOnlyFeatures).Concat(EnterpriseOnlyFeatures))
        ),
    };

    public static async Task SeedAsync(ApplicationDbContext db)
    {
        // 1. Plans
        var existingPlans = await db.SubscriptionPlans.ToListAsync();
        foreach (var def in Defaults)
        {
            var p = existingPlans.FirstOrDefault(x => x.TierKey == def.Tier);
            if (p == null)
            {
                db.SubscriptionPlans.Add(new SubscriptionPlan
                {
                    TierKey = def.Tier,
                    Name = def.Name,
                    Tagline = def.Tagline,
                    Accent = def.Accent,
                    SortOrder = def.Sort,
                    Currency = "INR",
                    MonthlyPricePerAgent = def.Price,
                    AnnualDiscountPct = 20m,
                    FeatureKeysCsv = def.Features,
                    IsActive = true,
                });
            }
            else
            {
                // Always sync feature keys so plan changes in code propagate to DB on restart.
                p.FeatureKeysCsv = def.Features;
                p.Tagline = def.Tagline;
            }
        }
        await db.SaveChangesAsync();

        // 2. Trial subscription for orgs that don't have one
        var growth = await db.SubscriptionPlans.FirstAsync(x => x.TierKey == "growth");
        var orgs = await db.Organizations.IgnoreQueryFilters().Select(o => o.Id).ToListAsync();
        var orgsWithSub = await db.OrganizationSubscriptions
            .IgnoreQueryFilters()
            .Where(s => s.Status == SubscriptionStatus.Trial
                     || s.Status == SubscriptionStatus.Active
                     || s.Status == SubscriptionStatus.PastDue)
            .Select(s => s.OrganizationId)
            .ToListAsync();
        var missing = orgs.Except(orgsWithSub).ToList();
        if (missing.Count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var orgId in missing)
            {
                db.OrganizationSubscriptions.Add(new OrganizationSubscription
                {
                    OrganizationId = orgId,
                    PlanId = growth.Id,
                    BillingCycle = BillingCycle.Monthly,
                    AgentSeats = 1,
                    Status = SubscriptionStatus.Trial,
                    Amount = 0,
                    Currency = "INR",
                    StartedAt = now,
                    CurrentPeriodEnd = now.AddDays(30),
                });
            }
            await db.SaveChangesAsync();
        }
    }
}
