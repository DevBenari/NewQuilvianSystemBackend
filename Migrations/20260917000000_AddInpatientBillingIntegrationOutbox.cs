using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-127 / RWI-DEC-161, langkah migration Fondasi Outbox dan Occupancy Tracking.
    /// Memasang tabel InpIntegrationOutboxes dan kolom tracking hunian pada InpBedPlacement dan InpEpisode.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260917000000_AddInpatientBillingIntegrationOutbox")]
    public partial class AddInpatientBillingIntegrationOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InpIntegrationOutboxes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SourceDomain = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceDetailId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NextRetryAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InpIntegrationOutboxes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_InpIntegrationOutbox_IdempotencyKey",
                schema: "public",
                table: "InpIntegrationOutboxes",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpIntegrationOutbox_Status_Pending",
                schema: "public",
                table: "InpIntegrationOutboxes",
                column: "Status",
                filter: "\"Status\" IN (0, 3)");

            migrationBuilder.CreateIndex(
                name: "IX_InpIntegrationOutboxes_SourceDomain",
                schema: "public",
                table: "InpIntegrationOutboxes",
                column: "SourceDomain");

            migrationBuilder.CreateIndex(
                name: "IX_InpIntegrationOutboxes_SourceType",
                schema: "public",
                table: "InpIntegrationOutboxes",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_InpIntegrationOutboxes_SourceDetailId",
                schema: "public",
                table: "InpIntegrationOutboxes",
                column: "SourceDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_InpIntegrationOutboxes_EventType",
                schema: "public",
                table: "InpIntegrationOutboxes",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_InpIntegrationOutboxes_NextRetryAtUtc",
                schema: "public",
                table: "InpIntegrationOutboxes",
                column: "NextRetryAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_InpIntegrationOutboxes_CreatedAtUtc",
                schema: "public",
                table: "InpIntegrationOutboxes",
                column: "CreatedAtUtc");

            // Tracking fields pada InpBedPlacement
            migrationBuilder.AddColumn<DateTime>(
                name: "PhysicallyLeftAt",
                schema: "public",
                table: "InpBedPlacement",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "public",
                table: "InpBedPlacement",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "ChangeReason",
                schema: "public",
                table: "InpBedPlacement",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuperseded",
                schema: "public",
                table: "InpBedPlacement",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupersededAtUtc",
                schema: "public",
                table: "InpBedPlacement",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_IsSuperseded",
                schema: "public",
                table: "InpBedPlacement",
                column: "IsSuperseded");

            // Tracking clearance & override pada InpEpisode
            migrationBuilder.AddColumn<int>(
                name: "ClearanceStatus",
                schema: "public",
                table: "InpEpisode",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ClearanceRevokedReason",
                schema: "public",
                table: "InpEpisode",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSupervisorOverridden",
                schema: "public",
                table: "InpEpisode",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SupervisorOverrideReason",
                schema: "public",
                table: "InpEpisode",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SupervisorOverriddenByUserId",
                schema: "public",
                table: "InpEpisode",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupervisorOverriddenAtUtc",
                schema: "public",
                table: "InpEpisode",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InpIntegrationOutboxes",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_InpBedPlacement_IsSuperseded",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropColumn(
                name: "SupersededAtUtc",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropColumn(
                name: "IsSuperseded",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropColumn(
                name: "ChangeReason",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropColumn(
                name: "PhysicallyLeftAt",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropColumn(
                name: "SupervisorOverriddenAtUtc",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "SupervisorOverriddenByUserId",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "SupervisorOverrideReason",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "IsSupervisorOverridden",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "ClearanceRevokedReason",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "ClearanceStatus",
                schema: "public",
                table: "InpEpisode");
        }
    }
}
