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
/// Rencana kebutuhan tahunan berversi — draf, ajukan, batal, revisi, banding versi, dan penerapan
/// hasil approval (BE-PBJ-009; <c>state-transition-matrix.md</c> bagian 2, <c>validation-matrix.md</c>
/// bagian 2).
/// </summary>
/// <remarks>
/// <para><b>Cakupan unit (PBJ-DEC-020/036).</b> Query disaring <see cref="ProcurementDataScopeService"/>
/// dengan resource <c>ProcurementNeedPlan</c>; rencana unit lain dijawab 404. Membuat rencana hanya
/// untuk unit penempatan penyusun sendiri.</para>
/// <para><b>Versi.</b> Versi yang disetujui tidak diubah; revisi menyalin baris beserta <c>LineKey</c>
/// sehingga PR yang menunjuk baris rencana tetap tertaut. Saat revisi disetujui, versi lama menjadi
/// <c>SUPERSEDED</c> dan penanda versi berlaku berpindah dalam satu transaksi.</para>
/// <para><b>Approval.</b> Lewat <see cref="ProcurementApprovalGateway"/> dengan definisi
/// <c>PBJ-RKB</c>, atas nama penyusun yang mengajukan; Wfl dulu, status rencana sesudahnya.</para>
/// </remarks>
public sealed class ProcurementNeedPlanService
{
    private const string LogCategory = "Corporate.ProcurementManagement.Planning";
    private const string NotFoundMessage = "Rencana tidak ditemukan atau bukan milik unit Anda.";
    private const string InvalidTransitionMessage = "Status rencana tidak memungkinkan tindakan ini.";
    private const string StaleMessage = "Data sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda.";

    private readonly ApplicationDbContext _dbContext;
    private readonly ProcurementDataScopeService _scopeService;
    private readonly ProcurementDocumentHistoryWriter _historyWriter;
    private readonly ProcurementPlanQuotaService _quotaService;
    private readonly ProcurementApprovalGateway _approvalGateway;
    private readonly NumberSeriesAllocator _numberAllocator;
    private readonly LoggerService _loggerService;

    public ProcurementNeedPlanService(
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

    public NeedPlanFilterMetadataResponse GetFilterMetadata()
    {
        var year = DateTime.UtcNow.Year;
        return new NeedPlanFilterMetadataResponse
        {
            DefaultFilter = new NeedPlanQuery(),
            StatusOptions = ProcurementStatuses.Plan.Labels
                .Select(x => new NeedPlanOptionValue { Value = x.Key, Label = x.Value }).ToList(),
            SortOptions =
            [
                new() { Value = "createdAt", Label = "Tanggal dibuat" },
                new() { Value = "planNumber", Label = "Nomor rencana" },
                new() { Value = "fiscalYear", Label = "Tahun anggaran" },
                new() { Value = "estimatedTotal", Label = "Total estimasi" }
            ],
            FiscalYearOptions = Enumerable.Range(year - 1, 4).ToList()
        };
    }

    public async Task<NeedPlanSummaryResponse> GetSummaryAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var byStatus = await query
            .GroupBy(x => x.Status)
            .Select(g => new NeedPlanStatusCount { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new NeedPlanSummaryResponse
        {
            Total = byStatus.Sum(x => x.Count),
            CurrentVersions = await query.CountAsync(x => x.IsCurrentVersion, cancellationToken),
            PendingApproval = byStatus.Where(x => x.Status == ProcurementStatuses.Plan.PendingApproval).Sum(x => x.Count),
            ByStatus = byStatus.OrderBy(x => x.Status).ToList()
        };
    }

    public async Task<PagedResult<NeedPlanResponse>> GetPagedAsync(ClaimsPrincipal user, NeedPlanQuery request, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        query = query.WhereCreatedWithin(ProcurementPeriodFilter.Resolve(request.StartDate, request.EndDate, request.CustomPeriod));
        if (!request.IncludeHistory) query = query.Where(x => x.Status != ProcurementStatuses.Plan.Superseded);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status.Trim());
        if (request.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
        if (request.FiscalYear.HasValue) query = query.Where(x => x.FiscalYear == request.FiscalYear.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x =>
                (x.PlanNumber != null && x.PlanNumber.ToUpper().Contains(search)) ||
                x.Department!.DepartmentName.ToUpper().Contains(search));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "plannumber" => descending ? query.OrderByDescending(x => x.PlanNumber).ThenByDescending(x => x.VersionNo) : query.OrderBy(x => x.PlanNumber).ThenBy(x => x.VersionNo),
            "fiscalyear" => descending ? query.OrderByDescending(x => x.FiscalYear) : query.OrderBy(x => x.FiscalYear),
            "estimatedtotal" => descending ? query.OrderByDescending(x => x.EstimatedTotal) : query.OrderBy(x => x.EstimatedTotal),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await Project(query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize))
            .ToListAsync(cancellationToken);
        foreach (var item in items) ApplyFlags(item, hasUnfinishedRevision: false);

        return new PagedResult<NeedPlanResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = request.PageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    public async Task<NeedPlanResponse> GetByIdAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var response = await Project(query.Where(x => x.Id == id)).FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);

        response.Lines = await ProjectLines(id).ToListAsync(cancellationToken);
        var usage = await _quotaService.GetUsageAsync(response.Lines.Select(x => x.LineKey).ToList(), null, cancellationToken);
        foreach (var line in response.Lines)
        {
            var used = usage.GetValueOrDefault(line.LineKey) ?? PlanLineUsage.None;
            line.UsedByPurchaseRequest = used.UsedByPurchaseRequest;
            line.AllocatedToPackage = used.AllocatedToPackage;
            line.RemainingForPurchaseRequest = Math.Max(0, line.Quantity - used.UsedByPurchaseRequest);
        }

        var hasUnfinishedRevision = response.IsCurrentVersion &&
            await HasUnfinishedVersionAsync(response.DepartmentId, response.FiscalYear, response.Id, cancellationToken);
        ApplyFlags(response, hasUnfinishedRevision);
        return response;
    }

