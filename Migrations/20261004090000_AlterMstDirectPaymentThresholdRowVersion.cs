using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-086 (FIN-DES-094, FIN-DEC-145/146/153): menambah penanda versi optimistic concurrency
    /// pada MstDirectPaymentThreshold, dan membuang kolom EffectiveFrom.
    ///
    /// Kenapa EffectiveFrom dibuang: ambang pembayaran langsung SELALU berlaku seketika. Kolom itu
    /// menjanjikan perubahan terjadwal yang tidak pernah ditegakkan siapa pun — tidak ada satu pun
    /// pembaca yang membandingkannya dengan tanggal hari ini. Penggantinya BUKAN kolom lain: tanggal
    /// perubahan terakhir dibaca dari kolom audit IdentityModel.
    ///
    /// Kenapa satu migration, bukan dua: kedua perubahan menyentuh tabel yang sama (FIN-DES-094).
    /// </summary>
    /// <remarks>
    /// URUTAN RILIS YANG MUST DIPATUHI. Migration ini membuang kolom dari tabel yang sudah ada,
    /// sehingga ia MUST diterapkan BERSAMAAN dengan rilis aplikasi yang sudah berhenti memakai
    /// EffectiveFrom (rilis BE-FIN-086). Aplikasi versi lama yang masih memetakan kolom itu akan
    /// gagal membaca tabel ini setelah migration berjalan.
    ///
    /// Risikonya kecil dalam keadaan sekarang, dan itu disengaja: tabel ini direncanakan NOL BARIS
    /// sampai pejabat berwenang menetapkan ambang pertama lewat layar (FIN-DEC-154, prasyarat go-live
    /// pada 04-prd-to-mvp.md 56.6), dan satu-satunya pembaca EffectiveFrom adalah
    /// DirectPaymentThresholdService yang ikut berubah pada rilis yang sama.
    ///
    /// Berkas ini DITULIS MANUAL atas permintaan pengguna, tanpa `dotnet ef migrations add`.
    /// Snapshot dan Designer disesuaikan pada perubahan yang sama. Kesesuaiannya MUST dibuktikan
    /// `dotnet ef migrations has-pending-model-changes` sebelum diterapkan.
    /// </remarks>
    public partial class AlterMstDirectPaymentThresholdRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // (1) Kolom ditambah NOT NULL dengan bawaan gen_random_uuid() supaya baris yang sudah ada
            // — bila ternyata ada — ikut terisi nilai yang BERBEDA satu dari yang lain. Bawaan statis
            // akan memberi semua baris versi yang sama, dan penanda versi kehilangan gunanya.
            migrationBuilder.AddColumn<Guid>(
                name: "RowVersion",
                schema: "public",
                table: "MstDirectPaymentThreshold",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            // (2) Bawaan dicabut sesudah terisi, supaya nilai berikutnya datang dari aplikasi
            // (DirectPaymentThresholdService memutarnya pada setiap penyimpanan), bukan dari database.
            // Dibiarkan terpasang, database akan diam-diam mengisi versi baru pada INSERT dan
            // membuat pemeriksaan ConcurrencyCheck tidak dapat diandalkan.
            migrationBuilder.Sql(
                "ALTER TABLE public.\"MstDirectPaymentThreshold\" ALTER COLUMN \"RowVersion\" DROP DEFAULT;");

            // (3) Kolom yang dibuang FIN-DEC-153.
            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                schema: "public",
                table: "MstDirectPaymentThreshold");
        }

        /// <inheritdoc />
        /// <remarks>
        /// Down mengembalikan BENTUK kolom, bukan ISINYA. Nilai tanggal lama TIDAK DAPAT dikembalikan —
        /// ia memang dibuang FIN-DEC-153, dan tidak disalin ke mana pun lebih dulu. Baris yang sudah
        /// ada akan menerima CURRENT_DATE, yang berarti "tidak diketahui", bukan tanggal aslinya.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveFrom",
                schema: "public",
                table: "MstDirectPaymentThreshold",
                type: "date",
                nullable: false,
                defaultValueSql: "CURRENT_DATE");

            // Bawaan dicabut supaya bentuk kolomnya kembali persis seperti sebelum migration ini:
            // `date NOT NULL` tanpa bawaan (lihat 20261002120000_AddFinanceTransactionProofAndDirectPaymentThreshold).
            migrationBuilder.Sql(
                "ALTER TABLE public.\"MstDirectPaymentThreshold\" ALTER COLUMN \"EffectiveFrom\" DROP DEFAULT;");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "public",
                table: "MstDirectPaymentThreshold");
        }
    }
}
