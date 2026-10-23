using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Tickets;

public class CommentReactionConfiguration : IEntityTypeConfiguration<CommentReaction>
{
    public void Configure(EntityTypeBuilder<CommentReaction> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.CommentId, x.UserId })
            .IsUnique();
        builder.Property(x => x.ReactionType)
            .HasMaxLength(20)
            .HasDefaultValue("like");
        builder.HasOne(r => r.Comment)
            .WithMany()
            .HasForeignKey(r => r.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
