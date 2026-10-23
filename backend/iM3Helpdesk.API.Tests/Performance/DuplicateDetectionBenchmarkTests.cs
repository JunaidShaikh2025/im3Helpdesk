using System.Diagnostics;
using iM3Helpdesk.Application.DTOs.AI;
using iM3Helpdesk.Application.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using Xunit;
using Xunit.Abstractions;

namespace iM3Helpdesk.API.Tests.Performance;

public class DuplicateDetectionBenchmarkTests
{
    private readonly ITestOutputHelper _output;

    public DuplicateDetectionBenchmarkTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void DuplicateDetection_With10000Tickets_ProcessesUnder2Seconds_AndIdentifiesDuplicatesAccurately()
    {
        // Arrange: Generate 10,000 realistic helpdesk tickets with 10 injected duplicate clusters
        const int totalRecords = 10_000;
        var tickets = GenerateSyntheticHelpdeskTickets(totalRecords, out var expectedClusters);

        Assert.Equal(totalRecords, tickets.Count);

        // Warm-up JIT compilation on a small batch
        var warmUp = tickets.Take(20).ToList();
        _ = AIFeaturesService.DetectDuplicates(warmUp);

        // Act: Measure execution time for all 10,000 records
        var sw = Stopwatch.StartNew();
        var result = AIFeaturesService.DetectDuplicates(tickets);
        sw.Stop();

        _output.WriteLine($"Execution Time: {sw.ElapsedMilliseconds} ms");
        _output.WriteLine($"Total Records: {tickets.Count}");
        _output.WriteLine($"Duplicate Groups Identified: {result.TotalDuplicates}");
        _output.WriteLine($"Potential Savings (Duplicates): {result.PotentialSavings}");

        // Assert: Performance threshold (strictly under 2,000 ms)
        Assert.True(sw.ElapsedMilliseconds < 2000,
            $"Performance SLA breached: 10,000 records took {sw.ElapsedMilliseconds} ms (target: < 2000 ms)");

        // Assert: Accuracy of duplicate identification
        Assert.NotNull(result);
        Assert.Equal(expectedClusters.Count, result.TotalDuplicates);

        int expectedDuplicateTickets = expectedClusters.Sum(c => c.Count - 1);
        Assert.Equal(expectedDuplicateTickets, result.PotentialSavings);

        // Verify each injected cluster was correctly grouped
        foreach (var cluster in expectedClusters)
        {
            var originalId = cluster[0].Id;
            var matchedGroup = result.Groups.FirstOrDefault(g => g.Tickets.Any(t => t.Id == originalId));

            Assert.NotNull(matchedGroup);
            Assert.Equal(cluster.Count, matchedGroup.Count);

            foreach (var expectedTicket in cluster)
            {
                Assert.Contains(matchedGroup.Tickets, t => t.Id == expectedTicket.Id);
            }
        }
    }

    [Fact]
    public void DetectDuplicates_EmptyList_ReturnsZeroDuplicates()
    {
        var result = AIFeaturesService.DetectDuplicates(new List<Ticket>());
        Assert.Equal(0, result.TotalDuplicates);
        Assert.Equal(0, result.PotentialSavings);
        Assert.Empty(result.Groups);
    }

