using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddNursingIntervention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CliNursingIntervention",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CarePlanItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    InterventionName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PerformedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultNote = table.Column<string>(type: "text", nullable: true),
                    RecordStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalizedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsBillable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    BillingDispatchStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BillingDispatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BillingDispatchAttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BillingDispatchFailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_CliNursingIntervention", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliNursingIntervention_CliNursingCarePlanItem_CarePlanItemId",
                        column: x => x.CarePlanItemId,
                        principalSchema: "public",
                        principalTable: "CliNursingCarePlanItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CliNursingIntervention_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliNursingIntervention_MstEmployee_PerformedByEmployeeId",
                        column: x => x.PerformedByEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliNursingIntervention_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliNursingIntervention_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingIntervention_BillingDispatchStatus",
                schema: "public",
                table: "CliNursingIntervention",
                column: "BillingDispatchStatus");

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingIntervention_CarePlanItemId",
                schema: "public",
                table: "CliNursingIntervention",
                column: "CarePlanItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingIntervention_EncounterId",
                schema: "public",
                table: "CliNursingIntervention",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingIntervention_IdempotencyKey",
                schema: "public",
                table: "CliNursingIntervention",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingIntervention_InpEpisodeId_PerformedAt",
                schema: "public",
                table: "CliNursingIntervention",
                columns: new[] { "InpEpisodeId", "PerformedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingIntervention_PatientId",
                schema: "public",
                table: "CliNursingIntervention",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingIntervention_PerformedByEmployeeId",
                schema: "public",
                table: "CliNursingIntervention",
                column: "PerformedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingIntervention_RecordStatus",
                schema: "public",
                table: "CliNursingIntervention",
                column: "RecordStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CliNursingIntervention",
                schema: "public");
        }
    }
}
