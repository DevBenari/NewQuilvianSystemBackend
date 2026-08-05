using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class addOvertimeClosingSchedulerAndReconciliation4I : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrxOvertimePeriod",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PeriodName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Open"),
                    RequireAttendanceFinal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RequireVerificationComplete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RequireSettlementComplete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ScheduledCloseAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastValidatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastReconciledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidationSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    ReconciliationSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CloseReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReopenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReopenedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReopenReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReopenCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CloseVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_TrxOvertimePeriod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePeriod_AspNetUsers_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePeriod_AspNetUsers_ReopenedByUserId",
                        column: x => x.ReopenedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePeriod_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePeriod_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePeriod_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePeriod_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxOvertimeSchedulerJob",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    JobType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "FullCycle"),
                    JobStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    OvertimePeriodId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AllowRepair = table.Column<bool>(type: "boolean", nullable: false),
                    ForceRecalculate = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaxRetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AvailableAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HeartbeatAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkerInstanceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TriggeredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ParametersJson = table.Column<string>(type: "jsonb", nullable: true),
                    ResultJson = table.Column<string>(type: "jsonb", nullable: true),
                    LastError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_TrxOvertimeSchedulerJob", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxOvertimeSchedulerJob_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimeSchedulerJob_AspNetUsers_TriggeredByUserId",
                        column: x => x.TriggeredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimeSchedulerJob_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimeSchedulerJob_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimeSchedulerJob_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimeSchedulerJob_MstOrganizationUnit_OrganizationUni~",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimeSchedulerJob_MstWorkforceProfile_WorkforceProfil~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimeSchedulerJob_TrxOvertimePeriod_OvertimePeriodId",
                        column: x => x.OvertimePeriodId,
                        principalSchema: "public",
                        principalTable: "TrxOvertimePeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePeriod_ClosedByUserId",
                schema: "public",
                table: "TrxOvertimePeriod",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePeriod_DepartmentId",
                schema: "public",
                table: "TrxOvertimePeriod",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePeriod_HospitalSiteId",
                schema: "public",
                table: "TrxOvertimePeriod",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePeriod_LegalEntityId_HospitalSiteId_Organization~",
                schema: "public",
                table: "TrxOvertimePeriod",
                columns: new[] { "LegalEntityId", "HospitalSiteId", "OrganizationUnitId", "DepartmentId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePeriod_OrganizationUnitId",
                schema: "public",
                table: "TrxOvertimePeriod",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePeriod_PeriodCode",
                schema: "public",
                table: "TrxOvertimePeriod",
                column: "PeriodCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePeriod_PeriodStatus_ScheduledCloseAt_IsActive_Is~",
                schema: "public",
                table: "TrxOvertimePeriod",
                columns: new[] { "PeriodStatus", "ScheduledCloseAt", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePeriod_ReopenedByUserId",
                schema: "public",
                table: "TrxOvertimePeriod",
                column: "ReopenedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePeriod_StartDate_EndDate_PeriodStatus",
                schema: "public",
                table: "TrxOvertimePeriod",
                columns: new[] { "StartDate", "EndDate", "PeriodStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimeSchedulerJob_CancelledByUserId",
                schema: "public",
                table: "TrxOvertimeSchedulerJob",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimeSchedulerJob_CorrelationId",
                schema: "public",
                table: "TrxOvertimeSchedulerJob",
                column: "CorrelationId",
                unique: true,
                filter: "\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimeSchedulerJob_DepartmentId",
                schema: "public",
                table: "TrxOvertimeSchedulerJob",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimeSchedulerJob_HospitalSiteId",
                schema: "public",
                table: "TrxOvertimeSchedulerJob",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimeSchedulerJob_JobNumber",
                schema: "public",
                table: "TrxOvertimeSchedulerJob",
                column: "JobNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimeSchedulerJob_JobStatus_AvailableAt_Priority",
                schema: "public",
                table: "TrxOvertimeSchedulerJob",
                columns: new[] { "JobStatus", "AvailableAt", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimeSchedulerJob_LegalEntityId",
                schema: "public",
                table: "TrxOvertimeSchedulerJob",
                column: "LegalEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimeSchedulerJob_OrganizationUnitId",
                schema: "public",
                table: "TrxOvertimeSchedulerJob",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimeSchedulerJob_OvertimePeriodId_JobStatus",
                schema: "public",
                table: "TrxOvertimeSchedulerJob",
                columns: new[] { "OvertimePeriodId", "JobStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimeSchedulerJob_StartDate_EndDate_JobType",
                schema: "public",
                table: "TrxOvertimeSchedulerJob",
                columns: new[] { "StartDate", "EndDate", "JobType" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimeSchedulerJob_TriggeredByUserId",
                schema: "public",
                table: "TrxOvertimeSchedulerJob",
                column: "TriggeredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimeSchedulerJob_WorkforceProfileId_HospitalSiteId_O~",
                schema: "public",
                table: "TrxOvertimeSchedulerJob",
                columns: new[] { "WorkforceProfileId", "HospitalSiteId", "OrganizationUnitId", "DepartmentId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxOvertimeSchedulerJob",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxOvertimePeriod",
                schema: "public");
        }
    }
}
