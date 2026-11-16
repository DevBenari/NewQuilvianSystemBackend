using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Services;

/// <summary>
/// Verifikasi PR oleh Tim Pengadaan — antrean, cek stok, selesaikan, kembalikan, tolak, batal setelah
/// disetujui — serta penerapan hasil approval Wfl ke PR (BE-PBJ-008; <c>state-transition-matrix.md</c>
/// bagian 1 dan 5, <c>validation-matrix.md</c> bagian 1).
/// </summary>
/// <remarks>
/// <para><b>Nominal approval (PBJ-DEC-027)</b> adalah nilai kebutuhan beli setelah cek stok. Contoh: 20 kursi
/// roda @Rp 3.000.000 dengan 5 dipenuhi stok → kebutuhan beli 15, nilai beli Rp 45.000.000.</para>
/// <para><b>Lewat pagu (PBJ-DEC-030)</b> bukan penolakan. Per kategori: kelebihan = max(0, nilai beli −
/// sisa pagu); pagu yang tidak ada dianggap sisa 0. PR yang lewat pagu membawa kode kondisi
/// <c>PBJ_OVER_BUDGET</c>, PR di luar rencana <c>PBJ_OUT_OF_PLAN</c>, sehingga langkah approval
/// bersyaratnya ikut dibuat.</para>
/// <para><b>Verifikator ≠ pengusul.</b> Pengusul yang juga Tim Pengadaan tidak dapat memverifikasi PR-nya
/// sendiri (403).</para>
/// <para><b>Wfl dulu, status PR sesudahnya (PBJ-DEC-038).</b> Bila Wfl gagal, PR tetap
/// <c>SUBMITTED</c>; pengulangan dengan kunci idempoten yang sama memakai instance yang sudah ada.</para>
/// </remarks>
public sealed class ProcurementPurchaseRequestVerificationService
{
    private const string LogCategory = "Corporate.ProcurementManagement.Request";
    private const string NotFoundMessage = "Pengajuan tidak ditemukan atau bukan milik unit Anda.";
    private const string InvalidTransitionMessage = "Status pengajuan tidak memungkinkan tindakan ini.";
    private const string StaleMessage = "Data sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda.";
    private const string SelfVerifyMessage = "Pengusul tidak dapat memverifikasi pengajuannya sendiri.";

    private readonly ApplicationDbContext _dbContext;
    private readonly ProcurementDataScopeService _scopeService;
    private readonly ProcurementDocumentHistoryWriter _historyWriter;
    private readonly ProcurementPurchaseRequestService _purchaseRequestService;
    private readonly ProcurementBudgetService _budgetService;
    private readonly ProcurementApprovalGateway _approvalGateway;
    private readonly WorkflowReferenceLifecycleService _lifecycleService;
    private readonly LoggerService _loggerService;

