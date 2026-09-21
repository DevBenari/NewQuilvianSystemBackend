using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Seeders
{
    /// <summary>
    /// Mengisi data induk jenis specimen dengan baris baseline (<c>LAB-DEC-040</c>,
    /// <c>BE-LAB-20</c>).
    ///
    /// <b>Mengapa seeder ini ada.</b> Tabel jenis specimen yang kosong membuat petugas tidak
    /// dapat mencatat satu wadah pun — dan yang tertahan bukan formulir, melainkan bahan yang
    /// sudah terlanjur diambil dari tubuh pasien. Baris baseline juga diisi migration, tetapi
    /// migration hanya menolong lingkungan yang menjalankannya dari awal. Seeder ini menjadi
    /// jaring pengaman untuk lingkungan baru yang databasenya disiapkan dengan cara lain.
    ///
    /// <b>Yang sengaja tidak dikerjakan seeder ini.</b> Ia hanya menambah kode yang belum ada,
    /// dan tidak pernah menimpa baris yang sudah tersimpan. Nama, keterangan, urutan tampil,
    /// dan status aktif adalah milik kepala instalasi; menimpanya saat aplikasi menyala berarti
    /// membatalkan keputusannya diam-diam setiap kali server dinyalakan ulang.
    ///
    /// Baris yang sudah ditandai terhapus juga dihormati: kodenya tidak diisi ulang, karena
    /// penghapusannya adalah keputusan pengguna.
    /// </summary>
    public static class LabSpecimenTypeSeeder
    {
        /// <summary>
        /// Tujuh baris baseline dari <c>LAB-EVD-001</c>. <c>Id</c> ditetapkan tetap — bukan
        /// dibangkitkan — dan nilainya sama persis dengan yang dipakai migration, supaya sebuah
        /// jenis specimen memiliki identitas yang sama di setiap lingkungan.
        ///
        /// Baris terakhir adalah satu-satunya yang berpenanda <c>Lainnya</c>, dan urutannya
        /// sengaja <c>99</c> supaya ia selalu berada di bawah pilihan yang lebih tepat.
        /// </summary>
        private static readonly BaselineSpecimenType[] BaselineSpecimenTypes =
        {
            new("2c7e5d10-0001-4b20-8e11-7c2e1b6f8d01", "BLOOD", "Blood", false, 1),
            new("2c7e5d10-0002-4b20-8e11-7c2e1b6f8d02", "URINE", "Urine", false, 2),
            new("2c7e5d10-0003-4b20-8e11-7c2e1b6f8d03", "BODYFLUID", "Body Fluid", false, 3),
            new("2c7e5d10-0004-4b20-8e11-7c2e1b6f8d04", "SPUTUM", "Sputum", false, 4),
            new("2c7e5d10-0005-4b20-8e11-7c2e1b6f8d05", "PUS", "Pus", false, 5),
            new("2c7e5d10-0006-4b20-8e11-7c2e1b6f8d06", "TISSUE", "Jaringan", false, 6),
            new("2c7e5d10-0007-4b20-8e11-7c2e1b6f8d07", "OTHER", "Lainnya", true, 99)
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
                .CreateLogger(nameof(LabSpecimenTypeSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;

            if (!seedEnabled)
            {
                logger.LogInformation("Seeder jenis specimen dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            await SeedAsync(dbContext, logger, cancellationToken);
        }

        /// <summary>
        /// Menjalankan pengisian terhadap satu <see cref="ApplicationDbContext"/> yang sudah ada.
        /// Dipisahkan agar perilakunya dapat diperiksa tanpa membangun service provider.
        /// </summary>
        public static async Task<int> SeedAsync(
            ApplicationDbContext dbContext,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            // Baris yang sudah ditandai terhapus ikut dihitung, supaya kodenya tidak diisi ulang.
            var existingCodes = await dbContext.LabSpecimenTypes
                .AsNoTracking()
                .Select(x => x.SpecimenTypeCode)
                .ToListAsync(cancellationToken);

            var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            // Index unik parsial hanya mengizinkan satu baris Lainnya yang aktif. Bila baris itu
            // sudah ada dengan kode lain, menambahkan baris baseline OTHER akan ditolak database
            // — dan penolakannya menggagalkan seluruh penyalaan aplikasi. Diperiksa lebih dulu
            // supaya seeder melewatinya dengan tenang.
            var otherBucketExists = await dbContext.LabSpecimenTypes
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.IsActive && x.IsOtherBucket, cancellationToken);

            var now = DateTime.UtcNow;
            var added = 0;
            var skippedOtherBucket = false;

            foreach (var specimenType in BaselineSpecimenTypes)
            {
                if (existing.Contains(specimenType.Code))
                    continue;

                if (specimenType.IsOtherBucket && otherBucketExists)
                {
                    skippedOtherBucket = true;
                    continue;
                }

                dbContext.LabSpecimenTypes.Add(new LabSpecimenType
                {
                    Id = Guid.Parse(specimenType.Id),
                    SpecimenTypeCode = specimenType.Code,
                    SpecimenTypeName = specimenType.Name,
                    Description = null,
                    IsOtherBucket = specimenType.IsOtherBucket,
                    IsActive = true,
                    SortOrder = specimenType.SortOrder,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty
                });

                if (specimenType.IsOtherBucket)
                    otherBucketExists = true;

                added++;
            }

            if (skippedOtherBucket)
            {
                logger.LogWarning(
                    "Baris baseline OTHER dilewati karena sudah ada jenis specimen Lainnya yang aktif dengan kode berbeda.");
            }

            if (added == 0)
            {
                logger.LogInformation("Jenis specimen sudah lengkap; tidak ada baris baru yang ditambahkan.");
                return 0;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Seeder jenis specimen menambahkan {AddedCount} baris baseline.",
                added);

            return added;
        }

        private sealed record BaselineSpecimenType(
            string Id,
            string Code,
            string Name,
            bool IsOtherBucket,
            int SortOrder);
    }
}
