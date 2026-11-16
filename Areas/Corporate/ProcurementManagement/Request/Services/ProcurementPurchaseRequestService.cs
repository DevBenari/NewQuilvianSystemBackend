using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Services;

/// <summary>
/// Permintaan pengadaan (PR) — draf, daftar, ringkasan, detail, riwayat (BE-PBJ-006;
/// <c>state-transition-matrix.md</c> bagian 1, <c>validation-matrix.md</c> bagian 1).
/// </summary>
/// <remarks>
/// <para><b>Cakupan unit (PBJ-DEC-020/036).</b> Setiap query disaring
/// <see cref="ProcurementDataScopeService"/> dengan resource <c>ProcurementRequest</c>. PR di luar
/// cakupan dijawab 404, bukan 403, agar keberadaannya tidak bocor. Contoh: pengguna IGD membuka
/// tautan PR Radiologi → "Pengajuan tidak ditemukan atau bukan milik unit Anda."</para>
/// <para><b>Membuat PR</b> hanya untuk unit penempatan pengusul sendiri (PBJ-VAL-010), walaupun
/// pengguna boleh melihat semua unit.</para>
/// <para><b>Mengubah draf</b> hanya oleh pembuatnya, saat <c>DRAFT</c>/<c>RETURNED</c>, dengan
/// <c>ExpectedRowVersion</c> yang cocok. Baris lama ditandai terhapus (tidak dihapus fisik) dan
/// baris baru ditulis, sehingga riwayat isi tetap dapat ditelusuri.</para>
/// <para><b>Ajukan (BE-PBJ-007).</b> Nomor <c>PR-…</c> dialokasikan saat pertama diajukan. Kuantitas tiap
/// baris dibagi dalam/luar rencana terhadap sisa baris rencana berlaku, di bawah kunci per
/// <c>LineKey</c> (PBJ-DEC-029). Contoh: sisa rencana 4, diminta 6 → dalam rencana 4, luar rencana 2,
/// dan baris itu wajib beralasan (PBJ-VAL-009).</para>
/// <para><b>Batal dan tarik (BE-PBJ-007).</b> PR <c>DRAFT</c>/<c>SUBMITTED</c>/<c>RETURNED</c> dibatalkan
/// langsung; PR <c>PENDING_APPROVAL</c> ditarik lewat Wfl lebih dulu. Verifikasi ada di
/// <c>ProcurementPurchaseRequestVerificationService</c> (BE-PBJ-008).</para>
/// </remarks>
public sealed class ProcurementPurchaseRequestService
{
    private const string LogCategory = "Corporate.ProcurementManagement.Request";
    private const string NotFoundMessage = "Pengajuan tidak ditemukan atau bukan milik unit Anda.";

    private static readonly IReadOnlyDictionary<string, string> StatusLabels = new Dictionary<string, string>
    {
        [ProcurementStatuses.PurchaseRequest.Draft] = "Draf",
        [ProcurementStatuses.PurchaseRequest.Submitted] = "Menunggu Verifikasi",
        [ProcurementStatuses.PurchaseRequest.Returned] = "Dikembalikan",
        [ProcurementStatuses.PurchaseRequest.PendingApproval] = "Menunggu Persetujuan",
        [ProcurementStatuses.PurchaseRequest.Approved] = "Disetujui",
        [ProcurementStatuses.PurchaseRequest.FulfilledFromStock] = "Dipenuhi dari Stok",
        [ProcurementStatuses.PurchaseRequest.Rejected] = "Ditolak",
        [ProcurementStatuses.PurchaseRequest.Cancelled] = "Dibatalkan",
        [ProcurementStatuses.PurchaseRequest.ConvertedToPackage] = "Masuk Paket"
    };

    private const string InvalidTransitionMessage = "Status pengajuan tidak memungkinkan tindakan ini.";
    private const string StaleMessage = "Data sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda.";

    private readonly ApplicationDbContext _dbContext;
    private readonly ProcurementDataScopeService _scopeService;
    private readonly ProcurementDocumentHistoryWriter _historyWriter;
    private readonly ProcurementPlanQuotaService _quotaService;
    private readonly ProcurementApprovalGateway _approvalGateway;
    private readonly NumberSeriesAllocator _numberAllocator;
    private readonly LoggerService _loggerService;

    public ProcurementPurchaseRequestService(
        ApplicationDbContext dbContext,
        ProcurementDataScopeService scopeService,
        ProcurementDocumentHistoryWriter historyWriter,
        ProcurementPlanQuotaService quotaService,
        ProcurementApprovalGateway approvalGateway,
        NumberSeriesAllocator numberAllocator,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _scopeService = scopeService;
        _historyWriter = historyWriter;
        _quotaService = quotaService;
        _approvalGateway = approvalGateway;
        _numberAllocator = numberAllocator;
        _loggerService = loggerService;
    }

    public static bool IsEditableStatus(string status) =>
        status == ProcurementStatuses.PurchaseRequest.Draft ||
        status == ProcurementStatuses.PurchaseRequest.Returned;

    // ------------------------------------------------------------------ baca

    public PurchaseRequestFilterMetadataResponse GetFilterMetadata() => new()
    {
        DefaultFilter = new PurchaseRequestQuery(),
        StatusOptions = StatusLabels.Select(x => new PurchaseRequestOptionValue { Value = x.Key, Label = x.Value }).ToList(),
        UrgencyOptions =
        [
            new() { Value = ProcurementUrgencyLevels.Normal, Label = "Biasa" },
            new() { Value = ProcurementUrgencyLevels.Urgent, Label = "Mendesak" }
        ],
        SortOptions =
        [
            new() { Value = "createdAt", Label = "Tanggal dibuat" },
            new() { Value = "requestNumber", Label = "Nomor PR" },
            new() { Value = "neededByDate", Label = "Tanggal dibutuhkan" },
            new() { Value = "purchaseTotal", Label = "Nilai beli" }
        ]
    };

