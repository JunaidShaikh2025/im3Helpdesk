using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Communication;

public class AgentGroupConfiguration : IEntityTypeConfiguration<AgentGroup>
{
    public void Configure(EntityTypeBuilder<AgentGroup> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(x => x.BusinessHoursId);
    }
}
