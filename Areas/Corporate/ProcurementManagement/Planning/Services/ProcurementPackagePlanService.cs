using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Services;

/// <summary>
/// Rencana paket hasil konsolidasi — draf, ajukan, batal, dan penerapan hasil approval (BE-PBJ-011;
/// <c>state-transition-matrix.md</c> bagian 3, <c>validation-matrix.md</c> PBJ-VAL-050/051).
/// </summary>
/// <remarks>
/// <para><b>Alokasi (PBJ-VAL-050).</b> Σ alokasi seluruh rencana paket yang masih hidup untuk satu baris
/// rencana tidak boleh melebihi kuantitasnya. Contoh: baris rencana 10, paket A 6 → paket B paling banyak
/// 4; paket B 5 ditolak. Pemeriksaan saat simpan berjalan di bawah kunci per <c>LineKey</c>.</para>
/// <para><b>Satu jenis (PBJ-VAL-051).</b> Semua baris berjenis pengadaan sama dengan kepala paket.</para>
/// <para><b>Cakupan.</b> Rencana paket lintas unit: pengguna tanpa <c>ProcurementPackagePlan : ReadAllUnits</c>
/// hanya melihat paket yang memuat baris unit penempatannya.</para>
/// </remarks>
public sealed class ProcurementPackagePlanService
{
    private const string LogCategory = "Corporate.ProcurementManagement.Planning";
    private const string NotFoundMessage = "Rencana paket tidak ditemukan atau di luar cakupan Anda.";
    private const string InvalidTransitionMessage = "Status rencana paket tidak memungkinkan tindakan ini.";
    private const string StaleMessage = "Data sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda.";

    private readonly ApplicationDbContext _dbContext;
    private readonly ProcurementDataScopeService _scopeService;
    private readonly ProcurementDocumentHistoryWriter _historyWriter;
    private readonly ProcurementPlanQuotaService _quotaService;
    private readonly ProcurementApprovalGateway _approvalGateway;
    private readonly NumberSeriesAllocator _numberAllocator;
    private readonly LoggerService _loggerService;

    public ProcurementPackagePlanService(
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

    // ------------------------------------------------------------------ baca

    public PackagePlanFilterMetadataResponse GetFilterMetadata()
    {
        var year = DateTime.UtcNow.Year;
        return new PackagePlanFilterMetadataResponse
        {
            DefaultFilter = new PackagePlanQuery(),
            StatusOptions = ProcurementStatuses.Plan.Labels
                .Where(x => x.Key != ProcurementStatuses.Plan.Superseded)
                .Select(x => new PackagePlanOptionValue { Value = x.Key, Label = x.Value }).ToList(),
            SortOptions =
            [
                new() { Value = "createdAt", Label = "Tanggal dibuat" },
                new() { Value = "packagePlanNumber", Label = "Nomor rencana paket" },
                new() { Value = "estimatedValue", Label = "Nilai estimasi" },
                new() { Value = "targetMonth", Label = "Bulan target" }
            ],
            FiscalYearOptions = Enumerable.Range(year - 1, 4).ToList()
        };
    }

    public async Task<PackagePlanSummaryResponse> GetSummaryAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var byStatus = await query
            .GroupBy(x => x.Status)
            .Select(g => new PackagePlanStatusCount { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        return new PackagePlanSummaryResponse
        {
            Total = byStatus.Sum(x => x.Count),
            PendingApproval = byStatus.Where(x => x.Status == ProcurementStatuses.Plan.PendingApproval).Sum(x => x.Count),
            ApprovedValue = await query.Where(x => x.Status == ProcurementStatuses.Plan.Approved).SumAsync(x => x.EstimatedValue, cancellationToken),
            ByStatus = byStatus.OrderBy(x => x.Status).ToList()
        };
    }

    public async Task<PagedResult<PackagePlanResponse>> GetPagedAsync(ClaimsPrincipal user, PackagePlanQuery request, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status.Trim());
        if (request.FiscalYear.HasValue) query = query.Where(x => x.FiscalYear == request.FiscalYear.Value);
        if (request.ProcurementTypeId.HasValue) query = query.Where(x => x.ProcurementTypeId == request.ProcurementTypeId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x =>
                (x.PackagePlanNumber != null && x.PackagePlanNumber.ToUpper().Contains(search)) ||
                x.Title.ToUpper().Contains(search));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "packageplannumber" => descending ? query.OrderByDescending(x => x.PackagePlanNumber) : query.OrderBy(x => x.PackagePlanNumber),
            "estimatedvalue" => descending ? query.OrderByDescending(x => x.EstimatedValue) : query.OrderBy(x => x.EstimatedValue),
            "targetmonth" => descending ? query.OrderByDescending(x => x.TargetMonth) : query.OrderBy(x => x.TargetMonth),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await Project(query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize))
            .ToListAsync(cancellationToken);
        foreach (var item in items) ApplyFlags(item);

        return new PagedResult<PackagePlanResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = request.PageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    public async Task<PackagePlanResponse> GetByIdAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var response = await Project(query.Where(x => x.Id == id)).FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);

