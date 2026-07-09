using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeTrxPatientIntegratedProgressNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrxPatientIntegratedProgressNote",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgressNoteNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    QueueId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    VitalSignId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProfessionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProfessionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProviderUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderDisplayNameSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ProviderRoleSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ServiceUnitNameSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LocationSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    SourceModule = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    SourceReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceReferenceNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    SubjectiveSummary = table.Column<string>(type: "text", nullable: true),
                    ObjectiveSummary = table.Column<string>(type: "text", nullable: true),
                    AssessmentSummary = table.Column<string>(type: "text", nullable: true),
                    PlanSummary = table.Column<string>(type: "text", nullable: true),
                    Instruction = table.Column<string>(type: "text", nullable: true),
                    Evaluation = table.Column<string>(type: "text", nullable: true),
                    NoteText = table.Column<string>(type: "text", nullable: true),
                    PrivateNote = table.Column<string>(type: "text", nullable: true),
                    IsGeneratedFromSource = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsReadOnlyGenerated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_TrxPatientIntegratedProgressNote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPatientIntegratedProgressNote_AspNetUsers_CancelledByUse~",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientIntegratedProgressNote_AspNetUsers_ProviderUserId",
                        column: x => x.ProviderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientIntegratedProgressNote_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientIntegratedProgressNote_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientIntegratedProgressNote_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientIntegratedProgressNote_MstServiceUnit_ServiceUnit~",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientIntegratedProgressNote_TrxDoctorConsultation_Cons~",
                        column: x => x.ConsultationId,
                        principalSchema: "public",
                        principalTable: "TrxDoctorConsultation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientIntegratedProgressNote_TrxPatientAssessment_Asses~",
                        column: x => x.AssessmentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientIntegratedProgressNote_TrxPatientEncounter_Encoun~",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientIntegratedProgressNote_TrxPatientVitalSign_VitalS~",
                        column: x => x.VitalSignId,
                        principalSchema: "public",
                        principalTable: "TrxPatientVitalSign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientIntegratedProgressNote_TrxQueue_QueueId",
                        column: x => x.QueueId,
                        principalSchema: "public",
                        principalTable: "TrxQueue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_AssessmentId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_CancelledByUserId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_ClinicId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_ConsultationId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_DoctorId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_EncounterId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_EncounterId_NoteDateTime_I~",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                columns: new[] { "EncounterId", "NoteDateTime", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_EncounterId_ProfessionType~",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                columns: new[] { "EncounterId", "ProfessionType", "NoteDateTime", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_IsActive_IsDelete_IsCancel",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                columns: new[] { "IsActive", "IsDelete", "IsCancel" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_PatientId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_PatientId_NoteDateTime_IsD~",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                columns: new[] { "PatientId", "NoteDateTime", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_PatientId_ProfessionType_N~",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                columns: new[] { "PatientId", "ProfessionType", "NoteDateTime", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_ProgressNoteNumber",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "ProgressNoteNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_ProviderUserId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "ProviderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_QueueId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_ServiceUnitId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_SourceModule_SourceReferen~",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                columns: new[] { "SourceModule", "SourceReferenceId", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_VitalSignId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "VitalSignId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxPatientIntegratedProgressNote",
                schema: "public");
        }
    }
}
