using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeTrxAttendancePeriodAndTrxAttendanceSchedulerJob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxAttendanceRawLog_AttendanceDeviceId_ExternalLogId",
                schema: "public",
                table: "TrxAttendanceRawLog");

            migrationBuilder.DropIndex(
                name: "IX_TrxAttendanceDaily_WorkforceProfileId_AttendanceDate",
                schema: "public",
                table: "TrxAttendanceDaily");

            migrationBuilder.DropIndex(
                name: "IX_TrxAttendanceCorrectionRequest_WorkflowDefinitionId",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest");

            migrationBuilder.DropIndex(
                name: "IX_TrxAttendanceCorrectionRequest_WorkflowInstanceId",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest");

            migrationBuilder.AlterColumn<string>(
                name: "SourceType",
                schema: "public",
                table: "TrxAttendanceRawLog",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Device",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessingStatus",
                schema: "public",
                table: "TrxAttendanceRawLog",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                schema: "public",
                table: "TrxAttendanceRawLog",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Unknown",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "SegmentType",
                schema: "public",
                table: "TrxAttendanceDailySegment",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Work",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "SegmentStatus",
                schema: "public",
                table: "TrxAttendanceDailySegment",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Calculated",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "SegmentSource",
                schema: "public",
                table: "TrxAttendanceDailySegment",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Processor",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<Guid>(
                name: "ShiftAssignmentId",
                schema: "public",
                table: "TrxAttendanceDailySegment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessingStatus",
                schema: "public",
                table: "TrxAttendanceDaily",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "PayrollInputStatus",
                schema: "public",
                table: "TrxAttendanceDaily",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "AttendanceStatus",
                schema: "public",
                table: "TrxAttendanceDaily",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Unprocessed",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "AttendancePeriodId",
                schema: "public",
                table: "TrxAttendanceDaily",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryShiftAssignmentId",
                schema: "public",
                table: "TrxAttendanceDaily",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScheduleResolutionJson",
                schema: "public",
                table: "TrxAttendanceDaily",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScheduleSource",
                schema: "public",
                table: "TrxAttendanceDaily",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Unresolved");

            migrationBuilder.AlterColumn<string>(
                name: "RequestStatus",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Draft",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "CorrectionType",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "AttendanceTime",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateTable(
                name: "TrxAttendanceProcessingRun",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ProcessingMode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Batch"),
                    RunStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    TriggerSource = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "System"),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TargetWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessingVersion = table.Column<int>(type: "integer", nullable: false),
                    TargetCount = table.Column<int>(type: "integer", nullable: false),
                    SuccessCount = table.Column<int>(type: "integer", nullable: false),
                    FailedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TriggeredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ParametersJson = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_TrxAttendanceProcessingRun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxAttendanceProcessingRun_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendanceProcessingRun_AspNetUsers_TriggeredByUserId",
                        column: x => x.TriggeredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendanceProcessingRun_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendanceProcessingRun_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendanceProcessingRun_MstOrganizationUnit_Organization~",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendanceProcessingRun_MstWorkforceProfile_TargetWorkfo~",
                        column: x => x.TargetWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxAttendancePeriod",
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
                    RequirePayrollHandoff = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ScheduledCloseAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastProcessingRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastValidatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidationSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CloseReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReopenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReopenedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReopenReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReopenCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_TrxAttendancePeriod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxAttendancePeriod_AspNetUsers_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendancePeriod_AspNetUsers_ReopenedByUserId",
                        column: x => x.ReopenedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendancePeriod_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendancePeriod_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendancePeriod_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendancePeriod_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendancePeriod_TrxAttendanceProcessingRun_LastProcessi~",
                        column: x => x.LastProcessingRunId,
                        principalSchema: "public",
                        principalTable: "TrxAttendanceProcessingRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxAttendanceSchedulerJob",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    JobType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "ProcessRange"),
                    JobStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    AttendancePeriodId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ForceReprocess = table.Column<bool>(type: "boolean", nullable: false),
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
                    ProcessingRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    TriggeredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ParametersJson = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_TrxAttendanceSchedulerJob", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxAttendanceSchedulerJob_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendanceSchedulerJob_AspNetUsers_TriggeredByUserId",
                        column: x => x.TriggeredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendanceSchedulerJob_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendanceSchedulerJob_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendanceSchedulerJob_MstOrganizationUnit_OrganizationU~",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendanceSchedulerJob_MstWorkforceProfile_WorkforceProf~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendanceSchedulerJob_TrxAttendancePeriod_AttendancePer~",
                        column: x => x.AttendancePeriodId,
                        principalSchema: "public",
                        principalTable: "TrxAttendancePeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxAttendanceSchedulerJob_TrxAttendanceProcessingRun_Proces~",
                        column: x => x.ProcessingRunId,
                        principalSchema: "public",
                        principalTable: "TrxAttendanceProcessingRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceRawLog_AttendanceDeviceId_ExternalLogId",
                schema: "public",
                table: "TrxAttendanceRawLog",
                columns: new[] { "AttendanceDeviceId", "ExternalLogId" },
                unique: true,
                filter: "\"AttendanceDeviceId\" IS NOT NULL AND \"ExternalLogId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceRawLog_SourceType_EventAt",
                schema: "public",
                table: "TrxAttendanceRawLog",
                columns: new[] { "SourceType", "EventAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceDailySegment_ShiftAssignmentId_SegmentType",
                schema: "public",
                table: "TrxAttendanceDailySegment",
                columns: new[] { "ShiftAssignmentId", "SegmentType" },
                filter: "\"ShiftAssignmentId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceDaily_AttendancePeriodId",
                schema: "public",
                table: "TrxAttendanceDaily",
                column: "AttendancePeriodId",
                filter: "\"AttendancePeriodId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceDaily_PrimaryShiftAssignmentId",
                schema: "public",
                table: "TrxAttendanceDaily",
                column: "PrimaryShiftAssignmentId",
                filter: "\"PrimaryShiftAssignmentId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceDaily_ScheduleSource_AttendanceDate",
                schema: "public",
                table: "TrxAttendanceDaily",
                columns: new[] { "ScheduleSource", "AttendanceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceDaily_WorkforceProfileId_AttendanceDate",
                schema: "public",
                table: "TrxAttendanceDaily",
                columns: new[] { "WorkforceProfileId", "AttendanceDate" },
                unique: true,
                filter: "\"WorkforceProfileId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceCorrectionRequest_WorkflowDefinitionId_Request~",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest",
                columns: new[] { "WorkflowDefinitionId", "RequestStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceCorrectionRequest_WorkflowInstanceId",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest",
                column: "WorkflowInstanceId",
                unique: true,
                filter: "\"WorkflowInstanceId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendancePeriod_ClosedByUserId",
                schema: "public",
                table: "TrxAttendancePeriod",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendancePeriod_DepartmentId",
                schema: "public",
                table: "TrxAttendancePeriod",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendancePeriod_HospitalSiteId",
                schema: "public",
                table: "TrxAttendancePeriod",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendancePeriod_LastProcessingRunId",
                schema: "public",
                table: "TrxAttendancePeriod",
                column: "LastProcessingRunId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendancePeriod_LegalEntityId_HospitalSiteId_Organizati~",
                schema: "public",
                table: "TrxAttendancePeriod",
                columns: new[] { "LegalEntityId", "HospitalSiteId", "OrganizationUnitId", "DepartmentId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendancePeriod_OrganizationUnitId",
                schema: "public",
                table: "TrxAttendancePeriod",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendancePeriod_PeriodCode",
                schema: "public",
                table: "TrxAttendancePeriod",
                column: "PeriodCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendancePeriod_PeriodStatus_ScheduledCloseAt_IsActive_~",
                schema: "public",
                table: "TrxAttendancePeriod",
                columns: new[] { "PeriodStatus", "ScheduledCloseAt", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendancePeriod_ReopenedByUserId",
                schema: "public",
                table: "TrxAttendancePeriod",
                column: "ReopenedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendancePeriod_StartDate_EndDate_PeriodStatus",
                schema: "public",
                table: "TrxAttendancePeriod",
                columns: new[] { "StartDate", "EndDate", "PeriodStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceProcessingRun_CancelledByUserId",
                schema: "public",
                table: "TrxAttendanceProcessingRun",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceProcessingRun_CorrelationId",
                schema: "public",
                table: "TrxAttendanceProcessingRun",
                column: "CorrelationId",
                unique: true,
                filter: "\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceProcessingRun_DepartmentId",
                schema: "public",
                table: "TrxAttendanceProcessingRun",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceProcessingRun_HospitalSiteId_OrganizationUnitI~",
                schema: "public",
                table: "TrxAttendanceProcessingRun",
                columns: new[] { "HospitalSiteId", "OrganizationUnitId", "DepartmentId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceProcessingRun_OrganizationUnitId",
                schema: "public",
                table: "TrxAttendanceProcessingRun",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceProcessingRun_RunNumber",
                schema: "public",
                table: "TrxAttendanceProcessingRun",
                column: "RunNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceProcessingRun_RunStatus_StartedAt",
                schema: "public",
                table: "TrxAttendanceProcessingRun",
                columns: new[] { "RunStatus", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceProcessingRun_StartDate_EndDate_ProcessingMode",
                schema: "public",
                table: "TrxAttendanceProcessingRun",
                columns: new[] { "StartDate", "EndDate", "ProcessingMode" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceProcessingRun_TargetWorkforceProfileId_StartDa~",
                schema: "public",
                table: "TrxAttendanceProcessingRun",
                columns: new[] { "TargetWorkforceProfileId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceProcessingRun_TriggeredByUserId",
                schema: "public",
                table: "TrxAttendanceProcessingRun",
                column: "TriggeredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceSchedulerJob_AttendancePeriodId_JobStatus",
                schema: "public",
                table: "TrxAttendanceSchedulerJob",
                columns: new[] { "AttendancePeriodId", "JobStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceSchedulerJob_CancelledByUserId",
                schema: "public",
                table: "TrxAttendanceSchedulerJob",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceSchedulerJob_CorrelationId",
                schema: "public",
                table: "TrxAttendanceSchedulerJob",
                column: "CorrelationId",
                unique: true,
                filter: "\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceSchedulerJob_DepartmentId",
                schema: "public",
                table: "TrxAttendanceSchedulerJob",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceSchedulerJob_HospitalSiteId",
                schema: "public",
                table: "TrxAttendanceSchedulerJob",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceSchedulerJob_JobNumber",
                schema: "public",
                table: "TrxAttendanceSchedulerJob",
                column: "JobNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceSchedulerJob_JobStatus_AvailableAt_Priority",
                schema: "public",
                table: "TrxAttendanceSchedulerJob",
                columns: new[] { "JobStatus", "AvailableAt", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceSchedulerJob_OrganizationUnitId",
                schema: "public",
                table: "TrxAttendanceSchedulerJob",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceSchedulerJob_ProcessingRunId",
                schema: "public",
                table: "TrxAttendanceSchedulerJob",
                column: "ProcessingRunId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceSchedulerJob_StartDate_EndDate_JobType",
                schema: "public",
                table: "TrxAttendanceSchedulerJob",
                columns: new[] { "StartDate", "EndDate", "JobType" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceSchedulerJob_TriggeredByUserId",
                schema: "public",
                table: "TrxAttendanceSchedulerJob",
                column: "TriggeredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceSchedulerJob_WorkforceProfileId_HospitalSiteId~",
                schema: "public",
                table: "TrxAttendanceSchedulerJob",
                columns: new[] { "WorkforceProfileId", "HospitalSiteId", "OrganizationUnitId", "DepartmentId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrxAttendanceCorrectionRequest_TrxWorkflowInstance_Workflow~",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest",
                column: "WorkflowInstanceId",
                principalSchema: "public",
                principalTable: "TrxWorkflowInstance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxAttendanceDaily_TrxAttendancePeriod_AttendancePeriodId",
                schema: "public",
                table: "TrxAttendanceDaily",
                column: "AttendancePeriodId",
                principalSchema: "public",
                principalTable: "TrxAttendancePeriod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxAttendanceDaily_TrxShiftAssignment_PrimaryShiftAssignmen~",
                schema: "public",
                table: "TrxAttendanceDaily",
                column: "PrimaryShiftAssignmentId",
                principalSchema: "public",
                principalTable: "TrxShiftAssignment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxAttendanceDailySegment_TrxShiftAssignment_ShiftAssignmen~",
                schema: "public",
                table: "TrxAttendanceDailySegment",
                column: "ShiftAssignmentId",
                principalSchema: "public",
                principalTable: "TrxShiftAssignment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxAttendanceCorrectionRequest_TrxWorkflowInstance_Workflow~",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxAttendanceDaily_TrxAttendancePeriod_AttendancePeriodId",
                schema: "public",
                table: "TrxAttendanceDaily");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxAttendanceDaily_TrxShiftAssignment_PrimaryShiftAssignmen~",
                schema: "public",
                table: "TrxAttendanceDaily");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxAttendanceDailySegment_TrxShiftAssignment_ShiftAssignmen~",
                schema: "public",
                table: "TrxAttendanceDailySegment");

            migrationBuilder.DropTable(
                name: "TrxAttendanceSchedulerJob",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxAttendancePeriod",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxAttendanceProcessingRun",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_TrxAttendanceRawLog_AttendanceDeviceId_ExternalLogId",
                schema: "public",
                table: "TrxAttendanceRawLog");

            migrationBuilder.DropIndex(
                name: "IX_TrxAttendanceRawLog_SourceType_EventAt",
                schema: "public",
                table: "TrxAttendanceRawLog");

            migrationBuilder.DropIndex(
                name: "IX_TrxAttendanceDailySegment_ShiftAssignmentId_SegmentType",
                schema: "public",
                table: "TrxAttendanceDailySegment");

            migrationBuilder.DropIndex(
                name: "IX_TrxAttendanceDaily_AttendancePeriodId",
                schema: "public",
                table: "TrxAttendanceDaily");

            migrationBuilder.DropIndex(
                name: "IX_TrxAttendanceDaily_PrimaryShiftAssignmentId",
                schema: "public",
                table: "TrxAttendanceDaily");

            migrationBuilder.DropIndex(
                name: "IX_TrxAttendanceDaily_ScheduleSource_AttendanceDate",
                schema: "public",
                table: "TrxAttendanceDaily");

            migrationBuilder.DropIndex(
                name: "IX_TrxAttendanceDaily_WorkforceProfileId_AttendanceDate",
                schema: "public",
                table: "TrxAttendanceDaily");

            migrationBuilder.DropIndex(
                name: "IX_TrxAttendanceCorrectionRequest_WorkflowDefinitionId_Request~",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest");

            migrationBuilder.DropIndex(
                name: "IX_TrxAttendanceCorrectionRequest_WorkflowInstanceId",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest");

            migrationBuilder.DropColumn(
                name: "ShiftAssignmentId",
                schema: "public",
                table: "TrxAttendanceDailySegment");

            migrationBuilder.DropColumn(
                name: "AttendancePeriodId",
                schema: "public",
                table: "TrxAttendanceDaily");

            migrationBuilder.DropColumn(
                name: "PrimaryShiftAssignmentId",
                schema: "public",
                table: "TrxAttendanceDaily");

            migrationBuilder.DropColumn(
                name: "ScheduleResolutionJson",
                schema: "public",
                table: "TrxAttendanceDaily");

            migrationBuilder.DropColumn(
                name: "ScheduleSource",
                schema: "public",
                table: "TrxAttendanceDaily");

            migrationBuilder.AlterColumn<string>(
                name: "SourceType",
                schema: "public",
                table: "TrxAttendanceRawLog",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Device");

            migrationBuilder.AlterColumn<string>(
                name: "ProcessingStatus",
                schema: "public",
                table: "TrxAttendanceRawLog",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                schema: "public",
                table: "TrxAttendanceRawLog",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Unknown");

            migrationBuilder.AlterColumn<string>(
                name: "SegmentType",
                schema: "public",
                table: "TrxAttendanceDailySegment",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Work");

            migrationBuilder.AlterColumn<string>(
                name: "SegmentStatus",
                schema: "public",
                table: "TrxAttendanceDailySegment",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Calculated");

            migrationBuilder.AlterColumn<string>(
                name: "SegmentSource",
                schema: "public",
                table: "TrxAttendanceDailySegment",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Processor");

            migrationBuilder.AlterColumn<string>(
                name: "ProcessingStatus",
                schema: "public",
                table: "TrxAttendanceDaily",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<string>(
                name: "PayrollInputStatus",
                schema: "public",
                table: "TrxAttendanceDaily",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<string>(
                name: "AttendanceStatus",
                schema: "public",
                table: "TrxAttendanceDaily",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Unprocessed");

            migrationBuilder.AlterColumn<string>(
                name: "RequestStatus",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Draft");

            migrationBuilder.AlterColumn<string>(
                name: "CorrectionType",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "AttendanceTime");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceRawLog_AttendanceDeviceId_ExternalLogId",
                schema: "public",
                table: "TrxAttendanceRawLog",
                columns: new[] { "AttendanceDeviceId", "ExternalLogId" },
                unique: true,
                filter: "\"ExternalLogId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceDaily_WorkforceProfileId_AttendanceDate",
                schema: "public",
                table: "TrxAttendanceDaily",
                columns: new[] { "WorkforceProfileId", "AttendanceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceCorrectionRequest_WorkflowDefinitionId",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxAttendanceCorrectionRequest_WorkflowInstanceId",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest",
                column: "WorkflowInstanceId");
        }
    }
}
