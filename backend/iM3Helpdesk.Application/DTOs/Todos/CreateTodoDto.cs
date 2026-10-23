namespace iM3Helpdesk.Application.DTOs.Todos;

public class CreateTodoDto
{
  public string Title { get; set; } = "";
  public string? TicketNumber { get; set; }
  public Guid? TicketId { get; set; }
}
