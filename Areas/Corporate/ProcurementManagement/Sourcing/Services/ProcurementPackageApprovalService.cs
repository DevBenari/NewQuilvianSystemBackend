using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Services;

/// <summary>
/// Persetujuan persiapan paket lewat <c>Wfl</c> (BE-PBJ-PS-008; PBJ-PS-API 1.0 <c>preparation/*</c>, <c>workflow/synchronize</c>;
/// PBJ-PS-STATE 1.1 bagian 1; PBJ-PS-VAL <c>PBJ-VAL-090</c>, <c>091</c>, <c>124</c>..<c>128</c>, <c>148</c>; PBJ-DEC-072, 073, 097).
/// </summary>
/// <remarks>
/// <para><b>Ajukan.</b> Diperiksa berurutan: status persiapan, versi data, HPS &gt; 0 setiap barang (<c>PBJ-VAL-124</c>), metode
/// aktif (<c>125</c>), alasan bila HPS di luar rentang salinan metode (<c>126</c>), checklist wajib terlampir (<c>127</c>). Nomor
/// <c>PKT-…</c> dialokasikan saat pertama diajukan. <c>Wfl</c> lebih dulu, baru status paket; definisi
/// <c>PBJ-PKT-PERSIAPAN</c> tidak aktif → 409 <c>PBJ-VAL-091</c> dan paket tetap <c>DRAFT</c> (PBJ-DEC-097). Metode di luar
/// rentang mengirim kondisi <c>PBJ_METHOD_OUT_OF_RANGE</c>, sehingga matriks menambah langkah. Contoh: PKT-000012 HPS
/// Rp 42.750.000 dengan Pembandingan Penawaran → dua langkah persetujuan.</para>
/// <para><b>Hasil <c>Wfl</c></b> (<see cref="ApplyWorkflowStatusAsync"/>): disetujui → <c>READY_FOR_RFQ</c> dan HPS disetujui
/// dikunci (<c>ApprovedHpsTotal</c>); dikembalikan → <c>PREPARATION_RETURNED</c>; ditolak → <c>REJECTED</c> dan seluruh baris
/// PR dilepas (<c>ReleaseLinesAsync</c>); ditarik → <c>DRAFT</c>.</para>
/// <para><b>Buka ulang</b> paket <c>READY_FOR_RFQ</c>/<c>EVALUATION</c> → <c>DRAFT</c> dengan alasan wajib; RFQ yang masih
/// <c>DRAFT</c>/<c>SENT</c> dibatalkan, riwayat RFQ tetap.</para>
/// </remarks>
public sealed class ProcurementPackageApprovalService
{
    private const string LogCategory = "Corporate.ProcurementManagement.Sourcing";
    private const string NotFoundMessage = "Paket tidak ditemukan atau di luar cakupan Anda.";
    private const string StaleMessage = "Data sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda.";
    private const string ConfigurationMissingMessage = "Alur persetujuan pengadaan belum dikonfigurasi. Hubungi administrator.";

