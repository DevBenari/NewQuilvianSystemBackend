using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-052 (FIN-DEC-097, FIN-DES-070/071, 02-backend-architecture.md AMENDMENT REVISI 13
    /// §J). Menambahkan sumbu klaim penjamin pada FinReceivableInvoiceBatch — TABEL YANG SUDAH
    /// BERJALAN.
    ///
    /// ClaimStatus adalah sumbu KEDUA, terpisah dari Status: Status menjawab "apakah dokumen
    /// terbit dan apakah lunas" (penulis Sistem untuk pelunasan), ClaimStatus menjawab "apa kata
    /// penjamin" (penulis petugas AR, kecuali SUBMITTED yang ditulis Sistem saat IssueAsync).
    /// ClaimVarianceAmount (selisih TotalAmount - ApprovedAmount) SENGAJA TIDAK mendapat kolom —
    /// dihitung pada response, tidak disimpan (FIN-DES-071).
    ///
    /// Aditif murni dan dapat dijalankan tanpa downtime: ketujuh kolom nullable, nol kolom wajib,
    /// nol perubahan tipe, nol rename. Batch lama langsung valid terhadap
    /// CK_FinReceivableInvoiceBatch_ClaimStatus tanpa backfill — ClaimStatus kosong dibaca sebagai
    /// "belum diterbitkan ke penjamin" untuk batch DRAFT, dan untuk batch ISSUED lama,
    /// FinanceReceivableInvoiceBatchService.ResolveEffectiveClaimStatus memperlakukannya seolah
    /// SUBMITTED tanpa menulis apa pun ke baris lama.
    /// </summary>
    /// <inheritdoc />
    public partial class AddClaimTrackingToFinReceivableInvoiceBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedAmount",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClaimApprovedAt",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClaimClosedAt",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaimNote",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaimStatus",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayerClaimReference",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PayerVerifiedAt",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                "ALTER TABLE \"FinReceivableInvoiceBatch\" ADD CONSTRAINT \"CK_FinReceivableInvoiceBatch_ClaimStatus\" " +
                "CHECK (\"ClaimStatus\" IS NULL OR \"ClaimStatus\" IN ('SUBMITTED','PAYER_VERIFIED','APPROVED','CLOSED'));");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInvoiceBatch_ClaimStatus",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                column: "ClaimStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinReceivableInvoiceBatch_ClaimStatus",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.Sql(
                "ALTER TABLE \"FinReceivableInvoiceBatch\" DROP CONSTRAINT \"CK_FinReceivableInvoiceBatch_ClaimStatus\";");

            migrationBuilder.DropColumn(
                name: "ApprovedAmount",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "ClaimApprovedAt",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "ClaimClosedAt",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "ClaimNote",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "ClaimStatus",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "PayerClaimReference",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.DropColumn(
                name: "PayerVerifiedAt",
                schema: "public",
                table: "FinReceivableInvoiceBatch");
        }
    }
}
