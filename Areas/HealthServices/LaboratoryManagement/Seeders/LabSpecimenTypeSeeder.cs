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
        /// <remarks>
        /// <b>Diperluas 7 → 31 pada 2026-09-21</b> (<c>LAB-DEC-129</c>, <c>BE-LAB-55</c>),
        /// mengikuti kolom <c>jenis_specimen</c> pada <c>LAB-EVD-007</c>.
        ///
        /// <b>GUID ketujuh baris lama DIPERTAHANKAN.</b> Ketujuhnya terbukti punya padanan di
        /// antara 31 kelompok, sehingga ini penambahan 24 baris beserta penyelarasan nama —
        /// bukan pembongkaran. <c>LabSpecimen.SpecimenTypeId</c> yang sudah menunjuk
        /// ketujuhnya <b>nol perlu dipetakan ulang</b>.
        /// </remarks>
        private static readonly BaselineSpecimenType[] BaselineSpecimenTypes =
        {
            // Tujuh baris asli — GUID TIDAK BOLEH BERUBAH.
            new("2c7e5d10-0001-4b20-8e11-7c2e1b6f8d01", "BLOOD", "Darah", false, 1),
            new("2c7e5d10-0002-4b20-8e11-7c2e1b6f8d02", "URINE", "Urine", false, 2),
            new("2c7e5d10-0003-4b20-8e11-7c2e1b6f8d03", "BODYFLUID", "Cairan Tubuh", false, 3),
            new("2c7e5d10-0004-4b20-8e11-7c2e1b6f8d04", "SPUTUM", "Sputum / Specimen Respirasi", false, 4),
            new("2c7e5d10-0005-4b20-8e11-7c2e1b6f8d05", "PUS", "Pus / Luka / Drainase", false, 5),
            new("2c7e5d10-0006-4b20-8e11-7c2e1b6f8d06", "TISSUE", "Jaringan / Spesimen Bedah", false, 6),
            new("2c7e5d10-0007-4b20-8e11-7c2e1b6f8d07", "OTHER", "Lainnya / Belum Spesifik", true, 99),

            // Dua puluh empat kelompok tambahan dari LAB-EVD-007.
            new("2c7e5d10-0008-4b20-8e11-7c2e1b6f8d08", "BIOPSY", "Biopsi", false, 7),
            new("2c7e5d10-0009-4b20-8e11-7c2e1b6f8d09", "ANATOMIC", "Spesimen Anatomi - Material Tidak Disebutkan", false, 8),
            new("2c7e5d10-0010-4b20-8e11-7c2e1b6f8d10", "CYTOLOGY", "Sitologi / Smear / Brushing", false, 9),
            new("2c7e5d10-0011-4b20-8e11-7c2e1b6f8d11", "SWAB", "Swab", false, 10),
            new("2c7e5d10-0012-4b20-8e11-7c2e1b6f8d12", "ASPIRATE", "Aspirat / Pungsi", false, 11),
            new("2c7e5d10-0013-4b20-8e11-7c2e1b6f8d13", "DEVICE", "Perangkat / Kateter / Benda Asing", false, 12),
            new("2c7e5d10-0014-4b20-8e11-7c2e1b6f8d14", "ENVIRONMENT", "Lingkungan / Non-pasien", false, 13),
            new("2c7e5d10-0015-4b20-8e11-7c2e1b6f8d15", "SECRETION", "Sekret / Isi Organ", false, 14),
            new("2c7e5d10-0016-4b20-8e11-7c2e1b6f8d16", "PLACENTA", "Produk Kehamilan / Plasenta", false, 15),
            new("2c7e5d10-0017-4b20-8e11-7c2e1b6f8d17", "MILK", "ASI / Milk", false, 16),
            new("2c7e5d10-0018-4b20-8e11-7c2e1b6f8d18", "MARROW", "Sumsum Tulang", false, 17),
            new("2c7e5d10-0019-4b20-8e11-7c2e1b6f8d19", "SALIVA", "Saliva / Oral Fluid", false, 18),
            new("2c7e5d10-0020-4b20-8e11-7c2e1b6f8d20", "HAIRNAIL", "Rambut / Kuku / Kerokan", false, 19),
            new("2c7e5d10-0021-4b20-8e11-7c2e1b6f8d21", "SERUM", "Serum", false, 20),
            new("2c7e5d10-0022-4b20-8e11-7c2e1b6f8d22", "ISOLATE", "Isolat / Organisme", false, 21),
            new("2c7e5d10-0023-4b20-8e11-7c2e1b6f8d23", "PLASMA", "Plasma", false, 22),
            new("2c7e5d10-0024-4b20-8e11-7c2e1b6f8d24", "STOOL", "Feses / Stool", false, 23),
            new("2c7e5d10-0025-4b20-8e11-7c2e1b6f8d25", "BONE", "Tulang / Gigi", false, 24),
            new("2c7e5d10-0026-4b20-8e11-7c2e1b6f8d26", "CONTAINER", "Wadah / Preparat / Media Koleksi", false, 25),
            new("2c7e5d10-0027-4b20-8e11-7c2e1b6f8d27", "CALCULUS", "Batu / Kalkulus / Kristal", false, 26),
            new("2c7e5d10-0028-4b20-8e11-7c2e1b6f8d28", "SEMEN", "Semen", false, 27),
            new("2c7e5d10-0029-4b20-8e11-7c2e1b6f8d29", "CELL", "Sel / Material Seluler", false, 28),
            new("2c7e5d10-0030-4b20-8e11-7c2e1b6f8d30", "MOLECULAR", "Material Molekuler", false, 29),
            new("2c7e5d10-0031-4b20-8e11-7c2e1b6f8d31", "CONTROL", "Kontrol / Material Referensi", false, 30)
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
