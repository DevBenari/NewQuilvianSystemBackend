using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalMilestoneFactHandoff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrxClinicalMilestoneFact",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceContext = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceAggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    EffectType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MilestoneFactId = table.Column<Guid>(type: "uuid", nullable: false),
                    MilestoneFactVersion = table.Column<int>(type: "integer", nullable: false),
                    MilestoneKind = table.Column<int>(type: "integer", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TariffSnapshot = table.Column<string>(type: "jsonb", nullable: true),
                    RuleSnapshot = table.Column<string>(type: "jsonb", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PayloadFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DispatchStatus = table.Column<int>(type: "integer", nullable: false),
                    DispatchAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    DispatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BillingProcessingEffectId = table.Column<Guid>(type: "uuid", nullable: true),
                    BillingFolioId = table.Column<Guid>(type: "uuid", nullable: true),
                    BillingChargeLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    BillingOutcomeCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BillingOutcomeMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxClinicalMilestoneFact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxClinicalMilestoneFact_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalMilestoneFact_DispatchStatus",
                schema: "public",
                table: "TrxClinicalMilestoneFact",
                column: "DispatchStatus");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalMilestoneFact_EncounterId",
                schema: "public",
                table: "TrxClinicalMilestoneFact",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalMilestoneFact_IdempotencyKey",
                schema: "public",
                table: "TrxClinicalMilestoneFact",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalMilestoneFact_SourceContext_MilestoneFactId_Mile~",
                schema: "public",
                table: "TrxClinicalMilestoneFact",
                columns: new[] { "SourceContext", "MilestoneFactId", "MilestoneFactVersion", "EffectType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalMilestoneFact_SourceContext_SourceAggregateId_So~",
                schema: "public",
                table: "TrxClinicalMilestoneFact",
                columns: new[] { "SourceContext", "SourceAggregateId", "SourceItemId", "EffectType" },
                filter: "\"IsDelete\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxClinicalMilestoneFact",
                schema: "public");
        }
    }
}
