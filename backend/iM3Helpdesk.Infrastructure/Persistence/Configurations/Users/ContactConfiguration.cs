using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Users;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Email
        }).IsUnique();
        builder.Property(x => x.FullName)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(x => x.Email)
            .HasMaxLength(200)
            .IsRequired();
    }
}