    private static readonly HashSet<string> ReopenableStatuses = new(StringComparer.Ordinal)
    {
        ProcurementStatuses.Package.ReadyForRfq,
        ProcurementStatuses.Package.Evaluation
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly ProcurementPackageService _packageService;
    private readonly ProcurementPurchaseRequestService _purchaseRequestService;
    private readonly ProcurementApprovalGateway _approvalGateway;
    private readonly WorkflowReferenceLifecycleService _lifecycleService;
    private readonly ProcurementDocumentHistoryWriter _historyWriter;
    private readonly NumberSeriesAllocator _numberAllocator;
    private readonly LoggerService _loggerService;

    public ProcurementPackageApprovalService(
        ApplicationDbContext dbContext,
        ProcurementPackageService packageService,
        ProcurementPurchaseRequestService purchaseRequestService,
        ProcurementApprovalGateway approvalGateway,
        WorkflowReferenceLifecycleService lifecycleService,
        ProcurementDocumentHistoryWriter historyWriter,
        NumberSeriesAllocator numberAllocator,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _packageService = packageService;
        _purchaseRequestService = purchaseRequestService;
        _approvalGateway = approvalGateway;
        _lifecycleService = lifecycleService;
        _historyWriter = historyWriter;
        _numberAllocator = numberAllocator;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------ ajukan, tarik, buka ulang

    public async Task<PackageResponse> SubmitAsync(
        ClaimsPrincipal user, Guid id, SubmitPackagePreparationRequest request, string? idempotencyKey, CancellationToken cancellationToken)
    {
        var actorUserId = ProcurementDataScopeService.GetUserId(user);
        await EnsureInScopeAsync(user, id, cancellationToken);
        if (await _historyWriter.HasIdempotencyKeyAsync(
                ProcurementDocumentTypes.Package, id, ProcurementDocumentHistoryActions.Submit, idempotencyKey, cancellationToken))
            return await _packageService.GetByIdAsync(user, id, cancellationToken);

        var package = await _dbContext.PrcPackages.AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);
        if (!ProcurementStatuses.Package.IsPreparationEditable(package.Status))
            throw StatusConflict(package.Status);
        if (package.RowVersion != request.ExpectedRowVersion)
            throw new ProcurementSourcingConflictException("PBJ-VAL-090", StaleMessage);

        var conditionCodes = await ValidateForSubmissionAsync(package, cancellationToken);

        // Wfl lebih dulu, sebelum entity apa pun dilacak (Wfl menyimpan context yang sama).
        var closed = await _approvalGateway.CloseReturnedAsync(
            package.PreparationWorkflowInstanceId, "Persiapan paket diajukan ulang setelah diperbaiki.", cancellationToken);
        if (!closed.Success)
            throw new ProcurementSourcingConflictException("PBJ-VAL-128", closed.Message);

        var number = package.PackageNumber ?? await AllocateNumberAsync(actorUserId, cancellationToken);
        var submissionCount = package.PreparationSubmissionCount + 1;
        var approval = await _approvalGateway.SubmitAsync(
            new ProcurementApprovalRequest(
                ProcurementDocumentTypes.Package, package.Id, number, ProcurementWorkflowCodes.PackagePreparationDefinition,
                actorUserId, package.HpsTotal, conditionCodes, submissionCount),
            cancellationToken);
        if (!approval.Success)
            throw new ProcurementSourcingConflictException(
                "PBJ-VAL-091", approval.ConfigurationMissing ? ConfigurationMissingMessage : approval.Message);

        var entity = await _dbContext.PrcPackages.FirstAsync(x => x.Id == id, cancellationToken);
        await _dbContext.Entry(entity).ReloadAsync(cancellationToken);
        if (entity.RowVersion != package.RowVersion || !ProcurementStatuses.Package.IsPreparationEditable(entity.Status))
            throw new ProcurementSourcingConflictException("PBJ-VAL-090", StaleMessage);

        var now = DateTimeOffset.UtcNow;
        var fromStatus = entity.Status;
        entity.PackageNumber = number;
        entity.Status = ProcurementStatuses.Package.PreparationPendingApproval;
        entity.PreparationSubmissionCount = submissionCount;
        entity.PreparationWorkflowInstanceId = approval.WorkflowInstanceId;
        entity.PreparationSubmittedAt = now;
        Touch(entity, actorUserId, now);
        _historyWriter.Record(
            ProcurementDocumentTypes.Package, entity.Id, number, ProcurementDocumentHistoryActions.Submit,
            fromStatus, entity.Status, null,
            new { idempotencyKey = idempotencyKey?.Trim(), submissionCount, conditionCodes, entity.HpsTotal },
            actorUserId, now, approval.WorkflowInstanceId);
        await SaveAsync(cancellationToken);

        await AuditAsync("ProcurementPackage.SubmitPreparation", entity, actorUserId);
        return await _packageService.GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>Tarik pengajuan yang belum diputus; hanya pengaju (pemohon instance <c>Wfl</c>).</summary>
    public async Task<PackageResponse> WithdrawAsync(
        ClaimsPrincipal user, Guid id, WithdrawPackagePreparationRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = ProcurementDataScopeService.GetUserId(user);
        await EnsureInScopeAsync(user, id, cancellationToken);
        var package = await _dbContext.PrcPackages.AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);
        if (package.Status != ProcurementStatuses.Package.PreparationPendingApproval || !package.PreparationWorkflowInstanceId.HasValue)
            throw StatusConflict(package.Status);
        if (package.RowVersion != request.ExpectedRowVersion)
            throw new ProcurementSourcingConflictException("PBJ-VAL-090", StaleMessage);
        var requester = await _dbContext.Set<TrxWorkflowInstance>().AsNoTracking()
            .Where(x => x.Id == package.PreparationWorkflowInstanceId.Value)
            .Select(x => x.RequestedByUserId)
            .FirstOrDefaultAsync(cancellationToken);
        if (requester != actorUserId)
            throw new ProcurementSourcingForbiddenException("Hanya pengaju yang dapat menarik pengajuan persiapan ini.");

        var reason = string.IsNullOrWhiteSpace(request.Reason) ? "Pengajuan persiapan ditarik pengaju." : request.Reason.Trim();
        var withdrawn = await _approvalGateway.WithdrawAsync(package.PreparationWorkflowInstanceId.Value, reason, cancellationToken);
        if (!withdrawn.Success)
            throw new ProcurementSourcingConflictException("PBJ-VAL-128", withdrawn.Message);

        // Penerap status mungkin sudah memindahkan paket saat penarikan disinkronkan; bila belum, dipindahkan di sini.
        var entity = await _dbContext.PrcPackages.FirstAsync(x => x.Id == id, cancellationToken);
        await _dbContext.Entry(entity).ReloadAsync(cancellationToken);
        if (entity.Status != ProcurementStatuses.Package.Draft)
        {
            if (entity.Status != ProcurementStatuses.Package.PreparationPendingApproval)
                throw new ProcurementSourcingConflictException("PBJ-VAL-090", StaleMessage);
            var now = DateTimeOffset.UtcNow;
            entity.Status = ProcurementStatuses.Package.Draft;
            Touch(entity, actorUserId, now);
            _historyWriter.Record(
                ProcurementDocumentTypes.Package, entity.Id, entity.PackageNumber, ProcurementDocumentHistoryActions.Withdraw,
                ProcurementStatuses.Package.PreparationPendingApproval, entity.Status, reason, null, actorUserId, now,
                entity.PreparationWorkflowInstanceId);
            await SaveAsync(cancellationToken);
        }

        await AuditAsync("ProcurementPackage.WithdrawPreparation", entity, actorUserId);
        return await _packageService.GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>
    /// Buka ulang persiapan yang sudah disetujui agar HPS/metode dapat diubah lalu diajukan ulang (PBJ-DEC-073). RFQ terbuka
    /// dibatalkan dengan alasan yang sama; riwayatnya tetap.
    /// </summary>
    public async Task<PackageResponse> ReopenAsync(
        ClaimsPrincipal user, Guid id, ReopenPackagePreparationRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = ProcurementDataScopeService.GetUserId(user);
        await EnsureInScopeAsync(user, id, cancellationToken);
        var entity = await _dbContext.PrcPackages.FirstAsync(x => x.Id == id, cancellationToken);
        if (!ReopenableStatuses.Contains(entity.Status))
            throw StatusConflict(entity.Status);
        if (entity.RowVersion != request.ExpectedRowVersion)
            throw new ProcurementSourcingConflictException("PBJ-VAL-090", StaleMessage);
        var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
        if (reason == null)
            throw new ProcurementSourcingValidationException("PBJ-VAL-148", "Tuliskan alasannya.");

        var now = DateTimeOffset.UtcNow;
        var openRfqs = await _dbContext.PrcRfqs
            .Where(x => x.PackageId == id && !x.IsDelete
                && (x.Status == ProcurementStatuses.Rfq.Draft || x.Status == ProcurementStatuses.Rfq.Sent))
            .ToListAsync(cancellationToken);
        foreach (var rfq in openRfqs)
        {
            var rfqFrom = rfq.Status;
            rfq.Status = ProcurementStatuses.Rfq.Cancelled;
            rfq.ClosedAt = now;
            rfq.RowVersion = Guid.NewGuid();
            rfq.UpdateBy = actorUserId;
            rfq.UpdateDateTime = now.UtcDateTime;
            _historyWriter.Record(
                ProcurementDocumentTypes.Rfq, rfq.Id, rfq.RfqNumber, ProcurementDocumentHistoryActions.Cancel,
                rfqFrom, rfq.Status, reason, new { PackageId = id, Cause = ProcurementDocumentHistoryActions.Reopen }, actorUserId, now);
        }

        var fromStatus = entity.Status;
        entity.Status = ProcurementStatuses.Package.Draft;
        Touch(entity, actorUserId, now);
        _historyWriter.Record(
            ProcurementDocumentTypes.Package, entity.Id, entity.PackageNumber, ProcurementDocumentHistoryActions.Reopen,
            fromStatus, entity.Status, reason,
            new { CancelledRfqIds = openRfqs.Select(x => x.Id).ToList(), entity.ApprovedHpsTotal }, actorUserId, now);
        await SaveAsync(cancellationToken);

        await AuditAsync("ProcurementPackage.ReopenPreparation", entity, actorUserId);
        return await _packageService.GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>Sinkron ulang hasil <c>Wfl</c> yang gagal diterapkan (pola <c>BE-PBJ-008</c>).</summary>
    public async Task<PackageResponse> SynchronizeWorkflowAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var actorUserId = ProcurementDataScopeService.GetUserId(user);
        await EnsureInScopeAsync(user, id, cancellationToken);
        var workflowInstanceId = await _dbContext.PrcPackages.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => x.PreparationWorkflowInstanceId)
            .FirstAsync(cancellationToken);
        if (!workflowInstanceId.HasValue)
            throw new ProcurementSourcingConflictException("PBJ-VAL-128", "Paket belum memiliki alur persetujuan.");

        await _lifecycleService.SynchronizeAsync(workflowInstanceId.Value, actorUserId, allowAutoApply: true, cancellationToken);
        return await _packageService.GetByIdAsync(user, id, cancellationToken);
    }

    // ------------------------------------------------------------------ hasil approval

    /// <summary>
    /// Menerapkan status <c>Wfl</c> ke paket (PBJ-PS-STATE bagian 1). Hanya paket <c>PREPARATION_PENDING_APPROVAL</c> yang
    /// tertaut ke instance ini yang dipindahkan; pemanggilan ulang tidak mengubah apa pun dan tidak menambah riwayat.
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

        var package = await _dbContext.PrcPackages.FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken);
        if (package == null) return result;
        result.PreviousReferenceStatus = package.Status;
        result.CurrentReferenceStatus = package.Status;

