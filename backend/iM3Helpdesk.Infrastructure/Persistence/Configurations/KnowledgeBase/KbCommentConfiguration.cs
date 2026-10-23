using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.KnowledgeBase;

public class KbCommentConfiguration : IEntityTypeConfiguration<KbComment>
{
    public void Configure(EntityTypeBuilder<KbComment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Text)
            .HasColumnType("nvarchar(max)")
            .IsRequired();
        builder.HasOne(c => c.Article)
            .WithMany(a => a.Comments)
            .HasForeignKey(c => c.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
