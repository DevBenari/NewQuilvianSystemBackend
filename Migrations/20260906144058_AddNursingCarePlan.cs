using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddNursingCarePlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CliNursingCarePlan",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OpenedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_CliNursingCarePlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliNursingCarePlan_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliNursingCarePlan_MstEmployee_OpenedByEmployeeId",
                        column: x => x.OpenedByEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliNursingCarePlan_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliNursingCarePlan_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliNursingCarePlanItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CarePlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    NursingDiagnosisId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceAssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProblemStatement = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    GoalStatement = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PlannedIntervention = table.Column<string>(type: "text", nullable: true),
                    EvaluationNote = table.Column<string>(type: "text", nullable: true),
                    LastEvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ItemStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CloseReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    AuthoredByEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_CliNursingCarePlanItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliNursingCarePlanItem_CliNursingCarePlan_CarePlanId",
                        column: x => x.CarePlanId,
                        principalSchema: "public",
                        principalTable: "CliNursingCarePlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CliNursingCarePlanItem_MstEmployee_AuthoredByEmployeeId",
                        column: x => x.AuthoredByEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliNursingCarePlanItem_TrxPatientAssessment_SourceAssessmen~",
                        column: x => x.SourceAssessmentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingCarePlan_EncounterId",
                schema: "public",
                table: "CliNursingCarePlan",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingCarePlan_InpEpisodeId",
                schema: "public",
                table: "CliNursingCarePlan",
                column: "InpEpisodeId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingCarePlan_OpenedByEmployeeId",
                schema: "public",
                table: "CliNursingCarePlan",
                column: "OpenedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingCarePlan_PatientId",
                schema: "public",
                table: "CliNursingCarePlan",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingCarePlanItem_AuthoredByEmployeeId",
                schema: "public",
                table: "CliNursingCarePlanItem",
                column: "AuthoredByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingCarePlanItem_CarePlanId",
                schema: "public",
                table: "CliNursingCarePlanItem",
                column: "CarePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingCarePlanItem_ItemStatus",
                schema: "public",
                table: "CliNursingCarePlanItem",
                column: "ItemStatus");

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingCarePlanItem_SourceAssessmentId",
                schema: "public",
                table: "CliNursingCarePlanItem",
                column: "SourceAssessmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CliNursingCarePlanItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliNursingCarePlan",
                schema: "public");
        }
    }
}
