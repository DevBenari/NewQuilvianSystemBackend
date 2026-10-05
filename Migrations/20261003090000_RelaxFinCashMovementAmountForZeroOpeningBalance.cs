using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Melonggarkan CK_FinCashMovement_Amount (FIN-VAL-168) hanya untuk mutasi SALDO-AWAL bernilai nol,
    /// agar penguncian saldo awal Kas Kasir yang kosong tetap meninggalkan satu jejak di buku kas (BE-FIN-084).
    /// Nilai negatif tetap ditolak untuk semua jenis mutasi; jenis lain tetap harus lebih besar dari nol.
    /// </summary>
    public partial class RelaxFinCashMovementAmountForZeroOpeningBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FinCashMovement_Amount",
                schema: "public",
                table: "FinCashMovement");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinCashMovement_Amount",
                schema: "public",
                table: "FinCashMovement",
                sql: "\"Amount\" > 0 OR (\"MovementType\" = 'SALDO-AWAL' AND \"Amount\" = 0)");
        }

        /// <inheritdoc />
        /// <remarks>
        /// Down GAGAL bila sudah ada mutasi SALDO-AWAL bernilai nol, karena constraint lama menolaknya.
        /// Sengaja tidak menghapus baris apa pun secara otomatis: buku mutasi bersifat append-only, sehingga
        /// penanganan baris nol harus diputuskan manusia sebelum Down dijalankan.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FinCashMovement_Amount",
                schema: "public",
                table: "FinCashMovement");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinCashMovement_Amount",
                schema: "public",
                table: "FinCashMovement",
                sql: "\"Amount\" > 0");
        }
    }
}
