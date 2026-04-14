using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iM3Helpdesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSlackTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SlackWebhookUrl",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamsWebhookUrl",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlackWebhookUrl",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "TeamsWebhookUrl",
                table: "Organizations");
        }
    }
}
