using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Users;

public class UserOnlineStatusConfiguration : IEntityTypeConfiguration<UserOnlineStatus>
{
    public void Configure(EntityTypeBuilder<UserOnlineStatus> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserId)
            .IsUnique();
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
