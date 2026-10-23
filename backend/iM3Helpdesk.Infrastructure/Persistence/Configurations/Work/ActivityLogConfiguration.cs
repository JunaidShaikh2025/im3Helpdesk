using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Work;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.Description)
            .HasColumnType("nvarchar(max)");
        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(a => a.OrganizationId);
    }
}
