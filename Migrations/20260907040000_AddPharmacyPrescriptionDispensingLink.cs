using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260907040000_AddPharmacyPrescriptionDispensingLink")]
    public partial class AddPharmacyPrescriptionDispensingLink : Migration
    {
        // Menautkan pemakaian obat ke resep yang diserahkannya.
        //
        // Penyerahan obat resep dicatat sebagai `TrxDrugUsage`, bukan sebagai tabel penyerahan
        // tersendiri. Obat yang keluar dari stok untuk seorang pasien hanya boleh punya satu
        // catatan yang authoritative; tabel kedua akan membuat pertanyaan "berapa yang sudah
        // diserahkan" punya dua jawaban yang bisa berbeda.
        //
        // Kedua kolom nullable. Pemakaian yang tidak berasal dari resep — obat bangsal dan
        // material kamar operasi — mengosongkannya, dan alur keduanya tidak berubah sama sekali.
        //
        // DITULIS TANGAN, mengikuti konvensi migration tulis-tangan yang sudah ada di
        // repository ini (atribut `[Migration]` pada berkas utama, tanpa Designer).
        //
        // Alasannya: snapshot model sempat ter-revert ke keadaan pra-merge oleh
        // `dotnet ef migrations remove`, sehingga migration hasil generate memuat pembongkaran
        // tabel `TrxClinicalMilestoneFact`, `TrxLabSpecimen`, dan `TrxLabTransitionHistory`
        // yang sudah dinamai ulang oleh migration branch integration. Snapshot sudah
        // diselaraskan kembali dengan model; sesudah itu generator menghasilkan migration
        // kosong karena snapshot memang sudah memuat kedua kolom ini. Yang tersisa hanyalah
        // menuliskan perubahan fisiknya, dan itu dikerjakan di sini secara eksplisit supaya
        // isinya dapat dibaca dan diperiksa apa adanya.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PrescriptionId",
                schema: "public",
                table: "TrxDrugUsage",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrescriptionItemId",
                schema: "public",
                table: "TrxDrugUsageItem",
                type: "uuid",
                nullable: true);

            // Terfilter: indeksnya hanya memuat penyerahan resep, bukan seluruh pemakaian
            // bangsal dan kamar operasi yang kolomnya kosong.
            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugUsage_PrescriptionId",
                schema: "public",
                table: "TrxDrugUsage",
                column: "PrescriptionId",
                filter: "\"PrescriptionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDrugUsageItem_PrescriptionItemId",
                schema: "public",
                table: "TrxDrugUsageItem",
                column: "PrescriptionItemId",
                filter: "\"PrescriptionItemId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxDrugUsageItem_PrescriptionItemId",
                schema: "public",
                table: "TrxDrugUsageItem");

            migrationBuilder.DropIndex(
                name: "IX_TrxDrugUsage_PrescriptionId",
                schema: "public",
                table: "TrxDrugUsage");

            migrationBuilder.DropColumn(
                name: "PrescriptionItemId",
                schema: "public",
                table: "TrxDrugUsageItem");

            migrationBuilder.DropColumn(
                name: "PrescriptionId",
                schema: "public",
                table: "TrxDrugUsage");
        }
    }
}
