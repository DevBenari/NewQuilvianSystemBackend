using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Seeders
{
    /// <summary>
    /// Mengisi pengaturan footer cetak ketiga disiplin (<c>LAB-DEC-119</c>, <c>LAB-DEC-127</c>,
    /// <c>BE-LAB-63</c>).
    ///
    /// <b>Kenapa data induk ini BOLEH di-seed sementara tiga lainnya tidak.</b> Ketiga nilainya
    /// <b>terbaca langsung</b> dari bukti cetak <c>LAB-EVD-005</c> — label, nama konsultan, dan
    /// kalimat baku semuanya tercetak apa adanya pada lembar yang diserahkan. Breakpoint,
    /// kandungan cakram, dan pemetaan set bakteri tetap kosong karena isinya <b>penilaian</b>,
    /// bukan pembacaan.
    ///
    /// <b>Seeder ini HANYA MENYISIPKAN.</b> Nama konsultan berganti ketika orangnya berganti,
    /// dan itu keputusan kepala instalasi. Seeder yang menyegarkan isinya setiap aplikasi
    /// menyala akan mengembalikan nama pejabat lama pada setiap penempatan ulang — pada
    /// dokumen yang dipegang pasien.
    /// </summary>
    public static class LabDisciplineSettingSeeder
    {
        private static readonly BaselineDisciplineSetting[] Baseline =
        {
            new(
                LabDiscipline.Microbiology,
                "Konsultan Mikrobiologi Klinik",
                "Usman Chatib Warsa, PhD, SpMK-K, Prof. dr.",
                "LEBAR ZONA ANTIBIOTIK TIDAK MEMPENGARUHI TINGKAT KEPEKAAN BAKTERI."),

            new(
                LabDiscipline.AnatomicalPathology,
                "Spesialis Patologi Anatomi",
                "Ening Krisnuhoni, SpPA-K, dr.",
                // Kosong, dan itu pembacaan — bukan kelalaian. Footer Patologi Anatomi pada
                // LAB-EVD-005 nol memuat kalimat baku.
                null),

            new(
                LabDiscipline.ClinicalPathology,
                "Konsultan",
                "Prof.Dr.Riadi Wirawan SpPK(K)",
                null)
        };

        public static async Task SeedAsync(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(LabDisciplineSettingSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;

            if (!seedEnabled)
            {
                logger.LogInformation("Seeder pengaturan disiplin dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            await SeedAsync(dbContext, logger, cancellationToken);
        }

        /// <summary>
        /// Menjalankan pengisian terhadap satu <see cref="ApplicationDbContext"/> yang sudah
        /// ada. Dipisahkan agar perilakunya dapat diperiksa tanpa membangun service provider.
        /// </summary>
        public static async Task<int> SeedAsync(
            ApplicationDbContext dbContext,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            // Baris yang sudah ditandai terhapus IKUT dihitung, supaya disiplinnya tidak diisi
            // ulang — penghapusannya adalah keputusan pengguna.
            var terpakai = await dbContext.LabDisciplineSettings
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Select(x => x.Discipline)
                .ToListAsync(cancellationToken);

            var ada = new HashSet<LabDiscipline>(terpakai);

            var now = DateTime.UtcNow;
            var baru = new List<LabDisciplineSetting>();

            foreach (var baris in Baseline)
            {
                if (ada.Contains(baris.Discipline)) continue;

                baru.Add(new LabDisciplineSetting
                {
                    Discipline = baris.Discipline,
                    ConsultantLabel = baris.ConsultantLabel,
                    ConsultantName = baris.ConsultantName,
                    StandingNote = baris.StandingNote,
                    ReportNumberPrefix = null,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty
                });
            }

            if (baru.Count == 0)
            {
                logger.LogInformation("Pengaturan disiplin sudah lengkap; nol baris disisipkan.");
                return 0;
            }

            dbContext.LabDisciplineSettings.AddRange(baru);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Seeder pengaturan disiplin menyisipkan {Count} baris.", baru.Count);

            return baru.Count;
        }

        private sealed record BaselineDisciplineSetting(
            LabDiscipline Discipline,
            string ConsultantLabel,
            string? ConsultantName,
            string? StandingNote);
    }
}
