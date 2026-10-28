using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchasingApAllocationAndMedicalFeeFinanceHandoff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinInvoiceExchangeGoodsReceipt",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceExchangeId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoodsReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FinInvoiceExchangeGoodsReceipt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinInvoiceExchangeGoodsReceipt_FinGoodsReceipt_GoodsReceipt~",
                        column: x => x.GoodsReceiptId,
                        principalSchema: "public",
                        principalTable: "FinGoodsReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinInvoiceExchangeGoodsReceipt_FinInvoiceExchange_InvoiceEx~",
                        column: x => x.InvoiceExchangeId,
                        principalSchema: "public",
                        principalTable: "FinInvoiceExchange",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinPurchasingInvoiceItemAllocation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchasingInvoiceItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoodsReceiptItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocatedQuantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_FinPurchasingInvoiceItemAllocation", x => x.Id);
                    table.CheckConstraint("CK_FinPurchasingInvoiceItemAllocation_Amount", "\"AllocatedAmount\" >= 0");
                    table.CheckConstraint("CK_FinPurchasingInvoiceItemAllocation_Quantity", "\"AllocatedQuantity\" > 0");
                    table.ForeignKey(
                        name: "FK_FinPurchasingInvoiceItemAllocation_FinGoodsReceiptItem_Good~",
                        column: x => x.GoodsReceiptItemId,
                        principalSchema: "public",
                        principalTable: "FinGoodsReceiptItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinPurchasingInvoiceItemAllocation_FinPurchasingInvoiceItem~",
                        column: x => x.PurchasingInvoiceItemId,
                        principalSchema: "public",
                        principalTable: "FinPurchasingInvoiceItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MdfFinanceHandoff",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceFeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    HandoffKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PayeeType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PayeeReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Created"),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_MdfFinanceHandoff", x => x.Id);
                    table.CheckConstraint("CK_MdfFinanceHandoff_Amount", "\"GrossAmount\" >= 0");
                    table.CheckConstraint("CK_MdfFinanceHandoff_Status", "\"Status\" IN ('Created','Acknowledged','Failed')");
                    table.ForeignKey(
                        name: "FK_MdfFinanceHandoff_MdfServiceFee_ServiceFeeId",
                        column: x => x.ServiceFeeId,
                        principalSchema: "public",
                        principalTable: "MdfServiceFee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinInvoiceExchangeGoodsReceipt_ExchangeGoodsReceipt",
                schema: "public",
                table: "FinInvoiceExchangeGoodsReceipt",
                columns: new[] { "InvoiceExchangeId", "GoodsReceiptId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinInvoiceExchangeGoodsReceipt_GoodsReceiptId",
                schema: "public",
                table: "FinInvoiceExchangeGoodsReceipt",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingInvoiceItemAllocation_GoodsReceiptItemId",
                schema: "public",
                table: "FinPurchasingInvoiceItemAllocation",
                column: "GoodsReceiptItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingInvoiceItemAllocation_InvoiceItem_GrItem",
                schema: "public",
                table: "FinPurchasingInvoiceItemAllocation",
                columns: new[] { "PurchasingInvoiceItemId", "GoodsReceiptItemId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MdfFinanceHandoff_HandoffKey",
                schema: "public",
                table: "MdfFinanceHandoff",
                column: "HandoffKey",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MdfFinanceHandoff_PeriodCode_PayeeType",
                schema: "public",
                table: "MdfFinanceHandoff",
                columns: new[] { "PeriodCode", "PayeeType" });

            migrationBuilder.CreateIndex(
                name: "IX_MdfFinanceHandoff_ServiceFeeId",
                schema: "public",
                table: "MdfFinanceHandoff",
                column: "ServiceFeeId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MdfFinanceHandoff_Status_CreateDateTime",
                schema: "public",
                table: "MdfFinanceHandoff",
                columns: new[] { "Status", "CreateDateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinInvoiceExchangeGoodsReceipt",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinPurchasingInvoiceItemAllocation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MdfFinanceHandoff",
                schema: "public");
        }
    }
}
