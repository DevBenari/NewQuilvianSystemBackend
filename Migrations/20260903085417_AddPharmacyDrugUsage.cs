using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyDrugUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrxDrugUsage",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsageNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedByWorkforceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BilledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_TrxDrugUsage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxDrugUsage_MstDrugStorageLocation_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalSchema: "public",
                        principalTable: "MstDrugStorageLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugUsage_MstWorkforceProfile_RecordedByWorkforceId",
                        column: x => x.RecordedByWorkforceId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugUsage_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxDrugUsageItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugUsageId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeasurementId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DrugNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MeasurementNameSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
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
                    table.PrimaryKey("PK_TrxDrugUsageItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxDrugUsageItem_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugUsageItem_MstMeasurement_MeasurementId",
                        column: x => x.MeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugUsageItem_TrxDrugUsage_DrugUsageId",
                        column: x => x.DrugUsageId,
                        principalSchema: "public",
                        principalTable: "TrxDrugUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrxDrugUsageAllocation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugUsageItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
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
                    table.PrimaryKey("PK_TrxDrugUsageAllocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxDrugUsageAllocation_MstDrugBatch_DrugBatchId",
                        column: x => x.DrugBatchId,
                        principalSchema: "public",
                        principalTable: "MstDrugBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxDrugUsageAllocation_TrxDrugUsageItem_DrugUsageItemId",
                        column: x => x.DrugUsageItemId,
                        principalSchema: "public",
                        principalTable: "TrxDrugUsageItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugUsage_EncounterId_UsedAt",
                schema: "public",
                table: "TrxDrugUsage",
                columns: new[] { "EncounterId", "UsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugUsage_RecordedByWorkforceId",
                schema: "public",
                table: "TrxDrugUsage",
                column: "RecordedByWorkforceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugUsage_Status_UsedAt",
                schema: "public",
                table: "TrxDrugUsage",
                columns: new[] { "Status", "UsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugUsage_StorageLocationId_UsedAt",
                schema: "public",
                table: "TrxDrugUsage",
                columns: new[] { "StorageLocationId", "UsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugUsage_UsageNumber",
                schema: "public",
                table: "TrxDrugUsage",
                column: "UsageNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugUsageAllocation_DrugBatchId",
                schema: "public",
                table: "TrxDrugUsageAllocation",
                column: "DrugBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugUsageAllocation_DrugUsageItemId_SequenceNumber",
                schema: "public",
                table: "TrxDrugUsageAllocation",
                columns: new[] { "DrugUsageItemId", "SequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugUsageItem_DrugId",
                schema: "public",
                table: "TrxDrugUsageItem",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugUsageItem_DrugUsageId",
                schema: "public",
                table: "TrxDrugUsageItem",
                column: "DrugUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugUsageItem_MeasurementId",
                schema: "public",
                table: "TrxDrugUsageItem",
                column: "MeasurementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxDrugUsageAllocation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxDrugUsageItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxDrugUsage",
                schema: "public");
        }
    }
}
