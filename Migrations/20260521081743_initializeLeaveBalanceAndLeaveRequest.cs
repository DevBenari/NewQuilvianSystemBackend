using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeLeaveBalanceAndLeaveRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WfpLeaveBalance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveYear = table.Column<int>(type: "integer", nullable: false),
                    LeaveType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    OpeningBalance = table.Column<decimal>(type: "numeric(6,2)", nullable: false, defaultValue: 0m),
                    EntitledDays = table.Column<decimal>(type: "numeric(6,2)", nullable: false, defaultValue: 0m),
                    UsedDays = table.Column<decimal>(type: "numeric(6,2)", nullable: false, defaultValue: 0m),
                    PendingDays = table.Column<decimal>(type: "numeric(6,2)", nullable: false, defaultValue: 0m),
                    RemainingDays = table.Column<decimal>(type: "numeric(6,2)", nullable: false, defaultValue: 0m),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_WfpLeaveBalance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpLeaveBalance_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpLeaveRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveBalanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    TotalDays = table.Column<decimal>(type: "numeric(6,2)", nullable: false, defaultValue: 0m),
                    IsHalfDay = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDeductBalance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_WfpLeaveRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpLeaveRequest_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpLeaveRequest_WfpLeaveBalance_LeaveBalanceId",
                        column: x => x.LeaveBalanceId,
                        principalSchema: "public",
                        principalTable: "WfpLeaveBalance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveBalance_LeaveYear_LeaveType_IsActive_IsDelete",
                schema: "public",
                table: "WfpLeaveBalance",
                columns: new[] { "LeaveYear", "LeaveType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveBalance_WorkforceProfileId",
                schema: "public",
                table: "WfpLeaveBalance",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveBalance_WorkforceProfileId_LeaveYear_IsActive_IsDel~",
                schema: "public",
                table: "WfpLeaveBalance",
                columns: new[] { "WorkforceProfileId", "LeaveYear", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveBalance_WorkforceProfileId_LeaveYear_LeaveType",
                schema: "public",
                table: "WfpLeaveBalance",
                columns: new[] { "WorkforceProfileId", "LeaveYear", "LeaveType" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveRequest_ApprovedByUserId",
                schema: "public",
                table: "WfpLeaveRequest",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveRequest_CancelledByUserId",
                schema: "public",
                table: "WfpLeaveRequest",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveRequest_LeaveBalanceId",
                schema: "public",
                table: "WfpLeaveRequest",
                column: "LeaveBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveRequest_LeaveType_ApprovalStatus_StartDate_EndDate",
                schema: "public",
                table: "WfpLeaveRequest",
                columns: new[] { "LeaveType", "ApprovalStatus", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveRequest_RejectedByUserId",
                schema: "public",
                table: "WfpLeaveRequest",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveRequest_RequestedAt_ApprovalStatus_IsDelete",
                schema: "public",
                table: "WfpLeaveRequest",
                columns: new[] { "RequestedAt", "ApprovalStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveRequest_WorkforceProfileId",
                schema: "public",
                table: "WfpLeaveRequest",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveRequest_WorkforceProfileId_LeaveType_ApprovalStatus~",
                schema: "public",
                table: "WfpLeaveRequest",
                columns: new[] { "WorkforceProfileId", "LeaveType", "ApprovalStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpLeaveRequest_WorkforceProfileId_StartDate_EndDate_Approv~",
                schema: "public",
                table: "WfpLeaveRequest",
                columns: new[] { "WorkforceProfileId", "StartDate", "EndDate", "ApprovalStatus", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WfpLeaveRequest",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpLeaveBalance",
                schema: "public");
        }
    }
}
