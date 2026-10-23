namespace iM3Helpdesk.Application.DTOs.KnowledgeBase;

public class CreateArticleDto
{
  public string Title { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
  public string Category { get; set; } = string.Empty;
  public string Tags { get; set; } = string.Empty;
  public bool IsPublished { get; set; } = true;
  public string? MediaUrl { get; set; }
  public string? MediaType { get; set; }
}

public class ReactDto
{
  public string ReactionType { get; set; } = "like";
}

public class KbAddCommentDto
{
  public string Text { get; set; } = string.Empty;
}
