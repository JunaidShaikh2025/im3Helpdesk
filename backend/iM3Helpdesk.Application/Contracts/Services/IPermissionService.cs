using iM3Helpdesk.Domain.Enums;

namespace iM3Helpdesk.Application.Contracts.Services;

public enum PermissionAction { View, Add, Edit, Delete, Export }

public interface IPermissionService
{
    /// <summary>
    /// Resolves whether <paramref name="role"/> in organization <paramref name="orgId"/>
    /// is allowed to perform <paramref name="action"/> on <paramref name="module"/>.
    /// Merges org-level overrides on top of system defaults. SuperAdmin bypasses.
    /// </summary>
    Task<bool> CanAsync(Guid? orgId, UserRole role, string module, PermissionAction action);

    /// <summary>Invalidate the cache for an org (call after RoleRights save/reset).</summary>
    void InvalidateOrg(Guid orgId);
}