        var target = ToPackageStatus(workflow.WorkflowStatus);
        if (package.PreparationWorkflowInstanceId != workflow.Id ||
            package.Status != ProcurementStatuses.Package.PreparationPendingApproval ||
            target == null || target == package.Status)
            return result;

        var actor = actorUserId == Guid.Empty ? workflow.RequestedByUserId : actorUserId;
        var now = DateTimeOffset.UtcNow;
        var ownTransaction = _dbContext.Database.CurrentTransaction == null
            ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;
        try
        {
            string action;
            switch (target)
            {
                case ProcurementStatuses.Package.ReadyForRfq:
                    package.ApprovedHpsTotal = package.HpsTotal;
                    package.PreparationApprovedAt = now;
                    action = ProcurementDocumentHistoryActions.WorkflowApproved;
                    break;
                case ProcurementStatuses.Package.Rejected:
                    await ReleaseAllSourcesAsync(package, actor, now, workflow.CancellationReason, cancellationToken);
                    action = ProcurementDocumentHistoryActions.WorkflowRejected;
                    break;
                case ProcurementStatuses.Package.Draft:
                    action = ProcurementDocumentHistoryActions.Withdraw;
                    break;
                default:
                    action = ProcurementDocumentHistoryActions.WorkflowReturned;
                    break;
            }

            var from = package.Status;
            package.Status = target;
            Touch(package, actor, now);
            _historyWriter.Record(
                ProcurementDocumentTypes.Package, package.Id, package.PackageNumber, action, from, target,
                workflow.CancellationReason, new { workflow.WorkflowStatus }, actor, now, workflow.Id);
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (ownTransaction != null) await ownTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            if (ownTransaction != null) await ownTransaction.DisposeAsync();
        }

