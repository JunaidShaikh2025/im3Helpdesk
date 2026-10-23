using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Users;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrganizationName)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(x => x.OwnerName)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(x => x.WorkEmail)
            .HasMaxLength(256)
            .IsRequired();
        builder.Property(x => x.Phone)
            .HasMaxLength(30);
        builder.Property(x => x.Notes)
            .HasColumnType("nvarchar(max)");
        builder.Property(x => x.RejectionReason)
            .HasMaxLength(500);
        builder.Property(x => x.Status)
            .HasConversion<int>();
        builder.HasIndex(x => x.WorkEmail);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.RegistrationToken)
            .IsUnique()
            .HasFilter("[RegistrationToken] IS NOT NULL");
    }
}
