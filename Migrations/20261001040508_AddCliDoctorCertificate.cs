using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCliDoctorCertificate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CliDoctorCertificate",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificateNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CertificateType = table.Column<int>(type: "integer", nullable: false),
                    CertificateStatus = table.Column<int>(type: "integer", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    QueueId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Diagnosis = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ClinicalSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AdditionalNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PatientNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    MedicalRecordNumberSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BirthDateSnapshot = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GenderSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AddressSnapshot = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OccupationSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DoctorNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DoctorSipSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ClinicNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DoctorSignatureDataUrl = table.Column<string>(type: "text", nullable: true),
                    SickStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SickEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SickDurationDays = table.Column<int>(type: "integer", nullable: true),
                    ActivityRestriction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ExaminationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HealthConclusion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BloodPressure = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Pulse = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Temperature = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Weight = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Height = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ColorBlindResult = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HealthRecommendation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReferralAdmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReferralDiagnosis = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReferralReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReferralTargetServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferralTargetClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferralTargetNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RequestedRoomClass = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SpecialInstruction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_CliDoctorCertificate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliDoctorCertificate_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliDoctorCertificate_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CliDoctorCertificate_CertificateNumber",
                schema: "public",
                table: "CliDoctorCertificate",
                column: "CertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CliDoctorCertificate_EncounterId",
                schema: "public",
                table: "CliDoctorCertificate",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_CliDoctorCertificate_PatientId_IssuedDate",
                schema: "public",
                table: "CliDoctorCertificate",
                columns: new[] { "PatientId", "IssuedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CliDoctorCertificate_QueueId",
                schema: "public",
                table: "CliDoctorCertificate",
                column: "QueueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CliDoctorCertificate",
                schema: "public");
        }
    }
}
