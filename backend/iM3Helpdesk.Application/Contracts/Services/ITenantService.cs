namespace iM3Helpdesk.Application.Contracts.Services;

public interface ITenantService
{
    Guid? OrganizationId { get; }
    bool IsSuperAdmin { get; }
}
