using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRegOnlineRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegOnlineRegistration",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PatientMode = table.Column<int>(type: "integer", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdentityNumber = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: true),
                    VisitDate = table.Column<DateTime>(type: "date", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientType = table.Column<int>(type: "integer", nullable: false),
                    PaymentType = table.Column<int>(type: "integer", nullable: false),
                    PatientInsuranceId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientCompanyGuarantorId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewInsuranceProviderId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewInsurancePolicyNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NewInsuranceHolderName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    NewInsuranceHolderRelationship = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsReferral = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReferralInstitutionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferralNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReferralDate = table.Column<DateTime>(type: "date", nullable: true),
                    ReferralDoctorName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ReferralDiagnosisNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReferralReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HasConsent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ConsentSignerType = table.Column<int>(type: "integer", nullable: true),
                    ConsentSignerName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ConsentSignerRelationshipText = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConsentSignerIdentityNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConsentSignerPhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ConsentSignerAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ConsentAgreedClauses = table.Column<int[]>(type: "integer[]", nullable: false, defaultValueSql: "'{}'::integer[]"),
                    ConsentSignatureImageBase64 = table.Column<string>(type: "text", nullable: true),
                    ConsentSignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContactWhatsAppNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    EmailDeliveryStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EmailAttemptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckInTokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CheckInTokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckedInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_RegOnlineRegistration", x => x.Id);
                    table.CheckConstraint("CK_RegOnlineRegistration_Contact", "\"ContactWhatsAppNumber\" IS NOT NULL OR \"ContactEmail\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_RegOnlineRegistration_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegOnlineRegistration_MstDoctorSchedule_DoctorScheduleId",
                        column: x => x.DoctorScheduleId,
                        principalSchema: "public",
                        principalTable: "MstDoctorSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegOnlineRegistration_MstInsuranceProvider_NewInsuranceProv~",
                        column: x => x.NewInsuranceProviderId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceProvider",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegOnlineRegistration_MstPatientCompanyGuarantor_PatientCom~",
                        column: x => x.PatientCompanyGuarantorId,
                        principalSchema: "public",
                        principalTable: "MstPatientCompanyGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegOnlineRegistration_MstPatientInsurance_PatientInsuranceId",
                        column: x => x.PatientInsuranceId,
                        principalSchema: "public",
                        principalTable: "MstPatientInsurance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegOnlineRegistration_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegOnlineRegistration_MstReferralInstitution_ReferralInstit~",
                        column: x => x.ReferralInstitutionId,
                        principalSchema: "public",
                        principalTable: "MstReferralInstitution",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegOnlineRegistration_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegOnlineRegistrationHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnlineRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    FromVisitDate = table.Column<DateTime>(type: "date", nullable: true),
                    ToVisitDate = table.Column<DateTime>(type: "date", nullable: true),
                    FromDoctorScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToDoctorScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    FromContact = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ToContact = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_RegOnlineRegistrationHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegOnlineRegistrationHistory_RegOnlineRegistration_OnlineRe~",
                        column: x => x.OnlineRegistrationId,
                        principalSchema: "public",
                        principalTable: "RegOnlineRegistration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegOnlineRegistrationReferralDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnlineRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PageOrder = table.Column<int>(type: "integer", nullable: false),
                    EncounterReferralDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_RegOnlineRegistrationReferralDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegOnlineRegistrationReferralDocument_RegEncounterReferralD~",
                        column: x => x.EncounterReferralDocumentId,
                        principalSchema: "public",
                        principalTable: "RegEncounterReferralDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegOnlineRegistrationReferralDocument_RegOnlineRegistration~",
                        column: x => x.OnlineRegistrationId,
                        principalSchema: "public",
                        principalTable: "RegOnlineRegistration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistration_ClinicId",
                schema: "public",
                table: "RegOnlineRegistration",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistration_CodeHash_Awaiting",
                schema: "public",
                table: "RegOnlineRegistration",
                column: "CodeHash",
                unique: true,
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistration_DoctorScheduleId_VisitDate_Status",
                schema: "public",
                table: "RegOnlineRegistration",
                columns: new[] { "DoctorScheduleId", "VisitDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistration_EncounterId",
                schema: "public",
                table: "RegOnlineRegistration",
                column: "EncounterId",
                unique: true,
                filter: "\"EncounterId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistration_IdentityNumber_VisitDate_Status",
                schema: "public",
                table: "RegOnlineRegistration",
                columns: new[] { "IdentityNumber", "VisitDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistration_NewInsuranceProviderId",
                schema: "public",
                table: "RegOnlineRegistration",
                column: "NewInsuranceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistration_PatientCompanyGuarantorId",
                schema: "public",
                table: "RegOnlineRegistration",
                column: "PatientCompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistration_PatientId_VisitDate_Status",
                schema: "public",
                table: "RegOnlineRegistration",
                columns: new[] { "PatientId", "VisitDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistration_PatientInsuranceId",
                schema: "public",
                table: "RegOnlineRegistration",
                column: "PatientInsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistration_ReferralInstitutionId",
                schema: "public",
                table: "RegOnlineRegistration",
                column: "ReferralInstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistration_RegistrationNumber",
                schema: "public",
                table: "RegOnlineRegistration",
                column: "RegistrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistration_VisitDate_Status",
                schema: "public",
                table: "RegOnlineRegistration",
                columns: new[] { "VisitDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistrationHistory_OnlineRegistrationId_OccurredAt",
                schema: "public",
                table: "RegOnlineRegistrationHistory",
                columns: new[] { "OnlineRegistrationId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistrationReferralDocument_EncounterReferralDocu~",
                schema: "public",
                table: "RegOnlineRegistrationReferralDocument",
                column: "EncounterReferralDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_RegOnlineRegistrationReferralDocument_OnlineRegistrationId",
                schema: "public",
                table: "RegOnlineRegistrationReferralDocument",
                column: "OnlineRegistrationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegOnlineRegistrationHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RegOnlineRegistrationReferralDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RegOnlineRegistration",
                schema: "public");
        }
    }
}
