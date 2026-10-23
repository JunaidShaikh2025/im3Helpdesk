using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Work;

public class NoteBookConfiguration : IEntityTypeConfiguration<NoteBook>
{
    public void Configure(EntityTypeBuilder<NoteBook> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(50);
        builder.HasMany(x => x.Sections)
            .WithOne(s => s.NoteBook)
            .HasForeignKey(s => s.NoteBookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