    public ProcurementPurchaseRequestVerificationService(
        ApplicationDbContext dbContext,
        ProcurementDataScopeService scopeService,
        ProcurementDocumentHistoryWriter historyWriter,
        ProcurementPurchaseRequestService purchaseRequestService,
        ProcurementBudgetService budgetService,
        ProcurementApprovalGateway approvalGateway,
        WorkflowReferenceLifecycleService lifecycleService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _scopeService = scopeService;
        _historyWriter = historyWriter;
        _purchaseRequestService = purchaseRequestService;
        _budgetService = budgetService;
        _approvalGateway = approvalGateway;
        _lifecycleService = lifecycleService;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------ baca

    /// <summary>Antrean verifikasi: PR <c>SUBMITTED</c> dalam cakupan pengguna (3.2, 11.2).</summary>
    public Task<PagedResult<PurchaseRequestListItem>> GetQueueAsync(
        ClaimsPrincipal user, PurchaseRequestQuery request, CancellationToken cancellationToken)
    {
        request.Status = ProcurementStatuses.PurchaseRequest.Submitted;
        request.MineOnly = null;
        // Bawaan antrean: mendesak lebih dulu (03-frontend-architecture.md §4.6), kecuali klien memilih urutan lain.
        if (string.Equals(request.SortBy, "createdAt", StringComparison.OrdinalIgnoreCase)) request.SortBy = "urgency";
        return _purchaseRequestService.GetPagedAsync(user, request, cancellationToken);
    }

    /// <summary>Daftar hasil cek stok per baris (3.3).</summary>
    public async Task<PagedResult<StockCheckListItem>> GetStockChecksAsync(
        ClaimsPrincipal user, StockCheckQuery request, CancellationToken cancellationToken)
    {
        var scope = await _scopeService.ResolveAsync(user, ProcurementPermissions.PurchaseRequestResource, cancellationToken);
        var departments = scope.DepartmentIds.ToList();
        var status = string.IsNullOrWhiteSpace(request.Status) ? ProcurementStatuses.PurchaseRequest.Submitted : request.Status.Trim();

        var query = _dbContext.PrcPurchaseRequestLines.AsNoTracking()
            .Where(x => !x.IsDelete && !x.PurchaseRequest!.IsDelete && x.PurchaseRequest.Status == status);
        if (!scope.IsAllUnits) query = query.Where(x => departments.Contains(x.PurchaseRequest!.DepartmentId));
        if (request.DepartmentId.HasValue) query = query.Where(x => x.PurchaseRequest!.DepartmentId == request.DepartmentId.Value);
        if (request.IsChecked.HasValue)
            query = request.IsChecked.Value
                ? query.Where(x => x.StockFulfilledQuantity != null)
                : query.Where(x => x.StockFulfilledQuantity == null);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x =>
                (x.PurchaseRequest!.RequestNumber != null && x.PurchaseRequest.RequestNumber.ToUpper().Contains(search)) ||
                x.Item!.ItemCode.ToUpper().Contains(search) ||
                x.Item.ItemName.ToUpper().Contains(search));
        }

