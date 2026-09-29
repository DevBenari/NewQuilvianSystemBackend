using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-038 (FIN-DES-041, 048, 049; erd/data-dictionary.md §C.13-C.14 dan §D.3-D.4(c)
    /// AMENDMENT REVISI 5). Tiga tabel baru: FinReceivableInvoiceBatch dan
    /// FinReceivableInvoiceBatchItem (submodul Receivable), FinReceiptDeduction (submodul
    /// Collection).
    ///
    /// FinReceiptDeduction dibangun dari bentuk REVISI 5 (§D.3/§D.4(c)) — ber-DeductionNumber,
    /// ReceiptAllocationId, IsReversal, ReversalOfDeductionId — BUKAN dari rancangan §C.15 yang
    /// hanya menunjuk ReceiptId. §C.15 tidak pernah dibangun, sehingga penggantian ini tidak
    /// menuntut pembetulan data.
    /// </summary>
    /// <inheritdoc />
    public partial class AddArInvoiceBatchAndReceiptDeduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinReceivableInvoiceBatch",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DebtorType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "PAYER"),
                    DebtorReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "DRAFT"),
                    IssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_FinReceivableInvoiceBatch", x => x.Id);
                    table.CheckConstraint("CK_FinReceivableInvoiceBatch_DebtorType", "\"DebtorType\" = 'PAYER'");
                    table.CheckConstraint("CK_FinReceivableInvoiceBatch_Status", "\"Status\" IN ('DRAFT','ISSUED','PARTIALLY_PAID','PAID','CANCELLED')");
                });

            migrationBuilder.CreateTable(
                name: "FinReceivableInvoiceBatchItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivableId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FinReceivableInvoiceBatchItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinReceivableInvoiceBatchItem_FinReceivableInvoiceBatch_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "public",
                        principalTable: "FinReceivableInvoiceBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinReceivableInvoiceBatchItem_FinReceivable_ReceivableId",
                        column: x => x.ReceivableId,
                        principalSchema: "public",
                        principalTable: "FinReceivable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinReceiptDeduction",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeductionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptAllocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeductionType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsReversal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReversalOfDeductionId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_FinReceiptDeduction", x => x.Id);
                    table.CheckConstraint("CK_FinReceiptDeduction_Type", "\"DeductionType\" IN ('PPH23','BANK_ADMIN_FEE','OTHER')");
                    table.CheckConstraint("CK_FinReceiptDeduction_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_FinReceiptDeduction_OtherReason", "\"DeductionType\" <> 'OTHER' OR \"Reason\" IS NOT NULL");
                    table.CheckConstraint("CK_FinReceiptDeduction_Reversal", "\"IsReversal\" = (\"ReversalOfDeductionId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_FinReceiptDeduction_FinReceipt_ReceiptId",
                        column: x => x.ReceiptId,
                        principalSchema: "public",
                        principalTable: "FinReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinReceiptDeduction_FinReceiptAllocation_ReceiptAllocationId",
                        column: x => x.ReceiptAllocationId,
                        principalSchema: "public",
                        principalTable: "FinReceiptAllocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinReceiptDeduction_FinReceiptDeduction_ReversalOfDeductionId",
                        column: x => x.ReversalOfDeductionId,
                        principalSchema: "public",
                        principalTable: "FinReceiptDeduction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatch_BatchNumber",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                column: "BatchNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatch_DebtorType",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                column: "DebtorType");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatch_DebtorReferenceId",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                column: "DebtorReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatch_PeriodStart",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                column: "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatch_Status",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatchItem_BatchId",
                schema: "public",
                table: "FinReceivableInvoiceBatchItem",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatchItem_ActiveReceivable",
                schema: "public",
                table: "FinReceivableInvoiceBatchItem",
                column: "ReceivableId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceiptDeduction_DeductionNumber",
                schema: "public",
                table: "FinReceiptDeduction",
                column: "DeductionNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceiptDeduction_ReceiptId",
                schema: "public",
                table: "FinReceiptDeduction",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceiptDeduction_DeductionType",
                schema: "public",
                table: "FinReceiptDeduction",
                column: "DeductionType");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceiptDeduction_Allocation",
                schema: "public",
                table: "FinReceiptDeduction",
                column: "ReceiptAllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceiptDeduction_ReversalOnce",
                schema: "public",
                table: "FinReceiptDeduction",
                column: "ReversalOfDeductionId",
                unique: true,
                filter: "\"ReversalOfDeductionId\" IS NOT NULL AND \"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinReceiptDeduction",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinReceivableInvoiceBatchItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinReceivableInvoiceBatch",
                schema: "public");
        }
    }
}
