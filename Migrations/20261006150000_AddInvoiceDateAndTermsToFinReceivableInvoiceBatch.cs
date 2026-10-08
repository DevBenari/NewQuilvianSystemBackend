using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Menambahkan metadata dokumen dan tenor jatuh tempo (InvoiceDate, DueDate, PaymentTermDays, Note)
    /// pada FinReceivableInvoiceBatch untuk mendukung UX Buat Tagihan Billing V1 pada domain V2.
    /// Seluruh kolom bersifat nullable dan murni aditif.
    /// </summary>
    public partial class AddInvoiceDateAndTermsToFinReceivableInvoiceBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "InvoiceDate",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DueDate",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentTermDays",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceDate",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "DueDate",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "PaymentTermDays",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "Note",
                schema: "public",
                table: "FinReceivableInvoiceBatch");
        }
    }
}
