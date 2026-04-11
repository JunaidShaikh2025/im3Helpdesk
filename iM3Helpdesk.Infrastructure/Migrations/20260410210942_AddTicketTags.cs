using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iM3Helpdesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Tickets");
        }
    }
}
