using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Communication;

public class EmailNotificationSettingConfiguration : IEntityTypeConfiguration<EmailNotificationSetting>
{
    public void Configure(EntityTypeBuilder<EmailNotificationSetting> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.NotifKey
        });
    }
}
