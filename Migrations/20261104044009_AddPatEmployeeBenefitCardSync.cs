using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPatEmployeeBenefitCardSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatEmployeeBenefitCardSync",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeBenefitEnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    SyncStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientCompanyGuarantorId = table.Column<Guid>(type: "uuid", nullable: true),
                    SyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastTriggerSource = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LastTriggeredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_PatEmployeeBenefitCardSync", x => x.Id);
                    table.CheckConstraint("CK_PatEmployeeBenefitCardSync_AttemptCount", "\"AttemptCount\" >= 0");
                    table.CheckConstraint("CK_PatEmployeeBenefitCardSync_CardLink", "(\"SyncStatus\" IN ('SYNCED','REVOKED')) = (\"PatientCompanyGuarantorId\" IS NOT NULL AND \"PatientId\" IS NOT NULL)");
                    table.CheckConstraint("CK_PatEmployeeBenefitCardSync_Reason", "(\"SyncStatus\" = 'SYNCED') = (\"ReasonCode\" IS NULL)");
                    table.CheckConstraint("CK_PatEmployeeBenefitCardSync_Status", "\"SyncStatus\" IN ('SYNCED','WAITING_PATIENT','ON_HOLD','FAILED_RETRYABLE','REVOKED')");
                    table.CheckConstraint("CK_PatEmployeeBenefitCardSync_TriggerSource", "\"LastTriggerSource\" IN ('ACTIVATION','SCHEDULER','MANUAL_RESYNC','CANCEL')");
                    table.ForeignKey(
                        name: "FK_PatEmployeeBenefitCardSync_MstPatientCompanyGuarantor_Patie~",
                        column: x => x.PatientCompanyGuarantorId,
                        principalSchema: "public",
                        principalTable: "MstPatientCompanyGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatEmployeeBenefitCardSync_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatEmployeeBenefitCardSync_MstWorkforceProfile_WorkforcePro~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatEmployeeBenefitCardSync_TrxEmployeeBenefitEnrollment_Emp~",
                        column: x => x.EmployeeBenefitEnrollmentId,
                        principalSchema: "public",
                        principalTable: "TrxEmployeeBenefitEnrollment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatEmployeeBenefitCardSync_Due",
                schema: "public",
                table: "PatEmployeeBenefitCardSync",
                columns: new[] { "SyncStatus", "NextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PatEmployeeBenefitCardSync_Patient",
                schema: "public",
                table: "PatEmployeeBenefitCardSync",
                column: "PatientId",
                filter: "\"PatientId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PatEmployeeBenefitCardSync_WorkforceProfile",
                schema: "public",
                table: "PatEmployeeBenefitCardSync",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "UX_PatEmployeeBenefitCardSync_Card",
                schema: "public",
                table: "PatEmployeeBenefitCardSync",
                column: "PatientCompanyGuarantorId",
                unique: true,
                filter: "\"PatientCompanyGuarantorId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "UX_PatEmployeeBenefitCardSync_Enrollment",
                schema: "public",
                table: "PatEmployeeBenefitCardSync",
                column: "EmployeeBenefitEnrollmentId",
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatEmployeeBenefitCardSync",
                schema: "public");
        }
    }
}
