using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-043 (FIN-DEC-068, FIN-DES-055, 02-backend-architecture.md §E.9-E.10). Menyiapkan
    /// tempat memisahkan porsi PPN retur pembelian pada FinSupplierReturn — TABEL YANG SUDAH
    /// BERJALAN.
    ///
    /// Arti TotalAmount TIDAK berubah: ia tetap pokok tanpa PPN, yaitu jumlah
    /// FinSupplierReturnItem.LineTotal, dan itulah yang sudah dikirim kejadian RETUR-PEMBELIAN
    /// sejak BE-FIN-035. Yang ditambahkan hanya tempat menyimpan porsi PPN-nya.
    ///
    /// Aditif murni dan dapat dijalankan tanpa downtime: kolom ber-default 0, sehingga seluruh
    /// baris lama langsung memenuhi CK_FinSupplierReturn_PPNAmount tanpa backfill. Kredit retur
    /// (FinSupplierReturnDeposit.AvailableAmount) untuk retur lama bernilai TotalAmount + 0 =
    /// identik dengan sebelum migration ini.
    /// </summary>
    /// <inheritdoc />
    public partial class AddPPNAmountToFinSupplierReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PPNAmount",
                schema: "public",
                table: "FinSupplierReturn",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                "ALTER TABLE \"FinSupplierReturn\" ADD CONSTRAINT \"CK_FinSupplierReturn_PPNAmount\" " +
                "CHECK (\"PPNAmount\" >= 0);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // AMAN HANYA selama belum ada baris FinSupplierReturn ber-PPNAmount > 0. Bila sudah
            // ada, DROP COLUMN membuang nilai PPN yang sudah tercatat, dan kredit retur
            // (FinSupplierReturnDeposit) yang terlanjur terbit sebesar TotalAmount + PPNAmount
            // MUST dihitung ulang lebih dulu — lihat 02-backend-architecture.md §E.10.
            migrationBuilder.Sql(
                "ALTER TABLE \"FinSupplierReturn\" DROP CONSTRAINT \"CK_FinSupplierReturn_PPNAmount\";");

            migrationBuilder.DropColumn(
                name: "PPNAmount",
                schema: "public",
                table: "FinSupplierReturn");
        }
    }
}
