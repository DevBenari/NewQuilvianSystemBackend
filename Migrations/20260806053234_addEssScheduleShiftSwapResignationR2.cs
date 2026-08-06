using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class addEssScheduleShiftSwapResignationR2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxResignationRequest_EmployeeId",
                schema: "public",
                table: "TrxResignationRequest");

            migrationBuilder.DropIndex(
                name: "IX_TrxResignationRequest_EmployeeSeparationId",
                schema: "public",
                table: "TrxResignationRequest");

            migrationBuilder.DropIndex(
                name: "IX_TrxResignationRequest_WorkforceProfileId_RequestStatus_Prop~",
                schema: "public",
                table: "TrxResignationRequest");

            migrationBuilder.AlterColumn<string>(
                name: "RequestStatus",
                schema: "public",
                table: "WfpShiftSwapRequest",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Draft",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "PendingTarget");

            migrationBuilder.AlterColumn<string>(
                name: "RequestStatus",
                schema: "public",
                table: "TrxResignationRequest",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Draft",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDate",
                schema: "public",
                table: "TrxResignationRequest",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProposedLastWorkingDate",
                schema: "public",
                table: "TrxResignationRequest",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.CreateIndex(
                name: "IX_WfpShiftSwapRequest_WorkflowInstanceId",
                schema: "public",
                table: "WfpShiftSwapRequest",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpScheduleChangeRequest_WorkflowInstanceId",
                schema: "public",
                table: "WfpScheduleChangeRequest",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxResignationRequest_EmployeeId_RequestStatus_IsDelete",
                schema: "public",
                table: "TrxResignationRequest",
                columns: new[] { "EmployeeId", "RequestStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxResignationRequest_EmployeeSeparationId",
                schema: "public",
                table: "TrxResignationRequest",
                column: "EmployeeSeparationId",
                unique: true,
                filter: "\"EmployeeSeparationId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxResignationRequest_WorkflowInstanceId",
                schema: "public",
                table: "TrxResignationRequest",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxResignationRequest_WorkforceProfileId_RequestStatus_IsDe~",
                schema: "public",
                table: "TrxResignationRequest",
                columns: new[] { "WorkforceProfileId", "RequestStatus", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WfpShiftSwapRequest_WorkflowInstanceId",
                schema: "public",
                table: "WfpShiftSwapRequest");

            migrationBuilder.DropIndex(
                name: "IX_WfpScheduleChangeRequest_WorkflowInstanceId",
                schema: "public",
                table: "WfpScheduleChangeRequest");

            migrationBuilder.DropIndex(
                name: "IX_TrxResignationRequest_EmployeeId_RequestStatus_IsDelete",
                schema: "public",
                table: "TrxResignationRequest");

            migrationBuilder.DropIndex(
                name: "IX_TrxResignationRequest_EmployeeSeparationId",
                schema: "public",
                table: "TrxResignationRequest");

            migrationBuilder.DropIndex(
                name: "IX_TrxResignationRequest_WorkflowInstanceId",
                schema: "public",
                table: "TrxResignationRequest");

            migrationBuilder.DropIndex(
                name: "IX_TrxResignationRequest_WorkforceProfileId_RequestStatus_IsDe~",
                schema: "public",
                table: "TrxResignationRequest");

            migrationBuilder.AlterColumn<string>(
                name: "RequestStatus",
                schema: "public",
                table: "WfpShiftSwapRequest",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "PendingTarget",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Draft");

            migrationBuilder.AlterColumn<string>(
                name: "RequestStatus",
                schema: "public",
                table: "TrxResignationRequest",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Draft");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDate",
                schema: "public",
                table: "TrxResignationRequest",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProposedLastWorkingDate",
                schema: "public",
                table: "TrxResignationRequest",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_TrxResignationRequest_EmployeeId",
                schema: "public",
                table: "TrxResignationRequest",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxResignationRequest_EmployeeSeparationId",
                schema: "public",
                table: "TrxResignationRequest",
                column: "EmployeeSeparationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxResignationRequest_WorkforceProfileId_RequestStatus_Prop~",
                schema: "public",
                table: "TrxResignationRequest",
                columns: new[] { "WorkforceProfileId", "RequestStatus", "ProposedLastWorkingDate" });
        }
    }
}
