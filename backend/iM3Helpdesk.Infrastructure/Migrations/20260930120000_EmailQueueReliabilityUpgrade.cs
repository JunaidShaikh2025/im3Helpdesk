using System;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iM3Helpdesk.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260930120000_EmailQueueReliabilityUpgrade")]
    public partial class EmailQueueReliabilityUpgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "EmailQueues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "EmailQueues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FailedAt",
                table: "EmailQueues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HangfireJobId",
                table: "EmailQueues",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAttemptAt",
                table: "EmailQueues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastWebhookEvent",
                table: "EmailQueues",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWebhookAt",
                table: "EmailQueues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageId",
                table: "EmailQueues",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayloadJson",
                table: "EmailQueues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderMessageId",
                table: "EmailQueues",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "EmailQueues",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Queued");

            migrationBuilder.CreateTable(
                name: "EmailWebhookLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailQueueId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Recipient = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ProviderMessageId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Processed = table.Column<bool>(type: "bit", nullable: false),
                    ProcessingError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailWebhookLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_CreatedAt",
                table: "EmailQueues",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_MessageId",
                table: "EmailQueues",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_NextRetryAt",
                table: "EmailQueues",
                column: "NextRetryAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_ProviderMessageId",
                table: "EmailQueues",
                column: "ProviderMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_Status",
                table: "EmailQueues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmailWebhookLogs_EmailQueueId",
                table: "EmailWebhookLogs",
                column: "EmailQueueId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailWebhookLogs_ProviderMessageId",
                table: "EmailWebhookLogs",
                column: "ProviderMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailWebhookLogs_ReceivedAt",
                table: "EmailWebhookLogs",
                column: "ReceivedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailWebhookLogs");

            migrationBuilder.DropIndex(
                name: "IX_EmailQueues_CreatedAt",
                table: "EmailQueues");

            migrationBuilder.DropIndex(
                name: "IX_EmailQueues_MessageId",
                table: "EmailQueues");

            migrationBuilder.DropIndex(
                name: "IX_EmailQueues_NextRetryAt",
                table: "EmailQueues");

            migrationBuilder.DropIndex(
                name: "IX_EmailQueues_ProviderMessageId",
                table: "EmailQueues");

            migrationBuilder.DropIndex(
                name: "IX_EmailQueues_Status",
                table: "EmailQueues");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "EmailQueues");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "EmailQueues");

            migrationBuilder.DropColumn(
                name: "FailedAt",
                table: "EmailQueues");

            migrationBuilder.DropColumn(
                name: "HangfireJobId",
                table: "EmailQueues");

            migrationBuilder.DropColumn(
                name: "LastAttemptAt",
                table: "EmailQueues");

            migrationBuilder.DropColumn(
                name: "LastWebhookEvent",
                table: "EmailQueues");

            migrationBuilder.DropColumn(
                name: "LastWebhookAt",
                table: "EmailQueues");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "EmailQueues");

            migrationBuilder.DropColumn(
                name: "PayloadJson",
                table: "EmailQueues");

            migrationBuilder.DropColumn(
                name: "ProviderMessageId",
                table: "EmailQueues");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EmailQueues");
        }
    }
}
