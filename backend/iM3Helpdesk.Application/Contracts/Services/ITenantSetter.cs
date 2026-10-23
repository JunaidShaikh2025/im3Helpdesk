using System;

namespace iM3Helpdesk.Application.Contracts.Services;

/// <summary>
/// Defines mutation operations for setting the current tenant context.
/// Implemented by scoped tenant state providers and consumed by middleware.
/// </summary>
public interface ITenantSetter
{
    void SetTenant(Guid? organizationId, bool isSuperAdmin)
    {
    }

    void SetOrganizationId(Guid? organizationId)
    {
    }

    void SetSuperAdmin(bool isSuperAdmin)
    {
    }
}
