using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.KnowledgeBase;

public class KbArticleConfiguration : IEntityTypeConfiguration<KbArticle>
{
    public void Configure(EntityTypeBuilder<KbArticle> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title)
            .HasMaxLength(300)
            .IsRequired();
        builder.Property(x => x.Content)
            .IsRequired();
        builder.Property(x => x.Category)
            .HasMaxLength(100);
        builder.Property(x => x.MediaUrl)
            .HasMaxLength(500)
            .HasDefaultValue("");
        builder.Property(x => x.MediaType)
            .HasMaxLength(20)
            .HasDefaultValue("none");
        builder.HasOne(a => a.CreatedBy)
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
