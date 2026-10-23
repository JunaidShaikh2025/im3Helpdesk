namespace iM3Helpdesk.Application.Contracts.Services;

/// <summary>
/// Combines read-only tenant retrieval (ITenantService) with safe scoped tenant mutation (ITenantSetter)
/// for application, infrastructure, and request pipeline consumers.
/// </summary>
public interface ICurrentTenantService : ITenantService, ITenantSetter
{
}
