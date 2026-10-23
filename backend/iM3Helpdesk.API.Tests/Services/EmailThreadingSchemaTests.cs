using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace iM3Helpdesk.API.Tests.Services;

public class EmailThreadingSchemaTests
{
  [Fact]
  public void TicketEntity_MustExpose_OriginalMessageId_ForThreadIdempotency()
  {
    // Contract-first test: schema must support message-id based deduplication.
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    using var db = new ApplicationDbContext(options, new FakeCurrentTenantService());

    var ticketEntity = db.Model.FindEntityType("iM3Helpdesk.Domain.Entities.Ticket");
    Assert.NotNull(ticketEntity);

    var originalMessageId = ticketEntity!.FindProperty("OriginalMessageId");
    Assert.NotNull(originalMessageId);
  }

  [Fact]
  public void TicketEntity_MustExpose_LastReplyMessageId_ForOutgoingThreadState()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    using var db = new ApplicationDbContext(options, new FakeCurrentTenantService());

    var ticketEntity = db.Model.FindEntityType("iM3Helpdesk.Domain.Entities.Ticket");
    Assert.NotNull(ticketEntity);

    var lastReplyMessageId = ticketEntity!.FindProperty("LastReplyMessageId");
    Assert.NotNull(lastReplyMessageId);
  }

  [Fact]
  public void TicketCommentEntity_MustExpose_MessageId_And_InReplyTo_ForHeaderMatching()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    using var db = new ApplicationDbContext(options, new FakeCurrentTenantService());

    var commentEntity = db.Model.FindEntityType("iM3Helpdesk.Domain.Entities.TicketComment");
    Assert.NotNull(commentEntity);

    Assert.NotNull(commentEntity!.FindProperty("MessageId"));
    Assert.NotNull(commentEntity.FindProperty("InReplyTo"));
  }

  [Fact]
  public void TicketCommentEntity_MustExpose_SenderRoutingMetadata_ForThreadingIntegrity()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    using var db = new ApplicationDbContext(options, new FakeCurrentTenantService());

    var commentEntity = db.Model.FindEntityType("iM3Helpdesk.Domain.Entities.TicketComment");
    Assert.NotNull(commentEntity);

    Assert.NotNull(commentEntity!.FindProperty("References"));
    Assert.NotNull(commentEntity.FindProperty("FromEmail"));
    Assert.NotNull(commentEntity.FindProperty("ToEmail"));
    Assert.NotNull(commentEntity.FindProperty("SenderType"));
  }

  private sealed class FakeCurrentTenantService : ICurrentTenantService
  {
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public Guid? OrganizationId => TenantId;
    public bool IsSuperAdmin => false;
  }
}
