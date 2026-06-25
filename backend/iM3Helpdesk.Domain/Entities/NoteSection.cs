using iM3Helpdesk.Domain.Interfaces;

namespace iM3Helpdesk.Domain.Entities;

public class NoteSection : IMustHaveTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid UserId { get; set; }
    public Guid NoteBookId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public NoteBook? NoteBook { get; set; }
    public ICollection<NotePage> Pages { get; set; } = new List<NotePage>();
}
