using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkforcePlanningCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HrdInitialFormationBatch",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDocumentReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceDocumentPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SourceDocumentFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BatchStatus = table.Column<int>(type: "integer", nullable: false),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    ValidRows = table.Column<int>(type: "integer", nullable: false),
                    InvalidRows = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_HrdInitialFormationBatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdInitialFormationBatch_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdInitialFormationBatch_MstWorkflowDefinition_WorkflowDefi~",
                        column: x => x.WorkflowDefinitionId,
                        principalSchema: "public",
                        principalTable: "MstWorkflowDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdManpowerPlan",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlanKind = table.Column<int>(type: "integer", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ParentPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    AddendumSequence = table.Column<int>(type: "integer", nullable: true),
                    ReferenceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PlanStatus = table.Column<int>(type: "integer", nullable: false),
                    Justification = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "IDR"),
                    TotalEstimatedCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FinanceAdjustedTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FinanceAdjustmentReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FinanceAdjustedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FinanceAdjustedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_HrdManpowerPlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdManpowerPlan_HrdManpowerPlan_ParentPlanId",
                        column: x => x.ParentPlanId,
                        principalSchema: "public",
                        principalTable: "HrdManpowerPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdManpowerPlan_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdManpowerPlan_MstWorkflowDefinition_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalSchema: "public",
                        principalTable: "MstWorkflowDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdPositionFormation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    FormationQuantity = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PreviousFormationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_HrdPositionFormation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdPositionFormation_HrdPositionFormation_PreviousFormation~",
                        column: x => x.PreviousFormationId,
                        principalSchema: "public",
                        principalTable: "HrdPositionFormation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdPositionFormation_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdPositionFormation_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdPositionFormation_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdStaffingGapSnapshot",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotType = table.Column<int>(type: "integer", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SourceDocumentType = table.Column<int>(type: "integer", nullable: true),
                    SourceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodMonth = table.Column<DateOnly>(type: "date", nullable: true),
                    TakenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReconstructedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_HrdStaffingGapSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdStaffingGapSnapshot_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdStaffingStandard",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StandardNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StandardStatus = table.Column<int>(type: "integer", nullable: false),
                    Justification = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SupersedesStandardId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_HrdStaffingStandard", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdStaffingStandard_HrdStaffingStandard_SupersedesStandardId",
                        column: x => x.SupersedesStandardId,
                        principalSchema: "public",
                        principalTable: "HrdStaffingStandard",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdStaffingStandard_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdStaffingStandard_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdStaffingStandard_MstWorkflowDefinition_WorkflowDefinitio~",
                        column: x => x.WorkflowDefinitionId,
                        principalSchema: "public",
                        principalTable: "MstWorkflowDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdStaffingWorkloadInput",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WorkloadValue = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    SourceNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_HrdStaffingWorkloadInput", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdStaffingWorkloadInput_MstOrganizationUnit_OrganizationUn~",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdStaffingWorkloadInput_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdWorkforcePlanningSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    LongLeaveThresholdDays = table.Column<int>(type: "integer", nullable: true),
                    OverdueRequestThresholdDays = table.Column<int>(type: "integer", nullable: true),
                    ChangeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_HrdWorkforcePlanningSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdWorkforcePlanningSetting_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdHeadcountRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaryGradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestType = table.Column<int>(type: "integer", nullable: false),
                    RequestStatus = table.Column<int>(type: "integer", nullable: false),
                    RequiredStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TemporaryEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TemporaryReasonWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Justification = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RequestedQuantity = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false, defaultValue: 0m),
                    ExternalSeatCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    InternalSeatCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FormationVersionIdAtSubmission = table.Column<Guid>(type: "uuid", nullable: true),
                    RemainingSeatsAtSubmission = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    EstimatedMonthlyRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CostStatus = table.Column<int>(type: "integer", nullable: false),
                    FinanceAdjustedCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FinanceAdjustmentReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FinanceAdjustedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FinanceAdjustedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsOverFormationAfterAddendum = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsOverdue = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedFormationId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadyForRecruitmentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WithdrawnAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_HrdHeadcountRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequest_HrdPositionFormation_CreatedFormationId",
                        column: x => x.CreatedFormationId,
                        principalSchema: "public",
                        principalTable: "HrdPositionFormation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequest_HrdPositionFormation_FormationVersionId~",
                        column: x => x.FormationVersionIdAtSubmission,
                        principalSchema: "public",
                        principalTable: "HrdPositionFormation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequest_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequest_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequest_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequest_MstSalaryGrade_SalaryGradeId",
                        column: x => x.SalaryGradeId,
                        principalSchema: "public",
                        principalTable: "MstSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequest_MstWorkflowDefinition_WorkflowDefinitio~",
                        column: x => x.WorkflowDefinitionId,
                        principalSchema: "public",
                        principalTable: "MstWorkflowDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequest_MstWorkforceProfile_TemporaryReasonWork~",
                        column: x => x.TemporaryReasonWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdInitialFormationBatchRow",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormationQuantity = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ValidationMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedFormationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_HrdInitialFormationBatchRow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdInitialFormationBatchRow_HrdInitialFormationBatch_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "public",
                        principalTable: "HrdInitialFormationBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdInitialFormationBatchRow_HrdPositionFormation_CreatedFor~",
                        column: x => x.CreatedFormationId,
                        principalSchema: "public",
                        principalTable: "HrdPositionFormation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdInitialFormationBatchRow_MstOrganizationUnit_Organizatio~",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdInitialFormationBatchRow_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdManpowerPlanLine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaryGradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentFormationQuantity = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false, defaultValue: 0m),
                    RecommendedQuantity = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    RecommendationBreakdown = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ProposedFormationQuantity = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    ChangeQuantity = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false, defaultValue: 0m),
                    DeviationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MonthlyRateSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MonthsInPeriod = table.Column<int>(type: "integer", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IncrementalCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CostStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedFormationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_HrdManpowerPlanLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdManpowerPlanLine_HrdManpowerPlan_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "public",
                        principalTable: "HrdManpowerPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdManpowerPlanLine_HrdPositionFormation_CreatedFormationId",
                        column: x => x.CreatedFormationId,
                        principalSchema: "public",
                        principalTable: "HrdPositionFormation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdManpowerPlanLine_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdManpowerPlanLine_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdManpowerPlanLine_MstSalaryGrade_SalaryGradeId",
                        column: x => x.SalaryGradeId,
                        principalSchema: "public",
                        principalTable: "MstSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdStaffingGapSnapshotLine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormationQuantity = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    FilledFte = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    ToBeVacatedFte = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    TemporarilyUnavailableFte = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    NotYetCompetentFte = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    OutsourcedFte = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    ConcurrentAssignmentCount = table.Column<int>(type: "integer", nullable: true),
                    PermanentGap = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    TemporaryGap = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    CompetencyGap = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    PlacementDataComplete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LeaveDataComplete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CompetencyDataComplete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    WorkforceTypeDataComplete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ShortageSummaryCount = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_HrdStaffingGapSnapshotLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdStaffingGapSnapshotLine_HrdStaffingGapSnapshot_SnapshotId",
                        column: x => x.SnapshotId,
                        principalSchema: "public",
                        principalTable: "HrdStaffingGapSnapshot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdStaffingGapSnapshotLine_MstOrganizationUnit_Organization~",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdStaffingGapSnapshotLine_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdStaffingSkillMixRule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffingStandardId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProfessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompetencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinimumCompetencyLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MinimumQualifiedHeadcount = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_HrdStaffingSkillMixRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdStaffingSkillMixRule_HrdStaffingStandard_StaffingStandar~",
                        column: x => x.StaffingStandardId,
                        principalSchema: "public",
                        principalTable: "HrdStaffingStandard",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdStaffingSkillMixRule_MstCompetency_CompetencyId",
                        column: x => x.CompetencyId,
                        principalSchema: "public",
                        principalTable: "MstCompetency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdStaffingSkillMixRule_MstProfession_ProfessionId",
                        column: x => x.ProfessionId,
                        principalSchema: "public",
                        principalTable: "MstProfession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdStaffingSkillMixRule_MstShift_ShiftId",
                        column: x => x.ShiftId,
                        principalSchema: "public",
                        principalTable: "MstShift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdStaffingStandardLine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffingStandardId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CalculationMethod = table.Column<int>(type: "integer", nullable: false),
                    MinimumHeadcount = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    RatioStaffQuantity = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    RatioWorkloadQuantity = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    WorkloadUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ShiftsPerDay = table.Column<int>(type: "integer", nullable: true),
                    StaffPerShift = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    ServiceHoursPerDay = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ReliefFactor = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    RoundingMethod = table.Column<int>(type: "integer", nullable: false),
                    CountsOutsourced = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_HrdStaffingStandardLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdStaffingStandardLine_HrdStaffingStandard_StaffingStandar~",
                        column: x => x.StaffingStandardId,
                        principalSchema: "public",
                        principalTable: "HrdStaffingStandard",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdStaffingStandardLine_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdStaffingStandardLine_MstProfession_ProfessionId",
                        column: x => x.ProfessionId,
                        principalSchema: "public",
                        principalTable: "MstProfession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdHeadcountRequestCancellation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CancellationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HeadcountRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    CancellationStatus = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedFormationId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_HrdHeadcountRequestCancellation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequestCancellation_HrdHeadcountRequest_Headcou~",
                        column: x => x.HeadcountRequestId,
                        principalSchema: "public",
                        principalTable: "HrdHeadcountRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequestCancellation_HrdPositionFormation_Create~",
                        column: x => x.CreatedFormationId,
                        principalSchema: "public",
                        principalTable: "HrdPositionFormation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdHeadcountRequestSeat",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeadcountRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatNumber = table.Column<int>(type: "integer", nullable: false),
                    SeatFte = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false, defaultValue: 1.00m),
                    FulfilmentSource = table.Column<int>(type: "integer", nullable: false),
                    SeatStatus = table.Column<int>(type: "integer", nullable: false),
                    ReplacedWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedPositionAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedPersonnelActionOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FulfilledAt = table.Column<DateOnly>(type: "date", nullable: true),
                    CancellationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_HrdHeadcountRequestSeat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequestSeat_HrdHeadcountRequestCancellation_Can~",
                        column: x => x.CancellationId,
                        principalSchema: "public",
                        principalTable: "HrdHeadcountRequestCancellation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequestSeat_HrdHeadcountRequest_HeadcountReques~",
                        column: x => x.HeadcountRequestId,
                        principalSchema: "public",
                        principalTable: "HrdHeadcountRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequestSeat_HrdPersonnelActionOrder_LinkedPerso~",
                        column: x => x.LinkedPersonnelActionOrderId,
                        principalSchema: "public",
                        principalTable: "HrdPersonnelActionOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequestSeat_MstWorkforceProfile_ReplacedWorkfor~",
                        column: x => x.ReplacedWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdHeadcountRequestSeat_WfpPositionAssignment_LinkedPositio~",
                        column: x => x.LinkedPositionAssignmentId,
                        principalSchema: "public",
                        principalTable: "WfpPositionAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdFulfilmentPlanChange",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeadcountRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromSource = table.Column<int>(type: "integer", nullable: false),
                    ToSource = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_HrdFulfilmentPlanChange", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdFulfilmentPlanChange_HrdHeadcountRequestSeat_SeatId",
                        column: x => x.SeatId,
                        principalSchema: "public",
                        principalTable: "HrdHeadcountRequestSeat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdFulfilmentPlanChange_HrdHeadcountRequest_HeadcountReques~",
                        column: x => x.HeadcountRequestId,
                        principalSchema: "public",
                        principalTable: "HrdHeadcountRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HrdFulfilmentPlanChange_HeadcountRequestId",
                schema: "public",
                table: "HrdFulfilmentPlanChange",
                column: "HeadcountRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdFulfilmentPlanChange_SeatId",
                schema: "public",
                table: "HrdFulfilmentPlanChange",
                column: "SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequest_CreatedFormationId",
                schema: "public",
                table: "HrdHeadcountRequest",
                column: "CreatedFormationId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequest_FormationVersionIdAtSubmission",
                schema: "public",
                table: "HrdHeadcountRequest",
                column: "FormationVersionIdAtSubmission");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequest_HospitalSiteId",
                schema: "public",
                table: "HrdHeadcountRequest",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequest_IsOverdue",
                schema: "public",
                table: "HrdHeadcountRequest",
                column: "IsOverdue");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequest_IsOverFormationAfterAddendum",
                schema: "public",
                table: "HrdHeadcountRequest",
                column: "IsOverFormationAfterAddendum");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequest_PositionId",
                schema: "public",
                table: "HrdHeadcountRequest",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequest_RequestNumber",
                schema: "public",
                table: "HrdHeadcountRequest",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequest_SalaryGradeId",
                schema: "public",
                table: "HrdHeadcountRequest",
                column: "SalaryGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequest_TemporaryReasonWorkforceProfileId",
                schema: "public",
                table: "HrdHeadcountRequest",
                column: "TemporaryReasonWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequest_Unit_Position_Status",
                schema: "public",
                table: "HrdHeadcountRequest",
                columns: new[] { "OrganizationUnitId", "PositionId", "RequestStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequest_WorkflowDefinitionId",
                schema: "public",
                table: "HrdHeadcountRequest",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequest_WorkflowInstanceId",
                schema: "public",
                table: "HrdHeadcountRequest",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequestCancellation_CreatedFormationId",
                schema: "public",
                table: "HrdHeadcountRequestCancellation",
                column: "CreatedFormationId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequestCancellation_HeadcountRequestId",
                schema: "public",
                table: "HrdHeadcountRequestCancellation",
                column: "HeadcountRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequestCancellation_Number",
                schema: "public",
                table: "HrdHeadcountRequestCancellation",
                column: "CancellationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequestSeat_CancellationId",
                schema: "public",
                table: "HrdHeadcountRequestSeat",
                column: "CancellationId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequestSeat_LinkedPersonnelActionOrder",
                schema: "public",
                table: "HrdHeadcountRequestSeat",
                column: "LinkedPersonnelActionOrderId",
                unique: true,
                filter: "\"LinkedPersonnelActionOrderId\" IS NOT NULL AND \"SeatStatus\" <> 5 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequestSeat_LinkedPositionAssignment",
                schema: "public",
                table: "HrdHeadcountRequestSeat",
                column: "LinkedPositionAssignmentId",
                unique: true,
                filter: "\"LinkedPositionAssignmentId\" IS NOT NULL AND \"SeatStatus\" <> 5 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequestSeat_ReplacedWorkforceProfileId",
                schema: "public",
                table: "HrdHeadcountRequestSeat",
                column: "ReplacedWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequestSeat_Request_Seat",
                schema: "public",
                table: "HrdHeadcountRequestSeat",
                columns: new[] { "HeadcountRequestId", "SeatNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdHeadcountRequestSeat_SeatStatus",
                schema: "public",
                table: "HrdHeadcountRequestSeat",
                column: "SeatStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialFormationBatch_BatchNumber",
                schema: "public",
                table: "HrdInitialFormationBatch",
                column: "BatchNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialFormationBatch_BatchStatus",
                schema: "public",
                table: "HrdInitialFormationBatch",
                column: "BatchStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialFormationBatch_HospitalSiteId",
                schema: "public",
                table: "HrdInitialFormationBatch",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialFormationBatch_WorkflowDefinitionId",
                schema: "public",
                table: "HrdInitialFormationBatch",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialFormationBatch_WorkflowInstanceId",
                schema: "public",
                table: "HrdInitialFormationBatch",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialFormationBatchRow_Batch_Unit_Position",
                schema: "public",
                table: "HrdInitialFormationBatchRow",
                columns: new[] { "BatchId", "OrganizationUnitId", "PositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialFormationBatchRow_CreatedFormationId",
                schema: "public",
                table: "HrdInitialFormationBatchRow",
                column: "CreatedFormationId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialFormationBatchRow_OrganizationUnitId_PositionId",
                schema: "public",
                table: "HrdInitialFormationBatchRow",
                columns: new[] { "OrganizationUnitId", "PositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialFormationBatchRow_PositionId",
                schema: "public",
                table: "HrdInitialFormationBatchRow",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdManpowerPlan_Parent_Sequence",
                schema: "public",
                table: "HrdManpowerPlan",
                columns: new[] { "ParentPlanId", "AddendumSequence" },
                unique: true,
                filter: "\"ParentPlanId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HrdManpowerPlan_PlanNumber",
                schema: "public",
                table: "HrdManpowerPlan",
                column: "PlanNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdManpowerPlan_PlanStatus",
                schema: "public",
                table: "HrdManpowerPlan",
                column: "PlanStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HrdManpowerPlan_Site_Year_Annual",
                schema: "public",
                table: "HrdManpowerPlan",
                columns: new[] { "HospitalSiteId", "PlanYear" },
                unique: true,
                filter: "\"PlanKind\" = 1 AND \"PlanStatus\" NOT IN (4, 6) AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdManpowerPlan_WorkflowDefinitionId",
                schema: "public",
                table: "HrdManpowerPlan",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdManpowerPlan_WorkflowInstanceId",
                schema: "public",
                table: "HrdManpowerPlan",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdManpowerPlanLine_CreatedFormationId",
                schema: "public",
                table: "HrdManpowerPlanLine",
                column: "CreatedFormationId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdManpowerPlanLine_OrganizationUnitId",
                schema: "public",
                table: "HrdManpowerPlanLine",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdManpowerPlanLine_Plan_Unit_Position",
                schema: "public",
                table: "HrdManpowerPlanLine",
                columns: new[] { "PlanId", "OrganizationUnitId", "PositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdManpowerPlanLine_PositionId",
                schema: "public",
                table: "HrdManpowerPlanLine",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdManpowerPlanLine_SalaryGradeId",
                schema: "public",
                table: "HrdManpowerPlanLine",
                column: "SalaryGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPositionFormation_HospitalSiteId",
                schema: "public",
                table: "HrdPositionFormation",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPositionFormation_PositionId",
                schema: "public",
                table: "HrdPositionFormation",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPositionFormation_PreviousFormationId",
                schema: "public",
                table: "HrdPositionFormation",
                column: "PreviousFormationId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPositionFormation_SourceDocumentId",
                schema: "public",
                table: "HrdPositionFormation",
                column: "SourceDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPositionFormation_Unit_Position_Start",
                schema: "public",
                table: "HrdPositionFormation",
                columns: new[] { "OrganizationUnitId", "PositionId", "EffectiveStartDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdPositionFormation_Unit_Position_Version",
                schema: "public",
                table: "HrdPositionFormation",
                columns: new[] { "OrganizationUnitId", "PositionId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingGapSnapshot_Site_Month",
                schema: "public",
                table: "HrdStaffingGapSnapshot",
                columns: new[] { "HospitalSiteId", "PeriodMonth" },
                unique: true,
                filter: "\"SnapshotType\" = 2 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingGapSnapshot_SourceDocumentType_SourceDocumentId",
                schema: "public",
                table: "HrdStaffingGapSnapshot",
                columns: new[] { "SourceDocumentType", "SourceDocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingGapSnapshotLine_OrganizationUnitId",
                schema: "public",
                table: "HrdStaffingGapSnapshotLine",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingGapSnapshotLine_PositionId",
                schema: "public",
                table: "HrdStaffingGapSnapshotLine",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingGapSnapshotLine_Snapshot_Unit_Position",
                schema: "public",
                table: "HrdStaffingGapSnapshotLine",
                columns: new[] { "SnapshotId", "OrganizationUnitId", "PositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingSkillMixRule_CompetencyId",
                schema: "public",
                table: "HrdStaffingSkillMixRule",
                column: "CompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingSkillMixRule_ProfessionId",
                schema: "public",
                table: "HrdStaffingSkillMixRule",
                column: "ProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingSkillMixRule_ShiftId",
                schema: "public",
                table: "HrdStaffingSkillMixRule",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingSkillMixRule_Standard_Shift_Comp_Prof",
                schema: "public",
                table: "HrdStaffingSkillMixRule",
                columns: new[] { "StaffingStandardId", "ShiftId", "CompetencyId", "ProfessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingStandard_HospitalSiteId",
                schema: "public",
                table: "HrdStaffingStandard",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingStandard_StandardNumber",
                schema: "public",
                table: "HrdStaffingStandard",
                column: "StandardNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingStandard_StandardStatus",
                schema: "public",
                table: "HrdStaffingStandard",
                column: "StandardStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingStandard_SupersedesStandardId",
                schema: "public",
                table: "HrdStaffingStandard",
                column: "SupersedesStandardId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingStandard_Unit_StartDate",
                schema: "public",
                table: "HrdStaffingStandard",
                columns: new[] { "OrganizationUnitId", "EffectiveStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingStandard_Unit_Version",
                schema: "public",
                table: "HrdStaffingStandard",
                columns: new[] { "OrganizationUnitId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingStandard_WorkflowDefinitionId",
                schema: "public",
                table: "HrdStaffingStandard",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingStandard_WorkflowInstanceId",
                schema: "public",
                table: "HrdStaffingStandard",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingStandardLine_PositionId",
                schema: "public",
                table: "HrdStaffingStandardLine",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingStandardLine_ProfessionId",
                schema: "public",
                table: "HrdStaffingStandardLine",
                column: "ProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingStandardLine_Standard_Position",
                schema: "public",
                table: "HrdStaffingStandardLine",
                columns: new[] { "StaffingStandardId", "PositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingWorkloadInput_PositionId",
                schema: "public",
                table: "HrdStaffingWorkloadInput",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdStaffingWorkloadInput_Unit_Position_Period",
                schema: "public",
                table: "HrdStaffingWorkloadInput",
                columns: new[] { "OrganizationUnitId", "PositionId", "PeriodStartDate" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdWorkforcePlanningSetting_Site",
                schema: "public",
                table: "HrdWorkforcePlanningSetting",
                column: "HospitalSiteId",
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HrdFulfilmentPlanChange",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdInitialFormationBatchRow",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdManpowerPlanLine",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdStaffingGapSnapshotLine",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdStaffingSkillMixRule",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdStaffingStandardLine",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdStaffingWorkloadInput",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdWorkforcePlanningSetting",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdHeadcountRequestSeat",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdInitialFormationBatch",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdManpowerPlan",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdStaffingGapSnapshot",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdStaffingStandard",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdHeadcountRequestCancellation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdHeadcountRequest",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdPositionFormation",
                schema: "public");
        }
    }
}
