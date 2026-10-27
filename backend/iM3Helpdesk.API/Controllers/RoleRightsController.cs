using System.Security.Claims;
using iM3Helpdesk.Infrastructure.Services;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.API.Controllers;

/// <summary>
/// Role Rights / Permissions admin screen.
///
/// Backend continues to enforce <c>[Authorize(Roles=...)]</c> as the hard
/// security gate. This controller manages an *override matrix* stored per
/// organization in <see cref="RolePermission"/> that the frontend uses to
/// hide / disable UI elements (buttons, menu items, columns).
///
/// All endpoints are admin-only.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "CompanyAdmin,SuperAdmin")]
public class RoleRightsController : ControllerBase
{
  private readonly ApplicationDbContext _db;
  private readonly ICurrentTenantService _tenant;
  private readonly IPermissionService _perms;

  public RoleRightsController(ApplicationDbContext db, ICurrentTenantService tenant, IPermissionService perms)
  {
    _db = db;
    _tenant = tenant;
    _perms = perms;
  }

  // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  // DTOs
  // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  public sealed record ModuleDef(string Key, string Label, string Category, string Icon);

  public sealed record PermissionRow(
      string Module,
      bool CanView,
      bool CanAdd,
      bool CanEdit,
      bool CanDelete,
      bool CanExport);

  public sealed record RoleMatrixDto(
      string Role,
      Dictionary<string, PermissionRow> Permissions);

  public sealed record CatalogDto(
      List<ModuleDef> Modules,
      List<string> Roles,
      Dictionary<string, Dictionary<string, PermissionRow>> Defaults);

  public sealed class SaveRequest
  {
    public string Role { get; set; } = string.Empty;
    public List<PermissionRow> Rows { get; set; } = new();
  }

  // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  // Module catalog (single source of truth)
  // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  private static readonly List<ModuleDef> Modules = new()
  {
    // Operations
    new("dashboard",            "Dashboard",             "Operations",   "ðŸ“Š"),
    new("tickets",              "Tickets",               "Operations",   "ðŸŽ«"),
    new("contacts",             "Contacts",              "Operations",   "ðŸ‘¥"),
    new("knowledge-base",       "Knowledge Base",        "Operations",   "ðŸ“š"),
    new("chat",                 "Team Chat",             "Operations",   "ðŸ’¬"),
    new("calendar",             "Calendar",              "Operations",   "ðŸ“…"),
    new("notifications",        "Notifications",         "Operations",   "ðŸ””"),
    new("todo",                 "To-Do",                 "Operations",   "âœ…"),    new("notes",                "Notes",                 "Operations",   "ðŸ“"),    new("call-logs",            "Call Logs",             "Operations",   "ðŸ“ž"),

    // Insights
    new("reports",              "Reports",               "Insights",     "ðŸ“ˆ"),
    new("analytics-heatmap",    "Ticket Heatmap",        "Insights",     "ðŸ”¥"),
    new("ai-insights",          "AI Insights",           "Insights",     "ðŸ¤–"),
    new("audit-log",            "Audit Log",             "Insights",     "ðŸ§¾"),

    // People
    new("agents",               "Agents / Team",         "People",       "ðŸ§‘â€ðŸ’¼"),
    new("agent-groups",         "Agent Groups",          "People",       "ðŸ‘¥"),
    new("customers",            "Customers",             "People",       "ðŸ™‹"),
    new("leads",                "Leads",                 "People",       "ðŸŽ¯"),

    // Configuration
    new("ticket-templates",     "Ticket Templates",      "Configuration","ðŸ“"),
    new("custom-fields",        "Custom Fields",         "Configuration","ðŸ§©"),
    new("ticket-masters",       "Ticket Configuration",  "Configuration","ðŸ·ï¸"),
    new("settings",             "Workspace Settings",    "Configuration","âš™ï¸"),
    new("organization-profile", "Organization Profile",  "Configuration","ðŸ¢"),
    new("holiday-setup",        "Holiday Setup",         "Configuration","ðŸŽ‰"),
    new("recycle-bin",          "Recycle Bin",           "Configuration","ðŸ—‘ï¸"),
    new("role-rights",          "Role Rights",           "Configuration","ðŸ›¡ï¸"),

    // Integrations
    new("integrations-email",   "Email Integration",     "Integrations", "âœ‰ï¸"),
    new("integrations-slack",   "Slack Integration",     "Integrations", "ðŸ’¼"),
    new("integrations-whatsapp","WhatsApp Integration",  "Integrations", "ðŸ“±"),
  };

