using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.AI;

public class AIAssignmentLogConfiguration : IEntityTypeConfiguration<AIAssignmentLog>
{
    public void Configure(EntityTypeBuilder<AIAssignmentLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ConfidenceScore).HasColumnType("float");
        builder.Property(x => x.MlScore).HasColumnType("float");
        builder.Property(x => x.WorkloadScore).HasColumnType("float");
        builder.Property(x => x.AvailabilityScore).HasColumnType("float");
        builder.Property(x => x.ReasonJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Mode).HasMaxLength(50).HasDefaultValue("Suggest");

        builder.HasOne(x => x.Ticket)
            .WithMany()
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SuggestedAgent)
            .WithMany()
            .HasForeignKey(x => x.SuggestedAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FinalAgent)
            .WithMany()
            .HasForeignKey(x => x.FinalAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OverriddenByUser)
            .WithMany()
            .HasForeignKey(x => x.OverriddenByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TicketId);
        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.OrganizationId, x.CreatedAt });
    }
}
