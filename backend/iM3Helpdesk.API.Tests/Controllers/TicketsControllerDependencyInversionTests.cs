using System.Linq;
using System.Reflection;
using iM3Helpdesk.API.Controllers;
using iM3Helpdesk.Infrastructure.Persistence;
using Xunit;

namespace iM3Helpdesk.API.Tests.Controllers;

public class TicketsControllerDependencyInversionTests
{
    [Fact]
    public void TicketsController_Constructor_ShouldNotDependOn_ApplicationDbContext()
    {
        // Enforces dependency inversion and Clean Architecture at controller boundary.
        var constructorParameters = typeof(TicketsController)
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .SelectMany(c => c.GetParameters())
            .Select(p => p.ParameterType)
            .ToList();

        Assert.DoesNotContain(typeof(ApplicationDbContext), constructorParameters);
    }

    [Theory]
    [InlineData(typeof(TicketsControllerBase))]
    [InlineData(typeof(TicketAssignmentsController))]
    [InlineData(typeof(TicketCommentsController))]
    [InlineData(typeof(TicketStatusesController))]
    public void TicketControllers_ShouldNotDependOn_ApplicationDbContext(Type controllerType)
    {
        var constructorParameters = controllerType
            .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .SelectMany(c => c.GetParameters())
            .Select(p => p.ParameterType)
            .ToList();

        Assert.DoesNotContain(typeof(ApplicationDbContext), constructorParameters);
    }
}
