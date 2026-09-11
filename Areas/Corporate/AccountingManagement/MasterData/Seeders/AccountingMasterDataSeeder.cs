using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Seeders
{
    /// <summary>
    /// Mengisi data master Accounting agar modul dapat dipakai. Seeder ini hanya menambah baris
    /// yang belum ada berdasarkan JournalTypeCode, dan tidak pernah menimpa baris yang sudah
    /// tersimpan, supaya nilai yang sudah disesuaikan admin tidak hilang saat aplikasi
    /// dijalankan ulang.
    /// </summary>
    /// <remarks>
    /// Tiga batas yang mengikat seeder ini:
    ///
    /// 1. Isinya berasal dari <c>02-backend-architecture.md</c> bagian 9.1, bukan dari tebakan.
    ///    Empat jenis jurnal itu masing-masing punya keputusan pendukungnya sendiri:
    ///    <c>ACC-DEC-010</c> (JU), <c>ACC-DEC-017</c> (JP), <c>ACC-DEC-029</c> (JB), serta
    ///    <c>ACC-DEC-018</c> dan <c>ACC-DEC-033</c> (SA).
    /// 2. Seeder TIDAK PERNAH mengisi <c>AccChartOfAccount</c> maupun <c>AccAccountingPeriod</c>.
    ///    Daftar akun adalah kebijakan akuntansi rumah sakit dan wajib disusun pemilik proses
    ///    (bagian 9.3), sedangkan periode dibangkitkan lewat endpoint <c>POST /generate</c> pada
    ///    <c>BE-ACC-009</c> (bagian 9.2). Menebak keduanya menghasilkan master palsu yang
    ///    terlanjur dipakai pembukuan.
    /// 3. Roadmap <c>BE-ACC-006</c> melarang pengisian lewat skrip SQL manual. Karena itu
    ///    pengisian dilakukan lewat aplikasi, di sini.
    /// </remarks>
    public static class AccountingMasterDataSeeder
    {
        public static async Task<AccountingMasterDataSeedResult> SeedAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var result = new AccountingMasterDataSeedResult();
            var now = DateTime.UtcNow;

            await SeedJournalTypesAsync(db, actorUserId, now, result, ct);

            return result;
        }

        /// <summary>
        /// Menentukan apakah sebuah master sudah diisi sumber lain.
        /// </summary>
        /// <remarks>
        /// Seeder ini mengisi master yang kosong; ia tidak menggabungkan versinya sendiri ke
        /// dalam master yang sudah dipakai. Bila di tabel ada kode yang tidak dikenal daftar
        /// seeder — misalnya "GJ" sementara seeder mengenal "JU" — artinya isinya berasal dari
        /// sumber lain, dan menambah daftar seeder ke sana menghasilkan dua set jenis jurnal
        /// yang hidup berdampingan.
        ///
        /// Akibatnya di sini lebih keras daripada di master IGD. NumberPrefix menjadi awalan
        /// nomor jurnal, sehingga dua set jenis jurnal berarti dua skema penomoran berjalan
        /// bersamaan di atas satu buku besar. Laporan yang dikelompokkan menurut jenis jurnal
        /// menjadi salah tanpa ada yang menyadarinya, dan nomor jurnal berhenti dapat dibaca
        /// sebagai penanda jenisnya.
        ///
        /// Menjalankan seeder dua kali atas datanya sendiri tetap aman: seluruh kode yang ada
        /// dikenal, sehingga tidak ada yang dianggap asing.
        /// </remarks>
        private static bool IsOwnedByAnotherSource(
            IEnumerable<string> existingCodes,
            IEnumerable<string> definitionCodes)
        {
            var known = new HashSet<string>(definitionCodes, StringComparer.OrdinalIgnoreCase);

            return existingCodes.Any(code => !known.Contains(code));
        }

        // ------------------------------------------------------------------
        // Jenis jurnal — empat baris, 02-backend-architecture.md bagian 9.1
        // ------------------------------------------------------------------

        /// <remarks>
        /// Keempatnya berstatus RequiresApproval, mengikuti ACC-DEC-010: jurnal manual selalu
        /// melewati pemeriksaan orang kedua, tanpa pengecualian jenis.
        ///
        /// IsSystemType hanya pada JB dan SA. Keduanya lahir dari langkah yang dikendalikan
        /// sistem — pembalikan jurnal dan pembukaan saldo awal — sehingga kode maupun awalan
        /// nomornya tidak boleh diubah admin lewat BE-ACC-008. JU dan JP sebaliknya memang
        /// milik pemilik proses dan boleh disesuaikan.
        /// </remarks>
        private static async Task SeedJournalTypesAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            AccountingMasterDataSeedResult result,
            CancellationToken ct)
        {
            var definitions = new[]
            {
                new JournalTypeDefinition("JU", "Jurnal Umum", "JU", true, false),
                new JournalTypeDefinition("JP", "Jurnal Penyesuaian", "JP", true, false),
                new JournalTypeDefinition("JB", "Jurnal Pembalik", "JB", true, true),
                new JournalTypeDefinition("SA", "Saldo Awal", "SA", true, true)
            };

            var existingRows = await db.Set<AccJournalType>()
                .Select(x => new { x.JournalTypeCode, x.IsDelete })
                .ToListAsync(ct);

            // Penjaga kepemilikan membaca baris yang masih hidup saja. Kode asing yang sudah
            // dihapus lunak tidak sedang dipakai siapa pun, jadi ia tidak boleh mengunci
            // seeder dari mengisi master yang sebenarnya kosong.
            if (IsOwnedByAnotherSource(
                    existingRows.Where(x => !x.IsDelete).Select(x => x.JournalTypeCode),
                    definitions.Select(d => d.JournalTypeCode)))
            {
                result.JournalTypeSkippedReason =
                    "Master jenis jurnal sudah diisi sumber lain; seeder tidak menambah apa pun " +
                    "supaya tidak ada dua skema penomoran jurnal yang berjalan bersamaan.";
                return;
            }

            // Idempotensi sebaliknya membaca SELURUH baris, termasuk yang sudah dihapus lunak.
            //
            // Unique index IX_AccJournalType_JournalTypeCode disaring "IsDelete" = false,
            // sehingga menyisipkan ulang kode yang pernah dihapus TIDAK akan menabrak index.
            // Jadi pilihan ini bukan soal menghindari tabrakan, melainkan soal menghormati
            // keputusan admin: baris yang sengaja dihapus tidak dihidupkan lagi diam-diam
            // setiap kali seeder dijalankan. Yang dilewati terhitung pada JournalTypeSkipped,
            // supaya master yang tampak kurang lengkap ada penjelasannya.
            var existing = new HashSet<string>(
                existingRows.Select(x => x.JournalTypeCode),
                StringComparer.OrdinalIgnoreCase);

            foreach (var d in definitions)
            {
                if (existing.Contains(d.JournalTypeCode))
                {
                    result.JournalTypeSkipped++;
                    continue;
                }

                db.Set<AccJournalType>().Add(new AccJournalType
                {
                    Id = Guid.NewGuid(),
                    JournalTypeCode = d.JournalTypeCode,
                    JournalTypeName = d.JournalTypeName,
                    NumberPrefix = d.NumberPrefix,
                    RequiresApproval = d.RequiresApproval,
                    IsSystemType = d.IsSystemType,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.JournalTypeInserted++;
            }

            if (result.JournalTypeInserted > 0)
            {
                await db.SaveChangesAsync(ct);
            }
        }

        private sealed record JournalTypeDefinition(
            string JournalTypeCode,
            string JournalTypeName,
            string NumberPrefix,
            bool RequiresApproval,
            bool IsSystemType);
    }

    /// <summary>
    /// Ringkasan hasil seeder, dipakai untuk pencatatan log agar terlihat berapa baris yang
    /// benar-benar ditambahkan dan bagian mana yang dilewati beserta alasannya.
    /// </summary>
    public class AccountingMasterDataSeedResult
    {
        public int JournalTypeInserted { get; set; }

        public int JournalTypeSkipped { get; set; }

        public string? JournalTypeSkippedReason { get; set; }

        public int TotalInserted => JournalTypeInserted;
    }
}
