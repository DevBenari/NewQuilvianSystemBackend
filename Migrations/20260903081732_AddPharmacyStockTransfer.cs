using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyStockTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrxStockTransfer",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceStorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationStorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestedByWorkforceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ItemCount = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_TrxStockTransfer", x => x.Id);
                    table.CheckConstraint("CK_TrxStockTransfer_SourceNotDestination", "\"SourceStorageLocationId\" <> \"DestinationStorageLocationId\"");
                    table.ForeignKey(
                        name: "FK_TrxStockTransfer_MstDrugStorageLocation_DestinationStorageL~",
                        column: x => x.DestinationStorageLocationId,
                        principalSchema: "public",
                        principalTable: "MstDrugStorageLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxStockTransfer_MstDrugStorageLocation_SourceStorageLocati~",
                        column: x => x.SourceStorageLocationId,
                        principalSchema: "public",
                        principalTable: "MstDrugStorageLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxStockTransferHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_TrxStockTransferHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxStockTransferHistory_TrxStockTransfer_StockTransferId",
                        column: x => x.StockTransferId,
                        principalSchema: "public",
                        principalTable: "TrxStockTransfer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrxStockTransferItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DrugNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RequestedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    IssuedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    ReceivedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_TrxStockTransferItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxStockTransferItem_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxStockTransferItem_TrxStockTransfer_StockTransferId",
                        column: x => x.StockTransferId,
                        principalSchema: "public",
                        principalTable: "TrxStockTransfer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrxStockTransferAllocation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockTransferItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    IsReleased = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_TrxStockTransferAllocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxStockTransferAllocation_MstDrugBatch_DrugBatchId",
                        column: x => x.DrugBatchId,
                        principalSchema: "public",
                        principalTable: "MstDrugBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxStockTransferAllocation_TrxStockTransferItem_StockTransf~",
                        column: x => x.StockTransferItemId,
                        principalSchema: "public",
                        principalTable: "TrxStockTransferItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockTransfer_DestinationStorageLocationId_Status_Reques~",
                schema: "public",
                table: "TrxStockTransfer",
                columns: new[] { "DestinationStorageLocationId", "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockTransfer_RequestedAt",
                schema: "public",
                table: "TrxStockTransfer",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockTransfer_SourceStorageLocationId_Status_RequestedAt",
                schema: "public",
                table: "TrxStockTransfer",
                columns: new[] { "SourceStorageLocationId", "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockTransfer_TransferNumber",
                schema: "public",
                table: "TrxStockTransfer",
                column: "TransferNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockTransferAllocation_DrugBatchId",
                schema: "public",
                table: "TrxStockTransferAllocation",
                column: "DrugBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockTransferAllocation_StockTransferItemId",
                schema: "public",
                table: "TrxStockTransferAllocation",
                column: "StockTransferItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockTransferHistory_Action_CorrelationId",
                schema: "public",
                table: "TrxStockTransferHistory",
                columns: new[] { "Action", "CorrelationId" },
                unique: true,
                filter: "\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockTransferHistory_StockTransferId_OccurredAt",
                schema: "public",
                table: "TrxStockTransferHistory",
                columns: new[] { "StockTransferId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockTransferItem_DrugId",
                schema: "public",
                table: "TrxStockTransferItem",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockTransferItem_StockTransferId_DrugId",
                schema: "public",
                table: "TrxStockTransferItem",
                columns: new[] { "StockTransferId", "DrugId" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxStockTransferAllocation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxStockTransferHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxStockTransferItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxStockTransfer",
                schema: "public");
        }
    }
}