        response.Lines = await _dbContext.PrcPackagePlanLines.AsNoTracking()
            .Where(x => x.PackagePlanId == id && !x.IsDelete)
            .OrderBy(x => x.Item!.ItemCode).ThenBy(x => x.Department!.DepartmentName)
            .Select(x => new PackagePlanLineResponse
            {
                Id = x.Id,
                NeedPlanLineKey = x.NeedPlanLineKey,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department!.DepartmentName,
                ItemId = x.ItemId,
                ItemCode = x.Item!.ItemCode,
                ItemName = x.Item.ItemName,
                MeasurementName = x.Item.Measurement!.MeasurementName,
                AllocatedQuantity = x.AllocatedQuantity
            })
            .ToListAsync(cancellationToken);

        var planLines = await _quotaService.GetCurrentLinesAsync(response.Lines.Select(x => x.NeedPlanLineKey).ToList(), cancellationToken);
        foreach (var line in response.Lines)
        {
            if (!planLines.TryGetValue(line.NeedPlanLineKey, out var planLine)) continue;
            line.PlannedQuantity = planLine.Quantity;
            line.EstimatedUnitPrice = planLine.EstimatedUnitPrice;
            line.EstimatedAmount = Math.Round(line.AllocatedQuantity * planLine.EstimatedUnitPrice, 2, MidpointRounding.AwayFromZero);
        }

