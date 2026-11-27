using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Services;

/// <summary>
/// Hak atas dokumen induk lampiran Pemilihan Penyedia (BE-PBJ-PS-006; arsitektur §6, PBJ-PS-PERM 1.0 bagian 3–4):
/// paket <c>PBJ_PACKAGE</c> dan RFQ <c>PBJ_RFQ</c>. Dipakai <see cref="ProcurementAttachmentService"/> lewat dua adaptor
/// <see cref="IProcurementDocumentAccessProvider"/> di bawah.
/// </summary>
/// <remarks>
/// <para>Kewenangan dibaca dari izin yang dicentang admin di layar Akses Role, bukan dari nama peran. Contoh bundel
/// PBJ-DEC-080: Tim Pengadaan memegang <c>ProcurementPackage : Read, Update</c> → <c>Edit</c> selama persiapan;
/// penyetuju dan Keuangan memegang <c>ProcurementPackage : Read</c> → <c>Read</c>; Unit Peminta hanya
/// <c>ReadSummary</c> → 404.</para>
/// <para><b>Paket:</b> <c>Read</c> bila paket dalam cakupan unit (<c>ReadAllUnits</c>, PBJ-DEC-020) dan pengguna memegang
/// <c>ProcurementPackage : Read</c>; <c>Edit</c> bila juga memegang <c>Update</c> dan paket masih <c>DRAFT</c>/
/// <c>PREPARATION_RETURNED</c>. Evaluator yang ditugaskan aktif (<c>PrcPackageEvaluator</c>, PBJ-DEC-074) mendapat
/// <c>Read</c> walaupun tanpa <c>ProcurementPackage : Read</c>.</para>
/// <para><b>RFQ:</b> <c>Read</c> bila paket induknya dalam cakupan dan pengguna memegang <c>ProcurementRfq : Read</c> atau
/// <c>ProcurementQuotation : Read</c>, atau evaluator paket itu; <c>Edit</c> bila memegang <c>ProcurementRfq : Update</c>,
/// <c>ProcurementQuotation : Create</c>, atau <c>ProcurementQuotation : Update</c> selama RFQ <c>DRAFT</c>/<c>SENT</c>/
/// <c>CLOSED</c> dan paket <c>READY_FOR_RFQ</c>/<c>RFQ_OPEN</c>/<c>EVALUATION</c>.</para>
/// <para>Harga tidak diatur di sini, melainkan oleh <see cref="ProcurementSourcingAttachmentCategoryPolicy"/>.</para>
/// </remarks>
public sealed class ProcurementSourcingAccessProvider
{
    private static readonly HashSet<string> RfqEditableStatuses = new(StringComparer.Ordinal)
    {
        ProcurementStatuses.Rfq.Draft,
        ProcurementStatuses.Rfq.Sent,
        ProcurementStatuses.Rfq.Closed
    };

