using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;
using Statuses = QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants.ProcurementStatuses.VendorStatusRequest;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Services;

/// <summary>
/// Usulan blokir dan buka blokir vendor lewat approval <c>Wfl</c> (BE-PBJ-VD-007; PBJ-DEC-021, 054, 060; PBJ-ASM-003;
/// PBJ-VD-STATE bagian 3).
/// </summary>
/// <remarks>
/// <para>
/// <b>Alur.</b> Manajer Pengadaan membuat usulan (<c>DRAFT</c>), mengajukannya (nomor <c>UBV-…</c> dialokasikan, instance
/// <c>Wfl</c> dibuat atas nama pengusul), lalu penyetuju memutus di kotak masuk <c>Wfl</c>. Keputusan diterapkan oleh
/// <see cref="ApplyWorkflowStatusAsync"/>: hanya usulan yang <b>disetujui</b> yang menulis <c>MstSupplier.IsBlacklisted</c>,
/// lewat <see cref="SupplierProcurementStatusService"/> dalam transaksi yang sama (PBJ-DEC-060).
/// </para>
/// <para>
/// Contoh: UBV-000003 "blokir PT Alkes Maju — 3 NCR kritis dalam 6 bulan", bukti penilaian Semester I 2027. Direktur
/// menyetujui → PT Alkes Maju diblokir, Finance melihatnya terblokir, RFQ baru tidak menawarkannya; proses yang sudah
/// berjalan tidak dibatalkan (PBJ-DEC-054).
/// </para>
/// <para>
/// Maksimal satu usulan terbuka per vendor (<c>PBJ-VAL-107</c>, juga index unik parsial). Definisi <c>PBJ-VND-*</c> tidak
/// aktif sampai penyetuju diisi dari SOP; pengajuan saat itu ditolak 409 <c>PBJ-VAL-091</c> dan usulan tetap <c>DRAFT</c>.
/// </para>
/// </remarks>
public sealed class ProcurementVendorStatusService
{
    private const string LogCategory = "Corporate.ProcurementManagement.Vendor";
    private const string NotFoundMessage = "Usulan status vendor tidak ditemukan atau sudah dihapus.";
    private const string StaleMessage = "Usulan sudah diubah orang lain. Muat ulang lalu ulangi tindakan Anda.";
    private const string InvalidTransitionMessage = "Status usulan tidak memungkinkan tindakan ini.";
    private const string RequestTypeBlock = "BLOCK";
    private const string RequestTypeUnblock = "UNBLOCK";

    private static readonly IReadOnlyDictionary<string, string> StatusLabels = new Dictionary<string, string>
    {
        [Statuses.Draft] = "Draf",
        [Statuses.PendingApproval] = "Menunggu persetujuan",
        [Statuses.Returned] = "Dikembalikan",
        [Statuses.Approved] = "Disetujui",
        [Statuses.Rejected] = "Ditolak",
        [Statuses.Cancelled] = "Dibatalkan"
    };

    private static readonly IReadOnlyDictionary<string, string> TypeLabels = new Dictionary<string, string>
    {
        [RequestTypeBlock] = "Blokir",
        [RequestTypeUnblock] = "Buka blokir"
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly ProcurementDocumentHistoryWriter _historyWriter;
    private readonly ProcurementApprovalGateway _approvalGateway;
    private readonly ProcurementVendorProfileService _profileService;
    private readonly SupplierProcurementStatusService _supplierStatusService;
    private readonly NumberSeriesAllocator _numberAllocator;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly WorkflowReferenceLifecycleService _lifecycleService;
    private readonly LoggerService _loggerService;

    public ProcurementVendorStatusService(
        ApplicationDbContext dbContext,
        ProcurementDocumentHistoryWriter historyWriter,
        ProcurementApprovalGateway approvalGateway,
        ProcurementVendorProfileService profileService,
        SupplierProcurementStatusService supplierStatusService,
        NumberSeriesAllocator numberAllocator,
        AccessPermissionService accessPermissionService,
        WorkflowReferenceLifecycleService lifecycleService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _historyWriter = historyWriter;
        _approvalGateway = approvalGateway;
        _profileService = profileService;
        _supplierStatusService = supplierStatusService;
        _numberAllocator = numberAllocator;
        _accessPermissionService = accessPermissionService;
        _lifecycleService = lifecycleService;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------ baca

    public Task<VendorStatusRequestFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new VendorStatusRequestFilterMetadataResponse
        {
            SortOptions =
            [
                new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                new() { Value = "requestNumber", Label = "Nomor usulan" },
                new() { Value = "supplier", Label = "Vendor" },
                new() { Value = "status", Label = "Status" }
            ],
            RequestTypeOptions = TypeLabels.Select(x => new VendorValueOptionResponse { Value = x.Key, Label = x.Value }).ToList(),
            StatusOptions = StatusLabels.Select(x => new VendorValueOptionResponse { Value = x.Key, Label = x.Value }).ToList()
        });

    public async Task<VendorStatusRequestSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var counts = await _dbContext.PrcVendorStatusRequests.AsNoTracking()
            .Where(x => !x.IsDelete && !x.VendorProfile!.IsDelete)
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        int Of(string status) => counts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;
        return new VendorStatusRequestSummaryResponse
        {
            Draft = Of(Statuses.Draft),
            PendingApproval = Of(Statuses.PendingApproval),
            Returned = Of(Statuses.Returned),
            Approved = Of(Statuses.Approved),
            Rejected = Of(Statuses.Rejected),
            Cancelled = Of(Statuses.Cancelled)
        };
    }

