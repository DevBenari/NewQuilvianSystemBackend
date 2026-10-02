using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Memperbaiki CK_FinReceipt_TenderRequired (dibuat AddFinanceCollection) yang bertentangan
    /// dengan desain baris pembalik penerimaan yang sudah didokumentasikan FinReceipt.cs sejak
    /// awal (SourceTenderId kosong pada baris pembalik, ditunjuk lewat ReversalOfReceiptId).
    /// Versi lama mewajibkan SourceTenderId terisi untuk SourceType = BILLING_TENDER tanpa
    /// kecuali, sehingga baris pembalik (yang sengaja mengosongkan SourceTenderId supaya tidak
    /// memakai ulang slot idempotensi IX_FinReceipt_SourceTenderId milik baris asli) selalu
    /// ditolak database. Ditemukan saat review BE-FIN-016 (bagian 1.5 laporannya), diperbaiki
    /// di sini dengan menambah klausa pengecualian untuk baris pembalik.
    /// </summary>
    /// <inheritdoc />
    public partial class FixFinReceiptTenderRequiredForReversal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"FinReceipt\" DROP CONSTRAINT \"CK_FinReceipt_TenderRequired\";");
            migrationBuilder.Sql(
                "ALTER TABLE \"FinReceipt\" ADD CONSTRAINT \"CK_FinReceipt_TenderRequired\" " +
                "CHECK (\"SourceType\" <> 'BILLING_TENDER' OR \"SourceTenderId\" IS NOT NULL OR \"ReversalOfReceiptId\" IS NOT NULL);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"FinReceipt\" DROP CONSTRAINT \"CK_FinReceipt_TenderRequired\";");
            migrationBuilder.Sql(
                "ALTER TABLE \"FinReceipt\" ADD CONSTRAINT \"CK_FinReceipt_TenderRequired\" " +
                "CHECK (\"SourceType\" <> 'BILLING_TENDER' OR \"SourceTenderId\" IS NOT NULL);");
        }
    }
}
