using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class addOvertimeRequestAndPlanningFoundation4A3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestSource",
                schema: "public",
                table: "WfpOvertimeRequest",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "EmployeeSelfService");

            migrationBuilder.AddColumn<Guid>(
                name: "SourceOvertimePlanDetailId",
                schema: "public",
                table: "WfpOvertimeRequest",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrxOvertimePlan",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlanTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RosterPeriodId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlanStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PlanEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PlanStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "Draft"),
                    ValidatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_TrxOvertimePlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlan_AspNetUsers_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlan_AspNetUsers_PublishedByUserId",
                        column: x => x.PublishedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlan_AspNetUsers_ValidatedByUserId",
                        column: x => x.ValidatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlan_MstCostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "public",
                        principalTable: "MstCostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlan_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlan_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlan_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlan_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlan_MstWorkLocation_WorkLocationId",
                        column: x => x.WorkLocationId,
                        principalSchema: "public",
                        principalTable: "MstWorkLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlan_TrxRosterPeriod_RosterPeriodId",
                        column: x => x.RosterPeriodId,
                        principalSchema: "public",
                        principalTable: "TrxRosterPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxOvertimePlanDetail",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OvertimePlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkScheduleAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RosterPeriodId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShiftAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    OvertimePolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    OvertimeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PlannedEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PlannedStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedEndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedMinutes = table.Column<int>(type: "integer", nullable: false),
                    EstimatedBreakMinutes = table.Column<int>(type: "integer", nullable: false),
                    DayType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Workday"),
                    OvertimeCategory = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "AfterShift"),
                    WorkDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HasScheduleConflict = table.Column<bool>(type: "boolean", nullable: false),
                    HasLeaveConflict = table.Column<bool>(type: "boolean", nullable: false),
                    HasTrainingConflict = table.Column<bool>(type: "boolean", nullable: false),
                    HasMinimumRestConflict = table.Column<bool>(type: "boolean", nullable: false),
                    HasWorkHourLimitConflict = table.Column<bool>(type: "boolean", nullable: false),
                    IsPolicyCompliant = table.Column<bool>(type: "boolean", nullable: false),
                    ValidationResultJson = table.Column<string>(type: "jsonb", nullable: true),
                    DetailStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "Draft"),
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
                    table.PrimaryKey("PK_TrxOvertimePlanDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_MstCostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "public",
                        principalTable: "MstCostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_MstOvertimePolicy_OvertimePolicyId",
                        column: x => x.OvertimePolicyId,
                        principalSchema: "public",
                        principalTable: "MstOvertimePolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_MstShift_ShiftId",
                        column: x => x.ShiftId,
                        principalSchema: "public",
                        principalTable: "MstShift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_MstWorkLocation_WorkLocationId",
                        column: x => x.WorkLocationId,
                        principalSchema: "public",
                        principalTable: "MstWorkLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_MstWorkSchedule_WorkScheduleId",
                        column: x => x.WorkScheduleId,
                        principalSchema: "public",
                        principalTable: "MstWorkSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_TrxOvertimePlan_OvertimePlanId",
                        column: x => x.OvertimePlanId,
                        principalSchema: "public",
                        principalTable: "TrxOvertimePlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_TrxRosterPeriod_RosterPeriodId",
                        column: x => x.RosterPeriodId,
                        principalSchema: "public",
                        principalTable: "TrxRosterPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_TrxShiftAssignment_ShiftAssignmentId",
                        column: x => x.ShiftAssignmentId,
                        principalSchema: "public",
                        principalTable: "TrxShiftAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_WfpOrganizationAssignment_Organizatio~",
                        column: x => x.OrganizationAssignmentId,
                        principalSchema: "public",
                        principalTable: "WfpOrganizationAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxOvertimePlanDetail_WfpWorkScheduleAssignment_WorkSchedul~",
                        column: x => x.WorkScheduleAssignmentId,
                        principalSchema: "public",
                        principalTable: "WfpWorkScheduleAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_RequestSource_OvertimeRequestStatus_Over~",
                schema: "public",
                table: "WfpOvertimeRequest",
                columns: new[] { "RequestSource", "OvertimeRequestStatus", "OvertimeDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOvertimeRequest_SourceOvertimePlanDetailId",
                schema: "public",
                table: "WfpOvertimeRequest",
                column: "SourceOvertimePlanDetailId",
                unique: true,
                filter: "\"SourceOvertimePlanDetailId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlan_ClosedByUserId",
                schema: "public",
                table: "TrxOvertimePlan",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlan_CostCenterId",
                schema: "public",
                table: "TrxOvertimePlan",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlan_DepartmentId",
                schema: "public",
                table: "TrxOvertimePlan",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlan_HospitalSiteId_DepartmentId_PlanStartDate_P~",
                schema: "public",
                table: "TrxOvertimePlan",
                columns: new[] { "HospitalSiteId", "DepartmentId", "PlanStartDate", "PlanEndDate", "PlanStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlan_LegalEntityId",
                schema: "public",
                table: "TrxOvertimePlan",
                column: "LegalEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlan_OrganizationUnitId",
                schema: "public",
                table: "TrxOvertimePlan",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlan_PlanNumber",
                schema: "public",
                table: "TrxOvertimePlan",
                column: "PlanNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlan_PublishedByUserId",
                schema: "public",
                table: "TrxOvertimePlan",
                column: "PublishedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlan_RosterPeriodId_PlanStatus_IsDelete",
                schema: "public",
                table: "TrxOvertimePlan",
                columns: new[] { "RosterPeriodId", "PlanStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlan_ValidatedByUserId",
                schema: "public",
                table: "TrxOvertimePlan",
                column: "ValidatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlan_WorkLocationId",
                schema: "public",
                table: "TrxOvertimePlan",
                column: "WorkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_CostCenterId",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_DepartmentId_OvertimeDate_DetailStatu~",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                columns: new[] { "DepartmentId", "OvertimeDate", "DetailStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_EmployeeId",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_HospitalSiteId",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_OrganizationAssignmentId",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                column: "OrganizationAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_OrganizationUnitId",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_OvertimePlanId_SequenceNumber",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                columns: new[] { "OvertimePlanId", "SequenceNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_OvertimePlanId_WorkforceProfileId_Pla~",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                columns: new[] { "OvertimePlanId", "WorkforceProfileId", "PlannedStartAt" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_OvertimePolicyId",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                column: "OvertimePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_PositionId",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_RosterPeriodId",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                column: "RosterPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_ShiftAssignmentId",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                column: "ShiftAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_ShiftId",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_WorkforceProfileId_OvertimeDate_Detai~",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                columns: new[] { "WorkforceProfileId", "OvertimeDate", "DetailStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_WorkLocationId",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                column: "WorkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_WorkScheduleAssignmentId",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                column: "WorkScheduleAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlanDetail_WorkScheduleId",
                schema: "public",
                table: "TrxOvertimePlanDetail",
                column: "WorkScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_WfpOvertimeRequest_TrxOvertimePlanDetail_SourceOvertimePlan~",
                schema: "public",
                table: "WfpOvertimeRequest",
                column: "SourceOvertimePlanDetailId",
                principalSchema: "public",
                principalTable: "TrxOvertimePlanDetail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WfpOvertimeRequest_TrxOvertimePlanDetail_SourceOvertimePlan~",
                schema: "public",
                table: "WfpOvertimeRequest");

            migrationBuilder.DropTable(
                name: "TrxOvertimePlanDetail",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxOvertimePlan",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_WfpOvertimeRequest_RequestSource_OvertimeRequestStatus_Over~",
                schema: "public",
                table: "WfpOvertimeRequest");

            migrationBuilder.DropIndex(
                name: "IX_WfpOvertimeRequest_SourceOvertimePlanDetailId",
                schema: "public",
                table: "WfpOvertimeRequest");

            migrationBuilder.DropColumn(
                name: "RequestSource",
                schema: "public",
                table: "WfpOvertimeRequest");

            migrationBuilder.DropColumn(
                name: "SourceOvertimePlanDetailId",
                schema: "public",
                table: "WfpOvertimeRequest");
        }
    }
}
