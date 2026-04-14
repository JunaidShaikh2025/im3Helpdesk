using iM3Helpdesk.API.DTOs.Auth;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
  private readonly ApplicationDbContext _context;
  private readonly ICurrentTenantService _tenantService;
  private readonly IWebHostEnvironment _env; // Environment variable added

  public ProfileController(
      ApplicationDbContext context,
      ICurrentTenantService tenantService,
      IWebHostEnvironment env) // Injected IWebHostEnvironment
  {
    _context = context;
    _tenantService = tenantService;
    _env = env; // Assigned injected value
  }

  [HttpGet]
  public async Task<IActionResult> GetProfile()
  {
    var userId = GetUserId();
    if (userId == null) return Unauthorized();

    var user = await _context.Users
        .IgnoreQueryFilters()
        .Include(u => u.Organization)
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null) return NotFound();

    return Ok(new
    {
      user.Id,
      user.FullName,
      user.Email,
      user.PhoneNumber,
      user.PhotoUrl, // Included photo URL in response
      Role = user.Role.ToString(),
      user.IsEmailVerified,
      user.CreatedAt,
      user.LastLoginAt,
      organization = user.Organization == null ? null : new
      {
        user.Organization.Id,
        user.Organization.Name,
        user.Organization.Slug,
        user.Organization.LogoUrl,
        user.Organization.BrandColor,
        user.Organization.SupportEmail,
        user.Organization.TrialEndsAt,
        user.Organization.IsActive
      }
    });
  }

  [HttpPost("upload-photo")]
  [RequestSizeLimit(5 * 1024 * 1024)]
  public async Task<IActionResult> UploadPhoto(IFormFile file)
  {
    if (file == null) return BadRequest();

    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
    if (!Guid.TryParse(userIdClaim, out var userId))
      return Unauthorized();

    var user = await _context.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Id == userId);
    if (user == null) return NotFound();

    // Using injected _env to find wwwroot path
    var uploadPath = Path.Combine(
        _env.WebRootPath ?? "wwwroot", "avatars");
    Directory.CreateDirectory(uploadPath);

    var ext = Path.GetExtension(file.FileName);
    var fileName = $"avatar-{userId}{ext}";
    var filePath = Path.Combine(uploadPath, fileName);

    using var stream = new FileStream(filePath, FileMode.Create);
    await file.CopyToAsync(stream);

    user.PhotoUrl = $"/avatars/{fileName}";
    await _context.SaveChangesAsync();

    return Ok(new { photoUrl = user.PhotoUrl });
  }

  [HttpPut]
  public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
  {
    var userId = GetUserId();
    if (userId == null) return Unauthorized();

    var user = await _context.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null) return NotFound();

    user.FullName = dto.FullName;
    user.PhoneNumber = dto.PhoneNumber;
    await _context.SaveChangesAsync();

    return Ok(new { message = "Profile updated successfully" });
  }

  [HttpPut("change-password")]
  public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
  {
    var userId = GetUserId();
    if (userId == null) return Unauthorized();

    var user = await _context.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null) return NotFound();

    if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
      return BadRequest(new { message = "Current password is incorrect" });

    if (dto.NewPassword != dto.ConfirmNewPassword)
      return BadRequest(new { message = "New passwords do not match" });

    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Password changed successfully" });
  }

  [HttpPut("organization")]
  public async Task<IActionResult> UpdateOrganization([FromBody] UpdateOrganizationDto dto)
  {
    var orgId = _tenantService.OrganizationId;
    if (orgId == null) return Unauthorized();

    var org = await _context.Organizations
        .FirstOrDefaultAsync(o => o.Id == orgId);

    if (org == null) return NotFound();

    org.Name = dto.Name;
    org.SupportEmail = dto.SupportEmail;
    org.LogoUrl = dto.LogoUrl;
    org.BrandColor = dto.BrandColor;
    await _context.SaveChangesAsync();

    return Ok(new { message = "Organization updated successfully" });
  }

  private Guid? GetUserId()
  {
    var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
    return Guid.TryParse(claim, out var id) ? id : null;
  }
}