    public async Task<PurchaseRequestSummaryResponse> GetSummaryAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var byStatus = await query
            .GroupBy(x => x.Status)
            .Select(g => new PurchaseRequestStatusCount { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new PurchaseRequestSummaryResponse
        {
            Total = byStatus.Sum(x => x.Count),
            Urgent = await query.CountAsync(x => x.UrgencyLevel == ProcurementUrgencyLevels.Urgent, cancellationToken),
            OutOfPlan = await query.CountAsync(x => x.IsOutOfPlan, cancellationToken),
            OverBudget = await query.CountAsync(x => x.IsOverBudget, cancellationToken),
            ByStatus = byStatus.OrderBy(x => x.Status).ToList()
        };
    }

    public async Task<PagedResult<PurchaseRequestListItem>> GetPagedAsync(
        ClaimsPrincipal user, PurchaseRequestQuery request, CancellationToken cancellationToken)
    {
        if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate)
            throw new ProcurementRequestValidationException("PBJ-VAL-007", "Tanggal akhir tidak boleh sebelum tanggal awal.");

        var query = await ScopedAsync(user, cancellationToken);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status.Trim());
        if (request.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
        if (request.FiscalYear.HasValue) query = query.Where(x => x.FiscalYear == request.FiscalYear.Value);
        if (!string.IsNullOrWhiteSpace(request.UrgencyLevel)) query = query.Where(x => x.UrgencyLevel == request.UrgencyLevel.Trim());
        if (request.IsOutOfPlan.HasValue) query = query.Where(x => x.IsOutOfPlan == request.IsOutOfPlan.Value);
        if (request.IsOverBudget.HasValue) query = query.Where(x => x.IsOverBudget == request.IsOverBudget.Value);
        if (request.MineOnly == true)
        {
            var userId = ProcurementDataScopeService.GetUserId(user);
            query = query.Where(x => x.RequesterUserId == userId);
        }
        if (request.StartDate.HasValue)
        {
            var start = request.StartDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(x => x.CreateDateTime >= start);
        }
        if (request.EndDate.HasValue)
        {
            // Tanggal akhir inklusif: sampai sebelum awal hari berikutnya.
            var endExclusive = request.EndDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(x => x.CreateDateTime < endExclusive);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.RequestNumber != null && x.RequestNumber.ToUpper().Contains(search));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "requestnumber" => descending ? query.OrderByDescending(x => x.RequestNumber) : query.OrderBy(x => x.RequestNumber),
            "neededbydate" => descending ? query.OrderByDescending(x => x.NeededByDate) : query.OrderBy(x => x.NeededByDate),
            "purchasetotal" => descending ? query.OrderByDescending(x => x.PurchaseTotal) : query.OrderBy(x => x.PurchaseTotal),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new PurchaseRequestListItem
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department!.DepartmentName,
                RequesterUserId = x.RequesterUserId,
                RequesterName = _dbContext.Users.Where(u => u.Id == x.RequesterUserId).Select(u => u.DisplayName).FirstOrDefault(),
                FiscalYear = x.FiscalYear,
                NeededByDate = x.NeededByDate,
                UrgencyLevel = x.UrgencyLevel,
                Status = x.Status,
                IsOutOfPlan = x.IsOutOfPlan,
                IsOverBudget = x.IsOverBudget,
                RequestedTotal = x.RequestedTotal,
                PurchaseTotal = x.PurchaseTotal,
                LineCount = x.Lines.Count(l => !l.IsDelete),
                SubmittedAt = x.SubmittedAt,
                CreateDateTime = x.CreateDateTime
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PurchaseRequestListItem>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = request.PageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    public async Task<PurchaseRequestResponse> GetByIdAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var response = await Project(query.Where(x => x.Id == id)).FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);

        response.Lines = await _dbContext.PrcPurchaseRequestLines.AsNoTracking()
            .Where(x => x.PurchaseRequestId == id && !x.IsDelete)
            .OrderBy(x => x.LineNo)
            .Select(x => new PurchaseRequestLineResponse
            {
                Id = x.Id,
                LineNo = x.LineNo,
                ItemId = x.ItemId,
                ItemCode = x.Item!.ItemCode,
                ItemName = x.Item.ItemName,
                ProcurementTypeCode = x.Item.ProcurementType!.TypeCode,
                ItemCategoryId = x.Item.ItemCategoryId,
                ItemCategoryName = x.Item.ItemCategory!.CategoryName,
                MeasurementName = x.Item.Measurement!.MeasurementName,
                Specification = x.Specification,
                RequestedQuantity = x.RequestedQuantity,
                EstimatedUnitPrice = x.EstimatedUnitPrice,
                RequestedAmount = Math.Round(x.RequestedQuantity * x.EstimatedUnitPrice, 2),
                NeedPlanLineKey = x.NeedPlanLineKey,
                InPlanQuantity = x.InPlanQuantity,
                OutOfPlanQuantity = x.OutOfPlanQuantity,
                OutOfPlanReason = x.OutOfPlanReason,
                StockAvailableQuantity = x.StockAvailableQuantity,
                StockFulfilledQuantity = x.StockFulfilledQuantity,
                StockSourceNote = x.StockSourceNote,
                StockCheckedAt = x.StockCheckedAt,
                PurchaseQuantity = x.PurchaseQuantity,
                PurchaseAmount = x.PurchaseAmount
            })
            .ToListAsync(cancellationToken);

