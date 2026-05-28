using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace iM3Helpdesk.API.Services;

/// <summary>
/// Best-effort holiday extraction from an uploaded PDF holiday calendar.
///
/// The format of corporate holiday lists varies a lot, so we try several
/// passes (date prefix, occasion–date pairs, calendar-month tables) and
/// merge the unique results. Floating / optional holidays are auto-flagged
/// when keywords like "floating", "optional", "restricted" appear on the
/// same line.
/// </summary>
public static class HolidayPdfParser
{
  public sealed record ParsedHoliday(
      DateOnly Date,
      string Occasion,
      string? Day,
      bool IsFloating);

  private static readonly string[] DateFormats = new[]
  {
    "d MMM yyyy", "dd MMM yyyy", "d MMMM yyyy", "dd MMMM yyyy",
    "d-MMM-yyyy", "dd-MMM-yyyy", "d/MMM/yyyy", "dd/MMM/yyyy",
    "yyyy-MM-dd", "dd-MM-yyyy", "d-M-yyyy", "dd/MM/yyyy", "d/M/yyyy",
    "MMMM d, yyyy", "MMM d, yyyy",
    "d MMM", "dd MMM", "d MMMM", "dd MMMM",
    "MMM d", "MMMM d"
  };

  private static readonly Regex FloatingRegex = new(
      @"\b(float(ing)?|optional|restricted|RH)\b",
      RegexOptions.IgnoreCase | RegexOptions.Compiled);

  // e.g. "1. 26-Jan-2025 Sunday Republic Day"
  //      "26 January 2025  Sunday  Republic Day"
  //      "Republic Day   26-Jan-2025"
  //      "26-01-2025 - Republic Day"
  //      "January 26, 2026 Republic Day"
  //      "Jan 26 2026 Republic Day"
  private static readonly Regex DatePrefixRegex = new(
      @"(?<date>" +
      @"\d{1,2}[\s/\-\.](?:[A-Za-z]{3,9}|\d{1,2})[\s/\-\.]\d{2,4}" + // 26-Jan-2025 or 26-01-2025
      @"|\d{4}-\d{1,2}-\d{1,2}" +                                    // 2025-01-26
      @"|(?:January|February|March|April|May|June|July|August|September|October|November|December|Jan|Feb|Mar|Apr|Jun|Jul|Aug|Sep|Sept|Oct|Nov|Dec)\s+\d{1,2}(?:[,\s]+\d{2,4})?" + // January 26, 2026
      @"|\d{1,2}\s+(?:January|February|March|April|May|June|July|August|September|October|November|December|Jan|Feb|Mar|Apr|Jun|Jul|Aug|Sep|Sept|Oct|Nov|Dec)(?:\s+\d{2,4})?" + // 26 January 2026
      @")",
      RegexOptions.Compiled | RegexOptions.IgnoreCase);

  /// <summary>
  /// Extracts holidays for the given year from the PDF stream.
  /// </summary>
  public static List<ParsedHoliday> Extract(Stream pdfStream, int year)
  {
    var lines = ReadLines(pdfStream);
    var results = new List<ParsedHoliday>();

    foreach (var rawLine in lines)
    {
      var line = NormalizeLine(rawLine);
      if (line.Length < 4) continue;

      // Skip obvious header rows.
      var upper = line.ToUpperInvariant();
      if (upper.Contains("HOLIDAY LIST") || upper.Contains("LIST OF HOLIDAYS")
          || (upper.Contains("SR") && upper.Contains("DATE") && upper.Contains("OCCASION"))
          || (upper.Contains("DATE") && upper.Contains("DAY") && upper.Contains("OCCASION")))
      {
        continue;
      }

      foreach (var m in DatePrefixRegex.Matches(line).Cast<Match>())
      {
        if (!TryParseDate(m.Groups["date"].Value, year, out var date)) continue;
        if (date.Year != year && date.Year != year - 1 && date.Year != year + 1) continue;
        // Snap to requested year when year was missing/short.
        if (date.Year != year)
        {
          try { date = new DateOnly(year, date.Month, date.Day); }
          catch { continue; }
        }

        var occasion = ExtractOccasion(line, m);
        if (string.IsNullOrWhiteSpace(occasion)) continue;
        if (occasion.Length > 290) occasion = occasion[..290];

        var day = SafeDayName(date);
        var floating = FloatingRegex.IsMatch(line);

        results.Add(new ParsedHoliday(date, occasion.Trim(), day, floating));
      }
    }

    // De-duplicate on (Date, Occasion) — case insensitive.
    return results
        .GroupBy(h => (h.Date, Occ: h.Occasion.Trim().ToLowerInvariant()))
        .Select(g => g.First())
        .OrderBy(h => h.Date)
        .ToList();
  }

