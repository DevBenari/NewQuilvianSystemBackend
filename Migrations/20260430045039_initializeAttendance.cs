using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeAttendance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                schema: "public",
                table: "MstEmployee",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                schema: "public",
                table: "MstDoctor",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "EmpAttendance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckOutAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckInLatitude = table.Column<double>(type: "double precision", nullable: false),
                    CheckInLongitude = table.Column<double>(type: "double precision", nullable: false),
                    CheckInAccuracyMeters = table.Column<double>(type: "double precision", nullable: true),
                    CheckInDistanceMeters = table.Column<double>(type: "double precision", nullable: false),
                    CheckOutLatitude = table.Column<double>(type: "double precision", nullable: true),
                    CheckOutLongitude = table.Column<double>(type: "double precision", nullable: true),
                    CheckOutAccuracyMeters = table.Column<double>(type: "double precision", nullable: true),
                    CheckOutDistanceMeters = table.Column<double>(type: "double precision", nullable: true),
                    WorkDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    PersonType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CheckInSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Login"),
                    CheckOutSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "CheckedIn"),
                    CheckInIpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CheckOutIpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CheckInUserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CheckOutUserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_EmpAttendance", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_AttendanceDate",
                schema: "public",
                table: "EmpAttendance",
                column: "AttendanceDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_DoctorId",
                schema: "public",
                table: "EmpAttendance",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_EmployeeId",
                schema: "public",
                table: "EmpAttendance",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_Status",
                schema: "public",
                table: "EmpAttendance",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmpAttendance_UserId_AttendanceDate",
                schema: "public",
                table: "EmpAttendance",
                columns: new[] { "UserId", "AttendanceDate" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpAttendance",
                schema: "public");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                schema: "public",
                table: "MstEmployee",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                schema: "public",
                table: "MstDoctor",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);
        }
    }
}