        query = query.OrderBy(x => x.PurchaseRequest!.SubmittedAt).ThenBy(x => x.PurchaseRequestId).ThenBy(x => x.LineNo);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new StockCheckListItem
            {
                PurchaseRequestId = x.PurchaseRequestId,
                RequestNumber = x.PurchaseRequest!.RequestNumber,
                DepartmentId = x.PurchaseRequest.DepartmentId,
                DepartmentName = x.PurchaseRequest.Department!.DepartmentName,
                Status = x.PurchaseRequest.Status,
                UrgencyLevel = x.PurchaseRequest.UrgencyLevel,
                LineId = x.Id,
                LineNo = x.LineNo,
                ItemId = x.ItemId,
                ItemCode = x.Item!.ItemCode,
                ItemName = x.Item.ItemName,
                MeasurementName = x.Item.Measurement!.MeasurementName,
                RequestedQuantity = x.RequestedQuantity,
                StockAvailableQuantity = x.StockAvailableQuantity,
                StockFulfilledQuantity = x.StockFulfilledQuantity,
                StockSourceNote = x.StockSourceNote,
                StockCheckedAt = x.StockCheckedAt,
                StockCheckedByName = _dbContext.Users.Where(u => u.Id == x.StockCheckedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                PurchaseQuantity = x.PurchaseQuantity,
                PurchaseAmount = x.PurchaseAmount
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<StockCheckListItem>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = request.PageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    // ------------------------------------------------------------------ tulis

    /// <summary>Catat atau ubah cek stok per baris saat <c>SUBMITTED</c> (PBJ-VAL-020).</summary>
    public async Task<PurchaseRequestResponse> RecordStockCheckAsync(
        ClaimsPrincipal user, Guid id, RecordStockCheckRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        var entity = await LoadForVerificationAsync(user, id, actorUserId, request.ExpectedRowVersion, cancellationToken);
        if (request.Lines.Count == 0)
            throw new ProcurementRequestValidationException("PBJ-VAL-020", "Isi hasil cek stok minimal satu baris.");

        var linesById = entity.Lines.ToDictionary(x => x.Id);
        var now = DateTimeOffset.UtcNow;
        foreach (var item in request.Lines)
        {
            if (!linesById.TryGetValue(item.LineId, out var line))
                throw new ProcurementRequestValidationException("PBJ-VAL-020", "Baris cek stok tidak dikenal pada pengajuan ini. Muat ulang pengajuan.");
            if (item.StockAvailableQuantity < 0 || item.StockFulfilledQuantity < 0 ||
                item.StockFulfilledQuantity > item.StockAvailableQuantity ||
                item.StockFulfilledQuantity > line.RequestedQuantity)
                throw new ProcurementRequestValidationException(
                    "PBJ-VAL-020",
                    $"Jumlah dipenuhi dari stok tidak boleh melebihi stok tersedia ({Format(item.StockAvailableQuantity)}) maupun jumlah diminta ({Format(line.RequestedQuantity)}).",
                    line.LineNo);

            line.StockAvailableQuantity = item.StockAvailableQuantity;
            line.StockFulfilledQuantity = item.StockFulfilledQuantity;
            line.StockSourceNote = string.IsNullOrWhiteSpace(item.StockSourceNote) ? null : item.StockSourceNote.Trim();
            line.StockCheckedAt = now;
            line.StockCheckedByUserId = actorUserId;
            line.UpdateDateTime = now.UtcDateTime;
            line.UpdateBy = actorUserId;
        }

        RecomputePurchase(entity);
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateDateTime = now.UtcDateTime;
        entity.UpdateBy = actorUserId;
        _historyWriter.Record(
            ProcurementDocumentTypes.PurchaseRequest, entity.Id, entity.RequestNumber, ProcurementDocumentHistoryActions.StockCheck,
            entity.Status, entity.Status, null,
            new
            {
                entity.PurchaseTotal,
                Lines = request.Lines.Select(x => new { x.LineId, x.StockAvailableQuantity, x.StockFulfilledQuantity })
            },
            actorUserId, now);
        await SaveAsync(cancellationToken);

        await AuditAsync("PurchaseRequest.StockCheck", entity, actorUserId);
        return await _purchaseRequestService.GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>
    /// Selesaikan verifikasi. Seluruh baris dipenuhi stok → <c>FULFILLED_FROM_STOCK</c>; ada kebutuhan beli
    /// → instance approval dibuat atas nama pengusul lalu PR menjadi <c>PENDING_APPROVAL</c>.
    /// </summary>
    public async Task<PurchaseRequestResponse> CompleteAsync(
        ClaimsPrincipal user, Guid id, CompleteVerificationRequest request, string? idempotencyKey, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        if (await _historyWriter.HasIdempotencyKeyAsync(
                ProcurementDocumentTypes.PurchaseRequest, id, ProcurementDocumentHistoryActions.VerifyComplete, idempotencyKey, cancellationToken))
            return await _purchaseRequestService.GetByIdAsync(user, id, cancellationToken);

        // Hitungan dari salinan tanpa pelacakan: Wfl menyimpan context yang sama, jadi entity terlacak baru
        // diubah sesudah Wfl selesai.
        var pr = await LoadForVerificationAsync(user, id, actorUserId, request.ExpectedRowVersion, cancellationToken, tracked: false);
        var uncheckedCount = pr.Lines.Count(x => x.StockFulfilledQuantity == null);
        if (uncheckedCount > 0)
            throw new ProcurementRequestValidationException("PBJ-VAL-021", $"Masih ada {uncheckedCount} baris yang belum dicek stoknya.");

        RecomputePurchase(pr);
        var purchaseQuantity = pr.Lines.Sum(x => x.PurchaseQuantity);
        var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        var now = DateTimeOffset.UtcNow;

        if (purchaseQuantity == 0)
        {
            var fulfilled = await TrackAsync(pr, cancellationToken);
            fulfilled.Status = ProcurementStatuses.PurchaseRequest.FulfilledFromStock;
            fulfilled.ClosedAt = now;
            fulfilled.VerifiedAt = now;
            fulfilled.VerifiedByUserId = actorUserId;
            _historyWriter.Record(
                ProcurementDocumentTypes.PurchaseRequest, fulfilled.Id, fulfilled.RequestNumber, ProcurementDocumentHistoryActions.VerifyComplete,
                ProcurementStatuses.PurchaseRequest.Submitted, fulfilled.Status, note,
                new { idempotencyKey = idempotencyKey?.Trim(), fulfilled.PurchaseTotal, PurchaseQuantity = 0m },
                actorUserId, now);
            Touch(fulfilled, actorUserId, now);
            await SaveAsync(cancellationToken);
            await AuditAsync("PurchaseRequest.VerifyComplete", fulfilled, actorUserId);
            return await _purchaseRequestService.GetByIdAsync(user, id, cancellationToken);
        }

        // PBJ-DEC-030: uji lewat pagu per kategori barang.
        var categories = await _dbContext.MstProcurementItems.AsNoTracking()
            .Where(x => pr.Lines.Select(l => l.ItemId).Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.ItemCategoryId, cancellationToken);
        var amountByCategory = pr.Lines
            .GroupBy(x => categories[x.ItemId])
            .ToDictionary(g => g.Key, g => g.Sum(x => x.PurchaseAmount));
        var remaining = await _budgetService.GetRemainingByCategoryAsync(
            pr.FiscalYear, pr.DepartmentId, pr.FundSourceId, amountByCategory.Keys.ToList(), cancellationToken);
        var overBudgetAmount = amountByCategory.Sum(x => Math.Max(0, x.Value - remaining.GetValueOrDefault(x.Key)));
        var isOverBudget = overBudgetAmount > 0;

        var conditionCodes = new List<string>();
        if (pr.IsOutOfPlan) conditionCodes.Add(ProcurementWorkflowCodes.OutOfPlanCondition);
        if (isOverBudget) conditionCodes.Add(ProcurementWorkflowCodes.OverBudgetCondition);
        var definition = pr.UrgencyLevel == ProcurementUrgencyLevels.Urgent
            ? ProcurementWorkflowCodes.UrgentPurchaseRequestDefinition
            : ProcurementWorkflowCodes.PurchaseRequestDefinition;

        var approval = await _approvalGateway.SubmitAsync(
            new ProcurementApprovalRequest(
                ProcurementDocumentTypes.PurchaseRequest, pr.Id, pr.RequestNumber, definition,
                pr.RequesterUserId, pr.PurchaseTotal, conditionCodes, pr.SubmissionCount),
            cancellationToken);
        if (!approval.Success)
            throw new ProcurementRequestConflictException(
                "PBJ-VAL-091",
                approval.ConfigurationMissing ? "Alur persetujuan pengadaan belum dikonfigurasi. Hubungi administrator." : approval.Message);

        var entity = await TrackAsync(pr, cancellationToken);
        entity.IsOverBudget = isOverBudget;
        entity.OverBudgetAmount = overBudgetAmount;
        entity.Status = ProcurementStatuses.PurchaseRequest.PendingApproval;
        entity.WorkflowInstanceId = approval.WorkflowInstanceId;
        entity.VerifiedAt = now;
        entity.VerifiedByUserId = actorUserId;
        _historyWriter.Record(
            ProcurementDocumentTypes.PurchaseRequest, entity.Id, entity.RequestNumber, ProcurementDocumentHistoryActions.VerifyComplete,
            ProcurementStatuses.PurchaseRequest.Submitted, entity.Status, note,
            new
            {
                idempotencyKey = idempotencyKey?.Trim(),
                entity.PurchaseTotal,
                entity.IsOverBudget,
                entity.OverBudgetAmount,
                entity.IsOutOfPlan,
                Definition = definition,
                ConditionCodes = conditionCodes
            },
            actorUserId, now, approval.WorkflowInstanceId);
        Touch(entity, actorUserId, now);
        await SaveAsync(cancellationToken);

        await AuditAsync("PurchaseRequest.VerifyComplete", entity, actorUserId);
        return await _purchaseRequestService.GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>Kembalikan ke pengusul (<c>SUBMITTED</c> → <c>RETURNED</c>); alasan wajib (PBJ-VAL-030).</summary>
    public Task<PurchaseRequestResponse> ReturnAsync(
        ClaimsPrincipal user, Guid id, ReturnPurchaseRequestRequest request, CancellationToken cancellationToken) =>
        CloseFromVerificationAsync(user, id, request.Reason, ProcurementStatuses.PurchaseRequest.Returned,
            ProcurementDocumentHistoryActions.Return, "PurchaseRequest.Return", cancellationToken);

    /// <summary>Tolak (<c>SUBMITTED</c> → <c>REJECTED</c>, final); alasan wajib (PBJ-VAL-030).</summary>
    public Task<PurchaseRequestResponse> RejectAsync(
        ClaimsPrincipal user, Guid id, RejectPurchaseRequestRequest request, CancellationToken cancellationToken) =>
        CloseFromVerificationAsync(user, id, request.Reason, ProcurementStatuses.PurchaseRequest.Rejected,
            ProcurementDocumentHistoryActions.Reject, "PurchaseRequest.Reject", cancellationToken);

    /// <summary>
    /// Batalkan PR <c>APPROVED</c> yang belum masuk paket (→ <c>CANCELLED</c>); alasan wajib. PR yang sudah
    /// <c>CONVERTED_TO_PACKAGE</c> ditolak 409 PBJ-VAL-092.
    /// </summary>
    public async Task<PurchaseRequestResponse> CancelApprovedAsync(
        ClaimsPrincipal user, Guid id, CancelPurchaseRequestRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await EnsureVisibleAsync(user, id, cancellationToken);
        var reason = RequireReason(request.Reason);

        var entity = await _dbContext.PrcPurchaseRequests.FirstAsync(x => x.Id == id, cancellationToken);
        if (entity.Status == ProcurementStatuses.PurchaseRequest.ConvertedToPackage || entity.ConvertedPackageId.HasValue)
            throw new ProcurementRequestConflictException("PBJ-VAL-092", "Pengajuan sudah masuk paket dan tidak dapat dibatalkan dari sini.");
        if (entity.Status != ProcurementStatuses.PurchaseRequest.Approved)
            throw new ProcurementRequestConflictException("PBJ-VAL-093", InvalidTransitionMessage);

        var now = DateTimeOffset.UtcNow;
        entity.Status = ProcurementStatuses.PurchaseRequest.Cancelled;
        entity.CancelReason = reason;
        entity.ClosedAt = now;
        _historyWriter.Record(
            ProcurementDocumentTypes.PurchaseRequest, entity.Id, entity.RequestNumber, ProcurementDocumentHistoryActions.Cancel,
            ProcurementStatuses.PurchaseRequest.Approved, entity.Status, reason, new { entity.PurchaseTotal }, actorUserId, now);
        Touch(entity, actorUserId, now);
        await SaveAsync(cancellationToken);

        await AuditAsync("PurchaseRequest.CancelApproved", entity, actorUserId);
        return await _purchaseRequestService.GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>
    /// Sinkron ulang status PR dari instance Wfl-nya (<c>02-backend-architecture.md</c> bagian 8, "kegagalan
    /// sinkron"). Dipakai bila penerapan hasil approval sempat gagal: keputusan Wfl sudah sah, PR masih
    /// <c>PENDING_APPROVAL</c>. Idempoten — PR yang sudah selaras tidak berubah dan riwayatnya tidak ganda.
    /// </summary>
    public async Task<PurchaseRequestResponse> SynchronizeWorkflowAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await EnsureVisibleAsync(user, id, cancellationToken);
        var workflowInstanceId = await _dbContext.PrcPurchaseRequests.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => x.WorkflowInstanceId)
            .FirstAsync(cancellationToken);
        if (!workflowInstanceId.HasValue)
            throw new ProcurementRequestConflictException("PBJ-VAL-093", "Pengajuan belum memiliki alur persetujuan.");

        await _lifecycleService.SynchronizeAsync(workflowInstanceId.Value, actorUserId, allowAutoApply: true, cancellationToken);
        return await _purchaseRequestService.GetByIdAsync(user, id, cancellationToken);
    }

    // ------------------------------------------------------------------ hasil approval

    /// <summary>
    /// Menerapkan status Wfl ke PR (matriks status bagian 5). Hanya PR <c>PENDING_APPROVAL</c> yang tertaut
    /// ke instance ini yang dipindahkan; pemanggilan ulang tidak mengubah apa pun dan tidak menambah riwayat.
    /// Status <c>APPROVED</c> hanya lahir di sini.
    /// </summary>
    public async Task<WorkflowReferenceLifecycleSynchronizationResult> ApplyWorkflowStatusAsync(
        TrxWorkflowInstance workflow, Guid actorUserId, CancellationToken cancellationToken)
    {
        var result = new WorkflowReferenceLifecycleSynchronizationResult
        {
            IsHandled = true,
            WorkflowInstanceId = workflow.Id,
            ReferenceId = workflow.ReferenceId,
            WorkflowStatus = workflow.WorkflowStatus
        };

        var pr = await _dbContext.PrcPurchaseRequests.FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken);
        if (pr == null) return result;
        result.PreviousReferenceStatus = pr.Status;
        result.CurrentReferenceStatus = pr.Status;

        var target = ProcurementWorkflowStatusMap.ToDocumentStatus(workflow.WorkflowStatus, isPurchaseRequest: true);
        if (pr.WorkflowInstanceId != workflow.Id ||
            pr.Status != ProcurementStatuses.PurchaseRequest.PendingApproval ||
            target == null || target == pr.Status)
            return result;

        var actor = actorUserId == Guid.Empty ? workflow.RequestedByUserId : actorUserId;
        var now = DateTimeOffset.UtcNow;
        var from = pr.Status;
        pr.Status = target;
        string action;
        switch (target)
        {
            case ProcurementStatuses.PurchaseRequest.Approved:
                pr.ApprovedAt = now;
                action = ProcurementDocumentHistoryActions.WorkflowApproved;
                break;
            case ProcurementStatuses.PurchaseRequest.Rejected:
                pr.ClosedAt = now;
                action = ProcurementDocumentHistoryActions.WorkflowRejected;
                break;
            case ProcurementStatuses.PurchaseRequest.Cancelled:
                pr.ClosedAt = now;
                pr.CancelReason = workflow.CancellationReason;
                action = string.Equals(workflow.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.Withdrawn, StringComparison.OrdinalIgnoreCase)
                    ? ProcurementDocumentHistoryActions.Withdraw
                    : ProcurementDocumentHistoryActions.Cancel;
                break;
            default:
                action = ProcurementDocumentHistoryActions.WorkflowReturned;
                break;
        }

        _historyWriter.Record(
            ProcurementDocumentTypes.PurchaseRequest, pr.Id, pr.RequestNumber, action, from, target,
            workflow.CancellationReason, new { workflow.WorkflowStatus, pr.PurchaseTotal }, actor, now, workflow.Id);
        Touch(pr, actor, now);
        await _dbContext.SaveChangesAsync(cancellationToken);

        result.CurrentReferenceStatus = pr.Status;
        result.StatusChanged = true;
        await AuditAsync("PurchaseRequest.WorkflowSync", pr, actor);
        return result;
    }

    // ------------------------------------------------------------------ bantuan

    private async Task<PurchaseRequestResponse> CloseFromVerificationAsync(
        ClaimsPrincipal user, Guid id, string? reasonText, string targetStatus, string actionCode, string auditAction,
        CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        var reason = RequireReason(reasonText);
        var entity = await LoadForVerificationAsync(user, id, actorUserId, null, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        entity.Status = targetStatus;
        if (targetStatus == ProcurementStatuses.PurchaseRequest.Rejected) entity.ClosedAt = now;
        entity.VerifiedAt = now;
        entity.VerifiedByUserId = actorUserId;
        _historyWriter.Record(
            ProcurementDocumentTypes.PurchaseRequest, entity.Id, entity.RequestNumber, actionCode,
            ProcurementStatuses.PurchaseRequest.Submitted, targetStatus, reason, null, actorUserId, now);
        Touch(entity, actorUserId, now);
        await SaveAsync(cancellationToken);

        await AuditAsync(auditAction, entity, actorUserId);
        return await _purchaseRequestService.GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>
    /// Memuat PR <c>SUBMITTED</c> yang terlihat pengguna, menolak pengusul (403) dan data basi (409).
    /// </summary>
    private async Task<PrcPurchaseRequest> LoadForVerificationAsync(
        ClaimsPrincipal user, Guid id, Guid actorUserId, Guid? expectedRowVersion, CancellationToken cancellationToken, bool tracked = true)
    {
        await EnsureVisibleAsync(user, id, cancellationToken);
        var query = _dbContext.PrcPurchaseRequests.Include(x => x.Lines.Where(l => !l.IsDelete));
        var entity = tracked
            ? await query.FirstAsync(x => x.Id == id, cancellationToken)
            : await query.AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);
        if (entity.RequesterUserId == actorUserId)
            throw new ProcurementRequestForbiddenException(SelfVerifyMessage);
        if (entity.Status != ProcurementStatuses.PurchaseRequest.Submitted)
            throw new ProcurementRequestConflictException("PBJ-VAL-093", InvalidTransitionMessage);
        if (expectedRowVersion.HasValue && entity.RowVersion != expectedRowVersion.Value)
            throw new ProcurementRequestConflictException("PBJ-VAL-090", StaleMessage);
        return entity;
    }

    /// <summary>Memuat entity terlacak dan memindahkan hasil hitungan dari salinan tanpa pelacakan.</summary>
    private async Task<PrcPurchaseRequest> TrackAsync(PrcPurchaseRequest snapshot, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.PrcPurchaseRequests
            .Include(x => x.Lines.Where(l => !l.IsDelete))
            .FirstAsync(x => x.Id == snapshot.Id, cancellationToken);
        if (entity.RowVersion != snapshot.RowVersion || entity.Status != ProcurementStatuses.PurchaseRequest.Submitted)
            throw new ProcurementRequestConflictException("PBJ-VAL-090", StaleMessage);
        var lines = snapshot.Lines.ToDictionary(x => x.Id);
        foreach (var line in entity.Lines)
        {
            line.PurchaseQuantity = lines[line.Id].PurchaseQuantity;
            line.PurchaseAmount = lines[line.Id].PurchaseAmount;
        }
        entity.PurchaseTotal = snapshot.PurchaseTotal;
        return entity;
    }

    /// <summary>
    /// Kebutuhan beli = diminta − dipenuhi stok (baris yang belum dicek dihitung penuh); nilai beli =
    /// kebutuhan beli × perkiraan harga, dibulatkan 2 desimal.
    /// </summary>
    private static void RecomputePurchase(PrcPurchaseRequest entity)
    {
        decimal total = 0;
        foreach (var line in entity.Lines)
        {
            line.PurchaseQuantity = Math.Max(0, line.RequestedQuantity - (line.StockFulfilledQuantity ?? 0));
            line.PurchaseAmount = Math.Round(line.PurchaseQuantity * line.EstimatedUnitPrice, 2, MidpointRounding.AwayFromZero);
            total += line.PurchaseAmount;
        }
        entity.PurchaseTotal = total;
    }

    private async Task EnsureVisibleAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var scope = await _scopeService.ResolveAsync(user, ProcurementPermissions.PurchaseRequestResource, cancellationToken);
        var departments = scope.DepartmentIds.ToList();
        var visible = await _dbContext.PrcPurchaseRequests.AsNoTracking()
            .AnyAsync(x => x.Id == id && !x.IsDelete && (scope.IsAllUnits || departments.Contains(x.DepartmentId)), cancellationToken);
        if (!visible)
            throw new KeyNotFoundException(NotFoundMessage);
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementRequestConflictException("PBJ-VAL-090", StaleMessage);
        }
    }

    private static void Touch(PrcPurchaseRequest entity, Guid actorUserId, DateTimeOffset now)
    {
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateDateTime = now.UtcDateTime;
        entity.UpdateBy = actorUserId;
    }

    private static string RequireReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ProcurementRequestValidationException("PBJ-VAL-030", "Alasan wajib diisi.");
        var trimmed = reason.Trim();
        if (trimmed.Length > 1000)
            throw new ProcurementRequestValidationException("PBJ-VAL-030", "Alasan maksimal 1000 karakter.");
        return trimmed;
    }

