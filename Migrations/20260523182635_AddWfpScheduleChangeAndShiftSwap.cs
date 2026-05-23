using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddWfpScheduleChangeAndShiftSwap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WfpScheduleChangeRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentWorkScheduleAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedScheduleDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RequestedWorkScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApprovalStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_WfpScheduleChangeRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpScheduleChangeRequest_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpScheduleChangeRequest_AspNetUsers_RejectedByUserId",
                        column: x => x.RejectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpScheduleChangeRequest_MstWorkforceProfile_WorkforceProfi~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpScheduleChangeRequest_MstWorkSchedule_RequestedWorkSched~",
                        column: x => x.RequestedWorkScheduleId,
                        principalSchema: "public",
                        principalTable: "MstWorkSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpScheduleChangeRequest_WfpWorkScheduleAssignment_CurrentW~",
                        column: x => x.CurrentWorkScheduleAssignmentId,
                        principalSchema: "public",
                        principalTable: "WfpWorkScheduleAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpShiftSwapRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterScheduleAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetScheduleAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApprovalStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_WfpShiftSwapRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpShiftSwapRequest_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpShiftSwapRequest_AspNetUsers_RejectedByUserId",
                        column: x => x.RejectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpShiftSwapRequest_MstWorkforceProfile_RequesterWorkforceP~",
                        column: x => x.RequesterWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpShiftSwapRequest_MstWorkforceProfile_TargetWorkforceProf~",
                        column: x => x.TargetWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpShiftSwapRequest_WfpWorkScheduleAssignment_RequesterSche~",
                        column: x => x.RequesterScheduleAssignmentId,
                        principalSchema: "public",
                        principalTable: "WfpWorkScheduleAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpShiftSwapRequest_WfpWorkScheduleAssignment_TargetSchedul~",
                        column: x => x.TargetScheduleAssignmentId,
                        principalSchema: "public",
                        principalTable: "WfpWorkScheduleAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpScheduleChangeRequest_ApprovalStatus",
                schema: "public",
                table: "WfpScheduleChangeRequest",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_WfpScheduleChangeRequest_ApprovedByUserId",
                schema: "public",
                table: "WfpScheduleChangeRequest",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpScheduleChangeRequest_CurrentWorkScheduleAssignmentId",
                schema: "public",
                table: "WfpScheduleChangeRequest",
                column: "CurrentWorkScheduleAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpScheduleChangeRequest_CurrentWorkScheduleAssignmentId_Ap~",
                schema: "public",
                table: "WfpScheduleChangeRequest",
                columns: new[] { "CurrentWorkScheduleAssignmentId", "ApprovalStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpScheduleChangeRequest_RejectedByUserId",
                schema: "public",
                table: "WfpScheduleChangeRequest",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpScheduleChangeRequest_RequestedAt",
                schema: "public",
                table: "WfpScheduleChangeRequest",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WfpScheduleChangeRequest_RequestedScheduleDate",
                schema: "public",
                table: "WfpScheduleChangeRequest",
                column: "RequestedScheduleDate");

            migrationBuilder.CreateIndex(
                name: "IX_WfpScheduleChangeRequest_RequestedWorkScheduleId",
                schema: "public",
                table: "WfpScheduleChangeRequest",
                column: "RequestedWorkScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpScheduleChangeRequest_RequestType",
                schema: "public",
                table: "WfpScheduleChangeRequest",
                column: "RequestType");

            migrationBuilder.CreateIndex(
                name: "IX_WfpScheduleChangeRequest_WorkforceProfileId",
                schema: "public",
                table: "WfpScheduleChangeRequest",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpScheduleChangeRequest_WorkforceProfileId_RequestedSched~1",
                schema: "public",
                table: "WfpScheduleChangeRequest",
                columns: new[] { "WorkforceProfileId", "RequestedScheduleDate", "RequestType", "ApprovalStatus" },
                filter: "\"ApprovalStatus\" = 1 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpScheduleChangeRequest_WorkforceProfileId_RequestedSchedu~",
                schema: "public",
                table: "WfpScheduleChangeRequest",
                columns: new[] { "WorkforceProfileId", "RequestedScheduleDate", "ApprovalStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpScheduleChangeRequest_WorkforceProfileId_RequestType_App~",
                schema: "public",
                table: "WfpScheduleChangeRequest",
                columns: new[] { "WorkforceProfileId", "RequestType", "ApprovalStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpShiftSwapRequest_ApprovalStatus",
                schema: "public",
                table: "WfpShiftSwapRequest",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_WfpShiftSwapRequest_ApprovedByUserId",
                schema: "public",
                table: "WfpShiftSwapRequest",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpShiftSwapRequest_RejectedByUserId",
                schema: "public",
                table: "WfpShiftSwapRequest",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpShiftSwapRequest_RequestedAt",
                schema: "public",
                table: "WfpShiftSwapRequest",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WfpShiftSwapRequest_RequesterScheduleAssignmentId",
                schema: "public",
                table: "WfpShiftSwapRequest",
                column: "RequesterScheduleAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpShiftSwapRequest_RequesterScheduleAssignmentId_TargetSc~1",
                schema: "public",
                table: "WfpShiftSwapRequest",
                columns: new[] { "RequesterScheduleAssignmentId", "TargetScheduleAssignmentId", "ApprovalStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpShiftSwapRequest_RequesterScheduleAssignmentId_TargetSch~",
                schema: "public",
                table: "WfpShiftSwapRequest",
                columns: new[] { "RequesterScheduleAssignmentId", "TargetScheduleAssignmentId", "ApprovalStatus" },
                filter: "\"ApprovalStatus\" = 1 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpShiftSwapRequest_RequesterWorkforceProfileId",
                schema: "public",
                table: "WfpShiftSwapRequest",
                column: "RequesterWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpShiftSwapRequest_RequesterWorkforceProfileId_RequesterSc~",
                schema: "public",
                table: "WfpShiftSwapRequest",
                columns: new[] { "RequesterWorkforceProfileId", "RequesterScheduleAssignmentId", "ApprovalStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpShiftSwapRequest_RequesterWorkforceProfileId_TargetWorkf~",
                schema: "public",
                table: "WfpShiftSwapRequest",
                columns: new[] { "RequesterWorkforceProfileId", "TargetWorkforceProfileId", "ApprovalStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpShiftSwapRequest_TargetScheduleAssignmentId",
                schema: "public",
                table: "WfpShiftSwapRequest",
                column: "TargetScheduleAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpShiftSwapRequest_TargetWorkforceProfileId",
                schema: "public",
                table: "WfpShiftSwapRequest",
                column: "TargetWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpShiftSwapRequest_TargetWorkforceProfileId_TargetSchedule~",
                schema: "public",
                table: "WfpShiftSwapRequest",
                columns: new[] { "TargetWorkforceProfileId", "TargetScheduleAssignmentId", "ApprovalStatus", "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WfpScheduleChangeRequest",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpShiftSwapRequest",
                schema: "public");
        }
    }
}
