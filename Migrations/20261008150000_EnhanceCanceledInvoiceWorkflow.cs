using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Meningkatkan alur kerja Canceled Invoice dan Reissue pada modul Finance:
    /// 1. Menambahkan CancelReason, ServiceType, ReissuedFromBatchId, dan ReissuedToBatchId pada FinReceivableInvoiceBatch.
    /// 2. Menambahkan indeks performa pencarian untuk InvoiceDate, CancelDateTime, ServiceType, ReissuedFromBatchId, dan ReissuedToBatchId.
    /// 3. Menambahkan IsActiveMembership pada FinReceivableInvoiceBatchItem dan memperbarui unique index parsial
    ///    menjadi (ReceivableId WHERE IsDelete = false AND IsActiveMembership = true) agar receivable dari batch
    ///    yang dibatalkan dapat digabung ulang / di-reissue.
    /// 4. Menyertakan backfill SQL agar item batch CANCELLED yang sudah ada beralih ke IsActiveMembership = false.
    /// </summary>
    public partial class EnhanceCanceledInvoiceWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReissuedFromBatchId",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReissuedToBatchId",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveMembership",
                schema: "public",
                table: "FinReceivableInvoiceBatchItem",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatch_InvoiceDate",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                column: "InvoiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatch_CancelDateTime",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                column: "CancelDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatch_ServiceType",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                column: "ServiceType");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatch_ReissuedFromBatchId",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                column: "ReissuedFromBatchId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"ReissuedFromBatchId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatch_ReissuedToBatchId",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                column: "ReissuedToBatchId");

            migrationBuilder.DropIndex(
                name: "IX_FinReceivableInvoiceBatchItem_ActiveReceivable",
                schema: "public",
                table: "FinReceivableInvoiceBatchItem");

            // Backfill data existing: tandai seluruh item dari batch CANCELLED sebagai inactive membership
            migrationBuilder.Sql("UPDATE \"FinReceivableInvoiceBatchItem\" SET \"IsActiveMembership\" = FALSE WHERE \"BatchId\" IN (SELECT \"Id\" FROM \"FinReceivableInvoiceBatch\" WHERE \"Status\" = 'CANCELLED');");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatchItem_ActiveReceivable",
                schema: "public",
                table: "FinReceivableInvoiceBatchItem",
                column: "ReceivableId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"IsActiveMembership\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinReceivableInvoiceBatchItem_ActiveReceivable",
                schema: "public",
                table: "FinReceivableInvoiceBatchItem");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatchItem_ActiveReceivable",
                schema: "public",
                table: "FinReceivableInvoiceBatchItem",
                column: "ReceivableId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.DropColumn(
                name: "IsActiveMembership",
                schema: "public",
                table: "FinReceivableInvoiceBatchItem");

            migrationBuilder.DropIndex(
                name: "IX_FinReceivableInvoiceBatch_ReissuedToBatchId",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropIndex(
                name: "IX_FinReceivableInvoiceBatch_ReissuedFromBatchId",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropIndex(
                name: "IX_FinReceivableInvoiceBatch_ServiceType",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropIndex(
                name: "IX_FinReceivableInvoiceBatch_CancelDateTime",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropIndex(
                name: "IX_FinReceivableInvoiceBatch_InvoiceDate",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "ReissuedToBatchId",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "ReissuedFromBatchId",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                schema: "public",
                table: "FinReceivableInvoiceBatch");
        }
    }
}
