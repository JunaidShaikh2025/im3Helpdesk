using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Users;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.DeviceInfo).HasMaxLength(250);
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.ReplacedByTokenHash).HasMaxLength(64);

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.FamilyId);
        builder.HasIndex(x => new { x.UserId, x.IsRevoked });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
