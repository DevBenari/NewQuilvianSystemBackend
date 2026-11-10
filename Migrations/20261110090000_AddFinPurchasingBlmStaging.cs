using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddFinPurchasingBlmStaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPurchaseOrder_ApprovalTier",
                schema: "public",
                table: "FinPurchaseOrder");

            migrationBuilder.AddColumn<string>(
                name: "SourceSystem",
                schema: "public",
                table: "FinPurchaseOrder",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "QUILVIAN");

            migrationBuilder.AddColumn<Guid>(
                name: "ExternalPurchaseOrderId",
                schema: "public",
                table: "FinPurchaseOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExternalUpdatedAt",
                schema: "public",
                table: "FinPurchaseOrder",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                schema: "public",
                table: "FinPurchaseOrder",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExternalPurchaseOrderItemId",
                schema: "public",
                table: "FinPurchaseOrderItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExternalProductId",
                schema: "public",
                table: "FinPurchaseOrderItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalProductCode",
                schema: "public",
                table: "FinPurchaseOrderItem",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUomCode",
                schema: "public",
                table: "FinPurchaseOrderItem",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityPerBaseUnit",
                schema: "public",
                table: "FinPurchaseOrderItem",
                type: "numeric(18,6)",
                precision: 18, scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaseUomCode",
                schema: "public",
                table: "FinPurchaseOrderItem",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseQuantity",
                schema: "public",
                table: "FinPurchaseOrderItem",
                type: "numeric(18,6)",
                precision: 18, scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceSystem",
                schema: "public",
                table: "FinGoodsReceipt",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "QUILVIAN");

            migrationBuilder.AddColumn<Guid>(
                name: "ExternalReceiveOrderId",
                schema: "public",
                table: "FinGoodsReceipt",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExternalUpdatedAt",
                schema: "public",
                table: "FinGoodsReceipt",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                schema: "public",
                table: "FinGoodsReceipt",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExternalReceiveOrderItemId",
                schema: "public",
                table: "FinGoodsReceiptItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConditionCode",
                schema: "public",
                table: "FinGoodsReceiptItem",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SourceQuantity",
                schema: "public",
                table: "FinGoodsReceiptItem",
                type: "numeric(18,6)",
                precision: 18, scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUomCode",
                schema: "public",
                table: "FinGoodsReceiptItem",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityPerBaseUnit",
                schema: "public",
                table: "FinGoodsReceiptItem",
                type: "numeric(18,6)",
                precision: 18, scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaseUomCode",
                schema: "public",
                table: "FinGoodsReceiptItem",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseQuantity",
                schema: "public",
                table: "FinGoodsReceiptItem",
                type: "numeric(18,6)",
                precision: 18, scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AcceptedBaseQuantity",
                schema: "public",
                table: "FinGoodsReceiptItem",
                type: "numeric(18,6)",
                precision: 18, scale: 6,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPurchaseOrder_ApprovalTier",
                schema: "public",
                table: "FinPurchaseOrder",
                sql: "\"ApprovalTier\" IN ('TIER_1','TIER_2','EXTERNAL')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPurchaseOrder_SourceSystem",
                schema: "public",
                table: "FinPurchaseOrder",
                sql: "\"SourceSystem\" IN ('QUILVIAN','BLM')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPurchaseOrder_ExternalIdentity",
                schema: "public",
                table: "FinPurchaseOrder",
                sql: "(\"SourceSystem\" = 'BLM' AND \"ExternalPurchaseOrderId\" IS NOT NULL) OR (\"SourceSystem\" <> 'BLM' AND \"ExternalPurchaseOrderId\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPurchaseOrder_ExternalTier",
                schema: "public",
                table: "FinPurchaseOrder",
                sql: "(\"ApprovalTier\" = 'EXTERNAL' AND \"SourceSystem\" = 'BLM') OR (\"ApprovalTier\" <> 'EXTERNAL' AND \"SourceSystem\" <> 'BLM')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPurchaseOrderItem_BlmFields",
                schema: "public",
                table: "FinPurchaseOrderItem",
                sql: "(\"ExternalPurchaseOrderItemId\" IS NULL AND \"ExternalProductId\" IS NULL AND \"SourceUomCode\" IS NULL AND \"QuantityPerBaseUnit\" IS NULL AND \"BaseUomCode\" IS NULL AND \"BaseQuantity\" IS NULL) OR (\"ExternalPurchaseOrderItemId\" IS NOT NULL AND \"ExternalProductId\" IS NOT NULL AND \"SourceUomCode\" IS NOT NULL AND \"QuantityPerBaseUnit\" IS NOT NULL AND \"BaseUomCode\" IS NOT NULL AND \"BaseQuantity\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPurchaseOrderItem_QuantityPerBaseUnit",
                schema: "public",
                table: "FinPurchaseOrderItem",
                sql: "\"QuantityPerBaseUnit\" IS NULL OR \"QuantityPerBaseUnit\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinGoodsReceipt_SourceSystem",
                schema: "public",
                table: "FinGoodsReceipt",
                sql: "\"SourceSystem\" IN ('QUILVIAN','BLM')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinGoodsReceipt_ExternalIdentity",
                schema: "public",
                table: "FinGoodsReceipt",
                sql: "(\"SourceSystem\" = 'BLM' AND \"ExternalReceiveOrderId\" IS NOT NULL) OR (\"SourceSystem\" <> 'BLM' AND \"ExternalReceiveOrderId\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinGoodsReceiptItem_BlmFields",
                schema: "public",
                table: "FinGoodsReceiptItem",
                sql: "(\"ExternalReceiveOrderItemId\" IS NULL AND \"ConditionCode\" IS NULL AND \"SourceQuantity\" IS NULL AND \"SourceUomCode\" IS NULL AND \"QuantityPerBaseUnit\" IS NULL AND \"BaseUomCode\" IS NULL AND \"BaseQuantity\" IS NULL AND \"AcceptedBaseQuantity\" IS NULL) OR (\"ExternalReceiveOrderItemId\" IS NOT NULL AND \"ConditionCode\" IS NOT NULL AND \"SourceQuantity\" IS NOT NULL AND \"SourceUomCode\" IS NOT NULL AND \"QuantityPerBaseUnit\" IS NOT NULL AND \"BaseUomCode\" IS NOT NULL AND \"BaseQuantity\" IS NOT NULL AND \"AcceptedBaseQuantity\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinGoodsReceiptItem_ConditionCode",
                schema: "public",
                table: "FinGoodsReceiptItem",
                sql: "\"ConditionCode\" IS NULL OR \"ConditionCode\" IN ('GOOD','DAMAGED','PENDING_INSPECTION')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinGoodsReceiptItem_QuantityPerBaseUnit",
                schema: "public",
                table: "FinGoodsReceiptItem",
                sql: "\"QuantityPerBaseUnit\" IS NULL OR \"QuantityPerBaseUnit\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinGoodsReceiptItem_AcceptedBaseQuantity",
                schema: "public",
                table: "FinGoodsReceiptItem",
                sql: "\"AcceptedBaseQuantity\" IS NULL OR (\"AcceptedBaseQuantity\" >= 0 AND \"AcceptedBaseQuantity\" <= \"BaseQuantity\" AND (\"ConditionCode\" = 'GOOD' OR \"AcceptedBaseQuantity\" = 0))");

            migrationBuilder.CreateTable(
                name: "FinPurchasingInboundMessage",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "BLM"),
                    EventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ExternalDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalDocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SourceVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SourceOccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProcessingStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "RECEIVED"),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastErrorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AppliedPurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppliedGoodsReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_FinPurchasingInboundMessage", x => x.Id);
                    table.CheckConstraint("CK_FinPurchasingInboundMessage_Applied", "\"ProcessingStatus\" <> 'APPLIED' OR (\"DocumentType\" = 'PURCHASE_ORDER' AND \"AppliedPurchaseOrderId\" IS NOT NULL AND \"AppliedGoodsReceiptId\" IS NULL) OR (\"DocumentType\" = 'GOODS_RECEIPT' AND \"AppliedGoodsReceiptId\" IS NOT NULL AND \"AppliedPurchaseOrderId\" IS NULL)");
                    table.CheckConstraint("CK_FinPurchasingInboundMessage_AttemptCount", "\"AttemptCount\" >= 0");
                    table.CheckConstraint("CK_FinPurchasingInboundMessage_DocumentType", "\"DocumentType\" IN ('PURCHASE_ORDER','GOODS_RECEIPT')");
                    table.CheckConstraint("CK_FinPurchasingInboundMessage_ProcessingStatus", "\"ProcessingStatus\" IN ('RECEIVED','APPLIED','WAITING_MAPPING','CONFLICT','REJECTED','SUPERSEDED')");
                    table.CheckConstraint("CK_FinPurchasingInboundMessage_SourceSystem", "\"SourceSystem\" IN ('BLM')");
                    table.ForeignKey(
                        name: "FK_FinPurchasingInboundMessage_FinGoodsReceipt_AppliedGoodsRec~",
                        column: x => x.AppliedGoodsReceiptId,
                        principalSchema: "public",
                        principalTable: "FinGoodsReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinPurchasingInboundMessage_FinPurchaseOrder_AppliedPurchas~",
                        column: x => x.AppliedPurchaseOrderId,
                        principalSchema: "public",
                        principalTable: "FinPurchaseOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinPurchasingSupplierMap",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "BLM"),
                    ExternalSupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalSupplierName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_FinPurchasingSupplierMap", x => x.Id);
                    table.CheckConstraint("CK_FinPurchasingSupplierMap_SourceSystem", "\"SourceSystem\" IN ('BLM')");
                    table.ForeignKey(
                        name: "FK_FinPurchasingSupplierMap_MstSupplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "public",
                        principalTable: "MstSupplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinPurchasingSyncConflict",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InboundMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConflictType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    GoodsReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    DependentDocumentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DependentDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "OPEN"),
                    ResolutionCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ResolutionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_FinPurchasingSyncConflict", x => x.Id);
                    table.CheckConstraint("CK_FinPurchasingSyncConflict_ConflictType", "\"ConflictType\" IN ('GR_CANCELLED_WITH_DEPENDENTS','PO_CANCELLED_WITH_DEPENDENTS','NUMBER_COLLISION','PAYLOAD_MISMATCH','CONVERSION_UNRESOLVED')");
                    table.CheckConstraint("CK_FinPurchasingSyncConflict_DependentId", "\"DependentDocumentType\" IS NULL OR \"DependentDocumentId\" IS NOT NULL");
                    table.CheckConstraint("CK_FinPurchasingSyncConflict_DependentType", "\"DependentDocumentType\" IS NULL OR \"DependentDocumentType\" IN ('INVOICE_EXCHANGE','PURCHASING_INVOICE','SUPPLIER_PAYABLE','PAYMENT')");
                    table.CheckConstraint("CK_FinPurchasingSyncConflict_ResolutionCode", "\"ResolutionCode\" IS NULL OR \"ResolutionCode\" IN ('ADJUSTMENT','REVERSAL','SUPPLIER_RETURN','SUPPLIER_CREDIT','NO_FINANCIAL_IMPACT')");
                    table.CheckConstraint("CK_FinPurchasingSyncConflict_Resolved", "\"Status\" <> 'RESOLVED' OR (\"ResolutionCode\" IS NOT NULL AND \"ResolvedByUserId\" IS NOT NULL AND \"ResolvedAt\" IS NOT NULL)");
                    table.CheckConstraint("CK_FinPurchasingSyncConflict_Status", "\"Status\" IN ('OPEN','RESOLVED')");
                    table.CheckConstraint("CK_FinPurchasingSyncConflict_Target", "\"PurchaseOrderId\" IS NOT NULL OR \"GoodsReceiptId\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_FinPurchasingSyncConflict_FinGoodsReceipt_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalSchema: "public",
                        principalTable: "FinGoodsReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinPurchasingSyncConflict_FinPurchasingInboundMessage_Inbou~",
                        column: x => x.InboundMessageId,
                        principalSchema: "public",
                        principalTable: "FinPurchasingInboundMessage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinPurchasingSyncConflict_FinPurchaseOrder_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "public",
                        principalTable: "FinPurchaseOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinPurchasingExternalCancellation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "BLM"),
                    Direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ExternalDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    GoodsReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CategoryCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OtherCategoryText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RejectionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NextAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InboundMessageId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_FinPurchasingExternalCancellation", x => x.Id);
                    table.CheckConstraint("CK_FinPurchasingExternalCancellation_AttemptCount", "\"AttemptCount\" >= 0");
                    table.CheckConstraint("CK_FinPurchasingExternalCancellation_Confirmed", "\"Status\" <> 'CONFIRMED' OR \"CancelledAt\" IS NOT NULL");
                    table.CheckConstraint("CK_FinPurchasingExternalCancellation_Direction", "\"Direction\" IN ('OUTBOUND_REQUEST','INBOUND_NOTICE')");
                    table.CheckConstraint("CK_FinPurchasingExternalCancellation_DocumentType", "\"DocumentType\" IN ('PURCHASE_ORDER','GOODS_RECEIPT')");
                    table.CheckConstraint("CK_FinPurchasingExternalCancellation_Requester", "\"Direction\" <> 'OUTBOUND_REQUEST' OR \"RequestedByUserId\" IS NOT NULL");
                    table.CheckConstraint("CK_FinPurchasingExternalCancellation_SourceSystem", "\"SourceSystem\" IN ('BLM')");
                    table.CheckConstraint("CK_FinPurchasingExternalCancellation_Status", "\"Status\" IN ('REQUESTED','SENT','CONFIRMED','REJECTED','FAILED')");
                    table.CheckConstraint("CK_FinPurchasingExternalCancellation_Target", "(\"DocumentType\" = 'PURCHASE_ORDER' AND \"PurchaseOrderId\" IS NOT NULL) OR (\"DocumentType\" = 'GOODS_RECEIPT' AND \"GoodsReceiptId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_FinPurchasingExternalCancellation_FinGoodsReceipt_GoodsRece~",
                        column: x => x.GoodsReceiptId,
                        principalSchema: "public",
                        principalTable: "FinGoodsReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinPurchasingExternalCancellation_FinPurchasingInboundMessa~",
                        column: x => x.InboundMessageId,
                        principalSchema: "public",
                        principalTable: "FinPurchasingInboundMessage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinPurchasingExternalCancellation_FinPurchaseOrder_Purchase~",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "public",
                        principalTable: "FinPurchaseOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinGoodsReceipt_SourceSystem_ExternalReceiveOrderId",
                schema: "public",
                table: "FinGoodsReceipt",
                columns: new[] { "SourceSystem", "ExternalReceiveOrderId" },
                unique: true,
                filter: "\"ExternalReceiveOrderId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinGoodsReceiptItem_ExternalReceiveOrderItemId",
                schema: "public",
                table: "FinGoodsReceiptItem",
                column: "ExternalReceiveOrderItemId",
                unique: true,
                filter: "\"ExternalReceiveOrderItemId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchaseOrder_SourceSystem_ExternalPurchaseOrderId",
                schema: "public",
                table: "FinPurchaseOrder",
                columns: new[] { "SourceSystem", "ExternalPurchaseOrderId" },
                unique: true,
                filter: "\"ExternalPurchaseOrderId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchaseOrderItem_ExternalProductId",
                schema: "public",
                table: "FinPurchaseOrderItem",
                column: "ExternalProductId",
                filter: "\"ExternalProductId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchaseOrderItem_ExternalPurchaseOrderItemId",
                schema: "public",
                table: "FinPurchaseOrderItem",
                column: "ExternalPurchaseOrderItemId",
                unique: true,
                filter: "\"ExternalPurchaseOrderItemId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingExternalCancellation_Document",
                schema: "public",
                table: "FinPurchasingExternalCancellation",
                columns: new[] { "DocumentType", "ExternalDocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingExternalCancellation_GoodsReceiptId",
                schema: "public",
                table: "FinPurchasingExternalCancellation",
                column: "GoodsReceiptId",
                filter: "\"GoodsReceiptId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingExternalCancellation_InboundMessageId",
                schema: "public",
                table: "FinPurchasingExternalCancellation",
                column: "InboundMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingExternalCancellation_PurchaseOrderId",
                schema: "public",
                table: "FinPurchasingExternalCancellation",
                column: "PurchaseOrderId",
                filter: "\"PurchaseOrderId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingExternalCancellation_SourceDirectionEvent",
                schema: "public",
                table: "FinPurchasingExternalCancellation",
                columns: new[] { "SourceSystem", "Direction", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingExternalCancellation_Status",
                schema: "public",
                table: "FinPurchasingExternalCancellation",
                columns: new[] { "Status", "NextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingInboundMessage_AppliedGoodsReceiptId",
                schema: "public",
                table: "FinPurchasingInboundMessage",
                column: "AppliedGoodsReceiptId",
                filter: "\"AppliedGoodsReceiptId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingInboundMessage_AppliedPurchaseOrderId",
                schema: "public",
                table: "FinPurchasingInboundMessage",
                column: "AppliedPurchaseOrderId",
                filter: "\"AppliedPurchaseOrderId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingInboundMessage_Document",
                schema: "public",
                table: "FinPurchasingInboundMessage",
                columns: new[] { "DocumentType", "ExternalDocumentId", "SourceOccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingInboundMessage_ProcessingStatus",
                schema: "public",
                table: "FinPurchasingInboundMessage",
                columns: new[] { "ProcessingStatus", "NextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingInboundMessage_SourceSystem_EventId",
                schema: "public",
                table: "FinPurchasingInboundMessage",
                columns: new[] { "SourceSystem", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingSupplierMap_SourceSystem_ExternalSupplierId",
                schema: "public",
                table: "FinPurchasingSupplierMap",
                columns: new[] { "SourceSystem", "ExternalSupplierId" },
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingSupplierMap_SupplierId",
                schema: "public",
                table: "FinPurchasingSupplierMap",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingSyncConflict_GoodsReceiptId",
                schema: "public",
                table: "FinPurchasingSyncConflict",
                column: "GoodsReceiptId",
                filter: "\"GoodsReceiptId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingSyncConflict_OpenPerMessageType",
                schema: "public",
                table: "FinPurchasingSyncConflict",
                columns: new[] { "InboundMessageId", "ConflictType" },
                unique: true,
                filter: "\"Status\" = 'OPEN' AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingSyncConflict_PurchaseOrderId",
                schema: "public",
                table: "FinPurchasingSyncConflict",
                column: "PurchaseOrderId",
                filter: "\"PurchaseOrderId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingSyncConflict_StatusConflictType",
                schema: "public",
                table: "FinPurchasingSyncConflict",
                columns: new[] { "Status", "ConflictType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinPurchasingExternalCancellation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinPurchasingSyncConflict",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinPurchasingSupplierMap",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinPurchasingInboundMessage",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_FinGoodsReceipt_SourceSystem_ExternalReceiveOrderId",
                schema: "public",
                table: "FinGoodsReceipt");

            migrationBuilder.DropIndex(
                name: "IX_FinGoodsReceiptItem_ExternalReceiveOrderItemId",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropIndex(
                name: "IX_FinPurchaseOrder_SourceSystem_ExternalPurchaseOrderId",
                schema: "public",
                table: "FinPurchaseOrder");

            migrationBuilder.DropIndex(
                name: "IX_FinPurchaseOrderItem_ExternalProductId",
                schema: "public",
                table: "FinPurchaseOrderItem");

            migrationBuilder.DropIndex(
                name: "IX_FinPurchaseOrderItem_ExternalPurchaseOrderItemId",
                schema: "public",
                table: "FinPurchaseOrderItem");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinGoodsReceiptItem_AcceptedBaseQuantity",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinGoodsReceiptItem_QuantityPerBaseUnit",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinGoodsReceiptItem_ConditionCode",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinGoodsReceiptItem_BlmFields",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinGoodsReceipt_ExternalIdentity",
                schema: "public",
                table: "FinGoodsReceipt");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinGoodsReceipt_SourceSystem",
                schema: "public",
                table: "FinGoodsReceipt");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPurchaseOrderItem_QuantityPerBaseUnit",
                schema: "public",
                table: "FinPurchaseOrderItem");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPurchaseOrderItem_BlmFields",
                schema: "public",
                table: "FinPurchaseOrderItem");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPurchaseOrder_ExternalTier",
                schema: "public",
                table: "FinPurchaseOrder");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPurchaseOrder_ExternalIdentity",
                schema: "public",
                table: "FinPurchaseOrder");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPurchaseOrder_SourceSystem",
                schema: "public",
                table: "FinPurchaseOrder");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPurchaseOrder_ApprovalTier",
                schema: "public",
                table: "FinPurchaseOrder");

            migrationBuilder.DropColumn(
                name: "AcceptedBaseQuantity",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropColumn(
                name: "BaseQuantity",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropColumn(
                name: "BaseUomCode",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropColumn(
                name: "QuantityPerBaseUnit",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropColumn(
                name: "SourceUomCode",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropColumn(
                name: "SourceQuantity",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropColumn(
                name: "ConditionCode",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropColumn(
                name: "ExternalReceiveOrderItemId",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                schema: "public",
                table: "FinGoodsReceipt");

            migrationBuilder.DropColumn(
                name: "ExternalUpdatedAt",
                schema: "public",
                table: "FinGoodsReceipt");

            migrationBuilder.DropColumn(
                name: "ExternalReceiveOrderId",
                schema: "public",
                table: "FinGoodsReceipt");

            migrationBuilder.DropColumn(
                name: "SourceSystem",
                schema: "public",
                table: "FinGoodsReceipt");

            migrationBuilder.DropColumn(
                name: "BaseQuantity",
                schema: "public",
                table: "FinPurchaseOrderItem");

            migrationBuilder.DropColumn(
                name: "BaseUomCode",
                schema: "public",
                table: "FinPurchaseOrderItem");

            migrationBuilder.DropColumn(
                name: "QuantityPerBaseUnit",
                schema: "public",
                table: "FinPurchaseOrderItem");

            migrationBuilder.DropColumn(
                name: "SourceUomCode",
                schema: "public",
                table: "FinPurchaseOrderItem");

            migrationBuilder.DropColumn(
                name: "ExternalProductCode",
                schema: "public",
                table: "FinPurchaseOrderItem");

            migrationBuilder.DropColumn(
                name: "ExternalProductId",
                schema: "public",
                table: "FinPurchaseOrderItem");

            migrationBuilder.DropColumn(
                name: "ExternalPurchaseOrderItemId",
                schema: "public",
                table: "FinPurchaseOrderItem");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                schema: "public",
                table: "FinPurchaseOrder");

            migrationBuilder.DropColumn(
                name: "ExternalUpdatedAt",
                schema: "public",
                table: "FinPurchaseOrder");

            migrationBuilder.DropColumn(
                name: "ExternalPurchaseOrderId",
                schema: "public",
                table: "FinPurchaseOrder");

            migrationBuilder.DropColumn(
                name: "SourceSystem",
                schema: "public",
                table: "FinPurchaseOrder");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPurchaseOrder_ApprovalTier",
                schema: "public",
                table: "FinPurchaseOrder",
                sql: "\"ApprovalTier\" IN ('TIER_1','TIER_2')");
        }
    }
}
