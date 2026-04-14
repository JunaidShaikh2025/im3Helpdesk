using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iM3Helpdesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketViewers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketViewers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketViewers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketViewers_TicketId_UserId",
                table: "TicketViewers",
                columns: new[] { "TicketId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketViewers");
        }
    }
}
