using System.Diagnostics;
using iM3Helpdesk.Application.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace iM3Helpdesk.API.Tests.Performance;

public class ActualDatabaseDuplicateDetectionTests
{
    private readonly ITestOutputHelper _output;
    private const string ConnectionString = "Server=JUNAID-PC;Database=iM3HelpdeskDb;uid=sa;pwd=im3db;TrustServerCertificate=True;";

    public ActualDatabaseDuplicateDetectionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DetectDuplicates_AgainstActualDatabaseTable_Processes10kRecordsSuccessfully()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        var tenantMock = new Moq.Mock<ICurrentTenantService>();
        tenantMock.Setup(t => t.IsSuperAdmin).Returns(true);

        using var context = new ApplicationDbContext(options, tenantMock.Object);

        // Fetch Organization ID for Amazon Pvt Ltd
        var org = await context.Organizations.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(org);

        var cutoff = DateTime.UtcNow.AddDays(-30);

        // Act 1: Query actual database table
        var dbQuerySw = Stopwatch.StartNew();
        var tickets = await context.Tickets
            .AsNoTracking()
            .Where(t => t.OrganizationId == org.Id && t.CreatedAt >= cutoff && !t.IsDeleted)
            .ToListAsync();
        dbQuerySw.Stop();

        _output.WriteLine($"Actual Database Query Time: {dbQuerySw.ElapsedMilliseconds} ms");
        _output.WriteLine($"Total Actual Records Fetched: {tickets.Count}");

        Assert.True(tickets.Count >= 10000, $"Expected >= 10,000 records in database, but found {tickets.Count}");

        // Act 2: Run optimized duplicate detection algorithm on actual records
        var detectSw = Stopwatch.StartNew();
        var result = AIFeaturesService.DetectDuplicates(tickets);
        detectSw.Stop();

        _output.WriteLine($"Algorithm Execution Time on Actual DB Records: {detectSw.ElapsedMilliseconds} ms");
        _output.WriteLine($"Duplicate Groups Identified: {result.TotalDuplicates}");
        _output.WriteLine($"Potential Duplicate Savings: {result.PotentialSavings}");

        // Assert
        Assert.True(detectSw.ElapsedMilliseconds < 2000,
            $"Expected detection time < 2000 ms, but took {detectSw.ElapsedMilliseconds} ms");
        Assert.True(result.TotalDuplicates >= 10,
            $"Expected at least 10 duplicate groups, found {result.TotalDuplicates}");
        Assert.True(result.PotentialSavings >= 27,
            $"Expected at least 27 potential duplicate savings, found {result.PotentialSavings}");
    }
}
