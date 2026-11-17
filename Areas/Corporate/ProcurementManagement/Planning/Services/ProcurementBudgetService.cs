using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Services;

/// <summary>
/// Pagu pengadaan berversi — draf, ajukan, revisi, sisa pagu, dan penerapan hasil approval
/// (BE-PBJ-010; <c>state-transition-matrix.md</c> bagian 4, <c>validation-matrix.md</c> bagian 2).
/// </summary>
/// <remarks>
/// <para><b>Sisa pagu</b> = nilai versi berlaku − Σ komitmen aktif pada garis keturunannya, selalu
/// dihitung server. Pagu yang tidak ada dianggap sisa 0 (PBJ-DEC-030). Contoh: pagu Radiologi Rp 500
/// juta, komitmen Rp 120 juta → sisa Rp 380 juta.</para>
/// <para><b>Cakupan unit (PBJ-DEC-036).</b> Query disaring resource <c>ProcurementBudget</c>; Keuangan
/// yang diberi <c>ProcurementBudget : ReadAllUnits</c> melihat semua unit.</para>
/// <para><b>Approval</b> lewat definisi <c>PBJ-PAGU</c>, atas nama pengguna yang mengajukan.</para>
/// </remarks>
public sealed class ProcurementBudgetService
{
    private const string LogCategory = "Corporate.ProcurementManagement.Planning";
    private const string NotFoundMessage = "Pagu tidak ditemukan atau bukan milik unit Anda.";
    private const string InvalidTransitionMessage = "Status pagu tidak memungkinkan tindakan ini.";
    private const string StaleMessage = "Data sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda.";

    private readonly ApplicationDbContext _dbContext;
    private readonly ProcurementDataScopeService _scopeService;
    private readonly ProcurementDocumentHistoryWriter _historyWriter;
    private readonly ProcurementApprovalGateway _approvalGateway;
    private readonly LoggerService _loggerService;

