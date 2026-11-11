using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261109160000_AddPerformanceManagementCoreEntities")]
    public partial class AddPerformanceManagementCoreEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================================================================
            // 1. TABEL 1: mst_kpi_metric_catalogs
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "mst_kpi_metric_catalogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MetricName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ComponentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetProfessionGroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TargetPolarity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DefaultTargetValue = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    ScoringRubricJson = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_mst_kpi_metric_catalogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mst_kpi_metric_catalogs_MetricCode",
                schema: "public",
                table: "mst_kpi_metric_catalogs",
                column: "MetricCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_mst_kpi_metric_catalogs_ComponentType",
                schema: "public",
                table: "mst_kpi_metric_catalogs",
                column: "ComponentType");

            migrationBuilder.CreateIndex(
                name: "IX_mst_kpi_metric_catalogs_IsActive",
                schema: "public",
                table: "mst_kpi_metric_catalogs",
                column: "IsActive");

            // =========================================================================
            // 2. TABEL 2: trx_performance_cycles
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "trx_performance_cycles",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CycleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CycleTitle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EvaluationYear = table.Column<int>(type: "integer", nullable: false),
                    CycleType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SelfAppraisalDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ManagerAppraisalDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CalibrationDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                    AllowEmployeeViewFinalResult = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RequireSelfAssessment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EnablePeerFeedback = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TotalEmployee = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompletedEmployee = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MasterPerformanceCycleId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformanceTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    RatingScaleId = table.Column<Guid>(type: "uuid", nullable: true),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CycleConfigurationJson = table.Column<string>(type: "jsonb", nullable: true),
                    PopulationSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    GoalSettingDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SelfAssessmentDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ManagerAssessmentDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_trx_performance_cycles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_cycles_CycleCode",
                schema: "public",
                table: "trx_performance_cycles",
                column: "CycleCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_cycles_EvaluationYear_Status",
                schema: "public",
                table: "trx_performance_cycles",
                columns: new[] { "EvaluationYear", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_cycles_StartDate_EndDate",
                schema: "public",
                table: "trx_performance_cycles",
                columns: new[] { "StartDate", "EndDate" });

            // =========================================================================
            // 3. TABEL 3: trx_performance_goals
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "trx_performance_goals",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceCycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    KpiMetricCatalogId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalDescription = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    WeightPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TargetValue = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    ActualValue = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    AchievementPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_trx_performance_goals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_performance_goals_trx_performance_cycles_PerformanceCycleId",
                        column: x => x.PerformanceCycleId,
                        principalSchema: "public",
                        principalTable: "trx_performance_cycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trx_performance_goals_mst_kpi_metric_catalogs_KpiMetricCatalogId",
                        column: x => x.KpiMetricCatalogId,
                        principalSchema: "public",
                        principalTable: "mst_kpi_metric_catalogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_goals_PerformanceCycleId",
                schema: "public",
                table: "trx_performance_goals",
                column: "PerformanceCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_goals_WorkforceProfileId",
                schema: "public",
                table: "trx_performance_goals",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_goals_KpiMetricCatalogId",
                schema: "public",
                table: "trx_performance_goals",
                column: "KpiMetricCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_goals_Status",
                schema: "public",
                table: "trx_performance_goals",
                column: "Status");

            // =========================================================================
            // 4. TABEL 4: trx_performance_appraisals
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "trx_performance_appraisals",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceCycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstLineManagerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecondLineManagerId = table.Column<Guid>(type: "uuid", nullable: true),
                    KpiScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    BehaviorScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    PreCalibrationScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    FinalCompositeScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    PerformanceBand = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                    CalibrationNotes = table.Column<string>(type: "text", nullable: true),
                    ManagerFeedbackNotes = table.Column<string>(type: "text", nullable: true),
                    EmployeeAcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_trx_performance_appraisals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_performance_appraisals_trx_performance_cycles_PerformanceCycleId",
                        column: x => x.PerformanceCycleId,
                        principalSchema: "public",
                        principalTable: "trx_performance_cycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisals_PerformanceCycleId_WorkforceProfileId",
                schema: "public",
                table: "trx_performance_appraisals",
                columns: new[] { "PerformanceCycleId", "WorkforceProfileId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisals_FirstLineManagerId",
                schema: "public",
                table: "trx_performance_appraisals",
                column: "FirstLineManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisals_SecondLineManagerId",
                schema: "public",
                table: "trx_performance_appraisals",
                column: "SecondLineManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisals_FinalCompositeScore",
                schema: "public",
                table: "trx_performance_appraisals",
                column: "FinalCompositeScore");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisals_PerformanceBand",
                schema: "public",
                table: "trx_performance_appraisals",
                column: "PerformanceBand");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisals_Status",
                schema: "public",
                table: "trx_performance_appraisals",
                column: "Status");

            // =========================================================================
            // 5. TABEL 5: trx_performance_appraisal_details
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "trx_performance_appraisal_details",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceAppraisalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MetricOrCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WeightPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    SelfScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    ManagerScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    FinalScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    EvidenceDocumentUrl = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EvaluatorNotes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_trx_performance_appraisal_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_performance_appraisal_details_trx_performance_appraisals_PerformanceAppraisalId",
                        column: x => x.PerformanceAppraisalId,
                        principalSchema: "public",
                        principalTable: "trx_performance_appraisals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisal_details_PerformanceAppraisalId",
                schema: "public",
                table: "trx_performance_appraisal_details",
                column: "PerformanceAppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisal_details_ComponentType",
                schema: "public",
                table: "trx_performance_appraisal_details",
                column: "ComponentType");

            // =========================================================================
            // 6. TABEL 6: trx_performance_calibration_sessions
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "trx_performance_calibration_sessions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceCycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SessionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuidingDistributionJson = table.Column<string>(type: "jsonb", nullable: false),
                    ActualDistributionJson = table.Column<string>(type: "jsonb", nullable: true),
                    TotalEmployeesCalibrated = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalOverridesApplied = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MinutesOfMeetingUrl = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Scheduled"),
                    CompletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_trx_performance_calibration_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_performance_calibration_sessions_trx_performance_cycles_PerformanceCycleId",
                        column: x => x.PerformanceCycleId,
                        principalSchema: "public",
                        principalTable: "trx_performance_cycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_calibration_sessions_SessionNumber",
                schema: "public",
                table: "trx_performance_calibration_sessions",
                column: "SessionNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_calibration_sessions_PerformanceCycleId",
                schema: "public",
                table: "trx_performance_calibration_sessions",
                column: "PerformanceCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_calibration_sessions_Status",
                schema: "public",
                table: "trx_performance_calibration_sessions",
                column: "Status");

            // =========================================================================
            // 7. TABEL 7: trx_performance_improvement_plans
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "trx_performance_improvement_plans",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PipNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PerformanceAppraisalId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    MentorWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActionPlanSummary = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                    FinalVerdict = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    VerdictNotes = table.Column<string>(type: "text", nullable: true),
                    PerformanceCycleId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformanceReviewId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObjectivesJson = table.Column<string>(type: "jsonb", nullable: true),
                    ActionPlanJson = table.Column<string>(type: "jsonb", nullable: true),
                    SuccessMetricsJson = table.Column<string>(type: "jsonb", nullable: true),
                    CheckInFrequencyDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    ProgressPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    Outcome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OutcomeNote = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    IsEmployeeAcknowledged = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EmployeeAcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_trx_performance_improvement_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_performance_improvement_plans_trx_performance_appraisals_PerformanceAppraisalId",
                        column: x => x.PerformanceAppraisalId,
                        principalSchema: "public",
                        principalTable: "trx_performance_appraisals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_improvement_plans_PipNumber",
                schema: "public",
                table: "trx_performance_improvement_plans",
                column: "PipNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_improvement_plans_PerformanceAppraisalId",
                schema: "public",
                table: "trx_performance_improvement_plans",
                column: "PerformanceAppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_improvement_plans_WorkforceProfileId",
                schema: "public",
                table: "trx_performance_improvement_plans",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_improvement_plans_Status",
                schema: "public",
                table: "trx_performance_improvement_plans",
                column: "Status");

            // =========================================================================
            // 8. TABEL 8: trx_performance_pip_action_logs
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "trx_performance_pip_action_logs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PipId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekNumber = table.Column<int>(type: "integer", nullable: false),
                    LogDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProgressSummary = table.Column<string>(type: "text", nullable: false),
                    DeficienciesObserved = table.Column<string>(type: "text", nullable: true),
                    MentorFeedback = table.Column<string>(type: "text", nullable: false),
                    IsTargetOnTrack = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LoggedBy = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_trx_performance_pip_action_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_performance_pip_action_logs_trx_performance_improvement_plans_PipId",
                        column: x => x.PipId,
                        principalSchema: "public",
                        principalTable: "trx_performance_improvement_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_pip_action_logs_PipId",
                schema: "public",
                table: "trx_performance_pip_action_logs",
                column: "PipId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_pip_action_logs_PipId_WeekNumber",
                schema: "public",
                table: "trx_performance_pip_action_logs",
                columns: new[] { "PipId", "WeekNumber" });

            // =========================================================================
            // 9. TABEL 9: trx_clinical_career_ladder_reviews
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "trx_clinical_career_ladder_reviews",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceAppraisalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentClinicalLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetClinicalLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClinicalExperienceYears = table.Column<decimal>(type: "numeric(4,1)", nullable: false),
                    MeetsPerformanceThreshold = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MeetsClinicalCompetency = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FreeFromSentinelIncident = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CommitteeRecommendation = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CommitteeNotes = table.Column<string>(type: "text", nullable: true),
                    FinalStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Submitted"),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NewSkNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("PK_trx_clinical_career_ladder_reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_clinical_career_ladder_reviews_trx_performance_appraisals_PerformanceAppraisalId",
                        column: x => x.PerformanceAppraisalId,
                        principalSchema: "public",
                        principalTable: "trx_performance_appraisals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trx_clinical_career_ladder_reviews_ReviewNumber",
                schema: "public",
                table: "trx_clinical_career_ladder_reviews",
                column: "ReviewNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_trx_clinical_career_ladder_reviews_WorkforceProfileId",
                schema: "public",
                table: "trx_clinical_career_ladder_reviews",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_clinical_career_ladder_reviews_PerformanceAppraisalId",
                schema: "public",
                table: "trx_clinical_career_ladder_reviews",
                column: "PerformanceAppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_clinical_career_ladder_reviews_FinalStatus",
                schema: "public",
                table: "trx_clinical_career_ladder_reviews",
                column: "FinalStatus");

            // =========================================================================
            // 10. TABEL 10: trx_performance_appraisal_disputes
            // =========================================================================
            migrationBuilder.CreateTable(
                name: "trx_performance_appraisal_disputes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisputeNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PerformanceAppraisalId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisputeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DisputedComponent = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GrievanceReason = table.Column<string>(type: "text", nullable: false),
                    EvidenceDocumentUrl = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    MediationNotes = table.Column<string>(type: "text", nullable: true),
                    FinalVerdict = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AdjustedCompositeScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    VerdictDeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Submitted"),
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
                    table.PrimaryKey("PK_trx_performance_appraisal_disputes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_performance_appraisal_disputes_trx_performance_appraisals_PerformanceAppraisalId",
                        column: x => x.PerformanceAppraisalId,
                        principalSchema: "public",
                        principalTable: "trx_performance_appraisals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisal_disputes_DisputeNumber",
                schema: "public",
                table: "trx_performance_appraisal_disputes",
                column: "DisputeNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisal_disputes_PerformanceAppraisalId",
                schema: "public",
                table: "trx_performance_appraisal_disputes",
                column: "PerformanceAppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisal_disputes_WorkforceProfileId",
                schema: "public",
                table: "trx_performance_appraisal_disputes",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisal_disputes_Status",
                schema: "public",
                table: "trx_performance_appraisal_disputes",
                column: "Status");

            // =========================================================================
            // 11. SEEDING: 7 CORE VALUES BERAKHLAK & 5 KATALOG INDIKATOR KPI RS MMC
            // =========================================================================
            migrationBuilder.Sql("""
DO $$
BEGIN
    -- 1. Seeding 7 Butir Core Values ASN BerAKHLAK (PermenPAN-RB No. 6/2022 & HRD-DEC-164)
    INSERT INTO public.mst_kpi_metric_catalogs (
        "Id", "MetricCode", "MetricName", "ComponentType", "Category", "TargetProfessionGroup", "UnitOfMeasure", "TargetPolarity", "DefaultTargetValue", "ScoringRubricJson", "IsActive", "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete"
    ) VALUES
    ('a0000001-0000-0000-0000-000000000001', 'BEH-AKH-01', 'Berorientasi Pelayanan', 'CORE_VALUES_BEHAVIOR', 'PEOPLE_CULTURE', 'ALL', 'SKALA_1_5', 'POSITIVE', 5.00, '{"1": "Tidak ramah dan pasif", "3": "Melayani sesuai SOP", "5": "Proaktif, ramah, solutif melampaui harapan pasien"}', true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', false, false),
    ('a0000001-0000-0000-0000-000000000002', 'BEH-AKH-02', 'Akuntabel', 'CORE_VALUES_BEHAVIOR', 'PEOPLE_CULTURE', 'ALL', 'SKALA_1_5', 'POSITIVE', 5.00, '{"1": "Sering lalai dokumentasi", "3": "Disiplin dan bertanggung jawab atas tugas", "5": "Integritas sempurna dan menjadi role model kepatuhan"}', true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', false, false),
    ('a0000001-0000-0000-0000-000000000003', 'BEH-AKH-03', 'Kompeten', 'CORE_VALUES_BEHAVIOR', 'PEOPLE_CULTURE', 'ALL', 'SKALA_1_5', 'POSITIVE', 5.00, '{"1": "Kinerja klinis di bawah standar", "3": "Menguasai keterampilan inti profesi", "5": "Ahli di bidangnya dan aktif membimbing rekan sejawat"}', true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', false, false),
    ('a0000001-0000-0000-0000-000000000004', 'BEH-AKH-04', 'Harmonis', 'CORE_VALUES_BEHAVIOR', 'PEOPLE_CULTURE', 'ALL', 'SKALA_1_5', 'POSITIVE', 5.00, '{"1": "Memicu konflik antar staf", "3": "Menghargai perbedaan dan kooperatif", "5": "Membangun sinergi tim positif dan lingkungan kondusif"}', true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', false, false),
    ('a0000001-0000-0000-0000-000000000005', 'BEH-AKH-05', 'Loyal', 'CORE_VALUES_BEHAVIOR', 'PEOPLE_CULTURE', 'ALL', 'SKALA_1_5', 'POSITIVE', 5.00, '{"1": "Membocorkan rahasia pasien/RS", "3": "Menjaga nama baik RS dan rahasia medis", "5": "Dedikasi luar biasa pada misi dan keselamatan pasien RS"}', true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', false, false),
    ('a0000001-0000-0000-0000-000000000006', 'BEH-AKH-06', 'Adaptif', 'CORE_VALUES_BEHAVIOR', 'PEOPLE_CULTURE', 'ALL', 'SKALA_1_5', 'POSITIVE', 5.00, '{"1": "Menolak digitalisasi dan SOP baru", "3": "Cepat beradaptasi terhadap perubahan sistem", "5": "Inovatif memprakarsai perbaikan alur kerja layanan"}', true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', false, false),
    ('a0000001-0000-0000-0000-000000000007', 'BEH-AKH-07', 'Kolaboratif', 'CORE_VALUES_BEHAVIOR', 'PEOPLE_CULTURE', 'ALL', 'SKALA_1_5', 'POSITIVE', 5.00, '{"1": "Silo antar profesi / ego sektoral", "3": "Bekerja sama lintas unit secara efektif", "5": "Mendorong kolaborasi interprofesional terintegrasi"}', true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', false, false)
    ON CONFLICT ("MetricCode") DO NOTHING;

    -- 2. Seeding 5 Indikator Kuantitatif Kinerja Rumah Sakit (HRD-DEC-164)
    INSERT INTO public.mst_kpi_metric_catalogs (
        "Id", "MetricCode", "MetricName", "ComponentType", "Category", "TargetProfessionGroup", "UnitOfMeasure", "TargetPolarity", "DefaultTargetValue", "ScoringRubricJson", "IsActive", "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete"
    ) VALUES
    ('a0000002-0000-0000-0000-000000000001', 'KPI-MED-01', 'Kepatuhan Jam Visite Dokter Spesialis (< 09:00 WIB)', 'QUANTITATIVE_KPI', 'SERVICE_QUALITY', 'MEDIS', 'PERSEN', 'POSITIVE', 90.00, NULL, true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', false, false),
    ('a0000002-0000-0000-0000-000000000002', 'KPI-MED-02', 'Kelengkapan & Ketepatan Pengisian EMR Rawat Inap (< 24 Jam)', 'QUANTITATIVE_KPI', 'PATIENT_SAFETY', 'MEDIS', 'PERSEN', 'POSITIVE', 95.00, NULL, true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', false, false),
    ('a0000002-0000-0000-0000-000000000003', 'KPI-NSG-01', 'Angka Kejadian Pasien Jatuh di Ruang Rawat Inap', 'QUANTITATIVE_KPI', 'PATIENT_SAFETY', 'KEPERAWATAN', 'JUMLAH_KASUS', 'NEGATIVE', 0.00, NULL, true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', false, false),
    ('a0000002-0000-0000-0000-000000000004', 'KPI-PHR-01', 'Waktu Tunggu Pelayanan Resep Obat Racikan (< 30 Menit)', 'QUANTITATIVE_KPI', 'SERVICE_QUALITY', 'PENUNJANG', 'PERSEN', 'POSITIVE', 85.00, NULL, true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', false, false),
    ('a0000002-0000-0000-0000-000000000005', 'KPI-EMG-01', 'Response Time Pelayanan IGD Kasus Triage P1 (< 5 Menit)', 'QUANTITATIVE_KPI', 'PATIENT_SAFETY', 'ALL', 'PERSEN', 'POSITIVE', 100.00, NULL, true, CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', false, false)
    ON CONFLICT ("MetricCode") DO NOTHING;
END $$;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "trx_performance_appraisal_disputes", schema: "public");
            migrationBuilder.DropTable(name: "trx_clinical_career_ladder_reviews", schema: "public");
            migrationBuilder.DropTable(name: "trx_performance_pip_action_logs", schema: "public");
            migrationBuilder.DropTable(name: "trx_performance_improvement_plans", schema: "public");
            migrationBuilder.DropTable(name: "trx_performance_calibration_sessions", schema: "public");
            migrationBuilder.DropTable(name: "trx_performance_appraisal_details", schema: "public");
            migrationBuilder.DropTable(name: "trx_performance_appraisals", schema: "public");
            migrationBuilder.DropTable(name: "trx_performance_goals", schema: "public");
            migrationBuilder.DropTable(name: "trx_performance_cycles", schema: "public");
            migrationBuilder.DropTable(name: "mst_kpi_metric_catalogs", schema: "public");
        }
    }
}
