using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iM3Helpdesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHolidaySetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Occasion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Day = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    IsFloating = table.Column<bool>(type: "bit", nullable: false),
                    HolidayYearSetupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HolidayYearSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    PdfFileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PdfFileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FloatingHolidayAllowance = table.Column<int>(type: "int", nullable: false),
                    PolicyText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayYearSetups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_OrganizationId_Year_Date",
                table: "Holidays",
                columns: new[] { "OrganizationId", "Year", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_HolidayYearSetups_OrganizationId_Year",
                table: "HolidayYearSetups",
                columns: new[] { "OrganizationId", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "HolidayYearSetups");
        }
    }
}
