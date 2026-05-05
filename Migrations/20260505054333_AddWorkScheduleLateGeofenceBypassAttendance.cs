using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddWorkScheduleLateGeofenceBypassAttendance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttendanceStatus",
                schema: "public",
                table: "EmpAttendance",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Present");

            migrationBuilder.AddColumn<int>(
                name: "CheckInToleranceMinutes",
                schema: "public",
                table: "EmpAttendance",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GeofenceBypassReason",
                schema: "public",
                table: "EmpAttendance",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGeofenceBypassed",
                schema: "public",
                table: "EmpAttendance",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLate",
                schema: "public",
                table: "EmpAttendance",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LateMinutes",
                schema: "public",
                table: "EmpAttendance",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "WorkEndTime",
                schema: "public",
                table: "EmpAttendance",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkScheduleId",
                schema: "public",
                table: "EmpAttendance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "WorkStartTime",
                schema: "public",
                table: "EmpAttendance",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeolocationBypassReason",
                table: "AspNetUsers",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GeolocationBypassUntil",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGeolocationBypassEnabled",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MstWorkSchedule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScheduleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserType = table.Column<int>(type: "integer", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkStartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    WorkEndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CheckInToleranceMinutes = table.Column<int>(type: "integer", nullable: false),
                    CheckOutToleranceMinutes = table.Column<int>(type: "integer", nullable: false),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstWorkSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstWorkSchedule_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstWorkSchedule_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstWorkSchedule_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_AttendanceStatus",
                schema: "public",
                table: "EmpAttendance",
                column: "AttendanceStatus");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_IsGeofenceBypassed",
                schema: "public",
                table: "EmpAttendance",
                column: "IsGeofenceBypassed");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_IsLate",
                schema: "public",
                table: "EmpAttendance",
                column: "IsLate");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_WorkScheduleId",
                schema: "public",
                table: "EmpAttendance",
                column: "WorkScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsGeolocationBypassEnabled",
                table: "AspNetUsers",
                column: "IsGeolocationBypassEnabled");

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
                name: "IX_MstWorkSchedule_ScheduleCode",
                schema: "public",
                table: "MstWorkSchedule",
                column: "ScheduleCode",
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_EmpAttendance_AspNetUsers_UserId",
                schema: "public",
                table: "EmpAttendance",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmpAttendance_MstWorkSchedule_WorkScheduleId",
                schema: "public",
                table: "EmpAttendance",
                column: "WorkScheduleId",
                principalSchema: "public",
                principalTable: "MstWorkSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmpAttendance_AspNetUsers_UserId",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmpAttendance_MstWorkSchedule_WorkScheduleId",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropTable(
                name: "MstWorkSchedule",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_EmpAttendance_AttendanceStatus",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropIndex(
                name: "IX_EmpAttendance_IsGeofenceBypassed",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropIndex(
                name: "IX_EmpAttendance_IsLate",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropIndex(
                name: "IX_EmpAttendance_WorkScheduleId",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IsGeolocationBypassEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AttendanceStatus",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "CheckInToleranceMinutes",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "GeofenceBypassReason",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "IsGeofenceBypassed",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "IsLate",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "LateMinutes",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "WorkEndTime",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "WorkScheduleId",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "WorkStartTime",
                schema: "public",
                table: "EmpAttendance");

            migrationBuilder.DropColumn(
                name: "GeolocationBypassReason",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GeolocationBypassUntil",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsGeolocationBypassEnabled",
                table: "AspNetUsers");
        }
    }
}
