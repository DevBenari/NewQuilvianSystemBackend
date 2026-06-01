using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeTransactionPatient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstIdentityScannerProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProfileName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ProfileType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ScannerVendorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ScannerModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InputFormat = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OutputFormat = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IdentityNumberRegex = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    MemberNumberRegex = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CardNumberRegex = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IdentityNumberFieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FullNameFieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BirthDateFieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GenderFieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AddressFieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsForIdentityCard = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsForPatientCard = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsForMembershipCard = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsForInsuranceCard = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsOcrEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsBarcodeEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsQrEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsManualInputAllowed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAutoCreatePatientAllowed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsVerificationRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ConfigurationJson = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstIdentityScannerProfile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstKioskDevice",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DeviceType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DeviceStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultScannerProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    LocationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FloorName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MacAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VendorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsAvailableForRegistration = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAvailableForCheckIn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAvailableForPayment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAllowWalkIn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAllowAppointment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAllowInsuranceRegistration = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LastOnlineAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastOfflineAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastErrorMessage = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstKioskDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstKioskDevice_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstKioskDevice_MstIdentityScannerProfile_DefaultScannerProf~",
                        column: x => x.DefaultScannerProfileId,
                        principalSchema: "public",
                        principalTable: "MstIdentityScannerProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstKioskDevice_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxKioskScanSession",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScanSource = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ScanStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    KioskDeviceId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdentityScannerProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdentityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IdentityNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CardNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MemberNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InsuranceCardNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    GenderText = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RawScanText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ParsedJson = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPatientFound = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsManualInput = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsUsedForRegistration = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_TrxKioskScanSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxKioskScanSession_MstIdentityScannerProfile_IdentityScann~",
                        column: x => x.IdentityScannerProfileId,
                        principalSchema: "public",
                        principalTable: "MstIdentityScannerProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxKioskScanSession_MstKioskDevice_KioskDeviceId",
                        column: x => x.KioskDeviceId,
                        principalSchema: "public",
                        principalTable: "MstKioskDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxKioskScanSession_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPatientEncounter",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorServiceRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentMethodId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientInsuranceId = table.Column<Guid>(type: "uuid", nullable: true),
                    InsuranceProviderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyGuarantorId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientCompanyGuarantorId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientMembershipId = table.Column<Guid>(type: "uuid", nullable: true),
                    KioskScanSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    EncounterDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EncounterType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    VisitType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RegistrationSource = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PaymentType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    EncounterStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ChiefComplaint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReferralNumber = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EligibilityReferenceNumber = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EligibilityCheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsNewPatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsFromKiosk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsWalkIn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAppointment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsReferral = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsScreeningRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsQueueRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDoctorRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    RegisteredByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckedInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NoShowAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NoShowByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoShowReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_TrxPatientEncounter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_AspNetUsers_NoShowByUserId",
                        column: x => x.NoShowByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_AspNetUsers_RegisteredByUserId",
                        column: x => x.RegisteredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_MstCompanyGuarantor_CompanyGuarantorId",
                        column: x => x.CompanyGuarantorId,
                        principalSchema: "public",
                        principalTable: "MstCompanyGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_MstDoctorSchedule_DoctorScheduleId",
                        column: x => x.DoctorScheduleId,
                        principalSchema: "public",
                        principalTable: "MstDoctorSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_MstDoctorServiceRule_DoctorServiceRuleId",
                        column: x => x.DoctorServiceRuleId,
                        principalSchema: "public",
                        principalTable: "MstDoctorServiceRule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_MstInsuranceProvider_InsuranceProviderId",
                        column: x => x.InsuranceProviderId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceProvider",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_MstPatientClass_PatientClassId",
                        column: x => x.PatientClassId,
                        principalSchema: "public",
                        principalTable: "MstPatientClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_MstPatientCompanyGuarantor_PatientCompa~",
                        column: x => x.PatientCompanyGuarantorId,
                        principalSchema: "public",
                        principalTable: "MstPatientCompanyGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_MstPatientInsurance_PatientInsuranceId",
                        column: x => x.PatientInsuranceId,
                        principalSchema: "public",
                        principalTable: "MstPatientInsurance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_MstPatientMembership_PatientMembershipId",
                        column: x => x.PatientMembershipId,
                        principalSchema: "public",
                        principalTable: "MstPatientMembership",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_MstPaymentMethod_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalSchema: "public",
                        principalTable: "MstPaymentMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounter_TrxKioskScanSession_KioskScanSessionId",
                        column: x => x.KioskScanSessionId,
                        principalSchema: "public",
                        principalTable: "TrxKioskScanSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxQueue",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    QueueDate = table.Column<DateTime>(type: "date", nullable: false),
                    QueueNumber = table.Column<int>(type: "integer", nullable: false),
                    QueueCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    QueueStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    NurseCallAttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastNurseCalledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastNurseCalledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    NurseCallExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DoctorCallAttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastDoctorCalledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastDoctorCalledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorCallExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScreeningStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScreeningCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsultationStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsultationCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SkipCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastSkippedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSkippedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SkipReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RequeueCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastRequeuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRequeuedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequeueReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    NoShowAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NoShowByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoShowReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPriorityQueue = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsFromKiosk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsWalkIn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAppointment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsScreeningRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDoctorRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_TrxQueue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxQueue_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxQueue_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxQueue_AspNetUsers_LastDoctorCalledByUserId",
                        column: x => x.LastDoctorCalledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxQueue_AspNetUsers_LastNurseCalledByUserId",
                        column: x => x.LastNurseCalledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxQueue_AspNetUsers_LastRequeuedByUserId",
                        column: x => x.LastRequeuedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxQueue_AspNetUsers_LastSkippedByUserId",
                        column: x => x.LastSkippedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxQueue_AspNetUsers_NoShowByUserId",
                        column: x => x.NoShowByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxQueue_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxQueue_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxQueue_MstDoctorSchedule_DoctorScheduleId",
                        column: x => x.DoctorScheduleId,
                        principalSchema: "public",
                        principalTable: "MstDoctorSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxQueue_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxQueue_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxQueue_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPatientAssessment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    QueueId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssessmentStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AssessmentByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChiefComplaint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CurrentIllnessHistory = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BloodPressureSystolic = table.Column<int>(type: "integer", nullable: true),
                    BloodPressureDiastolic = table.Column<int>(type: "integer", nullable: true),
                    PulseRate = table.Column<int>(type: "integer", nullable: true),
                    IsPulseReadable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RespiratoryRate = table.Column<int>(type: "integer", nullable: true),
                    Temperature = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    OxygenSaturation = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    IsUsingOxygen = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    OxygenSupportType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OxygenFlowRate = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    OxygenSupportNote = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConsciousnessStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Weight = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    Height = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    BMI = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    MeanArterialPressure = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    MapStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EarlyWarningScore = table.Column<int>(type: "integer", nullable: true),
                    EwsRiskLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EwsMonitoringRecommendation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    HasPain = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PainScale = table.Column<int>(type: "integer", nullable: true),
                    PainTrigger = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PainQuality = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PainLocation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PainFrequency = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PainManagement = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PainNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HasHereditaryDisease = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HereditaryDiseaseNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HasAllergy = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AllergyType = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AllergyNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AppetiteStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HasNausea = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HasVomiting = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    NutritionRiskStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NutritionRiskScore = table.Column<int>(type: "integer", nullable: true),
                    NutritionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HasFallRisk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HasAtaxia = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HasPosturalInstability = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FallRiskStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FallRiskScore = table.Column<int>(type: "integer", nullable: true),
                    FallRiskNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FunctionalStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FunctionalNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PsychosocialNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EducationNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NurseNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_TrxPatientAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPatientAssessment_AspNetUsers_AssessmentByUserId",
                        column: x => x.AssessmentByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAssessment_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAssessment_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAssessment_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAssessment_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAssessment_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAssessment_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAssessment_TrxQueue_QueueId",
                        column: x => x.QueueId,
                        principalSchema: "public",
                        principalTable: "TrxQueue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxDoctorConsultation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    QueueId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsultationDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsultationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsVitalSignCopiedFromAssessment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    BloodPressureSystolic = table.Column<int>(type: "integer", nullable: true),
                    BloodPressureDiastolic = table.Column<int>(type: "integer", nullable: true),
                    PulseRate = table.Column<int>(type: "integer", nullable: true),
                    RespiratoryRate = table.Column<int>(type: "integer", nullable: true),
                    Temperature = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    OxygenSaturation = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    Height = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    BMI = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    Subjective = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Objective = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Assessment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Plan = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DiagnosisText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PrimaryDiagnosisText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SecondaryDiagnosisText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DiagnosisCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HasPrimaryDiagnosis = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ProcedurePlan = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PrescriptionPlan = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SupportingExamPlan = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReferralPlan = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EducationPlan = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FollowUpDate = table.Column<DateTime>(type: "date", nullable: true),
                    FollowUpNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DoctorNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_TrxDoctorConsultation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxDoctorConsultation_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDoctorConsultation_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDoctorConsultation_AspNetUsers_StartedByUserId",
                        column: x => x.StartedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDoctorConsultation_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDoctorConsultation_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDoctorConsultation_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDoctorConsultation_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDoctorConsultation_TrxPatientAssessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDoctorConsultation_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDoctorConsultation_TrxQueue_QueueId",
                        column: x => x.QueueId,
                        principalSchema: "public",
                        principalTable: "TrxQueue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPatientDiagnosis",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiagnosisCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DiagnosisName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    DiagnosisType = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    DiagnosisStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsChronic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNewCase = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DiagnosisDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClinicalNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AssessmentNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PlanNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_TrxPatientDiagnosis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPatientDiagnosis_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientDiagnosis_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientDiagnosis_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientDiagnosis_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientDiagnosis_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientDiagnosis_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientDiagnosis_TrxDoctorConsultation_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "public",
                        principalTable: "TrxDoctorConsultation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientDiagnosis_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPatientProcedure",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: false),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    InsuranceTariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    InsuranceCoverageRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcedureCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProcedureNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProcedureTypeSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PatientTypeSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentTypeSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PatientClassNameSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InsuranceProviderNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BenefitPlanNameSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    InsuranceTariffCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InsuranceTariffNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ProcedureSource = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ProcedureStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ProcedureDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 1m),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    HospitalPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    InsuranceContractPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsFreeOfCharge = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FreeOfChargeReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsBillable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCoveredByInsurance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CoverageStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Unknown"),
                    CoveragePercent = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    CoveredAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    PatientPayAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    CoverageNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsExecuted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicalNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResultNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InstructionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DispositionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BillingItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsBillingGenerated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    BillingGeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_TrxPatientProcedure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPatientProcedure_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientProcedure_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientProcedure_AspNetUsers_ExecutedByUserId",
                        column: x => x.ExecutedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientProcedure_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientProcedure_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientProcedure_MstInsuranceCoverageRule_InsuranceCover~",
                        column: x => x.InsuranceCoverageRuleId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceCoverageRule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientProcedure_MstInsuranceTariff_InsuranceTariffId",
                        column: x => x.InsuranceTariffId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientProcedure_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientProcedure_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientProcedure_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientProcedure_MstTariff_TariffId",
                        column: x => x.TariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientProcedure_TrxDoctorConsultation_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "public",
                        principalTable: "TrxDoctorConsultation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientProcedure_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstIdentityScannerProfile_ProfileCode",
                schema: "public",
                table: "MstIdentityScannerProfile",
                column: "ProfileCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstIdentityScannerProfile_ProfileName",
                schema: "public",
                table: "MstIdentityScannerProfile",
                column: "ProfileName");

            migrationBuilder.CreateIndex(
                name: "IX_MstIdentityScannerProfile_ProfileType",
                schema: "public",
                table: "MstIdentityScannerProfile",
                column: "ProfileType");

            migrationBuilder.CreateIndex(
                name: "IX_MstIdentityScannerProfile_ProfileType_IsActive_IsDelete",
                schema: "public",
                table: "MstIdentityScannerProfile",
                columns: new[] { "ProfileType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_ClinicId",
                schema: "public",
                table: "MstKioskDevice",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_DefaultScannerProfileId",
                schema: "public",
                table: "MstKioskDevice",
                column: "DefaultScannerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_DeviceCode",
                schema: "public",
                table: "MstKioskDevice",
                column: "DeviceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_DeviceName",
                schema: "public",
                table: "MstKioskDevice",
                column: "DeviceName");

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_DeviceType_DeviceStatus_IsActive_IsDelete",
                schema: "public",
                table: "MstKioskDevice",
                columns: new[] { "DeviceType", "DeviceStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_IpAddress",
                schema: "public",
                table: "MstKioskDevice",
                column: "IpAddress",
                filter: "\"IpAddress\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_SerialNumber",
                schema: "public",
                table: "MstKioskDevice",
                column: "SerialNumber",
                filter: "\"SerialNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_ServiceUnitId",
                schema: "public",
                table: "MstKioskDevice",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_ServiceUnitId_ClinicId_IsAvailableForRegistr~",
                schema: "public",
                table: "MstKioskDevice",
                columns: new[] { "ServiceUnitId", "ClinicId", "IsAvailableForRegistration", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_AssessmentId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_CancelledByUserId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_ClinicId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_CompletedByUserId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_ConsultationNumber",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "ConsultationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_ConsultationStatus_IsActive_IsDelete",
                schema: "public",
                table: "TrxDoctorConsultation",
                columns: new[] { "ConsultationStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_DoctorId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_DoctorId_ConsultationDateTime_Consult~",
                schema: "public",
                table: "TrxDoctorConsultation",
                columns: new[] { "DoctorId", "ConsultationDateTime", "ConsultationStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_EncounterId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "EncounterId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_EncounterId_ConsultationStatus_IsDele~",
                schema: "public",
                table: "TrxDoctorConsultation",
                columns: new[] { "EncounterId", "ConsultationStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_HasPrimaryDiagnosis_IsDelete",
                schema: "public",
                table: "TrxDoctorConsultation",
                columns: new[] { "HasPrimaryDiagnosis", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_PatientId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_PatientId_ConsultationDateTime_IsDele~",
                schema: "public",
                table: "TrxDoctorConsultation",
                columns: new[] { "PatientId", "ConsultationDateTime", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_QueueId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "QueueId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_ServiceUnitId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_ServiceUnitId_ClinicId_ConsultationDa~",
                schema: "public",
                table: "TrxDoctorConsultation",
                columns: new[] { "ServiceUnitId", "ClinicId", "ConsultationDateTime", "ConsultationStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_StartedByUserId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "StartedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxKioskScanSession_CardNumber",
                schema: "public",
                table: "TrxKioskScanSession",
                column: "CardNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TrxKioskScanSession_IdentityNumber",
                schema: "public",
                table: "TrxKioskScanSession",
                column: "IdentityNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TrxKioskScanSession_IdentityScannerProfileId",
                schema: "public",
                table: "TrxKioskScanSession",
                column: "IdentityScannerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxKioskScanSession_KioskDeviceId",
                schema: "public",
                table: "TrxKioskScanSession",
                column: "KioskDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxKioskScanSession_MemberNumber",
                schema: "public",
                table: "TrxKioskScanSession",
                column: "MemberNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TrxKioskScanSession_PatientId",
                schema: "public",
                table: "TrxKioskScanSession",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxKioskScanSession_PatientId_IsUsedForRegistration_IsDelete",
                schema: "public",
                table: "TrxKioskScanSession",
                columns: new[] { "PatientId", "IsUsedForRegistration", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxKioskScanSession_ScanStatus_ScanSource_IsDelete",
                schema: "public",
                table: "TrxKioskScanSession",
                columns: new[] { "ScanStatus", "ScanSource", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxKioskScanSession_SessionCode",
                schema: "public",
                table: "TrxKioskScanSession",
                column: "SessionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxKioskScanSession_StartedAt_ScanStatus_IsDelete",
                schema: "public",
                table: "TrxKioskScanSession",
                columns: new[] { "StartedAt", "ScanStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_AssessmentByUserId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "AssessmentByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_AssessmentNumber",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "AssessmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_AssessmentStatus_IsActive_IsDelete",
                schema: "public",
                table: "TrxPatientAssessment",
                columns: new[] { "AssessmentStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_CancelledByUserId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_ClinicId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_CompletedByUserId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_DoctorId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_EncounterId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "EncounterId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_EncounterId_AssessmentStatus_IsDelete",
                schema: "public",
                table: "TrxPatientAssessment",
                columns: new[] { "EncounterId", "AssessmentStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_EwsRiskLevel_IsDelete",
                schema: "public",
                table: "TrxPatientAssessment",
                columns: new[] { "EwsRiskLevel", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_FallRiskStatus_IsDelete",
                schema: "public",
                table: "TrxPatientAssessment",
                columns: new[] { "FallRiskStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_PatientId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_PatientId_AssessmentDateTime_IsDelete",
                schema: "public",
                table: "TrxPatientAssessment",
                columns: new[] { "PatientId", "AssessmentDateTime", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_QueueId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "QueueId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_ServiceUnitId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_ServiceUnitId_ClinicId_AssessmentDateT~",
                schema: "public",
                table: "TrxPatientAssessment",
                columns: new[] { "ServiceUnitId", "ClinicId", "AssessmentDateTime", "AssessmentStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_CancelledByUserId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_ClinicId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_ConsultationId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_ConsultationId_DiagnosisCode_IsDelete",
                schema: "public",
                table: "TrxPatientDiagnosis",
                columns: new[] { "ConsultationId", "DiagnosisCode", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_ConsultationId_IsPrimary_IsDelete",
                schema: "public",
                table: "TrxPatientDiagnosis",
                columns: new[] { "ConsultationId", "IsPrimary", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_DiagnosisType_DiagnosisStatus_IsActive_~",
                schema: "public",
                table: "TrxPatientDiagnosis",
                columns: new[] { "DiagnosisType", "DiagnosisStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_DoctorId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_DoctorId_DiagnosisDateTime_IsDelete",
                schema: "public",
                table: "TrxPatientDiagnosis",
                columns: new[] { "DoctorId", "DiagnosisDateTime", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_EncounterId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_EncounterId_SortOrder_IsDelete",
                schema: "public",
                table: "TrxPatientDiagnosis",
                columns: new[] { "EncounterId", "SortOrder", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_PatientId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_PatientId_DiagnosisCode_DiagnosisStatus~",
                schema: "public",
                table: "TrxPatientDiagnosis",
                columns: new[] { "PatientId", "DiagnosisCode", "DiagnosisStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_ResolvedByUserId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_ServiceUnitId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_CancelledByUserId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_ClinicId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "CompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_DoctorId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_DoctorScheduleId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "DoctorScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_DoctorServiceRuleId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "DoctorServiceRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_EncounterNumber",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "EncounterNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_EncounterStatus_EncounterType_IsActive_~",
                schema: "public",
                table: "TrxPatientEncounter",
                columns: new[] { "EncounterStatus", "EncounterType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_InsuranceProviderId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "InsuranceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_KioskScanSessionId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "KioskScanSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_NoShowByUserId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "NoShowByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_PatientClassId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "PatientClassId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_PatientCompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "PatientCompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_PatientId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_PatientId_EncounterDate_IsDelete",
                schema: "public",
                table: "TrxPatientEncounter",
                columns: new[] { "PatientId", "EncounterDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_PatientInsuranceId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "PatientInsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_PatientMembershipId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "PatientMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_PaymentMethodId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_PaymentType_PaymentMethodId_IsDelete",
                schema: "public",
                table: "TrxPatientEncounter",
                columns: new[] { "PaymentType", "PaymentMethodId", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_RegisteredAt_EncounterStatus_IsDelete",
                schema: "public",
                table: "TrxPatientEncounter",
                columns: new[] { "RegisteredAt", "EncounterStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_RegisteredByUserId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "RegisteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_ServiceUnitId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_ServiceUnitId_ClinicId_DoctorId_Encount~",
                schema: "public",
                table: "TrxPatientEncounter",
                columns: new[] { "ServiceUnitId", "ClinicId", "DoctorId", "EncounterDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ApprovedByUserId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_CancelledByUserId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ClinicId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ConsultationId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ConsultationId_ProcedureId_IsDelete",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "ConsultationId", "ProcedureId", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_CoverageStatus_IsCoveredByInsurance_IsD~",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "CoverageStatus", "IsCoveredByInsurance", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_DoctorId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_DoctorId_ProcedureDateTime_IsDelete",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "DoctorId", "ProcedureDateTime", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_EncounterId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_EncounterId_ProcedureStatus_IsDelete",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "EncounterId", "ProcedureStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ExecutedByUserId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "ExecutedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_InsuranceCoverageRuleId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "InsuranceCoverageRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_InsuranceTariffId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "InsuranceTariffId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_IsFreeOfCharge_IsBillable_IsDelete",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "IsFreeOfCharge", "IsBillable", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_IsNeedApproval_IsApproved_IsDelete",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "IsNeedApproval", "IsApproved", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_PatientId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_PatientId_ProcedureDateTime_IsDelete",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "PatientId", "ProcedureDateTime", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ProcedureId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ProcedureStatus_IsBillable_IsBillingGen~",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "ProcedureStatus", "IsBillable", "IsBillingGenerated", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ServiceUnitId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ServiceUnitId_ClinicId_ProcedureDateTim~",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "ServiceUnitId", "ClinicId", "ProcedureDateTime", "ProcedureStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_TariffId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "TariffId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_CancelledByUserId",
                schema: "public",
                table: "TrxQueue",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_ClinicId",
                schema: "public",
                table: "TrxQueue",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_CompletedByUserId",
                schema: "public",
                table: "TrxQueue",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_DoctorId",
                schema: "public",
                table: "TrxQueue",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_DoctorId_QueueDate_QueueStatus_IsDelete",
                schema: "public",
                table: "TrxQueue",
                columns: new[] { "DoctorId", "QueueDate", "QueueStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_DoctorScheduleId",
                schema: "public",
                table: "TrxQueue",
                column: "DoctorScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_EncounterId",
                schema: "public",
                table: "TrxQueue",
                column: "EncounterId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_LastDoctorCalledByUserId",
                schema: "public",
                table: "TrxQueue",
                column: "LastDoctorCalledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_LastNurseCalledByUserId",
                schema: "public",
                table: "TrxQueue",
                column: "LastNurseCalledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_LastRequeuedByUserId",
                schema: "public",
                table: "TrxQueue",
                column: "LastRequeuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_LastSkippedByUserId",
                schema: "public",
                table: "TrxQueue",
                column: "LastSkippedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_NoShowByUserId",
                schema: "public",
                table: "TrxQueue",
                column: "NoShowByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_PatientId",
                schema: "public",
                table: "TrxQueue",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_PatientId_QueueDate_IsDelete",
                schema: "public",
                table: "TrxQueue",
                columns: new[] { "PatientId", "QueueDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_QueueCode",
                schema: "public",
                table: "TrxQueue",
                column: "QueueCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_QueueDate_QueueStatus_IsActive_IsDelete",
                schema: "public",
                table: "TrxQueue",
                columns: new[] { "QueueDate", "QueueStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_QueueDate_ServiceUnitId_ClinicId_DoctorId_QueueNum~",
                schema: "public",
                table: "TrxQueue",
                columns: new[] { "QueueDate", "ServiceUnitId", "ClinicId", "DoctorId", "QueueNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_QueueDate_ServiceUnitId_ClinicId_DoctorId_QueueSta~",
                schema: "public",
                table: "TrxQueue",
                columns: new[] { "QueueDate", "ServiceUnitId", "ClinicId", "DoctorId", "QueueStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_QueueStatus_IsScreeningRequired_IsDoctorRequired_I~",
                schema: "public",
                table: "TrxQueue",
                columns: new[] { "QueueStatus", "IsScreeningRequired", "IsDoctorRequired", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_ServiceUnitId",
                schema: "public",
                table: "TrxQueue",
                column: "ServiceUnitId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxPatientDiagnosis",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPatientProcedure",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxDoctorConsultation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPatientAssessment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxQueue",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPatientEncounter",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxKioskScanSession",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstKioskDevice",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstIdentityScannerProfile",
                schema: "public");
        }
    }
}
