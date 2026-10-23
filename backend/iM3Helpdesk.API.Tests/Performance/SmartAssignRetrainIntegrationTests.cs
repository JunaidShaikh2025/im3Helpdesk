using iM3Helpdesk.Infrastructure.ML;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace iM3Helpdesk.API.Tests.Performance;

public class SmartAssignRetrainIntegrationTests
{
    private readonly ITestOutputHelper _output;
    private const string ConnectionString = "Server=JUNAID-PC;Database=iM3HelpdeskDb;uid=sa;pwd=im3db;TrustServerCertificate=True;";

    public SmartAssignRetrainIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task RetrainModel_WithInsertedResolvedTickets_SuccessfullyTrainsAndDeploysModel()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        var tenantMock = new Mock<ICurrentTenantService>();
        tenantMock.Setup(t => t.IsSuperAdmin).Returns(true);

        using var context = new ApplicationDbContext(options, tenantMock.Object);

        var org = await context.Organizations.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(org);

        var mlTrainer = new SmartAssignMlTrainer(NullLogger<SmartAssignMlTrainer>.Instance);
        var service = new SmartAssignService(
            context,
            mlTrainer,
            NullLogger<SmartAssignService>.Instance);

        var tempStoragePath = Path.Combine(Path.GetTempPath(), "iM3HelpdeskTest_ML");
        Directory.CreateDirectory(tempStoragePath);

        // Act: Run RetrainModelAsync
        var result = await service.RetrainModelAsync(org.Id, tempStoragePath);

        _output.WriteLine($"Success: {result.Success}");
        _output.WriteLine($"Message: {result.Message}");
        _output.WriteLine($"Model Version: {result.Version}");
        _output.WriteLine($"Accuracy: {result.Accuracy}%");
        _output.WriteLine($"Top-3 Accuracy: {result.Top3Accuracy}%");
        _output.WriteLine($"Samples Count: {result.SamplesCount}");

        // Assert
        Assert.True(result.Success, $"Expected RetrainModelAsync to succeed, but got: {result.Message}");
        Assert.True(result.SamplesCount >= 5, $"Expected at least 5 samples, got {result.SamplesCount}");
        Assert.True(result.Accuracy > 0, "Expected accuracy > 0%");
        Assert.NotEqual("None", result.Version);

        // Verify active model version in database
        var activeModel = await context.AIModelVersions
            .IgnoreQueryFilters()
            .Where(m => m.OrganizationId == org.Id && m.IsActive)
            .FirstOrDefaultAsync();

        Assert.NotNull(activeModel);
        Assert.Equal(result.Version, activeModel.Version);
        Assert.True(activeModel.IsActive);
    }
}
