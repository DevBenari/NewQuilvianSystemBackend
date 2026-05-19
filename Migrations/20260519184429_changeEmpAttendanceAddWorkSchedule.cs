using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeEmpAttendanceAddWorkSchedule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstWorkSchedule_AspNetUsers_UserId",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_MstWorkSchedule_MstDepartment_DepartmentId",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_MstWorkSchedule_MstPosition_PositionId",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkSchedule_DepartmentId_PositionId",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkSchedule_IsActive",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkSchedule_IsDefault",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkSchedule_PositionId",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkSchedule_UserId",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkSchedule_UserType",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_EmpAttendance_UserId_AttendanceDate",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropColumn(
                name: "EffectiveEndDate",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropColumn(
                name: "EffectiveStartDate",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropColumn(
                name: "PositionId",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropColumn(
                name: "UserType",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "MstWorkSchedule",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "MstWorkSchedule",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "MstWorkSchedule",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "CheckOutToleranceMinutes",
                schema: "public",
                table: "MstWorkSchedule",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "CheckInToleranceMinutes",
                schema: "public",
                table: "MstWorkSchedule",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "IsOvernight",
                schema: "public",
                table: "MstWorkSchedule",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ScheduleType",
                schema: "public",
                table: "MstWorkSchedule",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Shift");

            migrationBuilder.AlterColumn<int>(
                name: "LateMinutes",
                schema: "public",
                table: "EmpAttendance",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsLate",
                schema: "public",
                table: "EmpAttendance",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "EmpAttendance",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "EmpAttendance",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "EmpAttendance",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "CheckInToleranceMinutes",
                schema: "public",
                table: "EmpAttendance",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "CheckOutToleranceMinutes",
                schema: "public",
                table: "EmpAttendance",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsOvernightSchedule",
                schema: "public",
                table: "EmpAttendance",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledCheckInAt",
                schema: "public",
                table: "EmpAttendance",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledCheckOutAt",
                schema: "public",
                table: "EmpAttendance",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkScheduleAssignmentId",
                schema: "public",
                table: "EmpAttendance",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WfpWorkScheduleAssignment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsOffDay = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsOvertimePlanned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsOnCall = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_WfpWorkScheduleAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpWorkScheduleAssignment_MstWorkforceProfile_WorkforceProf~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpWorkScheduleAssignment_MstWorkSchedule_WorkScheduleId",
                        column: x => x.WorkScheduleId,
                        principalSchema: "public",
                        principalTable: "MstWorkSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_IsDefault_IsActive_IsDelete",
                schema: "public",
                table: "MstWorkSchedule",
                columns: new[] { "IsDefault", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_ScheduleName",
                schema: "public",
                table: "MstWorkSchedule",
                column: "ScheduleName");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_ScheduleType",
                schema: "public",
                table: "MstWorkSchedule",
                column: "ScheduleType");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_ScheduleType_IsActive_IsDelete",
                schema: "public",
                table: "MstWorkSchedule",
                columns: new[] { "ScheduleType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_IsOvernightSchedule",
                schema: "public",
                table: "EmpAttendance",
                column: "IsOvernightSchedule");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_UserId_AttendanceDate",
                schema: "public",
                table: "EmpAttendance",
                columns: new[] { "UserId", "AttendanceDate" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_WorkforceProfileId_AttendanceDate_IsDelete",
                schema: "public",
                table: "EmpAttendance",
                columns: new[] { "WorkforceProfileId", "AttendanceDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_WorkScheduleAssignmentId",
                schema: "public",
                table: "EmpAttendance",
                column: "WorkScheduleAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_WorkScheduleAssignmentId_AttendanceDate_IsDel~",
                schema: "public",
                table: "EmpAttendance",
                columns: new[] { "WorkScheduleAssignmentId", "AttendanceDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_WorkScheduleId_AttendanceDate_IsDelete",
                schema: "public",
                table: "EmpAttendance",
                columns: new[] { "WorkScheduleId", "AttendanceDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpWorkScheduleAssignment_ScheduleDate",
                schema: "public",
                table: "WfpWorkScheduleAssignment",
                column: "ScheduleDate");

            migrationBuilder.CreateIndex(
                name: "IX_WfpWorkScheduleAssignment_WorkforceProfileId",
                schema: "public",
                table: "WfpWorkScheduleAssignment",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpWorkScheduleAssignment_WorkforceProfileId_ScheduleDate",
                schema: "public",
                table: "WfpWorkScheduleAssignment",
                columns: new[] { "WorkforceProfileId", "ScheduleDate" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpWorkScheduleAssignment_WorkforceProfileId_ScheduleDate_I~",
                schema: "public",
                table: "WfpWorkScheduleAssignment",
                columns: new[] { "WorkforceProfileId", "ScheduleDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpWorkScheduleAssignment_WorkScheduleId",
                schema: "public",
                table: "WfpWorkScheduleAssignment",
                column: "WorkScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpWorkScheduleAssignment_WorkScheduleId_ScheduleDate_IsAct~",
                schema: "public",
                table: "WfpWorkScheduleAssignment",
                columns: new[] { "WorkScheduleId", "ScheduleDate", "IsActive", "IsDelete" });

            migrationBuilder.AddForeignKey(
                name: "FK_EmpAttendance_WfpWorkScheduleAssignment_WorkScheduleAssignm~",
                schema: "public",
                table: "EmpAttendance",
                column: "WorkScheduleAssignmentId",
                principalSchema: "public",
                principalTable: "WfpWorkScheduleAssignment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmpAttendance_WfpWorkScheduleAssignment_WorkScheduleAssignm~",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropTable(
                name: "WfpWorkScheduleAssignment",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkSchedule_IsDefault_IsActive_IsDelete",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkSchedule_ScheduleName",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkSchedule_ScheduleType",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_MstWorkSchedule_ScheduleType_IsActive_IsDelete",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_EmpAttendance_IsOvernightSchedule",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropIndex(
                name: "IX_EmpAttendance_UserId_AttendanceDate",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropIndex(
                name: "IX_EmpAttendance_WorkforceProfileId_AttendanceDate_IsDelete",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropIndex(
                name: "IX_EmpAttendance_WorkScheduleAssignmentId",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropIndex(
                name: "IX_EmpAttendance_WorkScheduleAssignmentId_AttendanceDate_IsDel~",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropIndex(
                name: "IX_EmpAttendance_WorkScheduleId_AttendanceDate_IsDelete",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "IsOvernight",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropColumn(
                name: "ScheduleType",
                schema: "public",
                table: "MstWorkSchedule");

            migrationBuilder.DropColumn(
                name: "CheckOutToleranceMinutes",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "IsOvernightSchedule",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "ScheduledCheckInAt",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "ScheduledCheckOutAt",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "WorkScheduleAssignmentId",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "MstWorkSchedule",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "MstWorkSchedule",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "MstWorkSchedule",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<int>(
                name: "CheckOutToleranceMinutes",
                schema: "public",
                table: "MstWorkSchedule",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "CheckInToleranceMinutes",
                schema: "public",
                table: "MstWorkSchedule",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                schema: "public",
                table: "MstWorkSchedule",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveEndDate",
                schema: "public",
                table: "MstWorkSchedule",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveStartDate",
                schema: "public",
                table: "MstWorkSchedule",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                schema: "public",
                table: "MstWorkSchedule",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "public",
                table: "MstWorkSchedule",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserType",
                schema: "public",
                table: "MstWorkSchedule",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LateMinutes",
                schema: "public",
                table: "EmpAttendance",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsLate",
                schema: "public",
                table: "EmpAttendance",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "EmpAttendance",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "EmpAttendance",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "EmpAttendance",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<int>(
                name: "CheckInToleranceMinutes",
                schema: "public",
                table: "EmpAttendance",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_DepartmentId_PositionId",
                schema: "public",
                table: "MstWorkSchedule",
                columns: new[] { "DepartmentId", "PositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_IsActive",
                schema: "public",
                table: "MstWorkSchedule",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_IsDefault",
                schema: "public",
                table: "MstWorkSchedule",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_PositionId",
                schema: "public",
                table: "MstWorkSchedule",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_UserId",
                schema: "public",
                table: "MstWorkSchedule",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MstWorkSchedule_UserType",
                schema: "public",
                table: "MstWorkSchedule",
                column: "UserType");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_UserId_AttendanceDate",
                schema: "public",
                table: "EmpAttendance",
                columns: new[] { "UserId", "AttendanceDate" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MstWorkSchedule_AspNetUsers_UserId",
                schema: "public",
                table: "MstWorkSchedule",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstWorkSchedule_MstDepartment_DepartmentId",
                schema: "public",
                table: "MstWorkSchedule",
                column: "DepartmentId",
                principalSchema: "public",
                principalTable: "MstDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstWorkSchedule_MstPosition_PositionId",
                schema: "public",
                table: "MstWorkSchedule",
                column: "PositionId",
                principalSchema: "public",
                principalTable: "MstPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
