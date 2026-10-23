using System;
using System.Linq;
using System.Threading.Tasks;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace iM3Helpdesk.API.Tests.Infrastructure;

public class ApplicationDbContextConfigurationTests
{
    private sealed class FakeTenantService : ICurrentTenantService
    {
        public Guid? OrganizationId { get; set; }
        public bool IsSuperAdmin { get; set; }

        public FakeTenantService(Guid? organizationId, bool isSuperAdmin)
        {
            OrganizationId = organizationId;
            IsSuperAdmin = isSuperAdmin;
        }
    }

    private static ApplicationDbContext CreateDbContext(string dbName, Guid? tenantId, bool isSuperAdmin)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options, new FakeTenantService(tenantId, isSuperAdmin));
    }

    [Fact]
    public void ModelBuilder_ShouldApplyModularConfigurations_PreservingSchemaConstraints()
    {
        var dbName = Guid.NewGuid().ToString();
        using var db = CreateDbContext(dbName, Guid.NewGuid(), false);
        var model = db.Model;

        // 1. Organization Configuration
        var orgEntity = model.FindEntityType(typeof(Organization));
        Assert.NotNull(orgEntity);
        var slugProp = orgEntity!.FindProperty(nameof(Organization.Slug));
        Assert.NotNull(slugProp);
        Assert.Equal(100, slugProp!.GetMaxLength());
        Assert.False(slugProp.IsNullable);
        var slugIndex = orgEntity.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(Organization.Slug)));
        Assert.NotNull(slugIndex);
        Assert.True(slugIndex!.IsUnique);

        // 2. User Configuration & SuperAdmin Seed
        var userEntity = model.FindEntityType(typeof(User));
        Assert.NotNull(userEntity);
        var emailIndex = userEntity!.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(User.Email)));
        Assert.NotNull(emailIndex);
        Assert.True(emailIndex!.IsUnique);

        db.Database.EnsureCreated();
        var superAdmin = db.Users.IgnoreQueryFilters().FirstOrDefault(u => u.Email == "aadil080399@gmail.com");
        Assert.NotNull(superAdmin);
        Assert.Equal("Super Admin", superAdmin!.FullName);
        Assert.Equal(UserRole.SuperAdmin, superAdmin.Role);

        // 3. Ticket Configuration
        var ticketEntity = model.FindEntityType(typeof(Ticket));
        Assert.NotNull(ticketEntity);
        var ticketNumberProp = ticketEntity!.FindProperty(nameof(Ticket.TicketNumber));
        Assert.NotNull(ticketNumberProp);
        Assert.Equal(ValueGenerated.Never, ticketNumberProp!.ValueGenerated);

        // 4. SLA Cascade Relationships
        var slaPolicyEntity = model.FindEntityType(typeof(SlaPolicy));
        Assert.NotNull(slaPolicyEntity);
        var targetsNav = slaPolicyEntity!.FindNavigation(nameof(SlaPolicy.Targets));
        Assert.NotNull(targetsNav);
        Assert.Equal(DeleteBehavior.Cascade, targetsNav!.ForeignKey.DeleteBehavior);
    }

    [Fact]
    public async Task GlobalQueryFilter_ShouldEnforceTenantIsolation_ForStandardTenants()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        // Seed data using a direct context
        using (var seedDb = CreateDbContext(dbName, null, true))
        {
            seedDb.Tickets.Add(new Ticket
            {
                Id = Guid.NewGuid(),
                OrganizationId = tenantA,
                Title = "Tenant A Ticket",
                Status = TicketStatus.Open,
                IsDeleted = false
            });

            seedDb.Tickets.Add(new Ticket
            {
                Id = Guid.NewGuid(),
                OrganizationId = tenantB,
                Title = "Tenant B Ticket",
                Status = TicketStatus.Open,
                IsDeleted = false
            });

            await seedDb.SaveChangesAsync();
        }

        // Query with Tenant A context
        using (var tenantADb = CreateDbContext(dbName, tenantA, false))
        {
            var tickets = await tenantADb.Tickets.ToListAsync();
            Assert.Single(tickets);
            Assert.Equal("Tenant A Ticket", tickets[0].Title);
            Assert.Equal(tenantA, tickets[0].OrganizationId);
        }

        // Query with Tenant B context
        using (var tenantBDb = CreateDbContext(dbName, tenantB, false))
        {
            var tickets = await tenantBDb.Tickets.ToListAsync();
            Assert.Single(tickets);
            Assert.Equal("Tenant B Ticket", tickets[0].Title);
            Assert.Equal(tenantB, tickets[0].OrganizationId);
        }
    }

    [Fact]
    public async Task GlobalQueryFilter_ShouldAllowSuperAdmin_ToAccessAllTenants()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using (var seedDb = CreateDbContext(dbName, null, true))
        {
            seedDb.Tickets.Add(new Ticket
            {
                Id = Guid.NewGuid(),
                OrganizationId = tenantA,
                Title = "Tenant A Ticket",
                Status = TicketStatus.Open,
                IsDeleted = false
            });

            seedDb.Tickets.Add(new Ticket
            {
                Id = Guid.NewGuid(),
                OrganizationId = tenantB,
                Title = "Tenant B Ticket",
                Status = TicketStatus.Open,
                IsDeleted = false
            });

            await seedDb.SaveChangesAsync();
        }

        // SuperAdmin should see all tickets
        using (var superAdminDb = CreateDbContext(dbName, null, true))
        {
            var tickets = await superAdminDb.Tickets.ToListAsync();
            Assert.Equal(2, tickets.Count);
        }
    }

    [Fact]
    public async Task GlobalQueryFilter_ShouldExcludeSoftDeletedTickets_UnlessIgnored()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantId = Guid.NewGuid();

        using (var seedDb = CreateDbContext(dbName, tenantId, false))
        {
            seedDb.Tickets.Add(new Ticket
            {
                Id = Guid.NewGuid(),
                OrganizationId = tenantId,
                Title = "Active Ticket",
                Status = TicketStatus.Open,
                IsDeleted = false
            });

            seedDb.Tickets.Add(new Ticket
            {
                Id = Guid.NewGuid(),
                OrganizationId = tenantId,
                Title = "Soft Deleted Ticket",
                Status = TicketStatus.Closed,
                IsDeleted = true
            });

            await seedDb.SaveChangesAsync();
        }

        using (var queryDb = CreateDbContext(dbName, tenantId, false))
        {
            // Standard query hides soft-deleted tickets
            var activeTickets = await queryDb.Tickets.ToListAsync();
            Assert.Single(activeTickets);
            Assert.Equal("Active Ticket", activeTickets[0].Title);

            // IgnoreQueryFilters surfaces soft-deleted tickets
            var allTickets = await queryDb.Tickets.IgnoreQueryFilters().Where(t => t.OrganizationId == tenantId).ToListAsync();
            Assert.Equal(2, allTickets.Count);
        }
    }

    [Fact]
    public async Task GlobalQueryFilter_ShouldFilterAgentGroupMember_ThroughGroupOrganization()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using (var seedDb = CreateDbContext(dbName, null, true))
        {
            var groupA = new AgentGroup { Id = Guid.NewGuid(), OrganizationId = tenantA, Name = "Support A" };
            var groupB = new AgentGroup { Id = Guid.NewGuid(), OrganizationId = tenantB, Name = "Support B" };
            seedDb.AgentGroups.AddRange(groupA, groupB);

            seedDb.AgentGroupMembers.Add(new AgentGroupMember
            {
                Id = Guid.NewGuid(),
                AgentGroupId = groupA.Id,
                Group = groupA
            });

            seedDb.AgentGroupMembers.Add(new AgentGroupMember
            {
                Id = Guid.NewGuid(),
                AgentGroupId = groupB.Id,
                Group = groupB
            });

            await seedDb.SaveChangesAsync();
        }

        using (var tenantADb = CreateDbContext(dbName, tenantA, false))
        {
            var members = await tenantADb.AgentGroupMembers.Include(m => m.Group).ToListAsync();
            Assert.Single(members);
            Assert.Equal("Support A", members[0].Group?.Name);
        }
    }

    [Fact]
    public void Architecture_AllEntityConfigurations_MustImplementIEntityTypeConfiguration_AndBeModular()
    {
        var infraAssembly = typeof(ApplicationDbContext).Assembly;
        var configTypes = infraAssembly.GetTypes()
            .Where(t => !t.IsAbstract && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
            .ToList();

        // 51 entities modularized into dedicated configuration classes
        Assert.True(configTypes.Count >= 50, $"Expected at least 50 configuration types, but found {configTypes.Count}");

        foreach (var configType in configTypes)
        {
            Assert.StartsWith("iM3Helpdesk.Infrastructure.Persistence.Configurations", configType.Namespace!);
            Assert.NotNull(configType.GetConstructor(Type.EmptyTypes)); // Must have public parameterless ctor for assembly scanning
        }
    }
}
