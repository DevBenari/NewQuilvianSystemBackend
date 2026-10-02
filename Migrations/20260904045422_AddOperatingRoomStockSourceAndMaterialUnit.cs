using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddOperatingRoomStockSourceAndMaterialUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceOprMaterialUsageId",
                schema: "public",
                table: "TrxDrugReturn",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CorrectionOfUsageId",
                schema: "public",
                table: "OprMaterialUsage",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UnitMeasurementId",
                schema: "public",
                table: "OprMaterialUsage",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstOperatingRoomStockSource",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstOperatingRoomStockSource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstOperatingRoomStockSource_MstDrugStorageLocation_StorageL~",
                        column: x => x.StorageLocationId,
                        principalSchema: "public",
                        principalTable: "MstDrugStorageLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstOperatingRoomStockSource_MstRoom_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "public",
                        principalTable: "MstRoom",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugReturn_SourceOprMaterialUsageId",
                schema: "public",
                table: "TrxDrugReturn",
                column: "SourceOprMaterialUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_OprMaterialUsage_CorrectionOfUsageId",
                schema: "public",
                table: "OprMaterialUsage",
                column: "CorrectionOfUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_OprMaterialUsage_UnitMeasurementId",
                schema: "public",
                table: "OprMaterialUsage",
                column: "UnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstOperatingRoomStockSource_RoomId",
                schema: "public",
                table: "MstOperatingRoomStockSource",
                column: "RoomId",
                unique: true,
                filter: "\"IsActive\" = TRUE AND \"IsDelete\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_MstOperatingRoomStockSource_StorageLocationId",
                schema: "public",
                table: "MstOperatingRoomStockSource",
                column: "StorageLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_OprMaterialUsage_MstMeasurement_UnitMeasurementId",
                schema: "public",
                table: "OprMaterialUsage",
                column: "UnitMeasurementId",
                principalSchema: "public",
                principalTable: "MstMeasurement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OprMaterialUsage_MstMeasurement_UnitMeasurementId",
                schema: "public",
                table: "OprMaterialUsage");

            migrationBuilder.DropTable(
                name: "MstOperatingRoomStockSource",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_TrxDrugReturn_SourceOprMaterialUsageId",
                schema: "public",
                table: "TrxDrugReturn");

            migrationBuilder.DropIndex(
                name: "IX_OprMaterialUsage_CorrectionOfUsageId",
                schema: "public",
                table: "OprMaterialUsage");

            migrationBuilder.DropIndex(
                name: "IX_OprMaterialUsage_UnitMeasurementId",
                schema: "public",
                table: "OprMaterialUsage");

            migrationBuilder.DropColumn(
                name: "SourceOprMaterialUsageId",
                schema: "public",
                table: "TrxDrugReturn");

            migrationBuilder.DropColumn(
                name: "CorrectionOfUsageId",
                schema: "public",
                table: "OprMaterialUsage");

            migrationBuilder.DropColumn(
                name: "UnitMeasurementId",
                schema: "public",
                table: "OprMaterialUsage");
        }
    }
}