    private static string Format(decimal value) => value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);

    private static Guid RequireUser(ClaimsPrincipal user)
    {
        var userId = ProcurementDataScopeService.GetUserId(user);
        if (userId == Guid.Empty)
            throw new ProcurementRequestForbiddenException("Identitas pengguna tidak dikenali.");
        return userId;
    }

    private Task AuditAsync(string action, PrcPurchaseRequest entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan permintaan pengadaan.", new
        {
            EntityId = entity.Id,
            entity.Status,
            ActorUserId = actorUserId
        });
}

/// <summary>
/// Penerap status Wfl untuk PR (<c>PBJ_PR</c>, BE-PBJ-008). Service diambil saat dipanggil agar tidak
/// membentuk lingkaran dependensi dengan <c>WorkflowService</c>.
/// </summary>
public sealed class PurchaseRequestWorkflowStatusApplier : IProcurementWorkflowStatusApplier
{
    private readonly IServiceProvider _serviceProvider;

    public PurchaseRequestWorkflowStatusApplier(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public string DocumentType => ProcurementDocumentTypes.PurchaseRequest;

    public Task<WorkflowReferenceLifecycleSynchronizationResult> ApplyAsync(
        TrxWorkflowInstance workflow, Guid actorUserId, CancellationToken cancellationToken) =>
        _serviceProvider.GetRequiredService<ProcurementPurchaseRequestVerificationService>()
            .ApplyWorkflowStatusAsync(workflow, actorUserId, cancellationToken);
}
