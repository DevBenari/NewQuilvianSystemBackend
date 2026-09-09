using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddStockTransferAllocationSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxStockTransferAllocation_StockTransferItemId",
                schema: "public",
                table: "TrxStockTransferAllocation");

            migrationBuilder.AddColumn<int>(
                name: "SequenceNumber",
                schema: "public",
                table: "TrxStockTransferAllocation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockTransferAllocation_StockTransferItemId_SequenceNumb~",
                schema: "public",
                table: "TrxStockTransferAllocation",
                columns: new[] { "StockTransferItemId", "SequenceNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxStockTransferAllocation_StockTransferItemId_SequenceNumb~",
                schema: "public",
                table: "TrxStockTransferAllocation");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                schema: "public",
                table: "TrxStockTransferAllocation");

            migrationBuilder.CreateIndex(
                name: "IX_TrxStockTransferAllocation_StockTransferItemId",
                schema: "public",
                table: "TrxStockTransferAllocation",
                column: "StockTransferItemId");
        }
    }
}
