using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Seeders
{
    /// <summary>
    /// Data awal <c>E8</c> master butir persiapan bedah (<c>BE-RWI-173</c>,
    /// <c>02-backend-architecture.md</c> 12.12): empat kelompok <c>RWI-DEC-173</c> butir 3 —
    /// Verifikasi pasien, Persiapan fisik, Hasil pemeriksaan, Persiapan lain — dengan butir V1.
    /// </summary>
    /// <remarks>
    /// Tiga batas mengikat seeder ini, dan ketiganya disengaja:
    ///
    /// 1. Seeder MENOLAK berjalan di lingkungan produksi. Isi checklist <b>wajib disahkan pemilik
    ///    klinis sebelum produksi</b> (gerbang produksi <c>BE-RWI-173</c>); di produksi butir diisi
    ///    lewat layar admin. Polanya mengikuti <see cref="BloodComponentSeeder"/>.
    /// 2. Idempoten per <c>Code</c>, <b>termasuk</b> baris yang sudah ditandai terhapus. Butir yang
    ///    sengaja dihapus admin tidak dihidupkan kembali setiap kali aplikasi dinyalakan, dan nilai
    ///    yang sudah diubah admin tidak pernah ditimpa.
    /// 3. Seeder dapat dimatikan lewat <c>SeedDefaultData:Enabled = false</c>, sama seperti seeder
    ///    data master Hemodialisa.
    ///
    /// Butirnya dirangkum dari contoh V1 pada keputusan dan PRD Finishing ("Gelang identitas
    /// terpasang", "Puasa sejak jam …", "Persetujuan tindakan tersedia", "Hasil laboratorium
    /// pra-operasi ada"); butir selebihnya adalah usulan yang menunggu pengesahan klinis.
    /// </remarks>
    public static class SurgicalPreparationItemSeeder
    {
        /// <summary>Nama lingkungan yang membuat seeder berhenti tanpa menulis apa pun.</summary>
        public const string ProductionEnvironmentName = "Production";

        private sealed record Butir(string Code, string GroupName, string ItemName, bool IsMandatory, int SortOrder, string? Description);

        private static readonly Butir[] InitialItems =
        {
            new("SPI-ID-01", "Verifikasi pasien", "Gelang identitas terpasang", true, 1, null),
            new("SPI-ID-02", "Verifikasi pasien", "Identitas pasien dicocokkan dengan rekam medis", true, 2, null),
            new("SPI-ID-03", "Verifikasi pasien", "Riwayat alergi dikonfirmasi", true, 3, "Catat alergi yang ditemukan pada catatan butir."),

            new("SPI-PF-01", "Persiapan fisik", "Puasa sejak jam …", true, 1, "Catat jam mulai puasa pada catatan butir, misalnya \"Puasa sejak 22.00\"."),
            new("SPI-PF-02", "Persiapan fisik", "Area operasi sudah dibersihkan", true, 2, null),
            new("SPI-PF-03", "Persiapan fisik", "Perhiasan, gigi palsu, dan lensa kontak dilepas", true, 3, null),
            new("SPI-PF-04", "Persiapan fisik", "Pasien sudah berkemih atau kateter terpasang", false, 4, null),
            new("SPI-PF-05", "Persiapan fisik", "Infus terpasang", false, 5, null),

            new("SPI-HP-01", "Hasil pemeriksaan", "Hasil laboratorium pra-operasi ada", true, 1, null),
            new("SPI-HP-02", "Hasil pemeriksaan", "Hasil radiologi atau EKG terlampir bila diminta", false, 2, null),
            new("SPI-HP-03", "Hasil pemeriksaan", "Golongan darah dan persediaan darah dikonfirmasi bila diminta", false, 3, null),

            new("SPI-PL-01", "Persiapan lain", "Persetujuan tindakan tersedia", true, 1, null),
            new("SPI-PL-02", "Persiapan lain", "Persetujuan anestesi tersedia", true, 2, null),
            new("SPI-PL-03", "Persiapan lain", "Obat premedikasi diberikan sesuai instruksi", false, 3, null)
        };

        /// <summary>Titik masuk startup: membaca lingkungan dan konfigurasi, lalu mengisi data awal.</summary>
        public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken ct = default)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(SurgicalPreparationItemSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;

            if (!seedEnabled)
            {
                logger.LogInformation("Seeder butir persiapan bedah dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            var result = await SeedAsync(db, Guid.Empty, environment.EnvironmentName, ct);

            if (result.Refused)
            {
                logger.LogInformation("{Reason}", result.RefusedReason);
                return;
            }

            logger.LogInformation(
                "Seeder butir persiapan bedah: {Inserted} butir ditambahkan, {Skipped} sudah ada. " +
                "Isi checklist wajib disahkan pemilik klinis sebelum produksi.",
                result.ItemInserted,
                result.ItemSkipped);
        }

        public static async Task<SurgicalPreparationItemSeedResult> SeedAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            string environmentName,
            CancellationToken ct = default)
        {
            var result = new SurgicalPreparationItemSeedResult();

            if (IsProductionEnvironment(environmentName))
            {
                result.RefusedReason =
                    "Seeder butir persiapan bedah tidak dijalankan di lingkungan produksi. " +
                    "Isi checklist produksi disahkan pemilik klinis dan diisi lewat layar admin.";

                return result;
            }

            // Termasuk baris terhapus: butir yang sengaja dihapus admin tidak dihidupkan kembali.
            var existingCodes = await db.Set<MstSurgicalPreparationItem>()
                .AsNoTracking()
                .Select(x => x.Code.ToUpper())
                .ToListAsync(ct);

            var known = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);
            var now = DateTime.UtcNow;

            foreach (var butir in InitialItems)
            {
                if (known.Contains(butir.Code))
                {
                    result.ItemSkipped++;
                    continue;
                }

                db.Set<MstSurgicalPreparationItem>().Add(new MstSurgicalPreparationItem
                {
                    Id = Guid.NewGuid(),
                    Code = butir.Code,
                    GroupName = butir.GroupName,
                    ItemName = butir.ItemName,
                    IsMandatory = butir.IsMandatory,
                    SortOrder = butir.SortOrder,
                    Description = butir.Description,
                    IsActive = true,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                result.ItemInserted++;
            }

            if (result.ItemInserted > 0)
                await db.SaveChangesAsync(ct);

            return result;
        }

        public static bool IsProductionEnvironment(string? environmentName)
            => string.Equals(environmentName?.Trim(), ProductionEnvironmentName, StringComparison.OrdinalIgnoreCase);
    }

    public class SurgicalPreparationItemSeedResult
    {
        /// <summary>Terisi hanya bila seeder menolak berjalan, misalnya di lingkungan produksi.</summary>
        public string? RefusedReason { get; set; }

        public bool Refused => !string.IsNullOrWhiteSpace(RefusedReason);

        public int ItemInserted { get; set; }

        public int ItemSkipped { get; set; }
    }
}
