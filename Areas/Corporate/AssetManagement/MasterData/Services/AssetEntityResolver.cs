using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;

public sealed class AssetEntityOptions
{
    public const string SectionName = "AssetManagement";

    public Guid? LegalEntityId { get; set; }

    public Guid? HospitalSiteId { get; set; }
}

public sealed record AssetEntityContext(Guid LegalEntityId, string LegalEntityName, Guid HospitalSiteId, string HospitalSiteName);

public sealed class AssetEntityResolver
{
    public const string NotConfiguredCode = "AST-ENTITY-NOT-CONFIGURED";
    private const string NotConfiguredMessage =
        "Modul aset belum dapat dipakai karena badan hukum dan site rumah sakit belum diatur administrator.";
    private const string InvalidMessage =
        "Badan hukum atau site rumah sakit pada pengaturan modul aset tidak ditemukan, tidak aktif, atau tidak saling cocok. Hubungi administrator.";

    private readonly ApplicationDbContext _context;
    private readonly AssetEntityOptions _options;
    private AssetEntityContext? _resolved;

    public AssetEntityResolver(ApplicationDbContext context, IOptions<AssetEntityOptions> options)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<AssetEntityContext> ResolveAsync(CancellationToken cancellationToken = default)
    {
        if (_resolved != null)
        {
            return _resolved;
        }

        if (_options.LegalEntityId is not { } legalEntityId || legalEntityId == Guid.Empty ||
            _options.HospitalSiteId is not { } hospitalSiteId || hospitalSiteId == Guid.Empty)
        {
            throw AssetManagementException.Unavailable(NotConfiguredCode, NotConfiguredMessage);
        }

        var legalEntity = await _context.MstLegalEntities
            .AsNoTracking()
            .Where(x => x.Id == legalEntityId && x.IsActive && !x.IsDelete)
            .Select(x => new { x.Id, x.LegalEntityName })
            .FirstOrDefaultAsync(cancellationToken);

        var site = await _context.MstHospitalSites
            .AsNoTracking()
            .Where(x => x.Id == hospitalSiteId && x.IsActive && !x.IsDelete)
            .Select(x => new { x.Id, x.SiteName, x.LegalEntityId })
            .FirstOrDefaultAsync(cancellationToken);

        if (legalEntity == null || site == null || site.LegalEntityId != legalEntity.Id)
        {
            throw AssetManagementException.Unavailable(NotConfiguredCode, InvalidMessage);
        }

        return _resolved = new AssetEntityContext(legalEntity.Id, legalEntity.LegalEntityName, site.Id, site.SiteName);
    }
}
