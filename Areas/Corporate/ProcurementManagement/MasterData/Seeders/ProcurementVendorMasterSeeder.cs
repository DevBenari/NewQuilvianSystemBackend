using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Seeders
{
    /// <summary>
    /// Data awal master manajemen vendor (BE-PBJ-VD-001; <c>manajemen-vendor/02-backend-architecture.md</c> bagian 10).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Isi: lima jenis dokumen legalitas (PBJ-ASM-001) dan tiga kriteria penilaian untuk kategori <c>UMUM</c>
    /// (contoh PBJ-DEC-050: Ketepatan waktu 40%, Kualitas 40%, Layanan purnajual 20%). Semuanya nilai awal; daftar
    /// dan bobot final mengikuti SOP rumah sakit (PBJ-OQ-019, PBJ-OQ-034) dan diubah Tim Pengadaan dari layar master.
    /// </para>
    /// <para>
    /// <b>Dokumen wajib per kategori tidak diisi</b> (PBJ-DEC-061): admin mengaktifkannya bertahap setelah dokumen
    /// vendor kategori itu terkumpul, supaya vendor yang sudah dipakai tidak mendadak terkunci.
    /// </para>
    /// <para>
    /// <b>Idempoten.</b> Jenis dokumen dicari menurut Id atau kodenya, kriteria menurut Id atau kategori dan namanya,
    /// termasuk yang sudah dihapus; mencari menurut Id juga mencegah bentrok kunci bila admin mengganti nama kriteria. Hanya yang belum ada yang ditambahkan; nilai yang sudah diubah admin tidak ditimpa, dan data
    /// yang sengaja dihapus admin tidak dihidupkan lagi. Kriteria dilewati bila kategori <c>UMUM</c> tidak ada.
    /// </para>
    /// </remarks>
    public static class ProcurementVendorMasterSeeder
    {
        private sealed record DocumentTypeSeed(Guid Id, string Code, string Name, bool RequiresExpiryDate, string Description);
        private sealed record CriterionSeed(Guid Id, string Name, decimal WeightPercent, string Description);

        private const string DefaultCategoryCode = "UMUM";

        private static readonly DocumentTypeSeed[] DocumentTypes =
        {
            new(Guid.Parse("6b1f2c70-0301-4e0a-9c31-5a7d2e4f8b31"), "NIB", "Nomor Induk Berusaha", false,
                "Identitas berusaha dari OSS; tidak memiliki tanggal berlaku"),
            new(Guid.Parse("6b1f2c70-0302-4e0a-9c31-5a7d2e4f8b32"), "NPWP", "Nomor Pokok Wajib Pajak", false,
                "Identitas wajib pajak badan usaha"),
            new(Guid.Parse("6b1f2c70-0303-4e0a-9c31-5a7d2e4f8b33"), "IZIN_PBF", "Izin Pedagang Besar Farmasi", true,
                "Wajib bagi penyalur obat; memiliki tanggal berlaku"),
            new(Guid.Parse("6b1f2c70-0304-4e0a-9c31-5a7d2e4f8b34"), "IZIN_DISTRIBUSI_ALKES", "Izin Distribusi Alat Kesehatan", true,
                "Wajib bagi penyalur alat kesehatan; memiliki tanggal berlaku"),
            new(Guid.Parse("6b1f2c70-0305-4e0a-9c31-5a7d2e4f8b35"), "LAIN", "Dokumen lain", false,
                "Dokumen pendukung lain yang tidak termasuk jenis di atas")
        };

        private static readonly CriterionSeed[] DefaultCategoryCriteria =
        {
            new(Guid.Parse("6b1f2c70-0401-4e0a-9c31-5a7d2e4f8b41"), "Ketepatan waktu", 40m,
                "Barang/jasa diterima sesuai tanggal yang dijanjikan"),
            new(Guid.Parse("6b1f2c70-0402-4e0a-9c31-5a7d2e4f8b42"), "Kualitas", 40m,
                "Barang/jasa sesuai spesifikasi dan tanpa cacat"),
            new(Guid.Parse("6b1f2c70-0403-4e0a-9c31-5a7d2e4f8b43"), "Layanan purnajual", 20m,
                "Tanggapan atas keluhan, retur, dan garansi")
        };

        public sealed record SeedResult(int DocumentType, int Criterion);

        public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(ProcurementVendorMasterSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;
            if (!seedEnabled)
            {
                logger.LogInformation("Seeder master vendor Pengadaan dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            var result = await SeedAsync(dbContext, cancellationToken);
            logger.LogInformation(
                "Seeder master vendor Pengadaan: {DocumentType} jenis dokumen, {Criterion} kriteria penilaian ditambahkan.",
                result.DocumentType, result.Criterion);
        }

        public static async Task<SeedResult> SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var documentTypeCodes = DocumentTypes.Select(x => x.Code).ToList();
            var documentTypeIds = DocumentTypes.Select(x => x.Id).ToList();
            var existingDocumentTypes = await dbContext.MstProcurementVendorDocumentTypes
                .Where(x => documentTypeIds.Contains(x.Id) || documentTypeCodes.Contains(x.DocumentTypeCode))
                .Select(x => new { x.Id, x.DocumentTypeCode })
                .ToListAsync(cancellationToken);
            var documentTypeAdded = 0;
            foreach (var seed in DocumentTypes.Where(x =>
                         !existingDocumentTypes.Any(e => e.Id == x.Id || e.DocumentTypeCode == x.Code)))
            {
                dbContext.MstProcurementVendorDocumentTypes.Add(new MstProcurementVendorDocumentType
                {
                    Id = seed.Id, DocumentTypeCode = seed.Code, DocumentTypeName = seed.Name,
                    RequiresExpiryDate = seed.RequiresExpiryDate, Description = seed.Description,
                    IsActive = true, CreateDateTime = now
                });
                documentTypeAdded++;
            }

            var criterionAdded = 0;
            var defaultCategoryId = await dbContext.MstProcurementItemCategories
                .Where(x => x.CategoryCode == DefaultCategoryCode && !x.IsDelete)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (defaultCategoryId.HasValue)
            {
                var criterionNames = DefaultCategoryCriteria.Select(x => x.Name).ToList();
                var criterionIds = DefaultCategoryCriteria.Select(x => x.Id).ToList();
                var existingCriteria = await dbContext.MstProcurementEvaluationCriteria
                    .Where(x => criterionIds.Contains(x.Id)
                        || (x.ItemCategoryId == defaultCategoryId.Value && criterionNames.Contains(x.CriterionName)))
                    .Select(x => new { x.Id, x.CriterionName })
                    .ToListAsync(cancellationToken);
                foreach (var seed in DefaultCategoryCriteria.Where(x =>
                             !existingCriteria.Any(e => e.Id == x.Id || e.CriterionName == x.Name)))
                {
                    dbContext.MstProcurementEvaluationCriteria.Add(new MstProcurementEvaluationCriterion
                    {
                        Id = seed.Id, ItemCategoryId = defaultCategoryId.Value, CriterionName = seed.Name,
                        WeightPercent = seed.WeightPercent, Description = seed.Description,
                        IsActive = true, CreateDateTime = now
                    });
                    criterionAdded++;
                }
            }

            if (documentTypeAdded + criterionAdded > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return new SeedResult(documentTypeAdded, criterionAdded);
        }
    }
}
