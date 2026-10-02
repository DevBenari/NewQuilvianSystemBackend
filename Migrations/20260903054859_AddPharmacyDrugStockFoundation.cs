using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyDrugStockFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstDrugBatch",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DrugSupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrincipalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstDrugBatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDrugBatch_MstDrugSupplier_DrugSupplierId",
                        column: x => x.DrugSupplierId,
                        principalSchema: "public",
                        principalTable: "MstDrugSupplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDrugBatch_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxDrugStockBalance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    QuantityReserved = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
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
                    table.PrimaryKey("PK_TrxDrugStockBalance", x => x.Id);
                    table.CheckConstraint("CK_TrxDrugStockBalance_OnHandNotNegative", "\"QuantityOnHand\" >= 0");
                    table.CheckConstraint("CK_TrxDrugStockBalance_ReservedWithinOnHand", "\"QuantityReserved\" >= 0 AND \"QuantityReserved\" <= \"QuantityOnHand\"");
                    table.ForeignKey(
                        name: "FK_TrxDrugStockBalance_MstDrugBatch_DrugBatchId",
                        column: x => x.DrugBatchId,
                        principalSchema: "public",
                        principalTable: "MstDrugBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugStockBalance_MstDrugStorageLocation_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalSchema: "public",
                        principalTable: "MstDrugStorageLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugStockBalance_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxDrugStockMutation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MutationType = table.Column<int>(type: "integer", nullable: false),
                    QuantityChange = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SourceDocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SourceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrectionOfMutationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_TrxDrugStockMutation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxDrugStockMutation_MstDrugBatch_DrugBatchId",
                        column: x => x.DrugBatchId,
                        principalSchema: "public",
                        principalTable: "MstDrugBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugStockMutation_MstDrugStorageLocation_StorageLocation~",
                        column: x => x.StorageLocationId,
                        principalSchema: "public",
                        principalTable: "MstDrugStorageLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugStockMutation_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugStockMutation_TrxDrugStockMutation_CorrectionOfMutat~",
                        column: x => x.CorrectionOfMutationId,
                        principalSchema: "public",
                        principalTable: "TrxDrugStockMutation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugBatch_BatchNumber",
                schema: "public",
                table: "MstDrugBatch",
                column: "BatchNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugBatch_DrugId_BatchNumber",
                schema: "public",
                table: "MstDrugBatch",
                columns: new[] { "DrugId", "BatchNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugBatch_DrugSupplierId",
                schema: "public",
                table: "MstDrugBatch",
                column: "DrugSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugBatch_ExpiryDate",
                schema: "public",
                table: "MstDrugBatch",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugStockBalance_DrugBatchId_StorageLocationId_Status",
                schema: "public",
                table: "TrxDrugStockBalance",
                columns: new[] { "DrugBatchId", "StorageLocationId", "Status" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugStockBalance_DrugId_StorageLocationId_Status",
                schema: "public",
                table: "TrxDrugStockBalance",
                columns: new[] { "DrugId", "StorageLocationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugStockBalance_StorageLocationId",
                schema: "public",
                table: "TrxDrugStockBalance",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugStockMutation_CorrectionOfMutationId",
                schema: "public",
                table: "TrxDrugStockMutation",
                column: "CorrectionOfMutationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugStockMutation_DrugBatchId_StorageLocationId_Occurred~",
                schema: "public",
                table: "TrxDrugStockMutation",
                columns: new[] { "DrugBatchId", "StorageLocationId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugStockMutation_DrugId_StorageLocationId_OccurredAt",
                schema: "public",
                table: "TrxDrugStockMutation",
                columns: new[] { "DrugId", "StorageLocationId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugStockMutation_SourceDocumentType_CorrelationId",
                schema: "public",
                table: "TrxDrugStockMutation",
                columns: new[] { "SourceDocumentType", "CorrelationId" },
                unique: true,
                filter: "\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugStockMutation_SourceDocumentType_SourceDocumentId",
                schema: "public",
                table: "TrxDrugStockMutation",
                columns: new[] { "SourceDocumentType", "SourceDocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugStockMutation_StorageLocationId",
                schema: "public",
                table: "TrxDrugStockMutation",
                column: "StorageLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxDrugStockBalance",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxDrugStockMutation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDrugBatch",
                schema: "public");
        }
    }
}