  // Roles editable in the UI. SuperAdmin is intentionally excluded â€” they
  // always have full access (enforced via [Authorize] roles + the /me endpoint
  // returning an all-true matrix for SuperAdmin callers).
  private static readonly UserRole[] OrderedRoles = new[]
  {
    UserRole.CompanyAdmin,
    UserRole.Agent,
  };

  // Defaults map (sane out-of-the-box behaviour).
  private static PermissionRow DefaultFor(UserRole role, string module)
  {
    // SuperAdmin: full on everything.
    if (role == UserRole.SuperAdmin)
      return new PermissionRow(module, true, true, true, true, true);

    // CompanyAdmin: full on most things; cannot touch SuperAdmin-only stuff.
    if (role == UserRole.CompanyAdmin)
      return new PermissionRow(module, true, true, true, true, true);

    // Agent: view everywhere except admin-only modules; mutate on operations.
    bool adminOnly = module is "organization-profile" or "holiday-setup"
        or "recycle-bin" or "role-rights" or "agents" or "agent-groups"
        or "audit-log" or "leads" or "integrations-email"
        or "integrations-slack" or "integrations-whatsapp" or "ticket-masters"
        or "custom-fields" or "ticket-templates" or "settings";

    if (adminOnly)
      return new PermissionRow(module, false, false, false, false, false);

    var canEdit = module is not ("dashboard" or "reports" or "analytics-heatmap" or "ai-insights" or "audit-log");
    var canAdd = canEdit;
    var canDelete = module is "tickets" or "contacts" or "todo" or "notes" or "calendar" or "knowledge-base";
    var canExport = module is "tickets" or "reports" or "contacts" or "analytics-heatmap";
    return new PermissionRow(module, true, canAdd, canEdit, canDelete, canExport);
  }

  // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  // GET /api/RoleRights/catalog
  // Returns modules + roles + default matrix. Frontend uses this to draw
  // the empty grid before applying overrides.
  // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  [HttpGet("catalog")]
  public IActionResult GetCatalog()
  {
    var defaults = OrderedRoles.ToDictionary(
        r => r.ToString(),
        r => Modules.ToDictionary(m => m.Key, m => DefaultFor(r, m.Key)));

    return Ok(new CatalogDto(
        Modules: Modules,
        Roles: OrderedRoles.Select(r => r.ToString()).ToList(),
        Defaults: defaults));
  }

  // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  // GET /api/RoleRights
  // Returns the merged (default + saved overrides) matrix for the org.
  // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  [HttpGet]
  public async Task<IActionResult> GetMatrix()
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });

    var saved = await _db.RolePermissions
        .Where(rp => rp.OrganizationId == orgId)
        .ToListAsync();

    var result = new Dictionary<string, Dictionary<string, PermissionRow>>();
    foreach (var role in OrderedRoles)
    {
      var roleKey = role.ToString();
      var inner = new Dictionary<string, PermissionRow>();
      foreach (var m in Modules)
      {
        var s = saved.FirstOrDefault(x => x.Role == role && x.Module == m.Key);
        inner[m.Key] = s == null
            ? DefaultFor(role, m.Key)
            : new PermissionRow(m.Key, s.CanView, s.CanAdd, s.CanEdit, s.CanDelete, s.CanExport);
      }
      result[roleKey] = inner;
    }

    return Ok(result);
  }

  // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  // PUT /api/RoleRights
  // Bulk-save the matrix for a single role. Rows missing from the payload
  // are left untouched.
  // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  [HttpPut]
  public async Task<IActionResult> Save([FromBody] SaveRequest body)
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });
    if (!Enum.TryParse<UserRole>(body.Role, ignoreCase: true, out var role))
      return BadRequest(new { message = "Invalid role" });
    if (role == UserRole.SuperAdmin)
      return BadRequest(new { message = "SuperAdmin permissions cannot be modified." });

    var validKeys = Modules.Select(m => m.Key).ToHashSet();
    var byKey = body.Rows.Where(r => validKeys.Contains(r.Module))
                         .ToDictionary(r => r.Module, r => r);

    var existing = await _db.RolePermissions
        .Where(rp => rp.OrganizationId == orgId && rp.Role == role)
        .ToListAsync();

    var userId = GetUserId();
    var now = DateTime.UtcNow;

    foreach (var (key, row) in byKey)
    {
      var ex = existing.FirstOrDefault(e => e.Module == key);
      if (ex == null)
      {
        _db.RolePermissions.Add(new RolePermission
        {
          OrganizationId = orgId.Value,
          Role = role,
          Module = key,
          CanView = row.CanView,
          CanAdd = row.CanAdd,
          CanEdit = row.CanEdit,
          CanDelete = row.CanDelete,
          CanExport = row.CanExport,
          UpdatedByUserId = userId,
          UpdatedAt = now
        });
      }
      else
      {
        ex.CanView = row.CanView;
        ex.CanAdd = row.CanAdd;
        ex.CanEdit = row.CanEdit;
        ex.CanDelete = row.CanDelete;
        ex.CanExport = row.CanExport;
        ex.UpdatedByUserId = userId;
        ex.UpdatedAt = now;
      }
    }

    await _db.SaveChangesAsync();
    if (orgId.HasValue) _perms.InvalidateOrg(orgId.Value);
    return Ok(new { role = role.ToString(), saved = byKey.Count });
  }

  // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  // POST /api/RoleRights/reset?role={role}
  // Wipes overrides for a role (or all roles if omitted) so defaults apply.
  // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  [HttpPost("reset")]
  public async Task<IActionResult> Reset([FromQuery] string? role)
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });

    IQueryable<RolePermission> q = _db.RolePermissions.Where(rp => rp.OrganizationId == orgId);

    if (!string.IsNullOrWhiteSpace(role))
    {
      if (!Enum.TryParse<UserRole>(role, ignoreCase: true, out var r))
        return BadRequest(new { message = "Invalid role" });
      q = q.Where(rp => rp.Role == r);
    }

    var rows = await q.ToListAsync();
    if (rows.Count > 0) _db.RolePermissions.RemoveRange(rows);
    await _db.SaveChangesAsync();
    if (orgId.HasValue) _perms.InvalidateOrg(orgId.Value);
    return Ok(new { reset = rows.Count });
  }

  // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  // GET /api/RoleRights/me
  // Returns the effective permission map for the CURRENT user.
  // Lightweight + open to any authenticated user (overrides admin-only attr).
  // The frontend calls this on login and after the admin saves changes.
  // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  [HttpGet("me")]
  [AllowAnonymous]
  public async Task<IActionResult> GetMine()
  {
    if (User?.Identity?.IsAuthenticated != true)
      return Ok(new { isSuperAdmin = false, permissions = new Dictionary<string, PermissionRow>() });

    var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
    if (!Enum.TryParse<UserRole>(roleClaim, ignoreCase: true, out var role))
      return Ok(new { isSuperAdmin = false, permissions = new Dictionary<string, PermissionRow>() });

    // SuperAdmin gets a blanket all-true map regardless of any overrides.
    if (role == UserRole.SuperAdmin)
    {
      var allAllowed = Modules.ToDictionary(
          m => m.Key,
          m => new PermissionRow(m.Key, true, true, true, true, true));
      return Ok(new { isSuperAdmin = true, permissions = allAllowed });
    }

    var orgId = _tenant.OrganizationId;
    List<RolePermission> overrides = new();
    if (orgId != null)
    {
      overrides = await _db.RolePermissions
          .Where(rp => rp.OrganizationId == orgId && rp.Role == role)
          .ToListAsync();
    }

    var map = new Dictionary<string, PermissionRow>();
    foreach (var m in Modules)
    {
      var o = overrides.FirstOrDefault(x => x.Module == m.Key);
      map[m.Key] = o == null
          ? DefaultFor(role, m.Key)
          : new PermissionRow(m.Key, o.CanView, o.CanAdd, o.CanEdit, o.CanDelete, o.CanExport);
    }
    return Ok(new { isSuperAdmin = false, permissions = map });
  }

  private Guid? GetUserId()
  {
    var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
              ?? User.FindFirst("sub")?.Value;
    return Guid.TryParse(raw, out var id) ? id : null;
  }
}
