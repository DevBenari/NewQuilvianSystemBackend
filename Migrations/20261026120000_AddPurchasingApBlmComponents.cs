using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Amendment REV-22 (Finance AP mengikuti BLM): menyiapkan tempat lima data baru pada tabel
    /// Purchasing/AP yang SUDAH BERJALAN. Seluruhnya aditif dan dapat dijalankan tanpa downtime.
    ///
    /// BE-FIN-123 (AP-UAT-10): FinPurchasingInvoice.StampDutyAmount dan ShippingCostAmount, komponen
    /// penambah total faktur. Ber-default 0 sehingga invoice lama tetap memenuhi rumus total tanpa backfill.
    ///
    /// BE-FIN-124 (AP-UAT-11): FinSupplierPayable.DueDateBasis, tanggal acuan perhitungan jatuh tempo
    /// (SUPPLIER_INVOICE_DATE atau GOODS_RECEIVED_DATE). NULL untuk utang lama dan saldo awal migrasi:
    /// basisnya tidak tercatat, DueDate tidak berubah.
    ///
    /// BE-FIN-125 (AP-UAT-06): FinGoodsReceiptItem.BatchNumber dan ExpiredDate, nullable. Penerimaan lama
    /// tidak punya data batch dan tetap sah.
    /// </summary>
    /// <inheritdoc />
    public partial class AddPurchasingApBlmComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingCostAmount",
                schema: "public",
                table: "FinPurchasingInvoice",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StampDutyAmount",
                schema: "public",
                table: "FinPurchasingInvoice",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DueDateBasis",
                schema: "public",
                table: "FinSupplierPayable",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                schema: "public",
                table: "FinGoodsReceiptItem",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiredDate",
                schema: "public",
                table: "FinGoodsReceiptItem",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // AMAN HANYA selama kolom-kolom ini belum berisi data. Bila sudah ada invoice ber-materai
            // atau ber-ongkir, DROP COLUMN membuang komponen itu sementara TotalAmount tetap memuatnya,
            // sehingga rumus total tidak lagi dapat dijelaskan dari kolom yang tersisa.
            migrationBuilder.DropColumn(
                name: "ExpiredDate",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                schema: "public",
                table: "FinGoodsReceiptItem");

            migrationBuilder.DropColumn(
                name: "DueDateBasis",
                schema: "public",
                table: "FinSupplierPayable");

            migrationBuilder.DropColumn(
                name: "StampDutyAmount",
                schema: "public",
                table: "FinPurchasingInvoice");

            migrationBuilder.DropColumn(
                name: "ShippingCostAmount",
                schema: "public",
                table: "FinPurchasingInvoice");
        }
    }
}