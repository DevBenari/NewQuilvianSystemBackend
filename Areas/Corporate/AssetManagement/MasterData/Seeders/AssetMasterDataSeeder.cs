using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Seeders;

public sealed class AssetMasterDataSeeder : IHostedService
{
    public const string EnabledKey = "Seeders:RunAssetMasterDataSeed";
    public const string WarehouseUnitKey = "Seeders:AssetMasterData:WarehouseOrganizationUnitId";

    private static readonly (string Code, string Name, bool IsDepreciable)[] Categories =
    [
        ("ALAT_MEDIS", "Alat Medis", true),
        ("ALAT_NON_MEDIS", "Alat Non-Medis", true),
        ("KENDARAAN", "Kendaraan", true),
        ("GEDUNG", "Gedung dan Bangunan", true),
        ("TANAH", "Tanah", false),
        ("MEBEL", "Mebel dan Peralatan Kantor", true),
        ("PERALATAN_TI", "Peralatan TI", true)
    ];

    private static readonly (string Code, string Name, bool IsMedical)[] Classifications =
    [
        ("MEDIS", "Medis", true),
        ("NON_MEDIS", "Non-Medis", false)
    ];

    private static readonly (string Code, string Name, string CategoryCode, string ClassificationCode)[] Groups =
    [
        ("ALKMED", "Alat Kedokteran Umum", "ALAT_MEDIS", "MEDIS"),
        ("ALNM", "Alat Non-Medis Umum", "ALAT_NON_MEDIS", "NON_MEDIS"),
        ("KEND", "Kendaraan Roda Empat", "KENDARAAN", "NON_MEDIS"),
        ("GDG", "Gedung dan Bangunan", "GEDUNG", "NON_MEDIS"),
        ("TNH", "Tanah", "TANAH", "NON_MEDIS"),
        ("MBL", "Mebel", "MEBEL", "NON_MEDIS"),
        ("TI", "Peralatan TI", "PERALATAN_TI", "NON_MEDIS")
    ];

    private static readonly (string Code, string Name, bool IsCapitalizable)[] OwnershipTypes =
    [
        ("MILIK", "Milik Rumah Sakit", true),
        ("SEWA", "Sewa", false),
        ("KSO", "Kerja Sama Operasional", false),
        ("PINJAM_PAKAI", "Pinjam Pakai", false)
    ];

    private static readonly (string Code, string Name, bool HasExpiry)[] DocumentTypes =
    [
        ("SERTIFIKAT_TANAH", "Sertifikat Tanah", false),
        ("PBG_IMB", "PBG/IMB", false),
        ("BPKB", "BPKB", false),
        ("STNK", "STNK", true),
        ("POLIS_ASURANSI", "Polis Asuransi", true),
        ("FAKTUR_PEMBELIAN", "Faktur Pembelian", false),
        ("BA_HIBAH", "Berita Acara Hibah", false)
    ];

    private static readonly (string CategoryCode, string DocumentTypeCode)[] DocumentRequirements =
    [
        ("TANAH", "SERTIFIKAT_TANAH"),
        ("GEDUNG", "PBG_IMB"),
        ("KENDARAAN", "BPKB"),
        ("KENDARAAN", "STNK")
    ];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<AssetMasterDataSeeder> _logger;

    public AssetMasterDataSeeder(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<AssetMasterDataSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_configuration.GetValue<bool>(EnabledKey))
        {
            return;
        }

