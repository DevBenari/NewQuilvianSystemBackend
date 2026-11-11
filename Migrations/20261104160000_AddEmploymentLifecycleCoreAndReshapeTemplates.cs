using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261104160000_AddEmploymentLifecycleCoreAndReshapeTemplates")]
    public partial class AddEmploymentLifecycleCoreAndReshapeTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================================================================
            // 1. EMPTY_TABLE_GUARD & PEMBERSIHAN 6 TABEL LEGACY LIFECYCLE (HRD-DEC-096)
            // =========================================================================
            migrationBuilder.Sql(@"
DO $$
DECLARE
    legacy_tables text[] := ARRAY[
        'TrxProbationReview', 'TrxEmployeeOnboardingTask', 'TrxEmployeeOnboarding',
        'WfpOnboardingTask', 'WfpOnboardingChecklist', 'TrxContractNonRenewal'
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
            // 2. ALTER TABEL EXISTING
            // =========================================================================

            // 2.1 WfpContractHistory - penambahan kolom baru
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'WfpContractHistory' AND column_name = 'SourceDocumentType') THEN
        ALTER TABLE public.""WfpContractHistory"" ADD COLUMN ""SourceDocumentType"" integer NOT NULL DEFAULT 1;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'WfpContractHistory' AND column_name = 'SourceDocumentId') THEN
        ALTER TABLE public.""WfpContractHistory"" ADD COLUMN ""SourceDocumentId"" uuid NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'WfpContractHistory' AND column_name = 'FixedTermSeriesStartDate') THEN
        ALTER TABLE public.""WfpContractHistory"" ADD COLUMN ""FixedTermSeriesStartDate"" date NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'WfpContractHistory' AND column_name = 'DocumentFileName') THEN
        ALTER TABLE public.""WfpContractHistory"" ADD COLUMN ""DocumentFileName"" character varying(250) NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'WfpContractHistory' AND column_name = 'DocumentContentType') THEN
        ALTER TABLE public.""WfpContractHistory"" ADD COLUMN ""DocumentContentType"" character varying(150) NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'WfpContractHistory' AND column_name = 'SignedRecordedAt') THEN
        ALTER TABLE public.""WfpContractHistory"" ADD COLUMN ""SignedRecordedAt"" timestamp with time zone NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'WfpContractHistory' AND column_name = 'SignedRecordedByUserId') THEN
        ALTER TABLE public.""WfpContractHistory"" ADD COLUMN ""SignedRecordedByUserId"" uuid NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'WfpContractHistory' AND column_name = 'RowVersion') THEN
        ALTER TABLE public.""WfpContractHistory"" ADD COLUMN ""RowVersion"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid;
    END IF;
END $$;
");

            // 2.2 MstContractType - penambahan EmploymentTypeId
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'MstContractType' AND column_name = 'EmploymentTypeId') THEN
        ALTER TABLE public.""MstContractType"" ADD COLUMN ""EmploymentTypeId"" uuid NULL;
    END IF;
END $$;
");

            // 2.3 HrdJobOffer - penambahan ContractTypeId & ContractDurationMonths (BE-HRD-064)
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'HrdJobOffer' AND column_name = 'ContractTypeId') THEN
        ALTER TABLE public.""HrdJobOffer"" ADD COLUMN ""ContractTypeId"" uuid NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'HrdJobOffer' AND column_name = 'ContractDurationMonths') THEN
        ALTER TABLE public.""HrdJobOffer"" ADD COLUMN ""ContractDurationMonths"" integer NULL;
    END IF;
