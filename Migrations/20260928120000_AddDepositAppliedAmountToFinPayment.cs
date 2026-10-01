using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-041 (FIN-DES-045, 02-backend-architecture.md §D.6, D.7 baris 3, erd/data-dictionary.md
    /// §D.1/§D.4(a)). Menyiapkan tempat mencatat porsi pembayaran supplier yang dilunasi Deposit
    /// Retur (BE-FIN-036) pada FinPayment — TABEL YANG SUDAH BERJALAN. Seluruh baris lama bernilai
    /// DepositAppliedAmount = 0, sehingga CK_FinPayment_NetTransfer bentuk baru langsung terpenuhi
    /// tanpa backfill — NetTransferAmount hasil hitungan service untuk pembayaran lama tidak berubah.
    /// Nol perilaku berubah sebelum BE-FIN-036 menulis nilai selain 0 ke kolom ini.
    /// </summary>
    /// <inheritdoc />
    public partial class AddDepositAppliedAmountToFinPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DepositAppliedAmount",
                schema: "public",
                table: "FinPayment",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                "ALTER TABLE \"FinPayment\" DROP CONSTRAINT \"CK_FinPayment_NetTransfer\";");
            migrationBuilder.Sql(
                "ALTER TABLE \"FinPayment\" ADD CONSTRAINT \"CK_FinPayment_NetTransfer\" " +
                "CHECK (\"NetTransferAmount\" = \"TotalAmount\" - \"DeductionAmount\" + \"AdditionAmount\" - \"DepositAppliedAmount\");");
            migrationBuilder.Sql(
                "ALTER TABLE \"FinPayment\" ADD CONSTRAINT \"CK_FinPayment_DepositApplied\" " +
                "CHECK (\"DepositAppliedAmount\" >= 0);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // AMAN HANYA selama belum ada baris FinPayment ber-DepositAppliedAmount > 0 (lihat
            // ringkasan kelas dan laporan task) — bila sudah ada, DROP COLUMN akan membuang nilai
            // yang sudah tercatat.
            migrationBuilder.Sql(
                "ALTER TABLE \"FinPayment\" DROP CONSTRAINT \"CK_FinPayment_DepositApplied\";");
            migrationBuilder.Sql(
                "ALTER TABLE \"FinPayment\" DROP CONSTRAINT \"CK_FinPayment_NetTransfer\";");
            migrationBuilder.Sql(
                "ALTER TABLE \"FinPayment\" ADD CONSTRAINT \"CK_FinPayment_NetTransfer\" " +
                "CHECK (\"NetTransferAmount\" = \"TotalAmount\" - \"DeductionAmount\" + \"AdditionAmount\");");

            migrationBuilder.DropColumn(
                name: "DepositAppliedAmount",
                schema: "public",
                table: "FinPayment");
        }
    }
}
