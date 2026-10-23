using iM3Helpdesk.Infrastructure.Services;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace iM3Helpdesk.API.Tests.Services;

public class EmailThreadingServiceTests
{
  [Fact]
  public async Task ResolveThreadAsync_HeaderMatch_OnOpenTicket_ReturnsAddComment()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);
    var ticket = await SeedTicketAsync(db, tenantId, TicketStatus.Open, 1201);
    ticket.OriginalMessageId = "orig-1201@example";
    await db.SaveChangesAsync();

    var sut = CreateService(db, reopenDays: 2);

    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      Subject = "Re: Help",
      InReplyTo = "<orig-1201@example>",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.AddComment, result.Action);
    Assert.Equal(ticket.Id, result.TicketId);
  }

  [Fact]
  public async Task ResolveThreadAsync_UsesHeaderBeforeSubjectToken()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);

    var tokenTicket = await SeedTicketAsync(db, tenantId, TicketStatus.Open, 2001);
    var headerTicket = await SeedTicketAsync(db, tenantId, TicketStatus.Open, 2002);
    headerTicket.OriginalMessageId = "header-2002@example";
    await db.SaveChangesAsync();

    var sut = CreateService(db, reopenDays: 2);

    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      Subject = "Re: Issue [#TN2001]",
      InReplyTo = "<header-2002@example>",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.AddComment, result.Action);
    Assert.Equal(headerTicket.Id, result.TicketId);
  }

  [Fact]
  public async Task ResolveThreadAsync_SubjectTokenFallback_OnOpenTicket_ReturnsAddComment()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);
    var ticket = await SeedTicketAsync(db, tenantId, TicketStatus.Open, 3401);

    var sut = CreateService(db, reopenDays: 2);

    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      Subject = "Need update [#TN3401]",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.AddComment, result.Action);
    Assert.Equal(ticket.Id, result.TicketId);
  }

  [Fact]
  public async Task ResolveThreadAsync_ClosedWithinWindow_ReturnsReopenTicket()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);
    var ticket = await SeedTicketAsync(db, tenantId, TicketStatus.Closed, 5101);
    ticket.OriginalMessageId = "closed-5101@example";
    ticket.UpdatedAt = DateTime.UtcNow.AddHours(-12);
    await db.SaveChangesAsync();

    var sut = CreateService(db, reopenDays: 2);

    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      InReplyTo = "<closed-5101@example>",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.ReopenTicket, result.Action);
    Assert.Equal(ticket.Id, result.TicketId);
  }

  [Fact]
  public async Task ResolveThreadAsync_ClosedOutsideWindow_ReturnsNewTicketWithReference()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);
    var ticket = await SeedTicketAsync(db, tenantId, TicketStatus.Closed, 6101);
    ticket.OriginalMessageId = "closed-6101@example";
    ticket.UpdatedAt = DateTime.UtcNow.AddDays(-5);
    await db.SaveChangesAsync();

    var sut = CreateService(db, reopenDays: 2);

    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      InReplyTo = "<closed-6101@example>",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.NewTicketWithReference, result.Action);
    Assert.Equal(ticket.Id, result.TicketId);
    Assert.Equal(ticket.TicketNumber, result.ReferenceTicketNumber);
  }

  [Fact]
  public async Task ResolveThreadAsync_NoHeadersNoToken_ReturnsNewTicket()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);
    _ = await SeedTicketAsync(db, tenantId, TicketStatus.Open, 7101);

    var sut = CreateService(db, reopenDays: 2);

    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      Subject = "General question",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.NewTicket, result.Action);
    Assert.Null(result.TicketId);
  }

  [Fact]
  public async Task ResolveThreadAsync_MalformedToken_ReturnsNewTicket()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);
    _ = await SeedTicketAsync(db, tenantId, TicketStatus.Open, 7201);

    var sut = CreateService(db, reopenDays: 2);

    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      Subject = "Follow up [#TNABC]",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.NewTicket, result.Action);
  }

  [Fact]
  public async Task ResolveThreadAsync_ReferencesHeader_WithMultipleIds_MatchesTicket()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);
    var ticket = await SeedTicketAsync(db, tenantId, TicketStatus.Open, 7301);
    ticket.OriginalMessageId = "match-7301@example";
    await db.SaveChangesAsync();

    var sut = CreateService(db, reopenDays: 2);

    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      References = "<a@example> <match-7301@example> <c@example>",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.AddComment, result.Action);
    Assert.Equal(ticket.Id, result.TicketId);
  }

  [Fact]
  public async Task ResolveThreadAsync_InReplyTo_MatchesCommentMessageId()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);
    var ticket = await SeedTicketAsync(db, tenantId, TicketStatus.Open, 7401);
    await SeedCommentMessageIdAsync(db, tenantId, ticket.Id, "comment-7401@example");

    var sut = CreateService(db, reopenDays: 2);
    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      InReplyTo = "<comment-7401@example>",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.AddComment, result.Action);
    Assert.Equal(ticket.Id, result.TicketId);
  }

  [Fact]
  public async Task ResolveThreadAsync_InReplyTo_MatchesLastReplyMessageId()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);
    var ticket = await SeedTicketAsync(db, tenantId, TicketStatus.Open, 7501);
    ticket.LastReplyMessageId = "last-7501@example";
    await db.SaveChangesAsync();

    var sut = CreateService(db, reopenDays: 2);
    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      InReplyTo = "<last-7501@example>",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.AddComment, result.Action);
    Assert.Equal(ticket.Id, result.TicketId);
  }

  [Fact]
  public async Task ResolveThreadAsync_ClosedExactlyAtCutoff_Reopens()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);
    var ticket = await SeedTicketAsync(db, tenantId, TicketStatus.Closed, 7601);
    ticket.OriginalMessageId = "closed-7601@example";
    ticket.UpdatedAt = DateTime.UtcNow.AddDays(-2);
    await db.SaveChangesAsync();

    var sut = CreateService(db, reopenDays: 2);
    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      InReplyTo = "<closed-7601@example>",
      ReceivedAtUtc = ticket.UpdatedAt.Value.AddDays(2)
    });

    Assert.Equal(ThreadAction.ReopenTicket, result.Action);
  }

  [Fact]
  public async Task ResolveThreadAsync_ClosedAfterCutoff_CreatesNewWithReference()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);
    var ticket = await SeedTicketAsync(db, tenantId, TicketStatus.Closed, 7701);
    ticket.OriginalMessageId = "closed-7701@example";
    ticket.UpdatedAt = DateTime.UtcNow.AddDays(-3);
    await db.SaveChangesAsync();

    var sut = CreateService(db, reopenDays: 2);
    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      InReplyTo = "<closed-7701@example>",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.NewTicketWithReference, result.Action);
    Assert.Equal(7701, result.ReferenceTicketNumber);
  }

  [Fact]
  public async Task ResolveThreadAsync_TokenCaseInsensitive_MatchesTicket()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);
    var ticket = await SeedTicketAsync(db, tenantId, TicketStatus.Open, 7801);

    var sut = CreateService(db, reopenDays: 2);
    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      Subject = "Reply [#tn7801]",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.AddComment, result.Action);
    Assert.Equal(ticket.Id, result.TicketId);
  }

  [Fact]
  public async Task ResolveThreadAsync_HeaderNormalization_WorksWithAngleBracketsAndCase()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);
    var ticket = await SeedTicketAsync(db, tenantId, TicketStatus.Open, 7901);
    ticket.OriginalMessageId = "normalized-7901@example";
    await db.SaveChangesAsync();

    var sut = CreateService(db, reopenDays: 2);
    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      InReplyTo = "<NORMALIZED-7901@EXAMPLE>",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.AddComment, result.Action);
    Assert.Equal(ticket.Id, result.TicketId);
  }

  [Fact]
  public async Task ResolveThreadAsync_HeaderMatchFromOtherOrg_DoesNotCrossTenant()
  {
    var tenantA = Guid.NewGuid();
    var tenantB = Guid.NewGuid();
    await using var db = CreateDb(tenantA);
    var ticket = await SeedTicketAsync(db, tenantB, TicketStatus.Open, 8001);
    ticket.OriginalMessageId = "foreign-8001@example";
    await db.SaveChangesAsync();

    var sut = CreateService(db, reopenDays: 2);
    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantA,
      InReplyTo = "<foreign-8001@example>",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.NewTicket, result.Action);
    Assert.Null(result.TicketId);
  }

  [Fact]
  public async Task ResolveThreadAsync_MultipleTokens_UsesFirstMatch()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);
    var first = await SeedTicketAsync(db, tenantId, TicketStatus.Open, 8101);
    _ = await SeedTicketAsync(db, tenantId, TicketStatus.Open, 8102);

    var sut = CreateService(db, reopenDays: 2);
    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      Subject = "Re: [#TN8101] and [#TN8102]",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.AddComment, result.Action);
    Assert.Equal(first.Id, result.TicketId);
  }

  [Fact]
  public async Task ResolveThreadAsync_EmptyReferences_DoesNotCrash_AndFallsBack()
  {
    var tenantId = Guid.NewGuid();
    await using var db = CreateDb(tenantId);

    var sut = CreateService(db, reopenDays: 2);
    var result = await sut.ResolveThreadAsync(new EmailThreadingInput
    {
      OrganizationId = tenantId,
      Subject = "no token",
      References = "   ",
      ReceivedAtUtc = DateTime.UtcNow
    });

    Assert.Equal(ThreadAction.NewTicket, result.Action);
  }

  private static EmailThreadingService CreateService(ApplicationDbContext db, int reopenDays)
  {
    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["EmailThreading:ReopenWindowDays"] = reopenDays.ToString()
        })
        .Build();

    return new EmailThreadingService(db, config);
  }

  private static ApplicationDbContext CreateDb(Guid tenantId)
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    return new ApplicationDbContext(options, new FakeCurrentTenantService(tenantId));
  }

  private static async Task<Ticket> SeedTicketAsync(
      ApplicationDbContext db,
      Guid tenantId,
      TicketStatus status,
      int ticketNumber)
  {
    var user = new User
    {
      Id = Guid.NewGuid(),
      FullName = "User",
      Email = $"u{ticketNumber}@example.com",
      PasswordHash = "x",
      Role = UserRole.CompanyAdmin,
      OrganizationId = tenantId
    };
    db.Users.Add(user);

    var ticket = new Ticket
    {
      Id = Guid.NewGuid(),
      Title = $"Ticket {ticketNumber}",
      Description = "desc",
      Category = "General",
      Status = status,
      Priority = TicketPriority.Medium,
      OrganizationId = tenantId,
      CreatedByUserId = user.Id,
      TicketNumber = ticketNumber,
      CreatedAt = DateTime.UtcNow.AddDays(-7)
    };

    db.Tickets.Add(ticket);
    await db.SaveChangesAsync();
    return ticket;
  }

  private sealed class FakeCurrentTenantService : ICurrentTenantService
  {
    public FakeCurrentTenantService(Guid tenantId)
    {
      OrganizationId = tenantId;
    }

    public Guid? OrganizationId { get; }
    public bool IsSuperAdmin => false;
  }

  private static async Task SeedCommentMessageIdAsync(
      ApplicationDbContext db,
      Guid tenantId,
      Guid ticketId,
      string messageId)
  {
    var user = await db.Users.FirstAsync(u => u.OrganizationId == tenantId);
    db.TicketComments.Add(new TicketComment
    {
      TicketId = ticketId,
      UserId = user.Id,
      Comment = "body",
      OrganizationId = tenantId,
      MessageId = messageId
    });
    await db.SaveChangesAsync();
  }
}
