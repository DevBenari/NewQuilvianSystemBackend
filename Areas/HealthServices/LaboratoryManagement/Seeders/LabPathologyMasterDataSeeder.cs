using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Seeders
{
    /// <summary>
    /// Mengisi <b>tiga</b> dari empat data induk Patologi Anatomi dengan baris tetap
    /// (<c>LAB-DEC-086</c>, <c>BE-LAB-50</c>): golongan, ruas isian, dan keberlakuannya.
    ///
    /// <b>Yang keempat sengaja tidak diisi di sini, dan itu bukan kelalaian.</b> Pemetaan jenis
    /// pemeriksaan ke golongan bergantung pada katalog rumah sakit yang bersangkutan, sehingga
    /// pengisiannya milik kepala instalasi bersama <c>DR-LAB-003</c> — bukan milik seeder.
    /// Menebaknya di sini berarti menanam penggolongan diagnostik yang tidak pernah diperiksa
    /// siapa pun. Alat bantunya disediakan terpisah lewat jalur usulan.
    ///
    /// <b>Mengapa seeder ini ada.</b> Tabel yang kosong membuat formulir hasil Patologi Anatomi
    /// kosong sama sekali, dan yang tertahan adalah laporan diagnostik pasien yang jaringannya
    /// sudah terlanjur diambil. Ketiga isinya bersifat tetap: ia datang dari
    /// <c>LAB-EVD-003</c> bagian 5.6, bukan dari kebiasaan satu rumah sakit.
    ///
    /// <b>Yang sengaja tidak dikerjakan seeder ini.</b> Ia hanya menambah kode yang belum ada,
    /// dan tidak pernah menimpa baris yang sudah tersimpan. Label, urutan tampil, penanda wajib,
    /// dan status aktif adalah milik kepala instalasi; menimpanya saat aplikasi menyala berarti
    /// membatalkan keputusannya diam-diam setiap kali server dinyalakan ulang. Baris yang sudah
    /// ditandai terhapus juga dihormati.
    /// </summary>
    public static class LabPathologyMasterDataSeeder
    {
        /// <summary>
        /// Empat golongan dari <c>LAB-EVD-003</c> bagian 5.6. <c>Id</c> ditetapkan tetap supaya
        /// sebuah golongan memiliki identitas yang sama di setiap lingkungan.
        /// </summary>
        private static readonly BaselineCategory[] BaselineCategories =
        {
            new("6a1f2c30-0001-4d51-9a10-4f8b2c7e5a01", "HISTO", "Histologi", 1),
            new("6a1f2c30-0002-4d51-9a10-4f8b2c7e5a02", "SITO_GIN", "Sitologi Ginekologi", 2),
            new("6a1f2c30-0003-4d51-9a10-4f8b2c7e5a03", "SITO_NONGIN", "Sitologi Non-Ginekologi", 3),
            new("6a1f2c30-0004-4d51-9a10-4f8b2c7e5a04", "IHK", "Imunohistokimia", 4)
        };

        /// <summary>
        /// Lima belas ruas isian dari <c>LAB-EVD-003</c> bagian 5.6.
        ///
        /// <b>Urutannya berlaku lintas golongan</b>, dan itu yang membuat satu ruas dapat dipakai
        /// dua golongan tanpa disalin: <c>ANJURAN</c> berurutan <c>99</c> sehingga ia jatuh
        /// paling akhir baik pada Sitologi Ginekologi maupun pada Imunohistokimia — persis
        /// seperti pada bukti lapangannya.
        /// </summary>
        private static readonly BaselineParameter[] BaselineParameters =
        {
            new("7b2e3d40-0001-4e62-8b21-5a9c3d8f6b01", "MAKROSKOPIK", "Makroskopik", 1),
            new("7b2e3d40-0002-4e62-8b21-5a9c3d8f6b02", "MIKROSKOPIK", "Mikroskopik", 2),
            new("7b2e3d40-0003-4e62-8b21-5a9c3d8f6b03", "KESIMPULAN", "Kesimpulan", 3),
            new("7b2e3d40-0004-4e62-8b21-5a9c3d8f6b04", "KONDISI", "Kondisi", 4),
            new("7b2e3d40-0005-4e62-8b21-5a9c3d8f6b05", "KATEGORI", "Kategori", 5),
            new("7b2e3d40-0006-4e62-8b21-5a9c3d8f6b06", "DIAG_KLINIS", "Diagnosa Klinis", 6),
            new("7b2e3d40-0007-4e62-8b21-5a9c3d8f6b07", "DIAG_PA", "Diagnosa PA", 7),
            new("7b2e3d40-0008-4e62-8b21-5a9c3d8f6b08", "ER", "Reseptor Estrogen (ER)", 8),
            new("7b2e3d40-0009-4e62-8b21-5a9c3d8f6b09", "PR", "Reseptor Progesteron (PR)", 9),
            new("7b2e3d40-000a-4e62-8b21-5a9c3d8f6b0a", "HER2", "HER2", 10),
            new("7b2e3d40-000b-4e62-8b21-5a9c3d8f6b0b", "KI67", "Ki-67", 11),
            new("7b2e3d40-000c-4e62-8b21-5a9c3d8f6b0c", "STATUS_ER", "Status Reseptor Estrogen (ER)", 12),
            new("7b2e3d40-000d-4e62-8b21-5a9c3d8f6b0d", "STATUS_PR", "Status Reseptor Progesteron (PR)", 13),
            new("7b2e3d40-000e-4e62-8b21-5a9c3d8f6b0e", "HER2_IHK", "HER2 dengan pemeriksaan Imunohistokimia", 14),
            new("7b2e3d40-000f-4e62-8b21-5a9c3d8f6b0f", "ANJURAN", "Anjuran", 99)
        };

        /// <summary>
        /// Keberlakuan ruas per golongan — <b>19 pasangan</b> dari 15 ruas.
        ///
        /// <b>Selisih 19 terhadap 15 adalah ruas yang dipakai lebih dari satu golongan</b>, dan
        /// itu justru sebab keberlakuan dipisahkan menjadi tabel tersendiri: ketiga ruas
        /// Histologi dipakai ulang Sitologi Non-Ginekologi (+3), dan <c>ANJURAN</c> dipakai
        /// Sitologi Ginekologi sekaligus Imunohistokimia (+1).
        ///
        /// <b>Catatan selisih yang sengaja tidak ditambal diam-diam.</b> <c>AC-129</c> pada
        /// roadmap dan bagian 15.12 arsitektur sama-sama menulis <b>21</b> pasangan, sedangkan
        /// rincian yang keduanya bawa sendiri — Histologi 3, Sitologi Non-Gin 3, Sitologi Gin 3,
        /// IHK 10 — berjumlah <b>19</b>. Bukti sumbernya, <c>LAB-EVD-003</c> bagian 5.6, juga
        /// menghasilkan 19. Angka 21 karena itu diperlakukan sebagai selisih aritmetika pada
        /// dokumen perencanaan, bukan sebagai empat pasangan yang hilang; dilaporkan pada
        /// <c>BE-LAB-50.md</c> untuk diputuskan pemilik modul.
        ///
        /// Seluruhnya <c>IsRequired</c>: <c>LAB-EVD-003</c> menyatakan setiap ruas yang tampil
        /// sesuai golongannya wajib terisi.
        /// </summary>
        private static readonly (string CategoryCode, string ParameterCode)[] BaselineApplicability =
        {
            ("HISTO", "MAKROSKOPIK"),
            ("HISTO", "MIKROSKOPIK"),
            ("HISTO", "KESIMPULAN"),

            ("SITO_NONGIN", "MAKROSKOPIK"),
            ("SITO_NONGIN", "MIKROSKOPIK"),
            ("SITO_NONGIN", "KESIMPULAN"),

            ("SITO_GIN", "KONDISI"),
            ("SITO_GIN", "KATEGORI"),
            ("SITO_GIN", "ANJURAN"),

            ("IHK", "DIAG_KLINIS"),
            ("IHK", "DIAG_PA"),
            ("IHK", "ER"),
            ("IHK", "PR"),
            ("IHK", "HER2"),
            ("IHK", "KI67"),
            ("IHK", "STATUS_ER"),
            ("IHK", "STATUS_PR"),
            ("IHK", "HER2_IHK"),
            ("IHK", "ANJURAN")
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
                .CreateLogger(nameof(LabPathologyMasterDataSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;

            if (!seedEnabled)
            {
                logger.LogInformation("Seeder data induk Patologi Anatomi dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            await SeedAsync(dbContext, logger, cancellationToken);
        }

        /// <summary>
        /// Menjalankan pengisian terhadap satu <see cref="ApplicationDbContext"/> yang sudah ada.
        /// Dipisahkan agar perilakunya dapat diperiksa tanpa membangun service provider.
        /// </summary>
        public static async Task<LabPathologyMasterDataSeedResult> SeedAsync(
            ApplicationDbContext dbContext,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            // ---------------------------------------------------------------
            // Golongan
            // ---------------------------------------------------------------

            // Baris yang sudah ditandai terhapus ikut dihitung, supaya kodenya tidak diisi ulang.
            var existingCategoryCodes = new HashSet<string>(
                await dbContext.LabPathologyCategories
                    .AsNoTracking()
                    .Select(x => x.CategoryCode)
                    .ToListAsync(cancellationToken),
                StringComparer.OrdinalIgnoreCase);

            var addedCategories = 0;

            foreach (var category in BaselineCategories)
            {
                if (existingCategoryCodes.Contains(category.Code))
                    continue;

                dbContext.LabPathologyCategories.Add(new LabPathologyCategory
                {
                    Id = Guid.Parse(category.Id),
                    CategoryCode = category.Code,
                    CategoryName = category.Name,
                    SortOrder = category.SortOrder,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty
                });

                addedCategories++;
            }

            // ---------------------------------------------------------------
            // Ruas isian
            // ---------------------------------------------------------------

            var existingParameterCodes = new HashSet<string>(
                await dbContext.LabPathologyParameters
                    .AsNoTracking()
                    .Select(x => x.ParameterCode)
                    .ToListAsync(cancellationToken),
                StringComparer.OrdinalIgnoreCase);

            var addedParameters = 0;

            foreach (var parameter in BaselineParameters)
            {
                if (existingParameterCodes.Contains(parameter.Code))
                    continue;

                dbContext.LabPathologyParameters.Add(new LabPathologyParameter
                {
                    Id = Guid.Parse(parameter.Id),
                    ParameterCode = parameter.Code,
                    ParameterName = parameter.Name,
                    SortOrder = parameter.SortOrder,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty
                });

                addedParameters++;
            }

            // Golongan dan ruas disimpan lebih dulu: keberlakuan menunjuk keduanya, dan foreign
            // key tidak dapat dipenuhi oleh baris yang masih tertahan di ChangeTracker bersamaan
            // dengan pencarian ulang di bawah.
            if (addedCategories > 0 || addedParameters > 0)
                await dbContext.SaveChangesAsync(cancellationToken);

            // ---------------------------------------------------------------
            // Keberlakuan
            // ---------------------------------------------------------------

            var categoryIdByCode = await dbContext.LabPathologyCategories
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .ToDictionaryAsync(x => x.CategoryCode, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

            var parameterIdByCode = await dbContext.LabPathologyParameters
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .ToDictionaryAsync(x => x.ParameterCode, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

            // Pasangan yang sudah ditandai terhapus ikut dihitung. Keberlakuan yang sengaja
            // dicabut kepala instalasi tidak boleh dipasang kembali diam-diam setiap kali
            // aplikasi menyala — itu membatalkan keputusannya tanpa ia pernah tahu.
            var existingPairs = (await dbContext.LabPathologyParameterCategories
                    .AsNoTracking()
                    .Select(x => new { x.LabPathologyCategoryId, x.LabPathologyParameterId })
                    .ToListAsync(cancellationToken))
                .Select(x => (x.LabPathologyCategoryId, x.LabPathologyParameterId))
                .ToHashSet();

            var addedApplicability = 0;
            var skippedApplicability = 0;

            foreach (var (categoryCode, parameterCode) in BaselineApplicability)
            {
                if (!categoryIdByCode.TryGetValue(categoryCode, out var categoryId) ||
                    !parameterIdByCode.TryGetValue(parameterCode, out var parameterId))
                {
                    // Terjadi ketika golongan atau ruasnya sudah dinonaktifkan-lalu-dihapus
                    // penggunanya. Dilewati dengan tenang; memaksanya berdiri berarti memasang
                    // ulang sesuatu yang sengaja dicabut.
                    skippedApplicability++;
                    continue;
                }

                if (existingPairs.Contains((categoryId, parameterId)))
                    continue;

                dbContext.LabPathologyParameterCategories.Add(new LabPathologyParameterCategory
                {
                    LabPathologyCategoryId = categoryId,
                    LabPathologyParameterId = parameterId,
                    IsRequired = true,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty
                });

                addedApplicability++;
            }

            if (addedApplicability > 0)
                await dbContext.SaveChangesAsync(cancellationToken);

            if (skippedApplicability > 0)
            {
                logger.LogWarning(
                    "Seeder data induk Patologi Anatomi melewati {SkippedCount} pasangan keberlakuan karena golongan atau ruasnya tidak tersedia.",
                    skippedApplicability);
            }

            var result = new LabPathologyMasterDataSeedResult(
                addedCategories,
                addedParameters,
                addedApplicability);

            if (result.Total == 0)
            {
                logger.LogInformation("Data induk Patologi Anatomi sudah lengkap; tidak ada baris baru yang ditambahkan.");
                return result;
            }

            logger.LogInformation(
                "Seeder data induk Patologi Anatomi menambahkan {CategoryCount} golongan, {ParameterCount} ruas isian, dan {ApplicabilityCount} keberlakuan.",
                result.AddedCategories,
                result.AddedParameters,
                result.AddedApplicability);

            // Pemetaan jenis pemeriksaan SENGAJA tidak diisi di sini. Selama ia kosong, nol
            // pemeriksaan Patologi Anatomi punya golongan dan formulir hasilnya kosong sama
            // sekali (INV-39, VAL-100). Diingatkan sebagai peringatan penyalaan supaya keadaan
            // itu tidak baru ketahuan ketika patolog pertama membuka layarnya.
            var mappingCount = await dbContext.LabProcedurePathologyCategories
                .AsNoTracking()
                .CountAsync(x => !x.IsDelete, cancellationToken);

            if (mappingCount == 0)
            {
                logger.LogWarning(
                    "Pemetaan jenis pemeriksaan ke golongan Patologi Anatomi masih kosong. Formulir hasil Patologi Anatomi akan kosong sampai kepala instalasi mengisinya.");
            }

            return result;
        }

        private sealed record BaselineCategory(
            string Id,
            string Code,
            string Name,
            int SortOrder);

        private sealed record BaselineParameter(
            string Id,
            string Code,
            string Name,
            int SortOrder);
    }

    /// <summary>
    /// Berapa baris yang benar-benar ditambahkan seeder, dipisah per data induk supaya
    /// pembuktian <c>AC-129</c> tidak bergantung pada membaca log.
    /// </summary>
    public sealed record LabPathologyMasterDataSeedResult(
        int AddedCategories,
        int AddedParameters,
        int AddedApplicability)
    {
        public int Total => AddedCategories + AddedParameters + AddedApplicability;
    }
}
