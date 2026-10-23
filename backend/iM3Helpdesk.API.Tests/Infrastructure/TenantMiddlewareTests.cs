using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using iM3Helpdesk.API.Middleware;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace iM3Helpdesk.API.Tests.Infrastructure;

public class TenantMiddlewareTests
{
    private readonly Mock<ILogger<TenantMiddleware>> _loggerMock = new();
    private readonly TokenValidationParameters _tokenValidationParams = new();

    private sealed class CustomTenantServiceFake : ICurrentTenantService
    {
        public Guid? OrganizationId { get; set; }
        public bool IsSuperAdmin { get; set; }

        public void SetTenant(Guid? organizationId, bool isSuperAdmin)
        {
            OrganizationId = organizationId;
            IsSuperAdmin = isSuperAdmin;
        }

        public void SetOrganizationId(Guid? organizationId) => OrganizationId = organizationId;
        public void SetSuperAdmin(bool isSuperAdmin) => IsSuperAdmin = isSuperAdmin;
    }

    [Fact]
    public async Task InvokeAsync_WithMockedICurrentTenantService_ShouldNotThrowInvalidCastException_LspCompliant()
    {
        // Arrange: A strict mock of ICurrentTenantService (not a CurrentTenantService concrete instance).
        // Before ARCH-04, this threw InvalidCastException because of ((CurrentTenantService)tenantService) downcasting.
        var mockTenantService = new Mock<ICurrentTenantService>();
        var tenantId = Guid.NewGuid();

        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("organizationId", tenantId.ToString()),
            new Claim("role", "Agent")
        }, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);

        var nextInvoked = false;
        RequestDelegate next = (ctx) =>
        {
            nextInvoked = true;
            return Task.CompletedTask;
        };

        var middleware = new TenantMiddleware(next, _loggerMock.Object, _tokenValidationParams);

        // Act & Assert (Should not throw InvalidCastException)
        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(httpContext, mockTenantService.Object));
        Assert.Null(exception);
        Assert.True(nextInvoked);

        mockTenantService.Verify(s => s.SetTenant(tenantId, false), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenSuperAdmin_ShouldConfigureSuperAdminScopeWithoutTenantId()
    {
        var fakeTenantService = new CustomTenantServiceFake();
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("role", "SuperAdmin")
        }, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);

        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new TenantMiddleware(next, _loggerMock.Object, _tokenValidationParams);

        // Act
        await middleware.InvokeAsync(httpContext, fakeTenantService);

        // Assert
        Assert.True(fakeTenantService.IsSuperAdmin);
        Assert.Null(fakeTenantService.OrganizationId);
    }

    [Fact]
    public async Task InvokeAsync_WhenStandardTenant_ShouldConfigureTenantIdAndDisableSuperAdmin()
    {
        var fakeTenantService = new CustomTenantServiceFake();
        var tenantId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("organizationId", tenantId.ToString()),
            new Claim("role", "Customer")
        }, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);

        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new TenantMiddleware(next, _loggerMock.Object, _tokenValidationParams);

        // Act
        await middleware.InvokeAsync(httpContext, fakeTenantService);

        // Assert
        Assert.False(fakeTenantService.IsSuperAdmin);
        Assert.Equal(tenantId, fakeTenantService.OrganizationId);
    }
}
