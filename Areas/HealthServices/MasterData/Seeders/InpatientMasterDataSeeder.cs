using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Seeders
{
    /// <summary>
    /// Mengisi data master Rawat Inap agar modul dapat dinyalakan tanpa layar pengaturan
    /// menampilkan daftar kosong. Seeder ini hanya menambah baris yang belum ada berdasarkan
    /// Code dan ItemCode, dan tidak pernah menimpa baris yang sudah tersimpan, supaya nilai
    /// yang sudah disesuaikan admin tidak hilang saat aplikasi dijalankan ulang.
    /// </summary>
    /// <remarks>
    /// Dua batas yang mengikat seeder ini, keduanya dari RWI-DEC-048:
    ///
    /// 1. Seeder MENOLAK berjalan di lingkungan produksi. Data master produksi ditetapkan
    ///    pemilik proses bisnis lewat layar admin, bukan oleh baris kode yang ikut terbawa
    ///    setiap kali aplikasi dinyalakan.
    /// 2. Seeder TIDAK PERNAH membuat baris MstRoom maupun MstBed. Susunan kamar dan tempat
    ///    tidur khas tiap rumah sakit; menebaknya menghasilkan master palsu yang terlanjur
    ///    dipakai penempatan pasien.
    /// </remarks>
    public static class InpatientMasterDataSeeder
    {
        /// <summary>Kode baris pengaturan tunggal yang dipakai seluruh modul Rawat Inap.</summary>
        public const string DefaultSettingCode = "DEFAULT";

        /// <summary>Nama lingkungan yang membuat seeder berhenti tanpa menulis apa pun.</summary>
        public const string ProductionEnvironmentName = "Production";

        public static async Task<InpatientMasterDataSeedResult> SeedAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            string environmentName,
            CancellationToken ct = default)
        {
            var result = new InpatientMasterDataSeedResult();

            if (IsProductionEnvironment(environmentName))
            {
                result.RefusedReason =
                    "Seeder master Rawat Inap menolak berjalan di lingkungan produksi. " +
                    "Isi pengaturan dan butir administrasi produksi lewat layar admin, " +
                    "bukan lewat seeder.";

                return result;
            }

            var now = DateTime.UtcNow;

            await SeedDefaultSettingAsync(db, actorUserId, now, result, ct);
            await SeedClearanceItemsAsync(db, actorUserId, now, result, ct);

            return result;
        }

        /// <summary>
        /// Menentukan apakah nama lingkungan yang diberikan adalah produksi. Pembandingannya
        /// mengabaikan besar kecil huruf, mengikuti cara ASP.NET Core sendiri membaca
        /// ASPNETCORE_ENVIRONMENT.
        /// </summary>
        public static bool IsProductionEnvironment(string? environmentName)
            => string.Equals(
                environmentName?.Trim(),
                ProductionEnvironmentName,
                StringComparison.OrdinalIgnoreCase);

        // ------------------------------------------------------------------
        // Pengaturan Rawat Inap — satu baris berkode DEFAULT
        // ------------------------------------------------------------------

        /// <remarks>
        /// Nilai awal diambil dari 02-backend-architecture.md bagian 8.1.
        ///
        /// InitialAssessmentTargetHours dan ProgressNoteVerificationTargetHours bersumber dari
        /// RWI-RULE-021 yang BELUM final secara klinis. Keduanya di-seed sebagai nilai bawaan
        /// yang dapat diubah admin lewat layar pengaturan, dan tidak boleh diperlakukan sebagai
        /// angka yang sudah disahkan pemilik klinis.
        /// </remarks>
        private static async Task SeedDefaultSettingAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            InpatientMasterDataSeedResult result,
            CancellationToken ct)
        {
            // Idempotensi diperiksa lewat Code, kunci unik tabel ini. Pemeriksaan sengaja
            // tidak menyaring IsDelete: baris DEFAULT yang pernah dihapus lunak tetap
            // memakai Code yang sama di database, sehingga menambah baris kedua akan
            // menabrak index unik IX_MstInpatientSetting_Code dan menghentikan aplikasi
            // saat menyala.
            var alreadyExists = await db.Set<MstInpatientSetting>()
                .AnyAsync(x => x.Code == DefaultSettingCode, ct);

            if (alreadyExists)
            {
                result.SettingSkippedReason =
                    $"Baris pengaturan berkode {DefaultSettingCode} sudah ada, tidak ditambah lagi.";

                return;
            }

            db.Set<MstInpatientSetting>().Add(new MstInpatientSetting
            {
                Id = Guid.NewGuid(),
                Code = DefaultSettingCode,
                Name = "Pengaturan Rawat Inap Default",
                BedReservationMinutes = 120,
                DraftEpisodeExpiryHours = 24,
                InitialAssessmentTargetHours = 24,
                ProgressNoteVerificationTargetHours = 24,
                PendingClosureThresholdHours = 4,
                EpisodeNumberPrefix = "RI",
                IsDefault = true,
                IsActive = true,
                Notes =
                    "Nilai bawaan modul Rawat Inap. InitialAssessmentTargetHours dan " +
                    "ProgressNoteVerificationTargetHours bersumber dari RWI-RULE-021 yang " +
                    "belum final secara klinis; keduanya wajib ditinjau pemilik klinis " +
                    "sebelum dipakai untuk pasien sungguhan.",
                CreateDateTime = now,
                CreateBy = actorUserId
            });

            await db.SaveChangesAsync(ct);
            result.SettingInserted = 1;
        }

        // ------------------------------------------------------------------
        // Butir administrasi penutupan episode
        // ------------------------------------------------------------------

        /// <remarks>
        /// Tiga butir bawaan sesuai RWI-DEC-026 dan 02-backend-architecture.md bagian 8.2.
        /// DISCHARGE-MED sengaja tidak wajib (RWI-RULE-024): obat pulang belum dapat ditutup
        /// otomatis karena modul Farmasi berada di luar scope MVP, sehingga menjadikannya
        /// wajib akan menahan penutupan setiap episode tanpa ada cara menyelesaikannya.
        /// </remarks>
        private static async Task SeedClearanceItemsAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            DateTime now,
            InpatientMasterDataSeedResult result,
            CancellationToken ct)
        {
            var definitions = new[]
            {
                new ClearanceItemDefinition(
                    "ADM-DOC",
                    "Berkas administrasi pasien lengkap",
                    "Berkas administrasi pasien sudah lengkap dan diserahkan ke bagian terkait.",
                    true,
                    10),
                new ClearanceItemDefinition(
                    "RETURN-ITEM",
                    "Barang milik pasien dan barang rumah sakit sudah diselesaikan",
                    "Barang milik pasien sudah dikembalikan dan barang milik rumah sakit sudah diterima kembali.",
                    true,
                    20),
                new ClearanceItemDefinition(
                    "DISCHARGE-MED",
                    "Obat pulang sudah diserahkan",
                    "Tidak wajib pada MVP karena modul Farmasi di luar scope. Dapat dinonaktifkan admin.",
                    false,
                    30)
            };

            // Sama seperti pengaturan: ItemCode adalah kunci unik tabel, dan pemeriksaannya
            // tidak menyaring IsDelete supaya baris yang pernah dihapus lunak tidak memicu
            // penyisipan kedua yang menabrak IX_MstInpatientClearanceItem_ItemCode.
            var existingCodes = await db.Set<MstInpatientClearanceItem>()
                .Select(x => x.ItemCode)
                .ToListAsync(ct);

            var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            foreach (var d in definitions)
            {
                if (existing.Contains(d.ItemCode))
                {
                    result.ClearanceItemSkipped++;
                    continue;
                }

                db.Set<MstInpatientClearanceItem>().Add(new MstInpatientClearanceItem
                {
                    Id = Guid.NewGuid(),
                    ItemCode = d.ItemCode,
                    ItemName = d.ItemName,
                    Description = d.Description,
                    IsMandatory = d.IsMandatory,
                    SortOrder = d.SortOrder,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.ClearanceItemInserted++;
            }

            if (result.ClearanceItemInserted > 0)
            {
                await db.SaveChangesAsync(ct);
            }
        }

        private sealed record ClearanceItemDefinition(
            string ItemCode,
            string ItemName,
            string Description,
            bool IsMandatory,
            int SortOrder);
    }

    /// <summary>
    /// Ringkasan hasil seeder, dipakai untuk pencatatan log agar terlihat berapa baris yang
    /// benar-benar ditambahkan dan bagian mana yang dilewati beserta alasannya.
    /// </summary>
    public class InpatientMasterDataSeedResult
    {
        /// <summary>
        /// Terisi hanya bila seeder menolak berjalan, misalnya di lingkungan produksi.
        /// </summary>
        public string? RefusedReason { get; set; }

        public bool Refused => !string.IsNullOrWhiteSpace(RefusedReason);

        public int SettingInserted { get; set; }

        public string? SettingSkippedReason { get; set; }

        public int ClearanceItemInserted { get; set; }

        public int ClearanceItemSkipped { get; set; }

        public int TotalInserted => SettingInserted + ClearanceItemInserted;
    }
}
