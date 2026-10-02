using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyStockRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrxStockRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequestingServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByWorkforceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    NeededAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_TrxStockRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxStockRequest_MstDrugStorageLocation_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalSchema: "public",
                        principalTable: "MstDrugStorageLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxStockRequest_MstServiceUnit_RequestingServiceUnitId",
                        column: x => x.RequestingServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxStockRequest_MstWorkforceProfile_RequestedByWorkforceId",
                        column: x => x.RequestedByWorkforceId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxStockRequestHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockRequestId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_TrxStockRequestHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxStockRequestHistory_TrxStockRequest_StockRequestId",
                        column: x => x.StockRequestId,
                        principalSchema: "public",
                        principalTable: "TrxStockRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxStockRequestItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeasurementId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DrugNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MeasurementNameSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RequestedQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    FulfilledQuantity = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
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
                    table.PrimaryKey("PK_TrxStockRequestItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxStockRequestItem_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxStockRequestItem_MstMeasurement_MeasurementId",
                        column: x => x.MeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxStockRequestItem_TrxStockRequest_StockRequestId",
                        column: x => x.StockRequestId,
                        principalSchema: "public",
                        principalTable: "TrxStockRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockRequest_RequestedAt",
                schema: "public",
                table: "TrxStockRequest",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockRequest_RequestedByWorkforceId",
                schema: "public",
                table: "TrxStockRequest",
                column: "RequestedByWorkforceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockRequest_RequestingServiceUnitId_Status_RequestedAt",
                schema: "public",
                table: "TrxStockRequest",
                columns: new[] { "RequestingServiceUnitId", "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockRequest_RequestNumber",
                schema: "public",
                table: "TrxStockRequest",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockRequest_StorageLocationId_Status",
                schema: "public",
                table: "TrxStockRequest",
                columns: new[] { "StorageLocationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockRequestHistory_Action_CorrelationId",
                schema: "public",
                table: "TrxStockRequestHistory",
                columns: new[] { "Action", "CorrelationId" },
                unique: true,
                filter: "\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockRequestHistory_StockRequestId_OccurredAt",
                schema: "public",
                table: "TrxStockRequestHistory",
                columns: new[] { "StockRequestId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockRequestItem_DrugId",
                schema: "public",
                table: "TrxStockRequestItem",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockRequestItem_MeasurementId",
                schema: "public",
                table: "TrxStockRequestItem",
                column: "MeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockRequestItem_StockRequestId_DrugId",
                schema: "public",
                table: "TrxStockRequestItem",
                columns: new[] { "StockRequestId", "DrugId" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxStockRequestHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxStockRequestItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxStockRequest",
                schema: "public");
        }
    }
}
