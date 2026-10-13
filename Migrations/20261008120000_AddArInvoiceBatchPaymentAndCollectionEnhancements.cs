using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Menambahkan kolom VisitCode, metadata Diskon Invoice, dan Penerimaan Lainnya pada FinReceivableInvoiceBatch;
    /// BankAccountId pada FinReceipt; serta ChartOfAccountId pada FinReceiptDeduction.
    /// Seluruh kolom aditif bersifat nullable atau memiliki nilai bawaan default 0m.
    /// </summary>
    /// <inheritdoc />
    public partial class AddArInvoiceBatchPaymentAndCollectionEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. FinReceivableInvoiceBatch
            migrationBuilder.AddColumn<string>(
                name: "VisitCode",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDiscount",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "DiscountChartOfAccountId",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountType",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountNote",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherReceiptAmount",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "OtherReceiptChartOfAccountId",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherReceiptNote",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            // 2. FinReceipt
            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                schema: "public",
                table: "FinReceipt",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinReceipt_BankAccountId",
                schema: "public",
                table: "FinReceipt",
                column: "BankAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinReceipt_MstBankAccount_BankAccountId",
                schema: "public",
                table: "FinReceipt",
                column: "BankAccountId",
                principalSchema: "public",
                principalTable: "MstBankAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 3. FinReceiptDeduction
            migrationBuilder.AddColumn<Guid>(
                name: "ChartOfAccountId",
                schema: "public",
                table: "FinReceiptDeduction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinReceiptDeduction_ChartOfAccountId",
                schema: "public",
                table: "FinReceiptDeduction",
                column: "ChartOfAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinReceiptDeduction_AccChartOfAccount_ChartOfAccountId",
                schema: "public",
                table: "FinReceiptDeduction",
                column: "ChartOfAccountId",
                principalSchema: "public",
                principalTable: "AccChartOfAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinReceiptDeduction_AccChartOfAccount_ChartOfAccountId",
                schema: "public",
                table: "FinReceiptDeduction");

            migrationBuilder.DropIndex(
                name: "IX_FinReceiptDeduction_ChartOfAccountId",
                schema: "public",
                table: "FinReceiptDeduction");

            migrationBuilder.DropColumn(
                name: "ChartOfAccountId",
                schema: "public",
                table: "FinReceiptDeduction");

            migrationBuilder.DropForeignKey(
                name: "FK_FinReceipt_MstBankAccount_BankAccountId",
                schema: "public",
                table: "FinReceipt");

            migrationBuilder.DropIndex(
                name: "IX_FinReceipt_BankAccountId",
                schema: "public",
                table: "FinReceipt");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                schema: "public",
                table: "FinReceipt");

            migrationBuilder.DropColumn(
                name: "VisitCode",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "TotalDiscount",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "DiscountChartOfAccountId",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "DiscountType",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "DiscountNote",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "OtherReceiptAmount",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "OtherReceiptChartOfAccountId",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "OtherReceiptNote",
                schema: "public",
                table: "FinReceivableInvoiceBatch");
        }
    }
}
