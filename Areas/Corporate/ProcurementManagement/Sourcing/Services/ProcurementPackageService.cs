using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Services;

/// <summary>
/// Paket pengadaan bagian draf (BE-PBJ-PS-005; PBJ-PS-API 1.0 §Procurement Package, PBJ-PS-STATE 1.0 bagian 1,
/// PBJ-PS-VAL 1.0 <c>PBJ-VAL-120</c>..<c>123</c>, <c>128</c>, <c>090</c>).
/// </summary>
/// <remarks>
/// <para><b>Isi paket selalu baris PR <c>APPROVED</c></b> (PBJ-DEC-071, 082). Baris berbarang katalog sama digabung menjadi
/// satu <see cref="PrcPackageItem"/>; setiap baris PR menjadi satu <see cref="PrcPackageItemSource"/> yang membawa unit dan
/// sumber dananya, sehingga komitmen pagu kelak dapat dicatat per garis pagu (PBJ-DEC-078). Contoh: PR IGD 10 kursi roda
/// (Rp 2.800.000) + PR Rawat Inap 5 kursi roda (Rp 3.100.000) → satu barang 15 unit, HPS satuan Rp 2.900.000, dua sumber.</para>
/// <para><b>Satu jenis pengadaan dan satu tahun anggaran per paket</b> (PBJ-DEC-090, 091): 422 <c>PBJ-VAL-122</c>/<c>123</c>.</para>
/// <para><b>Baris PR dipegang lewat <see cref="ProcurementPurchaseRequestService"/></b> (PBJ-PP-INT 1.2 §2, PBJ-DEC-095):
/// service ini tidak menulis kolom PR sendiri. Metode PR ikut transaksi yang dibuka di sini.</para>
/// <para><b>Cakupan unit</b> (PBJ-DEC-020): pemegang <c>ProcurementPackage : ReadAllUnits</c> melihat semua paket; pengguna
/// lain hanya paket yang memuat baris PR unit penempatannya, atau paket yang ia pegang sebagai PIC. Di luar cakupan → 404.</para>
/// <para><b>Harga</b> (PBJ-DEC-079): HPS, harga perkiraan, dan jumlahnya dikosongkan bagi pengguna tanpa
/// <c>ProcurementPackage : ReadPrice</c>, dan tidak pernah masuk payload logger (PBJ-PS-PERM bagian 6).</para>
/// </remarks>
public sealed class ProcurementPackageService
{
    private const string LogCategory = "Corporate.ProcurementManagement.Sourcing";
    private const string Resource = ProcurementPermissions.PackageResource;
    private const string NotFoundMessage = "Paket tidak ditemukan atau di luar cakupan Anda.";
    private const string LineNotFoundMessage = "Baris permintaan tidak ditemukan atau di luar cakupan Anda. Muat ulang daftar baris.";
    private const string StaleMessage = "Data sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda.";
    private const int SpecificationMaxLength = 2000;

    private static readonly IReadOnlyDictionary<string, string> StatusLabels = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [ProcurementStatuses.Package.Draft] = "Draf",
        [ProcurementStatuses.Package.PreparationPendingApproval] = "Menunggu Persetujuan Persiapan",
        [ProcurementStatuses.Package.PreparationReturned] = "Persiapan Dikembalikan",
        [ProcurementStatuses.Package.ReadyForRfq] = "Siap RFQ",
        [ProcurementStatuses.Package.RfqOpen] = "RFQ Berjalan",
        [ProcurementStatuses.Package.Evaluation] = "Evaluasi",
        [ProcurementStatuses.Package.AwardPendingApproval] = "Menunggu Persetujuan Penetapan",
        [ProcurementStatuses.Package.Awarded] = "Penyedia Ditetapkan",
        [ProcurementStatuses.Package.Ordered] = "Sudah Dipesan",
        [ProcurementStatuses.Package.Rejected] = "Ditolak",
        [ProcurementStatuses.Package.Cancelled] = "Dibatalkan"
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly ProcurementDataScopeService _scopeService;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly ProcurementDocumentHistoryWriter _historyWriter;
    private readonly ProcurementPurchaseRequestService _purchaseRequestService;
    private readonly LoggerService _loggerService;

    public ProcurementPackageService(
        ApplicationDbContext dbContext,
        ProcurementDataScopeService scopeService,
        AccessPermissionService accessPermissionService,
        ProcurementDocumentHistoryWriter historyWriter,
        ProcurementPurchaseRequestService purchaseRequestService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _scopeService = scopeService;
        _accessPermissionService = accessPermissionService;
        _historyWriter = historyWriter;
        _purchaseRequestService = purchaseRequestService;
        _loggerService = loggerService;
    }

    public static string LabelOf(string? status) =>
        status != null && StatusLabels.TryGetValue(status, out var label) ? label : status ?? string.Empty;

    // ------------------------------------------------------------------ baca

