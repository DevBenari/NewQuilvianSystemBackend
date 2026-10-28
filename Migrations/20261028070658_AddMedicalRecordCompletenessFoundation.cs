using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalRecordCompletenessFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddendumPurpose",
                schema: "public",
                table: "MrcClinicalNoteAddendum",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "MrcEncounterClinicalClosure",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicalClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosureSource = table.Column<int>(type: "integer", nullable: false),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MrcEncounterClinicalClosure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MrcEncounterClinicalClosure_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MrcEncounterClinicalClosure_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstMedicalRecordCompletenessProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EncounterType = table.Column<int>(type: "integer", nullable: true),
                    ServiceUnitType = table.Column<int>(type: "integer", nullable: true),
                    Condition = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_MstMedicalRecordCompletenessProfile", x => x.Id);
                    table.CheckConstraint("CK_MstMedicalRecordCompletenessProfile_EncounterTypeWajib", "\"Condition\" <> 0 OR \"EncounterType\" IS NOT NULL");
                });

            migrationBuilder.CreateTable(
                name: "MrcEncounterCompletenessAssessment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicalClosureId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    DeficiencyCount = table.Column<int>(type: "integer", nullable: false),
                    Deficiencies = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    EvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProfileFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FirstCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextCheckAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsExcluded = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_MrcEncounterCompletenessAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MrcEncounterCompletenessAssessment_MrcEncounterClinicalClos~",
                        column: x => x.ClinicalClosureId,
                        principalSchema: "public",
                        principalTable: "MrcEncounterClinicalClosure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MrcEncounterCompletenessAssessment_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MrcEncounterCompletenessAssessment_RegPatientEncounter_Enco~",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstMedicalRecordCompletenessProfileItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Criterion = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_MstMedicalRecordCompletenessProfileItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstMedicalRecordCompletenessProfileItem_MstMedicalRecordCom~",
                        column: x => x.ProfileId,
                        principalSchema: "public",
                        principalTable: "MstMedicalRecordCompletenessProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MrcClinicalNoteAddendum_IntegrityId_Attestation",
                schema: "public",
                table: "MrcClinicalNoteAddendum",
                column: "IntegrityId",
                unique: true,
                filter: "\"AddendumPurpose\" = 2 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MrcEncounterClinicalClosure_EncounterId",
                schema: "public",
                table: "MrcEncounterClinicalClosure",
                column: "EncounterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MrcEncounterClinicalClosure_PatientId",
                schema: "public",
                table: "MrcEncounterClinicalClosure",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MrcEncounterCompletenessAssessment_ClinicalClosureId",
                schema: "public",
                table: "MrcEncounterCompletenessAssessment",
                column: "ClinicalClosureId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MrcEncounterCompletenessAssessment_EncounterId",
                schema: "public",
                table: "MrcEncounterCompletenessAssessment",
                column: "EncounterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MrcEncounterCompletenessAssessment_PatientId",
                schema: "public",
                table: "MrcEncounterCompletenessAssessment",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MrcEncounterCompletenessAssessment_State_EvaluatedAt",
                schema: "public",
                table: "MrcEncounterCompletenessAssessment",
                columns: new[] { "State", "EvaluatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MrcEncounterCompletenessAssessment_State_NextCheckAt",
                schema: "public",
                table: "MrcEncounterCompletenessAssessment",
                columns: new[] { "State", "NextCheckAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MstMedicalRecordCompletenessProfile_Code",
                schema: "public",
                table: "MstMedicalRecordCompletenessProfile",
                column: "Code",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstMedicalRecordCompletenessProfile_EncounterType_ServiceUn~",
                schema: "public",
                table: "MstMedicalRecordCompletenessProfile",
                columns: new[] { "EncounterType", "ServiceUnitType", "Condition" },
                unique: true,
                filter: "\"IsDelete\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_MstMedicalRecordCompletenessProfileItem_ProfileId_Criterion",
                schema: "public",
                table: "MstMedicalRecordCompletenessProfileItem",
                columns: new[] { "ProfileId", "Criterion" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MrcEncounterCompletenessAssessment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstMedicalRecordCompletenessProfileItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MrcEncounterClinicalClosure",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstMedicalRecordCompletenessProfile",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MrcClinicalNoteAddendum_IntegrityId_Attestation",
                schema: "public",
                table: "MrcClinicalNoteAddendum");

            migrationBuilder.DropColumn(
                name: "AddendumPurpose",
                schema: "public",
                table: "MrcClinicalNoteAddendum");
        }
    }
}
