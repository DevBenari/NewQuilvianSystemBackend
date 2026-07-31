using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeleaveOpeningBalanceManualAdjustmentFoundationV1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LeaveAdjustmentId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstLeaveAdjustmentReason",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReasonName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReasonCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "ManualAdjustment"),
                    AllowedDirection = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Both"),
                    AllowOpeningBalance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AllowManualAdjustment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AllowCorrection = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AllowReversal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MaximumAdjustmentDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    RequiresComment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RequiresAttachment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ApprovalWorkflowCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_MstLeaveAdjustmentReason", x => x.Id);
                    table.CheckConstraint("CK_MstLeaveAdjustmentReason_EffectiveDate", "\"EffectiveEndDate\" IS NULL OR \"EffectiveStartDate\" IS NULL OR \"EffectiveEndDate\" >= \"EffectiveStartDate\"");
                    table.CheckConstraint("CK_MstLeaveAdjustmentReason_MaximumAdjustmentDays", "\"MaximumAdjustmentDays\" IS NULL OR \"MaximumAdjustmentDays\" > 0");
                    table.ForeignKey(
                        name: "FK_MstLeaveAdjustmentReason_MstLeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalSchema: "public",
                        principalTable: "MstLeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxLeaveAdjustment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdjustmentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveBalanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveEntitlementPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveAdjustmentReasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    OriginalAdjustmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdjustmentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "ManualAdjustment"),
                    Direction = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Credit"),
                    RequestedDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ApprovedDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    PostedDays = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AdjustmentStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                    IdempotencyKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RequestNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SourceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "HrManual"),
                    SourceReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReversedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversalReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequestSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    ApprovalSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    PostingSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_TrxLeaveAdjustment", x => x.Id);
                    table.CheckConstraint("CK_TrxLeaveAdjustment_ApprovedDays", "\"ApprovedDays\" IS NULL OR \"ApprovedDays\" > 0");
                    table.CheckConstraint("CK_TrxLeaveAdjustment_PostedDays", "\"PostedDays\" >= 0");
                    table.CheckConstraint("CK_TrxLeaveAdjustment_RequestedDays", "\"RequestedDays\" > 0");
                    table.ForeignKey(
                        name: "FK_TrxLeaveAdjustment_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAdjustment_AspNetUsers_PostedByUserId",
                        column: x => x.PostedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAdjustment_AspNetUsers_RejectedByUserId",
                        column: x => x.RejectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAdjustment_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAdjustment_AspNetUsers_ReversedByUserId",
                        column: x => x.ReversedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAdjustment_AspNetUsers_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAdjustment_MstLeaveAdjustmentReason_LeaveAdjustment~",
                        column: x => x.LeaveAdjustmentReasonId,
                        principalSchema: "public",
                        principalTable: "MstLeaveAdjustmentReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAdjustment_MstLeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalSchema: "public",
                        principalTable: "MstLeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAdjustment_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAdjustment_TrxLeaveAdjustment_OriginalAdjustmentId",
                        column: x => x.OriginalAdjustmentId,
                        principalSchema: "public",
                        principalTable: "TrxLeaveAdjustment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAdjustment_TrxLeaveEntitlementPeriod_LeaveEntitleme~",
                        column: x => x.LeaveEntitlementPeriodId,
                        principalSchema: "public",
                        principalTable: "TrxLeaveEntitlementPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAdjustment_TrxWorkflowInstance_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalSchema: "public",
                        principalTable: "TrxWorkflowInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveAdjustment_WfpLeaveBalance_LeaveBalanceId",
                        column: x => x.LeaveBalanceId,
                        principalSchema: "public",
                        principalTable: "WfpLeaveBalance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceTransaction_LeaveAdjustmentId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                column: "LeaveAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MstLeaveAdjustmentReason_EffectiveStartDate_EffectiveEndDate",
                schema: "public",
                table: "MstLeaveAdjustmentReason",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeaveAdjustmentReason_LeaveTypeId_ReasonName",
                schema: "public",
                table: "MstLeaveAdjustmentReason",
                columns: new[] { "LeaveTypeId", "ReasonName" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstLeaveAdjustmentReason_LeaveTypeId_SortOrder_IsActive_IsD~",
                schema: "public",
                table: "MstLeaveAdjustmentReason",
                columns: new[] { "LeaveTypeId", "SortOrder", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeaveAdjustmentReason_ReasonCategory_AllowedDirection_Is~",
                schema: "public",
                table: "MstLeaveAdjustmentReason",
                columns: new[] { "ReasonCategory", "AllowedDirection", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstLeaveAdjustmentReason_ReasonCode",
                schema: "public",
                table: "MstLeaveAdjustmentReason",
                column: "ReasonCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_AdjustmentNumber",
                schema: "public",
                table: "TrxLeaveAdjustment",
                column: "AdjustmentNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_AdjustmentStatus_SubmittedAt_IsActive_Is~",
                schema: "public",
                table: "TrxLeaveAdjustment",
                columns: new[] { "AdjustmentStatus", "SubmittedAt", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_ApprovedByUserId",
                schema: "public",
                table: "TrxLeaveAdjustment",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_IdempotencyKey",
                schema: "public",
                table: "TrxLeaveAdjustment",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_LeaveAdjustmentReasonId",
                schema: "public",
                table: "TrxLeaveAdjustment",
                column: "LeaveAdjustmentReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_LeaveBalanceId_AdjustmentType",
                schema: "public",
                table: "TrxLeaveAdjustment",
                columns: new[] { "LeaveBalanceId", "AdjustmentType" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"AdjustmentType\" = 'OpeningBalance' AND \"AdjustmentStatus\" NOT IN ('Rejected', 'Cancelled', 'Reversed')");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_LeaveBalanceId_AdjustmentType_Adjustment~",
                schema: "public",
                table: "TrxLeaveAdjustment",
                columns: new[] { "LeaveBalanceId", "AdjustmentType", "AdjustmentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_LeaveEntitlementPeriodId_AdjustmentStatu~",
                schema: "public",
                table: "TrxLeaveAdjustment",
                columns: new[] { "LeaveEntitlementPeriodId", "AdjustmentStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_LeaveTypeId",
                schema: "public",
                table: "TrxLeaveAdjustment",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_OriginalAdjustmentId",
                schema: "public",
                table: "TrxLeaveAdjustment",
                column: "OriginalAdjustmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_PostedByUserId",
                schema: "public",
                table: "TrxLeaveAdjustment",
                column: "PostedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_RejectedByUserId",
                schema: "public",
                table: "TrxLeaveAdjustment",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_RequestedByUserId",
                schema: "public",
                table: "TrxLeaveAdjustment",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_ReversedByUserId",
                schema: "public",
                table: "TrxLeaveAdjustment",
                column: "ReversedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_SourceType_SourceReferenceId",
                schema: "public",
                table: "TrxLeaveAdjustment",
                columns: new[] { "SourceType", "SourceReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_SubmittedByUserId",
                schema: "public",
                table: "TrxLeaveAdjustment",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_WorkflowInstanceId",
                schema: "public",
                table: "TrxLeaveAdjustment",
                column: "WorkflowInstanceId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"WorkflowInstanceId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveAdjustment_WorkforceProfileId_LeaveTypeId_Effective~",
                schema: "public",
                table: "TrxLeaveAdjustment",
                columns: new[] { "WorkforceProfileId", "LeaveTypeId", "EffectiveDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrxLeaveBalanceTransaction_TrxLeaveAdjustment_LeaveAdjustme~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction",
                column: "LeaveAdjustmentId",
                principalSchema: "public",
                principalTable: "TrxLeaveAdjustment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxLeaveBalanceTransaction_TrxLeaveAdjustment_LeaveAdjustme~",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropTable(
                name: "TrxLeaveAdjustment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstLeaveAdjustmentReason",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_TrxLeaveBalanceTransaction_LeaveAdjustmentId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");

            migrationBuilder.DropColumn(
                name: "LeaveAdjustmentId",
                schema: "public",
                table: "TrxLeaveBalanceTransaction");
        }
    }
}
