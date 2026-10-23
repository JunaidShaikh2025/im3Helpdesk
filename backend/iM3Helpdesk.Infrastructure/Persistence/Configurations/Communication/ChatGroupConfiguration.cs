using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Communication;

public class ChatGroupConfiguration : IEntityTypeConfiguration<ChatGroup>
{
    public void Configure(EntityTypeBuilder<ChatGroup> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();
        builder.HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
