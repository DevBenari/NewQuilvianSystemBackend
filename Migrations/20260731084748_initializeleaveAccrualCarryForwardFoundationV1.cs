using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeleaveAccrualCarryForwardFoundationV1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LeaveCarryForwardId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrxLeaveAccrualRun",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveEntitlementPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveEntitlementPolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RunNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RunMode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Scheduled"),
                    RunStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                    ScheduledAccrualDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AccrualPeriodStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AccrualPeriodEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsDryRun = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ForceReprocess = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaximumRetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    TargetCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CalculatedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PostedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FailedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalCalculatedDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    TotalPostedDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    IdempotencyKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TriggeredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParametersJson = table.Column<string>(type: "jsonb", nullable: true),
                    ResultSummaryJson = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_TrxLeaveAccrualRun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAccrualRun_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAccrualRun_AspNetUsers_TriggeredByUserId",
                        column: x => x.TriggeredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAccrualRun_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAccrualRun_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAccrualRun_MstLeaveEntitlementPolicy_LeaveEntitleme~",
                        column: x => x.LeaveEntitlementPolicyId,
                        principalSchema: "public",
                        principalTable: "MstLeaveEntitlementPolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAccrualRun_MstLeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalSchema: "public",
                        principalTable: "MstLeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAccrualRun_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAccrualRun_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAccrualRun_TrxLeaveEntitlementPeriod_LeaveEntitleme~",
                        column: x => x.LeaveEntitlementPeriodId,
                        principalSchema: "public",
                        principalTable: "TrxLeaveEntitlementPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxLeaveCarryForwardRun",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceLeaveEntitlementPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationLeaveEntitlementPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveCarryForwardPolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RunNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RunMode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Manual"),
                    RunStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                    ExecutionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsDryRun = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ForceReprocess = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaximumRetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    TargetCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CalculatedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PostedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FailedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalSourceAvailableDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    TotalEligibleDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    TotalCarryForwardDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    TotalExpiredDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    TotalExcessDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    TotalPayoutDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    IdempotencyKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TriggeredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParametersJson = table.Column<string>(type: "jsonb", nullable: true),
                    ResultSummaryJson = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_TrxLeaveCarryForwardRun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForwardRun_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForwardRun_AspNetUsers_TriggeredByUserId",
                        column: x => x.TriggeredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForwardRun_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForwardRun_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForwardRun_MstLeaveCarryForwardPolicy_LeaveCar~",
                        column: x => x.LeaveCarryForwardPolicyId,
                        principalSchema: "public",
                        principalTable: "MstLeaveCarryForwardPolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForwardRun_MstLeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalSchema: "public",
                        principalTable: "MstLeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForwardRun_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForwardRun_MstOrganizationUnit_OrganizationUni~",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForwardRun_TrxLeaveEntitlementPeriod_Destinati~",
                        column: x => x.DestinationLeaveEntitlementPeriodId,
                        principalSchema: "public",
                        principalTable: "TrxLeaveEntitlementPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForwardRun_TrxLeaveEntitlementPeriod_SourceLea~",
                        column: x => x.SourceLeaveEntitlementPeriodId,
                        principalSchema: "public",
                        principalTable: "TrxLeaveEntitlementPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxLeaveCarryForward",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveCarryForwardRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveCarryForwardPolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceLeaveEntitlementPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationLeaveEntitlementPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceLeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationLeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceLeaveBalanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationLeaveBalanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CarryForwardNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CalculationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CarryForwardExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SourceAvailableDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    EligibleDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    CarryForwardDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    ExpiredDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    ExcessDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    PayoutDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    RoundingAdjustmentDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    CarryForwardStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                    SkipReasonCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SkipReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CalculatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReversedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceBalanceSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    CalculationDetailJson = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_TrxLeaveCarryForward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForward_AspNetUsers_CalculatedByUserId",
                        column: x => x.CalculatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForward_AspNetUsers_PostedByUserId",
                        column: x => x.PostedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForward_AspNetUsers_ReversedByUserId",
                        column: x => x.ReversedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForward_MstLeaveCarryForwardPolicy_LeaveCarryF~",
                        column: x => x.LeaveCarryForwardPolicyId,
                        principalSchema: "public",
                        principalTable: "MstLeaveCarryForwardPolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForward_MstLeaveType_DestinationLeaveTypeId",
                        column: x => x.DestinationLeaveTypeId,
                        principalSchema: "public",
                        principalTable: "MstLeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForward_MstLeaveType_SourceLeaveTypeId",
                        column: x => x.SourceLeaveTypeId,
                        principalSchema: "public",
                        principalTable: "MstLeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForward_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForward_TrxLeaveCarryForwardRun_LeaveCarryForw~",
                        column: x => x.LeaveCarryForwardRunId,
                        principalSchema: "public",
                        principalTable: "TrxLeaveCarryForwardRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForward_TrxLeaveEntitlementPeriod_DestinationL~",
                        column: x => x.DestinationLeaveEntitlementPeriodId,
                        principalSchema: "public",
                        principalTable: "TrxLeaveEntitlementPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForward_TrxLeaveEntitlementPeriod_SourceLeaveE~",
                        column: x => x.SourceLeaveEntitlementPeriodId,
                        principalSchema: "public",
                        principalTable: "TrxLeaveEntitlementPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForward_WfpLeaveBalance_DestinationLeaveBalanc~",
                        column: x => x.DestinationLeaveBalanceId,
                        principalSchema: "public",
                        principalTable: "WfpLeaveBalance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveCarryForward_WfpLeaveBalance_SourceLeaveBalanceId",
                        column: x => x.SourceLeaveBalanceId,
                        principalSchema: "public",
                        principalTable: "WfpLeaveBalance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceTransaction_LeaveCarryForwardId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                column: "LeaveCarryForwardId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrual_LeaveAccrualRunId",
                schema: "public",
                table: "TrxLeaveAccrual",
                column: "LeaveAccrualRunId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrualRun_CancelledByUserId",
                schema: "public",
                table: "TrxLeaveAccrualRun",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrualRun_CorrelationId",
                schema: "public",
                table: "TrxLeaveAccrualRun",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrualRun_DepartmentId",
                schema: "public",
                table: "TrxLeaveAccrualRun",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrualRun_HospitalSiteId",
                schema: "public",
                table: "TrxLeaveAccrualRun",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrualRun_IdempotencyKey",
                schema: "public",
                table: "TrxLeaveAccrualRun",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrualRun_LeaveEntitlementPeriodId_LeaveTypeId_Lea~",
                schema: "public",
                table: "TrxLeaveAccrualRun",
                columns: new[] { "LeaveEntitlementPeriodId", "LeaveTypeId", "LeaveEntitlementPolicyId", "ScheduledAccrualDate", "RunMode" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrualRun_LeaveEntitlementPolicyId",
                schema: "public",
                table: "TrxLeaveAccrualRun",
                column: "LeaveEntitlementPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrualRun_LeaveTypeId",
                schema: "public",
                table: "TrxLeaveAccrualRun",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrualRun_LegalEntityId_HospitalSiteId_Organizatio~",
                schema: "public",
                table: "TrxLeaveAccrualRun",
                columns: new[] { "LegalEntityId", "HospitalSiteId", "OrganizationUnitId", "DepartmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrualRun_OrganizationUnitId",
                schema: "public",
                table: "TrxLeaveAccrualRun",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrualRun_RunNumber",
                schema: "public",
                table: "TrxLeaveAccrualRun",
                column: "RunNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrualRun_RunStatus_ScheduledAccrualDate_IsActive_~",
                schema: "public",
                table: "TrxLeaveAccrualRun",
                columns: new[] { "RunStatus", "ScheduledAccrualDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAccrualRun_TriggeredByUserId",
                schema: "public",
                table: "TrxLeaveAccrualRun",
                column: "TriggeredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_CalculatedByUserId",
                schema: "public",
                table: "TrxLeaveCarryForward",
                column: "CalculatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_CarryForwardExpiryDate_CarryForwardSta~",
                schema: "public",
                table: "TrxLeaveCarryForward",
                columns: new[] { "CarryForwardExpiryDate", "CarryForwardStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_CarryForwardNumber",
                schema: "public",
                table: "TrxLeaveCarryForward",
                column: "CarryForwardNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_DestinationLeaveBalanceId_CarryForward~",
                schema: "public",
                table: "TrxLeaveCarryForward",
                columns: new[] { "DestinationLeaveBalanceId", "CarryForwardStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_DestinationLeaveEntitlementPeriodId",
                schema: "public",
                table: "TrxLeaveCarryForward",
                column: "DestinationLeaveEntitlementPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_DestinationLeaveTypeId",
                schema: "public",
                table: "TrxLeaveCarryForward",
                column: "DestinationLeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_IdempotencyKey",
                schema: "public",
                table: "TrxLeaveCarryForward",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_LeaveCarryForwardPolicyId",
                schema: "public",
                table: "TrxLeaveCarryForward",
                column: "LeaveCarryForwardPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_LeaveCarryForwardRunId_WorkforceProfil~",
                schema: "public",
                table: "TrxLeaveCarryForward",
                columns: new[] { "LeaveCarryForwardRunId", "WorkforceProfileId", "SourceLeaveTypeId", "SourceLeaveEntitlementPeriodId", "DestinationLeaveEntitlementPeriodId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_PostedByUserId",
                schema: "public",
                table: "TrxLeaveCarryForward",
                column: "PostedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_ReversedByUserId",
                schema: "public",
                table: "TrxLeaveCarryForward",
                column: "ReversedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_SourceLeaveBalanceId_CarryForwardStatu~",
                schema: "public",
                table: "TrxLeaveCarryForward",
                columns: new[] { "SourceLeaveBalanceId", "CarryForwardStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_SourceLeaveEntitlementPeriodId",
                schema: "public",
                table: "TrxLeaveCarryForward",
                column: "SourceLeaveEntitlementPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_SourceLeaveTypeId",
                schema: "public",
                table: "TrxLeaveCarryForward",
                column: "SourceLeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForward_WorkforceProfileId",
                schema: "public",
                table: "TrxLeaveCarryForward",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForwardRun_CancelledByUserId",
                schema: "public",
                table: "TrxLeaveCarryForwardRun",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForwardRun_CorrelationId",
                schema: "public",
                table: "TrxLeaveCarryForwardRun",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForwardRun_DepartmentId",
                schema: "public",
                table: "TrxLeaveCarryForwardRun",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForwardRun_DestinationLeaveEntitlementPeriodId",
                schema: "public",
                table: "TrxLeaveCarryForwardRun",
                column: "DestinationLeaveEntitlementPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForwardRun_HospitalSiteId",
                schema: "public",
                table: "TrxLeaveCarryForwardRun",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForwardRun_IdempotencyKey",
                schema: "public",
                table: "TrxLeaveCarryForwardRun",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForwardRun_LeaveCarryForwardPolicyId",
                schema: "public",
                table: "TrxLeaveCarryForwardRun",
                column: "LeaveCarryForwardPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForwardRun_LeaveTypeId",
                schema: "public",
                table: "TrxLeaveCarryForwardRun",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForwardRun_LegalEntityId_HospitalSiteId_Organi~",
                schema: "public",
                table: "TrxLeaveCarryForwardRun",
                columns: new[] { "LegalEntityId", "HospitalSiteId", "OrganizationUnitId", "DepartmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForwardRun_OrganizationUnitId",
                schema: "public",
                table: "TrxLeaveCarryForwardRun",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForwardRun_RunNumber",
                schema: "public",
                table: "TrxLeaveCarryForwardRun",
                column: "RunNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForwardRun_RunStatus_ExecutionDate_IsActive_Is~",
                schema: "public",
                table: "TrxLeaveCarryForwardRun",
                columns: new[] { "RunStatus", "ExecutionDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForwardRun_SourceLeaveEntitlementPeriodId_Dest~",
                schema: "public",
                table: "TrxLeaveCarryForwardRun",
                columns: new[] { "SourceLeaveEntitlementPeriodId", "DestinationLeaveEntitlementPeriodId", "LeaveTypeId", "LeaveCarryForwardPolicyId", "ExecutionDate", "RunMode" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveCarryForwardRun_TriggeredByUserId",
                schema: "public",
                table: "TrxLeaveCarryForwardRun",
                column: "TriggeredByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxLeaveAccrual_TrxLeaveAccrualRun_LeaveAccrualRunId",
                schema: "public",
                table: "TrxLeaveAccrual",
                column: "LeaveAccrualRunId",
                principalSchema: "public",
                principalTable: "TrxLeaveAccrualRun",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxLeaveBalanceTransaction_TrxLeaveCarryForward_LeaveCarryF~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                column: "LeaveCarryForwardId",
                principalSchema: "public",
                principalTable: "TrxLeaveCarryForward",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxLeaveAccrual_TrxLeaveAccrualRun_LeaveAccrualRunId",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxLeaveBalanceTransaction_TrxLeaveCarryForward_LeaveCarryF~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropTable(
                name: "TrxLeaveAccrualRun",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxLeaveCarryForward",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxLeaveCarryForwardRun",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveBalanceTransaction_LeaveCarryForwardId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveAccrual_LeaveAccrualRunId",
                schema: "public",
                table: "TrxLeaveAccrual");

            migrationBuilder.DropColumn(
                name: "LeaveCarryForwardId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");
        }
    }
}