    /// <summary>Seluruh versi rencana unit-tahun yang sama (bernomor sama), urut versi.</summary>
    public async Task<List<NeedPlanResponse>> GetVersionsAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var plan = await query.Where(x => x.Id == id)
            .Select(x => new { x.Id, x.PlanNumber })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);

        var versions = plan.PlanNumber == null
            ? query.Where(x => x.Id == plan.Id)
            : query.Where(x => x.PlanNumber == plan.PlanNumber);
        var items = await Project(versions.OrderBy(x => x.VersionNo)).ToListAsync(cancellationToken);
        foreach (var item in items) ApplyFlags(item, hasUnfinishedRevision: false);
        return items;
    }

    public async Task<NeedPlanVersionCompareResponse> CompareAsync(ClaimsPrincipal user, Guid id, Guid otherId, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var plans = await query.Where(x => x.Id == id || x.Id == otherId)
            .Select(x => new { x.Id, x.PlanNumber, x.VersionNo, x.EstimatedTotal, x.RevisionReason })
            .ToListAsync(cancellationToken);
        var first = plans.FirstOrDefault(x => x.Id == id) ?? throw new KeyNotFoundException(NotFoundMessage);
        var second = plans.FirstOrDefault(x => x.Id == otherId) ?? throw new KeyNotFoundException(NotFoundMessage);
        if (first.Id == second.Id || first.PlanNumber == null || first.PlanNumber != second.PlanNumber)
            throw new ProcurementPlanningValidationException(
                "PBJ-VAL-041", "Hanya versi dari rencana yang sama yang dapat dibandingkan.");

        var before = first.VersionNo <= second.VersionNo ? first : second;
        var after = before == first ? second : first;
        var beforeLines = await ProjectLines(before.Id).ToListAsync(cancellationToken);
        var afterLines = await ProjectLines(after.Id).ToListAsync(cancellationToken);

        var lines = new List<NeedPlanVersionCompareLine>();
        foreach (var key in beforeLines.Select(x => x.LineKey).Union(afterLines.Select(x => x.LineKey)))
        {
            var b = beforeLines.FirstOrDefault(x => x.LineKey == key);
            var a = afterLines.FirstOrDefault(x => x.LineKey == key);
            var any = a ?? b!;
            var changeType = b == null ? "ADDED"
                : a == null ? "REMOVED"
                : b.Quantity != a.Quantity || b.EstimatedUnitPrice != a.EstimatedUnitPrice ? "CHANGED"
                : "UNCHANGED";
            lines.Add(new NeedPlanVersionCompareLine
            {
                LineKey = key,
                ItemId = any.ItemId,
                ItemCode = any.ItemCode,
                ItemName = any.ItemName,
                MeasurementName = any.MeasurementName,
                ChangeType = changeType,
                BeforeQuantity = b?.Quantity,
                AfterQuantity = a?.Quantity,
                BeforeUnitPrice = b?.EstimatedUnitPrice,
                AfterUnitPrice = a?.EstimatedUnitPrice,
                BeforeTotal = b?.EstimatedTotal,
                AfterTotal = a?.EstimatedTotal
            });
        }

        return new NeedPlanVersionCompareResponse
        {
            BeforeId = before.Id,
            BeforeVersionNo = before.VersionNo,
            BeforeTotal = before.EstimatedTotal,
            AfterId = after.Id,
            AfterVersionNo = after.VersionNo,
            AfterTotal = after.EstimatedTotal,
            RevisionReason = after.RevisionReason,
            Lines = lines.OrderBy(x => x.ItemCode).ToList()
        };
    }

    public async Task<List<DocumentHistoryResponse>> GetStatusHistoryAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        if (!await query.AnyAsync(x => x.Id == id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
        return await _historyWriter.ReadAsync(ProcurementDocumentTypes.NeedPlan, id, cancellationToken);
    }

    /// <summary>Hak atas rencana untuk lampiran: tidak terlihat → None; DRAFT/RETURNED → Edit; selain itu → Read.</summary>
    public async Task<ProcurementDocumentAccessLevel> GetAccessLevelAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var status = await query.Where(x => x.Id == id).Select(x => x.Status).FirstOrDefaultAsync(cancellationToken);
        if (status == null) return ProcurementDocumentAccessLevel.None;
        return ProcurementStatuses.Plan.IsEditable(status) ? ProcurementDocumentAccessLevel.Edit : ProcurementDocumentAccessLevel.Read;
    }

    // ------------------------------------------------------------------ tulis

    public async Task<NeedPlanResponse> CreateAsync(ClaimsPrincipal user, CreateNeedPlanRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await RequirePlacementAsync(actorUserId, request.DepartmentId, cancellationToken);

        var year = DateTime.UtcNow.Year;
        if (request.FiscalYear < year - 1 || request.FiscalYear > year + 2)
            throw new ProcurementPlanningValidationException(
                "PBJ-VAL-043", $"Tahun rencana harus antara {year - 1} dan {year + 2}.");

        var existing = await _dbContext.PrcNeedPlans.AsNoTracking()
            .Where(x => x.DepartmentId == request.DepartmentId && x.FiscalYear == request.FiscalYear && !x.IsDelete &&
                        x.Status != ProcurementStatuses.Plan.Cancelled &&
                        x.Status != ProcurementStatuses.Plan.Rejected &&
                        x.Status != ProcurementStatuses.Plan.Superseded)
            .OrderByDescending(x => x.IsCurrentVersion).ThenByDescending(x => x.VersionNo)
            .Select(x => new { x.Id, DepartmentName = x.Department!.DepartmentName })
            .FirstOrDefaultAsync(cancellationToken);
        if (existing != null)
            throw new ProcurementPlanningConflictException(
                "PBJ-VAL-040",
                $"Unit {existing.DepartmentName} sudah memiliki rencana {request.FiscalYear}. Buka rencana itu atau buat revisi.",
                existing.Id);

        var lines = await ValidateLinesAsync(request.Lines, existingKeys: null, cancellationToken);

        var now = DateTime.UtcNow;
        var entity = new PrcNeedPlan
        {
            DepartmentId = request.DepartmentId,
            FiscalYear = request.FiscalYear,
            VersionNo = 1,
            Status = ProcurementStatuses.Plan.Draft,
            CreateDateTime = now,
            CreateBy = actorUserId
        };
        _dbContext.PrcNeedPlans.Add(entity);
        foreach (var line in lines) AddLine(entity, line, line.LineKey ?? Guid.NewGuid(), actorUserId, now);
        entity.EstimatedTotal = lines.Sum(x => x.Total);

        _historyWriter.Record(
            ProcurementDocumentTypes.NeedPlan, entity.Id, null, ProcurementDocumentHistoryActions.Create,
            null, entity.Status, null, new { entity.EstimatedTotal, LineCount = lines.Count, entity.VersionNo },
            actorUserId, DateTimeOffset.UtcNow);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ProcurementPlanningConflictException(
                "PBJ-VAL-040", $"Unit ini sudah memiliki rencana {request.FiscalYear}. Buka rencana itu atau buat revisi.");
        }

        await AuditAsync("NeedPlan.Create", entity, actorUserId);
        return await GetByIdAsync(user, entity.Id, cancellationToken);
    }

    public async Task<NeedPlanResponse> UpdateAsync(ClaimsPrincipal user, Guid id, UpdateNeedPlanRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await EnsureVisibleAsync(user, id, cancellationToken);

        var entity = await _dbContext.PrcNeedPlans
            .Include(x => x.Lines.Where(l => !l.IsDelete))
            .FirstAsync(x => x.Id == id, cancellationToken);
        if (!ProcurementStatuses.Plan.IsEditable(entity.Status))
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", InvalidTransitionMessage);
        if (entity.RowVersion != request.ExpectedRowVersion)
            throw new ProcurementPlanningConflictException("PBJ-VAL-090", StaleMessage);

        var existingByKey = entity.Lines.ToDictionary(x => x.LineKey);
        var lines = await ValidateLinesAsync(request.Lines, existingByKey, cancellationToken);
        var keptKeys = lines.Where(x => x.LineKey.HasValue).Select(x => x.LineKey!.Value).ToHashSet();
        var now = DateTime.UtcNow;

        // Dua tahap dalam satu transaksi: baris yang dihapus ditandai terhapus dan baris lama diperbarui
        // lebih dulu, baru baris baru ditulis. Urutan ini menjaga index unik (rencana, barang) dan
        // (rencana, LineKey) yang hanya berlaku untuk baris hidup.
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var old in entity.Lines.Where(x => !keptKeys.Contains(x.LineKey)).ToList())
            {
                old.IsDelete = true;
                old.DeleteDateTime = now;
                old.DeleteBy = actorUserId;
            }

            foreach (var line in lines.Where(x => x.LineKey.HasValue))
            {
                var row = existingByKey[line.LineKey!.Value];
                row.Quantity = line.Quantity;
                row.EstimatedUnitPrice = line.UnitPrice;
                row.EstimatedTotal = line.Total;
                row.NeededMonth = line.NeededMonth;
                row.FundSourceId = line.FundSourceId;
                row.Justification = line.Justification;
                row.Specification = line.Specification;
                row.UpdateDateTime = now;
                row.UpdateBy = actorUserId;
            }

            entity.EstimatedTotal = lines.Sum(x => x.Total);
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var line in lines.Where(x => !x.LineKey.HasValue))
                AddLine(entity, line, Guid.NewGuid(), actorUserId, now);
            _historyWriter.Record(
                ProcurementDocumentTypes.NeedPlan, entity.Id, entity.PlanNumber, ProcurementDocumentHistoryActions.Update,
                entity.Status, entity.Status, null, new { entity.EstimatedTotal, LineCount = lines.Count, entity.VersionNo },
                actorUserId, DateTimeOffset.UtcNow);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementPlanningConflictException("PBJ-VAL-090", StaleMessage);
        }

        await AuditAsync("NeedPlan.Update", entity, actorUserId);
        return await GetByIdAsync(user, entity.Id, cancellationToken);
    }

    /// <summary>
    /// Ajukan ke approval <c>PBJ-RKB</c>. Nomor <c>RKB-…</c> dialokasikan saat versi 1 pertama kali
    /// diajukan. Revisi diperiksa terhadap pemakaian baris oleh PR dan rencana paket (PBJ-VAL-042).
    /// </summary>
    public async Task<NeedPlanResponse> SubmitAsync(ClaimsPrincipal user, Guid id, string? idempotencyKey, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await EnsureVisibleAsync(user, id, cancellationToken);

        if (await _historyWriter.HasIdempotencyKeyAsync(
                ProcurementDocumentTypes.NeedPlan, id, ProcurementDocumentHistoryActions.Submit, idempotencyKey, cancellationToken))
            return await GetByIdAsync(user, id, cancellationToken);

        var plan = await _dbContext.PrcNeedPlans.AsNoTracking()
            .FirstAsync(x => x.Id == id, cancellationToken);
        if (!ProcurementStatuses.Plan.IsEditable(plan.Status))
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", InvalidTransitionMessage);

        var lines = await ProjectLines(id).ToListAsync(cancellationToken);
        if (lines.Count == 0)
            throw new ProcurementPlanningValidationException("PBJ-VAL-001", "Rencana harus berisi minimal satu barang atau jasa.");
        if (plan.PreviousVersionId.HasValue)
            await EnsureNotBelowUsageAsync(plan.PreviousVersionId.Value, lines, cancellationToken);

        // Wfl dulu (PBJ-DEC-038). Instance lama yang dikembalikan penyetuju ditutup lebih dulu.
        var closed = await _approvalGateway.CloseReturnedAsync(plan.WorkflowInstanceId, "Rencana diajukan ulang setelah diperbaiki.", cancellationToken);
        if (!closed.Success)
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", closed.Message);

        var planNumber = plan.PlanNumber ?? await AllocateNumberAsync(actorUserId, cancellationToken);
        var submissionCount = plan.SubmissionCount + 1;
        var approval = await _approvalGateway.SubmitAsync(
            new ProcurementApprovalRequest(
                ProcurementDocumentTypes.NeedPlan, plan.Id, planNumber, ProcurementWorkflowCodes.NeedPlanDefinition,
                actorUserId, plan.EstimatedTotal, Array.Empty<string>(), submissionCount),
            cancellationToken);
        ThrowIfApprovalFailed(approval);

        var entity = await _dbContext.PrcNeedPlans.FirstAsync(x => x.Id == id, cancellationToken);
        if (entity.RowVersion != plan.RowVersion || !ProcurementStatuses.Plan.IsEditable(entity.Status))
            throw new ProcurementPlanningConflictException("PBJ-VAL-090", StaleMessage);

        var fromStatus = entity.Status;
        var now = DateTimeOffset.UtcNow;
        entity.PlanNumber = planNumber;
        entity.Status = ProcurementStatuses.Plan.PendingApproval;
        entity.SubmissionCount = submissionCount;
        entity.WorkflowInstanceId = approval.WorkflowInstanceId;
        entity.SubmittedAt = now;
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateDateTime = now.UtcDateTime;
        entity.UpdateBy = actorUserId;
        _historyWriter.Record(
            ProcurementDocumentTypes.NeedPlan, entity.Id, planNumber, ProcurementDocumentHistoryActions.Submit,
            fromStatus, entity.Status, null,
            new { idempotencyKey = idempotencyKey?.Trim(), entity.EstimatedTotal, LineCount = lines.Count, entity.VersionNo, submissionCount },
            actorUserId, now, approval.WorkflowInstanceId);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementPlanningConflictException("PBJ-VAL-090", StaleMessage);
        }

        await AuditAsync("NeedPlan.Submit", entity, actorUserId);
        return await GetByIdAsync(user, id, cancellationToken);
    }

    public async Task<NeedPlanResponse> CancelAsync(ClaimsPrincipal user, Guid id, NeedPlanReasonRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await EnsureVisibleAsync(user, id, cancellationToken);

        var entity = await _dbContext.PrcNeedPlans.FirstAsync(x => x.Id == id, cancellationToken);
        if (entity.Status != ProcurementStatuses.Plan.Draft)
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", InvalidTransitionMessage);

        var now = DateTimeOffset.UtcNow;
        entity.Status = ProcurementStatuses.Plan.Cancelled;
        entity.IsCurrentVersion = false;
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateDateTime = now.UtcDateTime;
        entity.UpdateBy = actorUserId;
        _historyWriter.Record(
            ProcurementDocumentTypes.NeedPlan, entity.Id, entity.PlanNumber, ProcurementDocumentHistoryActions.Cancel,
            ProcurementStatuses.Plan.Draft, entity.Status, Clean(request.Reason, 1000, "Alasan pembatalan"), null,
            actorUserId, now);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await AuditAsync("NeedPlan.Cancel", entity, actorUserId);
        return await GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>
    /// Membuat versi baru <c>DRAFT</c> dari versi <c>APPROVED</c> yang berlaku (2.5). Baris disalin
    /// lengkap dengan <c>LineKey</c>-nya.
    /// </summary>
    public async Task<NeedPlanResponse> CreateRevisionAsync(ClaimsPrincipal user, Guid id, NeedPlanReasonRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await EnsureVisibleAsync(user, id, cancellationToken);

        var reason = Clean(request.Reason, 1000, "Alasan revisi")
            ?? throw new ProcurementPlanningValidationException("PBJ-VAL-030", "Alasan revisi wajib diisi.");

        var source = await _dbContext.PrcNeedPlans.AsNoTracking()
            .Include(x => x.Lines.Where(l => !l.IsDelete))
            .FirstAsync(x => x.Id == id, cancellationToken);
        if (source.Status != ProcurementStatuses.Plan.Approved || !source.IsCurrentVersion)
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", "Revisi hanya dapat dibuat dari versi yang disetujui dan berlaku.");
        if (await HasUnfinishedVersionAsync(source.DepartmentId, source.FiscalYear, source.Id, cancellationToken))
            throw new ProcurementPlanningConflictException("PBJ-VAL-041", "Masih ada revisi yang belum selesai.");

        var maxVersion = await _dbContext.PrcNeedPlans.AsNoTracking()
            .Where(x => x.PlanNumber == source.PlanNumber && !x.IsDelete)
            .MaxAsync(x => x.VersionNo, cancellationToken);

        var now = DateTime.UtcNow;
        var revision = new PrcNeedPlan
        {
            PlanNumber = source.PlanNumber,
            DepartmentId = source.DepartmentId,
            FiscalYear = source.FiscalYear,
            VersionNo = maxVersion + 1,
            PreviousVersionId = source.Id,
            IsCurrentVersion = false,
            Status = ProcurementStatuses.Plan.Draft,
            RevisionReason = reason,
            EstimatedTotal = source.EstimatedTotal,
            CreateDateTime = now,
            CreateBy = actorUserId
        };
        _dbContext.PrcNeedPlans.Add(revision);
        foreach (var line in source.Lines)
        {
            _dbContext.PrcNeedPlanLines.Add(new PrcNeedPlanLine
            {
                NeedPlanId = revision.Id,
                LineKey = line.LineKey,
                ItemId = line.ItemId,
                Quantity = line.Quantity,
                EstimatedUnitPrice = line.EstimatedUnitPrice,
                EstimatedTotal = line.EstimatedTotal,
                NeededMonth = line.NeededMonth,
                FundSourceId = line.FundSourceId,
                Justification = line.Justification,
                Specification = line.Specification,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }

        _historyWriter.Record(
            ProcurementDocumentTypes.NeedPlan, revision.Id, revision.PlanNumber, ProcurementDocumentHistoryActions.Revise,
            null, revision.Status, reason, new { FromVersionId = source.Id, FromVersionNo = source.VersionNo, revision.VersionNo },
            actorUserId, DateTimeOffset.UtcNow);
        _historyWriter.Record(
            ProcurementDocumentTypes.NeedPlan, source.Id, source.PlanNumber, ProcurementDocumentHistoryActions.Revise,
            source.Status, source.Status, reason, new { RevisionId = revision.Id, revision.VersionNo },
            actorUserId, DateTimeOffset.UtcNow);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ProcurementPlanningConflictException("PBJ-VAL-041", "Masih ada revisi yang belum selesai.");
        }

        await AuditAsync("NeedPlan.Revise", revision, actorUserId);
        return await GetByIdAsync(user, revision.Id, cancellationToken);
    }

    // ------------------------------------------------------------------ hasil approval

    /// <summary>
    /// Menerapkan status Wfl ke rencana (matriks status bagian 5). Hanya rencana
    /// <c>PENDING_APPROVAL</c> yang tertaut ke instance ini yang dipindahkan; pemanggilan ulang tidak
    /// mengubah apa pun. Saat disetujui, versi berlaku sebelumnya menjadi <c>SUPERSEDED</c> dan
    /// penanda versi berlaku berpindah dalam satu transaksi.
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

        var plan = await _dbContext.PrcNeedPlans.FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken);
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
        var ownTransaction = _dbContext.Database.CurrentTransaction == null
            ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;
        try
        {
            if (target == ProcurementStatuses.Plan.Approved)
            {
                // Lepaskan versi berlaku lama lebih dulu agar index unik versi berlaku tidak bentrok.
                var current = await _dbContext.PrcNeedPlans
                    .Where(x => x.DepartmentId == plan.DepartmentId && x.FiscalYear == plan.FiscalYear &&
                                x.IsCurrentVersion && x.Id != plan.Id && !x.IsDelete)
                    .ToListAsync(cancellationToken);
                foreach (var old in current)
                {
                    var oldStatus = old.Status;
                    old.IsCurrentVersion = false;
                    old.Status = ProcurementStatuses.Plan.Superseded;
                    old.RowVersion = Guid.NewGuid();
                    old.UpdateDateTime = now.UtcDateTime;
                    old.UpdateBy = actor;
                    _historyWriter.Record(
                        ProcurementDocumentTypes.NeedPlan, old.Id, old.PlanNumber, ProcurementDocumentHistoryActions.Supersede,
                        oldStatus, old.Status, null, new { SupersededById = plan.Id, plan.VersionNo }, actor, now, workflow.Id);
                }
                await _dbContext.SaveChangesAsync(cancellationToken);

                plan.IsCurrentVersion = true;
                plan.ApprovedAt = now;
            }

            var from = plan.Status;
            plan.Status = target;
            plan.RowVersion = Guid.NewGuid();
            plan.UpdateDateTime = now.UtcDateTime;
            plan.UpdateBy = actor;
            _historyWriter.Record(
                ProcurementDocumentTypes.NeedPlan, plan.Id, plan.PlanNumber, WorkflowActionOf(target), from, target,
                workflow.CancellationReason, new { workflow.WorkflowStatus }, actor, now, workflow.Id);
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (ownTransaction != null) await ownTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            if (ownTransaction != null) await ownTransaction.DisposeAsync();
        }

        result.CurrentReferenceStatus = plan.Status;
        result.StatusChanged = true;
        await AuditAsync("NeedPlan.WorkflowSync", plan, actor);
        return result;
    }

    // ------------------------------------------------------------------ bantuan

    private async Task<IQueryable<PrcNeedPlan>> ScopedAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var scope = await _scopeService.ResolveAsync(user, ProcurementPermissions.NeedPlanResource, cancellationToken);
        var query = _dbContext.PrcNeedPlans.AsNoTracking().Where(x => !x.IsDelete);
        if (scope.IsAllUnits) return query;
        var departments = scope.DepartmentIds.ToList();
        return query.Where(x => departments.Contains(x.DepartmentId));
    }

    private async Task EnsureVisibleAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        if (!await query.AnyAsync(x => x.Id == id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
    }

    private Task<bool> HasUnfinishedVersionAsync(Guid departmentId, int fiscalYear, Guid exceptId, CancellationToken cancellationToken)
    {
        var unfinished = ProcurementStatuses.Plan.Unfinished.ToList();
        return _dbContext.PrcNeedPlans.AsNoTracking().AnyAsync(x =>
            x.DepartmentId == departmentId && x.FiscalYear == fiscalYear && x.Id != exceptId &&
            !x.IsDelete && unfinished.Contains(x.Status), cancellationToken);
    }

    /// <summary>
    /// PBJ-VAL-042: kuantitas revisi tidak boleh di bawah yang sudah dipakai PR aktif atau dialokasikan
    /// ke rencana paket aktif. Baris yang dihapus dari revisi diperlakukan sebagai kuantitas 0.
    /// </summary>
    private async Task EnsureNotBelowUsageAsync(Guid previousVersionId, List<NeedPlanLineResponse> lines, CancellationToken cancellationToken)
    {
        var previousLines = await ProjectLines(previousVersionId).ToListAsync(cancellationToken);
        var keys = lines.Select(x => x.LineKey).Union(previousLines.Select(x => x.LineKey)).ToList();
        var usage = await _quotaService.GetUsageAsync(keys, null, cancellationToken);

        var lineNo = 0;
        foreach (var line in lines)
        {
            lineNo++;
            Check(line.LineKey, line.ItemName, line.MeasurementName, line.Quantity, lineNo);
        }
        foreach (var removed in previousLines.Where(p => lines.All(l => l.LineKey != p.LineKey)))
            Check(removed.LineKey, removed.ItemName, removed.MeasurementName, 0, null);

        void Check(Guid key, string itemName, string unit, decimal newQuantity, int? no)
        {
            var used = usage.GetValueOrDefault(key) ?? PlanLineUsage.None;
            var floor = Math.Max(used.UsedByPurchaseRequest, used.AllocatedToPackage);
            if (newQuantity < floor)
                throw new ProcurementPlanningValidationException(
                    "PBJ-VAL-042",
                    $"Baris {itemName}: sudah dipakai {Format(floor)} {unit}, tidak bisa diturunkan ke {Format(newQuantity)}.",
                    no);
        }
    }

    private static string Format(decimal value) => value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);

    private async Task<string> AllocateNumberAsync(Guid actorUserId, CancellationToken cancellationToken)
    {
        try
        {
            return await _numberAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: ProcurementNumberSeries.NeedPlanSequenceKey,
                    Prefix: ProcurementNumberSeries.NeedPlanPrefix,
                    ResetPolicy: ProcurementNumberSeries.ResetPolicy,
                    SequenceDigits: ProcurementNumberSeries.SequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow),
                cancellationToken);
        }
        catch (NumberSeriesAllocationException)
        {
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", "Nomor rencana gagal dibuat. Silakan coba lagi.");
        }
    }

    /// <summary>Galat Wfl → PBJ-VAL-091 bila konfigurasi approval belum siap, selain itu 409 dengan pesan Wfl.</summary>
    internal static void ThrowIfApprovalFailed(ProcurementApprovalResult approval)
    {
        if (approval.Success) return;
        if (approval.ConfigurationMissing)
            throw new ProcurementPlanningConflictException(
                "PBJ-VAL-091", "Alur persetujuan pengadaan belum dikonfigurasi. Hubungi administrator.");
        throw new ProcurementPlanningConflictException("PBJ-VAL-091", approval.Message);
    }

    internal static string WorkflowActionOf(string documentStatus) => documentStatus switch
    {
        ProcurementStatuses.Plan.Approved => ProcurementDocumentHistoryActions.WorkflowApproved,
        ProcurementStatuses.Plan.Rejected => ProcurementDocumentHistoryActions.WorkflowRejected,
        _ => ProcurementDocumentHistoryActions.WorkflowReturned
    };

    private static Guid RequireUser(ClaimsPrincipal user)
    {
        var userId = ProcurementDataScopeService.GetUserId(user);
        if (userId == Guid.Empty)
            throw new ProcurementPlanningForbiddenException("Identitas pengguna tidak dikenali.");
        return userId;
    }

    private async Task RequirePlacementAsync(Guid userId, Guid departmentId, CancellationToken cancellationToken)
    {
        var placements = await _scopeService.GetPlacementDepartmentIdsAsync(userId, cancellationToken);
        if (!placements.Contains(departmentId))
        {
            var name = await _dbContext.MstDepartments.AsNoTracking()
                .Where(x => x.Id == departmentId).Select(x => x.DepartmentName).FirstOrDefaultAsync(cancellationToken);
            throw new ProcurementPlanningValidationException("PBJ-VAL-010", $"Anda tidak terdaftar di unit {name ?? "yang dipilih"}.");
        }
    }

    private sealed record LineValues(
        Guid? LineKey, Guid ItemId, decimal Quantity, decimal UnitPrice, decimal Total, int? NeededMonth,
        Guid? FundSourceId, string? Justification, string? Specification);

    private async Task<List<LineValues>> ValidateLinesAsync(
        List<NeedPlanLineRequest>? lines,
        IReadOnlyDictionary<Guid, PrcNeedPlanLine>? existingKeys,
        CancellationToken cancellationToken)
    {
        lines ??= new List<NeedPlanLineRequest>();
        var itemIds = lines.Select(x => x.ItemId).Distinct().ToList();
        var items = await _dbContext.MstProcurementItems.AsNoTracking()
            .Where(x => itemIds.Contains(x.Id) && !x.IsDelete)
            .Select(x => new { x.Id, x.ItemCode, x.IsActive })
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var fundIds = lines.Where(x => x.FundSourceId.HasValue).Select(x => x.FundSourceId!.Value).Distinct().ToList();
        var activeFunds = await _dbContext.MstProcurementFundSources.AsNoTracking()
            .Where(x => fundIds.Contains(x.Id) && !x.IsDelete && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var result = new List<LineValues>();
        var seenItems = new HashSet<Guid>();
        var seenKeys = new HashSet<Guid>();
        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];
            var no = index + 1;
            PrcNeedPlanLine? existing = null;
            if (line.LineKey.HasValue)
            {
                if (existingKeys == null || !existingKeys.TryGetValue(line.LineKey.Value, out existing) || !seenKeys.Add(line.LineKey.Value))
                    throw new ProcurementPlanningValidationException("PBJ-VAL-044", $"Baris {no} tidak dikenal pada rencana ini. Muat ulang rencana.", no);
                if (existing.ItemId != line.ItemId)
                    throw new ProcurementPlanningValidationException(
                        "PBJ-VAL-044", $"Barang pada baris {no} tidak dapat diganti. Hapus baris itu lalu tambah baris baru.", no);
            }

            if (!items.TryGetValue(line.ItemId, out var item) || (existing == null && !item.IsActive))
                throw new ProcurementPlanningValidationException(
                    "PBJ-VAL-005", $"Barang pada baris {no} tidak aktif di katalog. Pilih barang lain atau minta Tim Pengadaan mengaktifkannya.", no);
            if (!seenItems.Add(line.ItemId))
                throw new ProcurementPlanningValidationException("PBJ-VAL-044", $"Barang {item.ItemCode} sudah ada di rencana ini. Gabungkan jumlahnya.", no);
            if (line.Quantity <= 0)
                throw new ProcurementPlanningValidationException("PBJ-VAL-002", $"Jumlah pada baris {no} harus lebih dari 0.", no);
            if (line.EstimatedUnitPrice < 0)
                throw new ProcurementPlanningValidationException("PBJ-VAL-004", "Perkiraan harga tidak boleh negatif.", no);
            if (line.NeededMonth is < 1 or > 12)
                throw new ProcurementPlanningValidationException("PBJ-VAL-044", $"Bulan kebutuhan baris {no} harus 1–12.", no);
            if (line.FundSourceId.HasValue && !activeFunds.Contains(line.FundSourceId.Value))
                throw new ProcurementPlanningValidationException("PBJ-VAL-011", "Pilih sumber dana yang masih aktif.", no);

            result.Add(new LineValues(
                line.LineKey,
                line.ItemId,
                line.Quantity,
                line.EstimatedUnitPrice,
                Math.Round(line.Quantity * line.EstimatedUnitPrice, 2, MidpointRounding.AwayFromZero),
                line.NeededMonth,
                line.FundSourceId,
                Clean(line.Justification, 1000, $"Alasan baris {no}"),
                Clean(line.Specification, 1000, $"Spesifikasi baris {no}")));
        }

        return result;
    }

    private void AddLine(PrcNeedPlan plan, LineValues line, Guid lineKey, Guid actorUserId, DateTime now) =>
        _dbContext.PrcNeedPlanLines.Add(new PrcNeedPlanLine
        {
            NeedPlanId = plan.Id,
            LineKey = lineKey,
            ItemId = line.ItemId,
            Quantity = line.Quantity,
            EstimatedUnitPrice = line.UnitPrice,
            EstimatedTotal = line.Total,
            NeededMonth = line.NeededMonth,
            FundSourceId = line.FundSourceId,
            Justification = line.Justification,
            Specification = line.Specification,
            CreateDateTime = now,
            CreateBy = actorUserId
        });

    private static string? Clean(string? value, int maxLength, string label)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ProcurementPlanningValidationException("PBJ-VAL-044", $"{label} maksimal {maxLength} karakter.");
        return trimmed;
    }

    private static void ApplyFlags(NeedPlanResponse response, bool hasUnfinishedRevision)
    {
        response.StatusLabel = ProcurementStatuses.Plan.LabelOf(response.Status);
        response.CanEdit = ProcurementStatuses.Plan.IsEditable(response.Status);
        response.CanSubmit = response.CanEdit;
        response.CanCancel = response.Status == ProcurementStatuses.Plan.Draft;
        response.CanRevise = response.Status == ProcurementStatuses.Plan.Approved && response.IsCurrentVersion && !hasUnfinishedRevision;
    }

    private IQueryable<NeedPlanLineResponse> ProjectLines(Guid planId) =>
        _dbContext.PrcNeedPlanLines.AsNoTracking()
            .Where(x => x.NeedPlanId == planId && !x.IsDelete)
            .OrderBy(x => x.Item!.ItemCode)
            .Select(x => new NeedPlanLineResponse
            {
                Id = x.Id,
                LineKey = x.LineKey,
                ItemId = x.ItemId,
                ItemCode = x.Item!.ItemCode,
                ItemName = x.Item.ItemName,
                ProcurementTypeCode = x.Item.ProcurementType!.TypeCode,
                ItemCategoryId = x.Item.ItemCategoryId,
                ItemCategoryName = x.Item.ItemCategory!.CategoryName,
                MeasurementName = x.Item.Measurement!.MeasurementName,
                Quantity = x.Quantity,
                EstimatedUnitPrice = x.EstimatedUnitPrice,
                EstimatedTotal = x.EstimatedTotal,
                NeededMonth = x.NeededMonth,
                FundSourceId = x.FundSourceId,
                FundSourceName = x.FundSource != null ? x.FundSource.FundSourceName : null,
                Justification = x.Justification,
                Specification = x.Specification
            });

    private IQueryable<NeedPlanResponse> Project(IQueryable<PrcNeedPlan> query) =>
        query.Select(x => new NeedPlanResponse
        {
            Id = x.Id,
            PlanNumber = x.PlanNumber,
            FiscalYear = x.FiscalYear,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.Department!.DepartmentName,
            VersionNo = x.VersionNo,
            PreviousVersionId = x.PreviousVersionId,
            IsCurrentVersion = x.IsCurrentVersion,
            Status = x.Status,
            RevisionReason = x.RevisionReason,
            EstimatedTotal = x.EstimatedTotal,
            LineCount = x.Lines.Count(l => !l.IsDelete),
            SubmissionCount = x.SubmissionCount,
            WorkflowInstanceId = x.WorkflowInstanceId,
            SubmittedAt = x.SubmittedAt,
            ApprovedAt = x.ApprovedAt,
            RowVersion = x.RowVersion,
            CreateBy = x.CreateBy,
            CreatedByName = _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName).FirstOrDefault(),
            CreateDateTime = x.CreateDateTime,
            UpdateDateTime = x.UpdateDateTime
        });

    private Task AuditAsync(string action, PrcNeedPlan entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan rencana kebutuhan pengadaan.", new
        {
            EntityId = entity.Id,
            entity.Status,
            entity.VersionNo,
            ActorUserId = actorUserId
        });
}

