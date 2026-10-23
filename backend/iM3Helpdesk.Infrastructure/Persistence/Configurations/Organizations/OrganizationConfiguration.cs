using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Organizations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Slug)
            .IsUnique();
        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(x => x.Slug)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.SupportEmail)
            .HasMaxLength(256);
        builder.Property(x => x.SmtpHost)
            .HasMaxLength(200);
        builder.Property(x => x.SmtpFromEmail)
            .HasMaxLength(256);
        builder.Property(x => x.SmtpFromName)
            .HasMaxLength(200);
        builder.Property(x => x.SmtpUsername)
            .HasMaxLength(256);
        builder.Property(x => x.SmtpPassword)
            .HasMaxLength(500);
        builder.Property(x => x.ImapHost)
            .HasMaxLength(200);
    }
}
