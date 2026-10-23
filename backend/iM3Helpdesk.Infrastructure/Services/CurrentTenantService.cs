using System;
using iM3Helpdesk.Application.Contracts.Services;

namespace iM3Helpdesk.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    public Guid? OrganizationId { get; set; }
    public bool IsSuperAdmin { get; set; }

    public void SetTenant(Guid? organizationId, bool isSuperAdmin)
    {
        OrganizationId = organizationId;
        IsSuperAdmin = isSuperAdmin;
    }

    public void SetOrganizationId(Guid? organizationId)
    {
        OrganizationId = organizationId;
    }

    public void SetSuperAdmin(bool isSuperAdmin)
    {
        IsSuperAdmin = isSuperAdmin;
    }
}