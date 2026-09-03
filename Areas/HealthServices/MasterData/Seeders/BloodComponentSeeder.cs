using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Seeders
{
    /// <summary>
    /// Mengisi katalog komponen darah minimum agar modul Bank Darah dapat dinyalakan tanpa
    /// layar katalognya menampilkan daftar kosong. Seeder hanya menambah baris yang belum ada
    /// berdasarkan <c>ComponentCode</c>, dan tidak pernah menimpa baris yang sudah tersimpan,
    /// supaya nilai yang sudah disesuaikan petugas tidak hilang saat aplikasi dijalankan ulang.
    /// </summary>
    /// <remarks>
    /// Dua batas mengikat seeder ini, dan keduanya disengaja:
    ///
    /// 1. Seeder MENOLAK berjalan di lingkungan produksi. Katalog komponen produksi ditetapkan
    ///    BDRS lewat layar admin, bukan oleh baris kode yang ikut terbawa setiap kali aplikasi
    ///    dinyalakan. Polanya mengikuti <see cref="InpatientMasterDataSeeder"/>.
    /// 2. Seeder TIDAK PERNAH mengisi <c>CompatibilityEvidenceValidityHours</c>. Angka jamnya
    ///    berasal dari kebijakan klinis MMC yang masih berjalan (<c>OQ-BD-012</c>), dan
    ///    menebaknya berarti membuka gerbang pemberian darah dengan angka karangan. Ketiga
    ///    komponen lahir dengan masa berlaku kosong, sehingga pemberiannya tertahan sampai
    ///    BDRS mengisinya sendiri (<c>VAL-BD-020b</c>, <c>INV-BD-023</c>).
    ///
    /// Isi minimumnya PRC, TC, dan FFP, sesuai rencana data master awal blueprint
    /// <c>BD-BP-001</c> dan <c>DEC-BD-024</c>.
    /// </remarks>
    public static class BloodComponentSeeder
    {
        /// <summary>Nama lingkungan yang membuat seeder berhenti tanpa menulis apa pun.</summary>
        public const string ProductionEnvironmentName = "Production";

        /// <summary>
        /// Katalog minimum. Kode dan namanya dipakai apa adanya; masa berlaku bukti kecocokan
        /// sengaja dibiarkan kosong.
        /// </summary>
        private static readonly (string Code, string Name)[] MinimumComponents =
        {
            ("PRC", "Packed Red Cells"),
            ("TC", "Trombosit Concentrate"),
            ("FFP", "Fresh Frozen Plasma")
        };

        public static async Task<BloodComponentSeedResult> SeedAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            string environmentName,
            CancellationToken ct = default)
        {
            var result = new BloodComponentSeedResult();

            if (IsProductionEnvironment(environmentName))
            {
                result.RefusedReason =
                    "Seeder katalog komponen darah tidak dijalankan di lingkungan produksi. " +
                    "Katalog produksi ditetapkan BDRS lewat layar admin.";

                return result;
            }

            var existingCodes = await db.Set<MstBloodComponent>()
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => x.ComponentCode.ToUpper())
                .ToListAsync(ct);

            var now = DateTime.UtcNow;

            foreach (var (code, name) in MinimumComponents)
            {
                if (existingCodes.Contains(code))
                {
                    result.ComponentSkipped++;
                    continue;
                }

                db.Set<MstBloodComponent>().Add(new MstBloodComponent
                {
                    Id = Guid.NewGuid(),
                    ComponentCode = code,
                    ComponentName = name,
                    CompatibilityEvidenceValidityHours = null,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.ComponentInserted++;
            }

            if (result.ComponentInserted > 0)
                await db.SaveChangesAsync(ct);

            return result;
        }

        public static bool IsProductionEnvironment(string environmentName)
            => string.Equals(environmentName, ProductionEnvironmentName, StringComparison.OrdinalIgnoreCase);
    }

    public class BloodComponentSeedResult
    {
        /// <summary>
        /// Terisi hanya bila seeder menolak berjalan, misalnya di lingkungan produksi.
        /// </summary>
        public string? RefusedReason { get; set; }

        public bool Refused => !string.IsNullOrWhiteSpace(RefusedReason);

        public int ComponentInserted { get; set; }

        public int ComponentSkipped { get; set; }
    }
}