    [Fact]
    public void DetectDuplicates_SingleTicket_ReturnsZeroDuplicates()
    {
        var tickets = new List<Ticket>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TicketNumber = 101,
                Title = "Unique ticket title without duplicates",
                Category = "IT Support",
                Status = TicketStatus.Open
            }
        };

        var result = AIFeaturesService.DetectDuplicates(tickets);
        Assert.Equal(0, result.TotalDuplicates);
        Assert.Empty(result.Groups);
    }

    [Fact]
    public void DetectDuplicates_SameUserAndCategory_AppliesSimilarityBonus()
    {
        var userId = Guid.NewGuid();
        var tickets = new List<Ticket>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TicketNumber = 201,
                Title = "VPN connection drops after standby",
                Category = "Network",
                CreatedByUserId = userId,
                Status = TicketStatus.Open
            },
            new()
            {
                Id = Guid.NewGuid(),
                TicketNumber = 202,
                Title = "VPN connection drops after wake",
                Category = "Network",
                CreatedByUserId = userId,
                Status = TicketStatus.Open
            }
        };

        var result = AIFeaturesService.DetectDuplicates(tickets);

        Assert.Equal(1, result.TotalDuplicates);
        Assert.Single(result.Groups);
        Assert.Equal(2, result.Groups[0].Count);
    }

    [Fact]
    public void Tokenize_ExtractsNormalizedTokensWithoutAllocatingDuplicates()
    {
        var tokens = AIFeaturesService.Tokenize("VPN, vpn! connection - Connection / failed.");

        Assert.Equal(3, tokens.Count);
        Assert.Contains("vpn", tokens);
        Assert.Contains("connection", tokens);
        Assert.Contains("failed", tokens);
    }

    [Fact]
    public void CalculateSimilarity_ReturnsExactJaccardCoefficient()
    {
        var set1 = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "vpn", "connection", "failed" };
        var set2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "vpn", "connection", "timeout" };

        // Intersection: { "vpn", "connection" } (2)
        // Union: { "vpn", "connection", "failed", "timeout" } (4)
        // Expected Jaccard: 2 / 4 = 0.5
        var sim = AIFeaturesService.CalculateSimilarity(set1, set2);

        Assert.Equal(0.5, sim, precision: 4);
    }

    private static List<Ticket> GenerateSyntheticHelpdeskTickets(int totalCount, out List<List<Ticket>> duplicateClusters)
    {
        var tickets = new List<Ticket>(totalCount);
        duplicateClusters = new List<List<Ticket>>();
        var random = new Random(42);
        int currentTicketNumber = 1000;

        // 1. Define 10 deliberate duplicate clusters
        var clusterDefinitions = new (string Category, string[] Titles)[]
        {
            ("VPN", new[]
            {
                "GlobalProtect VPN connection failed with error 500",
                "GlobalProtect VPN connection failed with error 500",
                "GlobalProtect VPN connection failed error 500",
                "GlobalProtect VPN connection failed with error 500 gateway timeout",
                "GlobalProtect VPN connection failure with error 500"
            }),
            ("Email", new[]
            {
                "Outlook desktop client crashes on startup after Windows update",
                "Outlook desktop client crashes on startup after Windows update",
                "Outlook desktop client crashes on startup after recent Windows update",
                "Outlook desktop client crash on startup after Windows update"
            }),
            ("Database", new[]
            {
                "Database deadlock on invoice payment processing endpoint",
                "Database deadlock on invoice payment processing endpoint",
                "Database deadlock on payment processing endpoint"
            }),
            ("Authentication", new[]
            {
                "Cannot access internal payroll portal via Okta SSO",
                "Cannot access internal payroll portal via Okta SSO",
                "Cannot access internal payroll portal via SSO",
                "Cannot access internal payroll portal via Okta SSO login"
            }),
            ("Hardware", new[]
            {
                "Printer 3rd floor marketing department offline and unresponsive",
                "Printer 3rd floor marketing department offline and unresponsive",
                "Printer 3rd floor marketing department offline"
            }),
            ("Integrations", new[]
            {
                "Salesforce CRM integration authentication token expired",
                "Salesforce CRM integration authentication token expired",
                "Salesforce CRM integration authentication token has expired",
                "Salesforce integration authentication token expired"
            }),
            ("Cloud", new[]
            {
                "Network timeout when uploading large file to S3 bucket",
                "Network timeout when uploading large file to S3 bucket",
                "Network timeout uploading large file to S3 bucket"
            }),
            ("Audio/Video", new[]
            {
                "Zoom meeting room audio hardware microphone not detected",
                "Zoom meeting room audio hardware microphone not detected",
                "Zoom meeting room audio microphone not detected",
                "Zoom meeting audio hardware microphone not detected"
            }),
            ("Network", new[]
            {
                "WiFi connection drops intermittently in Conference Room B",
                "WiFi connection drops intermittently in Conference Room B",
                "WiFi connection drops in Conference Room B"
            }),
            ("Billing", new[]
            {
                "Failed recurring credit card charge for customer subscription renewal",
                "Failed recurring credit card charge for customer subscription renewal",
                "Failed recurring credit card charge customer subscription renewal",
                "Failed credit card charge for customer subscription renewal"
            })
        };

        // Inject deliberate clusters
        foreach (var def in clusterDefinitions)
        {
            var clusterTickets = new List<Ticket>();
            var clusterUser = Guid.NewGuid();

            foreach (var title in def.Titles)
            {
                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    TicketNumber = currentTicketNumber++,
                    Title = title,
                    Category = def.Category,
                    CreatedByUserId = clusterUser,
                    Status = TicketStatus.Open,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(60, 20000))
                };
                clusterTickets.Add(ticket);
                tickets.Add(ticket);
            }

            duplicateClusters.Add(clusterTickets);
        }

        // 2. Generate remaining realistic unique tickets
        var actions = new[] { "Configure", "Provision", "Update", "Audit", "Inspect", "Migrate", "Deploy", "Decommission", "Rotate", "Monitor" };
        var systems = new[] { "Kubernetes cluster", "Elasticsearch index", "PostgreSQL replica", "RabbitMQ message queue", "Grafana telemetry dashboard", "Nginx reverse proxy", "Linux worker node", "Docker registry", "Terraform state store", "Prometheus exporter" };
        var environments = new[] { "Production-US-East", "Staging-EU-Central", "Dev-Sandbox-APAC", "QA-Environment-01", "DisasterRecovery-West", "Internal-Services-VPC" };
        var categories = new[] { "Infrastructure", "DevOps", "Database", "Security", "Operations", "Compliance" };

        int remaining = totalCount - tickets.Count;
        for (int i = 0; i < remaining; i++)
        {
            var act = actions[i % actions.Length];
            var sys = systems[(i / actions.Length) % systems.Length];
            var env = environments[(i / (actions.Length * systems.Length)) % environments.Length];
            var cat = categories[i % categories.Length];

            var uniqueTicket = new Ticket
            {
                Id = Guid.NewGuid(),
                TicketNumber = currentTicketNumber++,
                Title = $"{act} {sys} in {env} maintenance window [Batch-{i:D5}]",
                Category = cat,
                CreatedByUserId = Guid.NewGuid(),
                Status = TicketStatus.Open,
                CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1, 40000))
            };

            tickets.Add(uniqueTicket);
        }

        return tickets;
    }
}
