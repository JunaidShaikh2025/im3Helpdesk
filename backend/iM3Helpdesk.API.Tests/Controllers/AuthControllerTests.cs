using iM3Helpdesk.API.Controllers;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.DTOs.Auth;
using iM3Helpdesk.Application.Services;
using iM3Helpdesk.Infrastructure.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace iM3Helpdesk.API.Tests.Controllers;

public class AuthControllerTests
{
  [Fact]
  public async Task Login_ValidCredentials_SetsHttpOnlyAuthCookies()
  {
    // Arrange
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    var tenantService = new FakeCurrentTenantService();
    await using var context = new ApplicationDbContext(options, tenantService);

    var user = new User
    {
      Id = Guid.NewGuid(),
      FullName = "Test User",
      Email = "test@example.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123"),
      Role = UserRole.Agent,
      IsEmailVerified = true,
      OrganizationId = null
    };

    context.Users.Add(user);
    await context.SaveChangesAsync();

    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["JwtSettings:SecretKey"] = "01234567890123456789012345678901",
          ["JwtSettings:Issuer"] = "iM3Helpdesk",
          ["JwtSettings:Audience"] = "iM3Helpdesk"
        })
        .Build();

    var emailMock = new Mock<IEmailService>();
    var otpMock = new Mock<IOtpService>();
    var uow = new UnitOfWork(
        context,
        tenantService,
        new TicketRepository(context, tenantService),
        new UserRepository(context, tenantService),
        new AuditRepository(context));
    var authService = new AuthService(
        uow,
        config,
        emailMock.Object,
        otpMock.Object,
        NullLogger<AuthService>.Instance);

    var controller = new AuthController(
        authService,
        otpMock.Object,
        NullLogger<AuthController>.Instance)
    {
      ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext()
      }
    };

    var dto = new LoginDto
    {
      Email = user.Email,
      Password = "Pass@123",
      LoginWithOtp = false
    };

    // Act
    var result = await controller.Login(dto);

    // Assert
    Assert.IsType<OkObjectResult>(result);
    var setCookieValues = controller.Response.Headers["Set-Cookie"].ToString();
    Assert.False(string.IsNullOrWhiteSpace(setCookieValues));
    Assert.Contains("HttpOnly", setCookieValues, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task Refresh_ReusingRevokedToken_TriggersFamilyBreachRevocation()
  {
    // Arrange
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    var tenantService = new FakeCurrentTenantService();
    await using var context = new ApplicationDbContext(options, tenantService);

    var user = new User
    {
      Id = Guid.NewGuid(),
      FullName = "Test User",
      Email = "test@example.com",
      PasswordHash = "hashed",
      Role = UserRole.Agent,
      IsEmailVerified = true
    };
    context.Users.Add(user);

    var familyId = Guid.NewGuid();
    var rawStolenToken = "stolen-refresh-token-xyz";
    var stolenHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawStolenToken)));

    // Already revoked token
    var revokedSession = new UserSession
    {
      UserId = user.Id,
      TokenHash = stolenHash,
      FamilyId = familyId,
      ExpiresAt = DateTime.UtcNow.AddDays(30),
      IsRevoked = true,
      RevokedAt = DateTime.UtcNow.AddHours(-1)
    };

    // Legitimate active session in same family
    var activeSiblingSession = new UserSession
    {
      UserId = user.Id,
      TokenHash = "some-active-hash",
      FamilyId = familyId,
      ExpiresAt = DateTime.UtcNow.AddDays(30),
      IsRevoked = false
    };

    context.UserSessions.AddRange(revokedSession, activeSiblingSession);
    await context.SaveChangesAsync();

    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["JwtSettings:SecretKey"] = "01234567890123456789012345678901",
          ["JwtSettings:Issuer"] = "iM3Helpdesk",
          ["JwtSettings:Audience"] = "iM3Helpdesk"
        })
        .Build();

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers.Append("Cookie", $"im3_refresh={rawStolenToken}");

    var otpMock2 = new Mock<IOtpService>();
    var tenantService2 = new FakeCurrentTenantService();
    var uow2 = new UnitOfWork(
        context,
        tenantService2,
        new TicketRepository(context, tenantService2),
        new UserRepository(context, tenantService2),
        new AuditRepository(context));
    var authService2 = new AuthService(
        uow2,
        config,
        new Mock<IEmailService>().Object,
        otpMock2.Object,
        NullLogger<AuthService>.Instance);

    var controller = new AuthController(
        authService2,
        otpMock2.Object,
        NullLogger<AuthController>.Instance)
    {
      ControllerContext = new ControllerContext { HttpContext = httpContext }
    };

    // Act
    var result = await controller.Refresh(new RefreshTokenDto { RefreshToken = rawStolenToken });

    // Assert
    var statusCodeResult = Assert.IsType<ObjectResult>(result);
    Assert.Equal(StatusCodes.Status401Unauthorized, statusCodeResult.StatusCode);

    // Verify all sessions in that family got revoked!
    var siblingInDb = await context.UserSessions.FirstAsync(s => s.TokenHash == "some-active-hash");
    Assert.True(siblingInDb.IsRevoked);
    Assert.NotNull(siblingInDb.RevokedAt);
  }

  private sealed class FakeCurrentTenantService : ICurrentTenantService
  {
    public Guid? OrganizationId => null;
    public bool IsSuperAdmin => true;
  }
}
