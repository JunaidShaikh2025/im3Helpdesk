using System.Linq;
using System.Reflection;
using iM3Helpdesk.API.Controllers;
using iM3Helpdesk.Infrastructure.Persistence;
using Xunit;

namespace iM3Helpdesk.API.Tests.Controllers;

public class AuthControllerDependencyInversionTests
{
    [Fact]
    public void AuthController_Constructor_ShouldNotDependOn_ApplicationDbContext()
    {
        var constructorParameters = typeof(AuthController)
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .SelectMany(c => c.GetParameters())
            .Select(p => p.ParameterType)
            .ToList();

        Assert.DoesNotContain(typeof(ApplicationDbContext), constructorParameters);
    }
}
