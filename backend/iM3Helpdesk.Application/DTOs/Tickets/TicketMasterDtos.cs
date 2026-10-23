namespace iM3Helpdesk.Application.DTOs.Tickets;

public class TicketMasterOptionDto
{
  public Guid Id { get; set; }
  public string Field { get; set; } = string.Empty;
  public string Value { get; set; } = string.Empty;
  public string Label { get; set; } = string.Empty;
  public int SortOrder { get; set; }
  public bool IsActive { get; set; }
}

public class CreateTicketMasterOptionDto
{
  public string Field { get; set; } = string.Empty;
  public string Value { get; set; } = string.Empty;
  public string? Label { get; set; }
  public int SortOrder { get; set; } = 0;
}

public class UpdateTicketMasterOptionDto
{
  public string? Value { get; set; }
  public string? Label { get; set; }
  public int? SortOrder { get; set; }
  public bool? IsActive { get; set; }
}
