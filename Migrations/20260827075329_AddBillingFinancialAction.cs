using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingFinancialAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BilFinancialActionRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    FolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChargeLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChargeComponentId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetEncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetVersionAtSubmission = table.Column<int>(type: "integer", nullable: true),
                    RequestedAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ReasonNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovalPolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalPolicyVersion = table.Column<int>(type: "integer", nullable: true),
                    PolicyBlockReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MakerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    SupersedesRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExecutedAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    ExecutionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_BilFinancialActionRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BilFolioClosureHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    PriorStatus = table.Column<int>(type: "integer", nullable: false),
                    NewStatus = table.Column<int>(type: "integer", nullable: false),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FinancialActionRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureEvidence = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_BilFolioClosureHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BilFolioClosureHistory_BilFolio_FolioId",
                        column: x => x.FolioId,
                        principalSchema: "public",
                        principalTable: "BilFolio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstBillingApprovalPolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    PolicyCode = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    PolicyVersion = table.Column<int>(type: "integer", nullable: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MinimumAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    MaximumAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovalExpiryMinutes = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_MstBillingApprovalPolicy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BilFinancialApproval",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    CheckerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecisionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ApprovedAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    RequestContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    PriorStatus = table.Column<int>(type: "integer", nullable: false),
                    ResultingStatus = table.Column<int>(type: "integer", nullable: false),
                    MakerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChargeLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ApprovalPolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalPolicyVersion = table.Column<int>(type: "integer", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_BilFinancialApproval", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BilFinancialApproval_BilFinancialActionRequest_RequestId",
                        column: x => x.RequestId,
                        principalSchema: "public",
                        principalTable: "BilFinancialActionRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BilFinancialActionRequest_ActionType_Status",
                schema: "public",
                table: "BilFinancialActionRequest",
                columns: new[] { "ActionType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BilFinancialActionRequest_ChargeLineId",
                schema: "public",
                table: "BilFinancialActionRequest",
                column: "ChargeLineId");

            migrationBuilder.CreateIndex(
                name: "IX_BilFinancialActionRequest_EncounterId_Status",
                schema: "public",
                table: "BilFinancialActionRequest",
                columns: new[] { "EncounterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BilFinancialActionRequest_FolioId_Status",
                schema: "public",
                table: "BilFinancialActionRequest",
                columns: new[] { "FolioId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BilFinancialActionRequest_IdempotencyKey",
                schema: "public",
                table: "BilFinancialActionRequest",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IsDelete\" = false AND \"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BilFinancialActionRequest_MakerUserId",
                schema: "public",
                table: "BilFinancialActionRequest",
                column: "MakerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BilFinancialActionRequest_RequestNumber",
                schema: "public",
                table: "BilFinancialActionRequest",
                column: "RequestNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BilFinancialActionRequest_SupersedesRequestId",
                schema: "public",
                table: "BilFinancialActionRequest",
                column: "SupersedesRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BilFinancialApproval_CheckerUserId",
                schema: "public",
                table: "BilFinancialApproval",
                column: "CheckerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BilFinancialApproval_RequestId_DecidedAt",
                schema: "public",
                table: "BilFinancialApproval",
                columns: new[] { "RequestId", "DecidedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BilFinancialApproval_RequestId_Decision",
                schema: "public",
                table: "BilFinancialApproval",
                columns: new[] { "RequestId", "Decision" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"Decision\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_BilFolioClosureHistory_FinancialActionRequestId",
                schema: "public",
                table: "BilFolioClosureHistory",
                column: "FinancialActionRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BilFolioClosureHistory_FolioId_PerformedAt",
                schema: "public",
                table: "BilFolioClosureHistory",
                columns: new[] { "FolioId", "PerformedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MstBillingApprovalPolicy_ActionType_IsApproved_IsActive",
                schema: "public",
                table: "MstBillingApprovalPolicy",
                columns: new[] { "ActionType", "IsApproved", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MstBillingApprovalPolicy_PolicyCode_PolicyVersion",
                schema: "public",
                table: "MstBillingApprovalPolicy",
                columns: new[] { "PolicyCode", "PolicyVersion" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BilFinancialApproval",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilFolioClosureHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstBillingApprovalPolicy",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilFinancialActionRequest",
                schema: "public");
        }
    }
}
