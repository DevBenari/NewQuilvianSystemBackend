using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeleaveCalendarAttendanceExecutionV1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrxLeaveExecution",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutionNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveBalanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RequestedDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ExecutedDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ExecutionStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AttendanceIntegrationStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BalanceExecutionStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ExpectedAttendanceDayCount = table.Column<int>(type: "integer", nullable: false),
                    AppliedAttendanceDayCount = table.Column<int>(type: "integer", nullable: false),
                    ConflictAttendanceDayCount = table.Column<int>(type: "integer", nullable: false),
                    FailedAttendanceDayCount = table.Column<int>(type: "integer", nullable: false),
                    TotalScheduledMinutes = table.Column<int>(type: "integer", nullable: false),
                    TotalPayableLeaveMinutes = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReversedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ExecutionSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    ResultSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_TrxLeaveExecution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxLeaveExecution_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveExecution_AspNetUsers_ReversedByUserId",
                        column: x => x.ReversedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveExecution_AspNetUsers_StartedByUserId",
                        column: x => x.StartedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveExecution_MstLeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalSchema: "public",
                        principalTable: "MstLeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveExecution_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveExecution_WfpLeaveBalance_LeaveBalanceId",
                        column: x => x.LeaveBalanceId,
                        principalSchema: "public",
                        principalTable: "WfpLeaveBalance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveExecution_WfpLeaveRequest_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalSchema: "public",
                        principalTable: "WfpLeaveRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxLeaveAttendanceIntegration",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceDailyId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RequestedLeaveDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    RequestedMinutes = table.Column<int>(type: "integer", nullable: true),
                    IsHalfDay = table.Column<bool>(type: "boolean", nullable: false),
                    IsHourly = table.Column<bool>(type: "boolean", nullable: false),
                    IsPaidLeave = table.Column<bool>(type: "boolean", nullable: false),
                    ScheduledMinutes = table.Column<int>(type: "integer", nullable: false),
                    PayableLeaveMinutes = table.Column<int>(type: "integer", nullable: false),
                    IntegrationStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AttendanceStatusBefore = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AttendanceStatusAfter = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProcessingStatusBefore = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ProcessingStatusAfter = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppliedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReversedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ScheduleSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    ResultSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_TrxLeaveAttendanceIntegration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAttendanceIntegration_AspNetUsers_AppliedByUserId",
                        column: x => x.AppliedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAttendanceIntegration_AspNetUsers_ReversedByUserId",
                        column: x => x.ReversedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAttendanceIntegration_MstLeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalSchema: "public",
                        principalTable: "MstLeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAttendanceIntegration_MstWorkforceProfile_Workforce~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAttendanceIntegration_TrxAttendanceDaily_Attendance~",
                        column: x => x.AttendanceDailyId,
                        principalSchema: "public",
                        principalTable: "TrxAttendanceDaily",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAttendanceIntegration_TrxLeaveExecution_LeaveExecut~",
                        column: x => x.LeaveExecutionId,
                        principalSchema: "public",
                        principalTable: "TrxLeaveExecution",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAttendanceIntegration_WfpLeaveRequest_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalSchema: "public",
                        principalTable: "WfpLeaveRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAttendanceIntegration_AppliedByUserId",
                schema: "public",
                table: "TrxLeaveAttendanceIntegration",
                column: "AppliedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAttendanceIntegration_AttendanceDailyId",
                schema: "public",
                table: "TrxLeaveAttendanceIntegration",
                column: "AttendanceDailyId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAttendanceIntegration_IdempotencyKey",
                schema: "public",
                table: "TrxLeaveAttendanceIntegration",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAttendanceIntegration_IntegrationStatus_LeaveDate",
                schema: "public",
                table: "TrxLeaveAttendanceIntegration",
                columns: new[] { "IntegrationStatus", "LeaveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAttendanceIntegration_LeaveExecutionId",
                schema: "public",
                table: "TrxLeaveAttendanceIntegration",
                column: "LeaveExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAttendanceIntegration_LeaveRequestId_LeaveDate",
                schema: "public",
                table: "TrxLeaveAttendanceIntegration",
                columns: new[] { "LeaveRequestId", "LeaveDate" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAttendanceIntegration_LeaveTypeId",
                schema: "public",
                table: "TrxLeaveAttendanceIntegration",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAttendanceIntegration_ReversedByUserId",
                schema: "public",
                table: "TrxLeaveAttendanceIntegration",
                column: "ReversedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAttendanceIntegration_WorkforceProfileId_LeaveDate",
                schema: "public",
                table: "TrxLeaveAttendanceIntegration",
                columns: new[] { "WorkforceProfileId", "LeaveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveExecution_CompletedByUserId",
                schema: "public",
                table: "TrxLeaveExecution",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveExecution_ExecutionNumber",
                schema: "public",
                table: "TrxLeaveExecution",
                column: "ExecutionNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveExecution_ExecutionStatus_StartDate_EndDate",
                schema: "public",
                table: "TrxLeaveExecution",
                columns: new[] { "ExecutionStatus", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveExecution_IdempotencyKey",
                schema: "public",
                table: "TrxLeaveExecution",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveExecution_LeaveBalanceId",
                schema: "public",
                table: "TrxLeaveExecution",
                column: "LeaveBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveExecution_LeaveRequestId",
                schema: "public",
                table: "TrxLeaveExecution",
                column: "LeaveRequestId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveExecution_LeaveTypeId",
                schema: "public",
                table: "TrxLeaveExecution",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveExecution_ReversedByUserId",
                schema: "public",
                table: "TrxLeaveExecution",
                column: "ReversedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveExecution_StartedByUserId",
                schema: "public",
                table: "TrxLeaveExecution",
                column: "StartedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveExecution_WorkforceProfileId_StartDate_EndDate",
                schema: "public",
                table: "TrxLeaveExecution",
                columns: new[] { "WorkforceProfileId", "StartDate", "EndDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxLeaveAttendanceIntegration",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxLeaveExecution",
                schema: "public");
        }
    }
}