  private static List<string> ReadLines(Stream pdfStream)
  {
    var lines = new List<string>();
    using var doc = PdfDocument.Open(pdfStream);
    foreach (Page page in doc.GetPages())
    {
      // Strategy 1: use word positions to reconstruct rows.
      // Holiday PDFs are usually rendered as tables; `page.Text` returns one
      // big string per page without reliable newlines, so we group words by
      // their Y coordinate (rounded to the nearest pt) and join each group
      // left-to-right.
      try
      {
        var words = page.GetWords()?.ToList() ?? new();
        if (words.Count > 0)
        {
          var groups = words
              .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 0))
              .OrderByDescending(g => g.Key) // top of page first
              .Select(g => string.Join(" ",
                  g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)));

          foreach (var line in groups)
          {
            if (!string.IsNullOrWhiteSpace(line)) lines.Add(line);
          }
          continue;
        }
      }
      catch { /* fall through */ }

      // Strategy 2 (fallback): split the raw page text on newlines.
      var pageText = page.Text ?? string.Empty;
      foreach (var raw in pageText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
      {
        lines.Add(raw);
      }
    }
    return lines;
  }

  private static string NormalizeLine(string s)
  {
    // Collapse runs of whitespace.
    return Regex.Replace(s, @"\s+", " ").Trim();
  }

  private static bool TryParseDate(string raw, int year, out DateOnly date)
  {
    date = default;
    var s = raw.Trim().Replace(".", "-").Replace("/", "-");

    // Add the year if a "26 Jan" style match has none.
    var hasYear = Regex.IsMatch(s, @"\d{4}") || Regex.IsMatch(s, @"-\d{2}$");
    var candidate = hasYear ? s : $"{s}-{year}";

    foreach (var fmt in DateFormats)
    {
      if (DateTime.TryParseExact(candidate, fmt,
              CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
      {
        date = DateOnly.FromDateTime(dt);
        return true;
      }
    }

    // Last-ditch: invariant parser.
    if (DateTime.TryParse(candidate, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var dt2))
    {
      date = DateOnly.FromDateTime(dt2);
      return true;
    }

    return false;
  }

  private static string ExtractOccasion(string line, Match dateMatch)
  {
    // Remove the date span, then strip leading numeric serials / weekday name / separators.
    var before = line[..dateMatch.Index].Trim();
    var after = line[(dateMatch.Index + dateMatch.Length)..].Trim();

    var candidate = after.Length >= 3 ? after : before;

    // Drop a leading "1." / "1)" / "Sr.No 1" style prefix.
    candidate = Regex.Replace(candidate, @"^(sr\.?\s*no\.?\s*\d+|\d+[\.\)\-])\s*", "",
        RegexOptions.IgnoreCase);

    // Drop a leading weekday word so "Monday Republic Day" becomes "Republic Day".
    candidate = Regex.Replace(candidate,
        @"^(monday|tuesday|wednesday|thursday|friday|saturday|sunday|mon|tue|wed|thu|fri|sat|sun)\b[\s,\-:]*",
        "", RegexOptions.IgnoreCase);

    // Drop leading separators.
    candidate = candidate.TrimStart('-', ':', '|', ',', '–', '—', ' ', '\t');

    // Drop a trailing weekday tail in "Republic Day Monday".
    candidate = Regex.Replace(candidate,
        @"[\s,\-:]+(monday|tuesday|wednesday|thursday|friday|saturday|sunday)\s*$",
        "", RegexOptions.IgnoreCase);

    return candidate.Trim();
  }

  private static string? SafeDayName(DateOnly date)
  {
    try
    {
      return date.ToDateTime(TimeOnly.MinValue)
          .ToString("dddd", CultureInfo.InvariantCulture)
          .ToUpperInvariant();
    }
    catch { return null; }
  }
}