    public ProcurementBudgetService(
        ApplicationDbContext dbContext,
        ProcurementDataScopeService scopeService,
        ProcurementDocumentHistoryWriter historyWriter,
        ProcurementApprovalGateway approvalGateway,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _scopeService = scopeService;
        _historyWriter = historyWriter;
        _approvalGateway = approvalGateway;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------ baca

    public BudgetFilterMetadataResponse GetFilterMetadata()
    {
        var year = DateTime.UtcNow.Year;
        return new BudgetFilterMetadataResponse
        {
            DefaultFilter = new BudgetQuery(),
            StatusOptions = ProcurementStatuses.Plan.Labels
                .Select(x => new BudgetOptionValue { Value = x.Key, Label = x.Value }).ToList(),
            SortOptions =
            [
                new() { Value = "createdAt", Label = "Tanggal dibuat" },
                new() { Value = "fiscalYear", Label = "Tahun anggaran" },
                new() { Value = "amount", Label = "Nilai pagu" },
                new() { Value = "department", Label = "Unit" }
            ],
            FiscalYearOptions = Enumerable.Range(year - 1, 4).ToList()
        };
    }

    public async Task<BudgetSummaryResponse> GetSummaryAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var byStatus = await query
            .GroupBy(x => x.Status)
            .Select(g => new BudgetStatusCount { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var current = await query
            .Where(x => x.IsCurrentVersion && x.Status == ProcurementStatuses.Plan.Approved)
            .Select(x => new { x.LineageKey, x.Amount })
            .ToListAsync(cancellationToken);
        var committed = await CommittedByLineageAsync(current.Select(x => x.LineageKey).ToList(), cancellationToken);
        var currentAmount = current.Sum(x => x.Amount);
        var committedAmount = committed.Values.Sum();

        return new BudgetSummaryResponse
        {
            Total = byStatus.Sum(x => x.Count),
            PendingApproval = byStatus.Where(x => x.Status == ProcurementStatuses.Plan.PendingApproval).Sum(x => x.Count),
            CurrentAmount = currentAmount,
            CommittedAmount = committedAmount,
            RemainingAmount = currentAmount - committedAmount,
            ByStatus = byStatus.OrderBy(x => x.Status).ToList()
        };
    }

    public async Task<PagedResult<BudgetResponse>> GetPagedAsync(ClaimsPrincipal user, BudgetQuery request, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        if (!request.IncludeHistory) query = query.Where(x => x.Status != ProcurementStatuses.Plan.Superseded);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status.Trim());
        if (request.FiscalYear.HasValue) query = query.Where(x => x.FiscalYear == request.FiscalYear.Value);
        if (request.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
        if (request.ItemCategoryId.HasValue) query = query.Where(x => x.ItemCategoryId == request.ItemCategoryId.Value);
        if (request.FundSourceId.HasValue) query = query.Where(x => x.FundSourceId == request.FundSourceId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x =>
                x.Department!.DepartmentName.ToUpper().Contains(search) ||
                x.ItemCategory!.CategoryName.ToUpper().Contains(search));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "fiscalyear" => descending ? query.OrderByDescending(x => x.FiscalYear) : query.OrderBy(x => x.FiscalYear),
            "amount" => descending ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount),
            "department" => descending ? query.OrderByDescending(x => x.Department!.DepartmentName) : query.OrderBy(x => x.Department!.DepartmentName),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await Project(query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize))
            .ToListAsync(cancellationToken);
        await FillCommitmentsAsync(items, cancellationToken);

        return new PagedResult<BudgetResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = request.PageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    public async Task<BudgetResponse> GetByIdAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var response = await Project(query.Where(x => x.Id == id)).FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);
        await FillCommitmentsAsync([response], cancellationToken);
        if (response.CanRevise && await HasUnfinishedVersionAsync(response.LineageKey, response.Id, cancellationToken))
            response.CanRevise = false;
        return response;
    }

    /// <summary>
    /// Sisa pagu per kategori × sumber dana untuk satu unit-tahun, dari versi berlaku yang disetujui.
    /// Kombinasi tanpa pagu tetap muncul bila kategori dan sumber dananya disaring, dengan sisa 0.
    /// </summary>
    public Task<List<BudgetRemainingResponse>> GetRemainingAsync(ClaimsPrincipal user, BudgetRemainingQuery request, CancellationToken cancellationToken) =>
        GetRemainingAsync(user, request, ProcurementPermissions.BudgetResource, cancellationToken);

    /// <summary>
    /// Sisa pagu yang sama untuk ringkasan layar PR (PBJ-DEC-042): cakupan unit mengikuti hak baca PR,
    /// sehingga pengusul melihat sisa pagu unitnya tanpa diberi akses ke dokumen pagu.
    /// </summary>
    public Task<List<BudgetRemainingResponse>> GetRemainingForPurchaseRequestAsync(ClaimsPrincipal user, BudgetRemainingQuery request, CancellationToken cancellationToken) =>
        GetRemainingAsync(user, request, ProcurementPermissions.PurchaseRequestResource, cancellationToken);

    private async Task<List<BudgetRemainingResponse>> GetRemainingAsync(
        ClaimsPrincipal user, BudgetRemainingQuery request, string scopeResource, CancellationToken cancellationToken)
    {
        var scope = await _scopeService.ResolveAsync(user, scopeResource, cancellationToken);
        if (!scope.CanSee(request.DepartmentId))
            throw new KeyNotFoundException(NotFoundMessage);

        var query = _dbContext.PrcBudgets.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsCurrentVersion && x.Status == ProcurementStatuses.Plan.Approved &&
                        x.FiscalYear == request.FiscalYear && x.DepartmentId == request.DepartmentId);
        if (request.ItemCategoryId.HasValue) query = query.Where(x => x.ItemCategoryId == request.ItemCategoryId.Value);
        if (request.FundSourceId.HasValue) query = query.Where(x => x.FundSourceId == request.FundSourceId.Value);

        var rows = await query
            .Select(x => new BudgetRemainingResponse
            {
                FiscalYear = x.FiscalYear,
                DepartmentId = x.DepartmentId,
                ItemCategoryId = x.ItemCategoryId,
                ItemCategoryName = x.ItemCategory!.CategoryName,
                FundSourceId = x.FundSourceId,
                FundSourceName = x.FundSource!.FundSourceName,
                BudgetId = x.Id,
                HasBudget = true,
                Amount = x.Amount
            })
            .ToListAsync(cancellationToken);

        var lineage = await _dbContext.PrcBudgets.AsNoTracking()
            .Where(x => rows.Select(r => r.BudgetId).Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.LineageKey, cancellationToken);
        var committed = await CommittedByLineageAsync(lineage.Values.ToList(), cancellationToken);
        foreach (var row in rows)
        {
            row.CommittedAmount = committed.GetValueOrDefault(lineage[row.BudgetId!.Value]);
            row.RemainingAmount = row.Amount - row.CommittedAmount;
        }

        if (rows.Count == 0 && request.ItemCategoryId.HasValue && request.FundSourceId.HasValue)
        {
            var names = await _dbContext.MstProcurementItemCategories.AsNoTracking()
                .Where(x => x.Id == request.ItemCategoryId.Value).Select(x => x.CategoryName).FirstOrDefaultAsync(cancellationToken);
            var fund = await _dbContext.MstProcurementFundSources.AsNoTracking()
                .Where(x => x.Id == request.FundSourceId.Value).Select(x => x.FundSourceName).FirstOrDefaultAsync(cancellationToken);
            rows.Add(new BudgetRemainingResponse
            {
                FiscalYear = request.FiscalYear,
                DepartmentId = request.DepartmentId,
                ItemCategoryId = request.ItemCategoryId.Value,
                ItemCategoryName = names ?? string.Empty,
                FundSourceId = request.FundSourceId.Value,
                FundSourceName = fund ?? string.Empty,
                HasBudget = false
            });
        }

        return rows.OrderBy(x => x.ItemCategoryName).ThenBy(x => x.FundSourceName).ToList();
    }

    /// <summary>
    /// Sisa pagu per kategori untuk uji lewat pagu saat verifikasi PR (PBJ-DEC-030), tanpa
    /// penyaringan cakupan pengguna (dipanggil service server). Kategori tanpa pagu berlaku → 0.
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, decimal>> GetRemainingByCategoryAsync(
        int fiscalYear, Guid departmentId, Guid fundSourceId, IReadOnlyCollection<Guid> itemCategoryIds, CancellationToken cancellationToken)
    {
        var categories = itemCategoryIds.Distinct().ToList();
        var budgets = await _dbContext.PrcBudgets.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsCurrentVersion && x.Status == ProcurementStatuses.Plan.Approved &&
                        x.FiscalYear == fiscalYear && x.DepartmentId == departmentId && x.FundSourceId == fundSourceId &&
                        categories.Contains(x.ItemCategoryId))
            .Select(x => new { x.ItemCategoryId, x.LineageKey, x.Amount })
            .ToListAsync(cancellationToken);
        var committed = await CommittedByLineageAsync(budgets.Select(x => x.LineageKey).ToList(), cancellationToken);

        return categories.ToDictionary(
            category => category,
            category => budgets.Where(x => x.ItemCategoryId == category)
                .Sum(x => x.Amount - committed.GetValueOrDefault(x.LineageKey)));
    }

    public async Task<List<DocumentHistoryResponse>> GetStatusHistoryAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        if (!await query.AnyAsync(x => x.Id == id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
        return await _historyWriter.ReadAsync(ProcurementDocumentTypes.Budget, id, cancellationToken);
    }

    /// <summary>Hak atas pagu untuk lampiran: tidak terlihat → None; DRAFT/RETURNED → Edit; selain itu → Read.</summary>
    public async Task<ProcurementDocumentAccessLevel> GetAccessLevelAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var status = await query.Where(x => x.Id == id).Select(x => x.Status).FirstOrDefaultAsync(cancellationToken);
        if (status == null) return ProcurementDocumentAccessLevel.None;
        return ProcurementStatuses.Plan.IsEditable(status) ? ProcurementDocumentAccessLevel.Edit : ProcurementDocumentAccessLevel.Read;
    }

    // ------------------------------------------------------------------ tulis

    public async Task<BudgetResponse> CreateAsync(ClaimsPrincipal user, CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await ValidateKeyAsync(user, request.FiscalYear, request.DepartmentId, request.ItemCategoryId, request.FundSourceId, cancellationToken);
        ValidateAmount(request.Amount);
        await EnsureUniqueKeyAsync(request.FiscalYear, request.DepartmentId, request.ItemCategoryId, request.FundSourceId, null, cancellationToken);

        var now = DateTime.UtcNow;
        var entity = new PrcBudget
        {
            FiscalYear = request.FiscalYear,
            DepartmentId = request.DepartmentId,
            ItemCategoryId = request.ItemCategoryId,
            FundSourceId = request.FundSourceId,
            Amount = request.Amount,
            VersionNo = 1,
            Status = ProcurementStatuses.Plan.Draft,
            CreateDateTime = now,
            CreateBy = actorUserId
        };
        entity.LineageKey = entity.Id;
        _dbContext.PrcBudgets.Add(entity);
        _historyWriter.Record(
            ProcurementDocumentTypes.Budget, entity.Id, null, ProcurementDocumentHistoryActions.Create,
            null, entity.Status, null, new { entity.Amount, entity.VersionNo }, actorUserId, DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await AuditAsync("Budget.Create", entity, actorUserId);
        return await GetByIdAsync(user, entity.Id, cancellationToken);
    }

    public async Task<BudgetResponse> UpdateAsync(ClaimsPrincipal user, Guid id, UpdateBudgetRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await EnsureVisibleAsync(user, id, cancellationToken);

        var entity = await _dbContext.PrcBudgets.FirstAsync(x => x.Id == id, cancellationToken);
        if (!ProcurementStatuses.Plan.IsEditable(entity.Status))
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", InvalidTransitionMessage);
        if (entity.RowVersion != request.ExpectedRowVersion)
            throw new ProcurementPlanningConflictException("PBJ-VAL-090", StaleMessage);
        ValidateAmount(request.Amount);

        var keyChanged = entity.FiscalYear != request.FiscalYear || entity.DepartmentId != request.DepartmentId ||
                         entity.ItemCategoryId != request.ItemCategoryId || entity.FundSourceId != request.FundSourceId;
        if (keyChanged)
        {
            if (entity.PreviousVersionId.HasValue || entity.SubmissionCount > 0)
                throw new ProcurementPlanningValidationException(
                    "PBJ-VAL-060", "Tahun, unit, kategori, dan sumber dana hanya dapat diganti pada pagu baru yang belum pernah diajukan.");
            await ValidateKeyAsync(user, request.FiscalYear, request.DepartmentId, request.ItemCategoryId, request.FundSourceId, cancellationToken);
            await EnsureUniqueKeyAsync(request.FiscalYear, request.DepartmentId, request.ItemCategoryId, request.FundSourceId, entity.Id, cancellationToken);
            entity.FiscalYear = request.FiscalYear;
            entity.DepartmentId = request.DepartmentId;
            entity.ItemCategoryId = request.ItemCategoryId;
            entity.FundSourceId = request.FundSourceId;
        }

        var now = DateTimeOffset.UtcNow;
        entity.Amount = request.Amount;
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateDateTime = now.UtcDateTime;
        entity.UpdateBy = actorUserId;
        _historyWriter.Record(
            ProcurementDocumentTypes.Budget, entity.Id, null, ProcurementDocumentHistoryActions.Update,
            entity.Status, entity.Status, null, new { entity.Amount, entity.VersionNo }, actorUserId, now);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementPlanningConflictException("PBJ-VAL-090", StaleMessage);
        }

        await AuditAsync("Budget.Update", entity, actorUserId);
        return await GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>Ajukan ke approval <c>PBJ-PAGU</c>. Revisi tidak boleh di bawah komitmen aktif (PBJ-VAL-062).</summary>
    public async Task<BudgetResponse> SubmitAsync(ClaimsPrincipal user, Guid id, string? idempotencyKey, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await EnsureVisibleAsync(user, id, cancellationToken);

        if (await _historyWriter.HasIdempotencyKeyAsync(
                ProcurementDocumentTypes.Budget, id, ProcurementDocumentHistoryActions.Submit, idempotencyKey, cancellationToken))
            return await GetByIdAsync(user, id, cancellationToken);

        var budget = await _dbContext.PrcBudgets.AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);
        if (!ProcurementStatuses.Plan.IsEditable(budget.Status))
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", InvalidTransitionMessage);
        if (budget.PreviousVersionId.HasValue)
        {
            var committed = (await CommittedByLineageAsync([budget.LineageKey], cancellationToken)).GetValueOrDefault(budget.LineageKey);
            if (budget.Amount < committed)
                throw new ProcurementPlanningValidationException(
                    "PBJ-VAL-062", $"Pagu tidak boleh lebih kecil dari komitmen yang sudah berjalan ({FormatRupiah(committed)}).");
        }

        var closed = await _approvalGateway.CloseReturnedAsync(budget.WorkflowInstanceId, "Pagu diajukan ulang setelah diperbaiki.", cancellationToken);
        if (!closed.Success)
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", closed.Message);

        var submissionCount = budget.SubmissionCount + 1;
        var approval = await _approvalGateway.SubmitAsync(
            new ProcurementApprovalRequest(
                ProcurementDocumentTypes.Budget, budget.Id, null, ProcurementWorkflowCodes.BudgetDefinition,
                actorUserId, budget.Amount, Array.Empty<string>(), submissionCount),
            cancellationToken);
        ProcurementNeedPlanService.ThrowIfApprovalFailed(approval);

        var entity = await _dbContext.PrcBudgets.FirstAsync(x => x.Id == id, cancellationToken);
        if (entity.RowVersion != budget.RowVersion || !ProcurementStatuses.Plan.IsEditable(entity.Status))
            throw new ProcurementPlanningConflictException("PBJ-VAL-090", StaleMessage);

        var fromStatus = entity.Status;
        var now = DateTimeOffset.UtcNow;
        entity.Status = ProcurementStatuses.Plan.PendingApproval;
        entity.SubmissionCount = submissionCount;
        entity.WorkflowInstanceId = approval.WorkflowInstanceId;
        entity.SubmittedAt = now;
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateDateTime = now.UtcDateTime;
        entity.UpdateBy = actorUserId;
        _historyWriter.Record(
            ProcurementDocumentTypes.Budget, entity.Id, null, ProcurementDocumentHistoryActions.Submit,
            fromStatus, entity.Status, null,
            new { idempotencyKey = idempotencyKey?.Trim(), entity.Amount, entity.VersionNo, submissionCount },
            actorUserId, now, approval.WorkflowInstanceId);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementPlanningConflictException("PBJ-VAL-090", StaleMessage);
        }

        await AuditAsync("Budget.Submit", entity, actorUserId);
        return await GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>Membuat versi revisi <c>DRAFT</c> dari pagu berlaku; nilai diubah lewat <c>PUT</c>.</summary>
    public async Task<BudgetResponse> CreateRevisionAsync(ClaimsPrincipal user, Guid id, BudgetReasonRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await EnsureVisibleAsync(user, id, cancellationToken);

        var reason = string.IsNullOrWhiteSpace(request.Reason)
            ? throw new ProcurementPlanningValidationException("PBJ-VAL-030", "Alasan revisi wajib diisi.")
            : request.Reason.Trim();
        if (reason.Length > 1000)
            throw new ProcurementPlanningValidationException("PBJ-VAL-030", "Alasan revisi maksimal 1000 karakter.");

        var source = await _dbContext.PrcBudgets.AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);
        if (source.Status != ProcurementStatuses.Plan.Approved || !source.IsCurrentVersion)
            throw new ProcurementPlanningConflictException("PBJ-VAL-093", "Revisi hanya dapat dibuat dari pagu yang disetujui dan berlaku.");
        if (await HasUnfinishedVersionAsync(source.LineageKey, source.Id, cancellationToken))
            throw new ProcurementPlanningConflictException("PBJ-VAL-041", "Masih ada revisi pagu yang belum selesai.");

        var maxVersion = await _dbContext.PrcBudgets.AsNoTracking()
            .Where(x => x.LineageKey == source.LineageKey && !x.IsDelete)
            .MaxAsync(x => x.VersionNo, cancellationToken);
        var now = DateTime.UtcNow;
        var revision = new PrcBudget
        {
            FiscalYear = source.FiscalYear,
            DepartmentId = source.DepartmentId,
            ItemCategoryId = source.ItemCategoryId,
            FundSourceId = source.FundSourceId,
            Amount = source.Amount,
            VersionNo = maxVersion + 1,
            PreviousVersionId = source.Id,
            LineageKey = source.LineageKey,
            Status = ProcurementStatuses.Plan.Draft,
            RevisionReason = reason,
            CreateDateTime = now,
            CreateBy = actorUserId
        };
        _dbContext.PrcBudgets.Add(revision);
        _historyWriter.Record(
            ProcurementDocumentTypes.Budget, revision.Id, null, ProcurementDocumentHistoryActions.Revise,
            null, revision.Status, reason, new { FromVersionId = source.Id, FromVersionNo = source.VersionNo, revision.VersionNo },
            actorUserId, DateTimeOffset.UtcNow);
        _historyWriter.Record(
            ProcurementDocumentTypes.Budget, source.Id, null, ProcurementDocumentHistoryActions.Revise,
            source.Status, source.Status, reason, new { RevisionId = revision.Id, revision.VersionNo },
            actorUserId, DateTimeOffset.UtcNow);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ProcurementPlanningConflictException("PBJ-VAL-041", "Masih ada revisi pagu yang belum selesai.");
        }

        await AuditAsync("Budget.Revise", revision, actorUserId);
        return await GetByIdAsync(user, revision.Id, cancellationToken);
    }

    // ------------------------------------------------------------------ hasil approval

    /// <summary>
    /// Menerapkan status Wfl ke pagu (matriks status bagian 4–5), dengan aturan yang sama seperti
    /// rencana kebutuhan: hanya pagu <c>PENDING_APPROVAL</c> yang tertaut ke instance ini yang
    /// dipindahkan, dan saat disetujui versi berlaku lama menjadi <c>SUPERSEDED</c>.
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

        var budget = await _dbContext.PrcBudgets.FirstOrDefaultAsync(x => x.Id == workflow.ReferenceId && !x.IsDelete, cancellationToken);
        if (budget == null) return result;
        result.PreviousReferenceStatus = budget.Status;
        result.CurrentReferenceStatus = budget.Status;

        var target = ProcurementWorkflowStatusMap.ToDocumentStatus(workflow.WorkflowStatus, isPurchaseRequest: false);
        if (budget.WorkflowInstanceId != workflow.Id ||
            budget.Status != ProcurementStatuses.Plan.PendingApproval ||
            target == null || target == budget.Status)
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
                var current = await _dbContext.PrcBudgets
                    .Where(x => x.FiscalYear == budget.FiscalYear && x.DepartmentId == budget.DepartmentId &&
                                x.ItemCategoryId == budget.ItemCategoryId && x.FundSourceId == budget.FundSourceId &&
                                x.IsCurrentVersion && x.Id != budget.Id && !x.IsDelete)
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
                        ProcurementDocumentTypes.Budget, old.Id, null, ProcurementDocumentHistoryActions.Supersede,
                        oldStatus, old.Status, null, new { SupersededById = budget.Id, budget.VersionNo }, actor, now, workflow.Id);
                }
                await _dbContext.SaveChangesAsync(cancellationToken);

                budget.IsCurrentVersion = true;
                budget.ApprovedAt = now;
            }

            var from = budget.Status;
            budget.Status = target;
            budget.RowVersion = Guid.NewGuid();
            budget.UpdateDateTime = now.UtcDateTime;
            budget.UpdateBy = actor;
            _historyWriter.Record(
                ProcurementDocumentTypes.Budget, budget.Id, null, ProcurementNeedPlanService.WorkflowActionOf(target), from, target,
                workflow.CancellationReason, new { workflow.WorkflowStatus, budget.Amount }, actor, now, workflow.Id);
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (ownTransaction != null) await ownTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            if (ownTransaction != null) await ownTransaction.DisposeAsync();
        }

        result.CurrentReferenceStatus = budget.Status;
        result.StatusChanged = true;
        await AuditAsync("Budget.WorkflowSync", budget, actor);
        return result;
    }

    // ------------------------------------------------------------------ bantuan

    private async Task<IQueryable<PrcBudget>> ScopedAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var scope = await _scopeService.ResolveAsync(user, ProcurementPermissions.BudgetResource, cancellationToken);
        var query = _dbContext.PrcBudgets.AsNoTracking().Where(x => !x.IsDelete);
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

    private async Task ValidateKeyAsync(ClaimsPrincipal user, int fiscalYear, Guid departmentId, Guid itemCategoryId, Guid fundSourceId, CancellationToken cancellationToken)
    {
        if (fiscalYear is < 2000 or > 2100)
            throw new ProcurementPlanningValidationException("PBJ-VAL-043", "Tahun anggaran tidak valid.");
        var scope = await _scopeService.ResolveAsync(user, ProcurementPermissions.BudgetResource, cancellationToken);
        if (!scope.CanSee(departmentId) ||
            !await _dbContext.MstDepartments.AnyAsync(x => x.Id == departmentId && !x.IsDelete, cancellationToken))
            throw new ProcurementPlanningValidationException("PBJ-VAL-010", "Unit tidak ditemukan atau di luar cakupan Anda.");
        if (!await _dbContext.MstProcurementItemCategories.AnyAsync(x => x.Id == itemCategoryId && !x.IsDelete && x.IsActive, cancellationToken))
            throw new ProcurementPlanningValidationException("PBJ-VAL-060", "Pilih kategori barang yang masih aktif.");
        if (!await _dbContext.MstProcurementFundSources.AnyAsync(x => x.Id == fundSourceId && !x.IsDelete && x.IsActive, cancellationToken))
            throw new ProcurementPlanningValidationException("PBJ-VAL-011", "Pilih sumber dana yang masih aktif.");
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ProcurementPlanningValidationException("PBJ-VAL-061", "Nilai pagu harus lebih dari 0.");
        if (amount >= 10_000_000_000_000_000m)
            throw new ProcurementPlanningValidationException("PBJ-VAL-061", "Nilai pagu terlalu besar.");
    }

    /// <summary>PBJ-VAL-060: satu pagu hidup per tahun × unit × kategori × sumber dana.</summary>
    private async Task EnsureUniqueKeyAsync(int fiscalYear, Guid departmentId, Guid itemCategoryId, Guid fundSourceId, Guid? exceptId, CancellationToken cancellationToken)
    {
        var existingId = await _dbContext.PrcBudgets.AsNoTracking()
            .Where(x => x.FiscalYear == fiscalYear && x.DepartmentId == departmentId &&
                        x.ItemCategoryId == itemCategoryId && x.FundSourceId == fundSourceId && !x.IsDelete &&
                        (!exceptId.HasValue || x.Id != exceptId.Value) &&
                        x.Status != ProcurementStatuses.Plan.Cancelled &&
                        x.Status != ProcurementStatuses.Plan.Rejected &&
                        x.Status != ProcurementStatuses.Plan.Superseded)
            .OrderByDescending(x => x.IsCurrentVersion).ThenByDescending(x => x.VersionNo)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (existingId.HasValue)
            throw new ProcurementPlanningConflictException("PBJ-VAL-060", "Pagu untuk kombinasi ini sudah ada. Buat revisi.", existingId);
    }

    private Task<bool> HasUnfinishedVersionAsync(Guid lineageKey, Guid exceptId, CancellationToken cancellationToken)
    {
        var unfinished = ProcurementStatuses.Plan.Unfinished.ToList();
        return _dbContext.PrcBudgets.AsNoTracking().AnyAsync(x =>
            x.LineageKey == lineageKey && x.Id != exceptId && !x.IsDelete && unfinished.Contains(x.Status), cancellationToken);
    }

    private async Task<Dictionary<Guid, decimal>> CommittedByLineageAsync(List<Guid> lineageKeys, CancellationToken cancellationToken)
    {
        if (lineageKeys.Count == 0) return new Dictionary<Guid, decimal>();
        var keys = lineageKeys.Distinct().ToList();
        return await _dbContext.PrcBudgetCommitments.AsNoTracking()
            .Where(x => keys.Contains(x.BudgetLineageKey) && x.IsActive && !x.IsDelete)
            .GroupBy(x => x.BudgetLineageKey)
            .Select(g => new { g.Key, Amount = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.Key, x => x.Amount, cancellationToken);
    }

    private async Task FillCommitmentsAsync(List<BudgetResponse> items, CancellationToken cancellationToken)
    {
        var committed = await CommittedByLineageAsync(items.Select(x => x.LineageKey).ToList(), cancellationToken);
        foreach (var item in items)
        {
            item.CommittedAmount = committed.GetValueOrDefault(item.LineageKey);
            item.RemainingAmount = item.Amount - item.CommittedAmount;
            item.StatusLabel = ProcurementStatuses.Plan.LabelOf(item.Status);
            item.CanEdit = ProcurementStatuses.Plan.IsEditable(item.Status);
            item.CanSubmit = item.CanEdit;
            item.CanRevise = item.Status == ProcurementStatuses.Plan.Approved && item.IsCurrentVersion;
        }
    }

    private static string FormatRupiah(decimal value) =>
        "Rp " + value.ToString("#,##0.##", System.Globalization.CultureInfo.GetCultureInfo("id-ID"));

    private static Guid RequireUser(ClaimsPrincipal user)
    {
        var userId = ProcurementDataScopeService.GetUserId(user);
        if (userId == Guid.Empty)
            throw new ProcurementPlanningForbiddenException("Identitas pengguna tidak dikenali.");
        return userId;
    }

    private IQueryable<BudgetResponse> Project(IQueryable<PrcBudget> query) =>
        query.Select(x => new BudgetResponse
        {
            Id = x.Id,
            FiscalYear = x.FiscalYear,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.Department!.DepartmentName,
            ItemCategoryId = x.ItemCategoryId,
            ItemCategoryName = x.ItemCategory!.CategoryName,
            FundSourceId = x.FundSourceId,
            FundSourceName = x.FundSource!.FundSourceName,
            Amount = x.Amount,
            VersionNo = x.VersionNo,
            PreviousVersionId = x.PreviousVersionId,
            LineageKey = x.LineageKey,
            IsCurrentVersion = x.IsCurrentVersion,
            Status = x.Status,
            RevisionReason = x.RevisionReason,
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

    private Task AuditAsync(string action, PrcBudget entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan pagu pengadaan.", new
        {
            EntityId = entity.Id,
            entity.Status,
            entity.VersionNo,
            ActorUserId = actorUserId
        });
}

/// <summary>Penyedia akses lampiran untuk pagu (BE-PBJ-010).</summary>
public sealed class BudgetAttachmentAccessProvider : IProcurementDocumentAccessProvider
{
    private readonly ProcurementBudgetService _service;

    public BudgetAttachmentAccessProvider(ProcurementBudgetService service) => _service = service;

    public string DocumentType => ProcurementDocumentTypes.Budget;

    public Task<ProcurementDocumentAccessLevel> GetAccessAsync(ClaimsPrincipal user, Guid documentId, CancellationToken cancellationToken) =>
        _service.GetAccessLevelAsync(user, documentId, cancellationToken);
}

/// <summary>
/// Penerap status Wfl untuk pagu (<c>PBJ_BUDGET</c>, BE-PBJ-010). Service diambil saat dipanggil agar
/// tidak membentuk lingkaran dependensi dengan <c>WorkflowService</c>.
/// </summary>
public sealed class BudgetWorkflowStatusApplier : IProcurementWorkflowStatusApplier
{
    private readonly IServiceProvider _serviceProvider;

    public BudgetWorkflowStatusApplier(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public string DocumentType => ProcurementDocumentTypes.Budget;

    public Task<WorkflowReferenceLifecycleSynchronizationResult> ApplyAsync(
        TrxWorkflowInstance workflow, Guid actorUserId, CancellationToken cancellationToken) =>
        _serviceProvider.GetRequiredService<ProcurementBudgetService>()
            .ApplyWorkflowStatusAsync(workflow, actorUserId, cancellationToken);
}