        ApplyFlags(response);
        return response;
    }

    public async Task<List<DocumentHistoryResponse>> GetStatusHistoryAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        await EnsureVisibleAsync(user, id, cancellationToken);
        return await _historyWriter.ReadAsync(ProcurementDocumentTypes.PackagePlan, id, cancellationToken);
    }

    /// <summary>Hak atas rencana paket untuk lampiran: tidak terlihat → None; DRAFT/RETURNED → Edit; selain itu → Read.</summary>
    public async Task<ProcurementDocumentAccessLevel> GetAccessLevelAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var status = await query.Where(x => x.Id == id).Select(x => x.Status).FirstOrDefaultAsync(cancellationToken);
        if (status == null) return ProcurementDocumentAccessLevel.None;
        return ProcurementStatuses.Plan.IsEditable(status) ? ProcurementDocumentAccessLevel.Edit : ProcurementDocumentAccessLevel.Read;
    }

    // ------------------------------------------------------------------ tulis

    public async Task<PackagePlanResponse> CreateAsync(ClaimsPrincipal user, CreatePackagePlanRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        var header = await ValidateHeaderAsync(request.Title, request.IndicativeMethod, request.TargetMonth, request.PicUserId, cancellationToken);
        if (request.FiscalYear is < 2000 or > 2100)
            throw new ProcurementPlanningValidationException("PBJ-VAL-043", "Tahun anggaran tidak valid.");
        var type = await _dbContext.MstProcurementTypes.AsNoTracking()
            .Where(x => x.Id == request.ProcurementTypeId && !x.IsDelete && x.IsActive)
            .Select(x => new { x.Id, x.TypeName })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ProcurementPlanningValidationException("PBJ-VAL-051", "Pilih jenis pengadaan yang masih aktif.");

        var now = DateTime.UtcNow;
        var entity = new PrcPackagePlan
        {
            FiscalYear = request.FiscalYear,
            ProcurementTypeId = type.Id,
            Status = ProcurementStatuses.Plan.Draft,
            CreateDateTime = now,
            CreateBy = actorUserId
        };
        ApplyHeader(entity, header);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var lines = await ValidateLinesAsync(user, entity.Id, request.FiscalYear, type.Id, type.TypeName, request.Lines, cancellationToken);
        _dbContext.PrcPackagePlans.Add(entity);
        foreach (var line in lines) AddLine(entity, line, actorUserId, now);
        entity.EstimatedValue = lines.Sum(x => x.Amount);
        _historyWriter.Record(
            ProcurementDocumentTypes.PackagePlan, entity.Id, null, ProcurementDocumentHistoryActions.Create,
            null, entity.Status, null, new { entity.EstimatedValue, LineCount = lines.Count }, actorUserId, DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await AuditAsync("PackagePlan.Create", entity, actorUserId);
        return await GetByIdAsync(user, entity.Id, cancellationToken);
    }

    public async Task<PackagePlanResponse> UpdateAsync(ClaimsPrincipal user, Guid id, UpdatePackagePlanRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await EnsureVisibleAsync(user, id, cancellationToken);
        var header = await ValidateHeaderAsync(request.Title, request.IndicativeMethod, request.TargetMonth, request.PicUserId, cancellationToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var entity = await _dbContext.PrcPackagePlans
                .Include(x => x.Lines.Where(l => !l.IsDelete))
                .Include(x => x.ProcurementType)
                .FirstAsync(x => x.Id == id, cancellationToken);
            if (!ProcurementStatuses.Plan.IsEditable(entity.Status))
                throw new ProcurementPlanningConflictException("PBJ-VAL-093", InvalidTransitionMessage);
            if (entity.RowVersion != request.ExpectedRowVersion)
                throw new ProcurementPlanningConflictException("PBJ-VAL-090", StaleMessage);

            var lines = await ValidateLinesAsync(user, entity.Id, entity.FiscalYear, entity.ProcurementTypeId,
                entity.ProcurementType?.TypeName ?? string.Empty, request.Lines, cancellationToken);
            var now = DateTime.UtcNow;

            // Dua tahap: baris lama ditandai terhapus lebih dulu agar index unik (paket, baris rencana) aman.
            foreach (var old in entity.Lines.ToList())
            {
                old.IsDelete = true;
                old.DeleteDateTime = now;
                old.DeleteBy = actorUserId;
            }
            ApplyHeader(entity, header);
            entity.EstimatedValue = lines.Sum(x => x.Amount);
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var line in lines) AddLine(entity, line, actorUserId, now);
            _historyWriter.Record(
                ProcurementDocumentTypes.PackagePlan, entity.Id, entity.PackagePlanNumber, ProcurementDocumentHistoryActions.Update,
                entity.Status, entity.Status, null, new { entity.EstimatedValue, LineCount = lines.Count }, actorUserId, DateTimeOffset.UtcNow);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await AuditAsync("PackagePlan.Update", entity, actorUserId);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementPlanningConflictException("PBJ-VAL-090", StaleMessage);
        }

        return await GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>Ajukan ke approval <c>PBJ-RPK</c>; nomor <c>RPK-…</c> dialokasikan saat pertama diajukan.</summary>
    public async Task<PackagePlanResponse> SubmitAsync(ClaimsPrincipal user, Guid id, string? idempotencyKey, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await EnsureVisibleAsync(user, id, cancellationToken);
        if (await _historyWriter.HasIdempotencyKeyAsync(
                ProcurementDocumentTypes.PackagePlan, id, ProcurementDocumentHistoryActions.Submit, idempotencyKey, cancellationToken))
            return await GetByIdAsync(user, id, cancellationToken);

        var plan = await _dbContext.PrcPackagePlans.AsNoTracking()
            .Include(x => x.Lines.Where(l => !l.IsDelete))
            .Include(x => x.ProcurementType)
            .FirstAsync(x => x.Id == id, cancellationToken);
        if (!ProcurementStatuses.Plan.IsEditable(plan.Status))
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", InvalidTransitionMessage);
        if (plan.Lines.Count == 0)
            throw new ProcurementPlanningValidationException("PBJ-VAL-050", "Rencana paket harus berisi minimal satu baris.");

        // Alokasi diperiksa ulang terhadap rencana yang berlaku sekarang (PBJ-VAL-050/051).
        await ValidateLinesAsync(user, plan.Id, plan.FiscalYear, plan.ProcurementTypeId, plan.ProcurementType?.TypeName ?? string.Empty,
            plan.Lines.Select(x => new PackagePlanLineRequest { NeedPlanLineKey = x.NeedPlanLineKey, AllocatedQuantity = x.AllocatedQuantity }).ToList(),
            cancellationToken, lockLines: false);

        var closed = await _approvalGateway.CloseReturnedAsync(plan.WorkflowInstanceId, "Rencana paket diajukan ulang setelah diperbaiki.", cancellationToken);
        if (!closed.Success)
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", closed.Message);

        var number = plan.PackagePlanNumber ?? await AllocateNumberAsync(actorUserId, cancellationToken);
        var submissionCount = plan.SubmissionCount + 1;
        var approval = await _approvalGateway.SubmitAsync(
            new ProcurementApprovalRequest(
                ProcurementDocumentTypes.PackagePlan, plan.Id, number, ProcurementWorkflowCodes.PackagePlanDefinition,
                actorUserId, plan.EstimatedValue, Array.Empty<string>(), submissionCount),
            cancellationToken);
        ProcurementNeedPlanService.ThrowIfApprovalFailed(approval);

        var entity = await _dbContext.PrcPackagePlans.FirstAsync(x => x.Id == id, cancellationToken);
        if (entity.RowVersion != plan.RowVersion || !ProcurementStatuses.Plan.IsEditable(entity.Status))
            throw new ProcurementPlanningConflictException("PBJ-VAL-090", StaleMessage);

        var fromStatus = entity.Status;
        var now = DateTimeOffset.UtcNow;
        entity.PackagePlanNumber = number;
        entity.Status = ProcurementStatuses.Plan.PendingApproval;
        entity.SubmissionCount = submissionCount;
        entity.WorkflowInstanceId = approval.WorkflowInstanceId;
        entity.SubmittedAt = now;
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateDateTime = now.UtcDateTime;
        entity.UpdateBy = actorUserId;
        _historyWriter.Record(
            ProcurementDocumentTypes.PackagePlan, entity.Id, number, ProcurementDocumentHistoryActions.Submit,
            fromStatus, entity.Status, null,
            new { idempotencyKey = idempotencyKey?.Trim(), entity.EstimatedValue, LineCount = plan.Lines.Count, submissionCount },
            actorUserId, now, approval.WorkflowInstanceId);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementPlanningConflictException("PBJ-VAL-090", StaleMessage);
        }

        await AuditAsync("PackagePlan.Submit", entity, actorUserId);
        return await GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>
    /// Batalkan rencana paket <c>DRAFT</c>/<c>RETURNED</c>/<c>APPROVED</c>; alasan wajib bila sudah disetujui.
    /// Alokasinya kembali ke sisa konsolidasi.
    /// </summary>
    public async Task<PackagePlanResponse> CancelAsync(ClaimsPrincipal user, Guid id, PackagePlanReasonRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await EnsureVisibleAsync(user, id, cancellationToken);

        var plan = await _dbContext.PrcPackagePlans.AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);
        var cancellable = plan.Status == ProcurementStatuses.Plan.Draft ||
                          plan.Status == ProcurementStatuses.Plan.Returned ||
                          plan.Status == ProcurementStatuses.Plan.Approved;
        if (!cancellable)
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", InvalidTransitionMessage);
        var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
        if (reason?.Length > 1000)
            throw new ProcurementPlanningValidationException("PBJ-VAL-030", "Alasan maksimal 1000 karakter.");
        if (reason == null && plan.Status == ProcurementStatuses.Plan.Approved)
            throw new ProcurementPlanningValidationException("PBJ-VAL-030", "Alasan wajib diisi.");

        var closed = await _approvalGateway.CloseReturnedAsync(plan.WorkflowInstanceId, reason ?? "Rencana paket dibatalkan.", cancellationToken);
        if (!closed.Success)
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", closed.Message);

        var entity = await _dbContext.PrcPackagePlans.FirstAsync(x => x.Id == id, cancellationToken);
        if (entity.RowVersion != plan.RowVersion)
            throw new ProcurementPlanningConflictException("PBJ-VAL-090", StaleMessage);
        var now = DateTimeOffset.UtcNow;
        entity.Status = ProcurementStatuses.Plan.Cancelled;
        entity.CancelReason = reason;
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateDateTime = now.UtcDateTime;
        entity.UpdateBy = actorUserId;
        _historyWriter.Record(
            ProcurementDocumentTypes.PackagePlan, entity.Id, entity.PackagePlanNumber, ProcurementDocumentHistoryActions.Cancel,
            plan.Status, entity.Status, reason, new { entity.EstimatedValue }, actorUserId, now);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await AuditAsync("PackagePlan.Cancel", entity, actorUserId);
        return await GetByIdAsync(user, id, cancellationToken);
    }

    // ------------------------------------------------------------------ hasil approval

    /// <summary>
    /// Menerapkan status Wfl ke rencana paket (matriks status bagian 3 dan 5). Hanya rencana paket
    /// <c>PENDING_APPROVAL</c> yang tertaut ke instance ini yang dipindahkan; pemanggilan ulang tidak mengubah apa pun.
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

        var plan = await _dbContext.PrcPackagePlans.FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken);
        if (plan == null) return result;
        result.PreviousReferenceStatus = plan.Status;
        result.CurrentReferenceStatus = plan.Status;

        var target = ProcurementWorkflowStatusMap.ToDocumentStatus(workflow.WorkflowStatus, isPurchaseRequest: false);
        if (plan.WorkflowInstanceId != workflow.Id ||
            plan.Status != ProcurementStatuses.Plan.PendingApproval ||
            target == null || target == plan.Status)
            return result;

        var actor = actorUserId == Guid.Empty ? workflow.RequestedByUserId : actorUserId;
        var now = DateTimeOffset.UtcNow;
        var from = plan.Status;
        plan.Status = target;
        if (target == ProcurementStatuses.Plan.Approved) plan.ApprovedAt = now;
        plan.RowVersion = Guid.NewGuid();
        plan.UpdateDateTime = now.UtcDateTime;
        plan.UpdateBy = actor;
        _historyWriter.Record(
            ProcurementDocumentTypes.PackagePlan, plan.Id, plan.PackagePlanNumber, ProcurementNeedPlanService.WorkflowActionOf(target),
            from, target, workflow.CancellationReason, new { workflow.WorkflowStatus, plan.EstimatedValue }, actor, now, workflow.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        result.CurrentReferenceStatus = plan.Status;
        result.StatusChanged = true;
        await AuditAsync("PackagePlan.WorkflowSync", plan, actor);
        return result;
    }

    // ------------------------------------------------------------------ bantuan

    private async Task<IQueryable<PrcPackagePlan>> ScopedAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var scope = await _scopeService.ResolveAsync(user, ProcurementPermissions.PackagePlanResource, cancellationToken);
        var query = _dbContext.PrcPackagePlans.AsNoTracking().Where(x => !x.IsDelete);
        if (scope.IsAllUnits) return query;
        var departments = scope.DepartmentIds.ToList();
        return query.Where(x => x.Lines.Any(l => !l.IsDelete && departments.Contains(l.DepartmentId)));
    }

    private async Task EnsureVisibleAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        if (!await query.AnyAsync(x => x.Id == id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
    }

    private sealed record HeaderValues(string Title, string? IndicativeMethod, int? TargetMonth, Guid? PicUserId);

    private async Task<HeaderValues> ValidateHeaderAsync(string? title, string? method, int? targetMonth, Guid? picUserId, CancellationToken cancellationToken)
    {
        var cleanTitle = title?.Trim();
        if (string.IsNullOrEmpty(cleanTitle))
            throw new ProcurementPlanningValidationException("PBJ-VAL-051", "Judul rencana paket wajib diisi.");
        if (cleanTitle.Length > 200)
            throw new ProcurementPlanningValidationException("PBJ-VAL-051", "Judul rencana paket maksimal 200 karakter.");
        var cleanMethod = string.IsNullOrWhiteSpace(method) ? null : method.Trim();
        if (cleanMethod?.Length > 100)
            throw new ProcurementPlanningValidationException("PBJ-VAL-051", "Metode indikatif maksimal 100 karakter.");
        if (targetMonth is < 1 or > 12)
            throw new ProcurementPlanningValidationException("PBJ-VAL-051", "Bulan target harus 1–12.");
        if (picUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == picUserId.Value, cancellationToken))
            throw new ProcurementPlanningValidationException("PBJ-VAL-051", "Penanggung jawab paket tidak ditemukan.");
        return new HeaderValues(cleanTitle, cleanMethod, targetMonth, picUserId);
    }

    private static void ApplyHeader(PrcPackagePlan entity, HeaderValues header)
    {
        entity.Title = header.Title;
        entity.IndicativeMethod = header.IndicativeMethod;
        entity.TargetMonth = header.TargetMonth;
        entity.PicUserId = header.PicUserId;
    }

    private sealed record LineValues(Guid LineKey, Guid DepartmentId, Guid ItemId, decimal Quantity, decimal Amount);

    /// <summary>
    /// Memeriksa baris alokasi terhadap rencana berlaku: baris rencana harus berlaku dan setahun dengan paket,
    /// unitnya dalam cakupan pengguna, jenis barangnya sama dengan kepala (PBJ-VAL-051), dan Σ alokasi
    /// seluruh paket hidup tidak melebihi kuantitas baris rencana (PBJ-VAL-050). Bila
    /// <paramref name="lockLines"/>, baris rencana dikunci lebih dulu (wajib di dalam transaksi).
    /// </summary>
    private async Task<List<LineValues>> ValidateLinesAsync(
        ClaimsPrincipal user, Guid packagePlanId, int fiscalYear, Guid procurementTypeId, string procurementTypeName,
        List<PackagePlanLineRequest>? requestLines, CancellationToken cancellationToken, bool lockLines = true)
    {
        requestLines ??= new List<PackagePlanLineRequest>();
        var keys = requestLines.Select(x => x.NeedPlanLineKey).ToList();
        if (lockLines) await _quotaService.LockAsync(keys, cancellationToken);

        var scope = await _scopeService.ResolveAsync(user, ProcurementPermissions.PackagePlanResource, cancellationToken);
        var planLines = await _quotaService.GetCurrentLinesAsync(keys, cancellationToken);
        var usage = await _quotaService.GetUsageAsync(keys, null, cancellationToken, excludePackagePlanId: packagePlanId);

        var result = new List<LineValues>();
        var seen = new HashSet<Guid>();
        for (var index = 0; index < requestLines.Count; index++)
        {
            var line = requestLines[index];
            var no = index + 1;
            if (!seen.Add(line.NeedPlanLineKey))
                throw new ProcurementPlanningValidationException("PBJ-VAL-050", $"Baris {no} memakai baris rencana yang sudah dipilih.", no);
            if (!planLines.TryGetValue(line.NeedPlanLineKey, out var planLine) || planLine.FiscalYear != fiscalYear || !scope.CanSee(planLine.DepartmentId))
                throw new ProcurementPlanningValidationException(
                    "PBJ-VAL-050", $"Baris {no} tidak menunjuk rencana kebutuhan berlaku tahun {fiscalYear}.", no);
            if (planLine.ProcurementTypeId != procurementTypeId)
                throw new ProcurementPlanningValidationException("PBJ-VAL-051", $"Semua baris paket harus berjenis {procurementTypeName}.", no);
            if (line.AllocatedQuantity <= 0)
                throw new ProcurementPlanningValidationException("PBJ-VAL-050", $"Alokasi pada baris {no} harus lebih dari 0.", no);

            var allocatedElsewhere = (usage.GetValueOrDefault(line.NeedPlanLineKey) ?? PlanLineUsage.None).AllocatedToPackage;
            if (allocatedElsewhere + line.AllocatedQuantity > planLine.Quantity)
                throw new ProcurementPlanningValidationException(
                    "PBJ-VAL-050",
                    $"Alokasi {planLine.ItemName} unit {planLine.DepartmentName} melebihi rencana ({Format(planLine.Quantity)}), sudah dialokasikan {Format(allocatedElsewhere)}.",
                    no);

            result.Add(new LineValues(
                line.NeedPlanLineKey, planLine.DepartmentId, planLine.ItemId, line.AllocatedQuantity,
                Math.Round(line.AllocatedQuantity * planLine.EstimatedUnitPrice, 2, MidpointRounding.AwayFromZero)));
        }

        return result;
    }

    private void AddLine(PrcPackagePlan plan, LineValues line, Guid actorUserId, DateTime now) =>
        _dbContext.PrcPackagePlanLines.Add(new PrcPackagePlanLine
        {
            PackagePlanId = plan.Id,
            NeedPlanLineKey = line.LineKey,
            DepartmentId = line.DepartmentId,
            ItemId = line.ItemId,
            AllocatedQuantity = line.Quantity,
            CreateDateTime = now,
            CreateBy = actorUserId
        });

    private async Task<string> AllocateNumberAsync(Guid actorUserId, CancellationToken cancellationToken)
    {
        try
        {
            return await _numberAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: ProcurementNumberSeries.PackagePlanSequenceKey,
                    Prefix: ProcurementNumberSeries.PackagePlanPrefix,
                    ResetPolicy: ProcurementNumberSeries.ResetPolicy,
                    SequenceDigits: ProcurementNumberSeries.SequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow),
                cancellationToken);
        }
        catch (NumberSeriesAllocationException)
        {
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", "Nomor rencana paket gagal dibuat. Silakan coba lagi.");
        }
    }

    private static string Format(decimal value) => value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);

    private static void ApplyFlags(PackagePlanResponse response)
    {
        response.StatusLabel = ProcurementStatuses.Plan.LabelOf(response.Status);
        response.CanEdit = ProcurementStatuses.Plan.IsEditable(response.Status);
        response.CanSubmit = response.CanEdit;
        response.CanCancel = response.CanEdit || response.Status == ProcurementStatuses.Plan.Approved;
    }

    private static Guid RequireUser(ClaimsPrincipal user)
    {
        var userId = ProcurementDataScopeService.GetUserId(user);
        if (userId == Guid.Empty)
            throw new ProcurementPlanningForbiddenException("Identitas pengguna tidak dikenali.");
        return userId;
    }

    private IQueryable<PackagePlanResponse> Project(IQueryable<PrcPackagePlan> query) =>
        query.Select(x => new PackagePlanResponse
        {
            Id = x.Id,
            PackagePlanNumber = x.PackagePlanNumber,
            FiscalYear = x.FiscalYear,
            Title = x.Title,
            ProcurementTypeId = x.ProcurementTypeId,
            ProcurementTypeCode = x.ProcurementType!.TypeCode,
            ProcurementTypeName = x.ProcurementType.TypeName,
            IndicativeMethod = x.IndicativeMethod,
            TargetMonth = x.TargetMonth,
            EstimatedValue = x.EstimatedValue,
            PicUserId = x.PicUserId,
            PicName = _dbContext.Users.Where(u => u.Id == x.PicUserId).Select(u => u.DisplayName).FirstOrDefault(),
            Status = x.Status,
            CancelReason = x.CancelReason,
            SubmissionCount = x.SubmissionCount,
            WorkflowInstanceId = x.WorkflowInstanceId,
            SubmittedAt = x.SubmittedAt,
            ApprovedAt = x.ApprovedAt,
            LineCount = x.Lines.Count(l => !l.IsDelete),
            UnitCount = x.Lines.Where(l => !l.IsDelete).Select(l => l.DepartmentId).Distinct().Count(),
            RowVersion = x.RowVersion,
            CreateBy = x.CreateBy,
            CreatedByName = _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName).FirstOrDefault(),
            CreateDateTime = x.CreateDateTime,
            UpdateDateTime = x.UpdateDateTime
        });

    private Task AuditAsync(string action, PrcPackagePlan entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan rencana paket pengadaan.", new
        {
            EntityId = entity.Id,
            entity.Status,
            ActorUserId = actorUserId
        });
}

