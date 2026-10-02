using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Seeders
{
    /// <summary>
    /// Mengisi satu alasan terkendali untuk **setiap** kategori Bank Darah, agar tidak ada
    /// tindakan yang berhenti hanya karena kotak pilihan alasannya kosong. Seeder hanya menambah
    /// baris yang belum ada berdasarkan <c>ReasonCode</c>, dan tidak pernah menimpa baris yang
    /// sudah tersimpan.
    /// </summary>
    /// <remarks>
    /// <b>Kenapa satu per kategori, bukan sekadar tiga.</b> Sebuah kategori yang tidak punya satu
    /// pun alasan aktif membuat tindakan yang memerlukannya tidak dapat diselesaikan sama sekali:
    /// aturan menuntut alasan terkendali (<c>INV-BD-016</c>), sementara daftar pilihannya kosong.
    /// Menyisakan satu kategori kosong berarti menyisakan satu jalur proses yang buntu.
    ///
    /// <b>Teks di bawah adalah titik awal, bukan daftar resmi BDRS.</b> Rumusan alasan adalah
    /// kesepakatan proses (<c>DEC-BD-024</c>), dan tiap rumah sakit menuliskannya berbeda. Yang
    /// di-seed di sini sengaja dibuat umum dan netral supaya jelas perlu disesuaikan — bukan
    /// supaya dipakai apa adanya. Karena itu:
    ///
    /// 1. Seeder <b>MENOLAK</b> berjalan di lingkungan produksi. Daftar alasan produksi disusun
    ///    BDRS lewat layar admin.
    /// 2. Kode alasannya diberi awalan <c>SEED-</c> supaya baris bawaan mudah dibedakan dari
    ///    baris yang benar-benar disusun BDRS, dan mudah dinonaktifkan setelah daftar aslinya
    ///    masuk.
    ///
    /// Polanya mengikuti <see cref="InpatientMasterDataSeeder"/> dan
    /// <see cref="BloodStorageLocationSeeder"/>.
    /// </remarks>
    public static class BloodBankReasonSeeder
    {
        /// <summary>Nama lingkungan yang membuat seeder berhenti tanpa menulis apa pun.</summary>
        public const string ProductionEnvironmentName = "Production";

        /// <summary>Awalan kode yang menandai baris berasal dari seeder, bukan dari BDRS.</summary>
        public const string SeedCodePrefix = "SEED-";

        private static readonly (string Code, string Text, string Category)[] MinimumReasons =
        {
            (SeedCodePrefix + "CANCEL-KLINIS",
             "Kebutuhan transfusi dibatalkan dokter",
             BloodBankReasonCategories.OrderCancellationClinical),

            (SeedCodePrefix + "CANCEL-OPS",
             "Order ganda atau salah input, dirapikan petugas Bank Darah",
             BloodBankReasonCategories.OrderCancellationOperational),

            (SeedCodePrefix + "DARURAT",
             "Keadaan darurat, pemberian tidak dapat menunggu",
             BloodBankReasonCategories.Emergency),

            (SeedCodePrefix + "PENDING-SELESAI",
             "Kantong menunggu keputusan diselesaikan petugas Bank Darah",
             BloodBankReasonCategories.PendingReviewResolution),

            (SeedCodePrefix + "RETUR-PMI",
             "Kantong dikembalikan kepada penyedia",
             BloodBankReasonCategories.Return),

            (SeedCodePrefix + "TIDAK-LAYAK",
             "Kantong dinyatakan tidak layak pakai",
             BloodBankReasonCategories.NotUsable),

            (SeedCodePrefix + "LEBIH-KIRIM",
             "Kantong datang melebihi jumlah yang diminta",
             BloodBankReasonCategories.OverDelivery),

            (SeedCodePrefix + "BATAL-ALOKASI",
             "Alokasi kantong dibatalkan",
             BloodBankReasonCategories.AllocationCancellation),

            (SeedCodePrefix + "KOREKSI",
             "Pencatatan pemberian perlu dikoreksi",
             BloodBankReasonCategories.IssuanceCorrection),

            (SeedCodePrefix + "KOREKSI-TOLAK",
             "Permintaan koreksi ditolak Dokter Bank Darah",
             BloodBankReasonCategories.CorrectionRejection)
        };

        public static async Task<BloodBankReasonSeedResult> SeedAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            string environmentName,
            CancellationToken ct = default)
        {
            var result = new BloodBankReasonSeedResult();

            if (IsProductionEnvironment(environmentName))
            {
                result.RefusedReason =
                    "Seeder alasan Bank Darah tidak dijalankan di lingkungan produksi. " +
                    "Daftar alasan produksi disusun BDRS lewat layar admin, karena rumusan alasan " +
                    "adalah kesepakatan proses yang berbeda di tiap rumah sakit.";

                return result;
            }

            var existingCodes = await db.Set<MstBloodBankReason>()
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => x.ReasonCode.ToUpper())
                .ToListAsync(ct);

            var now = DateTime.UtcNow;

            foreach (var (code, text, category) in MinimumReasons)
            {
                if (existingCodes.Contains(code))
                {
                    result.ReasonSkipped++;
                    continue;
                }

                db.Set<MstBloodBankReason>().Add(new MstBloodBankReason
                {
                    Id = Guid.NewGuid(),
                    ReasonCode = code,
                    ReasonText = text,
                    ReasonCategory = category,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.ReasonInserted++;
            }

            if (result.ReasonInserted > 0)
                await db.SaveChangesAsync(ct);

            return result;
        }

        public static bool IsProductionEnvironment(string environmentName)
            => string.Equals(environmentName, ProductionEnvironmentName, StringComparison.OrdinalIgnoreCase);
    }

    public class BloodBankReasonSeedResult
    {
        /// <summary>
        /// Terisi hanya bila seeder menolak berjalan, misalnya di lingkungan produksi.
        /// </summary>
        public string? RefusedReason { get; set; }

        public bool Refused => !string.IsNullOrWhiteSpace(RefusedReason);

        public int ReasonInserted { get; set; }

        public int ReasonSkipped { get; set; }
    }
}
