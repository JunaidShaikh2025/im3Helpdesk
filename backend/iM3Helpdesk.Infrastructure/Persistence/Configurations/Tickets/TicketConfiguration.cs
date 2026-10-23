using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Tickets;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title)
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(x => x.Category)
            .HasMaxLength(100);

        // ValueGeneratedNever = no IDENTITY
        builder.Property(x => x.TicketNumber)
            .ValueGeneratedNever()
            .HasDefaultValue(0);

        builder.Property(x => x.Description)
            .HasColumnType("nvarchar(max)");
        builder.Property(x => x.Tags)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(t => t.IsDeleted);

        builder.HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Organization)
            .WithMany()
            .HasForeignKey(t => t.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.OrganizationId);
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => new
        {
            t.OrganizationId,
            t.Status
        });
    }
}
