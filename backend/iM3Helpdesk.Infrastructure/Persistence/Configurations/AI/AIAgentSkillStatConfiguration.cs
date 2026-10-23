using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.AI;

public class AIAgentSkillStatConfiguration : IEntityTypeConfiguration<AIAgentSkillStat>
{
    public void Configure(EntityTypeBuilder<AIAgentSkillStat> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Category).HasMaxLength(200).IsRequired();

        builder.HasOne(x => x.Agent)
            .WithMany()
            .HasForeignKey(x => x.AgentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.OrganizationId, x.AgentId, x.Category }).IsUnique();
    }
}
