using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iM3Helpdesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIAgentSkillStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalResolved = table.Column<int>(type: "int", nullable: false),
                    AvgResolutionTimeHours = table.Column<double>(type: "float", nullable: false),
                    SlaComplianceRate = table.Column<double>(type: "float", nullable: false),
                    ReopenRate = table.Column<double>(type: "float", nullable: false),
                    LastResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIAgentSkillStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIAgentSkillStats_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AIAgentSkillStats_Users_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIAssignmentLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SuggestedAgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinalAgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    MlScore = table.Column<double>(type: "float", nullable: false),
                    WorkloadScore = table.Column<double>(type: "float", nullable: false),
                    AvailabilityScore = table.Column<double>(type: "float", nullable: false),
                    ReasonJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Suggest"),
                    WasAutoAssigned = table.Column<bool>(type: "bit", nullable: false),
                    IsOverridden = table.Column<bool>(type: "bit", nullable: false),
                    OverriddenByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OverriddenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIAssignmentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIAssignmentLogs_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AIAssignmentLogs_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AIAssignmentLogs_Users_FinalAgentId",
                        column: x => x.FinalAgentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AIAssignmentLogs_Users_OverriddenByUserId",
                        column: x => x.OverriddenByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AIAssignmentLogs_Users_SuggestedAgentId",
                        column: x => x.SuggestedAgentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AIModelVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Algorithm = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "SdcaMaximumEntropy"),
                    Accuracy = table.Column<double>(type: "float", nullable: false),
                    Top3Accuracy = table.Column<double>(type: "float", nullable: false),
                    TrainingSamplesCount = table.Column<int>(type: "int", nullable: false),
                    ModelFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MetricsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TrainedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIModelVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIModelVersions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AISmartAssignConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Suggest"),
                    ConfidenceThreshold = table.Column<double>(type: "float", nullable: false, defaultValue: 0.75),
                    WeightMl = table.Column<double>(type: "float", nullable: false, defaultValue: 0.59999999999999998),
                    WeightWorkload = table.Column<double>(type: "float", nullable: false, defaultValue: 0.20000000000000001),
                    WeightAvailability = table.Column<double>(type: "float", nullable: false, defaultValue: 0.20000000000000001),
                    AutoRetrainFrequencyDays = table.Column<int>(type: "int", nullable: false),
                    MinTrainingSamples = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AISmartAssignConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AISmartAssignConfigs_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeviceInfo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIAgentSkillStats_AgentId",
                table: "AIAgentSkillStats",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAgentSkillStats_OrganizationId_AgentId_Category",
                table: "AIAgentSkillStats",
                columns: new[] { "OrganizationId", "AgentId", "Category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIAssignmentLogs_CreatedAt",
                table: "AIAssignmentLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIAssignmentLogs_FinalAgentId",
                table: "AIAssignmentLogs",
                column: "FinalAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAssignmentLogs_OrganizationId",
                table: "AIAssignmentLogs",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAssignmentLogs_OrganizationId_CreatedAt",
                table: "AIAssignmentLogs",
                columns: new[] { "OrganizationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AIAssignmentLogs_OverriddenByUserId",
                table: "AIAssignmentLogs",
                column: "OverriddenByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAssignmentLogs_SuggestedAgentId",
                table: "AIAssignmentLogs",
                column: "SuggestedAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAssignmentLogs_TicketId",
                table: "AIAssignmentLogs",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_AIModelVersions_OrganizationId_IsActive",
                table: "AIModelVersions",
                columns: new[] { "OrganizationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AIModelVersions_TrainedAt",
                table: "AIModelVersions",
                column: "TrainedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AISmartAssignConfigs_OrganizationId",
                table: "AISmartAssignConfigs",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_FamilyId",
                table: "UserSessions",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_TokenHash",
                table: "UserSessions",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId_IsRevoked",
                table: "UserSessions",
                columns: new[] { "UserId", "IsRevoked" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIAgentSkillStats");

            migrationBuilder.DropTable(
                name: "AIAssignmentLogs");

            migrationBuilder.DropTable(
                name: "AIModelVersions");

            migrationBuilder.DropTable(
                name: "AISmartAssignConfigs");

            migrationBuilder.DropTable(
                name: "UserSessions");
        }
    }
}
