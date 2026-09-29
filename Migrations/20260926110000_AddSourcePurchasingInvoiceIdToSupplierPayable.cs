using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-031 (BE-FIN-030, FIN-DES-040). Menghubungkan FinSupplierPayable existing ke
    /// FinPurchasingInvoice yang baru dibuat migration sebelumnya (AddPurchasingApRumpun).
    /// SourcePurchasingInvoiceId NULL untuk baris lama/manual — jalur input manual TIDAK
    /// dihapus (lihat FinSupplierPayableConfiguration.cs). Dipisah dari migration pembuatan
    /// tabel supaya migration itu tidak menyentuh tabel yang sudah berjalan.
    /// </summary>
    /// <inheritdoc />
    public partial class AddSourcePurchasingInvoiceIdToSupplierPayable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourcePurchasingInvoiceId",
                schema: "public",
                table: "FinSupplierPayable",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierPayable_SourcePurchasingInvoiceId",
                schema: "public",
                table: "FinSupplierPayable",
                column: "SourcePurchasingInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinSupplierPayable_SourcePurchasingInvoiceId",
                schema: "public",
                table: "FinSupplierPayable",
                column: "SourcePurchasingInvoiceId",
                principalSchema: "public",
                principalTable: "FinPurchasingInvoice",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinSupplierPayable_SourcePurchasingInvoiceId",
                schema: "public",
                table: "FinSupplierPayable");

            migrationBuilder.DropIndex(
                name: "IX_FinSupplierPayable_SourcePurchasingInvoiceId",
                schema: "public",
                table: "FinSupplierPayable");

            migrationBuilder.DropColumn(
                name: "SourcePurchasingInvoiceId",
                schema: "public",
                table: "FinSupplierPayable");
        }
    }
}
