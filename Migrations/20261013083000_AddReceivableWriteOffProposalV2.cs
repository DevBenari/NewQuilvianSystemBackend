using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddReceivableWriteOffProposalV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinReceivableWriteOffProposal",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposalNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProposalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ProposalType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "PER_INVOICE"),
                    ServiceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DebtorReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReasonDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "DRAFT"),
                    TotalInvoiceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalOutstandingAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalProposedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalApprovedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PostedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FinReceivableWriteOffProposal", x => x.Id);
                    table.CheckConstraint("CK_FinReceivableWriteOffProposal_Amounts", "\"TotalProposedAmount\" >= 0 AND \"TotalApprovedAmount\" >= 0");
                    table.CheckConstraint("CK_FinReceivableWriteOffProposal_MakerChecker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
                    table.CheckConstraint("CK_FinReceivableWriteOffProposal_Status", "\"Status\" IN ('DRAFT','SUBMITTED','APPROVED','POSTED','REJECTED')");
                });

            migrationBuilder.CreateTable(
                name: "FinReceivableWriteOffProposalItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivableId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutstandingSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProposedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WriteOffId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_FinReceivableWriteOffProposalItem", x => x.Id);
                    table.CheckConstraint("CK_FinReceivableWriteOffProposalItem_Amounts", "\"ProposedAmount\" > 0 AND \"ApprovedAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_FinReceivableWriteOffProposalItem_FinReceivableInvoiceBatch_InvoiceBatchId",
                        column: x => x.InvoiceBatchId,
                        principalSchema: "public",
                        principalTable: "FinReceivableInvoiceBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinReceivableWriteOffProposalItem_FinReceivableWriteOffProposal_ProposalId",
                        column: x => x.ProposalId,
                        principalSchema: "public",
                        principalTable: "FinReceivableWriteOffProposal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinReceivableWriteOffProposalItem_FinReceivableWriteOff_WriteOffId",
                        column: x => x.WriteOffId,
                        principalSchema: "public",
                        principalTable: "FinReceivableWriteOff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FinReceivableWriteOffProposalItem_FinReceivable_ReceivableId",
                        column: x => x.ReceivableId,
                        principalSchema: "public",
                        principalTable: "FinReceivable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableWriteOffProposal_Date",
                schema: "public",
                table: "FinReceivableWriteOffProposal",
                column: "ProposalDate");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableWriteOffProposal_DebtorReferenceId",
                schema: "public",
                table: "FinReceivableWriteOffProposal",
                column: "DebtorReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableWriteOffProposal_Number",
                schema: "public",
                table: "FinReceivableWriteOffProposal",
                column: "ProposalNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableWriteOffProposal_Status",
                schema: "public",
                table: "FinReceivableWriteOffProposal",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableWriteOffProposalItem_InvoiceBatchId",
                schema: "public",
                table: "FinReceivableWriteOffProposalItem",
                column: "InvoiceBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableWriteOffProposalItem_Proposal_Receivable",
                schema: "public",
                table: "FinReceivableWriteOffProposalItem",
                columns: new[] { "ProposalId", "ReceivableId" });

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableWriteOffProposalItem_ReceivableId",
                schema: "public",
                table: "FinReceivableWriteOffProposalItem",
                column: "ReceivableId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableWriteOffProposalItem_WriteOffId",
                schema: "public",
                table: "FinReceivableWriteOffProposalItem",
                column: "WriteOffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinReceivableWriteOffProposalItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinReceivableWriteOffProposal",
                schema: "public");
        }
    }
}