        ApplyFlags(response, ProcurementDataScopeService.GetUserId(user));
        return response;
    }

    /// <summary>Kelayakan aksi untuk menyembunyikan tombol; izin dan kelayakan tetap diperiksa ulang tiap endpoint.</summary>
    internal static void ApplyFlags(PurchaseRequestResponse response, Guid userId)
    {
        var isRequester = response.RequesterUserId == userId;
        var status = response.Status;
        response.CanEdit = IsEditableStatus(status) && isRequester;
        response.CanSubmit = response.CanEdit;
        response.CanCancel = isRequester &&
            (status == ProcurementStatuses.PurchaseRequest.Draft ||
             status == ProcurementStatuses.PurchaseRequest.Submitted ||
             status == ProcurementStatuses.PurchaseRequest.Returned);
        response.CanWithdraw = isRequester && status == ProcurementStatuses.PurchaseRequest.PendingApproval;
        response.CanVerify = !isRequester && status == ProcurementStatuses.PurchaseRequest.Submitted;
        response.CanCancelApproved = status == ProcurementStatuses.PurchaseRequest.Approved;
    }

    /// <summary>
    /// Baris rencana berlaku untuk unit × tahun (opsional satu barang) beserta sisanya, untuk dipilih
    /// pada baris PR. Unit di luar cakupan pengguna dijawab 404.
    /// </summary>
    public async Task<List<PlanLineQuotaResponse>> GetPlanLineOptionsAsync(
        ClaimsPrincipal user, PlanLineOptionsQuery request, CancellationToken cancellationToken)
    {
        var scope = await _scopeService.ResolveAsync(user, ProcurementPermissions.PurchaseRequestResource, cancellationToken);
        if (!scope.CanSee(request.DepartmentId))
            throw new KeyNotFoundException("Unit tidak ditemukan atau di luar cakupan Anda.");

        var options = await _quotaService.GetOptionsAsync(request.DepartmentId, request.FiscalYear, request.ItemId, cancellationToken);
        return options.Select(x => new PlanLineQuotaResponse
        {
            LineKey = x.Line.LineKey,
            NeedPlanId = x.Line.NeedPlanId,
            PlanNumber = x.Line.PlanNumber,
            VersionNo = x.Line.VersionNo,
            DepartmentId = x.Line.DepartmentId,
            DepartmentName = x.Line.DepartmentName,
            FiscalYear = x.Line.FiscalYear,
            ItemId = x.Line.ItemId,
            ItemCode = x.Line.ItemCode,
            ItemName = x.Line.ItemName,
            MeasurementName = x.Line.MeasurementName,
            PlannedQuantity = x.Line.Quantity,
            UsedQuantity = x.Usage.UsedByPurchaseRequest,
            RemainingQuantity = Math.Max(0, x.Line.Quantity - x.Usage.UsedByPurchaseRequest),
            EstimatedUnitPrice = x.Line.EstimatedUnitPrice
        }).ToList();
    }

    public async Task<List<DocumentHistoryResponse>> GetStatusHistoryAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        if (!await query.AnyAsync(x => x.Id == id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
        return await _historyWriter.ReadAsync(ProcurementDocumentTypes.PurchaseRequest, id, cancellationToken);
    }

    /// <summary>
    /// Hak atas PR untuk lampiran: tidak terlihat → None; pembuat saat DRAFT/RETURNED → Edit;
    /// selain itu → Read.
    /// </summary>
    public async Task<ProcurementDocumentAccessLevel> GetAccessLevelAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var row = await query.Where(x => x.Id == id)
            .Select(x => new { x.Status, x.RequesterUserId })
            .FirstOrDefaultAsync(cancellationToken);
        if (row == null) return ProcurementDocumentAccessLevel.None;
        return IsEditableStatus(row.Status) && row.RequesterUserId == ProcurementDataScopeService.GetUserId(user)
            ? ProcurementDocumentAccessLevel.Edit
            : ProcurementDocumentAccessLevel.Read;
    }

    // ------------------------------------------------------------------ tulis

    public async Task<PurchaseRequestResponse> CreateAsync(ClaimsPrincipal user, CreatePurchaseRequestRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await RequirePlacementAsync(actorUserId, request.DepartmentId, cancellationToken);
        var header = await ValidateHeaderAsync(request, cancellationToken);
        var lines = await ValidateLinesAsync(request.Lines, cancellationToken);

        var now = DateTime.UtcNow;
        var entity = new PrcPurchaseRequest
        {
            DepartmentId = request.DepartmentId,
            RequesterUserId = actorUserId,
            Status = ProcurementStatuses.PurchaseRequest.Draft,
            CreateDateTime = now,
            CreateBy = actorUserId
        };
        ApplyHeader(entity, request, header);
        AddLines(entity, lines, actorUserId, now);

        _dbContext.PrcPurchaseRequests.Add(entity);
        _historyWriter.Record(
            ProcurementDocumentTypes.PurchaseRequest, entity.Id, null, ProcurementDocumentHistoryActions.Create,
            null, entity.Status, null, new { entity.RequestedTotal, LineCount = lines.Count },
            actorUserId, DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("PurchaseRequest.Create", entity, actorUserId);
        return await GetByIdAsync(user, entity.Id, cancellationToken);
    }

    public async Task<PurchaseRequestResponse> UpdateAsync(ClaimsPrincipal user, Guid id, UpdatePurchaseRequestRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        var query = await ScopedAsync(user, cancellationToken);
        if (!await query.AnyAsync(x => x.Id == id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);

        var entity = await _dbContext.PrcPurchaseRequests
            .Include(x => x.Lines.Where(l => !l.IsDelete))
            .FirstAsync(x => x.Id == id, cancellationToken);

        if (!IsEditableStatus(entity.Status))
            throw new ProcurementRequestConflictException("PBJ-VAL-093", "Status pengajuan tidak memungkinkan tindakan ini.");
        if (entity.RequesterUserId != actorUserId)
            throw new ProcurementRequestForbiddenException("Hanya pembuat pengajuan yang dapat mengubahnya.");
        if (entity.RowVersion != request.ExpectedRowVersion)
            throw new ProcurementRequestConflictException("PBJ-VAL-090", "Data sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda.");
        if (request.DepartmentId != entity.DepartmentId)
            await RequirePlacementAsync(actorUserId, request.DepartmentId, cancellationToken);

        var header = await ValidateHeaderAsync(request, cancellationToken);
        var lines = await ValidateLinesAsync(request.Lines, cancellationToken);

        var now = DateTime.UtcNow;

        // Dua tahap dalam satu transaksi: baris lama ditandai terhapus dan versi kepala diganti lebih
        // dulu, baru baris pengganti ditulis. Urutan ini menjaga indeks unik (PR, nomor baris) yang
        // hanya berlaku untuk baris hidup, dan RowVersion pada tahap pertama menolak simpanan basi.
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var old in entity.Lines.ToList())
            {
                old.IsDelete = true;
                old.DeleteDateTime = now;
                old.DeleteBy = actorUserId;
            }

            entity.DepartmentId = request.DepartmentId;
            ApplyHeader(entity, request, header);
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            AddLines(entity, lines, actorUserId, now);
            _historyWriter.Record(
                ProcurementDocumentTypes.PurchaseRequest, entity.Id, entity.RequestNumber, ProcurementDocumentHistoryActions.Update,
                entity.Status, entity.Status, null, new { entity.RequestedTotal, LineCount = lines.Count },
                actorUserId, DateTimeOffset.UtcNow);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementRequestConflictException("PBJ-VAL-090", "Data sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda.");
        }

        await AuditAsync("PurchaseRequest.Update", entity, actorUserId);
        return await GetByIdAsync(user, entity.Id, cancellationToken);
    }

    /// <summary>
    /// Ajukan PR (<c>DRAFT</c>/<c>RETURNED</c> → <c>SUBMITTED</c>): validasi PBJ-VAL-001..012, alokasi nomor
    /// saat pertama, dan pembagian dalam/luar rencana di bawah kunci per baris rencana (BE-PBJ-007).
    /// Pemanggilan ulang dengan <c>Idempotency-Key</c> yang sama mengembalikan hasil pertama.
    /// </summary>
    public async Task<PurchaseRequestResponse> SubmitAsync(
        ClaimsPrincipal user, Guid id, string? idempotencyKey, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await EnsureVisibleAsync(user, id, cancellationToken);
        if (await _historyWriter.HasIdempotencyKeyAsync(
                ProcurementDocumentTypes.PurchaseRequest, id, ProcurementDocumentHistoryActions.Submit, idempotencyKey, cancellationToken))
            return await GetByIdAsync(user, id, cancellationToken);

        var pr = await _dbContext.PrcPurchaseRequests.AsNoTracking()
            .Include(x => x.Lines.Where(l => !l.IsDelete))
            .FirstAsync(x => x.Id == id, cancellationToken);
        if (pr.RequesterUserId != actorUserId)
            throw new ProcurementRequestForbiddenException("Hanya pembuat pengajuan yang dapat mengajukannya.");
        if (!IsEditableStatus(pr.Status))
            throw new ProcurementRequestConflictException("PBJ-VAL-093", InvalidTransitionMessage);

        var lines = pr.Lines.OrderBy(x => x.LineNo).ToList();
        await ValidateSubmissionAsync(pr, lines, actorUserId, cancellationToken);
        // Pemeriksaan awal tanpa kunci agar galat PBJ-VAL-009 tidak menghanguskan nomor; diulang di bawah kunci.
        await SplitAgainstPlanAsync(pr, lines, apply: false, cancellationToken);

        // Wfl dulu: instance lama yang dikembalikan penyetuju ditutup sebelum PR diajukan ulang.
        var closed = await _approvalGateway.CloseReturnedAsync(pr.WorkflowInstanceId, "PR diajukan ulang setelah diperbaiki.", cancellationToken);
        if (!closed.Success)
            throw new ProcurementRequestConflictException("PBJ-VAL-093", closed.Message);

        var requestNumber = pr.RequestNumber ?? await AllocateNumberAsync(actorUserId, cancellationToken);
        var keys = lines.Where(x => x.NeedPlanLineKey.HasValue).Select(x => x.NeedPlanLineKey!.Value).ToList();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _quotaService.LockAsync(keys, cancellationToken);
            var entity = await _dbContext.PrcPurchaseRequests
                .Include(x => x.Lines.Where(l => !l.IsDelete))
                .FirstAsync(x => x.Id == id, cancellationToken);
            if (entity.RowVersion != pr.RowVersion || !IsEditableStatus(entity.Status))
                throw new ProcurementRequestConflictException("PBJ-VAL-090", StaleMessage);

            var trackedLines = entity.Lines.OrderBy(x => x.LineNo).ToList();
            var outOfPlan = await SplitAgainstPlanAsync(entity, trackedLines, apply: true, cancellationToken);

            var fromStatus = entity.Status;
            var now = DateTimeOffset.UtcNow;
            entity.RequestNumber = requestNumber;
            entity.Status = ProcurementStatuses.PurchaseRequest.Submitted;
            entity.SubmissionCount += 1;
            entity.SubmittedAt = now;
            entity.IsOutOfPlan = outOfPlan;
            // Pengajuan ulang menuntut verifikasi ulang: hasil cek stok dan nilai beli lama dikosongkan.
            entity.PurchaseTotal = 0;
            entity.IsOverBudget = false;
            entity.OverBudgetAmount = 0;
            entity.VerifiedAt = null;
            entity.VerifiedByUserId = null;
            foreach (var line in trackedLines)
            {
                line.StockAvailableQuantity = null;
                line.StockFulfilledQuantity = null;
                line.StockSourceNote = null;
                line.StockCheckedAt = null;
                line.StockCheckedByUserId = null;
                line.PurchaseQuantity = 0;
                line.PurchaseAmount = 0;
            }
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = now.UtcDateTime;
            entity.UpdateBy = actorUserId;
            _historyWriter.Record(
                ProcurementDocumentTypes.PurchaseRequest, entity.Id, requestNumber, ProcurementDocumentHistoryActions.Submit,
                fromStatus, entity.Status, null,
                new
                {
                    idempotencyKey = idempotencyKey?.Trim(),
                    entity.RequestedTotal,
                    entity.SubmissionCount,
                    InPlanQuantity = trackedLines.Sum(x => x.InPlanQuantity),
                    OutOfPlanQuantity = trackedLines.Sum(x => x.OutOfPlanQuantity)
                },
                actorUserId, now);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await AuditAsync("PurchaseRequest.Submit", entity, actorUserId);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementRequestConflictException("PBJ-VAL-090", StaleMessage);
        }

        return await GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>
    /// Batal oleh pengusul (<c>DRAFT</c>/<c>SUBMITTED</c>/<c>RETURNED</c> → <c>CANCELLED</c>) atau tarik
    /// dari persetujuan (<c>PENDING_APPROVAL</c> → instance Wfl <c>Withdrawn</c> → <c>CANCELLED</c>).
    /// </summary>
    public async Task<PurchaseRequestResponse> CancelAsync(
        ClaimsPrincipal user, Guid id, CancelPurchaseRequestRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await EnsureVisibleAsync(user, id, cancellationToken);

        var pr = await _dbContext.PrcPurchaseRequests.AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);
        if (pr.RequesterUserId != actorUserId)
            throw new ProcurementRequestForbiddenException("Hanya pembuat pengajuan yang dapat membatalkannya.");

        var reason = Trim(request.Reason, 1000, "Alasan pembatalan");
        var isWithdraw = pr.Status == ProcurementStatuses.PurchaseRequest.PendingApproval;
        var cancellable = pr.Status == ProcurementStatuses.PurchaseRequest.Draft ||
                          pr.Status == ProcurementStatuses.PurchaseRequest.Submitted ||
                          pr.Status == ProcurementStatuses.PurchaseRequest.Returned;
        if (!isWithdraw && !cancellable)
            throw new ProcurementRequestConflictException("PBJ-VAL-093", InvalidTransitionMessage);
        if (reason == null && (pr.RequestNumber != null || isWithdraw))
            throw new ProcurementRequestValidationException("PBJ-VAL-030", "Alasan wajib diisi.");

        // Wfl dulu: tarik instance yang sedang berjalan, atau tutup instance yang dikembalikan penyetuju.
        if (isWithdraw)
        {
            if (!pr.WorkflowInstanceId.HasValue)
                throw new ProcurementRequestConflictException("PBJ-VAL-093", InvalidTransitionMessage);
            var withdrawn = await _approvalGateway.WithdrawAsync(pr.WorkflowInstanceId.Value, reason!, cancellationToken);
            if (!withdrawn.Success)
                throw new ProcurementRequestConflictException("PBJ-VAL-093", withdrawn.Message);
        }
        else
        {
            var closed = await _approvalGateway.CloseReturnedAsync(pr.WorkflowInstanceId, reason ?? "PR dibatalkan pengusul.", cancellationToken);
            if (!closed.Success)
                throw new ProcurementRequestConflictException("PBJ-VAL-093", closed.Message);
        }

        // Penerap status Wfl mungkin sudah membatalkan PR saat penarikan disinkronkan; bila belum, dibatalkan di sini.
        var entity = await _dbContext.PrcPurchaseRequests.FirstAsync(x => x.Id == id, cancellationToken);
        await _dbContext.Entry(entity).ReloadAsync(cancellationToken);
        if (entity.Status != ProcurementStatuses.PurchaseRequest.Cancelled)
        {
            if (entity.Status != pr.Status)
                throw new ProcurementRequestConflictException("PBJ-VAL-090", StaleMessage);
            var now = DateTimeOffset.UtcNow;
            entity.Status = ProcurementStatuses.PurchaseRequest.Cancelled;
            entity.CancelReason = reason;
            entity.ClosedAt = now;
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = now.UtcDateTime;
            entity.UpdateBy = actorUserId;
            _historyWriter.Record(
                ProcurementDocumentTypes.PurchaseRequest, entity.Id, entity.RequestNumber,
                isWithdraw ? ProcurementDocumentHistoryActions.Withdraw : ProcurementDocumentHistoryActions.Cancel,
                pr.Status, entity.Status, reason, null, actorUserId, now, isWithdraw ? pr.WorkflowInstanceId : null);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ProcurementRequestConflictException("PBJ-VAL-090", StaleMessage);
            }
        }

        await AuditAsync(isWithdraw ? "PurchaseRequest.Withdraw" : "PurchaseRequest.Cancel", entity, actorUserId);
        return await GetByIdAsync(user, id, cancellationToken);
    }

    // ------------------------------------------------------------------ bantuan

    private async Task EnsureVisibleAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        if (!await query.AnyAsync(x => x.Id == id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
    }

    /// <summary>PBJ-VAL-001..008, 010..012 saat diajukan. PBJ-VAL-009 diperiksa bersama pembagian rencana.</summary>
    private async Task ValidateSubmissionAsync(
        PrcPurchaseRequest pr, List<PrcPurchaseRequestLine> lines, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (lines.Count == 0)
            throw new ProcurementRequestValidationException("PBJ-VAL-001", "Pengajuan harus berisi minimal satu barang atau jasa.");

        var itemIds = lines.Select(x => x.ItemId).Distinct().ToList();
        var items = await _dbContext.MstProcurementItems.AsNoTracking()
            .Where(x => itemIds.Contains(x.Id) && !x.IsDelete)
            .Select(x => new { x.Id, x.ItemCode, x.IsActive })
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        foreach (var line in lines)
        {
            if (line.RequestedQuantity <= 0)
                throw new ProcurementRequestValidationException("PBJ-VAL-002", $"Jumlah pada baris {line.LineNo} harus lebih dari 0.", line.LineNo);
            if (!items.TryGetValue(line.ItemId, out var item) || !item.IsActive)
                throw new ProcurementRequestValidationException(
                    "PBJ-VAL-005",
                    $"Barang {item?.ItemCode ?? $"pada baris {line.LineNo}"} tidak aktif di katalog. Pilih barang lain atau minta Tim Pengadaan mengaktifkannya.",
                    line.LineNo);
        }

        if (string.IsNullOrWhiteSpace(pr.Justification) || pr.Justification.Trim().Length < 10)
            throw new ProcurementRequestValidationException("PBJ-VAL-006", "Alasan kebutuhan wajib diisi (minimal 10 karakter).");
        if (pr.NeededByDate < DateOnly.FromDateTime(DateTime.Now))
            throw new ProcurementRequestValidationException("PBJ-VAL-007", "Tanggal dibutuhkan tidak boleh sebelum hari ini.");
        if (pr.UrgencyLevel == ProcurementUrgencyLevels.Urgent &&
            (string.IsNullOrWhiteSpace(pr.UrgencyReason) || !pr.UrgencyDeadline.HasValue))
            throw new ProcurementRequestValidationException("PBJ-VAL-008", "Permintaan mendesak wajib berisi alasan dan batas waktu.");

        await RequirePlacementAsync(actorUserId, pr.DepartmentId, cancellationToken);

        if (!await _dbContext.MstProcurementFundSources.AnyAsync(x => x.Id == pr.FundSourceId && !x.IsDelete && x.IsActive, cancellationToken))
            throw new ProcurementRequestValidationException("PBJ-VAL-011", "Pilih sumber dana yang masih aktif.");

        var keys = lines.Where(x => x.NeedPlanLineKey.HasValue).Select(x => x.NeedPlanLineKey!.Value).ToList();
        var planLines = await _quotaService.GetCurrentLinesAsync(keys, cancellationToken);
        foreach (var line in lines.Where(x => x.NeedPlanLineKey.HasValue))
        {
            if (!planLines.TryGetValue(line.NeedPlanLineKey!.Value, out var planLine) ||
                planLine.ItemId != line.ItemId ||
                planLine.DepartmentId != pr.DepartmentId ||
                planLine.FiscalYear != pr.FiscalYear)
                throw new ProcurementRequestValidationException(
                    "PBJ-VAL-012", $"Baris {line.LineNo} tidak sesuai dengan rencana yang dipilih.", line.LineNo);
        }
    }

    /// <summary>
    /// Membagi kuantitas tiap baris menjadi dalam/luar rencana (PBJ-DEC-029) dan menegakkan PBJ-VAL-009.
    /// Sisa = kuantitas baris rencana − pemakaian PR lain berstatus pemakan − pemakaian baris sebelumnya
    /// pada PR ini yang menunjuk baris rencana yang sama. Baris tanpa baris rencana seluruhnya di luar
    /// rencana. Mengembalikan true bila ada kuantitas di luar rencana.
    /// </summary>
    private async Task<bool> SplitAgainstPlanAsync(
        PrcPurchaseRequest pr, List<PrcPurchaseRequestLine> lines, bool apply, CancellationToken cancellationToken)
    {
        var keys = lines.Where(x => x.NeedPlanLineKey.HasValue).Select(x => x.NeedPlanLineKey!.Value).ToList();
        var planLines = await _quotaService.GetCurrentLinesAsync(keys, cancellationToken);
        var usage = await _quotaService.GetUsageAsync(keys, pr.Id, cancellationToken);
        var itemIds = lines.Select(l => l.ItemId).Distinct().ToList();
        var measurement = await _dbContext.MstProcurementItems.AsNoTracking()
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Measurement!.MeasurementName })
            .ToDictionaryAsync(x => x.Id, x => x.MeasurementName, cancellationToken);

        var consumedHere = new Dictionary<Guid, decimal>();
        var anyOutOfPlan = false;
        foreach (var line in lines)
        {
            decimal inPlan = 0;
            if (line.NeedPlanLineKey.HasValue && planLines.TryGetValue(line.NeedPlanLineKey.Value, out var planLine))
            {
                var key = line.NeedPlanLineKey.Value;
                var used = (usage.GetValueOrDefault(key) ?? PlanLineUsage.None).UsedByPurchaseRequest + consumedHere.GetValueOrDefault(key);
                var remaining = Math.Max(0, planLine.Quantity - used);
                inPlan = Math.Min(line.RequestedQuantity, remaining);
                consumedHere[key] = consumedHere.GetValueOrDefault(key) + inPlan;
            }

            var outOfPlan = line.RequestedQuantity - inPlan;
            if (outOfPlan > 0)
            {
                anyOutOfPlan = true;
                if (string.IsNullOrWhiteSpace(line.OutOfPlanReason))
                    throw new ProcurementRequestValidationException(
                        "PBJ-VAL-009",
                        $"Baris {line.LineNo}: {outOfPlan.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture)} {measurement.GetValueOrDefault(line.ItemId, string.Empty)} di luar rencana. Isi alasannya.",
                        line.LineNo);
            }

            if (apply)
            {
                line.InPlanQuantity = inPlan;
                line.OutOfPlanQuantity = outOfPlan;
            }
        }

        return anyOutOfPlan;
    }

    private async Task<string> AllocateNumberAsync(Guid actorUserId, CancellationToken cancellationToken)
    {
        try
        {
            return await _numberAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: ProcurementNumberSeries.PurchaseRequestSequenceKey,
                    Prefix: ProcurementNumberSeries.PurchaseRequestPrefix,
                    ResetPolicy: ProcurementNumberSeries.ResetPolicy,
                    SequenceDigits: ProcurementNumberSeries.SequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow),
                cancellationToken);
        }
        catch (NumberSeriesAllocationException)
        {
            throw new ProcurementRequestConflictException("PBJ-VAL-093", "Nomor pengajuan gagal dibuat. Silakan coba lagi.");
        }
    }


    private async Task<IQueryable<PrcPurchaseRequest>> ScopedAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var scope = await _scopeService.ResolveAsync(user, ProcurementPermissions.PurchaseRequestResource, cancellationToken);
        var query = _dbContext.PrcPurchaseRequests.AsNoTracking().Where(x => !x.IsDelete);
        if (scope.IsAllUnits) return query;
        var departments = scope.DepartmentIds.ToList();
        return query.Where(x => departments.Contains(x.DepartmentId));
    }

    private static Guid RequireUser(ClaimsPrincipal user)
    {
        var userId = ProcurementDataScopeService.GetUserId(user);
        if (userId == Guid.Empty)
            throw new ProcurementRequestForbiddenException("Identitas pengguna tidak dikenali.");
        return userId;
    }

    private async Task RequirePlacementAsync(Guid userId, Guid departmentId, CancellationToken cancellationToken)
    {
        var placements = await _scopeService.GetPlacementDepartmentIdsAsync(userId, cancellationToken);
        if (!placements.Contains(departmentId))
        {
            var name = await _dbContext.MstDepartments.AsNoTracking()
                .Where(x => x.Id == departmentId).Select(x => x.DepartmentName).FirstOrDefaultAsync(cancellationToken);
            throw new ProcurementRequestValidationException(
                "PBJ-VAL-010", $"Anda tidak terdaftar di unit {name ?? "yang dipilih"}.");
        }
    }

    private sealed record HeaderValues(string? UrgencyReason, string Justification);

    private async Task<HeaderValues> ValidateHeaderAsync(CreatePurchaseRequestRequest request, CancellationToken cancellationToken)
    {
        if (!ProcurementUrgencyLevels.IsKnown(request.UrgencyLevel))
            throw new ProcurementRequestValidationException("PBJ-VAL-008", "Tingkat urgensi tidak dikenal.");
        if (request.FiscalYear is < 2000 or > 2100)
            throw new ProcurementRequestValidationException("PBJ-VAL-007", "Tahun anggaran tidak valid.");
        if (!await _dbContext.MstDepartments.AnyAsync(x => x.Id == request.DepartmentId && !x.IsDelete, cancellationToken))
            throw new ProcurementRequestValidationException("PBJ-VAL-010", "Unit tidak ditemukan.");
        if (!await _dbContext.MstProcurementFundSources.AnyAsync(x => x.Id == request.FundSourceId && !x.IsDelete, cancellationToken))
            throw new ProcurementRequestValidationException("PBJ-VAL-011", "Sumber dana tidak ditemukan.");

        // Alasan dan justifikasi boleh kosong selama draf; diwajibkan saat diajukan (PBJ-VAL-006/008, BE-PBJ-007).
        var urgencyReason = Trim(request.UrgencyReason, 1000, "Alasan mendesak");
        var justification = Trim(request.Justification, 2000, "Alasan kebutuhan") ?? string.Empty;
        return new HeaderValues(urgencyReason, justification);
    }

    private sealed record LineValues(
        Guid ItemId, string? Specification, decimal Quantity, decimal UnitPrice, Guid? NeedPlanLineKey, string? OutOfPlanReason);

    private async Task<List<LineValues>> ValidateLinesAsync(List<PurchaseRequestLineRequest>? lines, CancellationToken cancellationToken)
    {
        lines ??= new List<PurchaseRequestLineRequest>();
        var itemIds = lines.Select(x => x.ItemId).Distinct().ToList();
        var items = await _dbContext.MstProcurementItems.AsNoTracking()
            .Where(x => itemIds.Contains(x.Id) && !x.IsDelete)
            .Select(x => new { x.Id, x.ItemCode, x.IsActive, x.Measurement!.MeasurementName, x.Measurement.IsDecimalAllowed })
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var result = new List<LineValues>();
        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];
            var no = index + 1;
            if (!items.TryGetValue(line.ItemId, out var item) || !item.IsActive)
                throw new ProcurementRequestValidationException(
                    "PBJ-VAL-005",
                    $"Barang pada baris {no} tidak aktif di katalog. Pilih barang lain atau minta Tim Pengadaan mengaktifkannya.");
            if (line.RequestedQuantity <= 0)
                throw new ProcurementRequestValidationException("PBJ-VAL-002", $"Jumlah pada baris {no} harus lebih dari 0.");
            if (!item.IsDecimalAllowed && line.RequestedQuantity != decimal.Truncate(line.RequestedQuantity))
                throw new ProcurementRequestValidationException("PBJ-VAL-003", $"Satuan {item.MeasurementName} tidak boleh pecahan.");
            if (line.EstimatedUnitPrice < 0)
                throw new ProcurementRequestValidationException("PBJ-VAL-004", "Perkiraan harga tidak boleh negatif.");

            result.Add(new LineValues(
                line.ItemId,
                Trim(line.Specification, 1000, $"Spesifikasi baris {no}"),
                line.RequestedQuantity,
                line.EstimatedUnitPrice,
                line.NeedPlanLineKey,
                Trim(line.OutOfPlanReason, 1000, $"Alasan di luar rencana baris {no}")));
        }

        return result;
    }

    private static void ApplyHeader(PrcPurchaseRequest entity, CreatePurchaseRequestRequest request, HeaderValues header)
    {
        entity.FiscalYear = request.FiscalYear;
        entity.FundSourceId = request.FundSourceId;
        entity.NeededByDate = request.NeededByDate;
        entity.UrgencyLevel = request.UrgencyLevel;
        entity.UrgencyReason = request.UrgencyLevel == ProcurementUrgencyLevels.Urgent ? header.UrgencyReason : null;
        entity.UrgencyDeadline = request.UrgencyLevel == ProcurementUrgencyLevels.Urgent ? request.UrgencyDeadline : null;
        entity.Justification = header.Justification;
    }

    private void AddLines(PrcPurchaseRequest entity, List<LineValues> lines, Guid actorUserId, DateTime now)
    {
        decimal total = 0;
        var lineNo = 0;
        foreach (var line in lines)
        {
            lineNo++;
            var amount = Math.Round(line.Quantity * line.UnitPrice, 2, MidpointRounding.AwayFromZero);
            total += amount;
            var row = new PrcPurchaseRequestLine
            {
                PurchaseRequestId = entity.Id,
                LineNo = lineNo,
                ItemId = line.ItemId,
                Specification = line.Specification,
                RequestedQuantity = line.Quantity,
                EstimatedUnitPrice = line.UnitPrice,
                NeedPlanLineKey = line.NeedPlanLineKey,
                OutOfPlanReason = line.OutOfPlanReason,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            _dbContext.PrcPurchaseRequestLines.Add(row);
        }

        entity.RequestedTotal = total;
    }

    private static string? Trim(string? value, int maxLength, string label)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ProcurementRequestValidationException("PBJ-VAL-006", $"{label} maksimal {maxLength} karakter.");
        return trimmed;
    }

    private IQueryable<PurchaseRequestResponse> Project(IQueryable<PrcPurchaseRequest> query) =>
        query.Select(x => new PurchaseRequestResponse
        {
            Id = x.Id,
            RequestNumber = x.RequestNumber,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.Department!.DepartmentName,
            RequesterUserId = x.RequesterUserId,
            RequesterName = _dbContext.Users.Where(u => u.Id == x.RequesterUserId).Select(u => u.DisplayName).FirstOrDefault(),
            FiscalYear = x.FiscalYear,
            FundSourceId = x.FundSourceId,
            FundSourceName = x.FundSource!.FundSourceName,
            NeededByDate = x.NeededByDate,
            UrgencyLevel = x.UrgencyLevel,
            UrgencyReason = x.UrgencyReason,
            UrgencyDeadline = x.UrgencyDeadline,
            Justification = x.Justification,
            Status = x.Status,
            IsOutOfPlan = x.IsOutOfPlan,
            RequestedTotal = x.RequestedTotal,
            PurchaseTotal = x.PurchaseTotal,
            IsOverBudget = x.IsOverBudget,
            OverBudgetAmount = x.OverBudgetAmount,
            SubmissionCount = x.SubmissionCount,
            SubmittedAt = x.SubmittedAt,
            VerifiedAt = x.VerifiedAt,
            ApprovedAt = x.ApprovedAt,
            ClosedAt = x.ClosedAt,
            VerifiedByUserId = x.VerifiedByUserId,
            VerifiedByName = _dbContext.Users.Where(u => u.Id == x.VerifiedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
            WorkflowInstanceId = x.WorkflowInstanceId,
            CancelReason = x.CancelReason,
            RowVersion = x.RowVersion,
            CreateDateTime = x.CreateDateTime,
            UpdateDateTime = x.UpdateDateTime
        });

    private Task AuditAsync(string action, PrcPurchaseRequest entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan permintaan pengadaan.", new
        {
            EntityId = entity.Id,
            entity.Status,
            ActorUserId = actorUserId
        });
}

/// <summary>
/// Penyedia akses lampiran untuk PR (BE-PBJ-006; <see cref="IProcurementDocumentAccessProvider"/>).
/// </summary>
public sealed class PurchaseRequestAttachmentAccessProvider : IProcurementDocumentAccessProvider
{
    private readonly ProcurementPurchaseRequestService _service;

    public PurchaseRequestAttachmentAccessProvider(ProcurementPurchaseRequestService service) => _service = service;

    public string DocumentType => ProcurementDocumentTypes.PurchaseRequest;

    public Task<ProcurementDocumentAccessLevel> GetAccessAsync(ClaimsPrincipal user, Guid documentId, CancellationToken cancellationToken) =>
        _service.GetAccessLevelAsync(user, documentId, cancellationToken);
}
