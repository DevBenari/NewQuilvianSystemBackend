using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261104113000_AddRecruitmentManagementCoreAndRetireLegacy")]
    public partial class AddRecruitmentManagementCoreAndRetireLegacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================================================================
            // 1. EMPTY_TABLE_GUARD & PEMBERSIHAN 20 TABEL LEGACY RECRUITMENT (HRD-DEC-074)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
DECLARE
    legacy_tables text[] := ARRAY[
        'MstAssessmentMethod', 'MstCandidateStatus', 'MstInterviewTemplate', 'MstRecruitmentSource', 'MstRecruitmentStage',
        'TrxBackgroundCheck', 'TrxCandidate', 'TrxCandidateApplication', 'TrxCandidateAssessment', 'TrxCandidateDocument',
        'TrxCandidateHiring', 'TrxCandidateInterview', 'TrxCandidateScreening', 'TrxInterviewEvaluation', 'TrxJobOffer',
        'TrxJobRequisition', 'TrxJobRequisitionApproval', 'TrxJobVacancy', 'TrxPreEmploymentMedicalCheck', 'TrxReferenceCheck'
    ];
    tbl text;
    cnt bigint;
BEGIN
    FOREACH tbl IN ARRAY legacy_tables
    LOOP
        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = tbl) THEN
            EXECUTE format('SELECT count(*) FROM public.%I', tbl) INTO cnt;
            IF cnt > 0 THEN
                RAISE EXCEPTION 'MIGRATION ABORTED: Tabel legacy % memiliki % baris data riil! Operasi dihentikan untuk mencegah kehilangan data.', tbl, cnt;
            END IF;
            EXECUTE format('DROP TABLE IF EXISTS public.%I CASCADE', tbl);
        END IF;
    END LOOP;
