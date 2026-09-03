using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Seeders
{
    /// <summary>
    /// Mengisi lokasi penyimpanan darah minimum agar modul Bank Darah dapat dinyalakan di
    /// lingkungan pengembangan. Seeder hanya menambah baris yang belum ada berdasarkan
    /// <c>StorageLocationCode</c>, dan tidak pernah menimpa baris yang sudah tersimpan.
    /// </summary>
    /// <remarks>
    /// <b>Baca peringatan ini sebelum memakai seeder di lingkungan mana pun selain
    /// pengembangan.</b>
    ///
    /// Lokasi penyimpanan darah adalah <b>benda fisik yang benar-benar ada</b> di rumah sakit —
    /// kulkas tertentu, di ruang tertentu. Menebaknya menghasilkan master palsu yang terlanjur
    /// dipakai penempatan kantong, dan kantong darah yang tercatat berada di kulkas yang tidak
    /// pernah ada adalah kekeliruan yang jauh lebih mahal daripada daftar kosong. Pertimbangan
    /// yang sama membuat <see cref="InpatientMasterDataSeeder"/> menolak membuat baris
    /// <c>MstRoom</c> dan <c>MstBed</c>.
    ///
    /// Karena itu dua batas berlaku:
    ///
    /// 1. Seeder <b>MENOLAK</b> berjalan di lingkungan produksi. Daftar kulkas darah produksi
    ///    ditetapkan BDRS lewat layar admin.
    /// 2. Kedua lokasi yang di-seed adalah <b>contoh yang disebut pemilik proses</b> pada
    ///    rencana data master awal blueprint — "Kulkas Besar" dan "Kulkas Kecil" — bukan hasil
    ///    tebakan. Keduanya tetap harus diperiksa dan disesuaikan BDRS sebelum dipakai
    ///    sungguhan.
    ///
    /// Keduanya lahir <b>aktif</b>, karena tanpa satu pun lokasi aktif seluruh alur Bank Darah
    /// berhenti (<c>INV-BD-025</c>) dan lingkungan pengembangan menjadi tidak dapat dipakai
    /// menguji apa pun.
    /// </remarks>
    public static class BloodStorageLocationSeeder
    {
        /// <summary>Nama lingkungan yang membuat seeder berhenti tanpa menulis apa pun.</summary>
        public const string ProductionEnvironmentName = "Production";

        private static readonly (string Code, string Name, string Description)[] MinimumLocations =
        {
            ("KLK-BSR", "Kulkas Besar", "Kulkas darah utama BDRS. Sesuaikan dengan kulkas yang benar-benar ada."),
            ("KLK-KCL", "Kulkas Kecil", "Kulkas darah pendamping BDRS. Sesuaikan dengan kulkas yang benar-benar ada.")
        };

        public static async Task<BloodStorageLocationSeedResult> SeedAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            string environmentName,
            CancellationToken ct = default)
        {
            var result = new BloodStorageLocationSeedResult();

            if (IsProductionEnvironment(environmentName))
            {
                result.RefusedReason =
                    "Seeder lokasi penyimpanan darah tidak dijalankan di lingkungan produksi. " +
                    "Daftar kulkas darah produksi ditetapkan BDRS lewat layar admin, karena lokasi " +
                    "penyimpanan adalah benda fisik yang tidak boleh ditebak.";

                return result;
            }

            var existingCodes = await db.Set<MstBloodStorageLocation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => x.StorageLocationCode.ToUpper())
                .ToListAsync(ct);

            var now = DateTime.UtcNow;

            foreach (var (code, name, description) in MinimumLocations)
            {
                if (existingCodes.Contains(code))
                {
                    result.LocationSkipped++;
                    continue;
                }

                db.Set<MstBloodStorageLocation>().Add(new MstBloodStorageLocation
                {
                    Id = Guid.NewGuid(),
                    StorageLocationCode = code,
                    StorageLocationName = name,
                    Description = description,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.LocationInserted++;
            }

            if (result.LocationInserted > 0)
                await db.SaveChangesAsync(ct);

            return result;
        }

        public static bool IsProductionEnvironment(string environmentName)
            => string.Equals(environmentName, ProductionEnvironmentName, StringComparison.OrdinalIgnoreCase);
    }

    public class BloodStorageLocationSeedResult
    {
        /// <summary>
        /// Terisi hanya bila seeder menolak berjalan, misalnya di lingkungan produksi.
        /// </summary>
        public string? RefusedReason { get; set; }

        public bool Refused => !string.IsNullOrWhiteSpace(RefusedReason);

        public int LocationInserted { get; set; }

        public int LocationSkipped { get; set; }
    }
}