    private static readonly HashSet<string> RfqEditablePackageStatuses = new(StringComparer.Ordinal)
    {
        ProcurementStatuses.Package.ReadyForRfq,
        ProcurementStatuses.Package.RfqOpen,
        ProcurementStatuses.Package.Evaluation
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly ProcurementPackageService _packageService;

    public ProcurementSourcingAccessProvider(
        ApplicationDbContext dbContext,
        AccessPermissionService accessPermissionService,
        ProcurementPackageService packageService)
    {
        _dbContext = dbContext;
        _accessPermissionService = accessPermissionService;
        _packageService = packageService;
    }

    public async Task<ProcurementDocumentAccessLevel> GetPackageAccessAsync(
        ClaimsPrincipal user, Guid packageId, CancellationToken cancellationToken)
    {
        var status = await _dbContext.PrcPackages.AsNoTracking()
            .Where(x => x.Id == packageId && !x.IsDelete)
            .Select(x => x.Status)
            .FirstOrDefaultAsync(cancellationToken);
        if (status == null) return ProcurementDocumentAccessLevel.None;

        if (await HasAsync(user, ProcurementPermissions.PackageResource, ProcurementPermissions.ReadAction) &&
            await _packageService.IsInScopeAsync(user, packageId, cancellationToken))
        {
            return ProcurementStatuses.Package.IsPreparationEditable(status) &&
                   await HasAsync(user, ProcurementPermissions.PackageResource, ProcurementPermissions.UpdateAction)
                ? ProcurementDocumentAccessLevel.Edit
                : ProcurementDocumentAccessLevel.Read;
        }

        return await IsAssignedEvaluatorAsync(user, packageId, cancellationToken)
            ? ProcurementDocumentAccessLevel.Read
            : ProcurementDocumentAccessLevel.None;
    }

    public async Task<ProcurementDocumentAccessLevel> GetRfqAccessAsync(
        ClaimsPrincipal user, Guid rfqId, CancellationToken cancellationToken)
    {
        var rfq = await _dbContext.PrcRfqs.AsNoTracking()
            .Where(x => x.Id == rfqId && !x.IsDelete && !x.Package!.IsDelete)
            .Select(x => new { x.PackageId, x.Status, PackageStatus = x.Package!.Status })
            .FirstOrDefaultAsync(cancellationToken);
        if (rfq == null) return ProcurementDocumentAccessLevel.None;

        var canRead = await HasAsync(user, ProcurementPermissions.RfqResource, ProcurementPermissions.ReadAction) ||
                      await HasAsync(user, ProcurementPermissions.QuotationResource, ProcurementPermissions.ReadAction);
        if (canRead && await _packageService.IsInScopeAsync(user, rfq.PackageId, cancellationToken))
        {
            var editable = RfqEditableStatuses.Contains(rfq.Status) && RfqEditablePackageStatuses.Contains(rfq.PackageStatus);
            return editable && await CanEditRfqAsync(user)
                ? ProcurementDocumentAccessLevel.Edit
                : ProcurementDocumentAccessLevel.Read;
        }

        return await IsAssignedEvaluatorAsync(user, rfq.PackageId, cancellationToken)
            ? ProcurementDocumentAccessLevel.Read
            : ProcurementDocumentAccessLevel.None;
    }

    private async Task<bool> CanEditRfqAsync(ClaimsPrincipal user) =>
        await HasAsync(user, ProcurementPermissions.RfqResource, ProcurementPermissions.UpdateAction) ||
        await HasAsync(user, ProcurementPermissions.QuotationResource, ProcurementPermissions.CreateAction) ||
        await HasAsync(user, ProcurementPermissions.QuotationResource, ProcurementPermissions.UpdateAction);

    private Task<bool> IsAssignedEvaluatorAsync(ClaimsPrincipal user, Guid packageId, CancellationToken cancellationToken)
    {
        var userId = ProcurementDataScopeService.GetUserId(user);
        return userId == Guid.Empty
            ? Task.FromResult(false)
            : _dbContext.PrcPackageEvaluators.AsNoTracking()
                .AnyAsync(x => x.PackageId == packageId && x.EvaluatorUserId == userId && x.IsActive && !x.IsDelete, cancellationToken);
    }

    private Task<bool> HasAsync(ClaimsPrincipal user, string resource, string action) =>
        _accessPermissionService.HasAccessAsync(user, resource, action);
}

/// <summary>Penyedia akses lampiran paket <c>PBJ_PACKAGE</c> (BE-PBJ-PS-006).</summary>
public sealed class PackageAttachmentAccessProvider : IProcurementDocumentAccessProvider
{
    private readonly ProcurementSourcingAccessProvider _provider;

    public PackageAttachmentAccessProvider(ProcurementSourcingAccessProvider provider) => _provider = provider;

    public string DocumentType => ProcurementDocumentTypes.Package;

    public Task<ProcurementDocumentAccessLevel> GetAccessAsync(ClaimsPrincipal user, Guid documentId, CancellationToken cancellationToken) =>
        _provider.GetPackageAccessAsync(user, documentId, cancellationToken);
}

/// <summary>Penyedia akses lampiran RFQ dan penawarannya <c>PBJ_RFQ</c> (BE-PBJ-PS-006).</summary>
public sealed class RfqAttachmentAccessProvider : IProcurementDocumentAccessProvider
{
    private readonly ProcurementSourcingAccessProvider _provider;

