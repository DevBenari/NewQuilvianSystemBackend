using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTariffIdToBilInvoiceItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TariffId",
                schema: "public",
                table: "BilInvoiceItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoiceItem_TariffId",
                schema: "public",
                table: "BilInvoiceItem",
                column: "TariffId");

            migrationBuilder.AddForeignKey(
                name: "FK_BilInvoiceItem_MstTariff_TariffId",
                schema: "public",
                table: "BilInvoiceItem",
                column: "TariffId",
                principalSchema: "public",
                principalTable: "MstTariff",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BilInvoiceItem_MstTariff_TariffId",
                schema: "public",
                table: "BilInvoiceItem");

            migrationBuilder.DropIndex(
                name: "IX_BilInvoiceItem_TariffId",
                schema: "public",
                table: "BilInvoiceItem");

            migrationBuilder.DropColumn(
                name: "TariffId",
                schema: "public",
                table: "BilInvoiceItem");
        }
    }
}