    public async Task<PackageFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken)
    {
        var years = await _dbContext.PrcPackages.AsNoTracking()
            .Where(x => !x.IsDelete).Select(x => x.FiscalYear).Distinct().ToListAsync(cancellationToken);
        years.Add(AppDateTimeHelper.OperationalDate().Year);

        return new PackageFilterMetadataResponse
        {
            StatusOptions = StatusLabels.Select(x => new PackageOptionValue { Value = x.Key, Label = x.Value }).ToList(),
            ProcurementTypeOptions = await _dbContext.MstProcurementTypes.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.TypeName)
                .Select(x => new PackageProcurementTypeOption { Id = x.Id, Code = x.TypeCode, Name = x.TypeName })
                .ToListAsync(cancellationToken),
            FiscalYearOptions = years.Distinct().OrderByDescending(x => x).ToList(),
            SortOptions =
            [
                new() { Value = "createdAt", Label = "Tanggal dibuat" },
                new() { Value = "packageNumber", Label = "Nomor paket" },
                new() { Value = "title", Label = "Judul" },
                new() { Value = "hpsTotal", Label = "Nilai HPS" }
            ]
        };
    }

    public async Task<PackageSummaryResponse> GetSummaryAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var byStatus = await query.GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new PackageSummaryResponse
        {
            Total = byStatus.Sum(x => x.Count),
            ByStatus = byStatus
                .Select(x => new PackageStatusCount { Status = x.Status, StatusLabel = LabelOf(x.Status), Count = x.Count })
                .OrderBy(x => x.Status, StringComparer.Ordinal)
                .ToList()
        };
    }

    public async Task<PagedResult<PackageListItem>> GetPagedAsync(
        ClaimsPrincipal user, PackageQuery request, CancellationToken cancellationToken)
    {
        var period = ProcurementPeriodFilter.Resolve(request.StartDate, request.EndDate, request.CustomPeriod);
        var canViewPrice = await CanViewPriceAsync(user);
        var query = (await ScopedAsync(user, cancellationToken)).WhereCreatedWithin(period);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status.Trim());
        if (request.ProcurementTypeId.HasValue) query = query.Where(x => x.ProcurementTypeId == request.ProcurementTypeId.Value);
        if (request.FiscalYear.HasValue) query = query.Where(x => x.FiscalYear == request.FiscalYear.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.Title.ToUpper().Contains(search)
                || (x.PackageNumber != null && x.PackageNumber.ToUpper().Contains(search)));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "packagenumber" => descending ? query.OrderByDescending(x => x.PackageNumber) : query.OrderBy(x => x.PackageNumber),
            "title" => descending ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
            // Urut nilai HPS membocorkan peringkat harga, sehingga hanya berlaku bagi pemegang ReadPrice.
            "hpstotal" when canViewPrice => descending ? query.OrderByDescending(x => x.HpsTotal) : query.OrderBy(x => x.HpsTotal),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new PackageListItem
            {
                Id = x.Id,
                PackageNumber = x.PackageNumber,
                Title = x.Title,
                ProcurementTypeId = x.ProcurementTypeId,
                ProcurementTypeName = x.ProcurementType!.TypeName,
                FiscalYear = x.FiscalYear,
                Status = x.Status,
                MethodName = x.MethodName,
                ItemCount = x.Items.Count(i => !i.IsDelete),
                SourceCount = x.Items.Where(i => !i.IsDelete).SelectMany(i => i.Sources).Count(s => !s.IsDelete && !s.IsReleased),
                HpsTotal = x.HpsTotal,
                PicUserId = x.PicUserId,
                PicName = _dbContext.Users.Where(u => u.Id == x.PicUserId).Select(u => u.DisplayName).FirstOrDefault(),
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime
            })
            .ToListAsync(cancellationToken);

        var packageIds = items.Select(x => x.Id).ToList();
        var departments = await ActiveSources()
            .Where(s => packageIds.Contains(s.PackageItem!.PackageId))
            .Select(s => new { s.PackageItem!.PackageId, s.PurchaseRequest!.Department!.DepartmentName })
            .Distinct()
            .ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            item.StatusLabel = LabelOf(item.Status);
            item.DepartmentNames = departments.Where(d => d.PackageId == item.Id)
                .Select(d => d.DepartmentName).OrderBy(d => d, StringComparer.CurrentCulture).ToList();
            if (!canViewPrice) item.HpsTotal = null;
        }

        return new PagedResult<PackageListItem>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = request.PageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    /// <summary>
    /// Baris PR siap paket dalam cakupan pengguna pada resource <c>ProcurementPackage</c> (PBJ-PP-INT 1.2 §2). Contoh: Tim
    /// Pengadaan (<c>ReadAllUnits</c>) melihat baris IGD dan Rawat Inap; staf IGD hanya baris IGD.
    /// </summary>
    public async Task<PagedResult<PackageableRequestLineResponse>> GetPackageableLinesAsync(
        ClaimsPrincipal user, PackageableRequestLineQuery request, CancellationToken cancellationToken)
    {
        var scope = await _scopeService.ResolveAsync(user, Resource, cancellationToken);
        return await _purchaseRequestService.GetPackageableLinesAsync(scope, request, await CanViewPriceAsync(user), cancellationToken);
    }

    public async Task<PackageResponse> GetByIdAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        if (!await (await ScopedAsync(user, cancellationToken)).AnyAsync(x => x.Id == id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
        return await BuildResponseAsync(id, await CanViewPriceAsync(user), cancellationToken);
    }

    /// <summary>
    /// Apakah paket berada dalam cakupan unit pengguna (BE-PBJ-PS-006). Tidak memeriksa izin aksi; dipakai penyedia akses
    /// lampiran bersama pemeriksaan <c>ProcurementPackage : Read</c>/<c>Update</c>.
    /// </summary>
    public async Task<bool> IsInScopeAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken) =>
        await (await ScopedAsync(user, cancellationToken)).AnyAsync(x => x.Id == id, cancellationToken);

    public async Task<List<DocumentHistoryResponse>> GetHistoryAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        if (!await (await ScopedAsync(user, cancellationToken)).AnyAsync(x => x.Id == id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
        return await _historyWriter.ReadAsync(ProcurementDocumentTypes.Package, id, cancellationToken);
    }

    // ------------------------------------------------------------------ tulis

    /// <summary>
    /// Buat paket <c>DRAFT</c> dari baris PR (PBJ-DEC-071). Pembuat menjadi PIC. Nomor <c>PKT-…</c> belum dialokasikan.
    /// </summary>
    public async Task<PackageResponse> CreateAsync(ClaimsPrincipal user, CreatePackageRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = ProcurementDataScopeService.GetUserId(user);
        var title = request.Title?.Trim() ?? string.Empty;
        if (title.Length == 0) throw new ProcurementSourcingBadRequestException("Judul paket wajib diisi.");
        var lineIds = DistinctIds(request.PurchaseRequestLineIds);

        var scope = await _scopeService.ResolveAsync(user, Resource, cancellationToken);
        var lines = await LoadCandidateLinesAsync(scope, lineIds, cancellationToken);
        await EnsureLinesReadyAsync(lines, cancellationToken);

        var procurementTypeId = lines[0].ProcurementTypeId;
        var fiscalYear = lines[0].FiscalYear;
        EnsureSameTypeAndYear(lines, procurementTypeId, fiscalYear);

        if (request.PackagePlanId.HasValue &&
            !await _dbContext.PrcPackagePlans.AsNoTracking().AnyAsync(x => x.Id == request.PackagePlanId.Value && !x.IsDelete, cancellationToken))
            throw new ProcurementSourcingBadRequestException("Rencana paket yang dirujuk tidak ditemukan.");

        var now = DateTimeOffset.UtcNow;
        var package = new PrcPackage
        {
            Title = title,
            ProcurementTypeId = procurementTypeId,
            FiscalYear = fiscalYear,
            PackagePlanId = request.PackagePlanId,
            Status = ProcurementStatuses.Package.Draft,
            PicUserId = actorUserId,
            RowVersion = Guid.NewGuid(),
            CreateBy = actorUserId,
            CreateDateTime = now.UtcDateTime
        };
        var items = new List<PrcPackageItem>();
        var (newItems, newSources) = AppendLines(package, items, lines, actorUserId, now);
        package.HpsTotal = items.Sum(x => x.HpsAmount);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _dbContext.PrcPackages.Add(package);
        _dbContext.PrcPackageItems.AddRange(newItems);
        _dbContext.PrcPackageItemSources.AddRange(newSources);
        await _purchaseRequestService.MarkLinesPackagedAsync(lineIds, package.Id, actorUserId, now, cancellationToken);
        _historyWriter.Record(
            ProcurementDocumentTypes.Package, package.Id, package.PackageNumber, ProcurementDocumentHistoryActions.Create,
            null, package.Status, null,
            new { PurchaseRequestLineIds = lineIds, ItemCount = items.Count, package.PackagePlanId }, actorUserId, now);
        await SaveAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await AuditAsync("ProcurementPackage.Create", package, actorUserId, lineIds.Count);
        return await BuildResponseAsync(package.Id, await CanViewPriceAsync(user), cancellationToken);
    }

    /// <summary>
    /// Tambah baris PR ke paket <c>DRAFT</c>/<c>PREPARATION_RETURNED</c>. Barang yang sudah ada bertambah kuantitasnya dan
    /// HPS satuannya menjadi rata-rata tertimbang HPS saat ini dengan harga perkiraan baris baru.
    /// </summary>
    public async Task<PackageResponse> AddLinesAsync(
        ClaimsPrincipal user, Guid id, AddPackageLinesRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = ProcurementDataScopeService.GetUserId(user);
        var lineIds = DistinctIds(request.PurchaseRequestLineIds);
        var scope = await _scopeService.ResolveAsync(user, Resource, cancellationToken);
        var package = await LoadEditableAsync(user, id, request.ExpectedRowVersion, cancellationToken);

        var lines = await LoadCandidateLinesAsync(scope, lineIds, cancellationToken);
        await EnsureLinesReadyAsync(lines, cancellationToken);
        EnsureSameTypeAndYear(lines, package.ProcurementTypeId, package.FiscalYear);

        var items = await _dbContext.PrcPackageItems
            .Where(x => x.PackageId == package.Id)
            .Include(x => x.Sources)
            .ToListAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var (newItems, newSources) = AppendLines(package, items, lines, actorUserId, now);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _dbContext.PrcPackageItems.AddRange(newItems);
        _dbContext.PrcPackageItemSources.AddRange(newSources);
        await _purchaseRequestService.MarkLinesPackagedAsync(lineIds, package.Id, actorUserId, now, cancellationToken);
        Touch(package, items, actorUserId, now);
        _historyWriter.Record(
            ProcurementDocumentTypes.Package, package.Id, package.PackageNumber, ProcurementDocumentHistoryActions.LineAdd,
            package.Status, package.Status, null, new { PurchaseRequestLineIds = lineIds }, actorUserId, now);
        await SaveAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await AuditAsync("ProcurementPackage.LineAdd", package, actorUserId, lineIds.Count);
        return await BuildResponseAsync(package.Id, await CanViewPriceAsync(user), cancellationToken);
    }

    /// <summary>
    /// Lepas satu baris PR dari paket <c>DRAFT</c>/<c>PREPARATION_RETURNED</c> (PBJ-DEC-077). Sumbernya ditandai
    /// <c>IsReleased</c>, baris PR kembali siap paket, dan PR yang tadinya <c>CONVERTED_TO_PACKAGE</c> kembali <c>APPROVED</c>.
    /// Barang tanpa sumber tersisa ikut dihapus lunak. HPS satuan barang yang masih bersumber tidak dihitung ulang.
    /// </summary>
    public async Task<PackageResponse> RemoveSourceAsync(
        ClaimsPrincipal user, Guid id, Guid sourceId, RemovePackageSourceQuery request, CancellationToken cancellationToken)
    {
        var actorUserId = ProcurementDataScopeService.GetUserId(user);
        var package = await LoadEditableAsync(user, id, request.ExpectedRowVersion, cancellationToken);
        var items = await _dbContext.PrcPackageItems
            .Where(x => x.PackageId == package.Id)
            .Include(x => x.Sources)
            .ToListAsync(cancellationToken);
        var item = items.FirstOrDefault(x => !x.IsDelete && x.Sources.Any(s => s.Id == sourceId && !s.IsDelete && !s.IsReleased))
            ?? throw new KeyNotFoundException("Baris paket tidak ditemukan. Muat ulang paket.");
        var source = item.Sources.First(s => s.Id == sourceId);
        var activeSourceCount = items.Where(x => !x.IsDelete).SelectMany(x => x.Sources).Count(s => !s.IsDelete && !s.IsReleased);
        if (activeSourceCount <= 1)
            throw new ProcurementSourcingBadRequestException(
                "Paket harus berisi sekurangnya satu baris permintaan. Batalkan paket bila seluruh barisnya tidak jadi diproses.");

        var now = DateTimeOffset.UtcNow;
        source.IsReleased = true;
        source.ReleasedAt = now;
        source.UpdateBy = actorUserId;
        source.UpdateDateTime = now.UtcDateTime;
        if (!item.Sources.Any(s => !s.IsDelete && !s.IsReleased))
        {
            // Kuantitas dan HPS barang terakhir dibiarkan apa adanya (CHECK Quantity > 0); barangnya dihapus lunak.
            item.IsDelete = true;
            item.DeleteBy = actorUserId;
            item.DeleteDateTime = now.UtcDateTime;
            var criteria = await _dbContext.PrcTechnicalCriteria
                .Where(x => x.PackageItemId == item.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            foreach (var criterion in criteria)
            {
                criterion.IsDelete = true;
                criterion.DeleteBy = actorUserId;
                criterion.DeleteDateTime = now.UtcDateTime;
            }
        }
        else
        {
            item.Quantity -= source.Quantity;
            item.HpsAmount = Math.Round(item.Quantity * item.HpsUnitPrice, 2, MidpointRounding.AwayFromZero);
        }
        item.UpdateBy = actorUserId;
        item.UpdateDateTime = now.UtcDateTime;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _purchaseRequestService.ReleaseLinesAsync(
            [source.PurchaseRequestLineId], package.Id, actorUserId, now, null, cancellationToken);
        Touch(package, items, actorUserId, now);
        _historyWriter.Record(
            ProcurementDocumentTypes.Package, package.Id, package.PackageNumber, ProcurementDocumentHistoryActions.LineRemove,
            package.Status, package.Status, null,
            new { SourceId = source.Id, source.PurchaseRequestLineId, source.PurchaseRequestId }, actorUserId, now);
        await SaveAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await AuditAsync("ProcurementPackage.LineRemove", package, actorUserId, 1);
        return await BuildResponseAsync(package.Id, await CanViewPriceAsync(user), cancellationToken);
    }

    // ------------------------------------------------------------------ bantuan

    private sealed record CandidateLine(
        Guid Id, int LineNo, Guid PurchaseRequestId, string? RequestNumber, string RequestStatus, Guid DepartmentId,
        Guid FundSourceId, int FiscalYear, Guid ItemId, Guid ProcurementTypeId, Guid ItemCategoryId, string? Specification,
        decimal PurchaseQuantity, decimal EstimatedUnitPrice, Guid? PackageId);

    private async Task<IQueryable<PrcPackage>> ScopedAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var scope = await _scopeService.ResolveAsync(user, Resource, cancellationToken);
        var query = _dbContext.PrcPackages.AsNoTracking().Where(x => !x.IsDelete);
        if (scope.IsAllUnits) return query;

        var departments = scope.DepartmentIds.ToList();
        var userId = scope.UserId;
        var visiblePackageIds = ActiveSources()
            .Where(s => departments.Contains(s.DepartmentId))
            .Select(s => s.PackageItem!.PackageId);
        return query.Where(x => (userId != Guid.Empty && x.PicUserId == userId) || visiblePackageIds.Contains(x.Id));
    }

    private IQueryable<PrcPackageItemSource> ActiveSources() =>
        _dbContext.PrcPackageItemSources.AsNoTracking()
            .Where(s => !s.IsDelete && !s.IsReleased && !s.PackageItem!.IsDelete);

    private Task<bool> CanViewPriceAsync(ClaimsPrincipal user) =>
        _accessPermissionService.HasAccessAsync(user, Resource, ProcurementPermissions.ReadPriceAction);

    private static List<Guid> DistinctIds(IEnumerable<Guid>? ids)
    {
        var result = (ids ?? []).Where(x => x != Guid.Empty).Distinct().ToList();
        if (result.Count == 0) throw new ProcurementSourcingBadRequestException("Pilih sekurangnya satu baris permintaan.");
        return result;
    }

    /// <summary>Baris dalam cakupan pengguna; baris di luar cakupan dijawab sama dengan baris yang tidak ada (404).</summary>
    private async Task<List<CandidateLine>> LoadCandidateLinesAsync(
        ProcurementDataScope scope, IReadOnlyCollection<Guid> lineIds, CancellationToken cancellationToken)
    {
        var departments = scope.DepartmentIds.ToList();
        var allUnits = scope.IsAllUnits;
        var lines = await _dbContext.PrcPurchaseRequestLines.AsNoTracking()
            .Where(x => lineIds.Contains(x.Id) && !x.IsDelete && !x.PurchaseRequest!.IsDelete
                && (allUnits || departments.Contains(x.PurchaseRequest.DepartmentId)))
            .Select(x => new CandidateLine(
                x.Id, x.LineNo, x.PurchaseRequestId, x.PurchaseRequest!.RequestNumber, x.PurchaseRequest.Status,
                x.PurchaseRequest.DepartmentId, x.PurchaseRequest.FundSourceId, x.PurchaseRequest.FiscalYear, x.ItemId,
                x.Item!.ProcurementTypeId, x.Item.ItemCategoryId, x.Specification, x.PurchaseQuantity, x.EstimatedUnitPrice,
                x.PackageId))
            .ToListAsync(cancellationToken);
        if (lines.Count != lineIds.Count) throw new KeyNotFoundException(LineNotFoundMessage);

        // Urutan barang paket mengikuti urutan pilihan pengguna.
        var order = lineIds.Select((lineId, index) => (lineId, index)).ToDictionary(x => x.lineId, x => x.index);
        return lines.OrderBy(x => order[x.Id]).ToList();
    }

    private async Task EnsureLinesReadyAsync(IReadOnlyCollection<CandidateLine> lines, CancellationToken cancellationToken)
    {
        var notReady = lines.FirstOrDefault(x => x.PackageId == null &&
            (x.RequestStatus != ProcurementStatuses.PurchaseRequest.Approved || x.PurchaseQuantity <= 0));
        if (notReady != null)
            throw new ProcurementSourcingConflictException("PBJ-VAL-120",
                $"Baris {notReady.LineNo} permintaan {notReady.RequestNumber} belum disetujui atau tidak perlu dibeli.");

        var packaged = lines.FirstOrDefault(x => x.PackageId.HasValue);
        if (packaged != null)
        {
            var owner = await _dbContext.PrcPackages.AsNoTracking()
                .Where(x => x.Id == packaged.PackageId!.Value)
                .Select(x => x.PackageNumber ?? x.Title)
                .FirstOrDefaultAsync(cancellationToken);
            throw new ProcurementSourcingConflictException("PBJ-VAL-121",
                $"Baris {packaged.LineNo} permintaan {packaged.RequestNumber} sudah masuk paket {owner ?? "lain"}.");
        }
    }

    private static void EnsureSameTypeAndYear(IReadOnlyCollection<CandidateLine> lines, Guid procurementTypeId, int fiscalYear)
    {
        if (lines.Any(x => x.ProcurementTypeId != procurementTypeId))
            throw new ProcurementSourcingValidationException("PBJ-VAL-122", "Satu paket hanya untuk satu jenis pengadaan.");
        if (lines.Any(x => x.FiscalYear != fiscalYear))
            throw new ProcurementSourcingValidationException("PBJ-VAL-123", "Satu paket hanya untuk satu tahun anggaran.");
    }

    /// <summary>
    /// Masukkan baris ke barang paket. Barang katalog yang sudah ada (belum terhapus) bertambah; selain itu barang baru
    /// dibuat dengan nomor urut berikutnya. Mengembalikan barang dan sumber yang baru, untuk didaftarkan eksplisit ke
    /// <c>DbContext</c> (sumber baru pada barang yang sudah dilacak tidak boleh bergantung pada penemuan navigasi, karena
    /// kuncinya sudah terisi); semua barang hasil akhir ada di <paramref name="items"/>.
    /// </summary>
    private static (List<PrcPackageItem> Items, List<PrcPackageItemSource> Sources) AppendLines(
        PrcPackage package, List<PrcPackageItem> items, IReadOnlyCollection<CandidateLine> lines, Guid actorUserId, DateTimeOffset now)
    {
        var created = new List<PrcPackageItem>();
        var createdSources = new List<PrcPackageItemSource>();
        var nextLineNo = items.Count == 0 ? 1 : items.Max(x => x.LineNo) + 1;
        foreach (var group in lines.GroupBy(x => x.ItemId))
        {
            var item = items.FirstOrDefault(x => !x.IsDelete && x.ItemId == group.Key);
            if (item == null)
            {
                item = new PrcPackageItem
                {
                    PackageId = package.Id,
                    LineNo = nextLineNo++,
                    ItemId = group.Key,
                    ItemCategoryId = group.First().ItemCategoryId,
                    CreateBy = actorUserId,
                    CreateDateTime = now.UtcDateTime
                };
                items.Add(item);
                created.Add(item);
            }
            else
            {
                item.UpdateBy = actorUserId;
                item.UpdateDateTime = now.UtcDateTime;
            }

            // HPS satuan = rata-rata tertimbang (kuantitas) HPS saat ini dan harga perkiraan PR, termasuk PPN (PBJ-DEC-084).
            var addedQuantity = group.Sum(x => x.PurchaseQuantity);
            var weighted = item.Quantity * item.HpsUnitPrice + group.Sum(x => x.PurchaseQuantity * x.EstimatedUnitPrice);
            item.Quantity += addedQuantity;
            item.HpsUnitPrice = item.Quantity <= 0 ? 0 : Math.Round(weighted / item.Quantity, 2, MidpointRounding.AwayFromZero);
            item.HpsAmount = Math.Round(item.Quantity * item.HpsUnitPrice, 2, MidpointRounding.AwayFromZero);
            item.Specification = MergeSpecification(item.Specification, group.Select(x => x.Specification));

            foreach (var line in group)
            {
                var source = new PrcPackageItemSource
                {
                    PackageItemId = item.Id,
                    PurchaseRequestLineId = line.Id,
                    PurchaseRequestId = line.PurchaseRequestId,
                    DepartmentId = line.DepartmentId,
                    FundSourceId = line.FundSourceId,
                    Quantity = line.PurchaseQuantity,
                    CreateBy = actorUserId,
                    CreateDateTime = now.UtcDateTime
                };
                item.Sources.Add(source);
                createdSources.Add(source);
            }
        }
        return (created, createdSources);
    }

    /// <summary>Gabungan spesifikasi berbeda dari baris PR, dipisah "; ", dipotong ke batas kolom.</summary>
    private static string? MergeSpecification(string? current, IEnumerable<string?> additions)
    {
        var parts = new List<string>();
        foreach (var text in new[] { current }.Concat(additions))
        {
            foreach (var part in (text ?? string.Empty).Split("; ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!parts.Contains(part, StringComparer.OrdinalIgnoreCase)) parts.Add(part);
            }
        }
        if (parts.Count == 0) return null;
        var merged = string.Join("; ", parts);
        return merged.Length <= SpecificationMaxLength ? merged : merged[..SpecificationMaxLength];
    }

    private async Task<PrcPackage> LoadEditableAsync(
        ClaimsPrincipal user, Guid id, Guid expectedRowVersion, CancellationToken cancellationToken)
    {
        if (!await (await ScopedAsync(user, cancellationToken)).AnyAsync(x => x.Id == id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
        var package = await _dbContext.PrcPackages.FirstAsync(x => x.Id == id, cancellationToken);
        if (!ProcurementStatuses.Package.IsPreparationEditable(package.Status))
            throw new ProcurementSourcingConflictException("PBJ-VAL-128",
                $"Tindakan ini tidak dapat dilakukan pada status {LabelOf(package.Status)}.");
        if (package.RowVersion != expectedRowVersion)
            throw new ProcurementSourcingConflictException("PBJ-VAL-090", StaleMessage);
        return package;
    }

    private static void Touch(PrcPackage package, IEnumerable<PrcPackageItem> items, Guid actorUserId, DateTimeOffset now)
    {
        package.HpsTotal = items.Where(x => !x.IsDelete).Sum(x => x.HpsAmount);
        package.RowVersion = Guid.NewGuid();
        package.UpdateBy = actorUserId;
        package.UpdateDateTime = now.UtcDateTime;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementSourcingConflictException("PBJ-VAL-090", StaleMessage);
        }
        catch (DbUpdateException exception) when ((exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // Dua orang memaketkan baris yang sama bersamaan; index unik parsial sumber paket menahan yang kedua.
            throw new ProcurementSourcingConflictException("PBJ-VAL-121",
                "Baris permintaan ini baru saja dimasukkan ke paket lain. Muat ulang daftar baris.");
        }
    }

    private async Task<PackageResponse> BuildResponseAsync(Guid id, bool canViewPrice, CancellationToken cancellationToken)
    {
        var response = await _dbContext.PrcPackages.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PackageResponse
            {
                Id = x.Id,
                PackageNumber = x.PackageNumber,
                Title = x.Title,
                ProcurementTypeId = x.ProcurementTypeId,
                ProcurementTypeCode = x.ProcurementType!.TypeCode,
                ProcurementTypeName = x.ProcurementType.TypeName,
                FiscalYear = x.FiscalYear,
                PackagePlanId = x.PackagePlanId,
                PackagePlanNumber = x.PackagePlan != null ? x.PackagePlan.PackagePlanNumber : null,
                PackagePlanTitle = x.PackagePlan != null ? x.PackagePlan.Title : null,
                SplitFromPackageId = x.SplitFromPackageId,
                Status = x.Status,
                MethodId = x.MethodId,
                MethodCode = x.MethodCode,
                MethodName = x.MethodName,
                MethodMinHpsAmount = x.MethodMinHpsAmount,
                MethodMaxHpsAmount = x.MethodMaxHpsAmount,
                MethodMinimumQuotationCount = x.MethodMinimumQuotationCount,
                MethodRequiresTechnicalEvaluation = x.MethodRequiresTechnicalEvaluation,
                IsMethodOutOfRange = x.IsMethodOutOfRange,
                MethodDeviationReason = x.MethodDeviationReason,
                HpsTotal = x.HpsTotal,
                ApprovedHpsTotal = x.ApprovedHpsTotal,
                PicUserId = x.PicUserId,
                PicName = _dbContext.Users.Where(u => u.Id == x.PicUserId).Select(u => u.DisplayName).FirstOrDefault(),
                PreparationSubmissionCount = x.PreparationSubmissionCount,
                PreparationWorkflowInstanceId = x.PreparationWorkflowInstanceId,
                PreparationSubmittedAt = x.PreparationSubmittedAt,
                PreparationApprovedAt = x.PreparationApprovedAt,
                PurchaseOrderNumber = x.PurchaseOrderNumber,
                PurchaseOrderRecordedAt = x.PurchaseOrderRecordedAt,
                CancelReason = x.CancelReason,
                RowVersion = x.RowVersion,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);

        response.Items = await _dbContext.PrcPackageItems.AsNoTracking()
            .Where(i => i.PackageId == id && !i.IsDelete)
            .OrderBy(i => i.LineNo)
            .Select(i => new PackageItemResponse
            {
                Id = i.Id,
                LineNo = i.LineNo,
                ItemId = i.ItemId,
                ItemCode = i.Item!.ItemCode,
                ItemName = i.Item.ItemName,
                ItemCategoryId = i.ItemCategoryId,
                ItemCategoryName = _dbContext.MstProcurementItemCategories
                    .Where(c => c.Id == i.ItemCategoryId).Select(c => c.CategoryName).FirstOrDefault() ?? string.Empty,
                MeasurementName = i.Item.Measurement!.MeasurementName,
                Specification = i.Specification,
                Quantity = i.Quantity,
                HpsUnitPrice = i.HpsUnitPrice,
                HpsAmount = i.HpsAmount,
                Criteria = i.Criteria
                    .Where(c => !c.IsDelete)
                    .OrderBy(c => c.CriterionNo)
                    .Select(c => new PackageCriterionResponse { Id = c.Id, CriterionNo = c.CriterionNo, CriterionText = c.CriterionText })
                    .ToList(),
                Sources = i.Sources
                    .Where(s => !s.IsDelete && !s.IsReleased)
                    .OrderBy(s => s.PurchaseRequest!.RequestNumber).ThenBy(s => s.PurchaseRequestLine!.LineNo)
                    .Select(s => new PackageItemSourceResponse
                    {
                        Id = s.Id,
                        PurchaseRequestLineId = s.PurchaseRequestLineId,
                        PurchaseRequestLineNo = s.PurchaseRequestLine!.LineNo,
                        PurchaseRequestId = s.PurchaseRequestId,
                        RequestNumber = s.PurchaseRequest!.RequestNumber,
                        DepartmentId = s.DepartmentId,
                        DepartmentName = s.PurchaseRequest.Department!.DepartmentName,
                        FundSourceId = s.FundSourceId,
                        FundSourceName = s.PurchaseRequest.FundSource!.FundSourceName,
                        Quantity = s.Quantity,
                        EstimatedUnitPrice = s.PurchaseRequestLine.EstimatedUnitPrice,
                        NeedPlanLineKey = s.PurchaseRequestLine.NeedPlanLineKey
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        // Checklist (BE-PBJ-PS-007): terpenuhi bila ada lampiran paket berlaku berkategori kode jenis dokumennya.
        var attachedCodes = await _dbContext.PrcAttachments.AsNoTracking()
            .Where(a => a.DocumentType == ProcurementDocumentTypes.Package && a.DocumentId == id && a.IsCurrent && !a.IsCancel
                && !a.IsDelete && a.AttachmentCategory != null)
            .Select(a => a.AttachmentCategory!)
            .Distinct()
            .ToListAsync(cancellationToken);
        response.Checklist = await _dbContext.PrcPackageChecklistItems.AsNoTracking()
            .Where(c => c.PackageId == id && !c.IsDelete)
            .OrderBy(c => c.DocumentTypeCode)
            .Select(c => new PackageChecklistItemResponse
            {
                Id = c.Id,
                DocumentTypeId = c.DocumentTypeId,
                DocumentTypeCode = c.DocumentTypeCode,
                DocumentTypeName = c.DocumentTypeName,
                IsRequired = c.IsRequired,
                IsPriceSensitive = c.DocumentType!.IsPriceSensitive
            })
            .ToListAsync(cancellationToken);
        foreach (var row in response.Checklist)
            row.IsFulfilled = attachedCodes.Contains(row.DocumentTypeCode);

        response.Evaluators = await _dbContext.PrcPackageEvaluators.AsNoTracking()
            .Where(e => e.PackageId == id && e.IsActive && !e.IsDelete)
            .OrderBy(e => e.CreateDateTime)
            .Select(e => new PackageEvaluatorResponse
            {
                Id = e.Id,
                EvaluatorUserId = e.EvaluatorUserId,
                EvaluatorName = _dbContext.Users.Where(u => u.Id == e.EvaluatorUserId).Select(u => u.DisplayName).FirstOrDefault(),
                AssignedByUserId = e.AssignedByUserId,
                AssignedByName = _dbContext.Users.Where(u => u.Id == e.AssignedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                AssignedAt = e.CreateDateTime
            })
            .ToListAsync(cancellationToken);

        response.StatusLabel = LabelOf(response.Status);
        response.CanViewPrice = canViewPrice;
        if (!canViewPrice) MaskPrices(response);
        return response;
    }

    /// <summary>Satu-satunya titik penutup harga pada respons paket (PBJ-DEC-079).</summary>
    private static void MaskPrices(PackageResponse response)
    {
        response.HpsTotal = null;
        response.ApprovedHpsTotal = null;
        foreach (var item in response.Items)
        {
            item.HpsUnitPrice = null;
            item.HpsAmount = null;
            foreach (var source in item.Sources) source.EstimatedUnitPrice = null;
        }

        // Keberadaan lampiran harga tidak dibocorkan lewat status checklist (BE-PBJ-PS-006, 007).
        foreach (var row in response.Checklist.Where(x => x.IsPriceSensitive)) row.IsFulfilled = null;
    }

    /// <summary>Jejak audit tanpa field harga (PBJ-PS-PERM bagian 6).</summary>
    private Task AuditAsync(string action, PrcPackage package, Guid actorUserId, int lineCount) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan paket pengadaan.", new
        {
            EntityId = package.Id,
            package.Status,
            LineCount = lineCount,
            ActorUserId = actorUserId
        });
}