    public RfqAttachmentAccessProvider(ProcurementSourcingAccessProvider provider) => _provider = provider;

    public string DocumentType => ProcurementDocumentTypes.Rfq;

    public Task<ProcurementDocumentAccessLevel> GetAccessAsync(ClaimsPrincipal user, Guid documentId, CancellationToken cancellationToken) =>
        _provider.GetRfqAccessAsync(user, documentId, cancellationToken);
}

/// <summary>
/// Aturan kategori lampiran paket dan RFQ (BE-PBJ-PS-006; PBJ-DEC-079, PBJ-DEC-095, PBJ-CAP-004).
/// </summary>
/// <remarks>
/// <para><b>Kategori wajib dari daftar jenis dokumen paket.</b> Kategori = <c>DocumentTypeCode</c> jenis dokumen paket yang
/// aktif; kategori bebas ditolak 422 <c>PBJ-VAL-080</c>. Contoh: <c>KAK_TOR</c> diterima, <c>"Lain-lain"</c> ditolak.
/// Huruf kecil dibakukan menjadi huruf besar (<c>kak_tor</c> → <c>KAK_TOR</c>).</para>
/// <para><b>Lampiran harga tertutup.</b> Pengguna tanpa <c>ProcurementPackage : ReadPrice</c> hanya melihat lampiran
/// berkategori jenis dokumen yang <b>bukan</b> <c>IsPriceSensitive</c>. Kategori yang tidak dikenal ikut tersembunyi
/// (fail-closed). Contoh: Keuangan melihat KAK/TOR, tetapi tidak melihat Dasar HPS.</para>
/// </remarks>
public sealed class ProcurementSourcingAttachmentCategoryPolicy : IProcurementAttachmentCategoryPolicy
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AccessPermissionService _accessPermissionService;

    public ProcurementSourcingAttachmentCategoryPolicy(ApplicationDbContext dbContext, AccessPermissionService accessPermissionService)
    {
        _dbContext = dbContext;
        _accessPermissionService = accessPermissionService;
    }

    public bool Handles(string documentType) =>
        string.Equals(documentType, ProcurementDocumentTypes.Package, StringComparison.Ordinal) ||
        string.Equals(documentType, ProcurementDocumentTypes.Rfq, StringComparison.Ordinal);

    public async Task<string> NormalizeCategoryAsync(string documentType, string? category, CancellationToken cancellationToken)
    {
        var code = category?.Trim().ToUpperInvariant() ?? string.Empty;
        if (code.Length == 0)
            throw new ProcurementAttachmentValidationException("PBJ-VAL-080", "Pilih jenis dokumen paket untuk lampiran ini.");

        var known = await _dbContext.MstProcurementPackageDocumentTypes.AsNoTracking()
            .Where(x => x.DocumentTypeCode == code && x.IsActive && !x.IsDelete)
            .Select(x => x.DocumentTypeCode)
            .FirstOrDefaultAsync(cancellationToken);
        return known ?? throw new ProcurementAttachmentValidationException(
            "PBJ-VAL-080", "Jenis dokumen paket tidak dikenal atau sudah tidak aktif. Pilih dari daftar jenis dokumen paket.");
    }

    public async Task<IReadOnlySet<string>?> GetVisibleCategoriesAsync(
        ClaimsPrincipal user, string documentType, CancellationToken cancellationToken)
    {
        if (await _accessPermissionService.HasAccessAsync(user, ProcurementPermissions.PackageResource, ProcurementPermissions.ReadPriceAction))
            return null;

        var codes = await _dbContext.MstProcurementPackageDocumentTypes.AsNoTracking()
            .Where(x => !x.IsDelete && !x.IsPriceSensitive)
            .Select(x => x.DocumentTypeCode)
            .ToListAsync(cancellationToken);
        return new HashSet<string>(codes, StringComparer.Ordinal);
    }
}