        if (_environment.IsProduction())
        {
            _logger.LogWarning("AssetMasterDataSeeder ditolak: seeder data contoh aset tidak berjalan di lingkungan Production.");
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var result = await SeedAsync(context, scope.ServiceProvider.GetRequiredService<AssetEntityResolver>(), cancellationToken);
            _logger.LogInformation("AssetMasterDataSeeder selesai: {Result}", result);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "AssetMasterDataSeeder gagal; aplikasi tetap berjalan.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task<string> SeedAsync(ApplicationDbContext context, AssetEntityResolver entityResolver, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var inserted = new Dictionary<string, int>();

        var categories = await context.MstAssetCategories.Where(x => !x.IsDelete).ToDictionaryAsync(x => x.CategoryCode, cancellationToken);
        foreach (var (code, name, isDepreciable) in Categories.Where(x => !categories.ContainsKey(x.Code)))
        {
            var entity = new MstAssetCategory { CategoryCode = code, CategoryName = name, IsDepreciable = isDepreciable, CreateDateTime = now };
            context.MstAssetCategories.Add(entity);
            categories[code] = entity;
            Count(inserted, "kategori");
        }

        var classifications = await context.MstAssetClassifications.Where(x => !x.IsDelete).ToDictionaryAsync(x => x.ClassificationCode, cancellationToken);
        foreach (var (code, name, isMedical) in Classifications.Where(x => !classifications.ContainsKey(x.Code)))
        {
            var entity = new MstAssetClassification { ClassificationCode = code, ClassificationName = name, IsMedical = isMedical, CreateDateTime = now };
            context.MstAssetClassifications.Add(entity);
            classifications[code] = entity;
            Count(inserted, "klasifikasi");
        }

        var groupCodes = await context.MstAssetGroups.Where(x => !x.IsDelete).Select(x => x.InventoryCode).ToListAsync(cancellationToken);
        foreach (var (code, name, categoryCode, classificationCode) in Groups.Where(x => !groupCodes.Contains(x.Code)))
        {
            context.MstAssetGroups.Add(new MstAssetGroup
            {
                InventoryCode = code,
                GroupName = name,
                CategoryId = categories[categoryCode].Id,
                ClassificationId = classifications[classificationCode].Id,
                CreateDateTime = now
            });
            Count(inserted, "kelompok");
        }

        var ownershipCodes = await context.MstAssetOwnershipTypes.Where(x => !x.IsDelete).Select(x => x.OwnershipCode).ToListAsync(cancellationToken);
        foreach (var (code, name, isCapitalizable) in OwnershipTypes.Where(x => !ownershipCodes.Contains(x.Code)))
        {
            context.MstAssetOwnershipTypes.Add(new MstAssetOwnershipType { OwnershipCode = code, OwnershipName = name, IsCapitalizable = isCapitalizable, CreateDateTime = now });
            Count(inserted, "jenis kepemilikan");
        }

        var documentTypes = await context.MstAssetDocumentTypes.Where(x => !x.IsDelete).ToDictionaryAsync(x => x.DocumentTypeCode, cancellationToken);
        foreach (var (code, name, hasExpiry) in DocumentTypes.Where(x => !documentTypes.ContainsKey(x.Code)))
        {
            var entity = new MstAssetDocumentType { DocumentTypeCode = code, DocumentTypeName = name, HasExpiry = hasExpiry, CreateDateTime = now };
            context.MstAssetDocumentTypes.Add(entity);
            documentTypes[code] = entity;
            Count(inserted, "jenis dokumen");
        }

        var requirements = await context.MstAssetCategoryDocumentRequirements
            .Where(x => !x.IsDelete)
            .Select(x => new { x.CategoryId, x.DocumentTypeId })
            .ToListAsync(cancellationToken);
        foreach (var (categoryCode, documentTypeCode) in DocumentRequirements)
        {
            var categoryId = categories[categoryCode].Id;
            var documentTypeId = documentTypes[documentTypeCode].Id;
            if (requirements.Any(x => x.CategoryId == categoryId && x.DocumentTypeId == documentTypeId))
            {
                continue;
            }

            context.MstAssetCategoryDocumentRequirements.Add(new MstAssetCategoryDocumentRequirement
            {
                CategoryId = categoryId,
                DocumentTypeId = documentTypeId,
                CreateDateTime = now
            });
            Count(inserted, "dokumen wajib");
        }

        var skipped = new List<string>();
        var warehouseUnitId = _configuration.GetValue<Guid?>(WarehouseUnitKey);
        if (warehouseUnitId is { } unitId && unitId != Guid.Empty &&
            await context.MstOrganizationUnits.AnyAsync(x => x.Id == unitId && x.IsActive && !x.IsDelete, cancellationToken))
        {
            if (!await context.MstAssetLocations.AnyAsync(x => !x.IsDelete && x.LocationCode == "GUDANG-ASET", cancellationToken))
            {
                context.MstAssetLocations.Add(new MstAssetLocation
                {
                    LocationCode = "GUDANG-ASET",
                    LocationName = "Gudang Bagian Aset",
                    OrganizationUnitId = unitId,
                    CreateDateTime = now
                });
                Count(inserted, "lokasi");
            }
        }
        else
        {
            skipped.Add($"lokasi gudang ({WarehouseUnitKey} kosong atau unit tidak aktif)");
        }

        try
        {
            var entityContext = await entityResolver.ResolveAsync(cancellationToken);
            if (!await context.MstAssetSettings.AnyAsync(x => x.LegalEntityId == entityContext.LegalEntityId && !x.IsDelete, cancellationToken))
            {
                context.MstAssetSettings.Add(new MstAssetSetting
                {
                    LegalEntityId = entityContext.LegalEntityId,
                    HospitalSiteId = entityContext.HospitalSiteId,
                    IdleThresholdDays = AssetMasterDataService.DefaultIdleThresholdDays,
                    DocumentExpiryWarningDays = AssetMasterDataService.DefaultDocumentExpiryWarningDays,
                    UtilizationMaxMonthsForAssetHead = AssetMasterDataService.DefaultUtilizationMaxMonthsForAssetHead,
                    CreateDateTime = now
                });
                Count(inserted, "pengaturan");
            }
        }
        catch (AssetManagementException)
        {
            skipped.Add("pengaturan (AssetManagement:LegalEntityId/HospitalSiteId belum diatur)");
        }

        await context.SaveChangesAsync(cancellationToken);

        var insertedText = inserted.Count == 0 ? "tidak ada data baru" : string.Join(", ", inserted.Select(x => $"{x.Value} {x.Key}"));
        return skipped.Count == 0 ? insertedText : $"{insertedText}; dilewati: {string.Join("; ", skipped)}";
    }

    private static void Count(Dictionary<string, int> counter, string key) =>
        counter[key] = counter.TryGetValue(key, out var value) ? value + 1 : 1;
}
