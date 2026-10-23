using System.Linq;
using System.Reflection;
using iM3Helpdesk.API.Controllers;
using iM3Helpdesk.Infrastructure.Persistence;
using Xunit;

namespace iM3Helpdesk.API.Tests.Controllers;

public class AIFeaturesControllerDependencyInversionTests
{
    [Fact]
    public void AIFeaturesController_Constructor_ShouldNotDependOn_ApplicationDbContext()
    {
        // Enforces dependency inversion and Clean Architecture at controller boundary.
        var constructorParameters = typeof(AIFeaturesController)
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .SelectMany(c => c.GetParameters())
            .Select(p => p.ParameterType)
            .ToList();

        Assert.DoesNotContain(typeof(ApplicationDbContext), constructorParameters);
    }
}
