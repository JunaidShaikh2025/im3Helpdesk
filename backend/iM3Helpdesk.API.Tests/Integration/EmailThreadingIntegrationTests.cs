using System.Reflection;
using System.Security.Claims;
using iM3Helpdesk.API.Controllers;
using iM3Helpdesk.Infrastructure.Services;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace iM3Helpdesk.API.Tests.Integration;

public class EmailThreadingIntegrationTests
{
  [Fact]
  public async Task Integration_FullThreadedFlow_InboundReply_MapsToExistingOpenTicket()
  {
    var tenantId = Guid.NewGuid();
    var customerId = Guid.NewGuid();
    var ticketId = Guid.NewGuid();

    await using var context = BuildContext(tenantId);

    context.Organizations.Add(BuildOrganization(tenantId, "support@example.com"));
    context.Users.Add(new User
    {
      Id = customerId,
      FullName = "Customer A",
      Email = "customer@example.com",
      PasswordHash = "x",
      Role = UserRole.Agent,
      OrganizationId = tenantId
    });
    context.Tickets.Add(new Ticket
    {
      Id = ticketId,
      Title = "Existing thread",
      Description = "desc",
      Category = "General",
      Status = TicketStatus.Open,
      OrganizationId = tenantId,
      CreatedByUserId = customerId,
      TicketNumber = 5101,
      OriginalMessageId = "orig-5101@example.com"
    });
    await context.SaveChangesAsync();

    var controller = BuildInboundController(context);

    var result = await InvokeProcessEmailAsync(
        controller,
        "Customer A <customer@example.com>",
        "support@example.com",
        "Re: Existing thread [#TN5101]",
        "reply body",
        "<p>reply body</p>",
        "<incoming-5101@example.com>",
        "<orig-5101@example.com>",
        "<orig-5101@example.com>");

    var ok = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(ok.Value);

    var comments = await context.TicketComments
        .Where(c => c.TicketId == ticketId)
        .ToListAsync();
    Assert.Single(comments);
    Assert.Equal("incoming-5101@example.com", comments[0].MessageId);
    Assert.Equal("orig-5101@example.com", comments[0].InReplyTo);
    Assert.Equal("customer@example.com", comments[0].FromEmail);
    Assert.Equal("support@example.com", comments[0].ToEmail);
    Assert.Equal(TicketCommentSenderType.Customer, comments[0].SenderType);

    var ticketCount = await context.Tickets.CountAsync();
    Assert.Equal(1, ticketCount);
  }

  [Fact]
  public async Task Integration_ReplyThreadingContinuity_ReferencesPreviousCommentMessageId()
  {
    var tenantId = Guid.NewGuid();
    var customerId = Guid.NewGuid();
    var ticketId = Guid.NewGuid();

    await using var context = BuildContext(tenantId);

    context.Organizations.Add(BuildOrganization(tenantId, "support@example.com"));
    context.Users.Add(new User
    {
      Id = customerId,
      FullName = "Customer A",
      Email = "customer@example.com",
      PasswordHash = "x",
      Role = UserRole.Agent,
      OrganizationId = tenantId
    });
    context.Tickets.Add(new Ticket
    {
      Id = ticketId,
      Title = "Continuity thread",
      Description = "desc",
      Category = "General",
      Status = TicketStatus.Open,
      OrganizationId = tenantId,
      CreatedByUserId = customerId,
      TicketNumber = 5102,
      OriginalMessageId = "orig-5102@example.com",
      LastReplyMessageId = "reply-anchor-5102@example.com"
    });
    context.TicketComments.Add(new TicketComment
    {
      TicketId = ticketId,
      UserId = customerId,
      Comment = "prior",
      IsInternal = false,
      OrganizationId = tenantId,
      MessageId = "reply-anchor-5102@example.com",
      Source = "email"
    });
    await context.SaveChangesAsync();

    var controller = BuildInboundController(context);

    var result = await InvokeProcessEmailAsync(
        controller,
        "Customer A <customer@example.com>",
        "support@example.com",
        "Follow up",
        "second reply",
        "<p>second reply</p>",
        "<incoming-5102@example.com>",
        null,
        "<reply-anchor-5102@example.com>");

    Assert.IsType<OkObjectResult>(result);

    var comments = await context.TicketComments
        .Where(c => c.TicketId == ticketId)
        .ToListAsync();
    Assert.Equal(2, comments.Count);
    Assert.Contains(comments, c => c.MessageId == "incoming-5102@example.com");

    var ticketCount = await context.Tickets.CountAsync();
    Assert.Equal(1, ticketCount);
  }

