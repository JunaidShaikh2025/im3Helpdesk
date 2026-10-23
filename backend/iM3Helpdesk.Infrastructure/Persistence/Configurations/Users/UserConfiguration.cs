using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Email)
            .IsUnique();
        builder.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();
        builder.Property(x => x.FullName)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(x => x.Department)
            .HasMaxLength(120);
        builder.Property(x => x.Location)
            .HasMaxLength(120);
        builder.Property(x => x.Designation)
            .HasMaxLength(120);
        builder.Property(x => x.Gender)
            .HasMaxLength(30);
        builder.Property(x => x.IsTwoFactorEnabled)
            .HasDefaultValue(false);

        builder.HasOne(u => u.Organization)
            .WithMany(o => o.Users)
            .HasForeignKey(u => u.OrganizationId);

        // ── Seed: Super Admin (permanent — survives migration resets)
        builder.HasData(new User
        {
            Id = new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
            FullName = "Super Admin",
            Email = "aadil080399@gmail.com",
            PasswordHash = "$2a$11$pzZz3SNnU1NbLPnjJMrVKeeY4IH7CEESwbsvvmdXOChHAu6LptEkG",
            PhoneNumber = "9579818081",
            IsEmailVerified = true,
            Role = UserRole.SuperAdmin,
            OrganizationId = null,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            FailedLoginAttempts = 0
        });
    }
}