/// <summary>Penyedia akses lampiran untuk rencana kebutuhan (BE-PBJ-009).</summary>
public sealed class NeedPlanAttachmentAccessProvider : IProcurementDocumentAccessProvider
{
    private readonly ProcurementNeedPlanService _service;

    public NeedPlanAttachmentAccessProvider(ProcurementNeedPlanService service) => _service = service;

    public string DocumentType => ProcurementDocumentTypes.NeedPlan;

    public Task<ProcurementDocumentAccessLevel> GetAccessAsync(ClaimsPrincipal user, Guid documentId, CancellationToken cancellationToken) =>
        _service.GetAccessLevelAsync(user, documentId, cancellationToken);
}

/// <summary>Penerap status Wfl untuk rencana kebutuhan (<c>PBJ_NEED_PLAN</c>, BE-PBJ-009).</summary>
/// <remarks>
/// Service diambil saat dipanggil, bukan lewat konstruktor: service rencana memakai gerbang approval
/// → <c>WorkflowService</c> → penangan lifecycle → penerap ini, sehingga injeksi langsung membentuk
/// lingkaran dependensi.
/// </remarks>
public sealed class NeedPlanWorkflowStatusApplier : IProcurementWorkflowStatusApplier
{
    private readonly IServiceProvider _serviceProvider;

    public NeedPlanWorkflowStatusApplier(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public string DocumentType => ProcurementDocumentTypes.NeedPlan;

    public Task<WorkflowReferenceLifecycleSynchronizationResult> ApplyAsync(
        TrxWorkflowInstance workflow, Guid actorUserId, CancellationToken cancellationToken) =>
        _serviceProvider.GetRequiredService<ProcurementNeedPlanService>()
            .ApplyWorkflowStatusAsync(workflow, actorUserId, cancellationToken);
}
