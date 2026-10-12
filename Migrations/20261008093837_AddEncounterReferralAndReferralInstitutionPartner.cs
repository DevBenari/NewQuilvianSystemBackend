using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEncounterReferralAndReferralInstitutionPartner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPartner",
                schema: "public",
                table: "MstReferralInstitution",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RegEncounterReferral",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientEncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferralDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TargetUnitType = table.Column<int>(type: "integer", nullable: false),
                    TargetServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiagnosisId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiagnosisNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReferralReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InstitutionIsPartnerSnapshot = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CaptureSource = table.Column<int>(type: "integer", nullable: false),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_RegEncounterReferral", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegEncounterReferral_MstClinic_TargetClinicId",
                        column: x => x.TargetClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegEncounterReferral_MstDiagnosis_DiagnosisId",
                        column: x => x.DiagnosisId,
                        principalSchema: "public",
                        principalTable: "MstDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegEncounterReferral_MstServiceUnit_TargetServiceUnitId",
                        column: x => x.TargetServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegEncounterReferral_RegPatientEncounter_PatientEncounterId",
                        column: x => x.PatientEncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegEncounterReferralDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterReferralId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PageOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_RegEncounterReferralDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegEncounterReferralDocument_RegEncounterReferral_Encounter~",
                        column: x => x.EncounterReferralId,
                        principalSchema: "public",
                        principalTable: "RegEncounterReferral",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegEncounterReferralRevision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterReferralId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionType = table.Column<int>(type: "integer", nullable: false),
                    OldValuesJson = table.Column<string>(type: "jsonb", nullable: true),
                    NewValuesJson = table.Column<string>(type: "jsonb", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_RegEncounterReferralRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegEncounterReferralRevision_RegEncounterReferral_Encounter~",
                        column: x => x.EncounterReferralId,
                        principalSchema: "public",
                        principalTable: "RegEncounterReferral",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegEncounterReferral_DiagnosisId",
                schema: "public",
                table: "RegEncounterReferral",
                column: "DiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_RegEncounterReferral_IsComplete",
                schema: "public",
                table: "RegEncounterReferral",
                column: "IsComplete");

            migrationBuilder.CreateIndex(
                name: "IX_RegEncounterReferral_PatientEncounterId",
                schema: "public",
                table: "RegEncounterReferral",
                column: "PatientEncounterId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RegEncounterReferral_TargetClinicId",
                schema: "public",
                table: "RegEncounterReferral",
                column: "TargetClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_RegEncounterReferral_TargetServiceUnitId",
                schema: "public",
                table: "RegEncounterReferral",
                column: "TargetServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_RegEncounterReferralDocument_EncounterReferralId_IsDelete",
                schema: "public",
                table: "RegEncounterReferralDocument",
                columns: new[] { "EncounterReferralId", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_RegEncounterReferralRevision_EncounterReferralId_ChangedAt",
                schema: "public",
                table: "RegEncounterReferralRevision",
                columns: new[] { "EncounterReferralId", "ChangedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegEncounterReferralDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RegEncounterReferralRevision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RegEncounterReferral",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "IsPartner",
                schema: "public",
                table: "MstReferralInstitution");
        }
    }
}
