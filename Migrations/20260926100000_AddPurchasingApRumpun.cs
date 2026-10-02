using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-031 (FIN-DES-037, FIN-DES-038, FIN-DEC-045..051, 02-backend-architecture.md
    /// §C.1-C.2 dan §D.1-D.2 AMENDMENT REVISI 5). Sebelas tabel rumpun Purchasing/AP:
    /// Purchase Order → Tanda Terima Barang → Tukar Faktur → Purchasing Invoice, dan
    /// Retur Pembelian beserta Deposit Retur (bentuk REVISI 5 — pemakaian deposit dipasangkan
    /// ke FinPayment, BUKAN ke FinPurchasingInvoice; lihat erd/data-dictionary.md bagian C.11
    /// DIGANTIKAN).
    ///
    /// Migration kedua (AddSourcePurchasingInvoiceIdToSupplierPayable) menyusul, menambah
    /// kolom penghubung ke FinSupplierPayable existing — sengaja dipisah agar migration ini
    /// hanya membuat tabel baru, tanpa menyentuh tabel yang sudah berjalan.
    /// </summary>
    /// <inheritdoc />
    public partial class AddPurchasingApRumpun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinPurchaseOrder",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PONumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "DRAFT"),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovalTier = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_FinPurchaseOrder", x => x.Id);
                    table.CheckConstraint("CK_FinPurchaseOrder_ApprovalTier", "\"ApprovalTier\" IN ('TIER_1','TIER_2')");
                    table.CheckConstraint("CK_FinPurchaseOrder_Status", "\"Status\" IN ('DRAFT','PENDING_APPROVAL','APPROVED','REJECTED','CANCELLED','PARTIALLY_RECEIVED','FULLY_RECEIVED','CLOSED')");
                    table.ForeignKey(
                        name: "FK_FinPurchaseOrder_MstSupplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "public",
                        principalTable: "MstSupplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinPurchaseOrderItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_FinPurchaseOrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinPurchaseOrderItem_FinPurchaseOrder_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "public",
                        principalTable: "FinPurchaseOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinGoodsReceipt",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GRNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "RECEIVED"),
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
                    table.PrimaryKey("PK_FinGoodsReceipt", x => x.Id);
                    table.CheckConstraint("CK_FinGoodsReceipt_Status", "\"Status\" IN ('RECEIVED','CANCELLED')");
                    table.ForeignKey(
                        name: "FK_FinGoodsReceipt_FinPurchaseOrder_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "public",
                        principalTable: "FinPurchaseOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinGoodsReceiptItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoodsReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FinGoodsReceiptItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinGoodsReceiptItem_FinGoodsReceipt_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalSchema: "public",
                        principalTable: "FinGoodsReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinGoodsReceiptItem_FinPurchaseOrderItem_PurchaseOrderItemId",
                        column: x => x.PurchaseOrderItemId,
                        principalSchema: "public",
                        principalTable: "FinPurchaseOrderItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinInvoiceExchange",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    GoodsReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierInvoiceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SupplierInvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReceivedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EstimatedDueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "RECEIVED"),
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
                    table.PrimaryKey("PK_FinInvoiceExchange", x => x.Id);
                    table.CheckConstraint("CK_FinInvoiceExchange_Status", "\"Status\" IN ('RECEIVED','LINKED_TO_INVOICE','CANCELLED')");
                    table.ForeignKey(
                        name: "FK_FinInvoiceExchange_MstSupplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "public",
                        principalTable: "MstSupplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinInvoiceExchange_FinPurchaseOrder_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "public",
                        principalTable: "FinPurchaseOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FinInvoiceExchange_FinGoodsReceipt_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalSchema: "public",
                        principalTable: "FinGoodsReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FinPurchasingInvoice",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InvoiceExchangeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubtotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    PPNAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DownPaymentAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    OtherDeductionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "DRAFT"),
                    ApprovalTier = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_FinPurchasingInvoice", x => x.Id);
                    table.CheckConstraint("CK_FinPurchasingInvoice_ApprovalTier", "\"ApprovalTier\" IN ('TIER_1','TIER_2')");
                    table.CheckConstraint("CK_FinPurchasingInvoice_Status", "\"Status\" IN ('DRAFT','PENDING_APPROVAL','APPROVED','REJECTED','CANCELLED')");
                    table.ForeignKey(
                        name: "FK_FinPurchasingInvoice_FinInvoiceExchange_InvoiceExchangeId",
                        column: x => x.InvoiceExchangeId,
                        principalSchema: "public",
                        principalTable: "FinInvoiceExchange",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinPurchasingInvoice_MstSupplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "public",
                        principalTable: "MstSupplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinPurchasingInvoiceItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchasingInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_FinPurchasingInvoiceItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinPurchasingInvoiceItem_PurchasingInvoiceId",
                        column: x => x.PurchasingInvoiceId,
                        principalSchema: "public",
                        principalTable: "FinPurchasingInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinSupplierReturn",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReturnNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PurchasingInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "DRAFT"),
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
                    table.PrimaryKey("PK_FinSupplierReturn", x => x.Id);
                    table.CheckConstraint("CK_FinSupplierReturn_Status", "\"Status\" IN ('DRAFT','CONFIRMED','CANCELLED')");
                    table.ForeignKey(
                        name: "FK_FinSupplierReturn_FinPurchasingInvoice_PurchasingInvoiceId",
                        column: x => x.PurchasingInvoiceId,
                        principalSchema: "public",
                        principalTable: "FinPurchasingInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinSupplierReturnItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierReturnId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_FinSupplierReturnItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinSupplierReturnItem_FinSupplierReturn_SupplierReturnId",
                        column: x => x.SupplierReturnId,
                        principalSchema: "public",
                        principalTable: "FinSupplierReturn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinSupplierReturnDeposit",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceReturnId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailableAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "AVAILABLE"),
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
                    table.PrimaryKey("PK_FinSupplierReturnDeposit", x => x.Id);
                    table.CheckConstraint("CK_FinSupplierReturnDeposit_Available", "\"AvailableAmount\" >= 0");
                    table.CheckConstraint("CK_FinSupplierReturnDeposit_Status", "\"Status\" IN ('AVAILABLE','EXHAUSTED','CANCELLED')");
                    table.ForeignKey(
                        name: "FK_FinSupplierReturnDeposit_MstSupplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "public",
                        principalTable: "MstSupplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinSupplierReturnDeposit_FinSupplierReturn_SourceReturnId",
                        column: x => x.SourceReturnId,
                        principalSchema: "public",
                        principalTable: "FinSupplierReturn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinSupplierReturnDepositUsage",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierReturnDepositId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "RESERVED"),
                    UsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_FinSupplierReturnDepositUsage", x => x.Id);
                    table.CheckConstraint("CK_FinSupplierReturnDepositUsage_Amount", "\"UsedAmount\" > 0");
                    table.CheckConstraint("CK_FinSupplierReturnDepositUsage_ReleasedAt", "(\"Status\" = 'RELEASED') = (\"ReleasedAt\" IS NOT NULL)");
                    table.CheckConstraint("CK_FinSupplierReturnDepositUsage_Status", "\"Status\" IN ('RESERVED','APPLIED','RELEASED')");
                    table.ForeignKey(
                        name: "FK_FinSupplierReturnDepositUsage_SupplierReturnDepositId",
                        column: x => x.SupplierReturnDepositId,
                        principalSchema: "public",
                        principalTable: "FinSupplierReturnDeposit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinSupplierReturnDepositUsage_FinPayment_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "public",
                        principalTable: "FinPayment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchaseOrder_PONumber",
                schema: "public",
                table: "FinPurchaseOrder",
                column: "PONumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchaseOrder_Status",
                schema: "public",
                table: "FinPurchaseOrder",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchaseOrder_SupplierId",
                schema: "public",
                table: "FinPurchaseOrder",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchaseOrderItem_PurchaseOrderId",
                schema: "public",
                table: "FinPurchaseOrderItem",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_FinGoodsReceipt_GRNumber",
                schema: "public",
                table: "FinGoodsReceipt",
                column: "GRNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinGoodsReceipt_PurchaseOrderId",
                schema: "public",
                table: "FinGoodsReceipt",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_FinGoodsReceiptItem_GoodsReceiptId",
                schema: "public",
                table: "FinGoodsReceiptItem",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_FinGoodsReceiptItem_PurchaseOrderItemId",
                schema: "public",
                table: "FinGoodsReceiptItem",
                column: "PurchaseOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FinInvoiceExchange_EstimatedDueDate",
                schema: "public",
                table: "FinInvoiceExchange",
                column: "EstimatedDueDate");

            migrationBuilder.CreateIndex(
                name: "IX_FinInvoiceExchange_ExchangeNumber",
                schema: "public",
                table: "FinInvoiceExchange",
                column: "ExchangeNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinInvoiceExchange_GoodsReceiptId",
                schema: "public",
                table: "FinInvoiceExchange",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_FinInvoiceExchange_PurchaseOrderId",
                schema: "public",
                table: "FinInvoiceExchange",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_FinInvoiceExchange_SupplierId",
                schema: "public",
                table: "FinInvoiceExchange",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingInvoice_InvoiceExchangeId",
                schema: "public",
                table: "FinPurchasingInvoice",
                column: "InvoiceExchangeId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingInvoice_InvoiceNumber",
                schema: "public",
                table: "FinPurchasingInvoice",
                column: "InvoiceNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingInvoice_Status",
                schema: "public",
                table: "FinPurchasingInvoice",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingInvoice_SupplierId",
                schema: "public",
                table: "FinPurchasingInvoice",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingInvoiceItem_PurchasingInvoiceId",
                schema: "public",
                table: "FinPurchasingInvoiceItem",
                column: "PurchasingInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierReturn_PurchasingInvoiceId",
                schema: "public",
                table: "FinSupplierReturn",
                column: "PurchasingInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierReturn_ReturnNumber",
                schema: "public",
                table: "FinSupplierReturn",
                column: "ReturnNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierReturnItem_SupplierReturnId",
                schema: "public",
                table: "FinSupplierReturnItem",
                column: "SupplierReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierReturnDeposit_SourceReturnId",
                schema: "public",
                table: "FinSupplierReturnDeposit",
                column: "SourceReturnId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierReturnDeposit_Supplier_Status",
                schema: "public",
                table: "FinSupplierReturnDeposit",
                columns: new[] { "SupplierId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierReturnDepositUsage_ActivePerPayment",
                schema: "public",
                table: "FinSupplierReturnDepositUsage",
                columns: new[] { "PaymentId", "SupplierReturnDepositId" },
                unique: true,
                filter: "\"Status\" <> 'RELEASED' AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierReturnDepositUsage_PaymentId",
                schema: "public",
                table: "FinSupplierReturnDepositUsage",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierReturnDepositUsage_SupplierReturnDepositId",
                schema: "public",
                table: "FinSupplierReturnDepositUsage",
                column: "SupplierReturnDepositId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinSupplierReturnDepositUsage",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinSupplierReturnDeposit",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinSupplierReturnItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinSupplierReturn",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinPurchasingInvoiceItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinPurchasingInvoice",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinInvoiceExchange",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinGoodsReceiptItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinGoodsReceipt",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinPurchaseOrderItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinPurchaseOrder",
                schema: "public");
        }
    }
}
