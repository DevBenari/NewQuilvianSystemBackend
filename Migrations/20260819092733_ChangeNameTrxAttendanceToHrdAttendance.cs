using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameTrxAttendanceToHrdAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxAttendanceCorrectionRequest_TrxAttendance_AttendanceId",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxAttendanceRawLog_TrxAttendance_ProcessedAttendanceId",
                schema: "public",
                table: "TrxAttendanceRawLog");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxOvertimeRealizationDetail_TrxAttendance_AttendanceId",
                schema: "public",
                table: "TrxOvertimeRealizationDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxOvertimeRequestDetail_TrxAttendance_AttendanceId",
                schema: "public",
                table: "TrxOvertimeRequestDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpOvertimeRequest_TrxAttendance_AttendanceId",
                schema: "public",
                table: "WfpOvertimeRequest");

            // Rename physical table - data tetap berada di tabel yang sama.
            migrationBuilder.RenameTable(
                name: "TrxAttendance",
                schema: "public",
                newName: "HrdAttendance",
                newSchema: "public");

            // Rename PK + FK yang dimiliki oleh Attendance.
            var constraintRenames = new (string OldName, string NewName)[]
            {
        ("PK_TrxAttendance", "PK_HrdAttendance"),

        ("FK_TrxAttendance_AspNetUsers_UserId",
         "FK_HrdAttendance_AspNetUsers_UserId"),

        ("FK_TrxAttendance_MstAttendanceDevice_CheckInDeviceId",
         "FK_HrdAttendance_MstAttendanceDevice_CheckInDeviceId"),

        ("FK_TrxAttendance_MstAttendanceDevice_CheckOutDeviceId",
         "FK_HrdAttendance_MstAttendanceDevice_CheckOutDeviceId"),

        ("FK_TrxAttendance_MstAttendanceLocation_AttendanceLocationId",
         "FK_HrdAttendance_MstAttendanceLocation_AttendanceLocationId"),

        ("FK_TrxAttendance_MstAttendancePolicy_AttendancePolicyId",
         "FK_HrdAttendance_MstAttendancePolicy_AttendancePolicyId"),

        ("FK_TrxAttendance_MstDepartment_DepartmentId",
         "FK_HrdAttendance_MstDepartment_DepartmentId"),

        ("FK_TrxAttendance_MstDoctor_DoctorId",
         "FK_HrdAttendance_MstDoctor_DoctorId"),

        ("FK_TrxAttendance_MstEmployee_EmployeeId",
         "FK_HrdAttendance_MstEmployee_EmployeeId"),

        ("FK_TrxAttendance_MstGracePeriodPolicy_GracePeriodPolicyId",
         "FK_HrdAttendance_MstGracePeriodPolicy_GracePeriodPolicyId"),

        ("FK_TrxAttendance_MstHospitalSite_HospitalSiteId",
         "FK_HrdAttendance_MstHospitalSite_HospitalSiteId"),

        ("FK_TrxAttendance_MstOrganizationUnit_OrganizationUnitId",
         "FK_HrdAttendance_MstOrganizationUnit_OrganizationUnitId"),

        ("FK_TrxAttendance_MstShift_ShiftId",
         "FK_HrdAttendance_MstShift_ShiftId"),

        ("FK_TrxAttendance_MstWorkLocation_WorkLocationId",
         "FK_HrdAttendance_MstWorkLocation_WorkLocationId"),

        ("FK_TrxAttendance_MstWorkSchedule_WorkScheduleId",
         "FK_HrdAttendance_MstWorkSchedule_WorkScheduleId"),

        ("FK_TrxAttendance_MstWorkforceProfile_WorkforceProfileId",
         "FK_HrdAttendance_MstWorkforceProfile_WorkforceProfileId"),

        ("FK_TrxAttendance_TrxAttendanceDaily_AttendanceDailyId",
         "FK_HrdAttendance_TrxAttendanceDaily_AttendanceDailyId"),

        ("FK_TrxAttendance_WfpOrganizationAssignment_OrganizationAssignm~",
         "FK_HrdAttendance_WfpOrganizationAssignment_OrganizationAssignm~"),

        ("FK_TrxAttendance_WfpWorkScheduleAssignment_WorkScheduleAssignm~",
         "FK_HrdAttendance_WfpWorkScheduleAssignment_WorkScheduleAssignm~")
            };

            foreach (var (oldName, newName) in constraintRenames)
            {
                migrationBuilder.Sql(
                    $@"ALTER TABLE public.""HrdAttendance""
               RENAME CONSTRAINT ""{oldName}"" TO ""{newName}"";");
            }

            // Rename seluruh index Attendance.
            var indexRenames = new (string OldName, string NewName)[]
            {
        ("IX_TrxAttendance_AttendanceDailyId",
         "IX_HrdAttendance_AttendanceDailyId"),

        ("IX_TrxAttendance_AttendanceLocationId",
         "IX_HrdAttendance_AttendanceLocationId"),

        ("IX_TrxAttendance_AttendancePolicyId",
         "IX_HrdAttendance_AttendancePolicyId"),

        ("IX_TrxAttendance_AttendanceStatus_AttendanceDate",
         "IX_HrdAttendance_AttendanceStatus_AttendanceDate"),

        ("IX_TrxAttendance_CheckInDeviceId",
         "IX_HrdAttendance_CheckInDeviceId"),

        ("IX_TrxAttendance_CheckOutDeviceId",
         "IX_HrdAttendance_CheckOutDeviceId"),

        ("IX_TrxAttendance_DepartmentId",
         "IX_HrdAttendance_DepartmentId"),

        ("IX_TrxAttendance_DoctorId",
         "IX_HrdAttendance_DoctorId"),

        ("IX_TrxAttendance_EmployeeId",
         "IX_HrdAttendance_EmployeeId"),

        ("IX_TrxAttendance_GracePeriodPolicyId",
         "IX_HrdAttendance_GracePeriodPolicyId"),

        ("IX_TrxAttendance_HospitalSiteId",
         "IX_HrdAttendance_HospitalSiteId"),

        ("IX_TrxAttendance_OrganizationAssignmentId",
         "IX_HrdAttendance_OrganizationAssignmentId"),

        ("IX_TrxAttendance_OrganizationUnitId",
         "IX_HrdAttendance_OrganizationUnitId"),

        ("IX_TrxAttendance_ShiftId",
         "IX_HrdAttendance_ShiftId"),

        ("IX_TrxAttendance_Status_IsProcessed_AttendanceDate",
         "IX_HrdAttendance_Status_IsProcessed_AttendanceDate"),

        ("IX_TrxAttendance_UserId_AttendanceDate",
         "IX_HrdAttendance_UserId_AttendanceDate"),

        ("IX_TrxAttendance_WorkforceProfileId_AttendanceDate",
         "IX_HrdAttendance_WorkforceProfileId_AttendanceDate"),

        ("IX_TrxAttendance_WorkLocationId",
         "IX_HrdAttendance_WorkLocationId"),

        ("IX_TrxAttendance_WorkScheduleAssignmentId",
         "IX_HrdAttendance_WorkScheduleAssignmentId"),

        ("IX_TrxAttendance_WorkScheduleId",
         "IX_HrdAttendance_WorkScheduleId")
            };

            foreach (var (oldName, newName) in indexRenames)
            {
                migrationBuilder.RenameIndex(
                    name: oldName,
                    schema: "public",
                    table: "HrdAttendance",
                    newName: newName);
            }

            // Re-create inbound FK dengan nama physical baru.
            migrationBuilder.AddForeignKey(
                name: "FK_TrxAttendanceCorrectionRequest_HrdAttendance_AttendanceId",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest",
                column: "AttendanceId",
                principalSchema: "public",
                principalTable: "HrdAttendance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxAttendanceRawLog_HrdAttendance_ProcessedAttendanceId",
                schema: "public",
                table: "TrxAttendanceRawLog",
                column: "ProcessedAttendanceId",
                principalSchema: "public",
                principalTable: "HrdAttendance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxOvertimeRealizationDetail_HrdAttendance_AttendanceId",
                schema: "public",
                table: "TrxOvertimeRealizationDetail",
                column: "AttendanceId",
                principalSchema: "public",
                principalTable: "HrdAttendance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxOvertimeRequestDetail_HrdAttendance_AttendanceId",
                schema: "public",
                table: "TrxOvertimeRequestDetail",
                column: "AttendanceId",
                principalSchema: "public",
                principalTable: "HrdAttendance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpOvertimeRequest_HrdAttendance_AttendanceId",
                schema: "public",
                table: "WfpOvertimeRequest",
                column: "AttendanceId",
                principalSchema: "public",
                principalTable: "HrdAttendance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxAttendanceCorrectionRequest_HrdAttendance_AttendanceId",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxAttendanceRawLog_HrdAttendance_ProcessedAttendanceId",
                schema: "public",
                table: "TrxAttendanceRawLog");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxOvertimeRealizationDetail_HrdAttendance_AttendanceId",
                schema: "public",
                table: "TrxOvertimeRealizationDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxOvertimeRequestDetail_HrdAttendance_AttendanceId",
                schema: "public",
                table: "TrxOvertimeRequestDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpOvertimeRequest_HrdAttendance_AttendanceId",
                schema: "public",
                table: "WfpOvertimeRequest");

            var indexRenames = new (string OldName, string NewName)[]
            {
        ("IX_HrdAttendance_AttendanceDailyId",
         "IX_TrxAttendance_AttendanceDailyId"),

        ("IX_HrdAttendance_AttendanceLocationId",
         "IX_TrxAttendance_AttendanceLocationId"),

        ("IX_HrdAttendance_AttendancePolicyId",
         "IX_TrxAttendance_AttendancePolicyId"),

        ("IX_HrdAttendance_AttendanceStatus_AttendanceDate",
         "IX_TrxAttendance_AttendanceStatus_AttendanceDate"),

        ("IX_HrdAttendance_CheckInDeviceId",
         "IX_TrxAttendance_CheckInDeviceId"),

        ("IX_HrdAttendance_CheckOutDeviceId",
         "IX_TrxAttendance_CheckOutDeviceId"),

        ("IX_HrdAttendance_DepartmentId",
         "IX_TrxAttendance_DepartmentId"),

        ("IX_HrdAttendance_DoctorId",
         "IX_TrxAttendance_DoctorId"),

        ("IX_HrdAttendance_EmployeeId",
         "IX_TrxAttendance_EmployeeId"),

        ("IX_HrdAttendance_GracePeriodPolicyId",
         "IX_TrxAttendance_GracePeriodPolicyId"),

        ("IX_HrdAttendance_HospitalSiteId",
         "IX_TrxAttendance_HospitalSiteId"),

        ("IX_HrdAttendance_OrganizationAssignmentId",
         "IX_TrxAttendance_OrganizationAssignmentId"),

        ("IX_HrdAttendance_OrganizationUnitId",
         "IX_TrxAttendance_OrganizationUnitId"),

        ("IX_HrdAttendance_ShiftId",
         "IX_TrxAttendance_ShiftId"),

        ("IX_HrdAttendance_Status_IsProcessed_AttendanceDate",
         "IX_TrxAttendance_Status_IsProcessed_AttendanceDate"),

        ("IX_HrdAttendance_UserId_AttendanceDate",
         "IX_TrxAttendance_UserId_AttendanceDate"),

        ("IX_HrdAttendance_WorkforceProfileId_AttendanceDate",
         "IX_TrxAttendance_WorkforceProfileId_AttendanceDate"),

        ("IX_HrdAttendance_WorkLocationId",
         "IX_TrxAttendance_WorkLocationId"),

        ("IX_HrdAttendance_WorkScheduleAssignmentId",
         "IX_TrxAttendance_WorkScheduleAssignmentId"),

        ("IX_HrdAttendance_WorkScheduleId",
         "IX_TrxAttendance_WorkScheduleId")
            };

            foreach (var (oldName, newName) in indexRenames)
            {
                migrationBuilder.RenameIndex(
                    name: oldName,
                    schema: "public",
                    table: "HrdAttendance",
                    newName: newName);
            }

            var constraintRenames = new (string OldName, string NewName)[]
            {
        ("PK_HrdAttendance", "PK_TrxAttendance"),

        ("FK_HrdAttendance_AspNetUsers_UserId",
         "FK_TrxAttendance_AspNetUsers_UserId"),

        ("FK_HrdAttendance_MstAttendanceDevice_CheckInDeviceId",
         "FK_TrxAttendance_MstAttendanceDevice_CheckInDeviceId"),

        ("FK_HrdAttendance_MstAttendanceDevice_CheckOutDeviceId",
         "FK_TrxAttendance_MstAttendanceDevice_CheckOutDeviceId"),

        ("FK_HrdAttendance_MstAttendanceLocation_AttendanceLocationId",
         "FK_TrxAttendance_MstAttendanceLocation_AttendanceLocationId"),

        ("FK_HrdAttendance_MstAttendancePolicy_AttendancePolicyId",
         "FK_TrxAttendance_MstAttendancePolicy_AttendancePolicyId"),

        ("FK_HrdAttendance_MstDepartment_DepartmentId",
         "FK_TrxAttendance_MstDepartment_DepartmentId"),

        ("FK_HrdAttendance_MstDoctor_DoctorId",
         "FK_TrxAttendance_MstDoctor_DoctorId"),

        ("FK_HrdAttendance_MstEmployee_EmployeeId",
         "FK_TrxAttendance_MstEmployee_EmployeeId"),

        ("FK_HrdAttendance_MstGracePeriodPolicy_GracePeriodPolicyId",
         "FK_TrxAttendance_MstGracePeriodPolicy_GracePeriodPolicyId"),

        ("FK_HrdAttendance_MstHospitalSite_HospitalSiteId",
         "FK_TrxAttendance_MstHospitalSite_HospitalSiteId"),

        ("FK_HrdAttendance_MstOrganizationUnit_OrganizationUnitId",
         "FK_TrxAttendance_MstOrganizationUnit_OrganizationUnitId"),

        ("FK_HrdAttendance_MstShift_ShiftId",
         "FK_TrxAttendance_MstShift_ShiftId"),

        ("FK_HrdAttendance_MstWorkLocation_WorkLocationId",
         "FK_TrxAttendance_MstWorkLocation_WorkLocationId"),

        ("FK_HrdAttendance_MstWorkSchedule_WorkScheduleId",
         "FK_TrxAttendance_MstWorkSchedule_WorkScheduleId"),

        ("FK_HrdAttendance_MstWorkforceProfile_WorkforceProfileId",
         "FK_TrxAttendance_MstWorkforceProfile_WorkforceProfileId"),

        ("FK_HrdAttendance_TrxAttendanceDaily_AttendanceDailyId",
         "FK_TrxAttendance_TrxAttendanceDaily_AttendanceDailyId"),

        ("FK_HrdAttendance_WfpOrganizationAssignment_OrganizationAssignm~",
         "FK_TrxAttendance_WfpOrganizationAssignment_OrganizationAssignm~"),

        ("FK_HrdAttendance_WfpWorkScheduleAssignment_WorkScheduleAssignm~",
         "FK_TrxAttendance_WfpWorkScheduleAssignment_WorkScheduleAssignm~")
            };

            foreach (var (oldName, newName) in constraintRenames)
            {
                migrationBuilder.Sql(
                    $@"ALTER TABLE public.""HrdAttendance""
               RENAME CONSTRAINT ""{oldName}"" TO ""{newName}"";");
            }

            migrationBuilder.RenameTable(
                name: "HrdAttendance",
                schema: "public",
                newName: "TrxAttendance",
                newSchema: "public");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxAttendanceCorrectionRequest_TrxAttendance_AttendanceId",
                schema: "public",
                table: "TrxAttendanceCorrectionRequest",
                column: "AttendanceId",
                principalSchema: "public",
                principalTable: "TrxAttendance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxAttendanceRawLog_TrxAttendance_ProcessedAttendanceId",
                schema: "public",
                table: "TrxAttendanceRawLog",
                column: "ProcessedAttendanceId",
                principalSchema: "public",
                principalTable: "TrxAttendance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxOvertimeRealizationDetail_TrxAttendance_AttendanceId",
                schema: "public",
                table: "TrxOvertimeRealizationDetail",
                column: "AttendanceId",
                principalSchema: "public",
                principalTable: "TrxAttendance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxOvertimeRequestDetail_TrxAttendance_AttendanceId",
                schema: "public",
                table: "TrxOvertimeRequestDetail",
                column: "AttendanceId",
                principalSchema: "public",
                principalTable: "TrxAttendance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpOvertimeRequest_TrxAttendance_AttendanceId",
                schema: "public",
                table: "WfpOvertimeRequest",
                column: "AttendanceId",
                principalSchema: "public",
                principalTable: "TrxAttendance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
