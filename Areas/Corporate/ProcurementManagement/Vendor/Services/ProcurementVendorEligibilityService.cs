using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.DTOs;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Services;

/// <summary>
/// Kelayakan vendor untuk satu kategori (BE-PBJ-VD-005; PBJ-DEC-045, 047, 052, 054, 061; PBJ-VD-INT 1.0 §2). Hanya
/// membaca; tidak pernah mengubah <c>MstSupplier</c>, dokumen, atau proses lain.
/// </summary>
/// <remarks>
/// <para>Vendor <b>tidak layak</b> bila salah satu berlaku, dan setiap sebab dituliskan:</para>
/// <list type="number">
/// <item>Nonaktif di Master Data, atau Diblokir (<c>MstSupplier.IsBlacklisted</c>, PBJ-DEC-021).</item>
/// <item>Kategori yang diperiksa nonaktif.</item>
/// <item>Vendor tidak melayani kategori itu (PBJ-DEC-052).</item>
/// <item>Dokumen wajib kategori yang <b>aktif</b> belum terverifikasi, atau dokumen terverifikasi terakhirnya sudah
/// kedaluwarsa (PBJ-DEC-045). Kategori tanpa kewajiban aktif tidak menuntut dokumen apa pun (PBJ-DEC-061).</item>
/// </list>
/// <para>
/// Vendor <b>Dibatasi</b> tetap layak — boleh dipilih dengan alasan pemilih (PBJ-DEC-047) — dan ditandai
/// <c>isRestricted</c>. Izin yang segera kedaluwarsa juga tidak menggugurkan; keduanya muncul di <c>warnings</c>.
/// Contoh: kategori Obat mewajibkan Izin PBF; PT Sehat Farma punya Izin PBF terverifikasi yang berakhir kemarin →
/// tidak layak, "Dokumen wajib Izin Pedagang Besar Farmasi kedaluwarsa sejak 16-11-2026."
/// </para>
/// <para>Kelayakan hanya mencegah pilihan <b>baru</b>; RFQ atau kontrak yang sudah berjalan tidak disentuh (PBJ-DEC-054).</para>
/// </remarks>
public sealed class ProcurementVendorEligibilityService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public ProcurementVendorEligibilityService(ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    /// <summary>Kelayakan satu vendor untuk satu kategori (kontrak bagi <c>pemilihan-penyedia</c>).</summary>
    public async Task<VendorEligibilityResponse> CheckAsync(Guid supplierId, Guid itemCategoryId, CancellationToken cancellationToken) =>
        (await EvaluateAsync(itemCategoryId, supplierId, onlyEligible: false, cancellationToken)).Single();

    /// <summary>Vendor yang layak untuk kategori itu (daftar pilihan RFQ/penetapan).</summary>
    public Task<List<VendorEligibilityResponse>> GetEligibleAsync(Guid itemCategoryId, CancellationToken cancellationToken) =>
        EvaluateAsync(itemCategoryId, null, onlyEligible: true, cancellationToken);

    /// <summary>
    /// Kelayakan untuk <c>GET /vendors/eligibility</c>. Dengan <paramref name="supplierId"/>: satu vendor itu (walaupun tidak
    /// melayani kategori). Tanpa <paramref name="supplierId"/>: semua vendor yang melayani kategori.
    /// </summary>
    public async Task<List<VendorEligibilityResponse>> EvaluateAsync(
        Guid itemCategoryId, Guid? supplierId, bool onlyEligible, CancellationToken cancellationToken)
    {
        var category = await _dbContext.MstProcurementItemCategories.AsNoTracking()
            .Where(x => x.Id == itemCategoryId && !x.IsDelete)
            .Select(x => new { x.Id, x.CategoryName, x.IsActive })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Kategori tidak ditemukan atau sudah dihapus.");

        var suppliersQuery = _dbContext.MstSuppliers.AsNoTracking().Where(x => !x.IsDelete);
        if (supplierId.HasValue)
        {
            suppliersQuery = suppliersQuery.Where(x => x.Id == supplierId.Value);
        }
        else
        {
            suppliersQuery = suppliersQuery.Where(s => _dbContext.PrcVendorCategories.Any(c =>
                c.ItemCategoryId == itemCategoryId && !c.IsDelete && c.VendorProfile!.SupplierId == s.Id && !c.VendorProfile.IsDelete));
        }

        var suppliers = await suppliersQuery
            .OrderBy(x => x.SupplierName)
            .Select(x => new { x.Id, x.SupplierCode, x.SupplierName, x.IsActive, x.IsBlacklisted })
            .ToListAsync(cancellationToken);
        if (supplierId.HasValue && suppliers.Count == 0)
            throw new KeyNotFoundException("Vendor tidak ditemukan atau sudah dihapus di Master Data.");

        var supplierIds = suppliers.Select(x => x.Id).ToList();
        var profiles = await _dbContext.PrcVendorProfiles.AsNoTracking()
            .Where(x => supplierIds.Contains(x.SupplierId) && !x.IsDelete)
            .Select(x => new { x.Id, x.SupplierId, x.IsRestricted, x.RestrictedUntil, x.RestrictionReason })
            .ToListAsync(cancellationToken);
        var profileIds = profiles.Select(x => x.Id).ToList();
        var servingProfileIds = (await _dbContext.PrcVendorCategories.AsNoTracking()
                .Where(x => profileIds.Contains(x.VendorProfileId) && x.ItemCategoryId == itemCategoryId && !x.IsDelete)
                .Select(x => x.VendorProfileId)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var requirements = await _dbContext.MstProcurementCategoryDocumentRequirements.AsNoTracking()
            .Where(x => x.ItemCategoryId == itemCategoryId && x.IsActive && !x.IsDelete && !x.DocumentType!.IsDelete)
            .OrderBy(x => x.DocumentType!.DocumentTypeName)
            .Select(x => new { x.DocumentTypeId, x.DocumentType!.DocumentTypeName })
            .ToListAsync(cancellationToken);
        var requiredTypeIds = requirements.Select(x => x.DocumentTypeId).ToList();
        var verified = requiredTypeIds.Count == 0 || profileIds.Count == 0
            ? []
            : await _dbContext.PrcVendorDocuments.AsNoTracking()
                .Where(x => profileIds.Contains(x.VendorProfileId) && requiredTypeIds.Contains(x.DocumentTypeId) && !x.IsDelete
                    && x.VerificationStatus == ProcurementStatuses.VendorDocument.Verified)
                .Select(x => new DocumentRow(x.VendorProfileId, x.DocumentTypeId, x.ExpiryDate, x.VerifiedAt, x.CreateDateTime))
                .ToListAsync(cancellationToken);
        var effective = verified
            .GroupBy(x => (x.ProfileId, x.DocumentTypeId))
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.VerifiedAt ?? new DateTimeOffset(x.CreateDateTime, TimeSpan.Zero)).First());

        var today = DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());
        var warningDays = ProcurementVendorDocumentValidity.WarningDays(_configuration);

        var results = new List<VendorEligibilityResponse>();
        foreach (var supplier in suppliers)
        {
            var profile = profiles.FirstOrDefault(x => x.SupplierId == supplier.Id);
            var result = new VendorEligibilityResponse
            {
                SupplierId = supplier.Id,
                ItemCategoryId = itemCategoryId,
                SupplierCode = supplier.SupplierCode,
                SupplierName = supplier.SupplierName
            };

            if (!supplier.IsActive) result.Reasons.Add("Vendor nonaktif di Master Data.");
            if (supplier.IsBlacklisted) result.Reasons.Add("Vendor diblokir.");
            if (!category.IsActive) result.Reasons.Add($"Kategori {category.CategoryName} tidak aktif.");
            if (profile == null || !servingProfileIds.Contains(profile.Id)) result.Reasons.Add("Vendor tidak melayani kategori ini.");

            foreach (var requirement in requirements)
            {
                if (profile == null || !effective.TryGetValue((profile.Id, requirement.DocumentTypeId), out var document))
                {
                    result.Reasons.Add($"Dokumen wajib {requirement.DocumentTypeName} belum terverifikasi.");
                    continue;
                }

                var validity = ProcurementVendorDocumentValidity.Resolve(document.ExpiryDate, today, warningDays);
                if (validity == ProcurementVendorDocumentValidity.Expired)
                    result.Reasons.Add($"Dokumen wajib {requirement.DocumentTypeName} kedaluwarsa sejak {document.ExpiryDate!.Value:dd-MM-yyyy}.");
                else if (validity == ProcurementVendorDocumentValidity.Expiring)
                    result.Warnings.Add(
                        $"Dokumen wajib {requirement.DocumentTypeName} berakhir dalam {document.ExpiryDate!.Value.DayNumber - today.DayNumber} hari ({document.ExpiryDate.Value:dd-MM-yyyy}).");
            }

            result.IsRestricted = profile != null && profile.IsRestricted
                && (!profile.RestrictedUntil.HasValue || profile.RestrictedUntil.Value >= today);
            if (result.IsRestricted)
                result.Warnings.Insert(0, profile!.RestrictedUntil.HasValue
                    ? $"Dibatasi sampai {profile.RestrictedUntil.Value:dd-MM-yyyy}: {profile.RestrictionReason}"
                    : $"Dibatasi sampai dicabut: {profile.RestrictionReason}");

            result.IsEligible = result.Reasons.Count == 0;
            if (!onlyEligible || result.IsEligible) results.Add(result);
        }

        return results;
    }

    private sealed record DocumentRow(Guid ProfileId, Guid DocumentTypeId, DateOnly? ExpiryDate, DateTimeOffset? VerifiedAt, DateTime CreateDateTime);
}
