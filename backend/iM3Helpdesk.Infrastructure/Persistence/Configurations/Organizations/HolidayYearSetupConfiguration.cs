using iM3Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iM3Helpdesk.Infrastructure.Persistence.Configurations.Organizations;

public class HolidayYearSetupConfiguration : IEntityTypeConfiguration<HolidayYearSetup>
{
    public void Configure(EntityTypeBuilder<HolidayYearSetup> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PdfFileUrl).HasMaxLength(500);
        builder.Property(x => x.PdfFileName).HasMaxLength(300);
        builder.Property(x => x.PolicyText)
            .HasColumnType("nvarchar(max)");
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.Year
        }).IsUnique();
    }
}
