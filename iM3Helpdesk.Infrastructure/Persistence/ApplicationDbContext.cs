using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly Guid? _currentTenantId;
    private readonly bool _isSuperAdmin;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenantService tenantService) : base(options)
    {
        _currentTenantId = tenantService.OrganizationId;
        _isSuperAdmin = tenantService.IsSuperAdmin;
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Organization>(e => {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<User>(e => {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();

            e.HasQueryFilter(u =>
                _isSuperAdmin ||
                u.OrganizationId == _currentTenantId);

            e.HasOne(u => u.Organization)
             .WithMany(o => o.Users)
             .HasForeignKey(u => u.OrganizationId);
        });
        modelBuilder.Entity<Ticket>(e => {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(500).IsRequired();
            e.Property(x => x.Category).HasMaxLength(100);

            e.HasQueryFilter(t =>
                _isSuperAdmin ||
                t.OrganizationId == _currentTenantId);

            e.HasOne(t => t.CreatedBy)
             .WithMany()
             .HasForeignKey(t => t.CreatedByUserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.AssignedTo)
             .WithMany()
             .HasForeignKey(t => t.AssignedToUserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.Organization)
             .WithMany()
             .HasForeignKey(t => t.OrganizationId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TicketComment>(e => {
            e.HasKey(x => x.Id);
            e.Property(x => x.Comment).HasMaxLength(2000).IsRequired();

            e.HasQueryFilter(tc =>
                _isSuperAdmin ||
                tc.OrganizationId == _currentTenantId);

            e.HasOne(tc => tc.Ticket)
             .WithMany(t => t.Comments)
             .HasForeignKey(tc => tc.TicketId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(tc => tc.User)
             .WithMany()
             .HasForeignKey(tc => tc.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Notification>(e => {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Message).HasMaxLength(500).IsRequired();

            e.HasQueryFilter(n =>
                _isSuperAdmin ||
                n.OrganizationId == _currentTenantId);

            e.HasOne(n => n.User)
             .WithMany()
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(n => n.Ticket)
             .WithMany()
             .HasForeignKey(n => n.TicketId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ActivityLog>(e => {
            e.HasKey(x => x.Id);
            e.Property(x => x.Action).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);

            e.HasQueryFilter(a =>
                _isSuperAdmin ||
                a.OrganizationId == _currentTenantId);

            e.HasOne(a => a.User)
             .WithMany()
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}