using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Communication;

public class ChatGroupMemberConfiguration : IEntityTypeConfiguration<ChatGroupMember>
{
    public void Configure(EntityTypeBuilder<ChatGroupMember> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new
        {
            x.GroupId,
            x.UserId
        }).IsUnique();
        builder.HasOne(x => x.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