END $$;
");

            // =========================================================================
            // 2. PEMBUATAN 12 TABEL BARU HRD RECRUITMENT MANAGEMENT (HRD-DEC-073 s.d. 080)
            // =========================================================================

            // Tabel 1: HrdJobRequisition
            migrationBuilder.CreateTable(
                name: "HrdJobRequisition",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequisitionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HeadcountRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedSeats = table.Column<int>(type: "integer", nullable: false),
                    ApprovedSeats = table.Column<int>(type: "integer", nullable: false),
                    FulfilledSeats = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_HrdJobRequisition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdJobRequisition_HrdHeadcountRequest_HeadcountRequestId",
                        column: x => x.HeadcountRequestId,
                        principalSchema: "public",
                        principalTable: "HrdHeadcountRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdJobRequisition_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdJobRequisition_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdJobRequisition_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Tabel 2: HrdJobVacancy
            migrationBuilder.CreateTable(
                name: "HrdJobVacancy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VacancyNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequisitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    QuotaSeats = table.Column<int>(type: "integer", nullable: false),
                    IsClinical = table.Column<bool>(type: "boolean", nullable: false),
                    PublishDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_HrdJobVacancy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdJobVacancy_HrdJobRequisition_RequisitionId",
                        column: x => x.RequisitionId,
                        principalSchema: "public",
                        principalTable: "HrdJobRequisition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Tabel 3: HrdCandidate
            migrationBuilder.CreateTable(
                name: "HrdCandidate",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NationalIdNumber = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BirthPlace = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsRehire = table.Column<bool>(type: "boolean", nullable: false),
                    PreviousWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_HrdCandidate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdCandidate_MstWorkforceProfile_PreviousWorkforceProfileId",
                        column: x => x.PreviousWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Tabel 4: HrdCandidateApplication
            migrationBuilder.CreateTable(
                name: "HrdCandidateApplication",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    VacancyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentStage = table.Column<int>(type: "integer", nullable: false),
                    StageStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_HrdCandidateApplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdCandidateApplication_HrdCandidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "public",
                        principalTable: "HrdCandidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdCandidateApplication_HrdJobVacancy_VacancyId",
                        column: x => x.VacancyId,
                        principalSchema: "public",
                        principalTable: "HrdJobVacancy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Tabel 5: HrdCandidateDocument
            migrationBuilder.CreateTable(
                name: "HrdCandidateDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_HrdCandidateDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdCandidateDocument_HrdCandidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "public",
                        principalTable: "HrdCandidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdCandidateDocument_HrdCandidateApplication_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "public",
                        principalTable: "HrdCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Tabel 6: HrdCandidateScreening
            migrationBuilder.CreateTable(
                name: "HrdCandidateScreening",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScreeningDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsStrRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ClinicalScreeningResult = table.Column<int>(type: "integer", nullable: false),
                    StrNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StrExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EvaluatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdCandidateScreening", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdCandidateScreening_HrdCandidateApplication_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "public",
                        principalTable: "HrdCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Tabel 7: HrdCandidateEvaluation
            migrationBuilder.CreateTable(
                name: "HrdCandidateEvaluation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluationType = table.Column<int>(type: "integer", nullable: false),
                    EvaluatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatorName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EvaluatorRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EvaluationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Recommendation = table.Column<int>(type: "integer", nullable: false),
                    IsClinicalVetoApplied = table.Column<bool>(type: "boolean", nullable: false),
                    VetoReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_HrdCandidateEvaluation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdCandidateEvaluation_HrdCandidateApplication_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "public",
                        principalTable: "HrdCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Tabel 8: HrdJobOffer
            migrationBuilder.CreateTable(
                name: "HrdJobOffer",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaryGradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferedBasicSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MaximumGradeSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovalTier = table.Column<int>(type: "integer", nullable: false),
                    OfferedPositionTitle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ProposedJoinDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OfferDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CandidateResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CandidateResponseNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_HrdJobOffer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdJobOffer_HrdCandidateApplication_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "public",
                        principalTable: "HrdCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdJobOffer_MstSalaryGrade_SalaryGradeId",
                        column: x => x.SalaryGradeId,
                        principalSchema: "public",
                        principalTable: "MstSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Tabel 9: HrdJobOfferApproval
            migrationBuilder.CreateTable(
                name: "HrdJobOfferApproval",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepNumber = table.Column<int>(type: "integer", nullable: false),
                    ApproverRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApproverName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DecisionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_HrdJobOfferApproval", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdJobOfferApproval_HrdJobOffer_OfferId",
                        column: x => x.OfferId,
                        principalSchema: "public",
                        principalTable: "HrdJobOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Tabel 10: HrdPreEmploymentMedicalCheck
            migrationBuilder.CreateTable(
                name: "HrdPreEmploymentMedicalCheck",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferralNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReferralDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClinicOrFacility = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ClearanceResult = table.Column<int>(type: "integer", nullable: false),
                    DoctorNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CertificateFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExaminingDoctorName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
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
                    table.PrimaryKey("PK_HrdPreEmploymentMedicalCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdPreEmploymentMedicalCheck_HrdJobOffer_OfferId",
                        column: x => x.OfferId,
                        principalSchema: "public",
                        principalTable: "HrdJobOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Tabel 11: HrdTalentPool
            migrationBuilder.CreateTable(
                name: "HrdTalentPool",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalVacancyId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationSummary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RankingScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    RankingPosition = table.Column<int>(type: "integer", nullable: false),
                    EnrolledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RetentionExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PromotedToOfferId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_HrdTalentPool", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdTalentPool_HrdCandidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "public",
                        principalTable: "HrdCandidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdTalentPool_HrdJobVacancy_OriginalVacancyId",
                        column: x => x.OriginalVacancyId,
                        principalSchema: "public",
                        principalTable: "HrdJobVacancy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Tabel 12: HrdCandidateHiringHandoff
            migrationBuilder.CreateTable(
                name: "HrdCandidateHiringHandoff",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HandoffNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    HeadcountRequestSeatId = table.Column<Guid>(type: "uuid", nullable: true),
                    HandoffDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_HrdCandidateHiringHandoff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdCandidateHiringHandoff_HrdCandidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "public",
                        principalTable: "HrdCandidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdCandidateHiringHandoff_HrdHeadcountRequestSeat_HeadcountRequestSeatId",
                        column: x => x.HeadcountRequestSeatId,
                        principalSchema: "public",
                        principalTable: "HrdHeadcountRequestSeat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdCandidateHiringHandoff_HrdJobOffer_OfferId",
                        column: x => x.OfferId,
                        principalSchema: "public",
                        principalTable: "HrdJobOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdCandidateHiringHandoff_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // =========================================================================
            // 3. INDEKS UNIK DAN PENCARIAN EFISIEN
            // =========================================================================

            // HrdJobRequisition
            migrationBuilder.CreateIndex(
                name: "IX_HrdJobRequisition_HeadcountRequestId",
                schema: "public",
                table: "HrdJobRequisition",
                column: "HeadcountRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobRequisition_HospitalSiteId",
                schema: "public",
                table: "HrdJobRequisition",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobRequisition_OrganizationUnitId",
                schema: "public",
                table: "HrdJobRequisition",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobRequisition_PositionId",
                schema: "public",
                table: "HrdJobRequisition",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobRequisition_RequisitionNumber",
                schema: "public",
                table: "HrdJobRequisition",
                column: "RequisitionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobRequisition_Status",
                schema: "public",
                table: "HrdJobRequisition",
                column: "Status");

            // HrdJobVacancy
            migrationBuilder.CreateIndex(
                name: "IX_HrdJobVacancy_IsClinical",
                schema: "public",
                table: "HrdJobVacancy",
                column: "IsClinical");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobVacancy_RequisitionId",
                schema: "public",
                table: "HrdJobVacancy",
                column: "RequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobVacancy_Status",
                schema: "public",
                table: "HrdJobVacancy",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobVacancy_VacancyNumber",
                schema: "public",
                table: "HrdJobVacancy",
                column: "VacancyNumber",
                unique: true);

            // HrdCandidate
            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidate_CandidateCode",
                schema: "public",
                table: "HrdCandidate",
                column: "CandidateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidate_Email",
                schema: "public",
                table: "HrdCandidate",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidate_NationalIdNumber",
                schema: "public",
                table: "HrdCandidate",
                column: "NationalIdNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidate_PreviousWorkforceProfileId",
                schema: "public",
                table: "HrdCandidate",
                column: "PreviousWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidate_Status",
                schema: "public",
                table: "HrdCandidate",
                column: "Status");

            // HrdCandidateApplication
            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateApplication_ApplicationNumber",
                schema: "public",
                table: "HrdCandidateApplication",
                column: "ApplicationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateApplication_CandidateId_VacancyId",
                schema: "public",
                table: "HrdCandidateApplication",
                columns: new[] { "CandidateId", "VacancyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateApplication_CurrentStage",
                schema: "public",
                table: "HrdCandidateApplication",
                column: "CurrentStage");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateApplication_VacancyId",
                schema: "public",
                table: "HrdCandidateApplication",
                column: "VacancyId");

            // HrdCandidateDocument
            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateDocument_ApplicationId",
                schema: "public",
                table: "HrdCandidateDocument",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateDocument_CandidateId",
                schema: "public",
                table: "HrdCandidateDocument",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateDocument_DocumentType",
                schema: "public",
                table: "HrdCandidateDocument",
                column: "DocumentType");

            // HrdCandidateScreening
            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateScreening_ApplicationId",
                schema: "public",
                table: "HrdCandidateScreening",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateScreening_ClinicalScreeningResult",
                schema: "public",
                table: "HrdCandidateScreening",
                column: "ClinicalScreeningResult");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateScreening_Status",
                schema: "public",
                table: "HrdCandidateScreening",
                column: "Status");

            // HrdCandidateEvaluation
            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateEvaluation_ApplicationId",
                schema: "public",
                table: "HrdCandidateEvaluation",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateEvaluation_EvaluationType",
                schema: "public",
                table: "HrdCandidateEvaluation",
                column: "EvaluationType");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateEvaluation_EvaluatorUserId",
                schema: "public",
                table: "HrdCandidateEvaluation",
                column: "EvaluatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateEvaluation_IsClinicalVetoApplied",
                schema: "public",
                table: "HrdCandidateEvaluation",
                column: "IsClinicalVetoApplied");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateEvaluation_Recommendation",
                schema: "public",
                table: "HrdCandidateEvaluation",
                column: "Recommendation");

            // HrdJobOffer
            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOffer_ApplicationId",
                schema: "public",
                table: "HrdJobOffer",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOffer_ApprovalTier",
                schema: "public",
                table: "HrdJobOffer",
                column: "ApprovalTier");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOffer_OfferNumber",
                schema: "public",
                table: "HrdJobOffer",
                column: "OfferNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOffer_SalaryGradeId",
                schema: "public",
                table: "HrdJobOffer",
                column: "SalaryGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOffer_Status",
                schema: "public",
                table: "HrdJobOffer",
                column: "Status");

            // HrdJobOfferApproval
            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOfferApproval_ApproverUserId",
                schema: "public",
                table: "HrdJobOfferApproval",
                column: "ApproverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOfferApproval_OfferId",
                schema: "public",
                table: "HrdJobOfferApproval",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOfferApproval_Status",
                schema: "public",
                table: "HrdJobOfferApproval",
                column: "Status");

            // HrdPreEmploymentMedicalCheck
            migrationBuilder.CreateIndex(
                name: "IX_HrdPreEmploymentMedicalCheck_ClearanceResult",
                schema: "public",
                table: "HrdPreEmploymentMedicalCheck",
                column: "ClearanceResult");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPreEmploymentMedicalCheck_OfferId",
                schema: "public",
                table: "HrdPreEmploymentMedicalCheck",
                column: "OfferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdPreEmploymentMedicalCheck_ReferralNumber",
                schema: "public",
                table: "HrdPreEmploymentMedicalCheck",
                column: "ReferralNumber",
                unique: true);

            // HrdTalentPool
            migrationBuilder.CreateIndex(
                name: "IX_HrdTalentPool_CandidateId",
                schema: "public",
                table: "HrdTalentPool",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdTalentPool_OriginalVacancyId",
                schema: "public",
                table: "HrdTalentPool",
                column: "OriginalVacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdTalentPool_RetentionExpiryDate",
                schema: "public",
                table: "HrdTalentPool",
                column: "RetentionExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_HrdTalentPool_Status",
                schema: "public",
                table: "HrdTalentPool",
                column: "Status");

            // HrdCandidateHiringHandoff
            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateHiringHandoff_CandidateId",
                schema: "public",
                table: "HrdCandidateHiringHandoff",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateHiringHandoff_HandoffNumber",
                schema: "public",
                table: "HrdCandidateHiringHandoff",
                column: "HandoffNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateHiringHandoff_HeadcountRequestSeatId",
                schema: "public",
                table: "HrdCandidateHiringHandoff",
                column: "HeadcountRequestSeatId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateHiringHandoff_OfferId",
                schema: "public",
                table: "HrdCandidateHiringHandoff",
                column: "OfferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateHiringHandoff_Status",
                schema: "public",
                table: "HrdCandidateHiringHandoff",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateHiringHandoff_WorkforceProfileId",
                schema: "public",
                table: "HrdCandidateHiringHandoff",
                column: "WorkforceProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "HrdCandidateHiringHandoff", schema: "public");
            migrationBuilder.DropTable(name: "HrdTalentPool", schema: "public");
            migrationBuilder.DropTable(name: "HrdPreEmploymentMedicalCheck", schema: "public");
            migrationBuilder.DropTable(name: "HrdJobOfferApproval", schema: "public");
            migrationBuilder.DropTable(name: "HrdJobOffer", schema: "public");
            migrationBuilder.DropTable(name: "HrdCandidateEvaluation", schema: "public");
            migrationBuilder.DropTable(name: "HrdCandidateScreening", schema: "public");
            migrationBuilder.DropTable(name: "HrdCandidateDocument", schema: "public");
            migrationBuilder.DropTable(name: "HrdCandidateApplication", schema: "public");
            migrationBuilder.DropTable(name: "HrdCandidate", schema: "public");
            migrationBuilder.DropTable(name: "HrdJobVacancy", schema: "public");
            migrationBuilder.DropTable(name: "HrdJobRequisition", schema: "public");
        }
    }
}
