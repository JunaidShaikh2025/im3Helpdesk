using System.IO;
using iM3Helpdesk.Infrastructure.Services;
using Xunit;

namespace iM3Helpdesk.API.Tests.Services;

public class EmailReplyTemplateTests
{
  [Fact]
  public void RenderTicketReplyTemplate_ReplacesAllCorePlaceholders()
  {
    var template = @"<div>{{HEADER_TITLE}}|{{META_FROM}}|{{META_TO}}|{{META_SUBJECT}}|{{META_TICKET}}|{{BODY_HTML}}|{{DIVIDER_TEXT}}|{{SIGNATURE_HTML}}|{{HISTORY_HTML}}|{{FOOTER_NOTE}}</div>";

    var html = EmailService.RenderTicketReplyTemplate(
        template,
        "iM3 Helpdesk",
        "support@example.com",
        "customer@example.com",
        "Re: Login issue [Ticket: #TN5001]",
        "#TN5001",
        "<p>Reply body</p>",
        "Agent A",
        "<p>Thanks,<br/>Agent A</p>",
        "<p>Conversation history</p>");

    Assert.DoesNotContain("{{", html, System.StringComparison.Ordinal);
    Assert.Contains("New Reply on #TN5001", html, System.StringComparison.Ordinal);
    Assert.Contains("iM3 Helpdesk", html, System.StringComparison.Ordinal);
    Assert.Contains("customer@example.com", html, System.StringComparison.Ordinal);
    Assert.Contains("Re: Login issue [Ticket: #TN5001]", html, System.StringComparison.Ordinal);
    Assert.Contains("<p>Reply body</p>", html, System.StringComparison.Ordinal);
    Assert.Contains("<p>Thanks,<br/>Agent A</p>", html, System.StringComparison.Ordinal);
    Assert.Contains("<p>Conversation history</p>", html, System.StringComparison.Ordinal);
  }

  [Fact]
  public void TicketReplyTemplateFile_ContainsRequiredSections_AndNoExternalStylesheetLinks()
  {
    var templatePath = Path.Combine(AppContext.BaseDirectory, "templates", "emails", "ticket-reply.html");
    Assert.True(File.Exists(templatePath));

    var template = File.ReadAllText(templatePath);
    Assert.Contains("SECTION: header", template, System.StringComparison.Ordinal);
    Assert.Contains("SECTION: meta", template, System.StringComparison.Ordinal);
    Assert.Contains("SECTION: body", template, System.StringComparison.Ordinal);
    Assert.Contains("SECTION: divider", template, System.StringComparison.Ordinal);
    Assert.Contains("SECTION: history", template, System.StringComparison.Ordinal);
    Assert.Contains("SECTION: footer", template, System.StringComparison.Ordinal);
    Assert.DoesNotContain("<link", template, System.StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void TicketReplyTemplateFile_RenderingVerificationPath_ResolvesVariables()
  {
    var templatePath = Path.Combine(AppContext.BaseDirectory, "templates", "emails", "ticket-reply.html");
    Assert.True(File.Exists(templatePath));

    var template = File.ReadAllText(templatePath);
    var html = EmailService.RenderTicketReplyTemplate(
        template,
        "iM3 Helpdesk",
        "support@example.com",
        "customer@example.com",
        "Re: Printer issue [Ticket: #TN9001]",
        "#TN9001",
        "<p>Rendered from file template.</p>",
        "Agent B",
        "<p>Regards,<br/>Agent B</p>",
        "<p>Reply to this email to continue.</p>");

    Assert.DoesNotContain("{{", html, System.StringComparison.Ordinal);
    Assert.Contains("New Reply on #TN9001", html, System.StringComparison.Ordinal);
    Assert.Contains("support@example.com", html, System.StringComparison.Ordinal);
    Assert.Contains("customer@example.com", html, System.StringComparison.Ordinal);
    Assert.Contains("Re: Printer issue [Ticket: #TN9001]", html, System.StringComparison.Ordinal);
    Assert.Contains("<p>Rendered from file template.</p>", html, System.StringComparison.Ordinal);
    Assert.Contains("<p>Regards,<br/>Agent B</p>", html, System.StringComparison.Ordinal);
  }
}