/// <summary>Penyedia akses lampiran untuk rencana paket (BE-PBJ-011).</summary>
public sealed class PackagePlanAttachmentAccessProvider : IProcurementDocumentAccessProvider
{
    private readonly ProcurementPackagePlanService _service;

    public PackagePlanAttachmentAccessProvider(ProcurementPackagePlanService service) => _service = service;

    public string DocumentType => ProcurementDocumentTypes.PackagePlan;

    public Task<ProcurementDocumentAccessLevel> GetAccessAsync(ClaimsPrincipal user, Guid documentId, CancellationToken cancellationToken) =>
        _service.GetAccessLevelAsync(user, documentId, cancellationToken);
}

/// <summary>
/// Penerap status Wfl untuk rencana paket (<c>PBJ_PACKAGE_PLAN</c>, BE-PBJ-011). Service diambil saat
/// dipanggil agar tidak membentuk lingkaran dependensi dengan <c>WorkflowService</c>.
/// </summary>
public sealed class PackagePlanWorkflowStatusApplier : IProcurementWorkflowStatusApplier
{
    private readonly IServiceProvider _serviceProvider;

    public PackagePlanWorkflowStatusApplier(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public string DocumentType => ProcurementDocumentTypes.PackagePlan;

    public Task<WorkflowReferenceLifecycleSynchronizationResult> ApplyAsync(
        TrxWorkflowInstance workflow, Guid actorUserId, CancellationToken cancellationToken) =>
        _serviceProvider.GetRequiredService<ProcurementPackagePlanService>()
            .ApplyWorkflowStatusAsync(workflow, actorUserId, cancellationToken);
}
