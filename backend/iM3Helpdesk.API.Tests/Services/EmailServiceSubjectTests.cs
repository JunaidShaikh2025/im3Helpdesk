using iM3Helpdesk.Infrastructure.Services;
using Xunit;

namespace iM3Helpdesk.API.Tests.Services;

public class EmailServiceSubjectTests
{
  [Fact]
  public void BuildReplySubject_RemovesDuplicateReAndNormalizedToken_BeforeAppendingOnce()
  {
    var result = EmailService.BuildReplySubject(
        "Re: RE: Login issue [Ticket: #TN1001]",
        "#TN1001");

    Assert.Equal("Re: Login issue [Ticket: #TN1001]", result);
  }

  [Fact]
  public void BuildReplySubject_ReplacesLegacyToken_WithNormalizedTicketToken()
  {
    var result = EmailService.BuildReplySubject(
        "Re: Cannot login [TN2002]",
        "TN2002");

    Assert.Equal("Re: Cannot login [Ticket: #TN2002]", result);
  }

  [Fact]
  public void BuildReplySubject_EmptySubject_UsesTicketUpdateFallback()
  {
    var result = EmailService.BuildReplySubject(
        "   ",
        "#TN3003");

    Assert.Equal("Re: Ticket Update [Ticket: #TN3003]", result);
  }

  [Fact]
  public void BuildReplySubject_TicketNumberWithoutHash_IsNormalizedWithHashPrefix()
  {
    var result = EmailService.BuildReplySubject(
        "Re: Update needed",
        "TN3004");

    Assert.Equal("Re: Update needed [Ticket: #TN3004]", result);
  }

  [Fact]
  public void BuildReplySubject_WithSingleRePrefix_KeepsSingleReAfterNormalization()
  {
    var result = EmailService.BuildReplySubject(
        "Re: Access issue",
        "#TN3005");

    Assert.Equal("Re: Access issue [Ticket: #TN3005]", result);
  }
}
