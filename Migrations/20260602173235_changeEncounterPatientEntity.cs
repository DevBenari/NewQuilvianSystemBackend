using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeEncounterPatientEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounter_MstCompanyGuarantor_CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounter_MstInsuranceProvider_InsuranceProviderId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounter_MstPatientCompanyGuarantor_PatientCompa~",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounter_MstPatientInsurance_PatientInsuranceId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounter_MstPatientMembership_PatientMembershipId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_InsuranceProviderId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_PatientCompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_PatientInsuranceId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_PatientMembershipId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "InsuranceProviderId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "PatientCompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "PatientInsuranceId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "PatientMembershipId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompanyPatient",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEligibilityCompleted",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEligibilityRequired",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInsurancePatient",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMembershipPatient",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMixedPayment",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReferralRequired",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReferralVerified",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryGuarantorNameSnapshot",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryGuarantorTypeSnapshot",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrxPatientClinicalDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicalDocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    QueueId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientDiagnosisId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentSource = table.Column<int>(type: "integer", nullable: false),
                    DocumentStatus = table.Column<int>(type: "integer", nullable: false),
                    ConfidentialityLevel = table.Column<int>(type: "integer", nullable: false),
                    DocumentTitle = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    DocumentCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentCategoryName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ExternalDocumentNumber = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ExternalProviderName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ExternalDoctorName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DocumentDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UploadedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    FileExtension = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MimeType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    FileHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StorageProvider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ThumbnailPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PreviewPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PageCount = table.Column<int>(type: "integer", nullable: true),
                    DocumentSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ClinicalFindingSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Impression = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Recommendation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Keywords = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsConfidential = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPatientVisible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsShareable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsExternalDocument = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPartOfMedicalRecord = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsLegalDocument = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedReview = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsReviewed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArchivedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_TrxPatientClinicalDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_AspNetUsers_ArchivedByUserId",
                        column: x => x.ArchivedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_TrxDoctorConsultation_Consultati~",
                        column: x => x.ConsultationId,
                        principalSchema: "public",
                        principalTable: "TrxDoctorConsultation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_TrxPatientAssessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_TrxPatientDiagnosis_PatientDiagn~",
                        column: x => x.PatientDiagnosisId,
                        principalSchema: "public",
                        principalTable: "TrxPatientDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_TrxPatientProcedure_PatientProce~",
                        column: x => x.PatientProcedureId,
                        principalSchema: "public",
                        principalTable: "TrxPatientProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientClinicalDocument_TrxQueue_QueueId",
                        column: x => x.QueueId,
                        principalSchema: "public",
                        principalTable: "TrxQueue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPatientEncounterGuarantor",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterGuarantorNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuarantorType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GuarantorRole = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GuarantorStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CheckMethod = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CoveragePriority = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PaymentMethodId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientInsuranceId = table.Column<Guid>(type: "uuid", nullable: true),
                    InsuranceProviderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyGuarantorId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientCompanyGuarantorId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientMembershipId = table.Column<Guid>(type: "uuid", nullable: true),
                    GuarantorNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PolicyNumberSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CardNumberSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MemberNumberSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PlanNameSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ClassNameSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    BenefitPlanCodeSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EffectiveStartDateSnapshot = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveEndDateSnapshot = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsEligibilityRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsEligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EligibilityReferenceNumber = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EligibilityCheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationReferenceNumber = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    VerificationOfficerName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    VerificationNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedGuaranteeLetter = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedReferralLetter = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAllowExcessPaymentByPatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CoveragePercent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    AnnualLimitAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RemainingLimitAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    UsedLimitAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RoomLimitPerDayAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DeductibleAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CoPaymentPercent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CoPaymentAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EstimatedCoveredAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EstimatedPatientPayAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IsPolicyActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPremiumPaid = table.Column<bool>(type: "boolean", nullable: true),
                    IsCardActive = table.Column<bool>(type: "boolean", nullable: true),
                    IsInWaitingPeriod = table.Column<bool>(type: "boolean", nullable: true),
                    WaitingPeriodUntilDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HasSpecialExclusion = table.Column<bool>(type: "boolean", nullable: true),
                    SpecialExclusionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HasPreviousClaim = table.Column<bool>(type: "boolean", nullable: true),
                    PreviousClaimNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BenefitSnapshotJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ManualCheckResultJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_TrxPatientEncounterGuarantor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounterGuarantor_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounterGuarantor_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounterGuarantor_MstCompanyGuarantor_CompanyGua~",
                        column: x => x.CompanyGuarantorId,
                        principalSchema: "public",
                        principalTable: "MstCompanyGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounterGuarantor_MstInsuranceProvider_Insurance~",
                        column: x => x.InsuranceProviderId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceProvider",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounterGuarantor_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounterGuarantor_MstPatientCompanyGuarantor_Pat~",
                        column: x => x.PatientCompanyGuarantorId,
                        principalSchema: "public",
                        principalTable: "MstPatientCompanyGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounterGuarantor_MstPatientInsurance_PatientIns~",
                        column: x => x.PatientInsuranceId,
                        principalSchema: "public",
                        principalTable: "MstPatientInsurance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounterGuarantor_MstPatientMembership_PatientMe~",
                        column: x => x.PatientMembershipId,
                        principalSchema: "public",
                        principalTable: "MstPatientMembership",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounterGuarantor_MstPaymentMethod_PaymentMethod~",
                        column: x => x.PaymentMethodId,
                        principalSchema: "public",
                        principalTable: "MstPaymentMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientEncounterGuarantor_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPatientVitalSign",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VitalSignRecordNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    QueueId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObservationDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VitalSignSource = table.Column<int>(type: "integer", nullable: false),
                    VitalSignStatus = table.Column<int>(type: "integer", nullable: false),
                    ObservedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObservationLocation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PatientPosition = table.Column<int>(type: "integer", nullable: false),
                    MeasurementMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceSerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BloodPressureSystolic = table.Column<int>(type: "integer", nullable: true),
                    BloodPressureDiastolic = table.Column<int>(type: "integer", nullable: true),
                    MeanArterialPressure = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    MapStatus = table.Column<int>(type: "integer", nullable: false),
                    BloodPressureLocation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PulseRate = table.Column<int>(type: "integer", nullable: true),
                    IsPulseReadable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsPulseRegular = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PulseRhythmNote = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RespiratoryRate = table.Column<int>(type: "integer", nullable: true),
                    Temperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    TemperatureRoute = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OxygenSaturation = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    IsUsingOxygen = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    OxygenSupportType = table.Column<int>(type: "integer", nullable: false),
                    OxygenFlowRate = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    OxygenSupportNote = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    Height = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    BMI = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    WeightMeasurementNote = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConsciousnessStatus = table.Column<int>(type: "integer", nullable: false),
                    GcsEye = table.Column<int>(type: "integer", nullable: true),
                    GcsVerbal = table.Column<int>(type: "integer", nullable: true),
                    GcsMotor = table.Column<int>(type: "integer", nullable: true),
                    GcsTotal = table.Column<int>(type: "integer", nullable: true),
                    NeurologicalNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    HasPain = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PainScale = table.Column<int>(type: "integer", nullable: true),
                    PainLocation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PainNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EarlyWarningScore = table.Column<int>(type: "integer", nullable: true),
                    EwsRiskLevel = table.Column<int>(type: "integer", nullable: false),
                    EwsMonitoringRecommendation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsAbnormal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCritical = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    NeedDoctorNotification = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DoctorNotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DoctorNotifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorNotificationNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicalNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_TrxPatientVitalSign", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPatientVitalSign_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientVitalSign_AspNetUsers_DoctorNotifiedByUserId",
                        column: x => x.DoctorNotifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientVitalSign_AspNetUsers_ObservedByUserId",
                        column: x => x.ObservedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientVitalSign_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientVitalSign_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientVitalSign_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientVitalSign_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientVitalSign_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientVitalSign_TrxDoctorConsultation_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "public",
                        principalTable: "TrxDoctorConsultation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientVitalSign_TrxPatientAssessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientVitalSign_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientVitalSign_TrxQueue_QueueId",
                        column: x => x.QueueId,
                        principalSchema: "public",
                        principalTable: "TrxQueue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxClinicalNoteAttachment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    QueueId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientDiagnosisId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicalDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttachmentType = table.Column<int>(type: "integer", nullable: false),
                    AttachmentContext = table.Column<int>(type: "integer", nullable: false),
                    AttachmentStatus = table.Column<int>(type: "integer", nullable: false),
                    ConfidentialityLevel = table.Column<int>(type: "integer", nullable: false),
                    AttachmentTitle = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    AttachmentCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AttachmentCategoryName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AttachmentDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NoteSectionName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ClinicalNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FindingNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InterpretationNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FollowUpNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BodySite = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    BodySide = table.Column<int>(type: "integer", nullable: false),
                    BodySiteDescription = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ViewPosition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CapturedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CaptureDeviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    FileExtension = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MimeType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    FileHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StorageProvider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ThumbnailPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PreviewPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageWidth = table.Column<int>(type: "integer", nullable: true),
                    ImageHeight = table.Column<int>(type: "integer", nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    IsConfidential = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPatientVisible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsShareable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPartOfMedicalRecord = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsClinicalMedia = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsBeforeAfterComparison = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RelatedAttachmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsNeedReview = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsReviewed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArchivedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_TrxClinicalNoteAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_AspNetUsers_ArchivedByUserId",
                        column: x => x.ArchivedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_AspNetUsers_CapturedByUserId",
                        column: x => x.CapturedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_TrxClinicalNoteAttachment_Related~",
                        column: x => x.RelatedAttachmentId,
                        principalSchema: "public",
                        principalTable: "TrxClinicalNoteAttachment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_TrxDoctorConsultation_Consultatio~",
                        column: x => x.ConsultationId,
                        principalSchema: "public",
                        principalTable: "TrxDoctorConsultation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_TrxPatientAssessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_TrxPatientClinicalDocument_Clinic~",
                        column: x => x.ClinicalDocumentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientClinicalDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_TrxPatientDiagnosis_PatientDiagno~",
                        column: x => x.PatientDiagnosisId,
                        principalSchema: "public",
                        principalTable: "TrxPatientDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_TrxPatientProcedure_PatientProced~",
                        column: x => x.PatientProcedureId,
                        principalSchema: "public",
                        principalTable: "TrxPatientProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAttachment_TrxQueue_QueueId",
                        column: x => x.QueueId,
                        principalSchema: "public",
                        principalTable: "TrxQueue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxMedicalCertificate",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicalCertificateNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    QueueId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientDiagnosisId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiagnosisId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicalDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    CertificateType = table.Column<int>(type: "integer", nullable: false),
                    CertificateStatus = table.Column<int>(type: "integer", nullable: false),
                    RecipientType = table.Column<int>(type: "integer", nullable: false),
                    DeliveryMethod = table.Column<int>(type: "integer", nullable: false),
                    CertificateTitle = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CertificateCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CertificateCategoryName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CertificatePurpose = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DiagnosisCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DiagnosisNameSnapshot = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DiagnosisMasterType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Manual"),
                    IcdVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ClinicalSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MedicalRecommendation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CertificateStatement = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AdditionalStatement = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RestrictionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FollowUpInstruction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SickLeaveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SickLeaveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SickLeaveDays = table.Column<int>(type: "integer", nullable: true),
                    SickLeaveReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ControlDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ControlClinicName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ReferralDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReferralToProviderName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ReferralToDepartmentName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ReferralReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AdmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DischargeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeathDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CauseOfDeath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FitnessStatus = table.Column<int>(type: "integer", nullable: false),
                    FitnessAssessmentNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WorkRestrictionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequestedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequestedByRelationship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RecipientInstitutionName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RecipientAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CertificateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuePlace = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CertificateFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CertificateFileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CertificateMimeType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CertificateFileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    CertificateFileHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    QrCodePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VerificationCode = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    VerificationUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsIssued = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsRejected = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_TrxMedicalCertificate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_AspNetUsers_IssuedByUserId",
                        column: x => x.IssuedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_AspNetUsers_RejectedByUserId",
                        column: x => x.RejectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_AspNetUsers_RevokedByUserId",
                        column: x => x.RevokedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_MstDiagnosis_DiagnosisId",
                        column: x => x.DiagnosisId,
                        principalSchema: "public",
                        principalTable: "MstDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_MstDoctor_IssuedByDoctorId",
                        column: x => x.IssuedByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_TrxDoctorConsultation_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "public",
                        principalTable: "TrxDoctorConsultation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_TrxPatientAssessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_TrxPatientClinicalDocument_ClinicalDo~",
                        column: x => x.ClinicalDocumentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientClinicalDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_TrxPatientDiagnosis_PatientDiagnosisId",
                        column: x => x.PatientDiagnosisId,
                        principalSchema: "public",
                        principalTable: "TrxPatientDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalCertificate_TrxQueue_QueueId",
                        column: x => x.QueueId,
                        principalSchema: "public",
                        principalTable: "TrxQueue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPatientConsent",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    QueueId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicalDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsentType = table.Column<int>(type: "integer", nullable: false),
                    ConsentStatus = table.Column<int>(type: "integer", nullable: false),
                    ConsentMethod = table.Column<int>(type: "integer", nullable: false),
                    ConsentTitle = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ConsentCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConsentCategoryName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ConsentDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ProcedureCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProcedureNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ProcedureTypeSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PlannedProcedureDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcedureLocation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DiagnosisExplanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProcedureExplanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    BenefitExplanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RiskExplanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AlternativeExplanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ConsequenceExplanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PatientQuestionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsDiagnosisExplained = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsProcedureExplained = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRiskExplained = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAlternativeExplained = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPatientUnderstood = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPatientAgreed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsEmergencyConsent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsHighRiskConsent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsLegalDocument = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsPartOfMedicalRecord = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SignerType = table.Column<int>(type: "integer", nullable: false),
                    SignerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SignerRelationship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SignerIdentityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SignerIdentityNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SignerPhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SignerAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsSignerPatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsSignerLegalRepresentative = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExplainedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExplainedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExplainedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WitnessName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WitnessRelationship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WitnessByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PatientSignaturePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SignerSignaturePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DoctorSignaturePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WitnessSignaturePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ConsentFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ConsentFileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ConsentMimeType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ConsentFileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    ConsentFileHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConsentDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsRejected = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsWithdrawn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    WithdrawnAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WithdrawnByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    WithdrawalReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_TrxPatientConsent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_AspNetUsers_ExplainedByUserId",
                        column: x => x.ExplainedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_AspNetUsers_RejectedByUserId",
                        column: x => x.RejectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_AspNetUsers_WithdrawnByUserId",
                        column: x => x.WithdrawnByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_AspNetUsers_WitnessByUserId",
                        column: x => x.WitnessByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_MstDoctor_ExplainedByDoctorId",
                        column: x => x.ExplainedByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_TrxDoctorConsultation_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "public",
                        principalTable: "TrxDoctorConsultation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_TrxPatientAssessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_TrxPatientClinicalDocument_ClinicalDocume~",
                        column: x => x.ClinicalDocumentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientClinicalDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_TrxPatientProcedure_PatientProcedureId",
                        column: x => x.PatientProcedureId,
                        principalSchema: "public",
                        principalTable: "TrxPatientProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientConsent_TrxQueue_QueueId",
                        column: x => x.QueueId,
                        principalSchema: "public",
                        principalTable: "TrxQueue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_IsEligibilityRequired_IsEligibilityComp~",
                schema: "public",
                table: "TrxPatientEncounter",
                columns: new[] { "IsEligibilityRequired", "IsEligibilityCompleted", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_IsInsurancePatient_IsCompanyPatient_IsM~",
                schema: "public",
                table: "TrxPatientEncounter",
                columns: new[] { "IsInsurancePatient", "IsCompanyPatient", "IsMixedPayment", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_ArchivedByUserId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "ArchivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_AssessmentId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_AttachmentNumber",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "AttachmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_CancelledByUserId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_CapturedByUserId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "CapturedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_ClinicalDocumentId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "ClinicalDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_ClinicId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_ConsultationId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_ConsultationId_UploadedAt",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                columns: new[] { "ConsultationId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_DoctorId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_EncounterId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_EncounterId_UploadedAt",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                columns: new[] { "EncounterId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_FileHash",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_IsNeedReview_IsReviewed",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                columns: new[] { "IsNeedReview", "IsReviewed" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_IsVerified_IsArchived",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                columns: new[] { "IsVerified", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_PatientDiagnosisId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "PatientDiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_PatientDiagnosisId_UploadedAt",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                columns: new[] { "PatientDiagnosisId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_PatientId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_PatientId_AttachmentContext_Uploa~",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                columns: new[] { "PatientId", "AttachmentContext", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_PatientId_AttachmentType_Attachme~",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                columns: new[] { "PatientId", "AttachmentType", "AttachmentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_PatientId_IsConfidential_Confiden~",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                columns: new[] { "PatientId", "IsConfidential", "ConfidentialityLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_PatientId_IsPartOfMedicalRecord_I~",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                columns: new[] { "PatientId", "IsPartOfMedicalRecord", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_PatientProcedureId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "PatientProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_PatientProcedureId_UploadedAt",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                columns: new[] { "PatientProcedureId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_QueueId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_RelatedAttachmentId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "RelatedAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_ReviewedByUserId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_ServiceUnitId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_UploadedByUserId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAttachment_VerifiedByUserId",
                schema: "public",
                table: "TrxClinicalNoteAttachment",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_ApprovedByUserId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_AssessmentId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_CancelledByUserId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_CertificateFileHash",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "CertificateFileHash");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_CertificateType_IssuedAt",
                schema: "public",
                table: "TrxMedicalCertificate",
                columns: new[] { "CertificateType", "IssuedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_ClinicalDocumentId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "ClinicalDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_ClinicId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_ConsultationId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_ConsultationId_CertificateDateTime",
                schema: "public",
                table: "TrxMedicalCertificate",
                columns: new[] { "ConsultationId", "CertificateDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_DiagnosisId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "DiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_DoctorId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_EncounterId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_EncounterId_CertificateDateTime",
                schema: "public",
                table: "TrxMedicalCertificate",
                columns: new[] { "EncounterId", "CertificateDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_ExpiredDate_IsActive",
                schema: "public",
                table: "TrxMedicalCertificate",
                columns: new[] { "ExpiredDate", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_IsIssued_IsVerified_IsApproved",
                schema: "public",
                table: "TrxMedicalCertificate",
                columns: new[] { "IsIssued", "IsVerified", "IsApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_IsRejected_IsRevoked_IsActive",
                schema: "public",
                table: "TrxMedicalCertificate",
                columns: new[] { "IsRejected", "IsRevoked", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_IssuedByDoctorId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "IssuedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_IssuedByUserId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "IssuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_MedicalCertificateNumber",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "MedicalCertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_PatientDiagnosisId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "PatientDiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_PatientId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_PatientId_CertificateDateTime",
                schema: "public",
                table: "TrxMedicalCertificate",
                columns: new[] { "PatientId", "CertificateDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_PatientId_CertificateType_Certificate~",
                schema: "public",
                table: "TrxMedicalCertificate",
                columns: new[] { "PatientId", "CertificateType", "CertificateStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_QueueId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_RejectedByUserId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_RevokedByUserId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "RevokedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_ServiceUnitId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_VerificationCode",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "VerificationCode");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalCertificate_VerifiedByUserId",
                schema: "public",
                table: "TrxMedicalCertificate",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_ApprovedByUserId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_ArchivedByUserId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "ArchivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_AssessmentId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_CancelledByUserId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_ClinicalDocumentNumber",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "ClinicalDocumentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_ClinicId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_ConsultationId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_ConsultationId_DocumentDateTime",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                columns: new[] { "ConsultationId", "DocumentDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_DoctorId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_EncounterId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_EncounterId_DocumentDateTime",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                columns: new[] { "EncounterId", "DocumentDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_FileHash",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_IsArchived_IsActive",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                columns: new[] { "IsArchived", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_IsNeedReview_IsReviewed",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                columns: new[] { "IsNeedReview", "IsReviewed" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_IsVerified_IsApproved",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                columns: new[] { "IsVerified", "IsApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_PatientDiagnosisId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "PatientDiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_PatientId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_PatientId_DocumentDateTime",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                columns: new[] { "PatientId", "DocumentDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_PatientId_DocumentType_DocumentS~",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                columns: new[] { "PatientId", "DocumentType", "DocumentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_PatientId_IsConfidential_Confide~",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                columns: new[] { "PatientId", "IsConfidential", "ConfidentialityLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_PatientId_IsPartOfMedicalRecord_~",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                columns: new[] { "PatientId", "IsPartOfMedicalRecord", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_PatientProcedureId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "PatientProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_QueueId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_ReviewedByUserId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_ServiceUnitId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_UploadedByUserId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientClinicalDocument_VerifiedByUserId",
                schema: "public",
                table: "TrxPatientClinicalDocument",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_ApprovedByUserId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_AssessmentId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_CancelledByUserId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_ClinicalDocumentId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "ClinicalDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_ClinicId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_ConsentFileHash",
                schema: "public",
                table: "TrxPatientConsent",
                column: "ConsentFileHash");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_ConsentNumber",
                schema: "public",
                table: "TrxPatientConsent",
                column: "ConsentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_ConsultationId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_ConsultationId_ConsentDateTime",
                schema: "public",
                table: "TrxPatientConsent",
                columns: new[] { "ConsultationId", "ConsentDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_DoctorId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_EncounterId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_EncounterId_ConsentDateTime",
                schema: "public",
                table: "TrxPatientConsent",
                columns: new[] { "EncounterId", "ConsentDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_ExpiredDate_IsActive",
                schema: "public",
                table: "TrxPatientConsent",
                columns: new[] { "ExpiredDate", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_ExplainedByDoctorId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "ExplainedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_ExplainedByUserId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "ExplainedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_IsVerified_IsApproved_IsRejected",
                schema: "public",
                table: "TrxPatientConsent",
                columns: new[] { "IsVerified", "IsApproved", "IsRejected" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_IsWithdrawn_IsActive",
                schema: "public",
                table: "TrxPatientConsent",
                columns: new[] { "IsWithdrawn", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_PatientId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_PatientId_ConsentDateTime",
                schema: "public",
                table: "TrxPatientConsent",
                columns: new[] { "PatientId", "ConsentDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_PatientId_ConsentType_ConsentStatus",
                schema: "public",
                table: "TrxPatientConsent",
                columns: new[] { "PatientId", "ConsentType", "ConsentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_PatientProcedureId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "PatientProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_PatientProcedureId_ConsentStatus",
                schema: "public",
                table: "TrxPatientConsent",
                columns: new[] { "PatientProcedureId", "ConsentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_QueueId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_RejectedByUserId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_ServiceUnitId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_VerifiedByUserId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_WithdrawnByUserId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "WithdrawnByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientConsent_WitnessByUserId",
                schema: "public",
                table: "TrxPatientConsent",
                column: "WitnessByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_CancelledByUserId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "CompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_EligibilityReferenceNumber_IsD~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "EligibilityReferenceNumber", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterGuarantorNumber",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "EncounterGuarantorNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId_CoveragePriority_I~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "EncounterId", "CoveragePriority", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId_IsPrimary_IsActive~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "EncounterId", "IsPrimary", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_GuarantorType_GuarantorStatus_~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "GuarantorType", "GuarantorStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_InsuranceProviderId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "InsuranceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_IsEligibilityRequired_IsEligib~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "IsEligibilityRequired", "IsEligible", "GuarantorStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_IsNeedApproval_IsNeedGuarantee~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "IsNeedApproval", "IsNeedGuaranteeLetter", "IsNeedReferralLetter" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientCompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "PatientCompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientId_GuarantorType_IsDele~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "PatientId", "GuarantorType", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientInsuranceId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "PatientInsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientMembershipId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "PatientMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_PaymentMethodId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_PolicyNumberSnapshot_CardNumbe~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "PolicyNumberSnapshot", "CardNumberSnapshot", "MemberNumberSnapshot" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_VerifiedByUserId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_AssessmentId",
                schema: "public",
                table: "TrxPatientVitalSign",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_CancelledByUserId",
                schema: "public",
                table: "TrxPatientVitalSign",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_ClinicId",
                schema: "public",
                table: "TrxPatientVitalSign",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_ConsultationId",
                schema: "public",
                table: "TrxPatientVitalSign",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_ConsultationId_ObservationDateTime",
                schema: "public",
                table: "TrxPatientVitalSign",
                columns: new[] { "ConsultationId", "ObservationDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_DoctorId",
                schema: "public",
                table: "TrxPatientVitalSign",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_DoctorNotifiedByUserId",
                schema: "public",
                table: "TrxPatientVitalSign",
                column: "DoctorNotifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_EncounterId",
                schema: "public",
                table: "TrxPatientVitalSign",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_EncounterId_ObservationDateTime",
                schema: "public",
                table: "TrxPatientVitalSign",
                columns: new[] { "EncounterId", "ObservationDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_NeedDoctorNotification_DoctorNotifiedAt",
                schema: "public",
                table: "TrxPatientVitalSign",
                columns: new[] { "NeedDoctorNotification", "DoctorNotifiedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_ObservedByUserId",
                schema: "public",
                table: "TrxPatientVitalSign",
                column: "ObservedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_PatientId",
                schema: "public",
                table: "TrxPatientVitalSign",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_PatientId_EwsRiskLevel",
                schema: "public",
                table: "TrxPatientVitalSign",
                columns: new[] { "PatientId", "EwsRiskLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_PatientId_IsAbnormal_IsCritical",
                schema: "public",
                table: "TrxPatientVitalSign",
                columns: new[] { "PatientId", "IsAbnormal", "IsCritical" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_PatientId_ObservationDateTime",
                schema: "public",
                table: "TrxPatientVitalSign",
                columns: new[] { "PatientId", "ObservationDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_PatientId_VitalSignStatus_IsActive",
                schema: "public",
                table: "TrxPatientVitalSign",
                columns: new[] { "PatientId", "VitalSignStatus", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_QueueId",
                schema: "public",
                table: "TrxPatientVitalSign",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_ServiceUnitId",
                schema: "public",
                table: "TrxPatientVitalSign",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_VerifiedByUserId",
                schema: "public",
                table: "TrxPatientVitalSign",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_VitalSignRecordNumber",
                schema: "public",
                table: "TrxPatientVitalSign",
                column: "VitalSignRecordNumber",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxClinicalNoteAttachment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxMedicalCertificate",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPatientConsent",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPatientEncounterGuarantor",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPatientVitalSign",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPatientClinicalDocument",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_IsEligibilityRequired_IsEligibilityComp~",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_IsInsurancePatient_IsCompanyPatient_IsM~",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsCompanyPatient",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsEligibilityCompleted",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsEligibilityRequired",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsInsurancePatient",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsMembershipPatient",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsMixedPayment",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsReferralRequired",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "IsReferralVerified",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "PrimaryGuarantorNameSnapshot",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "PrimaryGuarantorTypeSnapshot",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InsuranceProviderId",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PatientCompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PatientInsuranceId",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PatientMembershipId",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "CompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_InsuranceProviderId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "InsuranceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_PatientCompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "PatientCompanyGuarantorId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounter_MstCompanyGuarantor_CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "CompanyGuarantorId",
                principalSchema: "public",
                principalTable: "MstCompanyGuarantor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounter_MstInsuranceProvider_InsuranceProviderId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "InsuranceProviderId",
                principalSchema: "public",
                principalTable: "MstInsuranceProvider",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounter_MstPatientCompanyGuarantor_PatientCompa~",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "PatientCompanyGuarantorId",
                principalSchema: "public",
                principalTable: "MstPatientCompanyGuarantor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounter_MstPatientInsurance_PatientInsuranceId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "PatientInsuranceId",
                principalSchema: "public",
                principalTable: "MstPatientInsurance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounter_MstPatientMembership_PatientMembershipId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "PatientMembershipId",
                principalSchema: "public",
                principalTable: "MstPatientMembership",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