    public async Task<PagedResult<VendorStatusRequestListItemResponse>> GetPagedAsync(VendorStatusRequestQuery request, CancellationToken cancellationToken)
    {
        var type = NormalizeOption(request.RequestType, "Jenis usulan", TypeLabels.Keys);
        var status = NormalizeOption(request.Status, "Status usulan", StatusLabels.Keys);
        var query = _dbContext.PrcVendorStatusRequests.AsNoTracking().Where(x => !x.IsDelete && !x.VendorProfile!.IsDelete);
        query = query.WhereCreatedWithin(ProcurementPeriodFilter.Resolve(request.StartDate, request.EndDate, request.CustomPeriod));
        if (request.SupplierId.HasValue) query = query.Where(x => x.VendorProfile!.SupplierId == request.SupplierId.Value);
        if (type != null) query = query.Where(x => x.RequestType == type);
        if (status != null) query = query.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => (x.RequestNumber != null && x.RequestNumber.ToUpper().Contains(search))
                || x.VendorProfile!.Supplier!.SupplierCode.ToUpper().Contains(search)
                || x.VendorProfile.Supplier.SupplierName.ToUpper().Contains(search));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "requestnumber" => descending ? query.OrderByDescending(x => x.RequestNumber) : query.OrderBy(x => x.RequestNumber),
            "supplier" => descending
                ? query.OrderByDescending(x => x.VendorProfile!.Supplier!.SupplierName)
                : query.OrderBy(x => x.VendorProfile!.Supplier!.SupplierName),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new VendorStatusRequestListItemResponse
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                SupplierId = x.VendorProfile!.SupplierId,
                SupplierCode = x.VendorProfile.Supplier!.SupplierCode,
                SupplierName = x.VendorProfile.Supplier.SupplierName,
                RequestType = x.RequestType,
                Status = x.Status,
                RequesterUserId = x.CreateBy,
                RequesterName = _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName).FirstOrDefault(),
                SubmittedAt = x.SubmittedAt,
                DecidedAt = x.DecidedAt,
                CreateDateTime = x.CreateDateTime
            })
            .ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            item.RequestTypeLabel = Label(TypeLabels, item.RequestType);
            item.StatusLabel = Label(StatusLabels, item.Status);
        }

        return new PagedResult<VendorStatusRequestListItemResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = request.PageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    public async Task<VendorStatusRequestResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await _dbContext.PrcVendorStatusRequests.AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete && !x.VendorProfile!.IsDelete)
            .Select(x => new
            {
                Response = new VendorStatusRequestResponse
                {
                    Id = x.Id,
                    RequestNumber = x.RequestNumber,
                    SupplierId = x.VendorProfile!.SupplierId,
                    SupplierCode = x.VendorProfile.Supplier!.SupplierCode,
                    SupplierName = x.VendorProfile.Supplier.SupplierName,
                    IsSupplierBlacklisted = x.VendorProfile.Supplier.IsBlacklisted,
                    VendorProfileId = x.VendorProfileId,
                    RequestType = x.RequestType,
                    Reason = x.Reason,
                    EvidenceEvaluationId = x.EvidenceEvaluationId,
                    Status = x.Status,
                    WorkflowInstanceId = x.WorkflowInstanceId,
                    SubmissionCount = x.SubmissionCount,
                    SubmittedAt = x.SubmittedAt,
                    DecidedAt = x.DecidedAt,
                    RequesterUserId = x.CreateBy,
                    RequesterName = _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName).FirstOrDefault(),
                    RowVersion = x.RowVersion,
                    CreateDateTime = x.CreateDateTime
                },
                Evidence = x.EvidenceEvaluation == null ? null : new
                {
                    x.EvidenceEvaluation.ItemCategory!.CategoryName,
                    x.EvidenceEvaluation.PeriodType,
                    x.EvidenceEvaluation.PeriodNo,
                    x.EvidenceEvaluation.PeriodYear,
                    x.EvidenceEvaluation.FinalScore,
                    x.EvidenceEvaluation.Recommendation
                }
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);

        var response = detail.Response;
        response.RequestTypeLabel = Label(TypeLabels, response.RequestType);
        response.StatusLabel = Label(StatusLabels, response.Status);
        if (detail.Evidence != null)
            response.EvidenceEvaluationLabel =
                $"{detail.Evidence.CategoryName} {ProcurementVendorEvaluationService.PeriodLabel(detail.Evidence.PeriodType, detail.Evidence.PeriodNo, detail.Evidence.PeriodYear)}"
                + (detail.Evidence.FinalScore.HasValue ? $" — {detail.Evidence.FinalScore:0.00}" : string.Empty)
                + (detail.Evidence.Recommendation == null ? string.Empty : $" ({detail.Evidence.Recommendation})");
        return response;
    }

    public async Task<List<DocumentHistoryResponse>> GetHistoryAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await _dbContext.PrcVendorStatusRequests.AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
        return await _historyWriter.ReadAsync(ProcurementDocumentTypes.VendorStatusRequest, id, cancellationToken);
    }

    /// <summary>
    /// Hak dokumen <c>PBJ_VENDOR_STATUS</c> untuk lampiran dan linimasa persetujuan: tanpa
    /// <c>ProcurementVendorStatusRequest : Read</c> → tidak terlihat; pengusul boleh menambah lampiran selama
    /// <c>DRAFT</c>/<c>RETURNED</c>; selain itu hanya baca.
    /// </summary>
    public async Task<ProcurementDocumentAccessLevel> GetAccessLevelAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var userId = ProcurementDataScopeService.GetUserId(user);
        if (userId == Guid.Empty
            || !await _accessPermissionService.HasAccessAsync(user, ProcurementPermissions.VendorStatusRequestResource, ProcurementPermissions.ReadAction))
            return ProcurementDocumentAccessLevel.None;
        var row = await _dbContext.PrcVendorStatusRequests.AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete && !x.VendorProfile!.IsDelete)
            .Select(x => new { x.Status, x.CreateBy })
            .FirstOrDefaultAsync(cancellationToken);
        if (row == null) return ProcurementDocumentAccessLevel.None;
        return (row.Status == Statuses.Draft || row.Status == Statuses.Returned) && row.CreateBy == userId
            ? ProcurementDocumentAccessLevel.Edit
            : ProcurementDocumentAccessLevel.Read;
    }

    // ------------------------------------------------------------------ tulis

    /// <summary>
    /// Buat usulan <c>DRAFT</c>. Jenis harus sesuai status vendor (<c>PBJ-VAL-106</c>) dan tidak boleh ada usulan terbuka lain
    /// untuk vendor itu (<c>PBJ-VAL-107</c>). Profil vendor dibuat bila belum ada.
    /// </summary>
    public async Task<VendorStatusRequestResponse> CreateAsync(CreateVendorStatusRequestRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        var type = NormalizeOption(request.RequestType, "Jenis usulan", TypeLabels.Keys)
            ?? throw new ProcurementVendorValidationException(null, "Jenis usulan wajib dipilih: BLOCK atau UNBLOCK.");
        var reason = RequireReason(request.Reason, "Alasan usulan wajib diisi.");
        var supplier = await _dbContext.MstSuppliers.AsNoTracking()
            .Where(x => x.Id == request.SupplierId && !x.IsDelete)
            .Select(x => new { x.Id, x.SupplierCode, x.IsActive, x.IsBlacklisted, x.BlacklistReason })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ProcurementVendorValidationException(null, "Vendor tidak ditemukan atau sudah dihapus di Master Data.");
        EnsureTypeMatchesStatus(type, supplier.IsBlacklisted);

        var now = DateTimeOffset.UtcNow;
        var profile = await _dbContext.PrcVendorProfiles.FirstOrDefaultAsync(x => x.SupplierId == supplier.Id && !x.IsDelete, cancellationToken);
        if (profile != null) await EnsureNoOpenRequestAsync(profile.Id, null, cancellationToken);
        profile ??= _profileService.AddProfile(supplier.Id, supplier.SupplierCode, supplier.IsActive, supplier.IsBlacklisted, supplier.BlacklistReason, actorUserId, now);
        var evidenceId = await ValidateEvidenceAsync(request.EvidenceEvaluationId, profile.Id, cancellationToken);

        var entity = new PrcVendorStatusRequest
        {
            VendorProfileId = profile.Id,
            RequestType = type,
            Reason = reason,
            EvidenceEvaluationId = evidenceId,
            Status = Statuses.Draft,
            CreateDateTime = now.UtcDateTime,
            CreateBy = actorUserId
        };
        _dbContext.PrcVendorStatusRequests.Add(entity);
        _historyWriter.Record(ProcurementDocumentTypes.VendorStatusRequest, entity.Id, null, ProcurementDocumentHistoryActions.Create,
            null, Statuses.Draft, reason, new { supplierId = supplier.Id, requestType = type, evidenceEvaluationId = evidenceId }, actorUserId, now);

        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementVendorStatusRequest.Create", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    /// <summary>Ubah alasan/bukti saat <c>DRAFT</c>/<c>RETURNED</c>; hanya oleh pengusulnya.</summary>
    public async Task<VendorStatusRequestResponse> UpdateAsync(Guid id, UpdateVendorStatusRequestRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        var entity = await LoadAsync(id, cancellationToken);
        if (entity.Status != Statuses.Draft && entity.Status != Statuses.Returned)
            throw new ProcurementVendorConflictException("PBJ-VAL-093", InvalidTransitionMessage);
        if (entity.CreateBy != actorUserId)
            throw new ProcurementVendorForbiddenException(null, "Hanya pengusul yang dapat mengubah usulan ini.");
        if (entity.RowVersion != request.ExpectedRowVersion)
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);

        entity.Reason = RequireReason(request.Reason, "Alasan usulan wajib diisi.");
        entity.EvidenceEvaluationId = await ValidateEvidenceAsync(request.EvidenceEvaluationId, entity.VendorProfileId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        Touch(entity, actorUserId, now);
        _historyWriter.Record(ProcurementDocumentTypes.VendorStatusRequest, id, entity.RequestNumber, ProcurementDocumentHistoryActions.Update,
            entity.Status, entity.Status, null, new { entity.EvidenceEvaluationId }, actorUserId, now);

        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementVendorStatusRequest.Update", entity, actorUserId);
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Ajukan ke approval (<c>DRAFT</c>/<c>RETURNED → PENDING_APPROVAL</c>); hanya pengusul. Nomor <c>UBV</c> dialokasikan
    /// pada pengajuan pertama. Definisi tidak aktif → 409 <c>PBJ-VAL-091</c>, usulan tidak berubah.
    /// </summary>
    public async Task<VendorStatusRequestResponse> SubmitAsync(
        Guid id, SubmitVendorStatusRequestRequest? request, string? idempotencyKey, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        if (await _historyWriter.HasIdempotencyKeyAsync(
                ProcurementDocumentTypes.VendorStatusRequest, id, ProcurementDocumentHistoryActions.Submit, idempotencyKey, cancellationToken))
            return await GetByIdAsync(id, cancellationToken);

        var snapshot = await _dbContext.PrcVendorStatusRequests.AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete && !x.VendorProfile!.IsDelete)
            .Select(x => new
            {
                x.Status, x.CreateBy, x.RowVersion, x.RequestType, x.RequestNumber, x.WorkflowInstanceId, x.SubmissionCount,
                x.VendorProfileId, x.VendorProfile!.SupplierId, x.VendorProfile.Supplier!.SupplierCode, x.VendorProfile.Supplier.IsBlacklisted
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);
        if (snapshot.Status != Statuses.Draft && snapshot.Status != Statuses.Returned)
            throw new ProcurementVendorConflictException("PBJ-VAL-093", InvalidTransitionMessage);
        if (snapshot.CreateBy != actorUserId)
            throw new ProcurementVendorForbiddenException(null, "Hanya pengusul yang dapat mengajukan usulan ini.");
        if (request?.ExpectedRowVersion is Guid expected && expected != snapshot.RowVersion)
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);
        EnsureTypeMatchesStatus(snapshot.RequestType, snapshot.IsBlacklisted);

        var closed = await _approvalGateway.CloseReturnedAsync(snapshot.WorkflowInstanceId, "Usulan status vendor diajukan ulang setelah diperbaiki.", cancellationToken);
        if (!closed.Success)
            throw new ProcurementVendorConflictException("PBJ-VAL-093", closed.Message);

        var number = snapshot.RequestNumber ?? await AllocateNumberAsync(actorUserId, cancellationToken);
        var submissionCount = snapshot.SubmissionCount + 1;
        var definition = snapshot.RequestType == RequestTypeBlock
            ? ProcurementWorkflowCodes.VendorBlockDefinition
            : ProcurementWorkflowCodes.VendorUnblockDefinition;
        var approval = await _approvalGateway.SubmitAsync(
            new ProcurementApprovalRequest(
                ProcurementDocumentTypes.VendorStatusRequest, id, number, definition,
                snapshot.CreateBy, null, Array.Empty<string>(), submissionCount),
            cancellationToken);
        if (!approval.Success)
            throw new ProcurementVendorConflictException("PBJ-VAL-091", approval.ConfigurationMissing
                ? "Alur persetujuan status vendor belum dikonfigurasi. Hubungi administrator."
                : approval.Message);

        var entity = await _dbContext.PrcVendorStatusRequests.FirstAsync(x => x.Id == id, cancellationToken);
        if (entity.RowVersion != snapshot.RowVersion || (entity.Status != Statuses.Draft && entity.Status != Statuses.Returned))
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);

        var now = DateTimeOffset.UtcNow;
        var from = entity.Status;
        entity.RequestNumber = number;
        entity.Status = Statuses.PendingApproval;
        entity.SubmissionCount = submissionCount;
        entity.WorkflowInstanceId = approval.WorkflowInstanceId;
        entity.SubmittedAt = now;
        Touch(entity, actorUserId, now);
        _historyWriter.Record(ProcurementDocumentTypes.VendorStatusRequest, id, number, ProcurementDocumentHistoryActions.Submit,
            from, entity.Status, null, new { idempotencyKey = idempotencyKey?.Trim(), submissionCount, definition }, actorUserId, now, approval.WorkflowInstanceId);
        _historyWriter.Record(ProcurementDocumentTypes.Vendor, snapshot.VendorProfileId, snapshot.SupplierCode, ProcurementDocumentHistoryActions.StatusRequestSubmit,
            null, null, $"Usulan {Label(TypeLabels, entity.RequestType).ToLowerInvariant()} {number} diajukan: {entity.Reason}",
            new { vendorStatusRequestId = id }, actorUserId, now, approval.WorkflowInstanceId);

        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementVendorStatusRequest.Submit", entity, actorUserId);
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Batalkan usulan <c>DRAFT</c>/<c>RETURNED</c>, atau tarik usulan <c>PENDING_APPROVAL</c> (instance <c>Wfl</c> →
    /// <c>Withdrawn</c>; hanya pengusul). Alasan wajib (<c>PBJ-VAL-104</c>).
    /// </summary>
    public async Task<VendorStatusRequestResponse> CancelAsync(Guid id, CancelVendorStatusRequestRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        var reason = request.Reason?.Trim() ?? string.Empty;
        if (reason.Length < 10)
            throw new ProcurementVendorValidationException("PBJ-VAL-104", "Alasan wajib diisi (minimal 10 karakter).");

        var snapshot = await _dbContext.PrcVendorStatusRequests.AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete && !x.VendorProfile!.IsDelete)
            .Select(x => new { x.Status, x.CreateBy, x.RowVersion, x.WorkflowInstanceId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);
        var isWithdraw = snapshot.Status == Statuses.PendingApproval;
        if (!isWithdraw && snapshot.Status != Statuses.Draft && snapshot.Status != Statuses.Returned)
            throw new ProcurementVendorConflictException("PBJ-VAL-093", InvalidTransitionMessage);
        if (request.ExpectedRowVersion is Guid expected && expected != snapshot.RowVersion)
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);

        if (isWithdraw)
        {
            if (snapshot.CreateBy != actorUserId)
                throw new ProcurementVendorForbiddenException(null, "Hanya pengusul yang dapat menarik usulan yang sedang diproses.");
            if (!snapshot.WorkflowInstanceId.HasValue)
                throw new ProcurementVendorConflictException("PBJ-VAL-093", InvalidTransitionMessage);
            var withdrawn = await _approvalGateway.WithdrawAsync(snapshot.WorkflowInstanceId.Value, reason, cancellationToken);
            if (!withdrawn.Success)
                throw new ProcurementVendorConflictException("PBJ-VAL-093", withdrawn.Message);
        }
        else
        {
            var closed = await _approvalGateway.CloseReturnedAsync(snapshot.WorkflowInstanceId, reason, cancellationToken);
            if (!closed.Success)
                throw new ProcurementVendorConflictException("PBJ-VAL-093", closed.Message);
        }

        // Penerap status Wfl mungkin sudah membatalkan usulan saat penarikan disinkronkan; bila belum, dibatalkan di sini.
        var entity = await _dbContext.PrcVendorStatusRequests.FirstAsync(x => x.Id == id, cancellationToken);
        await _dbContext.Entry(entity).ReloadAsync(cancellationToken);
        if (entity.Status != Statuses.Cancelled)
        {
            if (entity.Status != snapshot.Status)
                throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);
            var now = DateTimeOffset.UtcNow;
            entity.Status = Statuses.Cancelled;
            entity.DecidedAt = now;
            entity.IsCancel = true;
            entity.CancelDateTime = now.UtcDateTime;
            entity.CancelBy = actorUserId;
            Touch(entity, actorUserId, now);
            _historyWriter.Record(ProcurementDocumentTypes.VendorStatusRequest, id, entity.RequestNumber,
                isWithdraw ? ProcurementDocumentHistoryActions.Withdraw : ProcurementDocumentHistoryActions.Cancel,
                snapshot.Status, entity.Status, reason, null, actorUserId, now, isWithdraw ? snapshot.WorkflowInstanceId : null);
            await SaveAsync(cancellationToken);
        }

        await AuditAsync(isWithdraw ? "ProcurementVendorStatusRequest.Withdraw" : "ProcurementVendorStatusRequest.Cancel", entity, actorUserId);
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Sinkron ulang status usulan dari instance <c>Wfl</c>-nya bila penerapan keputusan sempat gagal (PBJ-VD-INT §1).
    /// Idempoten — usulan yang sudah selaras tidak berubah dan riwayatnya tidak ganda.
    /// </summary>
    public async Task<VendorStatusRequestResponse> SynchronizeWorkflowAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        var workflowInstanceId = await _dbContext.PrcVendorStatusRequests.AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete)
            .Select(x => new { x.WorkflowInstanceId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);
        if (!workflowInstanceId.WorkflowInstanceId.HasValue)
            throw new ProcurementVendorConflictException("PBJ-VAL-093", "Usulan belum memiliki alur persetujuan.");
        await _lifecycleService.SynchronizeAsync(workflowInstanceId.WorkflowInstanceId.Value, actorUserId, allowAutoApply: true, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    // ------------------------------------------------------------------ hasil approval

    /// <summary>
    /// Menerapkan status <c>Wfl</c> ke usulan (STATE bagian 3). Hanya usulan <c>PENDING_APPROVAL</c> yang tertaut ke instance ini
    /// yang dipindahkan; pemanggilan ulang tidak mengubah apa pun. Disetujui → <see cref="SupplierProcurementStatusService"/>
    /// menulis status blokir dalam transaksi yang sama, dan riwayat vendor bertambah satu baris keputusan.
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

        var entity = await _dbContext.PrcVendorStatusRequests
            .Include(x => x.VendorProfile!).ThenInclude(x => x.Supplier)
            .FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken);
        if (entity == null) return result;
        result.PreviousReferenceStatus = entity.Status;
        result.CurrentReferenceStatus = entity.Status;

        var target = ToRequestStatus(workflow.WorkflowStatus);
        if (entity.WorkflowInstanceId != workflow.Id || entity.Status != Statuses.PendingApproval || target == null || target == entity.Status)
            return result;

        var actor = actorUserId == Guid.Empty ? workflow.RequestedByUserId : actorUserId;
        var now = DateTimeOffset.UtcNow;
        var supplierCode = entity.VendorProfile?.Supplier?.SupplierCode;
        var ownTransaction = _dbContext.Database.CurrentTransaction == null
            ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;
        try
        {
            if (target == Statuses.Approved)
            {
                var block = entity.RequestType == RequestTypeBlock;
                await _supplierStatusService.ApplyAsync(entity.VendorProfile!.SupplierId, block, entity.Reason, entity.Id, actor, cancellationToken);
                _historyWriter.Record(ProcurementDocumentTypes.Vendor, entity.VendorProfileId, supplierCode,
                    block ? ProcurementDocumentHistoryActions.VendorBlock : ProcurementDocumentHistoryActions.VendorUnblock,
                    block ? "ACTIVE" : "BLOCKED", block ? "BLOCKED" : "ACTIVE",
                    $"Usulan {entity.RequestNumber} disetujui: {entity.Reason}", new { vendorStatusRequestId = entity.Id }, actor, now, workflow.Id);
            }
            else if (target == Statuses.Rejected)
            {
                _historyWriter.Record(ProcurementDocumentTypes.Vendor, entity.VendorProfileId, supplierCode, ProcurementDocumentHistoryActions.StatusRequestReject,
                    null, null, $"Usulan {entity.RequestNumber} ditolak.", new { vendorStatusRequestId = entity.Id }, actor, now, workflow.Id);
            }

            var from = entity.Status;
            entity.Status = target;
            if (target != Statuses.Returned) entity.DecidedAt = now;
            if (target == Statuses.Cancelled)
            {
                entity.IsCancel = true;
                entity.CancelDateTime = now.UtcDateTime;
                entity.CancelBy = actor;
            }
            Touch(entity, actor, now);
            _historyWriter.Record(ProcurementDocumentTypes.VendorStatusRequest, entity.Id, entity.RequestNumber, WorkflowActionOf(target),
                from, target, workflow.CancellationReason, new { workflow.WorkflowStatus }, actor, now, workflow.Id);
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (ownTransaction != null) await ownTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            if (ownTransaction != null) await ownTransaction.DisposeAsync();
        }

        result.CurrentReferenceStatus = entity.Status;
        result.StatusChanged = true;
        await AuditAsync("ProcurementVendorStatusRequest.WorkflowSync", entity, actor);
        return result;
    }

    /// <summary>Pemetaan STATE bagian 3: Approved/FinalApproved/Completed → APPROVED; Rejected → REJECTED; Returned/RevisionRequested → RETURNED; Withdrawn/Cancelled → CANCELLED.</summary>
    private static string? ToRequestStatus(string? workflowStatus)
    {
        var status = workflowStatus?.Trim() ?? string.Empty;
        bool Eq(string value) => string.Equals(status, value, StringComparison.OrdinalIgnoreCase);
        if (Eq(WorkflowValueConstants.WorkflowStatus.Approved) || Eq(WorkflowValueConstants.WorkflowStatus.Completed) || Eq("FinalApproved"))
            return Statuses.Approved;
        if (Eq(WorkflowValueConstants.WorkflowStatus.Rejected)) return Statuses.Rejected;
        if (Eq(WorkflowValueConstants.WorkflowStatus.Returned) || Eq(WorkflowValueConstants.WorkflowStatus.RevisionRequested)) return Statuses.Returned;
        if (Eq(WorkflowValueConstants.WorkflowStatus.Withdrawn) || Eq(WorkflowValueConstants.WorkflowStatus.Cancelled)) return Statuses.Cancelled;
        return null;
    }

    private static string WorkflowActionOf(string status) => status switch
    {
        Statuses.Approved => ProcurementDocumentHistoryActions.WorkflowApproved,
        Statuses.Rejected => ProcurementDocumentHistoryActions.WorkflowRejected,
        Statuses.Cancelled => ProcurementDocumentHistoryActions.Withdraw,
        _ => ProcurementDocumentHistoryActions.WorkflowReturned
    };

    // ------------------------------------------------------------------ bantu

    private static void EnsureTypeMatchesStatus(string type, bool isBlacklisted)
    {
        if (type == RequestTypeBlock && isBlacklisted)
            throw new ProcurementVendorConflictException("PBJ-VAL-106", "Vendor sudah diblokir.");
        if (type == RequestTypeUnblock && !isBlacklisted)
            throw new ProcurementVendorConflictException("PBJ-VAL-106", "Vendor tidak sedang diblokir.");
    }

    private async Task EnsureNoOpenRequestAsync(Guid profileId, Guid? exceptId, CancellationToken cancellationToken)
    {
        var open = await _dbContext.PrcVendorStatusRequests.AsNoTracking()
            .Where(x => x.VendorProfileId == profileId && x.Id != exceptId && !x.IsDelete
                && (x.Status == Statuses.Draft || x.Status == Statuses.PendingApproval || x.Status == Statuses.Returned))
            .Select(x => new { x.RequestNumber })
            .FirstOrDefaultAsync(cancellationToken);
        if (open != null)
            throw new ProcurementVendorConflictException("PBJ-VAL-107",
                $"Masih ada usulan {open.RequestNumber ?? "(draf)"} yang belum selesai untuk vendor ini.");
    }

    private async Task<Guid?> ValidateEvidenceAsync(Guid? evaluationId, Guid profileId, CancellationToken cancellationToken)
    {
        if (!evaluationId.HasValue) return null;
        var ok = await _dbContext.PrcVendorEvaluations.AnyAsync(x =>
            x.Id == evaluationId.Value && x.VendorProfileId == profileId && !x.IsDelete
            && x.Status == ProcurementStatuses.VendorEvaluation.Final, cancellationToken);
        if (!ok)
            throw new ProcurementVendorValidationException(null, "Bukti harus penilaian final milik vendor yang sama.");
        return evaluationId;
    }

    private async Task<string> AllocateNumberAsync(Guid actorUserId, CancellationToken cancellationToken)
    {
        try
        {
            return await _numberAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: ProcurementNumberSeries.VendorStatusRequestSequenceKey,
                    Prefix: ProcurementNumberSeries.VendorStatusRequestPrefix,
                    ResetPolicy: ProcurementNumberSeries.ResetPolicy,
                    SequenceDigits: ProcurementNumberSeries.SequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow),
                cancellationToken);
        }
        catch (NumberSeriesAllocationException)
        {
            throw new ProcurementVendorConflictException("PBJ-VAL-093", "Nomor usulan gagal dibuat. Silakan coba lagi.");
        }
    }

    private async Task<PrcVendorStatusRequest> LoadAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.PrcVendorStatusRequests
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete && !x.VendorProfile!.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException(NotFoundMessage);

    private static string RequireReason(string? reason, string emptyMessage)
    {
        var trimmed = reason?.Trim() ?? string.Empty;
        if (trimmed.Length == 0) throw new ProcurementVendorValidationException(null, emptyMessage);
        if (trimmed.Length > 1000) throw new ProcurementVendorValidationException(null, "Alasan maksimal 1000 karakter.");
        return trimmed;
    }

    private static string? NormalizeOption(string? value, string label, IEnumerable<string> allowed)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim().ToUpperInvariant();
        if (!allowed.Contains(normalized))
            throw new ProcurementVendorValidationException(null, $"{label} '{value.Trim()}' tidak dikenal.");
        return normalized;
    }

    private static string Label(IReadOnlyDictionary<string, string> labels, string value) =>
        labels.TryGetValue(value, out var label) ? label : value;

    private static void RequireActor(Guid actorUserId)
    {
        if (actorUserId == Guid.Empty)
            throw new ProcurementVendorForbiddenException(null, "Sesi pengguna tidak dikenali. Silakan masuk ulang.");
    }

    private static void Touch(PrcVendorStatusRequest entity, Guid actorUserId, DateTimeOffset now)
    {
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateDateTime = now.UtcDateTime;
        entity.UpdateBy = actorUserId;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);
        }
        catch (DbUpdateException exception) when ((exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // Dua usulan terbuka dibuat bersamaan; index unik parsial menahan yang kedua.
            throw new ProcurementVendorConflictException("PBJ-VAL-107", "Masih ada usulan yang belum selesai untuk vendor ini.");
        }
    }

    private Task AuditAsync(string action, PrcVendorStatusRequest entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan usulan status vendor.", new
        {
            EntityId = entity.Id,
            entity.VendorProfileId,
            entity.RequestType,
            entity.Status,
            ActorUserId = actorUserId
        });
}

