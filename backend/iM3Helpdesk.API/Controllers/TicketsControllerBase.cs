using iM3Helpdesk.Application.Contracts.Persistence;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace iM3Helpdesk.API.Controllers;

/// <summary>
/// Shared base for the split Tickets controllers.
/// Injects persistence abstraction (IUnitOfWork) adhering to the
/// Dependency Inversion Principle (DIP), with zero direct DbContext references.
/// </summary>
public abstract class TicketsControllerBase : ControllerBase
{
    protected readonly IUnitOfWork _uow;

    protected TicketsControllerBase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    protected Guid GetUserId()
    {
        var claim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        Guid.TryParse(claim, out var id);
        return id;
    }

    protected async Task<bool> IsMasterValueAllowedAsync(
        string field,
        string value)
    {
        return await _uow.Tickets.IsMasterValueAllowedAsync(field, value);
    }
}
