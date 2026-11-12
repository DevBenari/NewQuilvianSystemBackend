using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Seeders
{
    /// <summary>
    /// Data awal master Pengadaan (BE-PBJ-003, <c>02-backend-architecture.md</c> bagian 10).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Isi: empat jenis pengadaan (PBJ-DEC-024), satu kategori <c>UMUM</c>, dan dua sumber dana
    /// <c>OPERASIONAL</c> serta <c>INVESTASI</c> (nilai awal G-07/G-10, dapat diubah Tim Pengadaan).
    /// </para>
    /// <para>
    /// <b>Idempoten.</b> Data dicari menurut kodenya, termasuk yang sudah dihapus. Hanya yang belum
    /// ada yang ditambahkan; nilai yang sudah diubah admin tidak pernah ditimpa, dan data yang
    /// sengaja dihapus admin tidak dihidupkan lagi.
    /// </para>
    /// </remarks>
    public static class ProcurementMasterDataSeeder
    {
        private sealed record TypeSeed(Guid Id, string Code, string Name, bool IsGoods, string Description);
        private sealed record ReferenceSeed(Guid Id, string Code, string Name, string Description);

        private static readonly TypeSeed[] Types =
        {
            new(Guid.Parse("6b1f2c70-0001-4e0a-9c31-5a7d2e4f8b01"), "STOCK", "Barang Stok", true,
                "Barang persediaan yang disimpan di gudang"),
            new(Guid.Parse("6b1f2c70-0002-4e0a-9c31-5a7d2e4f8b02"), "ASSET", "Aset", true,
                "Barang yang dicatat sebagai aset tetap"),
            new(Guid.Parse("6b1f2c70-0003-4e0a-9c31-5a7d2e4f8b03"), "NON_STOCK", "Barang Nonstok", true,
                "Barang habis pakai yang tidak disimpan sebagai persediaan"),
            new(Guid.Parse("6b1f2c70-0004-4e0a-9c31-5a7d2e4f8b04"), "SERVICE", "Jasa", false,
                "Pekerjaan jasa; serah terima lewat BAST pekerjaan")
        };

        private static readonly ReferenceSeed[] Categories =
        {
            new(Guid.Parse("6b1f2c70-0101-4e0a-9c31-5a7d2e4f8b11"), "UMUM", "Umum",
                "Kategori bawaan; Tim Pengadaan menambah kategori lain sesuai kebutuhan")
        };

        private static readonly ReferenceSeed[] FundSources =
        {
            new(Guid.Parse("6b1f2c70-0201-4e0a-9c31-5a7d2e4f8b21"), "OPERASIONAL", "Operasional", "Belanja operasional"),
            new(Guid.Parse("6b1f2c70-0202-4e0a-9c31-5a7d2e4f8b22"), "INVESTASI", "Investasi", "Belanja modal/investasi")
        };

        public sealed record SeedResult(int Type, int Category, int FundSource);

        public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(ProcurementMasterDataSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;
            if (!seedEnabled)
            {
                logger.LogInformation("Seeder master Pengadaan dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            var result = await SeedAsync(dbContext, cancellationToken);
            logger.LogInformation(
                "Seeder master Pengadaan: {Type} jenis, {Category} kategori, {FundSource} sumber dana ditambahkan.",
                result.Type, result.Category, result.FundSource);
        }

        public static async Task<SeedResult> SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var typeCodes = Types.Select(x => x.Code).ToList();
            var existingTypes = await dbContext.MstProcurementTypes
                .Where(x => typeCodes.Contains(x.TypeCode))
                .Select(x => x.TypeCode)
                .ToListAsync(cancellationToken);
            var typeAdded = 0;
            foreach (var seed in Types.Where(x => !existingTypes.Contains(x.Code)))
            {
                dbContext.MstProcurementTypes.Add(new MstProcurementType
                {
                    Id = seed.Id, TypeCode = seed.Code, TypeName = seed.Name, IsGoods = seed.IsGoods,
                    Description = seed.Description, IsActive = true, CreateDateTime = now
                });
                typeAdded++;
            }

            var categoryCodes = Categories.Select(x => x.Code).ToList();
            var existingCategories = await dbContext.MstProcurementItemCategories
                .Where(x => categoryCodes.Contains(x.CategoryCode))
                .Select(x => x.CategoryCode)
                .ToListAsync(cancellationToken);
            var categoryAdded = 0;
            foreach (var seed in Categories.Where(x => !existingCategories.Contains(x.Code)))
            {
                dbContext.MstProcurementItemCategories.Add(new MstProcurementItemCategory
                {
                    Id = seed.Id, CategoryCode = seed.Code, CategoryName = seed.Name,
                    Description = seed.Description, IsActive = true, CreateDateTime = now
                });
                categoryAdded++;
            }

            var fundCodes = FundSources.Select(x => x.Code).ToList();
            var existingFunds = await dbContext.MstProcurementFundSources
                .Where(x => fundCodes.Contains(x.FundSourceCode))
                .Select(x => x.FundSourceCode)
                .ToListAsync(cancellationToken);
            var fundAdded = 0;
            foreach (var seed in FundSources.Where(x => !existingFunds.Contains(x.Code)))
            {
                dbContext.MstProcurementFundSources.Add(new MstProcurementFundSource
                {
                    Id = seed.Id, FundSourceCode = seed.Code, FundSourceName = seed.Name,
                    Description = seed.Description, IsActive = true, CreateDateTime = now
                });
                fundAdded++;
            }

            if (typeAdded + categoryAdded + fundAdded > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return new SeedResult(typeAdded, categoryAdded, fundAdded);
        }
    }
}
