using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BirthdaysController : ControllerBase
{
  private readonly ApplicationDbContext _context;
  private readonly ICurrentTenantService _tenant;

  private sealed record BirthdayCalendarItem(
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
      bool IsBirthday);

  private sealed record BirthdayReminderItem(
      Guid UserId,
      string FullName,
      string? PhotoUrl,
      string When, // "today" | "tomorrow"
      string Date  // yyyy-MM-dd (IST)
  );

  public BirthdaysController(
      ApplicationDbContext context,
      ICurrentTenantService tenant)
  {
    _context = context;
    _tenant = tenant;
  }

  private static DateOnly GetTodayIst()
  {
    try
    {
      // Windows timezone id
      var tz = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
      var istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
      return DateOnly.FromDateTime(istNow);
    }
    catch
    {
      return DateOnly.FromDateTime(DateTime.UtcNow);
    }
  }

  private static DateOnly NormalizeBirthdayDate(DateOnly dob, int year)
  {
    // Handle Feb 29 on non-leap years: show on Feb 28.
    var month = dob.Month;
    var day = dob.Day;
    var dim = DateTime.DaysInMonth(year, month);
    if (day > dim) day = dim;
    return new DateOnly(year, month, day);
  }

  private IQueryable<iM3Helpdesk.Domain.Entities.User> GetBirthdayUsersQuery(Guid orgId)
  {
    return _context.Users
        .IgnoreQueryFilters()
        .AsNoTracking()
        .Where(u => u.OrganizationId == orgId)
        .Where(u => u.DateOfBirth != null)
      // Include all org users except SuperAdmin
      .Where(u => u.Role != UserRole.SuperAdmin);
  }

  // ─────────────────────────────────────────────────────
  // GET /api/Birthdays/calendar?start=YYYY-MM-DD&end=YYYY-MM-DD
  // Returns read-only "CalendarEvent-like" items (all-day)
  // ─────────────────────────────────────────────────────
  [HttpGet("calendar")]
  public async Task<IActionResult> GetCalendar(
      [FromQuery] DateOnly start,
      [FromQuery] DateOnly end)
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });

    if (end < start)
      return BadRequest(new { message = "Invalid range" });

    // Keep it bounded to avoid accidental huge ranges.
    if ((end.ToDateTime(TimeOnly.MinValue) - start.ToDateTime(TimeOnly.MinValue)).TotalDays > 400)
      return BadRequest(new { message = "Range too large" });

    var users = await GetBirthdayUsersQuery(orgId.Value)
        .Select(u => new { u.Id, u.FullName, u.DateOfBirth })
        .ToListAsync();

    var years = Enumerable.Range(start.Year, end.Year - start.Year + 1).ToList();
    var result = new List<BirthdayCalendarItem>();

    foreach (var u in users)
    {
      if (!u.DateOfBirth.HasValue) continue;

      foreach (var y in years)
      {
        var occ = NormalizeBirthdayDate(u.DateOfBirth.Value, y);
        if (occ < start || occ > end) continue;

        var startUtc = new DateTime(occ.Year, occ.Month, occ.Day, 0, 0, 0, DateTimeKind.Utc);
        result.Add(new BirthdayCalendarItem(
            Id: $"birthday:{u.Id}:{occ:yyyy-MM-dd}",
            Title: $"🎂 {u.FullName}",
            Description: "Birthday",
            StartDate: startUtc,
            EndDate: null,
            AllDay: true,
            Type: "event",
            Priority: "low",
            TicketId: null,
            IsCompleted: false,
            ReminderMinutes: null,
            Color: "#ec4899",
            AttendeeEmails: "",
            ReminderSent: false,
            CreatedAt: DateTime.UtcNow,
            IsBirthday: true));
      }
    }

    return Ok(result.OrderBy(x => x.StartDate));
  }

  // ─────────────────────────────────────────────────────
  // GET /api/Birthdays/reminder-summary
  // Shows birthdays for TODAY and TOMORROW (IST)
  // ─────────────────────────────────────────────────────
  [HttpGet("reminder-summary")]
  public async Task<IActionResult> GetReminderSummary()
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });

    var today = GetTodayIst();
    var tomorrow = today.AddDays(1);

    var users = await GetBirthdayUsersQuery(orgId.Value)
      .Select(u => new { u.DateOfBirth })
        .ToListAsync();

    int CountFor(DateOnly day)
    {
      return users.Count(u =>
      {
        if (!u.DateOfBirth.HasValue) return false;
        var occ = NormalizeBirthdayDate(u.DateOfBirth.Value, day.Year);
        return occ == day;
      });
    }

    return Ok(new
    {
      today = today.ToString("yyyy-MM-dd"),
      tomorrow = tomorrow.ToString("yyyy-MM-dd"),
      todayCount = CountFor(today),
      tomorrowCount = CountFor(tomorrow)
    });
  }

  // ─────────────────────────────────────────────────────
  // GET /api/Birthdays/reminders
  // Detailed list for topbar dropdown (TODAY + TOMORROW IST)
  // ─────────────────────────────────────────────────────
  [HttpGet("reminders")]
  public async Task<IActionResult> GetReminders()
  {
    var orgId = _tenant.OrganizationId;
    if (orgId == null) return BadRequest(new { message = "Organization not found" });

    var today = GetTodayIst();
    var tomorrow = today.AddDays(1);

    var users = await GetBirthdayUsersQuery(orgId.Value)
        .Select(u => new { u.Id, u.FullName, u.PhotoUrl, u.DateOfBirth })
        .ToListAsync();

    var items = new List<BirthdayReminderItem>();

    foreach (var u in users)
    {
      if (!u.DateOfBirth.HasValue) continue;

      var occToday = NormalizeBirthdayDate(u.DateOfBirth.Value, today.Year);
      if (occToday == today)
      {
        items.Add(new BirthdayReminderItem(
            UserId: u.Id,
            FullName: u.FullName,
            PhotoUrl: u.PhotoUrl,
            When: "today",
            Date: today.ToString("yyyy-MM-dd")));
      }

      var occTomorrow = NormalizeBirthdayDate(u.DateOfBirth.Value, tomorrow.Year);
      if (occTomorrow == tomorrow)
      {
        items.Add(new BirthdayReminderItem(
            UserId: u.Id,
            FullName: u.FullName,
            PhotoUrl: u.PhotoUrl,
            When: "tomorrow",
            Date: tomorrow.ToString("yyyy-MM-dd")));
      }
    }

    var todayItems = items.Where(i => i.When == "today").ToList();
    var tomorrowItems = items.Where(i => i.When == "tomorrow").ToList();

    return Ok(new
    {
      today = today.ToString("yyyy-MM-dd"),
      tomorrow = tomorrow.ToString("yyyy-MM-dd"),
      todayCount = todayItems.Count,
      tomorrowCount = tomorrowItems.Count,
      items = items
          .OrderBy(i => i.When == "today" ? 0 : 1)
          .ThenBy(i => i.FullName)
          .ToList()
    });
  }
}
