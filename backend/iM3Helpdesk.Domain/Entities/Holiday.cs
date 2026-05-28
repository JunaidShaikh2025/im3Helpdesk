using iM3Helpdesk.Domain.Interfaces;

namespace iM3Helpdesk.Domain.Entities;

/// <summary>
/// A single organisation-wide holiday in a calendar year. Multi-tenant
/// (scoped by <see cref="OrganizationId"/>); the optional
/// <see cref="HolidayYearSetupId"/> back-pointer ties a holiday to the
/// "Year" record that owns its uploaded policy / PDF reference.
/// </summary>
public class Holiday
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid OrganizationId { get; set; }

  /// <summary>Calendar year (e.g. 2026). Denormalised for fast filtering.</summary>
  public int Year { get; set; }

  /// <summary>Actual holiday date (year + month + day).</summary>
  public DateOnly Date { get; set; }

  /// <summary>Display name, e.g. "Diwali".</summary>
  public string Occasion { get; set; } = string.Empty;

  /// <summary>
  /// Day-of-week label, e.g. "MONDAY" or "FRIDAY (Floating)". Free-text so
  /// admins can match the wording from their printed policy.
  /// </summary>
  public string? Day { get; set; }

  /// <summary>True for floating holidays that employees may opt out of.</summary>
  public bool IsFloating { get; set; }

  public Guid? HolidayYearSetupId { get; set; }

  public Guid? CreatedByUserId { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// One row per (organisation, year). Keeps the uploaded PDF reference and
/// the floating-holiday policy text so the Setup screen can show them
/// alongside the editable holiday table.
/// </summary>
public class HolidayYearSetup
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid OrganizationId { get; set; }
  public int Year { get; set; }

  /// <summary>Path under <c>/uploads/holidays/</c> to the original PDF.</summary>
  public string? PdfFileUrl { get; set; }
  public string? PdfFileName { get; set; }

  /// <summary>Number of floating holidays allowed in the year (0 = none).</summary>
  public int FloatingHolidayAllowance { get; set; }

  /// <summary>Free-text policy notes shown on the Setup screen.</summary>
  public string? PolicyText { get; set; }

  public Guid? CreatedByUserId { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; set; }
}
