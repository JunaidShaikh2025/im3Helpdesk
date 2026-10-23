using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Work;

public class NoteSectionConfiguration : IEntityTypeConfiguration<NoteSection>
{
    public void Configure(EntityTypeBuilder<NoteSection> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(50);
        builder.HasMany(x => x.Pages)
            .WithOne(p => p.Section)
            .HasForeignKey(p => p.NoteSectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