  [Fact]
  public async Task Integration_TicketDetailApiContract_IncludesThreadingMetadataAfterInboundProcessing()
  {
    var tenantId = Guid.NewGuid();
    var customerId = Guid.NewGuid();
    var agentId = Guid.NewGuid();

    var tenantService = new FakeCurrentTenantService(tenantId, isSuperAdmin: false);
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    await using var context = new ApplicationDbContext(options, tenantService);

    context.Organizations.Add(BuildOrganization(tenantId, "support@example.com"));
    context.Users.AddRange(
        new User
        {
          Id = customerId,
          FullName = "Customer A",
          Email = "customer@example.com",
          PasswordHash = "x",
          Role = UserRole.Agent,
          OrganizationId = tenantId
        },
        new User
        {
          Id = agentId,
          FullName = "Agent A",
          Email = "agent@example.com",
          PasswordHash = "x",
          Role = UserRole.Agent,
          OrganizationId = tenantId
        });
    await context.SaveChangesAsync();

    var inbound = BuildInboundController(context);

    var createResult = await InvokeProcessEmailAsync(
        inbound,
        "Customer A <customer@example.com>",
        "support@example.com",
        "Contract seed",
        "seed body",
        "<p>seed body</p>",
        "<incoming-5103@example.com>",
        null,
        null);

    var ok = Assert.IsType<OkObjectResult>(createResult);
    Assert.NotNull(ok.Value);
    var payload = ok.Value!;
    var ticketIdValue = payload.GetType().GetProperty("ticketId")?.GetValue(payload);
    Assert.NotNull(ticketIdValue);
    var ticketId = (Guid)ticketIdValue!;

    context.TicketComments.Add(new TicketComment
    {
      TicketId = ticketId,
      UserId = customerId,
      Comment = "Inbound follow up",
      IsInternal = false,
      OrganizationId = tenantId,
      MessageId = "comment-5103@example.com",
      FromEmail = "customer@example.com",
      ToEmail = "support@example.com",
      SenderType = TicketCommentSenderType.Customer,
      Source = "email"
    });
    await context.SaveChangesAsync();

    var ticketController = new TicketsController(
        context,
        tenantService,
        new Mock<INotificationService>().Object,
        new Mock<IEmailService>().Object,
        new Mock<ISlaService>().Object,
        NullLogger<TicketsController>.Instance);

    ticketController.ControllerContext = new ControllerContext
    {
      HttpContext = new DefaultHttpContext
      {
        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
          new Claim(ClaimTypes.NameIdentifier, agentId.ToString()),
          new Claim(ClaimTypes.Role, "Agent")
        }, "TestAuth"))
      }
    };

    var detailResult = await ticketController.GetById(ticketId);
    var detailOk = Assert.IsType<OkObjectResult>(detailResult);
    Assert.NotNull(detailOk.Value);

    var detail = detailOk.Value!;
    var detailType = detail.GetType();
    Assert.Equal("incoming-5103@example.com", detailType.GetProperty("OriginalMessageId")?.GetValue(detail));
    Assert.Equal("customer@example.com", detailType.GetProperty("FromEmail")?.GetValue(detail));
    Assert.Equal("support@example.com", detailType.GetProperty("ToEmail")?.GetValue(detail));

    var comments = detailType.GetProperty("Comments")?.GetValue(detail) as System.Collections.IEnumerable;
    Assert.NotNull(comments);

    object? firstComment = null;
    foreach (var c in comments!)
    {
      firstComment = c;
      break;
    }

    Assert.NotNull(firstComment);
    var commentType = firstComment!.GetType();
    Assert.NotNull(commentType.GetProperty("FromEmail")?.GetValue(firstComment));
    Assert.NotNull(commentType.GetProperty("ToEmail")?.GetValue(firstComment));
    Assert.NotNull(commentType.GetProperty("SenderType")?.GetValue(firstComment));
    Assert.NotNull(commentType.GetProperty("MessageId")?.GetValue(firstComment));
  }

  private static InboundEmailController BuildInboundController(ApplicationDbContext context)
  {
    var cfg = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["EmailThreading:ReopenWindowDays"] = "2",
          ["WebhookSecurity:InboundEmail:Secret"] = "disabled-for-integration"
        })
        .Build();

    return new InboundEmailController(
        context,
        NullLogger<InboundEmailController>.Instance,
        new MemoryCache(new MemoryCacheOptions()),
        cfg);
  }

  private static async Task<IActionResult> InvokeProcessEmailAsync(
      InboundEmailController controller,
      string? from,
      string? to,
      string? subject,
      string? textBody,
      string? htmlBody,
      string? messageId,
      string? inReplyTo,
      string? references)
  {
    var method = typeof(InboundEmailController)
        .GetMethod("ProcessEmail", BindingFlags.Instance | BindingFlags.NonPublic);
    Assert.NotNull(method);

    var task = (Task<IActionResult>)method!.Invoke(controller, new object?[]
    {
      from,
      to,
      subject,
      textBody,
      htmlBody,
      null,
      messageId,
      inReplyTo,
      references
    })!;

    return await task;
  }

  private static Organization BuildOrganization(Guid id, string supportEmail)
  {
    return new Organization
    {
      Id = id,
      Name = "Acme",
      Slug = "acme",
      SmtpHost = "smtp.example.com",
      SmtpPort = 587,
      SmtpUsername = "smtp-user",
      SmtpPassword = "enc",
      SmtpFromEmail = supportEmail,
      SupportEmail = supportEmail,
      TrialEndsAt = DateTime.UtcNow.AddDays(14),
      IsActive = true,
      CreatedAt = DateTime.UtcNow
    };
  }

  private static ApplicationDbContext BuildContext(Guid tenantId)
  {
    var tenantService = new FakeCurrentTenantService(tenantId, isSuperAdmin: false);
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    return new ApplicationDbContext(options, tenantService);
  }

  private sealed class FakeCurrentTenantService : ICurrentTenantService
  {
    public FakeCurrentTenantService(Guid? organizationId, bool isSuperAdmin)
    {
      OrganizationId = organizationId;
      IsSuperAdmin = isSuperAdmin;
    }

    public Guid? OrganizationId { get; }
    public bool IsSuperAdmin { get; }
  }
}
