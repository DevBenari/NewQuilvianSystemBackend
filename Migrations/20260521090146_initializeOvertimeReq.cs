using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeOvertimeReq : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WfpOvertimeRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkScheduleAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    OvertimeDate = table.Column<DateTime>(type: "date", nullable: false),
                    ScheduledStartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    ScheduledEndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    ActualCheckInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualCheckOutAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    IsOvernight = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TotalMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApprovalStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsPayrollProcessed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PayrollProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PayrollProcessedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayrollPeriodCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AttachmentPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AttachmentContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_WfpOvertimeRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpOvertimeRequest_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOvertimeRequest_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOvertimeRequest_AspNetUsers_PayrollProcessedByUserId",
                        column: x => x.PayrollProcessedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOvertimeRequest_AspNetUsers_RejectedByUserId",
                        column: x => x.RejectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOvertimeRequest_EmpAttendance_AttendanceId",
                        column: x => x.AttendanceId,
                        principalSchema: "public",
                        principalTable: "EmpAttendance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOvertimeRequest_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOvertimeRequest_MstWorkSchedule_WorkScheduleId",
                        column: x => x.WorkScheduleId,
                        principalSchema: "public",
                        principalTable: "MstWorkSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOvertimeRequest_WfpWorkScheduleAssignment_WorkScheduleAs~",
                        column: x => x.WorkScheduleAssignmentId,
                        principalSchema: "public",
                        principalTable: "WfpWorkScheduleAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_ApprovalStatus_IsPayrollProcessed_IsActi~",
                schema: "public",
                table: "WfpOvertimeRequest",
                columns: new[] { "ApprovalStatus", "IsPayrollProcessed", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_ApprovedByUserId",
                schema: "public",
                table: "WfpOvertimeRequest",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_AttendanceId",
                schema: "public",
                table: "WfpOvertimeRequest",
                column: "AttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_AttendanceId_IsDelete",
                schema: "public",
                table: "WfpOvertimeRequest",
                columns: new[] { "AttendanceId", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_CancelledByUserId",
                schema: "public",
                table: "WfpOvertimeRequest",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_PayrollPeriodCode_IsPayrollProcessed_IsD~",
                schema: "public",
                table: "WfpOvertimeRequest",
                columns: new[] { "PayrollPeriodCode", "IsPayrollProcessed", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_PayrollProcessedByUserId",
                schema: "public",
                table: "WfpOvertimeRequest",
                column: "PayrollProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_RejectedByUserId",
                schema: "public",
                table: "WfpOvertimeRequest",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_WorkforceProfileId",
                schema: "public",
                table: "WfpOvertimeRequest",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_WorkforceProfileId_OvertimeDate_Approval~",
                schema: "public",
                table: "WfpOvertimeRequest",
                columns: new[] { "WorkforceProfileId", "OvertimeDate", "ApprovalStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_WorkforceProfileId_OvertimeDate_StartTim~",
                schema: "public",
                table: "WfpOvertimeRequest",
                columns: new[] { "WorkforceProfileId", "OvertimeDate", "StartTime", "EndTime" },
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_WorkScheduleAssignmentId",
                schema: "public",
                table: "WfpOvertimeRequest",
                column: "WorkScheduleAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_WorkScheduleId",
                schema: "public",
                table: "WfpOvertimeRequest",
                column: "WorkScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_WorkScheduleId_OvertimeDate_IsDelete",
                schema: "public",
                table: "WfpOvertimeRequest",
                columns: new[] { "WorkScheduleId", "OvertimeDate", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WfpOvertimeRequest",
                schema: "public");
        }
    }
}
