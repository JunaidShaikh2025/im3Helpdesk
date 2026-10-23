using iM3Helpdesk.Application.Common;
using iM3Helpdesk.Application.DTOs.Agents;

namespace iM3Helpdesk.Application.Contracts.Services;

public interface IAgentService
{
    Task<IReadOnlyList<AgentListItemDto>> GetAllAsync(Guid? organizationIdOverride, bool isSuperAdmin, CancellationToken ct = default);
    Task<ServiceResult<AgentDetailDto>> GetByIdAsync(Guid id, Guid? organizationIdOverride, bool isSuperAdmin, CancellationToken ct = default);
    Task<Guid?> ResolveOrganizationIdAsync(Guid? contextOrgId, string? claimOrgId, Guid? userId, CancellationToken ct = default);
    Task<ServiceResult<InviteAgentResultDto>> InviteAgentAsync(InviteAgentDto dto, CancellationToken ct = default);
    Task<ServiceResult> UpdateAgentAsync(Guid id, UpdateAgentDto dto, CancellationToken ct = default);
    Task<ServiceResult<ToggleActiveResultDto>> ToggleActiveAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<BulkImportUsersResponse>> BulkImportAsync(Stream fileStream, string fileExtension, bool sendInviteEmail, CancellationToken ct = default);
}