END $$;
");

            // 2.4 Reshape MstOnboardingTemplate & MstOnboardingTemplateTask
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'MstOnboardingTemplate') THEN
        CREATE TABLE public.""MstOnboardingTemplate"" (
            ""Id"" uuid NOT NULL,
            ""TemplateCode"" character varying(50) NOT NULL,
            ""TemplateName"" character varying(200) NOT NULL,
            ""TemplateLayer"" integer NOT NULL,
            ""OrganizationUnitId"" uuid NULL,
            ""TargetCompletionDays"" integer NOT NULL DEFAULT 30,
            ""Version"" integer NOT NULL DEFAULT 1,
            ""Description"" character varying(1500) NULL,
            ""IsActive"" boolean NOT NULL DEFAULT true,
            ""CreateDateTime"" timestamp with time zone NOT NULL,
            ""CreateBy"" uuid NOT NULL,
            ""UpdateDateTime"" timestamp with time zone NULL,
            ""UpdateBy"" uuid NOT NULL,
            ""DeleteDateTime"" timestamp with time zone NULL,
            ""DeleteBy"" uuid NOT NULL,
            ""CancelDateTime"" timestamp with time zone NULL,
            ""CancelBy"" uuid NOT NULL,
            ""IsCancel"" boolean NOT NULL DEFAULT false,
            ""IsDelete"" boolean NOT NULL DEFAULT false,
            CONSTRAINT ""PK_MstOnboardingTemplate"" PRIMARY KEY (""Id"")
        );
    ELSE
        -- Ensure required columns exist
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'MstOnboardingTemplate' AND column_name = 'TemplateLayer') THEN
            ALTER TABLE public.""MstOnboardingTemplate"" ADD COLUMN ""TemplateLayer"" integer NOT NULL DEFAULT 0;
        END IF;
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'MstOnboardingTemplate' AND column_name = 'OrganizationUnitId') THEN
            ALTER TABLE public.""MstOnboardingTemplate"" ADD COLUMN ""OrganizationUnitId"" uuid NULL;
        END IF;
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'MstOnboardingTemplate' AND column_name = 'TargetCompletionDays') THEN
            ALTER TABLE public.""MstOnboardingTemplate"" ADD COLUMN ""TargetCompletionDays"" integer NOT NULL DEFAULT 30;
        END IF;
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'MstOnboardingTemplate' AND column_name = 'Version') THEN
            ALTER TABLE public.""MstOnboardingTemplate"" ADD COLUMN ""Version"" integer NOT NULL DEFAULT 1;
        END IF;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'MstOnboardingTemplateTask') THEN
        CREATE TABLE public.""MstOnboardingTemplateTask"" (
            ""Id"" uuid NOT NULL,
            ""OnboardingTemplateId"" uuid NOT NULL,
            ""TaskCode"" character varying(50) NOT NULL,
            ""TaskName"" character varying(250) NOT NULL,
            ""TaskCategory"" character varying(50) NOT NULL,
            ""Description"" character varying(1000) NULL,
            ""ResponsibleRole"" integer NOT NULL DEFAULT 0,
            ""ItemKind"" integer NOT NULL DEFAULT 0,
            ""AutomaticSource"" integer NULL,
            ""CertificationTypeId"" uuid NULL,
            ""SystemItemCode"" character varying(40) NULL,
            ""IsRequired"" boolean NOT NULL DEFAULT true,
            ""DueDayOffset"" integer NOT NULL DEFAULT 0,
            ""IsActive"" boolean NOT NULL DEFAULT true,
            ""CreateDateTime"" timestamp with time zone NOT NULL,
            ""CreateBy"" uuid NOT NULL,
            ""UpdateDateTime"" timestamp with time zone NULL,
            ""UpdateBy"" uuid NOT NULL,
            ""DeleteDateTime"" timestamp with time zone NULL,
            ""DeleteBy"" uuid NOT NULL,
            ""CancelDateTime"" timestamp with time zone NULL,
            ""CancelBy"" uuid NOT NULL,
            ""IsCancel"" boolean NOT NULL DEFAULT false,
            ""IsDelete"" boolean NOT NULL DEFAULT false,
            CONSTRAINT ""PK_MstOnboardingTemplateTask"" PRIMARY KEY (""Id"")
        );
    ELSE
        -- Ensure required columns exist
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'MstOnboardingTemplateTask' AND column_name = 'ResponsibleRole') THEN
            ALTER TABLE public.""MstOnboardingTemplateTask"" ADD COLUMN ""ResponsibleRole"" integer NOT NULL DEFAULT 0;
        END IF;
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'MstOnboardingTemplateTask' AND column_name = 'ItemKind') THEN
            ALTER TABLE public.""MstOnboardingTemplateTask"" ADD COLUMN ""ItemKind"" integer NOT NULL DEFAULT 0;
        END IF;
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'MstOnboardingTemplateTask' AND column_name = 'AutomaticSource') THEN
            ALTER TABLE public.""MstOnboardingTemplateTask"" ADD COLUMN ""AutomaticSource"" integer NULL;
        END IF;
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'MstOnboardingTemplateTask' AND column_name = 'CertificationTypeId') THEN
            ALTER TABLE public.""MstOnboardingTemplateTask"" ADD COLUMN ""CertificationTypeId"" uuid NULL;
        END IF;
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'MstOnboardingTemplateTask' AND column_name = 'SystemItemCode') THEN
            ALTER TABLE public.""MstOnboardingTemplateTask"" ADD COLUMN ""SystemItemCode"" character varying(40) NULL;
        END IF;
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'MstOnboardingTemplateTask' AND column_name = 'DueDayOffset') THEN
            ALTER TABLE public.""MstOnboardingTemplateTask"" ADD COLUMN ""DueDayOffset"" integer NOT NULL DEFAULT 0;
        END IF;
    END IF;
