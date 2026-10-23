using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Tickets;

public class CustomFieldConfiguration : IEntityTypeConfiguration<CustomField>
{
    public void Configure(EntityTypeBuilder<CustomField> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Label)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(x => x.FieldType)
            .HasMaxLength(50);
    }
}
