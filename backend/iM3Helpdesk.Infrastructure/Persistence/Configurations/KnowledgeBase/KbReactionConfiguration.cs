using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.KnowledgeBase;

public class KbReactionConfiguration : IEntityTypeConfiguration<KbReaction>
{
    public void Configure(EntityTypeBuilder<KbReaction> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.ArticleId, x.UserId })
            .IsUnique();
        builder.Property(x => x.ReactionType)
            .HasMaxLength(20)
            .HasDefaultValue("like");
        builder.HasOne(r => r.Article)
            .WithMany(a => a.Reactions)
            .HasForeignKey(r => r.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
