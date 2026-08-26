using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddNosocomialInfection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrxNosocomialInfection",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NosocomialRecordNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmergencyVisitId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    InfectionType = table.Column<int>(type: "integer", nullable: false),
                    InfectionTypeOther = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OnsetCategory = table.Column<int>(type: "integer", nullable: false),
                    OnsetDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdmissionDateTimeSnapshot = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HoursSinceAdmission = table.Column<int>(type: "integer", nullable: true),
                    IsDeviceAssociated = table.Column<bool>(type: "boolean", nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DeviceInsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeviceUsageDays = table.Column<int>(type: "integer", nullable: true),
                    CriteriaMet = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CultureSpecimenType = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CultureTakenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CultureResult = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CausativeOrganism = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AntibioticTherapy = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportedByNameSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedByNameSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RuledOutReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_TrxNosocomialInfection", x => x.Id);
                    table.CheckConstraint("CK_TrxNosocomialInfection_DeviceUsageDays", "\"DeviceUsageDays\" IS NULL OR \"DeviceUsageDays\" >= 0");
                    table.CheckConstraint("CK_TrxNosocomialInfection_HoursSinceAdmission", "\"HoursSinceAdmission\" IS NULL OR \"HoursSinceAdmission\" >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxNosocomialInfection_EmergencyVisitId",
                schema: "public",
                table: "TrxNosocomialInfection",
                column: "EmergencyVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxNosocomialInfection_EncounterId",
                schema: "public",
                table: "TrxNosocomialInfection",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxNosocomialInfection_NosocomialRecordNumber",
                schema: "public",
                table: "TrxNosocomialInfection",
                column: "NosocomialRecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxNosocomialInfection_PatientId_OnsetDateTime",
                schema: "public",
                table: "TrxNosocomialInfection",
                columns: new[] { "PatientId", "OnsetDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxNosocomialInfection_ServiceUnitId_InfectionType_Status_O~",
                schema: "public",
                table: "TrxNosocomialInfection",
                columns: new[] { "ServiceUnitId", "InfectionType", "Status", "OnsetDateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxNosocomialInfection",
                schema: "public");
        }
    }
}