END $$;
");

            // =========================================================================
            // 3. PEMBUATAN 12 TABEL BARU HRD LIFECYCLE MANAGEMENT (BE-HRD-063)
            // =========================================================================

            // 3.1 HrdEmploymentLifecycleSetting
            migrationBuilder.CreateTable(
                name: "HrdEmploymentLifecycleSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreceptorshipDefaultMonths = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    PreceptorMaxMentees = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    PreceptorMinActiveYears = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    FixedTermFirstReminderDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    FixedTermSecondReminderDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    FixedTermThirdReminderDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 14),
                    ProbationFirstReminderDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    ProbationSecondReminderDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 14),
                    ProbationEvaluationCutoffDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 7),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdEmploymentLifecycleSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdEmploymentLifecycleSetting_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3.2 HrdOnboardingCase
            migrationBuilder.CreateTable(
                name: "HrdOnboardingCase",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    HiringHandoffId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsClinical = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PlannedStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CoordinatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    FirstDayAttendedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    FirstDayRecordedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReadyForWorkAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadyForWorkByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<int>(type: "integer", nullable: true),
                    CancelNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    HasGeneralTemplate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdOnboardingCase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_HrdCandidateHiringHandoff_HiringHandoffId",
                        column: x => x.HiringHandoffId,
                        principalSchema: "public",
                        principalTable: "HrdCandidateHiringHandoff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_AspNetUsers_CoordinatorUserId",
                        column: x => x.CoordinatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3.3 HrdOnboardingCaseItem
            migrationBuilder.CreateTable(
                name: "HrdOnboardingCaseItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateLayer = table.Column<int>(type: "integer", nullable: false),
                    SourceTemplateTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    TaskCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SystemItemCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ResponsibleRole = table.Column<int>(type: "integer", nullable: false),
                    ItemKind = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AutomaticSource = table.Column<int>(type: "integer", nullable: true),
                    CertificationTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DueDayOffset = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ItemStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SelectedWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceStoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EvidenceFileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EvidenceContentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    EvidenceFileSize = table.Column<long>(type: "bigint", nullable: true),
                    AutomaticCheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AutomaticResultNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdOnboardingCaseItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCaseItem_HrdOnboardingCase_OnboardingCaseId",
                        column: x => x.OnboardingCaseId,
                        principalSchema: "public",
                        principalTable: "HrdOnboardingCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCaseItem_MstOnboardingTemplateTask_SourceTemplateTaskId",
                        column: x => x.SourceTemplateTaskId,
                        principalSchema: "public",
                        principalTable: "MstOnboardingTemplateTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCaseItem_MstCertificationType_CertificationTypeId",
                        column: x => x.CertificationTypeId,
                        principalSchema: "public",
                        principalTable: "MstCertificationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCaseItem_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCaseItem_MstWorkforceProfile_SelectedWorkforceProfileId",
                        column: x => x.SelectedWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3.4 HrdOnboardingAccessRequest
            migrationBuilder.CreateTable(
                name: "HrdOnboardingAccessRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestType = table.Column<int>(type: "integer", nullable: false),
                    RequestStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FailureMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SelfServiceActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdOnboardingAccessRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingAccessRequest_HrdOnboardingCase_OnboardingCaseId",
                        column: x => x.OnboardingCaseId,
                        principalSchema: "public",
                        principalTable: "HrdOnboardingCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingAccessRequest_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingAccessRequest_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3.5 HrdProbation
            migrationBuilder.CreateTable(
                name: "HrdProbation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    DirectSupervisorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProbationStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ProbationEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    FinalRecommendation = table.Column<int>(type: "integer", nullable: true),
                    EvaluationCutoffDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecisionWorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdProbation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdProbation_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdProbation_WfpContractHistory_ContractHistoryId",
                        column: x => x.ContractHistoryId,
                        principalSchema: "public",
                        principalTable: "WfpContractHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdProbation_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdProbation_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdProbation_AspNetUsers_DirectSupervisorUserId",
                        column: x => x.DirectSupervisorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3.6 HrdProbationAssessment
            migrationBuilder.CreateTable(
                name: "HrdProbationAssessment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProbationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AssessmentMonth = table.Column<int>(type: "integer", nullable: false),
                    AssessorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnicalCompetencyScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    DisciplineScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    InterpersonalScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    HospitalCoreValuesScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TotalScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Recommendation = table.Column<int>(type: "integer", nullable: false),
                    AssessorNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AssessmentStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedByEmployeeUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeFeedback = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdProbationAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdProbationAssessment_HrdProbation_ProbationId",
                        column: x => x.ProbationId,
                        principalSchema: "public",
                        principalTable: "HrdProbation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdProbationAssessment_AspNetUsers_AssessorUserId",
                        column: x => x.AssessorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3.7 HrdClinicalPreceptorship
            migrationBuilder.CreateTable(
                name: "HrdClinicalPreceptorship",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionKind = table.Column<int>(type: "integer", nullable: false),
                    PrimaryPreceptorWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TargetEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ActualEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PreceptorshipStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    LogbookCompletedEntries = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LogbookTotalEntries = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FinalAssessmentScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    FinalClearanceResult = table.Column<int>(type: "integer", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdClinicalPreceptorship", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdClinicalPreceptorship_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdClinicalPreceptorship_HrdOnboardingCase_OnboardingCaseId",
                        column: x => x.OnboardingCaseId,
                        principalSchema: "public",
                        principalTable: "HrdOnboardingCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdClinicalPreceptorship_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdClinicalPreceptorship_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdClinicalPreceptorship_MstWorkforceProfile_PrimaryPreceptorWorkforceProfileId",
                        column: x => x.PrimaryPreceptorWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3.8 HrdClinicalPreceptorshipPreceptor
            migrationBuilder.CreateTable(
                name: "HrdClinicalPreceptorshipPreceptor",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreceptorshipId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreceptorWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AssignedFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    AssignedTo = table.Column<DateOnly>(type: "date", nullable: true),
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
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdClinicalPreceptorshipPreceptor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdClinicalPreceptorshipPreceptor_HrdClinicalPreceptorship_PreceptorshipId",
                        column: x => x.PreceptorshipId,
                        principalSchema: "public",
                        principalTable: "HrdClinicalPreceptorship",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdClinicalPreceptorshipPreceptor_MstWorkforceProfile_PreceptorWorkforceProfileId",
                        column: x => x.PreceptorWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3.9 HrdContractDecision
            migrationBuilder.CreateTable(
                name: "HrdContractDecision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DecisionNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ContractHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecisionType = table.Column<int>(type: "integer", nullable: false),
                    DecisionStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ExtensionDurationMonths = table.Column<int>(type: "integer", nullable: true),
                    ProposedNewContractTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProposedStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ProposedEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NonRenewalNoticeDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NonRenewalSeveranceApplicable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    NonRenewalReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Justification = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResultingContractHistoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdContractDecision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdContractDecision_WfpContractHistory_ContractHistoryId",
                        column: x => x.ContractHistoryId,
                        principalSchema: "public",
                        principalTable: "WfpContractHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdContractDecision_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdContractDecision_MstContractType_ProposedNewContractTypeId",
                        column: x => x.ProposedNewContractTypeId,
                        principalSchema: "public",
                        principalTable: "MstContractType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdContractDecision_WfpContractHistory_ResultingContractHistoryId",
                        column: x => x.ResultingContractHistoryId,
                        principalSchema: "public",
                        principalTable: "WfpContractHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3.10 HrdContractCorrection
            migrationBuilder.CreateTable(
                name: "HrdContractCorrection",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectionNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ContractHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectionStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldContractNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NewContractNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OldContractTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    NewContractTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NewStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OldEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NewEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OldDurationMonths = table.Column<int>(type: "integer", nullable: true),
                    NewDurationMonths = table.Column<int>(type: "integer", nullable: true),
                    OldDocumentFileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    NewDocumentFileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    OldDocumentContentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    NewDocumentContentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    OldDocumentStoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NewDocumentStoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdContractCorrection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdContractCorrection_WfpContractHistory_ContractHistoryId",
                        column: x => x.ContractHistoryId,
                        principalSchema: "public",
                        principalTable: "WfpContractHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdContractCorrection_MstContractType_OldContractTypeId",
                        column: x => x.OldContractTypeId,
                        principalSchema: "public",
                        principalTable: "MstContractType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdContractCorrection_MstContractType_NewContractTypeId",
                        column: x => x.NewContractTypeId,
                        principalSchema: "public",
                        principalTable: "MstContractType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3.11 HrdInitialContractBatch
            migrationBuilder.CreateTable(
                name: "HrdInitialContractBatch",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppliedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdInitialContractBatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdInitialContractBatch_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3.12 HrdInitialContractBatchRow
            migrationBuilder.CreateTable(
                name: "HrdInitialContractBatchRow",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowIndex = table.Column<int>(type: "integer", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DurationMonths = table.Column<int>(type: "integer", nullable: true),
                    FixedTermSeriesStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ValidationErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResultingContractHistoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_HrdInitialContractBatchRow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdInitialContractBatchRow_HrdInitialContractBatch_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "public",
                        principalTable: "HrdInitialContractBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdInitialContractBatchRow_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdInitialContractBatchRow_MstContractType_ContractTypeId",
                        column: x => x.ContractTypeId,
                        principalSchema: "public",
                        principalTable: "MstContractType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdInitialContractBatchRow_WfpContractHistory_ResultingContractHistoryId",
                        column: x => x.ResultingContractHistoryId,
                        principalSchema: "public",
                        principalTable: "WfpContractHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // =========================================================================
            // 4. INDEKS-INDEKS
            // =========================================================================
            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCase_CaseNumber",
                schema: "public",
                table: "HrdOnboardingCase",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCase_HiringHandoffId",
                schema: "public",
                table: "HrdOnboardingCase",
                column: "HiringHandoffId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCase_WorkforceProfileId",
                schema: "public",
                table: "HrdOnboardingCase",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdProbation_WorkforceProfileId",
                schema: "public",
                table: "HrdProbation",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdProbation_ContractHistoryId",
                schema: "public",
                table: "HrdProbation",
                column: "ContractHistoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractDecision_DecisionNumber",
                schema: "public",
                table: "HrdContractDecision",
                column: "DecisionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractCorrection_CorrectionNumber",
                schema: "public",
                table: "HrdContractCorrection",
                column: "CorrectionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialContractBatch_BatchNumber",
                schema: "public",
                table: "HrdInitialContractBatch",
                column: "BatchNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "HrdInitialContractBatchRow", schema: "public");
            migrationBuilder.DropTable(name: "HrdInitialContractBatch", schema: "public");
            migrationBuilder.DropTable(name: "HrdContractCorrection", schema: "public");
            migrationBuilder.DropTable(name: "HrdContractDecision", schema: "public");
            migrationBuilder.DropTable(name: "HrdClinicalPreceptorshipPreceptor", schema: "public");
            migrationBuilder.DropTable(name: "HrdClinicalPreceptorship", schema: "public");
            migrationBuilder.DropTable(name: "HrdProbationAssessment", schema: "public");
            migrationBuilder.DropTable(name: "HrdProbation", schema: "public");
            migrationBuilder.DropTable(name: "HrdOnboardingAccessRequest", schema: "public");
            migrationBuilder.DropTable(name: "HrdOnboardingCaseItem", schema: "public");
            migrationBuilder.DropTable(name: "HrdOnboardingCase", schema: "public");
            migrationBuilder.DropTable(name: "HrdEmploymentLifecycleSetting", schema: "public");
        }
    }
}
