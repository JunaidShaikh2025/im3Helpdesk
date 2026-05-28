using System.Security.Claims;
using iM3Helpdesk.API.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.API.Controllers;

/// <summary>
/// Organisation-wide Holiday Setup. Year-based: every workspace can publish
/// one "year setup" (with an optional reference PDF + policy text) and any
/// number of holiday rows underneath it.
///
/// All endpoints except <c>/calendar</c> and <c>/reminders</c> are admin-only
/// — every authenticated user needs to *see* the holidays so the org calendar
/// and topbar Events badge work correctly, but only Company Admins (or the
/// Super Admin) can mutate the list.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HolidaysController : ControllerBase
{
  private readonly ApplicationDbContext _context;
  private readonly ICurrentTenantService _tenant;
  private readonly IWebHostEnvironment _env;

  public HolidaysController(
      ApplicationDbContext context,
      ICurrentTenantService tenant,
      IWebHostEnvironment env)
  {
    _context = context;
    _tenant = tenant;
    _env = env;
  }

  // ────────────────────────────────────────────────────────────
  // Helpers
  // ────────────────────────────────────────────────────────────
  private Guid? GetUserId()
  {
    var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
              ?? User.FindFirst("sub")?.Value;
    return Guid.TryParse(raw, out var id) ? id : null;
  }

  private static DateOnly TodayIst()
  {
    try
    {
      var tz = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
      var ist = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
      return DateOnly.FromDateTime(ist);
    }
    catch
    {
      return DateOnly.FromDateTime(DateTime.UtcNow);
    }
  }

  // DTOs
  public sealed record HolidayDto(
      Guid Id,
      int Year,
      string Date, // yyyy-MM-dd
      string Occasion,
      string? Day,
      bool IsFloating);

  public sealed record YearSetupDto(
      Guid? Id,
      int Year,
      int HolidayCount,
      string? PdfFileUrl,
      string? PdfFileName,
      int FloatingHolidayAllowance,
      string? PolicyText,
      DateTime? UpdatedAt);

  public sealed record YearDetailDto(
      YearSetupDto Setup,
      List<HolidayDto> Holidays);

  public sealed record CalendarItemDto(
      string Id,
      string Title,
      string Description,
      DateTime StartDate,
      DateTime? EndDate,
      bool AllDay,
      string Type,
      string Priority,
      Guid? TicketId,
      bool IsCompleted,
      int? ReminderMinutes,
      string Color,
      string AttendeeEmails,
      bool ReminderSent,
      DateTime CreatedAt,
      bool IsHoliday);

  public sealed record ReminderItem(
      Guid Id,
      string Occasion,
      string Date,
      string When, // "today" | "tomorrow"
      bool IsFloating);

  // Mutation payloads
  public sealed class HolidayUpsertRequest
  {
    public int Year { get; set; }
    public string Date { get; set; } = string.Empty; // yyyy-MM-dd
    public string Occasion { get; set; } = string.Empty;
    public string? Day { get; set; }
    public bool IsFloating { get; set; }
  }

  public sealed class YearSetupUpsertRequest
  {
    public int Year { get; set; }
    public int FloatingHolidayAllowance { get; set; }
    public string? PolicyText { get; set; }
  }

  // ────────────────────────────────────────────────────────────
  // GET /api/Holidays/years
  // Returns one card per year that has been set up (or has holidays).
  // ────────────────────────────────────────────────────────────
  [HttpGet("years")]
  public async Task<IActionResult> GetYears()
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });

    var setups = await _context.HolidayYearSetups
        .Where(s => s.OrganizationId == orgId)
        .ToListAsync();

    var counts = await _context.Holidays
        .Where(h => h.OrganizationId == orgId)
        .GroupBy(h => h.Year)
        .Select(g => new { Year = g.Key, Count = g.Count() })
        .ToListAsync();

    var years = setups.Select(s => s.Year)
        .Concat(counts.Select(c => c.Year))
        .Distinct()
        .OrderByDescending(y => y)
        .ToList();

    var result = years.Select(y =>
    {
      var s = setups.FirstOrDefault(x => x.Year == y);
      var cnt = counts.FirstOrDefault(c => c.Year == y)?.Count ?? 0;
      return new YearSetupDto(
          Id: s?.Id,
          Year: y,
          HolidayCount: cnt,
          PdfFileUrl: s?.PdfFileUrl,
          PdfFileName: s?.PdfFileName,
          FloatingHolidayAllowance: s?.FloatingHolidayAllowance ?? 0,
          PolicyText: s?.PolicyText,
          UpdatedAt: s?.UpdatedAt ?? s?.CreatedAt);
    }).ToList();

    return Ok(result);
  }

  // ────────────────────────────────────────────────────────────
  // GET /api/Holidays/years/{year}
  // ────────────────────────────────────────────────────────────
  [HttpGet("years/{year:int}")]
  public async Task<IActionResult> GetYearDetail(int year)
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });

    var setup = await _context.HolidayYearSetups
        .FirstOrDefaultAsync(s => s.OrganizationId == orgId && s.Year == year);

    var holidays = await _context.Holidays
        .Where(h => h.OrganizationId == orgId && h.Year == year)
        .OrderBy(h => h.Date)
        .ToListAsync();

    var dto = new YearDetailDto(
        Setup: new YearSetupDto(
            Id: setup?.Id,
            Year: year,
            HolidayCount: holidays.Count,
            PdfFileUrl: setup?.PdfFileUrl,
            PdfFileName: setup?.PdfFileName,
            FloatingHolidayAllowance: setup?.FloatingHolidayAllowance ?? 0,
            PolicyText: setup?.PolicyText,
            UpdatedAt: setup?.UpdatedAt ?? setup?.CreatedAt),
        Holidays: holidays.Select(h => new HolidayDto(
            h.Id, h.Year, h.Date.ToString("yyyy-MM-dd"),
            h.Occasion, h.Day, h.IsFloating)).ToList());

    return Ok(dto);
  }

  // ────────────────────────────────────────────────────────────
  // PUT /api/Holidays/years/{year}  (upsert setup row)
  // ────────────────────────────────────────────────────────────
  [HttpPut("years/{year:int}")]
  [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
  public async Task<IActionResult> UpsertYearSetup(int year, [FromBody] YearSetupUpsertRequest body)
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });
    if (year < 2000 || year > 2100) return BadRequest(new { message = "Invalid year" });

    var setup = await _context.HolidayYearSetups
        .FirstOrDefaultAsync(s => s.OrganizationId == orgId && s.Year == year);

    if (setup == null)
    {
      setup = new HolidayYearSetup
      {
        OrganizationId = orgId.Value,
        Year = year,
        CreatedByUserId = GetUserId()
      };
      _context.HolidayYearSetups.Add(setup);
    }

    setup.FloatingHolidayAllowance = Math.Max(0, body.FloatingHolidayAllowance);
    setup.PolicyText = body.PolicyText;
    setup.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();
    return Ok(new { setup.Id, setup.Year });
  }

  // ────────────────────────────────────────────────────────────
  // POST /api/Holidays/years/{year}/upload-pdf  (multipart)
  // Saves the PDF AND tries to auto-extract holiday rows from it.
  // Query: ?replace=true   → wipe existing rows for that year first
  //        ?replace=false  → only append rows that don't already exist
  //                          (matched on Date + occasion, case-insensitive)
  // ────────────────────────────────────────────────────────────
  [HttpPost("years/{year:int}/upload-pdf")]
  [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
  [RequestSizeLimit(10 * 1024 * 1024)]
  public async Task<IActionResult> UploadPdf(int year, IFormFile file, [FromQuery] bool replace = false)
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });
    if (file == null || file.Length == 0) return BadRequest(new { message = "No file" });

    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
    if (ext != ".pdf") return BadRequest(new { message = "Only PDF files are allowed" });

    var uploadDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "holidays");
    Directory.CreateDirectory(uploadDir);
    var unique = $"{Guid.NewGuid()}{ext}";
    var fullPath = Path.Combine(uploadDir, unique);
    using (var stream = new FileStream(fullPath, FileMode.Create))
      await file.CopyToAsync(stream);

    var fileUrl = $"/uploads/holidays/{unique}";

    var setup = await _context.HolidayYearSetups
        .FirstOrDefaultAsync(s => s.OrganizationId == orgId && s.Year == year);

    if (setup == null)
    {
      setup = new HolidayYearSetup
      {
        OrganizationId = orgId.Value,
        Year = year,
        CreatedByUserId = GetUserId()
      };
      _context.HolidayYearSetups.Add(setup);
    }

    setup.PdfFileUrl = fileUrl;
    setup.PdfFileName = file.FileName;
    setup.UpdatedAt = DateTime.UtcNow;

    // ── Auto-extract holidays from the PDF content. ───────────────────
    int extracted = 0, added = 0, skipped = 0;
    var warnings = new List<string>();
    try
    {
      using var pdfStream = System.IO.File.OpenRead(fullPath);
      var parsed = HolidayPdfParser.Extract(pdfStream, year);
      extracted = parsed.Count;

      if (parsed.Count > 0)
      {
        // Ensure setup has an Id so we can FK the inserted rows to it.
        if (setup.Id == Guid.Empty)
        {
          await _context.SaveChangesAsync();
        }

        if (replace)
        {
          var existing = await _context.Holidays
              .Where(h => h.OrganizationId == orgId && h.Year == year)
              .ToListAsync();
          if (existing.Count > 0) _context.Holidays.RemoveRange(existing);
        }

        var existingKeys = await _context.Holidays
            .Where(h => h.OrganizationId == orgId && h.Year == year)
            .Select(h => new { h.Date, h.Occasion })
            .ToListAsync();
        var existingSet = new HashSet<string>(
            existingKeys.Select(k => $"{k.Date:yyyy-MM-dd}|{k.Occasion.Trim().ToLowerInvariant()}"));

        var userId = GetUserId();
        foreach (var p in parsed)
        {
          var key = $"{p.Date:yyyy-MM-dd}|{p.Occasion.Trim().ToLowerInvariant()}";
          if (existingSet.Contains(key)) { skipped++; continue; }
          existingSet.Add(key);

          _context.Holidays.Add(new Holiday
          {
            OrganizationId = orgId.Value,
            Year = year,
            Date = p.Date,
            Occasion = p.Occasion,
            Day = p.Day,
            IsFloating = p.IsFloating,
            HolidayYearSetupId = setup.Id,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
          });
          added++;
        }
      }
      else
      {
        warnings.Add("Could not detect any holiday rows in the PDF. Please add them manually.");
      }
    }
    catch (Exception ex)
    {
      // Don't fail the upload itself — the PDF is still saved as reference.
      warnings.Add($"PDF parsed with errors: {ex.Message}");
    }

    await _context.SaveChangesAsync();

    return Ok(new
    {
      fileUrl,
      fileName = file.FileName,
      extracted,
      added,
      skipped,
      warnings
    });
  }

  // ────────────────────────────────────────────────────────────
  // DELETE /api/Holidays/years/{year}  (deletes setup + holidays)
  // ────────────────────────────────────────────────────────────
  [HttpDelete("years/{year:int}")]
  [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
  public async Task<IActionResult> DeleteYear(int year)
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });

    var holidays = await _context.Holidays
        .Where(h => h.OrganizationId == orgId && h.Year == year)
        .ToListAsync();
    if (holidays.Count > 0) _context.Holidays.RemoveRange(holidays);

    var setup = await _context.HolidayYearSetups
        .FirstOrDefaultAsync(s => s.OrganizationId == orgId && s.Year == year);
    if (setup != null) _context.HolidayYearSetups.Remove(setup);

    await _context.SaveChangesAsync();
    return Ok(new { deleted = holidays.Count });
  }

  // ────────────────────────────────────────────────────────────
  // POST /api/Holidays  (create a single holiday)
  // ────────────────────────────────────────────────────────────
  [HttpPost]
  [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
  public async Task<IActionResult> Create([FromBody] HolidayUpsertRequest body)
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });

    if (!DateOnly.TryParse(body.Date, out var date))
      return BadRequest(new { message = "Invalid date" });
    if (string.IsNullOrWhiteSpace(body.Occasion))
      return BadRequest(new { message = "Occasion is required" });

    var year = body.Year > 0 ? body.Year : date.Year;

    // Ensure a setup row exists so the year shows up in the years list.
    var setup = await _context.HolidayYearSetups
        .FirstOrDefaultAsync(s => s.OrganizationId == orgId && s.Year == year);
    if (setup == null)
    {
      setup = new HolidayYearSetup
      {
        OrganizationId = orgId.Value,
        Year = year,
        CreatedByUserId = GetUserId()
      };
      _context.HolidayYearSetups.Add(setup);
      await _context.SaveChangesAsync();
    }

    var holiday = new Holiday
    {
      OrganizationId = orgId.Value,
      Year = year,
      Date = date,
      Occasion = body.Occasion.Trim(),
      Day = body.Day?.Trim(),
      IsFloating = body.IsFloating,
      HolidayYearSetupId = setup.Id,
      CreatedByUserId = GetUserId()
    };
    _context.Holidays.Add(holiday);
    await _context.SaveChangesAsync();

    return Ok(new HolidayDto(
        holiday.Id, holiday.Year, holiday.Date.ToString("yyyy-MM-dd"),
        holiday.Occasion, holiday.Day, holiday.IsFloating));
  }

  // ────────────────────────────────────────────────────────────
  // PUT /api/Holidays/{id}
  // ────────────────────────────────────────────────────────────
  [HttpPut("{id:guid}")]
  [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
  public async Task<IActionResult> Update(Guid id, [FromBody] HolidayUpsertRequest body)
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });

    var h = await _context.Holidays.FirstOrDefaultAsync(x => x.Id == id);
    if (h == null) return NotFound();

    if (!DateOnly.TryParse(body.Date, out var date))
      return BadRequest(new { message = "Invalid date" });

    h.Date = date;
    h.Year = body.Year > 0 ? body.Year : date.Year;
    h.Occasion = body.Occasion?.Trim() ?? h.Occasion;
    h.Day = body.Day?.Trim();
    h.IsFloating = body.IsFloating;
    h.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();
    return Ok(new HolidayDto(
        h.Id, h.Year, h.Date.ToString("yyyy-MM-dd"),
        h.Occasion, h.Day, h.IsFloating));
  }

  // ────────────────────────────────────────────────────────────
  // DELETE /api/Holidays/{id}
  // ────────────────────────────────────────────────────────────
  [HttpDelete("{id:guid}")]
  [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
  public async Task<IActionResult> Delete(Guid id)
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });

    var h = await _context.Holidays.FirstOrDefaultAsync(x => x.Id == id);
    if (h == null) return NotFound();

    _context.Holidays.Remove(h);
    await _context.SaveChangesAsync();
    return NoContent();
  }

  // ────────────────────────────────────────────────────────────
  // GET /api/Holidays/calendar?start=YYYY-MM-DD&end=YYYY-MM-DD
  // ────────────────────────────────────────────────────────────
  [HttpGet("calendar")]
  public async Task<IActionResult> GetCalendar(
      [FromQuery] DateOnly start, [FromQuery] DateOnly end)
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });
    if (end < start) return BadRequest(new { message = "Invalid range" });
    if ((end.ToDateTime(TimeOnly.MinValue) - start.ToDateTime(TimeOnly.MinValue)).TotalDays > 800)
      return BadRequest(new { message = "Range too large" });

    var rows = await _context.Holidays
        .Where(h => h.OrganizationId == orgId
                 && h.Date >= start && h.Date <= end)
        .OrderBy(h => h.Date)
        .ToListAsync();

    var items = rows.Select(h =>
    {
      var startUtc = new DateTime(
          h.Date.Year, h.Date.Month, h.Date.Day, 0, 0, 0, DateTimeKind.Utc);
      return new CalendarItemDto(
          Id: $"holiday:{h.Id}",
          Title: $"🎉 {h.Occasion}",
          Description: h.IsFloating ? "Holiday (Floating)" : "Holiday",
          StartDate: startUtc,
          EndDate: null,
          AllDay: true,
          Type: "event",
          Priority: "low",
          TicketId: null,
          IsCompleted: false,
          ReminderMinutes: null,
          Color: h.IsFloating ? "#f59e0b" : "#10b981",
          AttendeeEmails: "",
          ReminderSent: false,
          CreatedAt: h.CreatedAt,
          IsHoliday: true);
    });

    return Ok(items);
  }

  // ────────────────────────────────────────────────────────────
  // GET /api/Holidays/reminders   (today + tomorrow IST)
  // ────────────────────────────────────────────────────────────
  [HttpGet("reminders")]
  public async Task<IActionResult> GetReminders()
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });

    var today = TodayIst();
    var tomorrow = today.AddDays(1);

    var rows = await _context.Holidays
        .Where(h => h.OrganizationId == orgId
                 && (h.Date == today || h.Date == tomorrow))
        .OrderBy(h => h.Date)
        .ToListAsync();

    var items = rows.Select(h => new ReminderItem(
        Id: h.Id,
        Occasion: h.Occasion,
        Date: h.Date.ToString("yyyy-MM-dd"),
        When: h.Date == today ? "today" : "tomorrow",
        IsFloating: h.IsFloating));

    return Ok(new
    {
      today = today.ToString("yyyy-MM-dd"),
      tomorrow = tomorrow.ToString("yyyy-MM-dd"),
      todayCount = rows.Count(r => r.Date == today),
      tomorrowCount = rows.Count(r => r.Date == tomorrow),
      items
    });
  }
}
