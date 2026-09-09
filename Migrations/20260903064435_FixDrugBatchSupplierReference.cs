using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixDrugBatchSupplierReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstDrugBatch_MstDrugSupplier_DrugSupplierId",
                schema: "public",
                table: "MstDrugBatch");

            migrationBuilder.RenameColumn(
                name: "DrugSupplierId",
                schema: "public",
                table: "MstDrugBatch",
                newName: "SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_MstDrugBatch_DrugSupplierId",
                schema: "public",
                table: "MstDrugBatch",
                newName: "IX_MstDrugBatch_SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstDrugBatch_MstSupplier_SupplierId",
                schema: "public",
                table: "MstDrugBatch",
                column: "SupplierId",
                principalSchema: "public",
                principalTable: "MstSupplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstDrugBatch_MstSupplier_SupplierId",
                schema: "public",
                table: "MstDrugBatch");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                schema: "public",
                table: "MstDrugBatch",
                newName: "DrugSupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_MstDrugBatch_SupplierId",
                schema: "public",
                table: "MstDrugBatch",
                newName: "IX_MstDrugBatch_DrugSupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstDrugBatch_MstDrugSupplier_DrugSupplierId",
                schema: "public",
                table: "MstDrugBatch",
                column: "DrugSupplierId",
                principalSchema: "public",
                principalTable: "MstDrugSupplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