/// <summary>Penyedia hak dokumen <c>PBJ_VENDOR_STATUS</c> untuk lampiran dan linimasa persetujuan.</summary>
public sealed class VendorStatusRequestAccessProvider : IProcurementDocumentAccessProvider
{
    private readonly ProcurementVendorStatusService _service;

    public VendorStatusRequestAccessProvider(ProcurementVendorStatusService service) => _service = service;

    public string DocumentType => ProcurementDocumentTypes.VendorStatusRequest;

    public Task<ProcurementDocumentAccessLevel> GetAccessAsync(ClaimsPrincipal user, Guid documentId, CancellationToken cancellationToken) =>
        _service.GetAccessLevelAsync(user, documentId, cancellationToken);
}

/// <summary>
/// Penerap status <c>Wfl</c> untuk usulan status vendor (<c>PBJ_VENDOR_STATUS</c>, BE-PBJ-VD-007). Service diambil saat
/// dipanggil agar tidak membentuk lingkaran dependensi dengan <c>WorkflowService</c>.
/// </summary>
public sealed class VendorStatusRequestWorkflowStatusApplier : IProcurementWorkflowStatusApplier
{
    private readonly IServiceProvider _serviceProvider;

    public VendorStatusRequestWorkflowStatusApplier(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public string DocumentType => ProcurementDocumentTypes.VendorStatusRequest;

    public Task<WorkflowReferenceLifecycleSynchronizationResult> ApplyAsync(
        TrxWorkflowInstance workflow, Guid actorUserId, CancellationToken cancellationToken) =>
        _serviceProvider.GetRequiredService<ProcurementVendorStatusService>()
            .ApplyWorkflowStatusAsync(workflow, actorUserId, cancellationToken);
}
