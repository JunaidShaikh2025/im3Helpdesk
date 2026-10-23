using System;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Domain.Exceptions;
using Xunit;

namespace iM3Helpdesk.API.Tests.Architecture;

public class DomainEntityInvariantTests
{
    // ==========================================
    // Ticket Entity Invariant Tests
    // ==========================================

    [Fact]
    public void Ticket_AssignTo_ValidAgent_SetsAssignedToAndUpdatesTimestamp()
    {
        var ticket = new Ticket { Title = "Test Ticket" };
        var agentId = Guid.NewGuid();

        ticket.AssignTo(agentId);

        Assert.Equal(agentId, ticket.AssignedToUserId);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void Ticket_AssignTo_EmptyGuid_ThrowsArgumentException()
    {
        var ticket = new Ticket { Title = "Test Ticket" };

        Assert.Throws<ArgumentException>(() => ticket.AssignTo(Guid.Empty));
    }

    [Fact]
    public void Ticket_Unassign_ClearsAssignedUser()
    {
        var ticket = new Ticket { Title = "Test Ticket", AssignedToUserId = Guid.NewGuid() };

        ticket.Unassign();

        Assert.Null(ticket.AssignedToUserId);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void Ticket_Resolve_SetsStatusAndResolvedAt()
    {
        var ticket = new Ticket { Title = "Bug", Status = TicketStatus.Open };
        var resolutionTime = DateTime.UtcNow;

        ticket.Resolve(resolutionTime);

        Assert.Equal(TicketStatus.Resolved, ticket.Status);
        Assert.Equal(resolutionTime, ticket.ResolvedAt);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void Ticket_Resolve_DeletedTicket_ThrowsDomainException()
    {
        var ticket = new Ticket { Title = "Bug", IsDeleted = true };

        Assert.Throws<DomainException>(() => ticket.Resolve());
    }

    [Fact]
    public void Ticket_Close_SetsStatusToClosed()
    {
        var ticket = new Ticket { Title = "Bug", Status = TicketStatus.Resolved };

        ticket.Close();

        Assert.Equal(TicketStatus.Closed, ticket.Status);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void Ticket_Close_DeletedTicket_ThrowsDomainException()
    {
        var ticket = new Ticket { Title = "Bug", IsDeleted = true };

        Assert.Throws<DomainException>(() => ticket.Close());
    }

    [Fact]
    public void Ticket_Reopen_FromClosedOrResolved_SetsStatusToOpenAndClearsResolvedAt()
    {
        var ticket = new Ticket
        {
            Title = "Bug",
            Status = TicketStatus.Resolved,
            ResolvedAt = DateTime.UtcNow
        };

        ticket.Reopen();

        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Null(ticket.ResolvedAt);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void Ticket_Reopen_AlreadyOpen_ThrowsDomainException()
    {
        var ticket = new Ticket { Title = "Bug", Status = TicketStatus.Open };

        Assert.Throws<DomainException>(() => ticket.Reopen());
    }

    [Fact]
    public void Ticket_LogTime_PositiveMinutes_AccumulatesTimeAndUpdatesActivity()
    {
        var ticket = new Ticket { Title = "Bug", TimeSpentMinutes = 15 };

        ticket.LogTime(30);

        Assert.Equal(45, ticket.TimeSpentMinutes);
        Assert.NotNull(ticket.LastActivityAt);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Ticket_LogTime_NonPositiveMinutes_ThrowsDomainException(int minutes)
    {
        var ticket = new Ticket { Title = "Bug" };

        Assert.Throws<DomainException>(() => ticket.LogTime(minutes));
    }

    [Fact]
    public void Ticket_ChangePriority_UpdatesPriorityAndTimestamp()
    {
        var ticket = new Ticket { Title = "Bug", Priority = TicketPriority.Low };

        ticket.ChangePriority(TicketPriority.Critical);

        Assert.Equal(TicketPriority.Critical, ticket.Priority);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void Ticket_SoftDelete_SetsDeletionFlagsAndActor()
    {
        var ticket = new Ticket { Title = "Bug" };
        var actorId = Guid.NewGuid();

        ticket.SoftDelete(actorId);

        Assert.True(ticket.IsDeleted);
        Assert.NotNull(ticket.DeletedAt);
        Assert.Equal(actorId, ticket.DeletedByUserId);
        Assert.NotNull(ticket.UpdatedAt);
    }

    [Fact]
    public void Ticket_Restore_ClearsDeletionFlags()
    {
        var ticket = new Ticket
        {
            Title = "Bug",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            DeletedByUserId = Guid.NewGuid()
        };

        ticket.Restore();

        Assert.False(ticket.IsDeleted);
        Assert.Null(ticket.DeletedAt);
        Assert.Null(ticket.DeletedByUserId);
        Assert.NotNull(ticket.UpdatedAt);
    }

    // ==========================================
    // User Entity Invariant Tests
    // ==========================================

    [Fact]
    public void User_RecordFailedLogin_LocksOutAfterThreshold()
    {
        var user = new User { Email = "test@example.com" };

        for (int i = 0; i < 4; i++)
        {
            user.RecordFailedLogin(maxFailedAttempts: 5);
            Assert.False(user.IsLockedOut());
            Assert.Equal(i + 1, user.FailedLoginAttempts);
        }

        user.RecordFailedLogin(maxFailedAttempts: 5);

        Assert.True(user.IsLockedOut());
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.NotNull(user.LockedUntil);
    }

    [Fact]
    public void User_RecordSuccessfulLogin_ResetsFailedAttemptsAndLockout()
    {
        var user = new User
        {
            Email = "test@example.com",
            FailedLoginAttempts = 3,
            LockedUntil = DateTime.UtcNow.AddMinutes(10)
        };

        user.RecordSuccessfulLogin();

        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockedUntil);
        Assert.NotNull(user.LastLoginAt);
        Assert.False(user.IsLockedOut());
    }

    [Fact]
    public void User_Unlock_ResetsLockout()
    {
        var user = new User
        {
            Email = "test@example.com",
            FailedLoginAttempts = 5,
            LockedUntil = DateTime.UtcNow.AddMinutes(15)
        };

        user.Unlock();

        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockedUntil);
        Assert.False(user.IsLockedOut());
    }

    [Fact]
    public void User_SetRefreshToken_SetsTokenAndExpiry()
    {
        var user = new User { Email = "test@example.com" };
        var expiry = DateTime.UtcNow.AddDays(7);

        user.SetRefreshToken("test_token_123", expiry);

        Assert.Equal("test_token_123", user.RefreshToken);
        Assert.Equal(expiry, user.RefreshTokenExpiresAt);
    }

    [Fact]
    public void User_RevokeRefreshToken_ClearsTokenAndExpiry()
    {
        var user = new User
        {
            Email = "test@example.com",
            RefreshToken = "existing_token",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        user.RevokeRefreshToken();

        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiresAt);
    }

    [Fact]
    public void User_VerifyEmail_MarksVerifiedAndClearsToken()
    {
        var user = new User
        {
            Email = "test@example.com",
            IsEmailVerified = false,
            EmailVerificationToken = "token_xyz"
        };

        user.VerifyEmail();

        Assert.True(user.IsEmailVerified);
        Assert.Null(user.EmailVerificationToken);
    }

    // ==========================================
    // Lead Entity Invariant Tests
    // ==========================================

    [Fact]
    public void Lead_Approve_PendingLead_TransitionsToApprovedAndSetsToken()
    {
        var lead = new Lead { OrganizationName = "Acme Corp", Status = LeadStatus.Pending };
        var adminId = Guid.NewGuid();
        var token = Guid.NewGuid();

        lead.Approve(adminId, token, TimeSpan.FromDays(5));

        Assert.Equal(LeadStatus.Approved, lead.Status);
        Assert.Equal(adminId, lead.ApprovedByUserId);
        Assert.Equal(token, lead.RegistrationToken);
        Assert.NotNull(lead.ApprovedAt);
        Assert.NotNull(lead.TokenExpiry);
        Assert.True(lead.IsTokenValid());
    }

    [Fact]
    public void Lead_Approve_NonPendingLead_ThrowsDomainException()
    {
        var lead = new Lead { OrganizationName = "Acme Corp", Status = LeadStatus.Approved };

        Assert.Throws<DomainException>(() => lead.Approve(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void Lead_Reject_PendingLead_TransitionsToRejectedWithReason()
    {
        var lead = new Lead { OrganizationName = "Acme Corp", Status = LeadStatus.Pending };

        lead.Reject("Invalid company registration");

        Assert.Equal(LeadStatus.Rejected, lead.Status);
        Assert.Equal("Invalid company registration", lead.RejectionReason);
        Assert.NotNull(lead.RejectedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Lead_Reject_EmptyReason_ThrowsDomainException(string? reason)
    {
        var lead = new Lead { OrganizationName = "Acme Corp", Status = LeadStatus.Pending };

        Assert.Throws<DomainException>(() => lead.Reject(reason!));
    }

    [Fact]
    public void Lead_Reject_NonPendingLead_ThrowsDomainException()
    {
        var lead = new Lead { OrganizationName = "Acme Corp", Status = LeadStatus.Approved };

        Assert.Throws<DomainException>(() => lead.Reject("Reason"));
    }

    [Fact]
    public void Lead_MarkTokenUsed_ApprovedLeadWithValidToken_TransitionsToCompleted()
    {
        var lead = new Lead
        {
            OrganizationName = "Acme Corp",
            Status = LeadStatus.Approved,
            RegistrationToken = Guid.NewGuid(),
            TokenExpiry = DateTime.UtcNow.AddDays(2)
        };

        lead.MarkTokenUsed();

        Assert.Equal(LeadStatus.Completed, lead.Status);
        Assert.NotNull(lead.TokenUsedAt);
        Assert.False(lead.IsTokenValid());
    }

    [Fact]
    public void Lead_MarkTokenUsed_ExpiredToken_ThrowsDomainException()
    {
        var lead = new Lead
        {
            OrganizationName = "Acme Corp",
            Status = LeadStatus.Approved,
            RegistrationToken = Guid.NewGuid(),
            TokenExpiry = DateTime.UtcNow.AddDays(-1)
        };

        Assert.Throws<DomainException>(() => lead.MarkTokenUsed());
    }
}