        result.CurrentReferenceStatus = package.Status;
        result.StatusChanged = true;
        await AuditAsync("ProcurementPackage.WorkflowSync", package, actor);
        return result;
    }

    /// <summary>
    /// Pemetaan status <c>Wfl</c> → status paket. Contoh: <c>Approved</c> → <c>READY_FOR_RFQ</c>; <c>RevisionRequested</c> →
    /// <c>PREPARATION_RETURNED</c>; <c>Withdrawn</c> → <c>DRAFT</c>. <c>null</c> = tidak memindahkan paket.
    /// </summary>
    public static string? ToPackageStatus(string? workflowStatus)
    {
        var status = workflowStatus?.Trim() ?? string.Empty;
        if (Eq(status, WorkflowValueConstants.WorkflowStatus.Approved) || Eq(status, WorkflowValueConstants.WorkflowStatus.Completed))
            return ProcurementStatuses.Package.ReadyForRfq;
        if (Eq(status, WorkflowValueConstants.WorkflowStatus.Rejected))
            return ProcurementStatuses.Package.Rejected;
        if (Eq(status, WorkflowValueConstants.WorkflowStatus.Returned) || Eq(status, WorkflowValueConstants.WorkflowStatus.RevisionRequested))
            return ProcurementStatuses.Package.PreparationReturned;
        if (Eq(status, WorkflowValueConstants.WorkflowStatus.Withdrawn) || Eq(status, WorkflowValueConstants.WorkflowStatus.Cancelled))
            return ProcurementStatuses.Package.Draft;
        return null;
    }

    // ------------------------------------------------------------------ bantuan

    /// <summary>Syarat pengajuan; mengembalikan kode kondisi <c>Wfl</c>.</summary>
    private async Task<string[]> ValidateForSubmissionAsync(PrcPackage package, CancellationToken cancellationToken)
    {
        var items = await _dbContext.PrcPackageItems.AsNoTracking()
            .Where(x => x.PackageId == package.Id && !x.IsDelete)
            .Select(x => new { x.LineNo, x.HpsUnitPrice })
            .ToListAsync(cancellationToken);
        if (items.Count == 0 || items.Any(x => x.HpsUnitPrice <= 0))
            throw new ProcurementSourcingValidationException("PBJ-VAL-124", "Lengkapi HPS setiap barang sebelum mengajukan.");

        var methodActive = package.MethodId.HasValue && await _dbContext.MstProcurementMethods.AsNoTracking()
            .AnyAsync(x => x.Id == package.MethodId.Value && x.IsActive && !x.IsDelete, cancellationToken);
        if (!methodActive)
            throw new ProcurementSourcingValidationException("PBJ-VAL-125", "Pilih metode pengadaan yang aktif.");

        var outOfRange = !ProcurementPackagePreparationService.IsInRange(
            package.HpsTotal, package.MethodMinHpsAmount, package.MethodMaxHpsAmount);
        if (outOfRange && string.IsNullOrWhiteSpace(package.MethodDeviationReason))
            throw new ProcurementSourcingValidationException("PBJ-VAL-126", "Nilai HPS di luar rentang metode ini. Tuliskan alasannya.");

        var attached = await _dbContext.PrcAttachments.AsNoTracking()
            .Where(a => a.DocumentType == ProcurementDocumentTypes.Package && a.DocumentId == package.Id && a.IsCurrent
                && !a.IsCancel && !a.IsDelete && a.AttachmentCategory != null)
            .Select(a => a.AttachmentCategory!)
            .Distinct()
            .ToListAsync(cancellationToken);
        var missing = await _dbContext.PrcPackageChecklistItems.AsNoTracking()
            .Where(x => x.PackageId == package.Id && !x.IsDelete && x.IsRequired && !attached.Contains(x.DocumentTypeCode))
            .OrderBy(x => x.DocumentTypeCode)
            .Select(x => x.DocumentTypeName)
            .ToListAsync(cancellationToken);
        if (missing.Count > 0)
            throw new ProcurementSourcingValidationException("PBJ-VAL-127", $"Dokumen wajib belum lengkap: {string.Join(", ", missing)}.");

        return outOfRange ? [ProcurementWorkflowCodes.MethodOutOfRangeCondition] : [];
    }

    /// <summary>Paket ditolak: semua sumber dilepas dan baris PR kembali siap dipaketkan (PBJ-PS-STATE bagian 1).</summary>
    private async Task ReleaseAllSourcesAsync(
        PrcPackage package, Guid actorUserId, DateTimeOffset now, string? reason, CancellationToken cancellationToken)
    {
        var sources = await _dbContext.PrcPackageItemSources
            .Where(s => !s.IsDelete && !s.IsReleased && s.PackageItem!.PackageId == package.Id)
            .ToListAsync(cancellationToken);
        foreach (var source in sources)
        {
            source.IsReleased = true;
            source.ReleasedAt = now;
            source.UpdateBy = actorUserId;
            source.UpdateDateTime = now.UtcDateTime;
        }
        await _purchaseRequestService.ReleaseLinesAsync(
            sources.Select(x => x.PurchaseRequestLineId).ToList(), package.Id, actorUserId, now,
            reason ?? "Persiapan paket ditolak.", cancellationToken);
    }

    private async Task EnsureInScopeAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        if (!await _packageService.IsInScopeAsync(user, id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
    }

    private static ProcurementSourcingConflictException StatusConflict(string status) =>
        new("PBJ-VAL-128", $"Tindakan ini tidak dapat dilakukan pada status {ProcurementPackageService.LabelOf(status)}.");

    private async Task<string> AllocateNumberAsync(Guid actorUserId, CancellationToken cancellationToken)
    {
        try
        {
            return await _numberAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: ProcurementNumberSeries.PackageSequenceKey,
                    Prefix: ProcurementNumberSeries.PackagePrefix,
                    ResetPolicy: ProcurementNumberSeries.ResetPolicy,
                    SequenceDigits: ProcurementNumberSeries.SequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow),
                cancellationToken);
        }
        catch (NumberSeriesAllocationException)
        {
            throw new ProcurementSourcingConflictException("PBJ-VAL-128", "Nomor paket gagal dibuat. Silakan coba lagi.");
        }
    }

    private static void Touch(PrcPackage package, Guid actorUserId, DateTimeOffset now)
    {
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
    }

    private static bool Eq(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    /// <summary>Jejak audit tanpa field harga (PBJ-PS-PERM bagian 6).</summary>
    private Task AuditAsync(string action, PrcPackage package, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Persetujuan persiapan paket pengadaan.", new
        {
            EntityId = package.Id,
            package.PackageNumber,
            package.Status,
            ActorUserId = actorUserId
        });
}

/// <summary>
/// Penerap status <c>Wfl</c> untuk paket <c>PBJ_PACKAGE</c> (BE-PBJ-PS-008; arsitektur: <c>ProcurementSourcingStatusApplier</c>).
/// Service diambil saat dipanggil agar tidak membentuk lingkaran dependensi dengan <c>WorkflowService</c>. Penetapan
/// <c>PBJ_PACKAGE_AWARD</c> menyusul di BE-PBJ-PS-013.
/// </summary>
public sealed class ProcurementSourcingStatusApplier : IProcurementWorkflowStatusApplier
{
    private readonly IServiceProvider _serviceProvider;

    public ProcurementSourcingStatusApplier(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public string DocumentType => ProcurementDocumentTypes.Package;

    public Task<WorkflowReferenceLifecycleSynchronizationResult> ApplyAsync(
        TrxWorkflowInstance workflow, Guid actorUserId, CancellationToken cancellationToken) =>
        _serviceProvider.GetRequiredService<ProcurementPackageApprovalService>()
            .ApplyWorkflowStatusAsync(workflow, actorUserId, cancellationToken);
}
