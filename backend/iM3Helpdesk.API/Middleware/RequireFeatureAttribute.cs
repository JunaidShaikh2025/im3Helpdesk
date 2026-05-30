using iM3Helpdesk.API.Services;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace iM3Helpdesk.API.Middleware;

/// <summary>
/// Action filter that returns 403 unless the current tenant's active
/// subscription includes the requested feature key. SuperAdmin bypasses
/// all checks.
///
///   [RequireFeature("whatsapp")]
///   [HttpGet]
///   public ... { }
///
/// Apply at the controller level or per action.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireFeatureAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _featureKey;
    public RequireFeatureAttribute(string featureKey)
    {
        _featureKey = (featureKey ?? string.Empty).Trim().ToLowerInvariant();
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (string.IsNullOrEmpty(_featureKey)) return;

        var tenant = context.HttpContext.RequestServices.GetService<ICurrentTenantService>();
        if (tenant == null) return;

        // SuperAdmin always has access (used for platform admin endpoints)
        if (tenant.IsSuperAdmin) return;

        var orgId = tenant.OrganizationId;
        if (!orgId.HasValue)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var sub = context.HttpContext.RequestServices.GetService<ISubscriptionService>();
        if (sub == null) return;

        var allowed = await sub.HasFeatureAsync(orgId.Value, _featureKey);
        if (!allowed)
        {
            context.Result = new ObjectResult(new
            {
                error = "feature_not_in_plan",
                feature = _featureKey,
                message = $"Your current subscription does not include '{_featureKey}'. Upgrade your plan to use this feature."
            })
            {
                StatusCode = 403
            };
        }
    }
}
