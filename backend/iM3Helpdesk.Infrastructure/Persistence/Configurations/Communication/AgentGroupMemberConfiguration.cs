using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Communication;

public class AgentGroupMemberConfiguration : IEntityTypeConfiguration<AgentGroupMember>
{
    public void Configure(EntityTypeBuilder<AgentGroupMember> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne(m => m.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(m => m.AgentGroupId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
