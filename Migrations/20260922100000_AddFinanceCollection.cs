using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Menuntaskan gap yang dilaporkan BE-FIN-007: 2 tabel penerimaan (FinReceipt,
    /// FinReceiptAllocation) yang semula direncanakan ikut migration AddFinanceReceivableAndCollection
    /// tidak pernah dibuat karena belum ada task pemilik. BE-FIN-016 menutupnya.
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinanceCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinReceipt",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceTenderId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceCollectionHandoffId = table.Column<Guid>(type: "uuid", nullable: true),
                    SettlementId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentMethodId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentMethodAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    UnallocatedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    KwitansiNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CashierShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderReference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ProviderEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceInvoiceStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "RECEIVED"),
                    ReversalOfReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FinReceipt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinReceipt_FinReceipt_ReversalOfReceiptId",
                        column: x => x.ReversalOfReceiptId,
                        principalSchema: "public",
                        principalTable: "FinReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.CheckConstraint("CK_FinReceipt_SourceType", "\"SourceType\" IN ('BILLING_TENDER','AR_COLLECTION','MANUAL')");
                    table.CheckConstraint("CK_FinReceipt_Status", "\"Status\" IN ('RECEIVED','ALLOCATED','RECONCILED','REVERSED')");
                    table.CheckConstraint("CK_FinReceipt_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_FinReceipt_Unallocated", "\"UnallocatedAmount\" >= 0");
                    table.CheckConstraint("CK_FinReceipt_AllocationBalance", "\"Amount\" = \"AllocatedAmount\" + \"UnallocatedAmount\"");
                    table.CheckConstraint("CK_FinReceipt_TenderRequired", "\"SourceType\" <> 'BILLING_TENDER' OR \"SourceTenderId\" IS NOT NULL");
                });

            migrationBuilder.CreateTable(
                name: "FinReceiptAllocation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivableId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceAllocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsReversal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReversalOfAllocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AllocatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_FinReceiptAllocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinReceiptAllocation_FinReceipt_ReceiptId",
                        column: x => x.ReceiptId,
                        principalSchema: "public",
                        principalTable: "FinReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinReceiptAllocation_FinReceivable_ReceivableId",
                        column: x => x.ReceivableId,
                        principalSchema: "public",
                        principalTable: "FinReceivable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinReceiptAllocation_FinReceiptAllocation_ReversalOfAllocationId",
                        column: x => x.ReversalOfAllocationId,
                        principalSchema: "public",
                        principalTable: "FinReceiptAllocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.CheckConstraint("CK_FinReceiptAllocation_TargetType", "\"TargetType\" IN ('RECEIVABLE','INVOICE_DIRECT')");
                    table.CheckConstraint("CK_FinReceiptAllocation_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_FinReceiptAllocation_ReceivableRequired", "\"TargetType\" <> 'RECEIVABLE' OR \"ReceivableId\" IS NOT NULL");
                    table.CheckConstraint("CK_FinReceiptAllocation_Reversal",
                        "(\"IsReversal\" = true AND \"ReversalOfAllocationId\" IS NOT NULL) OR (\"IsReversal\" = false AND \"ReversalOfAllocationId\" IS NULL)");
                });

            // Satu tender berhasil = paling banyak satu penerimaan (FIN-DES-010).
            migrationBuilder.CreateIndex(
                name: "IX_FinReceipt_SourceTenderId",
                schema: "public",
                table: "FinReceipt",
                column: "SourceTenderId",
                unique: true,
                filter: "\"SourceTenderId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceipt_ReceiptNumber",
                schema: "public",
                table: "FinReceipt",
                column: "ReceiptNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceipt_CashierShift_OccurredAt",
                schema: "public",
                table: "FinReceipt",
                columns: new[] { "CashierShiftId", "OccurredAt" });

            // Index otomatis EF Core untuk kolom FK self-reference (ReversalOfReceiptId belum
            // menjadi kolom terdepan index manapun di atas).
            migrationBuilder.CreateIndex(
                name: "IX_FinReceipt_ReversalOfReceiptId",
                schema: "public",
                table: "FinReceipt",
                column: "ReversalOfReceiptId");

            // Index otomatis EF Core untuk ketiga kolom FK FinReceiptAllocation.
            migrationBuilder.CreateIndex(
                name: "IX_FinReceiptAllocation_ReceiptId",
                schema: "public",
                table: "FinReceiptAllocation",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceiptAllocation_ReceivableId",
                schema: "public",
                table: "FinReceiptAllocation",
                column: "ReceivableId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceiptAllocation_ReversalOfAllocationId",
                schema: "public",
                table: "FinReceiptAllocation",
                column: "ReversalOfAllocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinReceiptAllocation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinReceipt",
                schema: "public");
        }
    }
}
