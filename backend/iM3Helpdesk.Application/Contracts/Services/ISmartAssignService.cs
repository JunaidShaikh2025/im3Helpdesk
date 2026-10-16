using iM3Helpdesk.Application.DTOs.SmartAssign;
using iM3Helpdesk.Domain.Entities;

namespace iM3Helpdesk.Application.Contracts.Services;

public interface ISmartAssignService
{
    Task<TicketSmartAssignResultDto?> EvaluateTicketAsync(Guid ticketId, Guid organizationId, bool allowAutoAssign = true);
    Task<bool> ApplyAssignmentAsync(Guid ticketId, Guid agentId, Guid currentUserId, Guid organizationId, string? overrideReason = null);
    Task<RetrainResultDto> RetrainModelAsync(Guid organizationId, string baseStoragePath);
    Task<SmartAssignStatsDto> GetStatsAsync(Guid organizationId);
    Task<List<TicketSmartAssignResultDto>> GetUnassignedQueueAsync(Guid organizationId, int limit = 20);
    Task<AISmartAssignConfig> GetOrCreateConfigAsync(Guid organizationId);
    Task<AISmartAssignConfig> UpdateConfigAsync(Guid organizationId, UpdateSmartAssignConfigDto dto);
    Task RefreshAgentSkillStatsAsync(Guid organizationId);
}

