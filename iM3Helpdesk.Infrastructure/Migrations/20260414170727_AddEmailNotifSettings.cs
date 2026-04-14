using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iM3Helpdesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailNotifSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailNotificationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotifKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailNotificationSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationSettings_OrganizationId_NotifKey",
                table: "EmailNotificationSettings",
                columns: new[] { "OrganizationId", "NotifKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailNotificationSettings");
        }
    }
}
