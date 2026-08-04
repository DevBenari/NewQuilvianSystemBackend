using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;
using QuilvianSystemBackend.Shared.HumanResource.Services;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public partial class WorkflowService
    {
        private static readonly HashSet<string> TerminalStepStatuses = new(
            StringComparer.OrdinalIgnoreCase)
        {
            WorkflowValueConstants.StepStatus.Approved,
            WorkflowValueConstants.StepStatus.Completed,
            WorkflowValueConstants.StepStatus.Skipped,
            WorkflowValueConstants.StepStatus.Cancelled,
            WorkflowValueConstants.StepStatus.Rejected
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly HumanResourceContextService _humanResourceContextService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly WorkflowReferenceLifecycleService _workflowReferenceLifecycleService;

        public WorkflowService(
            ApplicationDbContext dbContext,
            HumanResourceContextService humanResourceContextService,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            WorkflowReferenceLifecycleService workflowReferenceLifecycleService)
        {
            _dbContext = dbContext;
            _humanResourceContextService = humanResourceContextService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _workflowReferenceLifecycleService = workflowReferenceLifecycleService;
        }

        public WorkflowFilterMetadataResponse GetFilterMetadata()
        {
            return new WorkflowFilterMetadataResponse
            {
                DefaultFilter = new WorkflowFilterDefaultResponse(),
                PeriodOptions = BuildOptions(new[]
                {
                    ("today", "Hari Ini"),
                    ("last7days", "7 Hari Terakhir"),
                    ("last30days", "30 Hari Terakhir"),
                    ("thismonth", "Bulan Ini"),
                    ("lastmonth", "Bulan Lalu"),
                    ("custom", "Rentang Tanggal")
                }),
                WorkflowStatusOptions = BuildOptions(new[]
                {
                    (WorkflowValueConstants.WorkflowStatus.Draft, "Draft"),
                    (WorkflowValueConstants.WorkflowStatus.Submitted, "Diajukan"),
                    (WorkflowValueConstants.WorkflowStatus.InProgress, "Sedang Diproses"),
                    (WorkflowValueConstants.WorkflowStatus.RevisionRequested, "Perlu Revisi"),
                    (WorkflowValueConstants.WorkflowStatus.Returned, "Dikembalikan"),
                    (WorkflowValueConstants.WorkflowStatus.Completed, "Selesai"),
                    (WorkflowValueConstants.WorkflowStatus.Rejected, "Ditolak"),
                    (WorkflowValueConstants.WorkflowStatus.Cancelled, "Dibatalkan"),
                    (WorkflowValueConstants.WorkflowStatus.Withdrawn, "Ditarik")
                }),
                StepStatusOptions = BuildOptions(new[]
                {
                    (WorkflowValueConstants.StepStatus.Pending, "Menunggu"),
                    (WorkflowValueConstants.StepStatus.Available, "Tersedia"),
                    (WorkflowValueConstants.StepStatus.InProgress, "Sedang Diproses"),
                    (WorkflowValueConstants.StepStatus.Approved, "Disetujui"),
                    (WorkflowValueConstants.StepStatus.Completed, "Selesai"),
                    (WorkflowValueConstants.StepStatus.Skipped, "Dilewati"),
                    (WorkflowValueConstants.StepStatus.Rejected, "Ditolak")
                }),
                AssignmentStatusOptions = BuildOptions(new[]
                {
                    (WorkflowValueConstants.AssignmentStatus.Pending, "Menunggu"),
                    (WorkflowValueConstants.AssignmentStatus.Available, "Tersedia"),
                    (WorkflowValueConstants.AssignmentStatus.InProgress, "Sedang Diproses"),
                    (WorkflowValueConstants.AssignmentStatus.Approved, "Disetujui"),
                    (WorkflowValueConstants.AssignmentStatus.Skipped, "Dilewati"),
                    (WorkflowValueConstants.AssignmentStatus.Delegated, "Didelegasikan")
                }),
                ActionTypeOptions = BuildOptions(new[]
                {
                    (WorkflowValueConstants.ActionType.Submit, "Ajukan"),
                    (WorkflowValueConstants.ActionType.Approve, "Setujui"),
                    (WorkflowValueConstants.ActionType.Reject, "Tolak"),
                    (WorkflowValueConstants.ActionType.RequestRevision, "Minta Revisi"),
                    (WorkflowValueConstants.ActionType.Return, "Kembalikan"),
                    (WorkflowValueConstants.ActionType.Cancel, "Batalkan"),
                    (WorkflowValueConstants.ActionType.Withdraw, "Tarik Pengajuan"),
                    (WorkflowValueConstants.ActionType.Verify, "Verifikasi"),
                    (WorkflowValueConstants.ActionType.Acknowledge, "Ketahui")
                }),
                SourceChannelOptions = BuildOptions(new[]
                {
                    (WorkflowValueConstants.SourceChannel.Web, "Web"),
                    (WorkflowValueConstants.SourceChannel.Mobile, "Mobile"),
                    (WorkflowValueConstants.SourceChannel.Api, "API"),
                    (WorkflowValueConstants.SourceChannel.Integration, "Integrasi")
                }),
                SortOptions = new List<WorkflowSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" },
                    new() { Value = "requestNumber", Label = "Nomor Permintaan" },
                    new() { Value = "workflowName", Label = "Nama Workflow" },
                    new() { Value = "workflowStatus", Label = "Status" },
                    new() { Value = "submittedAt", Label = "Tanggal Diajukan" },
                    new() { Value = "lastActionAt", Label = "Aksi Terakhir" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
        }

        public async Task<WorkflowServiceResult<WorkflowSummaryResponse>> GetSummaryAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? period,
            Guid? workflowDefinitionId,
            string? workflowCode,
            string? referenceType,
            Guid? requestedByWorkforceProfileId,
            string? workflowStatus,
            string? currentStepCode,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var dateRange = ResolveDateRange(startDate, endDate, period);
            if (!dateRange.IsValid)
            {
                return WorkflowServiceResult<WorkflowSummaryResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage!);
            }

            var query = ApplyListFilters(
                BuildListQuery(),
                dateRange,
                workflowDefinitionId,
                workflowCode,
                referenceType,
                requestedByWorkforceProfileId,
                workflowStatus,
                currentStepCode,
                search);

            var result = new WorkflowSummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                Draft = await query.CountAsync(
                    x => x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Draft,
                    cancellationToken),
                Submitted = await query.CountAsync(
                    x => x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Submitted,
                    cancellationToken),
                InProgress = await query.CountAsync(
                    x => x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.InProgress,
                    cancellationToken),
                RevisionRequested = await query.CountAsync(
                    x => x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.RevisionRequested,
                    cancellationToken),
                Returned = await query.CountAsync(
                    x => x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Returned,
                    cancellationToken),
                Completed = await query.CountAsync(
                    x => x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Completed,
                    cancellationToken),
                Rejected = await query.CountAsync(
                    x => x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Rejected,
                    cancellationToken),
                Cancelled = await query.CountAsync(
                    x => x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Cancelled,
                    cancellationToken),
                Withdrawn = await query.CountAsync(
                    x => x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Withdrawn,
                    cancellationToken)
            };

            return WorkflowServiceResult<WorkflowSummaryResponse>.Ok(
                result,
                "Ringkasan workflow instance berhasil diambil.");
        }

        public async Task<WorkflowServiceResult<PagedResult<WorkflowInstanceListResponse>>> GetPagedAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? period,
            Guid? workflowDefinitionId,
            string? workflowCode,
            string? referenceType,
            Guid? requestedByWorkforceProfileId,
            string? workflowStatus,
            string? currentStepCode,
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var dateRange = ResolveDateRange(startDate, endDate, period);
            if (!dateRange.IsValid)
            {
                return WorkflowServiceResult<PagedResult<WorkflowInstanceListResponse>>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage!);
            }

            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = ApplyListFilters(
                BuildListQuery(),
                dateRange,
                workflowDefinitionId,
                workflowCode,
                referenceType,
                requestedByWorkforceProfileId,
                workflowStatus,
                currentStepCode,
                search);

            var totalData = await query.CountAsync(cancellationToken);
            var ordered = ApplyOrdering(query, sortBy, sortDirection);

            var items = await ordered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkflowInstanceListResponse
                {
                    Id = x.Id,
                    RequestNumber = x.RequestNumber,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowCode = x.WorkflowDefinition != null
                        ? x.WorkflowDefinition.WorkflowCode
                        : string.Empty,
                    WorkflowName = x.WorkflowDefinition != null
                        ? x.WorkflowDefinition.WorkflowName
                        : string.Empty,
                    WorkflowVersion = x.WorkflowDefinition != null
                        ? x.WorkflowDefinition.Version
                        : 0,
                    ReferenceType = x.ReferenceType,
                    ReferenceId = x.ReferenceId,
                    ExternalReferenceNumber = x.ExternalReferenceNumber,
                    RequestedByUserId = x.RequestedByUserId,
                    RequestedByWorkforceProfileId = x.RequestedByWorkforceProfileId,
                    RequestedByProfileCode = x.RequestedByWorkforceProfile != null
                        ? x.RequestedByWorkforceProfile.ProfileCode
                        : null,
                    RequestedByName = x.RequestedByWorkforceProfile != null
                        ? x.RequestedByWorkforceProfile.DisplayName
                        : x.RequestedByUser != null
                            ? x.RequestedByUser.DisplayName ??
                              x.RequestedByUser.UserName ??
                              x.RequestedByUser.Email ??
                              x.RequestedByUser.UserCode
                            : string.Empty,
                    WorkflowStatus = x.WorkflowStatus,
                    CurrentStepOrder = x.CurrentStepOrder,
                    CurrentStepCode = x.CurrentStepCode,
                    SourceChannel = x.SourceChannel,
                    CreateDateTime = x.CreateDateTime,
                    SubmittedAt = x.SubmittedAt,
                    LastActionAt = x.LastActionAt,
                    CompletedAt = x.CompletedAt
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WorkflowInstanceListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return WorkflowServiceResult<PagedResult<WorkflowInstanceListResponse>>.Ok(
                result,
                "Data workflow instance berhasil diambil.");
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id tidak valid.");
            }

            HumanResourceUserContextDto? actorContext = null;
            try
            {
                actorContext = await _humanResourceContextService.GetCurrentAsync(cancellationToken);
            }
            catch (UnauthorizedAccessException)
            {
                // Detail tetap dapat dibaca oleh permission controller.
                // AvailableActions akan kosong jika context login tidak dapat diselesaikan.
            }

            var detail = await LoadDetailAsync(id, actorContext, cancellationToken);
            if (detail == null)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow instance tidak ditemukan.");
            }

            return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                detail,
                "Detail workflow instance berhasil diambil.");
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> CreateAsync(
            CreateWorkflowInstanceRequest request,
            CancellationToken cancellationToken = default)
        {
            var validationMessage = ValidateCreateRequest(request);
            if (validationMessage != null)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    validationMessage);
            }

            HumanResourceUserContextDto actorContext;
            try
            {
                actorContext = await _humanResourceContextService.GetCurrentAsync(cancellationToken);
            }
            catch (UnauthorizedAccessException ex)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    ex.Message);
            }

            if (!actorContext.WorkforceProfileId.HasValue)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Akun login belum terhubung dengan workforce profile.");
            }

            if (!actorContext.HasOrganizationAssignment)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Penempatan organisasi aktif pemohon belum tersedia.");
            }

            var normalizedCode = request.WorkflowDefinitionCode.Trim().ToUpperInvariant();
            var normalizedReferenceType = request.ReferenceType.Trim();
            var sourceChannel = NormalizeSourceChannel(request.SourceChannel);
            var requestContextJson = SerializeJsonElement(request.RequestContext);

            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var existingId = await _dbContext.Set<TrxWorkflowInstance>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IdempotencyKey == request.IdempotencyKey.Trim())
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingId.HasValue)
                {
                    var existing = await LoadDetailAsync(
                        existingId.Value,
                        actorContext,
                        cancellationToken);

                    if (existing != null)
                    {
                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                            existing,
                            "Workflow instance dengan idempotency key yang sama sudah tersedia.");
                    }
                }
            }

            var duplicateActive = await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ReferenceType == normalizedReferenceType &&
                    x.ReferenceId == request.ReferenceId &&
                    (x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Submitted ||
                     x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.InProgress ||
                     x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.RevisionRequested ||
                     x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Returned),
                    cancellationToken);

            if (duplicateActive)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Transaksi sumber masih memiliki workflow aktif.");
            }

            var definition = await ResolveWorkflowDefinitionAsync(
                normalizedCode,
                actorContext,
                cancellationToken);

            if (definition == null)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Workflow definition aktif dengan kode {normalizedCode} tidak ditemukan untuk scope pemohon.");
            }

            var workflowSteps = await _dbContext.Set<MstWorkflowStep>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkflowDefinitionId == definition.Id &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel)
                .OrderBy(x => x.StepOrder)
                .ThenBy(x => x.StepCode)
                .ToListAsync(cancellationToken);

            if (workflowSteps.Count == 0)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow definition belum memiliki step aktif.");
            }

            var matrices = await _dbContext.Set<MstApprovalMatrix>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkflowDefinitionId == definition.Id &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel)
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.IsFallback)
                .ToListAsync(cancellationToken);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var now = DateTime.UtcNow;
                var instance = new TrxWorkflowInstance
                {
                    Id = Guid.NewGuid(),
                    WorkflowDefinitionId = definition.Id,
                    RequestedByWorkforceProfileId = actorContext.WorkforceProfileId,
                    RequestedByEmployeeId = actorContext.EmployeeId,
                    RequestedByUserId = actorContext.UserId,
                    OrganizationAssignmentId = actorContext.OrganizationAssignmentId,
                    LegalEntityId = actorContext.LegalEntityId,
                    HospitalSiteId = actorContext.HospitalSiteId,
                    OrganizationUnitId = actorContext.OrganizationUnitId,
                    DepartmentId = actorContext.DepartmentId,
                    CostCenterId = actorContext.CostCenterId,
                    ReferenceType = normalizedReferenceType,
                    ReferenceId = request.ReferenceId,
                    RequestNumber = BuildRequestNumber(),
                    WorkflowStatus = WorkflowValueConstants.WorkflowStatus.Draft,
                    SourceChannel = sourceChannel,
                    StartedAt = now,
                    RequestCorrelationId = NormalizeOptionalText(request.RequestCorrelationId),
                    ExternalReferenceNumber = NormalizeOptionalText(request.ExternalReferenceNumber),
                    IdempotencyKey = NormalizeOptionalText(request.IdempotencyKey),
                    WorkflowDefinitionSnapshotJson = BuildDefinitionSnapshot(definition, workflowSteps),
                    RequestContextJson = requestContextJson,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorContext.UserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<TrxWorkflowInstance>().Add(instance);

                var requestContext = WorkflowRequestContext.Parse(request.RequestContext);
                var stepInstances = new List<TrxWorkflowStepInstance>();

                foreach (var workflowStep in workflowSteps)
                {
                    var matrix = SelectApprovalMatrix(
                        matrices.Where(x => x.WorkflowStepId == workflowStep.Id),
                        actorContext,
                        requestContext,
                        now);

                    var stepInstance = CreateStepInstance(
                        instance,
                        workflowStep,
                        matrix,
                        actorContext.UserId,
                        now);

                    _dbContext.Set<TrxWorkflowStepInstance>().Add(stepInstance);
                    stepInstances.Add(stepInstance);

                    if (IsSystemOnlyStep(workflowStep.StepType))
                    {
                        continue;
                    }

                    var resolvedApprovers = await ResolveApproversAsync(
                        definition,
                        workflowStep,
                        matrix,
                        actorContext,
                        request.SelectedApproverUserIds,
                        cancellationToken);

                    if (resolvedApprovers.Count == 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);

                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            $"Approver untuk step {workflowStep.StepCode} tidak dapat ditentukan.");
                    }

                    if (!workflowStep.AllowSelfApproval)
                    {
                        resolvedApprovers = resolvedApprovers
                            .Where(x => x.UserId != actorContext.UserId)
                            .ToList();
                    }

                    if (resolvedApprovers.Count == 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);

                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            $"Step {workflowStep.StepCode} tidak memiliki approver valid setelah aturan self approval diterapkan.");
                    }

                    var assignments = await BuildAssignmentsAsync(
                        instance,
                        stepInstance,
                        definition,
                        workflowStep,
                        matrix,
                        resolvedApprovers,
                        actorContext.UserId,
                        now,
                        cancellationToken);

                    stepInstance.TotalAssignmentCount = assignments.Count;

                    if (string.Equals(
                            stepInstance.ApprovalModeSnapshot,
                            WorkflowValueConstants.ApprovalMode.Any,
                            StringComparison.OrdinalIgnoreCase) &&
                        stepInstance.RequiredApprovalCount > assignments.Count)
                    {
                        await transaction.RollbackAsync(cancellationToken);

                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            $"RequiredApprovalCount pada step {workflowStep.StepCode} melebihi jumlah approver yang ditemukan.");
                    }

                    if (string.Equals(
                            stepInstance.ApprovalModeSnapshot,
                            WorkflowValueConstants.ApprovalMode.Percentage,
                            StringComparison.OrdinalIgnoreCase) &&
                        (!stepInstance.RequiredApprovalPercentage.HasValue ||
                         stepInstance.RequiredApprovalPercentage.Value <= 0 ||
                         stepInstance.RequiredApprovalPercentage.Value > 100))
                    {
                        await transaction.RollbackAsync(cancellationToken);

                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            $"RequiredApprovalPercentage pada step {workflowStep.StepCode} harus lebih dari 0 dan maksimal 100.");
                    }

                    stepInstance.AssignmentResolutionJson = JsonSerializer.Serialize(
                        assignments.Select(x => new
                        {
                            x.AssignedApproverUserId,
                            x.AssignedApproverWorkforceProfileId,
                            x.OriginalApproverUserId,
                            x.OriginalApproverWorkforceProfileId,
                            x.ApprovalDelegationId,
                            x.ApproverSourceSnapshot,
                            x.AssignmentOrder
                        }));

                    _dbContext.Set<TrxWorkflowApproverAssignment>().AddRange(assignments);
                }

                AddStatusHistory(
                    instance,
                    null,
                    actorContext,
                    null,
                    WorkflowValueConstants.WorkflowStatus.Draft,
                    null,
                    null,
                    WorkflowValueConstants.ActionType.Start,
                    "Draft workflow instance dibuat.",
                    false,
                    now);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _workflowReferenceLifecycleService.HandleAsync(
                    instance.Id,
                    actorContext.UserId,
                    cancellationToken);

                var detail = await LoadDetailAsync(instance.Id, actorContext, cancellationToken);
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                    detail!,
                    "Draft workflow instance berhasil dibuat.",
                    StatusCodes.Status201Created);
            }
            catch (DbUpdateException)
            {
                await SafeRollbackAsync(transaction, cancellationToken);

                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Workflow instance gagal dibuat karena terjadi konflik data. Periksa kembali idempotency key atau transaksi sumber.");
            }
            catch (Exception ex)
            {
                await SafeRollbackAsync(transaction, cancellationToken);

                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Workflow instance gagal dibuat: {ex.Message}");
            }
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> SubmitAsync(
            Guid workflowInstanceId,
            WorkflowSubmitRequest? request,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id tidak valid.");
            }

            HumanResourceUserContextDto actorContext;
            try
            {
                actorContext = await _humanResourceContextService.GetCurrentAsync(cancellationToken);
            }
            catch (UnauthorizedAccessException ex)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    ex.Message);
            }

            if (!string.IsNullOrWhiteSpace(request?.IdempotencyKey))
            {
                var existingAction = await _dbContext.Set<TrxApprovalAction>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.WorkflowInstanceId == workflowInstanceId &&
                        x.IdempotencyKey == request.IdempotencyKey.Trim(),
                        cancellationToken);

                if (existingAction)
                {
                    var current = await LoadDetailAsync(
                        workflowInstanceId,
                        actorContext,
                        cancellationToken);

                    if (current != null)
                    {
                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                            current,
                            "Submit workflow sudah pernah diproses.");
                    }
                }
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var instance = await LoadTrackedInstanceAsync(
                    workflowInstanceId,
                    cancellationToken);

                if (instance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Workflow instance tidak ditemukan.");
                }

                if (instance.RequestedByUserId != actorContext.UserId)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status403Forbidden,
                        "Hanya pemohon yang dapat melakukan submit workflow.");
                }

                var isDraftSubmit = string.Equals(
                    instance.WorkflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Draft,
                    StringComparison.OrdinalIgnoreCase);

                var isRevisionResubmit = string.Equals(
                    instance.WorkflowStatus,
                    WorkflowValueConstants.WorkflowStatus.RevisionRequested,
                    StringComparison.OrdinalIgnoreCase);

                var isReturnedResubmit = string.Equals(
                    instance.WorkflowStatus,
                    WorkflowValueConstants.WorkflowStatus.Returned,
                    StringComparison.OrdinalIgnoreCase);

                if (!isDraftSubmit && !isRevisionResubmit && !isReturnedResubmit)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Workflow hanya dapat di-submit dari status Draft, RevisionRequested, atau Returned.");
                }

                var activeSteps = instance.StepInstances
                    .Where(x => x.IsActive && !x.IsDelete && !x.IsCancel)
                    .OrderBy(x => x.StepOrder)
                    .ThenBy(x => x.StepCodeSnapshot)
                    .ToList();

                if (activeSteps.Count == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Workflow instance belum memiliki step aktif.");
                }

                var now = DateTime.UtcNow;
                var previousStatus = instance.WorkflowStatus;

                instance.WorkflowStatus = WorkflowValueConstants.WorkflowStatus.InProgress;
                instance.SubmittedAt = now;
                instance.StartedAt = now;
                instance.LastActionAt = now;
                instance.UpdateDateTime = now;
                instance.UpdateBy = actorContext.UserId;

                if (isRevisionResubmit || isReturnedResubmit)
                {
                    var revisionStepOrder = PrepareRevisionResubmit(
                        instance,
                        actorContext.UserId,
                        now);

                    if (!revisionStepOrder.HasValue)
                    {
                        await transaction.RollbackAsync(cancellationToken);

                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                            StatusCodes.Status409Conflict,
                            "Step yang harus direvisi tidak ditemukan.");
                    }

                    ActivateStepGroup(instance, revisionStepOrder.Value, now);
                }
                else
                {
                    var firstOrder = activeSteps.Min(x => x.StepOrder);
                    ActivateStepGroup(instance, firstOrder, now);
                }

                _dbContext.Set<TrxApprovalAction>().Add(new TrxApprovalAction
                {
                    Id = Guid.NewGuid(),
                    WorkflowInstanceId = instance.Id,
                    ActualActionByUserId = actorContext.UserId,
                    ActualActionByWorkforceProfileId = actorContext.WorkforceProfileId,
                    ActionType = WorkflowValueConstants.ActionType.Submit,
                    ActionAt = now,
                    Comment = NormalizeOptionalText(request?.Comment),
                    IsDelegated = false,
                    IsSystemAction = false,
                    ActionSource = ResolveActionSource(instance.SourceChannel),
                    IdempotencyKey = NormalizeOptionalText(request?.IdempotencyKey),
                    IpAddress = GetIpAddress(),
                    UserAgent = GetUserAgent(),
                    PreviousWorkflowStatus = previousStatus,
                    ResultingWorkflowStatus = instance.WorkflowStatus,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorContext.UserId
                });

                if (!string.IsNullOrWhiteSpace(request?.Comment))
                {
                    AddComment(
                        instance,
                        null,
                        actorContext,
                        WorkflowValueConstants.CommentType.Requester,
                        request.Comment!,
                        now);
                }

                AddStatusHistory(
                    instance,
                    null,
                    actorContext,
                    previousStatus,
                    instance.WorkflowStatus,
                    null,
                    null,
                    WorkflowValueConstants.ActionType.Submit,
                    request?.Comment,
                    false,
                    now);

                await ProcessAutomaticStepsAsync(
                    instance,
                    actorContext,
                    now,
                    cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _workflowReferenceLifecycleService.HandleAsync(
                    instance.Id,
                    actorContext.UserId,
                    cancellationToken);

                var detail = await LoadDetailAsync(
                    instance.Id,
                    actorContext,
                    cancellationToken);

                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                    detail!,
                    "Workflow berhasil di-submit.");
            }
            catch (DbUpdateException)
            {
                await SafeRollbackAsync(transaction, cancellationToken);

                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Submit workflow gagal karena terjadi konflik data atau request yang sama sudah diproses.");
            }
            catch (Exception ex)
            {
                await SafeRollbackAsync(transaction, cancellationToken);

                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Submit workflow gagal: {ex.Message}");
            }
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> ApproveAsync(
            Guid workflowInstanceId,
            Guid assignmentId,
            WorkflowApproveRequest? request,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty || assignmentId == Guid.Empty)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id atau assignment id tidak valid.");
            }

            HumanResourceUserContextDto actorContext;
            try
            {
                actorContext = await _humanResourceContextService.GetCurrentAsync(cancellationToken);
            }
            catch (UnauthorizedAccessException ex)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    ex.Message);
            }

            if (!string.IsNullOrWhiteSpace(request?.IdempotencyKey))
            {
                var existingAction = await _dbContext.Set<TrxApprovalAction>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.WorkflowInstanceId == workflowInstanceId &&
                        x.IdempotencyKey == request.IdempotencyKey.Trim(),
                        cancellationToken);

                if (existingAction)
                {
                    var current = await LoadDetailAsync(
                        workflowInstanceId,
                        actorContext,
                        cancellationToken);

                    if (current != null)
                    {
                        return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                            current,
                            "Approval dengan idempotency key yang sama sudah diproses.");
                    }
                }
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var instance = await LoadTrackedInstanceAsync(
                    workflowInstanceId,
                    cancellationToken);

                if (instance == null)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Workflow instance tidak ditemukan.");
                }

                if (!string.Equals(
                    instance.WorkflowStatus,
                    WorkflowValueConstants.WorkflowStatus.InProgress,
                    StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Workflow tidak berada pada status yang dapat disetujui.");
                }

                var assignment = instance.StepInstances
                    .SelectMany(x => x.ApproverAssignments)
                    .FirstOrDefault(x =>
                        x.Id == assignmentId &&
                        x.WorkflowInstanceId == workflowInstanceId &&
                        x.IsActive &&
                        !x.IsDelete &&
                        !x.IsCancel);

                if (assignment == null)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Approver assignment tidak ditemukan.");
                }

                var step = instance.StepInstances.First(x =>
                    x.Id == assignment.WorkflowStepInstanceId);

                if (!step.IsCurrentStep ||
                    !string.Equals(
                        step.StepStatus,
                        WorkflowValueConstants.StepStatus.InProgress,
                        StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Step assignment belum aktif atau sudah selesai.");
                }

                if (!assignment.IsCurrentAssignment ||
                    !IsAvailableAssignmentStatus(assignment.AssignmentStatus))
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Assignment belum tersedia atau sudah diproses.");
                }

                var canAct = assignment.AssignedApproverUserId == actorContext.UserId;
                if (!canAct)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                        StatusCodes.Status403Forbidden,
                        "User login bukan approver yang ditugaskan.");
                }

                var now = DateTime.UtcNow;
                var previousWorkflowStatus = instance.WorkflowStatus;
                var previousStepStatus = step.StepStatus;

                assignment.AssignmentStatus = WorkflowValueConstants.AssignmentStatus.Approved;
                assignment.StartedAt ??= now;
                assignment.CompletedAt = now;
                assignment.IsCurrentAssignment = false;
                assignment.UpdateDateTime = now;
                assignment.UpdateBy = actorContext.UserId;

                step.ApprovedActionCount = step.ApproverAssignments.Count(x =>
                    string.Equals(
                        x.AssignmentStatus,
                        WorkflowValueConstants.AssignmentStatus.Approved,
                        StringComparison.OrdinalIgnoreCase));
                step.RejectedActionCount = step.ApproverAssignments.Count(x =>
                    string.Equals(
                        x.AssignmentStatus,
                        WorkflowValueConstants.AssignmentStatus.Rejected,
                        StringComparison.OrdinalIgnoreCase));
                step.UpdateDateTime = now;
                step.UpdateBy = actorContext.UserId;

                var stepCompleted = IsApprovalRequirementSatisfied(step);

                if (!stepCompleted &&
                    string.Equals(
                        step.ApprovalModeSnapshot,
                        WorkflowValueConstants.ApprovalMode.Sequential,
                        StringComparison.OrdinalIgnoreCase))
                {
                    ActivateNextSequentialAssignment(step, now);
                }

                if (stepCompleted)
                {
                    CompleteApprovedStep(step, actorContext.UserId, now);
                }

                instance.LastActionAt = now;
                instance.UpdateDateTime = now;
                instance.UpdateBy = actorContext.UserId;

                var action = new TrxApprovalAction
                {
                    Id = Guid.NewGuid(),
                    WorkflowInstanceId = instance.Id,
                    WorkflowStepInstanceId = step.Id,
                    WorkflowApproverAssignmentId = assignment.Id,
                    ApprovalDelegationId = assignment.ApprovalDelegationId,
                    AssignedApproverUserId = assignment.AssignedApproverUserId,
                    AssignedApproverWorkforceProfileId = assignment.AssignedApproverWorkforceProfileId,
                    ActualActionByUserId = actorContext.UserId,
                    ActualActionByWorkforceProfileId = actorContext.WorkforceProfileId,
                    DelegatedFromUserId = assignment.OriginalApproverUserId,
                    DelegatedFromWorkforceProfileId = assignment.OriginalApproverWorkforceProfileId,
                    ActionType = ResolveApprovalActionType(step.StepTypeSnapshot),
                    ActionAt = now,
                    Comment = NormalizeOptionalText(request?.Comment),
                    IsDelegated = assignment.IsDelegated,
                    IsSystemAction = false,
                    ActionSource = ResolveActionSource(instance.SourceChannel),
                    IdempotencyKey = NormalizeOptionalText(request?.IdempotencyKey),
                    IpAddress = GetIpAddress(),
                    UserAgent = GetUserAgent(),
                    PreviousWorkflowStatus = previousWorkflowStatus,
                    ResultingWorkflowStatus = instance.WorkflowStatus,
                    PreviousStepStatus = previousStepStatus,
                    ResultingStepStatus = step.StepStatus,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorContext.UserId
                };

                _dbContext.Set<TrxApprovalAction>().Add(action);

                if (!string.IsNullOrWhiteSpace(request?.Comment))
                {
                    AddComment(
                        instance,
                        step,
                        actorContext,
                        WorkflowValueConstants.CommentType.Approver,
                        request.Comment!,
                        now);
                }

                AddStatusHistory(
                    instance,
                    step,
                    actorContext,
                    previousWorkflowStatus,
                    instance.WorkflowStatus,
                    previousStepStatus,
                    step.StepStatus,
                    action.ActionType,
                    request?.Comment,
                    false,
                    now);

                if (stepCompleted)
                {
                    await AdvanceWorkflowAfterStepAsync(
                        instance,
                        step.StepOrder,
                        actorContext,
                        now,
                        cancellationToken);

                    action.ResultingWorkflowStatus = instance.WorkflowStatus;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _workflowReferenceLifecycleService.HandleAsync(
                    instance.Id,
                    actorContext.UserId,
                    cancellationToken);

                var detail = await LoadDetailAsync(
                    instance.Id,
                    actorContext,
                    cancellationToken);

                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Ok(
                    detail!,
                    "Approval workflow berhasil diproses.");
            }
            catch (DbUpdateException)
            {
                await SafeRollbackAsync(transaction, cancellationToken);

                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Approval workflow gagal karena terjadi konflik data atau request yang sama sudah diproses.");
            }
            catch (Exception ex)
            {
                await SafeRollbackAsync(transaction, cancellationToken);

                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Approval workflow gagal: {ex.Message}");
            }
        }

        private IQueryable<TrxWorkflowInstance> BuildListQuery()
        {
            return _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel);
        }

        private static IQueryable<TrxWorkflowInstance> ApplyListFilters(
            IQueryable<TrxWorkflowInstance> query,
            DateRangeResult dateRange,
            Guid? workflowDefinitionId,
            string? workflowCode,
            string? referenceType,
            Guid? requestedByWorkforceProfileId,
            string? workflowStatus,
            string? currentStepCode,
            string? search)
        {
            if (dateRange.Start.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            }

            if (dateRange.EndExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            }

            if (workflowDefinitionId.HasValue && workflowDefinitionId.Value != Guid.Empty)
            {
                query = query.Where(x => x.WorkflowDefinitionId == workflowDefinitionId.Value);
            }

            if (!string.IsNullOrWhiteSpace(workflowCode))
            {
                var normalized = workflowCode.Trim().ToLower();
                query = query.Where(x =>
                    x.WorkflowDefinition != null &&
                    x.WorkflowDefinition.WorkflowCode.ToLower() == normalized);
            }

            if (!string.IsNullOrWhiteSpace(referenceType))
            {
                var normalized = referenceType.Trim().ToLower();
                query = query.Where(x => x.ReferenceType.ToLower() == normalized);
            }

            if (requestedByWorkforceProfileId.HasValue &&
                requestedByWorkforceProfileId.Value != Guid.Empty)
            {
                query = query.Where(x =>
                    x.RequestedByWorkforceProfileId == requestedByWorkforceProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(workflowStatus))
            {
                var normalized = workflowStatus.Trim().ToLower();
                query = query.Where(x => x.WorkflowStatus.ToLower() == normalized);
            }

            if (!string.IsNullOrWhiteSpace(currentStepCode))
            {
                var normalized = currentStepCode.Trim().ToLower();
                query = query.Where(x =>
                    x.CurrentStepCode != null &&
                    x.CurrentStepCode.ToLower() == normalized);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.RequestNumber.ToLower().Contains(keyword) ||
                    x.ReferenceType.ToLower().Contains(keyword) ||
                    (x.ExternalReferenceNumber != null &&
                     x.ExternalReferenceNumber.ToLower().Contains(keyword)) ||
                    (x.CurrentStepCode != null &&
                     x.CurrentStepCode.ToLower().Contains(keyword)) ||
                    (x.WorkflowDefinition != null &&
                     x.WorkflowDefinition.WorkflowCode.ToLower().Contains(keyword)) ||
                    (x.WorkflowDefinition != null &&
                     x.WorkflowDefinition.WorkflowName.ToLower().Contains(keyword)) ||
                    (x.RequestedByWorkforceProfile != null &&
                     x.RequestedByWorkforceProfile.ProfileCode.ToLower().Contains(keyword)) ||
                    (x.RequestedByWorkforceProfile != null &&
                     x.RequestedByWorkforceProfile.DisplayName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<TrxWorkflowInstance> ApplyOrdering(
            IQueryable<TrxWorkflowInstance> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = !string.Equals(
                sortDirection,
                "asc",
                StringComparison.OrdinalIgnoreCase);

            var normalizedSort = string.IsNullOrWhiteSpace(sortBy)
                ? "createdatetime"
                : sortBy.Trim().ToLowerInvariant();

            return normalizedSort switch
            {
                "requestnumber" => descending
                    ? query.OrderByDescending(x => x.RequestNumber)
                    : query.OrderBy(x => x.RequestNumber),
                "workflowname" => descending
                    ? query.OrderByDescending(x => x.WorkflowDefinition!.WorkflowName)
                    : query.OrderBy(x => x.WorkflowDefinition!.WorkflowName),
                "workflowstatus" => descending
                    ? query.OrderByDescending(x => x.WorkflowStatus)
                    : query.OrderBy(x => x.WorkflowStatus),
                "submittedat" => descending
                    ? query.OrderByDescending(x => x.SubmittedAt)
                    : query.OrderBy(x => x.SubmittedAt),
                "lastactionat" => descending
                    ? query.OrderByDescending(x => x.LastActionAt)
                    : query.OrderBy(x => x.LastActionAt),
                _ => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private async Task<WorkflowInstanceDetailResponse?> LoadDetailAsync(
            Guid id,
            HumanResourceUserContextDto? actorContext,
            CancellationToken cancellationToken)
        {
            var instance = await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.WorkflowDefinition)
                .Include(x => x.RequestedByUser)
                .Include(x => x.RequestedByWorkforceProfile)
                .Include(x => x.StepInstances.Where(step => !step.IsDelete))
                    .ThenInclude(step => step.ApproverAssignments.Where(assignment => !assignment.IsDelete))
                        .ThenInclude(assignment => assignment.AssignedApproverUser)
                .Include(x => x.StepInstances.Where(step => !step.IsDelete))
                    .ThenInclude(step => step.ApproverAssignments.Where(assignment => !assignment.IsDelete))
                        .ThenInclude(assignment => assignment.AssignedApproverWorkforceProfile)
                .Include(x => x.StepInstances.Where(step => !step.IsDelete))
                    .ThenInclude(step => step.ApproverAssignments.Where(assignment => !assignment.IsDelete))
                        .ThenInclude(assignment => assignment.OriginalApproverUser)
                .Include(x => x.ApprovalActions.Where(action => !action.IsDelete))
                    .ThenInclude(action => action.ActualActionByUser)
                .Include(x => x.ApprovalActions.Where(action => !action.IsDelete))
                    .ThenInclude(action => action.ActualActionByWorkforceProfile)
                .Include(x => x.StatusHistories.Where(history => !history.IsDelete))
                    .ThenInclude(history => history.ChangedByUser)
                .Include(x => x.StatusHistories.Where(history => !history.IsDelete))
                    .ThenInclude(history => history.ChangedByWorkforceProfile)
                .Include(x => x.Comments.Where(comment => !comment.IsDelete))
                    .ThenInclude(comment => comment.CommentByUser)
                .Include(x => x.Comments.Where(comment => !comment.IsDelete))
                    .ThenInclude(comment => comment.CommentByWorkforceProfile)
                .Include(x => x.Attachments.Where(attachment => !attachment.IsDelete))
                    .ThenInclude(attachment => attachment.UploadedByUser)
                .Include(x => x.Attachments.Where(attachment => !attachment.IsDelete))
                    .ThenInclude(attachment => attachment.UploadedByWorkforceProfile)
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDelete,
                    cancellationToken);

            if (instance == null)
            {
                return null;
            }

            var isRequester = actorContext != null &&
                              actorContext.UserId == instance.RequestedByUserId;

            var detail = new WorkflowInstanceDetailResponse
            {
                Id = instance.Id,
                RequestNumber = instance.RequestNumber,
                WorkflowDefinitionId = instance.WorkflowDefinitionId,
                WorkflowCode = instance.WorkflowDefinition?.WorkflowCode ?? string.Empty,
                WorkflowName = instance.WorkflowDefinition?.WorkflowName ?? string.Empty,
                WorkflowVersion = instance.WorkflowDefinition?.Version ?? 0,
                ReferenceType = instance.ReferenceType,
                ReferenceId = instance.ReferenceId,
                ExternalReferenceNumber = instance.ExternalReferenceNumber,
                WorkflowStatus = instance.WorkflowStatus,
                CurrentStepOrder = instance.CurrentStepOrder,
                CurrentStepCode = instance.CurrentStepCode,
                SourceChannel = instance.SourceChannel,
                Requester = new WorkflowRequesterResponse
                {
                    UserId = instance.RequestedByUserId,
                    WorkforceProfileId = instance.RequestedByWorkforceProfileId,
                    EmployeeId = instance.RequestedByEmployeeId,
                    ProfileCode = instance.RequestedByWorkforceProfile?.ProfileCode,
                    DisplayName = instance.RequestedByWorkforceProfile?.DisplayName ??
                                  GetUserDisplayName(instance.RequestedByUser)
                },
                Organization = new WorkflowOrganizationSnapshotResponse
                {
                    OrganizationAssignmentId = instance.OrganizationAssignmentId,
                    LegalEntityId = instance.LegalEntityId,
                    HospitalSiteId = instance.HospitalSiteId,
                    OrganizationUnitId = instance.OrganizationUnitId,
                    DepartmentId = instance.DepartmentId,
                    CostCenterId = instance.CostCenterId
                },
                StartedAt = instance.StartedAt,
                SubmittedAt = instance.SubmittedAt,
                DueAt = instance.DueAt,
                LastActionAt = instance.LastActionAt,
                CompletedAt = instance.CompletedAt,
                CancelledAt = instance.CancelledAt,
                WithdrawnAt = instance.WithdrawnAt,
                RequestCorrelationId = instance.RequestCorrelationId,
                IdempotencyKey = instance.IdempotencyKey,
                CompletionNote = instance.CompletionNote,
                CancellationReason = instance.CancellationReason,
                WorkflowDefinitionSnapshotJson = instance.WorkflowDefinitionSnapshotJson,
                RequestContextJson = instance.RequestContextJson,
                CreateDateTime = instance.CreateDateTime,
                UpdateDateTime = instance.UpdateDateTime,
                AvailableActions = BuildInstanceAvailableActions(instance, actorContext)
            };

            detail.Steps = instance.StepInstances
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.StepOrder)
                .ThenBy(x => x.StepCodeSnapshot)
                .Select(step => new WorkflowStepInstanceResponse
                {
                    Id = step.Id,
                    WorkflowStepId = step.WorkflowStepId,
                    ApprovalMatrixId = step.ApprovalMatrixId,
                    StepOrder = step.StepOrder,
                    StepCode = step.StepCodeSnapshot,
                    StepName = step.StepNameSnapshot,
                    StepType = step.StepTypeSnapshot,
                    ApprovalMode = step.ApprovalModeSnapshot,
                    ApproverSource = step.ApproverSourceSnapshot,
                    RequiredApprovalCount = step.RequiredApprovalCount,
                    RequiredApprovalPercentage = step.RequiredApprovalPercentage,
                    TotalAssignmentCount = step.TotalAssignmentCount,
                    ApprovedActionCount = step.ApprovedActionCount,
                    RejectedActionCount = step.RejectedActionCount,
                    StepStatus = step.StepStatus,
                    AvailableAt = step.AvailableAt,
                    StartedAt = step.StartedAt,
                    DueAt = step.DueAt,
                    CompletedAt = step.CompletedAt,
                    SkippedAt = step.SkippedAt,
                    IsCurrentStep = step.IsCurrentStep,
                    IsDelegationAllowed = step.IsDelegationAllowed,
                    IsAutoAction = step.IsAutoAction,
                    Instructions = step.InstructionsSnapshot,
                    Assignments = step.ApproverAssignments
                        .Where(x => !x.IsDelete)
                        .OrderBy(x => x.AssignmentOrder)
                        .Select(assignment => new WorkflowApproverAssignmentResponse
                        {
                            Id = assignment.Id,
                            WorkflowStepInstanceId = assignment.WorkflowStepInstanceId,
                            ApprovalMatrixId = assignment.ApprovalMatrixId,
                            ApprovalDelegationId = assignment.ApprovalDelegationId,
                            AssignedApproverUserId = assignment.AssignedApproverUserId,
                            AssignedApproverWorkforceProfileId = assignment.AssignedApproverWorkforceProfileId,
                            AssignedApproverProfileCode = assignment.AssignedApproverWorkforceProfile?.ProfileCode,
                            AssignedApproverName = assignment.AssignedApproverWorkforceProfile?.DisplayName ??
                                                   GetUserDisplayName(assignment.AssignedApproverUser),
                            OriginalApproverUserId = assignment.OriginalApproverUserId,
                            OriginalApproverWorkforceProfileId = assignment.OriginalApproverWorkforceProfileId,
                            OriginalApproverName = assignment.OriginalApproverUser == null
                                ? null
                                : GetUserDisplayName(assignment.OriginalApproverUser),
                            AssignedApproverRoleCode = assignment.AssignedApproverRoleCode,
                            ApproverSource = assignment.ApproverSourceSnapshot,
                            AssignmentOrder = assignment.AssignmentOrder,
                            AssignmentStatus = assignment.AssignmentStatus,
                            AssignedAt = assignment.AssignedAt,
                            AvailableAt = assignment.AvailableAt,
                            StartedAt = assignment.StartedAt,
                            DueAt = assignment.DueAt,
                            CompletedAt = assignment.CompletedAt,
                            IsRequired = assignment.IsRequired,
                            IsCurrentAssignment = assignment.IsCurrentAssignment,
                            IsDelegated = assignment.IsDelegated,
                            AvailableActions = BuildAssignmentAvailableActions(
                                instance,
                                step,
                                assignment,
                                actorContext)
                        })
                        .ToList()
                })
                .ToList();

            detail.ApprovalActions = instance.ApprovalActions
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.ActionAt)
                .Select(action => new WorkflowApprovalActionResponse
                {
                    Id = action.Id,
                    WorkflowStepInstanceId = action.WorkflowStepInstanceId,
                    WorkflowApproverAssignmentId = action.WorkflowApproverAssignmentId,
                    ApprovalDelegationId = action.ApprovalDelegationId,
                    ActionType = action.ActionType,
                    ActionAt = action.ActionAt,
                    ActualActionByUserId = action.ActualActionByUserId,
                    ActualActionByWorkforceProfileId = action.ActualActionByWorkforceProfileId,
                    ActualActionByName = action.ActualActionByWorkforceProfile?.DisplayName ??
                                         GetUserDisplayName(action.ActualActionByUser),
                    IsDelegated = action.IsDelegated,
                    IsSystemAction = action.IsSystemAction,
                    ActionSource = action.ActionSource,
                    Comment = action.Comment,
                    ActionReasonId = action.ActionReasonId,
                    ActionReasonType = action.ActionReasonType,
                    ActionReasonCode = action.ActionReasonCodeSnapshot,
                    ActionReasonName = action.ActionReasonNameSnapshot,
                    PreviousWorkflowStatus = action.PreviousWorkflowStatus,
                    ResultingWorkflowStatus = action.ResultingWorkflowStatus,
                    PreviousStepStatus = action.PreviousStepStatus,
                    ResultingStepStatus = action.ResultingStepStatus
                })
                .ToList();

            detail.StatusHistories = instance.StatusHistories
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.SequenceNumber)
                .Select(history => new WorkflowStatusHistoryResponse
                {
                    Id = history.Id,
                    WorkflowStepInstanceId = history.WorkflowStepInstanceId,
                    SequenceNumber = history.SequenceNumber,
                    FromWorkflowStatus = history.FromWorkflowStatus,
                    ToWorkflowStatus = history.ToWorkflowStatus,
                    FromStepStatus = history.FromStepStatus,
                    ToStepStatus = history.ToStepStatus,
                    ActionType = history.ActionType,
                    ChangedAt = history.ChangedAt,
                    ChangedByUserId = history.ChangedByUserId,
                    ChangedByWorkforceProfileId = history.ChangedByWorkforceProfileId,
                    ChangedByName = history.ChangedByWorkforceProfile?.DisplayName ??
                                    GetUserDisplayName(history.ChangedByUser),
                    Comment = history.Comment,
                    IsSystemGenerated = history.IsSystemGenerated
                })
                .ToList();

            detail.Comments = instance.Comments
                .Where(x =>
                    !x.IsDelete &&
                    (!isRequester ||
                     x.IsRequesterVisible && !x.IsInternalComment))
                .OrderBy(x => x.CommentedAt)
                .Select(comment => new WorkflowCommentResponse
                {
                    Id = comment.Id,
                    WorkflowStepInstanceId = comment.WorkflowStepInstanceId,
                    ParentCommentId = comment.ParentCommentId,
                    CommentType = comment.CommentType,
                    CommentText = comment.CommentText,
                    CommentedAt = comment.CommentedAt,
                    CommentByUserId = comment.CommentByUserId,
                    CommentByWorkforceProfileId = comment.CommentByWorkforceProfileId,
                    CommentByName = comment.CommentByWorkforceProfile?.DisplayName ??
                                    GetUserDisplayName(comment.CommentByUser),
                    IsRequesterVisible = comment.IsRequesterVisible,
                    IsInternalComment = comment.IsInternalComment,
                    IsSystemGenerated = comment.IsSystemGenerated
                })
                .ToList();

            detail.Attachments = instance.Attachments
                .Where(x =>
                    !x.IsDelete &&
                    (!isRequester ||
                     x.IsRequesterVisible && !x.IsConfidential))
                .OrderBy(x => x.UploadedAt)
                .Select(attachment => new WorkflowAttachmentResponse
                {
                    Id = attachment.Id,
                    WorkflowStepInstanceId = attachment.WorkflowStepInstanceId,
                    ApprovalActionId = attachment.ApprovalActionId,
                    WorkflowCommentId = attachment.WorkflowCommentId,
                    FileName = attachment.FileName,
                    DownloadUrl = $"/api/v1/corporate/human-resource/workflow-instances/" +
                                  $"{instance.Id}/attachments/{attachment.Id}/download",
                    ContentType = attachment.ContentType,
                    FileSizeBytes = attachment.FileSizeBytes,
                    AttachmentCategory = attachment.AttachmentCategory,
                    Description = attachment.Description,
                    UploadedAt = attachment.UploadedAt,
                    UploadedByName = attachment.UploadedByWorkforceProfile?.DisplayName ??
                                     GetUserDisplayName(attachment.UploadedByUser),
                    IsRequesterVisible = attachment.IsRequesterVisible,
                    IsConfidential = attachment.IsConfidential
                })
                .ToList();

            return detail;
        }

        private async Task<TrxWorkflowInstance?> LoadTrackedInstanceAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<TrxWorkflowInstance>()
                .AsSplitQuery()
                .Include(x => x.WorkflowDefinition)
                .Include(x => x.StepInstances.Where(step => !step.IsDelete))
                    .ThenInclude(step => step.ApproverAssignments.Where(assignment => !assignment.IsDelete))
                .Include(x => x.StatusHistories.Where(history => !history.IsDelete))
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDelete && !x.IsCancel,
                    cancellationToken);
        }

        private async Task<MstWorkflowDefinition?> ResolveWorkflowDefinitionAsync(
            string workflowCode,
            HumanResourceUserContextDto context,
            CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.Date;

            var candidates = await _dbContext.Set<MstWorkflowDefinition>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.WorkflowStatus == "Active" &&
                    x.WorkflowCode.ToUpper() == workflowCode &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today) &&
                    (!x.LegalEntityId.HasValue || x.LegalEntityId == context.LegalEntityId) &&
                    (!x.HospitalSiteId.HasValue || x.HospitalSiteId == context.HospitalSiteId) &&
                    (!x.OrganizationUnitId.HasValue || x.OrganizationUnitId == context.OrganizationUnitId))
                .ToListAsync(cancellationToken);

            return candidates
                .OrderByDescending(x => GetDefinitionScopeRank(x, context))
                .ThenByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.Version)
                .ThenByDescending(x => x.EffectiveStartDate)
                .FirstOrDefault();
        }

        private static int GetDefinitionScopeRank(
            MstWorkflowDefinition definition,
            HumanResourceUserContextDto context)
        {
            if (definition.OrganizationUnitId.HasValue &&
                definition.OrganizationUnitId == context.OrganizationUnitId)
            {
                return 4;
            }

            if (definition.HospitalSiteId.HasValue &&
                definition.HospitalSiteId == context.HospitalSiteId)
            {
                return 3;
            }

            if (definition.LegalEntityId.HasValue &&
                definition.LegalEntityId == context.LegalEntityId)
            {
                return 2;
            }

            return 1;
        }

        private static MstApprovalMatrix? SelectApprovalMatrix(
            IEnumerable<MstApprovalMatrix> matrices,
            HumanResourceUserContextDto context,
            WorkflowRequestContext requestContext,
            DateTime now)
        {
            var applicable = matrices
                .Where(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= now.Date) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= now.Date) &&
                    (!x.LegalEntityId.HasValue || x.LegalEntityId == context.LegalEntityId) &&
                    (!x.HospitalSiteId.HasValue || x.HospitalSiteId == context.HospitalSiteId) &&
                    (!x.OrganizationUnitId.HasValue || x.OrganizationUnitId == context.OrganizationUnitId) &&
                    (!x.DepartmentId.HasValue || x.DepartmentId == context.DepartmentId) &&
                    (!x.RequesterPositionId.HasValue || x.RequesterPositionId == context.PositionId) &&
                    (!x.EmployeeCategoryId.HasValue || x.EmployeeCategoryId == requestContext.EmployeeCategoryId) &&
                    (!x.EmploymentTypeId.HasValue || x.EmploymentTypeId == requestContext.EmploymentTypeId) &&
                    (!x.MinimumAmount.HasValue ||
                     (requestContext.Amount.HasValue && requestContext.Amount.Value >= x.MinimumAmount.Value)) &&
                    (!x.MaximumAmount.HasValue ||
                     (requestContext.Amount.HasValue && requestContext.Amount.Value <= x.MaximumAmount.Value)) &&
                    (!x.MinimumDurationHours.HasValue ||
                     (requestContext.DurationHours.HasValue &&
                      requestContext.DurationHours.Value >= x.MinimumDurationHours.Value)) &&
                    (!x.MaximumDurationHours.HasValue ||
                     (requestContext.DurationHours.HasValue &&
                      requestContext.DurationHours.Value <= x.MaximumDurationHours.Value)) &&
                    (!x.MinimumDurationDays.HasValue ||
                     (requestContext.DurationDays.HasValue &&
                      requestContext.DurationDays.Value >= x.MinimumDurationDays.Value)) &&
                    (!x.MaximumDurationDays.HasValue ||
                     (requestContext.DurationDays.HasValue &&
                      requestContext.DurationDays.Value <= x.MaximumDurationDays.Value)))
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.IsFallback)
                .ToList();

            return applicable.FirstOrDefault(x => !x.IsFallback) ??
                   applicable.FirstOrDefault(x => x.IsFallback);
        }

        private TrxWorkflowStepInstance CreateStepInstance(
            TrxWorkflowInstance instance,
            MstWorkflowStep workflowStep,
            MstApprovalMatrix? matrix,
            Guid actorUserId,
            DateTime now)
        {
            var source = matrix?.ApproverSourceType ?? workflowStep.ApproverSourceType;
            var dueAfterHours = workflowStep.EscalationAfterHours ??
                                workflowStep.ReminderAfterHours;

            return new TrxWorkflowStepInstance
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                WorkflowStepId = workflowStep.Id,
                ApprovalMatrixId = matrix?.Id,
                StepOrder = workflowStep.StepOrder,
                StepCodeSnapshot = workflowStep.StepCode,
                StepNameSnapshot = workflowStep.StepName,
                StepTypeSnapshot = workflowStep.StepType,
                ApprovalModeSnapshot = workflowStep.ApprovalMode,
                ApproverSourceSnapshot = NormalizeApproverSource(source),
                RequiredApprovalCount = Math.Max(1, workflowStep.RequiredApprovalCount),
                RequiredApprovalPercentage = workflowStep.RequiredApprovalPercentage,
                TotalAssignmentCount = 0,
                ApprovedActionCount = 0,
                RejectedActionCount = 0,
                StepStatus = WorkflowValueConstants.StepStatus.Pending,
                DueAt = dueAfterHours.HasValue && dueAfterHours.Value > 0
                    ? now.AddHours(dueAfterHours.Value)
                    : null,
                IsCurrentStep = false,
                IsDelegationAllowed = workflowStep.AllowDelegation,
                IsAutoAction = IsSystemOnlyStep(workflowStep.StepType),
                InstructionsSnapshot = workflowStep.Instructions,
                StepConditionSnapshotJson = null,
                IsActive = workflowStep.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };
        }

        private async Task<List<ResolvedApprover>> ResolveApproversAsync(
            MstWorkflowDefinition definition,
            MstWorkflowStep step,
            MstApprovalMatrix? matrix,
            HumanResourceUserContextDto requesterContext,
            IReadOnlyCollection<Guid> selectedApproverUserIds,
            CancellationToken cancellationToken)
        {
            var source = NormalizeApproverSource(
                matrix?.ApproverSourceType ?? step.ApproverSourceType);

            var specificUserId = matrix?.SpecificApproverUserId ?? step.SpecificApproverUserId;
            var positionId = matrix?.ApproverPositionId ?? step.ApproverPositionId;
            var organizationUnitId = matrix?.ApproverOrganizationUnitId ??
                                     step.ApproverOrganizationUnitId;
            var roleCode = matrix?.ApproverRoleCode ?? step.ApproverRoleCode;
            var managerLevel = matrix?.ManagerLevel ?? step.ManagerLevel ?? 1;

            List<ResolvedApprover> resolved;

            switch (source)
            {
                case WorkflowValueConstants.ApproverSource.RequesterManager:
                    resolved = await ResolveManagerApproverAsync(
                        requesterContext,
                        1,
                        source,
                        cancellationToken);
                    break;

                case WorkflowValueConstants.ApproverSource.ManagerLevel:
                    resolved = await ResolveManagerApproverAsync(
                        requesterContext,
                        Math.Max(1, managerLevel),
                        source,
                        cancellationToken);
                    break;

                case WorkflowValueConstants.ApproverSource.SpecificUser:
                    resolved = await ResolveSpecificUsersAsync(
                        specificUserId.HasValue
                            ? new[] { specificUserId.Value }
                            : Array.Empty<Guid>(),
                        source,
                        roleCode,
                        cancellationToken);
                    break;

                case WorkflowValueConstants.ApproverSource.Position:
                    resolved = await ResolveByOrganizationAssignmentAsync(
                        positionId,
                        null,
                        source,
                        roleCode,
                        cancellationToken);
                    break;

                case WorkflowValueConstants.ApproverSource.OrganizationUnit:
                    resolved = await ResolveByOrganizationAssignmentAsync(
                        null,
                        organizationUnitId,
                        source,
                        roleCode,
                        cancellationToken);
                    break;

                case WorkflowValueConstants.ApproverSource.Role:
                case WorkflowValueConstants.ApproverSource.SpecificRole:
                case WorkflowValueConstants.ApproverSource.OrganizationHead:
                case WorkflowValueConstants.ApproverSource.DepartmentHead:
                case WorkflowValueConstants.ApproverSource.SiteHr:
                case WorkflowValueConstants.ApproverSource.CorporateHr:
                case WorkflowValueConstants.ApproverSource.PayrollOfficer:
                case WorkflowValueConstants.ApproverSource.FinanceOfficer:
                case WorkflowValueConstants.ApproverSource.CostCenterOwner:
                case WorkflowValueConstants.ApproverSource.CredentialingCommittee:
                    resolved = await ResolveByRoleAsync(
                        string.IsNullOrWhiteSpace(roleCode) ? source : roleCode,
                        source,
                        requesterContext,
                        cancellationToken);
                    break;

                case WorkflowValueConstants.ApproverSource.RequesterSelected:
                    resolved = await ResolveSpecificUsersAsync(
                        selectedApproverUserIds,
                        source,
                        roleCode,
                        cancellationToken);
                    break;

                case WorkflowValueConstants.ApproverSource.ApprovalMatrix:
                    if (matrix == null)
                    {
                        resolved = new List<ResolvedApprover>();
                        break;
                    }

                    resolved = await ResolveApproversByMatrixAsync(
                        matrix,
                        requesterContext,
                        selectedApproverUserIds,
                        cancellationToken);
                    break;

                default:
                    resolved = new List<ResolvedApprover>();
                    break;
            }

            return resolved
                .GroupBy(x => x.UserId)
                .Select(group => group.First())
                .OrderBy(x => x.DisplayName)
                .ToList();
        }

        private async Task<List<ResolvedApprover>> ResolveApproversByMatrixAsync(
            MstApprovalMatrix matrix,
            HumanResourceUserContextDto requesterContext,
            IReadOnlyCollection<Guid> selectedApproverUserIds,
            CancellationToken cancellationToken)
        {
            var matrixSource = NormalizeApproverSource(matrix.ApproverSourceType);

            return matrixSource switch
            {
                WorkflowValueConstants.ApproverSource.RequesterManager =>
                    await ResolveManagerApproverAsync(
                        requesterContext,
                        1,
                        matrixSource,
                        cancellationToken),
                WorkflowValueConstants.ApproverSource.ManagerLevel =>
                    await ResolveManagerApproverAsync(
                        requesterContext,
                        Math.Max(1, matrix.ManagerLevel ?? 1),
                        matrixSource,
                        cancellationToken),
                WorkflowValueConstants.ApproverSource.SpecificUser =>
                    await ResolveSpecificUsersAsync(
                        matrix.SpecificApproverUserId.HasValue
                            ? new[] { matrix.SpecificApproverUserId.Value }
                            : Array.Empty<Guid>(),
                        matrixSource,
                        matrix.ApproverRoleCode,
                        cancellationToken),
                WorkflowValueConstants.ApproverSource.Position =>
                    await ResolveByOrganizationAssignmentAsync(
                        matrix.ApproverPositionId,
                        null,
                        matrixSource,
                        matrix.ApproverRoleCode,
                        cancellationToken),
                WorkflowValueConstants.ApproverSource.OrganizationUnit =>
                    await ResolveByOrganizationAssignmentAsync(
                        null,
                        matrix.ApproverOrganizationUnitId,
                        matrixSource,
                        matrix.ApproverRoleCode,
                        cancellationToken),
                WorkflowValueConstants.ApproverSource.Role or
                WorkflowValueConstants.ApproverSource.SpecificRole =>
                    await ResolveByRoleAsync(
                        matrix.ApproverRoleCode ?? string.Empty,
                        matrixSource,
                        requesterContext,
                        cancellationToken),
                WorkflowValueConstants.ApproverSource.RequesterSelected =>
                    await ResolveSpecificUsersAsync(
                        selectedApproverUserIds,
                        matrixSource,
                        matrix.ApproverRoleCode,
                        cancellationToken),
                _ => new List<ResolvedApprover>()
            };
        }

        private async Task<List<ResolvedApprover>> ResolveManagerApproverAsync(
            HumanResourceUserContextDto requesterContext,
            int managerLevel,
            string source,
            CancellationToken cancellationToken)
        {
            if (!requesterContext.WorkforceProfileId.HasValue)
            {
                return new List<ResolvedApprover>();
            }

            var currentWorkforceProfileId = requesterContext.WorkforceProfileId.Value;
            Guid? managerWorkforceProfileId = null;
            var now = DateTime.UtcNow;

            for (var level = 1; level <= managerLevel; level++)
            {
                var managerAssignment = await _dbContext.Set<
                        QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models.WfpManagerAssignment>()
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == currentWorkforceProfileId &&
                        x.IsActive &&
                        !x.IsDelete &&
                        !x.IsCancel &&
                        x.CanApproveRequests &&
                        x.EffectiveStartDate <= now &&
                        (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now))
                    .OrderByDescending(x => x.IsPrimaryManager)
                    .ThenByDescending(x => x.EffectiveStartDate)
                    .FirstOrDefaultAsync(cancellationToken);

                if (managerAssignment == null)
                {
                    return new List<ResolvedApprover>();
                }

                managerWorkforceProfileId = managerAssignment.ManagerWorkforceProfileId;
                currentWorkforceProfileId = managerAssignment.ManagerWorkforceProfileId;
            }

            if (!managerWorkforceProfileId.HasValue)
            {
                return new List<ResolvedApprover>();
            }

            return await ResolveUsersByWorkforceProfileIdsAsync(
                new[] { managerWorkforceProfileId.Value },
                source,
                null,
                cancellationToken);
        }

        private async Task<List<ResolvedApprover>> ResolveSpecificUsersAsync(
            IEnumerable<Guid> userIds,
            string source,
            string? roleCode,
            CancellationToken cancellationToken)
        {
            var ids = userIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                return new List<ResolvedApprover>();
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id) && x.IsActive)
                .Select(x => new ResolvedApprover
                {
                    UserId = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    DisplayName = x.DisplayName ??
                                  x.UserName ??
                                  x.Email ??
                                  x.UserCode,
                    RoleCode = roleCode,
                    Source = source
                })
                .ToListAsync(cancellationToken);
        }

        private async Task<List<ResolvedApprover>> ResolveUsersByWorkforceProfileIdsAsync(
            IEnumerable<Guid> workforceProfileIds,
            string source,
            string? roleCode,
            CancellationToken cancellationToken)
        {
            var ids = workforceProfileIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                return new List<ResolvedApprover>();
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.WorkforceProfileId.HasValue &&
                    ids.Contains(x.WorkforceProfileId.Value))
                .Select(x => new ResolvedApprover
                {
                    UserId = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    DisplayName = x.DisplayName ??
                                  x.UserName ??
                                  x.Email ??
                                  x.UserCode,
                    RoleCode = roleCode,
                    Source = source
                })
                .ToListAsync(cancellationToken);
        }

        private async Task<List<ResolvedApprover>> ResolveByOrganizationAssignmentAsync(
            Guid? positionId,
            Guid? organizationUnitId,
            string source,
            string? roleCode,
            CancellationToken cancellationToken)
        {
            if ((!positionId.HasValue || positionId.Value == Guid.Empty) &&
                (!organizationUnitId.HasValue || organizationUnitId.Value == Guid.Empty))
            {
                return new List<ResolvedApprover>();
            }

            var now = DateTime.UtcNow;
            var query = _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.EffectiveStartDate <= now &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now));

            if (positionId.HasValue && positionId.Value != Guid.Empty)
            {
                query = query.Where(x => x.PositionId == positionId.Value);
            }

            if (organizationUnitId.HasValue && organizationUnitId.Value != Guid.Empty)
            {
                query = query.Where(x => x.OrganizationUnitId == organizationUnitId.Value);
            }

            var workforceProfileIds = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .Select(x => x.WorkforceProfileId)
                .Distinct()
                .ToListAsync(cancellationToken);

            return await ResolveUsersByWorkforceProfileIdsAsync(
                workforceProfileIds,
                source,
                roleCode,
                cancellationToken);
        }

        private async Task<List<ResolvedApprover>> ResolveByRoleAsync(
            string roleCode,
            string source,
            HumanResourceUserContextDto requesterContext,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(roleCode))
            {
                return new List<ResolvedApprover>();
            }

            IList<ApplicationUser> users;
            try
            {
                users = await _userManager.GetUsersInRoleAsync(roleCode.Trim());
            }
            catch (InvalidOperationException)
            {
                return new List<ResolvedApprover>();
            }

            var activeUsers = users
                .Where(x => x.IsActive)
                .ToList();

            var workforceProfileIds = activeUsers
                .Where(x => x.WorkforceProfileId.HasValue)
                .Select(x => x.WorkforceProfileId!.Value)
                .Distinct()
                .ToList();

            if (workforceProfileIds.Count > 0 && IsScopedRoleSource(source))
            {
                var now = DateTime.UtcNow;
                var assignmentQuery = _dbContext.WfpOrganizationAssignments
                    .AsNoTracking()
                    .Where(x =>
                        workforceProfileIds.Contains(x.WorkforceProfileId) &&
                        x.IsActive &&
                        !x.IsDelete &&
                        !x.IsCancel &&
                        x.EffectiveStartDate <= now &&
                        (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now));

                if (string.Equals(source, WorkflowValueConstants.ApproverSource.DepartmentHead, StringComparison.OrdinalIgnoreCase))
                {
                    if (!requesterContext.DepartmentId.HasValue)
                    {
                        return new List<ResolvedApprover>();
                    }

                    assignmentQuery = assignmentQuery.Where(x =>
                        x.DepartmentId == requesterContext.DepartmentId.Value);
                }
                else if (string.Equals(source, WorkflowValueConstants.ApproverSource.OrganizationHead, StringComparison.OrdinalIgnoreCase))
                {
                    if (!requesterContext.OrganizationUnitId.HasValue)
                    {
                        return new List<ResolvedApprover>();
                    }

                    assignmentQuery = assignmentQuery.Where(x =>
                        x.OrganizationUnitId == requesterContext.OrganizationUnitId.Value);
                }
                else if (string.Equals(source, WorkflowValueConstants.ApproverSource.SiteHr, StringComparison.OrdinalIgnoreCase))
                {
                    if (!requesterContext.HospitalSiteId.HasValue)
                    {
                        return new List<ResolvedApprover>();
                    }

                    assignmentQuery = assignmentQuery.Where(x =>
                        x.HospitalSiteId == requesterContext.HospitalSiteId.Value);
                }
                else if (string.Equals(source, WorkflowValueConstants.ApproverSource.CostCenterOwner, StringComparison.OrdinalIgnoreCase))
                {
                    if (!requesterContext.CostCenterId.HasValue)
                    {
                        return new List<ResolvedApprover>();
                    }

                    assignmentQuery = assignmentQuery.Where(x =>
                        x.CostCenterId == requesterContext.CostCenterId.Value);
                }
                else
                {
                    if (!requesterContext.LegalEntityId.HasValue)
                    {
                        return new List<ResolvedApprover>();
                    }

                    assignmentQuery = assignmentQuery.Where(x =>
                        x.LegalEntityId == requesterContext.LegalEntityId.Value);
                }

                var scopedProfileIds = await assignmentQuery
                    .Select(x => x.WorkforceProfileId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                activeUsers = activeUsers
                    .Where(x =>
                        x.WorkforceProfileId.HasValue &&
                        scopedProfileIds.Contains(x.WorkforceProfileId.Value))
                    .ToList();
            }

            return activeUsers
                .Select(x => new ResolvedApprover
                {
                    UserId = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    DisplayName = GetUserDisplayName(x),
                    RoleCode = roleCode.Trim(),
                    Source = source
                })
                .OrderBy(x => x.DisplayName)
                .ToList();
        }

        private static bool IsScopedRoleSource(string source)
        {
            return string.Equals(source, WorkflowValueConstants.ApproverSource.OrganizationHead, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(source, WorkflowValueConstants.ApproverSource.DepartmentHead, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(source, WorkflowValueConstants.ApproverSource.SiteHr, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(source, WorkflowValueConstants.ApproverSource.CorporateHr, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(source, WorkflowValueConstants.ApproverSource.PayrollOfficer, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(source, WorkflowValueConstants.ApproverSource.FinanceOfficer, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(source, WorkflowValueConstants.ApproverSource.CostCenterOwner, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(source, WorkflowValueConstants.ApproverSource.CredentialingCommittee, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<List<TrxWorkflowApproverAssignment>> BuildAssignmentsAsync(
            TrxWorkflowInstance instance,
            TrxWorkflowStepInstance stepInstance,
            MstWorkflowDefinition definition,
            MstWorkflowStep workflowStep,
            MstApprovalMatrix? matrix,
            IReadOnlyCollection<ResolvedApprover> approvers,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var assignments = new List<TrxWorkflowApproverAssignment>();
            var assignmentOrder = 1;

            foreach (var approver in approvers)
            {
                var assignment = new TrxWorkflowApproverAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkflowInstanceId = instance.Id,
                    WorkflowStepInstanceId = stepInstance.Id,
                    ApprovalMatrixId = matrix?.Id,
                    AssignedApproverUserId = approver.UserId,
                    AssignedApproverWorkforceProfileId = approver.WorkforceProfileId,
                    AssignedApproverRoleCode = approver.RoleCode,
                    ApproverSourceSnapshot = approver.Source,
                    AssignmentOrder = assignmentOrder,
                    AssignmentStatus = WorkflowValueConstants.AssignmentStatus.Pending,
                    AssignedAt = now,
                    DueAt = stepInstance.DueAt,
                    IsRequired = true,
                    IsCurrentAssignment = false,
                    IsDelegated = false,
                    ResolutionSnapshotJson = JsonSerializer.Serialize(new
                    {
                        source = approver.Source,
                        resolvedUserId = approver.UserId,
                        resolvedWorkforceProfileId = approver.WorkforceProfileId,
                        roleCode = approver.RoleCode,
                        workflowDefinitionId = definition.Id,
                        workflowStepId = workflowStep.Id,
                        approvalMatrixId = matrix?.Id
                    }),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                if (workflowStep.AllowDelegation)
                {
                    await ApplyDelegationAsync(
                        assignment,
                        definition.Id,
                        workflowStep.Id,
                        now,
                        cancellationToken);
                }

                assignments.Add(assignment);
                assignmentOrder++;
            }

            return assignments;
        }

        private async Task ApplyDelegationAsync(
            TrxWorkflowApproverAssignment assignment,
            Guid workflowDefinitionId,
            Guid workflowStepId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var delegation = await _dbContext.Set<TrxApprovalDelegation>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.DelegatorUserId == assignment.AssignedApproverUserId &&
                    (x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Active ||
                     x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Approved) &&
                    x.EffectiveStartAt <= now &&
                    x.EffectiveEndAt >= now &&
                    (x.AppliesToAllWorkflows ||
                     (!x.WorkflowDefinitionId.HasValue ||
                      x.WorkflowDefinitionId == workflowDefinitionId)) &&
                    (!x.WorkflowStepId.HasValue || x.WorkflowStepId == workflowStepId))
                .OrderByDescending(x => x.WorkflowStepId.HasValue)
                .ThenByDescending(x => x.WorkflowDefinitionId.HasValue)
                .ThenByDescending(x => x.EffectiveStartAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (delegation == null)
            {
                return;
            }

            var delegateUser = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == delegation.DelegateUserId && x.IsActive,
                    cancellationToken);

            if (delegateUser == null || delegateUser.Id == assignment.AssignedApproverUserId)
            {
                return;
            }

            assignment.OriginalApproverUserId = assignment.AssignedApproverUserId;
            assignment.OriginalApproverWorkforceProfileId =
                assignment.AssignedApproverWorkforceProfileId;
            assignment.AssignedApproverUserId = delegateUser.Id;
            assignment.AssignedApproverWorkforceProfileId = delegateUser.WorkforceProfileId;
            assignment.ApprovalDelegationId = delegation.Id;
            assignment.DelegatedAt = now;
            assignment.IsDelegated = true;
        }

        private static void ActivateStepGroup(
            TrxWorkflowInstance instance,
            int stepOrder,
            DateTime now)
        {
            var steps = instance.StepInstances
                .Where(x =>
                    x.StepOrder == stepOrder &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    string.Equals(
                        x.StepStatus,
                        WorkflowValueConstants.StepStatus.Pending,
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.StepCodeSnapshot)
                .ToList();

            foreach (var step in steps)
            {
                step.StepStatus = WorkflowValueConstants.StepStatus.InProgress;
                step.AvailableAt ??= now;
                step.StartedAt ??= now;
                step.IsCurrentStep = true;

                var assignments = step.ApproverAssignments
                    .Where(x => x.IsActive && !x.IsDelete && !x.IsCancel)
                    .OrderBy(x => x.AssignmentOrder)
                    .ToList();

                if (string.Equals(
                    step.ApprovalModeSnapshot,
                    WorkflowValueConstants.ApprovalMode.Sequential,
                    StringComparison.OrdinalIgnoreCase))
                {
                    var first = assignments.FirstOrDefault();
                    if (first != null)
                    {
                        SetAssignmentAvailable(first, now);
                    }
                }
                else
                {
                    foreach (var assignment in assignments)
                    {
                        SetAssignmentAvailable(assignment, now);
                    }
                }
            }

            if (steps.Count > 0)
            {
                instance.CurrentStepOrder = stepOrder;
                instance.CurrentStepCode = steps[0].StepCodeSnapshot;
            }
        }

        private static void SetAssignmentAvailable(
            TrxWorkflowApproverAssignment assignment,
            DateTime now)
        {
            if (!string.Equals(
                assignment.AssignmentStatus,
                WorkflowValueConstants.AssignmentStatus.Pending,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            assignment.AssignmentStatus = WorkflowValueConstants.AssignmentStatus.Available;
            assignment.AvailableAt ??= now;
            assignment.IsCurrentAssignment = true;
        }

        private async Task ProcessAutomaticStepsAsync(
            TrxWorkflowInstance instance,
            HumanResourceUserContextDto actorContext,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var safeguard = 0;

            while (safeguard < 100 &&
                   string.Equals(
                       instance.WorkflowStatus,
                       WorkflowValueConstants.WorkflowStatus.InProgress,
                       StringComparison.OrdinalIgnoreCase))
            {
                safeguard++;

                var currentOrder = instance.CurrentStepOrder;
                var currentSteps = instance.StepInstances
                    .Where(x =>
                        x.StepOrder == currentOrder &&
                        x.IsCurrentStep &&
                        x.IsActive &&
                        !x.IsDelete &&
                        !x.IsCancel)
                    .ToList();

                if (currentSteps.Count == 0)
                {
                    return;
                }

                var automaticSteps = currentSteps
                    .Where(x => x.IsAutoAction || IsSystemOnlyStep(x.StepTypeSnapshot))
                    .Where(x => !TerminalStepStatuses.Contains(x.StepStatus))
                    .ToList();

                foreach (var step in automaticSteps)
                {
                    var previousStepStatus = step.StepStatus;
                    step.StepStatus = WorkflowValueConstants.StepStatus.Completed;
                    step.CompletedAt = now;
                    step.IsCurrentStep = false;
                    step.UpdateDateTime = now;
                    step.UpdateBy = Guid.Empty;

                    _dbContext.Set<TrxApprovalAction>().Add(new TrxApprovalAction
                    {
                        Id = Guid.NewGuid(),
                        WorkflowInstanceId = instance.Id,
                        WorkflowStepInstanceId = step.Id,
                        ActionType = WorkflowValueConstants.ActionType.Complete,
                        ActionAt = now,
                        IsDelegated = false,
                        IsSystemAction = true,
                        ActionSource = WorkflowValueConstants.SourceChannel.System,
                        PreviousWorkflowStatus = instance.WorkflowStatus,
                        ResultingWorkflowStatus = instance.WorkflowStatus,
                        PreviousStepStatus = previousStepStatus,
                        ResultingStepStatus = step.StepStatus,
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = Guid.Empty
                    });

                    AddStatusHistory(
                        instance,
                        step,
                        null,
                        instance.WorkflowStatus,
                        instance.WorkflowStatus,
                        previousStepStatus,
                        step.StepStatus,
                        WorkflowValueConstants.ActionType.Complete,
                        $"Step {step.StepCodeSnapshot} diselesaikan otomatis oleh sistem.",
                        true,
                        now);
                }

                var currentGroupCompleted = currentSteps.All(x =>
                    TerminalStepStatuses.Contains(x.StepStatus));

                if (!currentGroupCompleted)
                {
                    return;
                }

                var advanced = await AdvanceToNextStepGroupAsync(
                    instance,
                    currentOrder,
                    actorContext,
                    now,
                    cancellationToken);

                if (!advanced)
                {
                    return;
                }
            }
        }

        private async Task AdvanceWorkflowAfterStepAsync(
            TrxWorkflowInstance instance,
            int completedStepOrder,
            HumanResourceUserContextDto actorContext,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var sameOrderSteps = instance.StepInstances
                .Where(x =>
                    x.StepOrder == completedStepOrder &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel)
                .ToList();

            if (sameOrderSteps.Any(x => !TerminalStepStatuses.Contains(x.StepStatus)))
            {
                return;
            }

            var advanced = await AdvanceToNextStepGroupAsync(
                instance,
                completedStepOrder,
                actorContext,
                now,
                cancellationToken);

            if (advanced)
            {
                await ProcessAutomaticStepsAsync(
                    instance,
                    actorContext,
                    now,
                    cancellationToken);
            }
        }

        private Task<bool> AdvanceToNextStepGroupAsync(
            TrxWorkflowInstance instance,
            int completedStepOrder,
            HumanResourceUserContextDto actorContext,
            DateTime now,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nextOrder = instance.StepInstances
                .Where(x =>
                    x.StepOrder > completedStepOrder &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    string.Equals(
                        x.StepStatus,
                        WorkflowValueConstants.StepStatus.Pending,
                        StringComparison.OrdinalIgnoreCase))
                .Select(x => (int?)x.StepOrder)
                .Min();

            if (!nextOrder.HasValue)
            {
                var previousWorkflowStatus = instance.WorkflowStatus;
                instance.WorkflowStatus = WorkflowValueConstants.WorkflowStatus.Completed;
                instance.CompletedAt = now;
                instance.LastActionAt = now;
                instance.CurrentStepOrder = 0;
                instance.CurrentStepCode = null;
                instance.UpdateDateTime = now;
                instance.UpdateBy = actorContext.UserId;

                AddStatusHistory(
                    instance,
                    null,
                    actorContext,
                    previousWorkflowStatus,
                    instance.WorkflowStatus,
                    null,
                    null,
                    WorkflowValueConstants.ActionType.Complete,
                    "Seluruh step workflow telah selesai.",
                    true,
                    now);

                return Task.FromResult(false);
            }

            var previousCode = instance.CurrentStepCode;
            ActivateStepGroup(instance, nextOrder.Value, now);

            AddStatusHistory(
                instance,
                null,
                actorContext,
                instance.WorkflowStatus,
                instance.WorkflowStatus,
                null,
                null,
                WorkflowValueConstants.ActionType.MoveNext,
                $"Workflow berpindah dari step {previousCode ?? "-"} ke {instance.CurrentStepCode ?? "-"}.",
                true,
                now);

            return Task.FromResult(true);
        }

        private static bool IsApprovalRequirementSatisfied(
            TrxWorkflowStepInstance step)
        {
            var requiredAssignments = step.ApproverAssignments
                .Where(x => x.IsRequired && x.IsActive && !x.IsDelete && !x.IsCancel)
                .ToList();

            if (requiredAssignments.Count == 0)
            {
                return true;
            }

            var approvedCount = requiredAssignments.Count(x =>
                string.Equals(
                    x.AssignmentStatus,
                    WorkflowValueConstants.AssignmentStatus.Approved,
                    StringComparison.OrdinalIgnoreCase));

            if (string.Equals(
                step.ApprovalModeSnapshot,
                WorkflowValueConstants.ApprovalMode.All,
                StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    step.ApprovalModeSnapshot,
                    WorkflowValueConstants.ApprovalMode.Sequential,
                    StringComparison.OrdinalIgnoreCase))
            {
                return approvedCount == requiredAssignments.Count;
            }

            if (string.Equals(
                step.ApprovalModeSnapshot,
                WorkflowValueConstants.ApprovalMode.Percentage,
                StringComparison.OrdinalIgnoreCase))
            {
                var requiredPercentage = step.RequiredApprovalPercentage ?? 100m;
                var actualPercentage = approvedCount * 100m / requiredAssignments.Count;
                return actualPercentage >= requiredPercentage;
            }

            var requiredCount = Math.Max(1, step.RequiredApprovalCount);
            return approvedCount >= requiredCount;
        }

        private static void ActivateNextSequentialAssignment(
            TrxWorkflowStepInstance step,
            DateTime now)
        {
            var next = step.ApproverAssignments
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    string.Equals(
                        x.AssignmentStatus,
                        WorkflowValueConstants.AssignmentStatus.Pending,
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.AssignmentOrder)
                .FirstOrDefault();

            if (next != null)
            {
                SetAssignmentAvailable(next, now);
            }
        }

        private static void CompleteApprovedStep(
            TrxWorkflowStepInstance step,
            Guid actorUserId,
            DateTime now)
        {
            step.StepStatus = WorkflowValueConstants.StepStatus.Approved;
            step.CompletedAt = now;
            step.IsCurrentStep = false;
            step.UpdateDateTime = now;
            step.UpdateBy = actorUserId;

            foreach (var assignment in step.ApproverAssignments.Where(x =>
                         x.IsActive &&
                         !x.IsDelete &&
                         !x.IsCancel &&
                         !string.Equals(
                             x.AssignmentStatus,
                             WorkflowValueConstants.AssignmentStatus.Approved,
                             StringComparison.OrdinalIgnoreCase)))
            {
                assignment.AssignmentStatus = WorkflowValueConstants.AssignmentStatus.Skipped;
                assignment.CompletedAt = now;
                assignment.IsCurrentAssignment = false;
                assignment.UpdateDateTime = now;
                assignment.UpdateBy = actorUserId;
            }
        }

        private void AddStatusHistory(
            TrxWorkflowInstance instance,
            TrxWorkflowStepInstance? step,
            HumanResourceUserContextDto? actorContext,
            string? fromWorkflowStatus,
            string toWorkflowStatus,
            string? fromStepStatus,
            string? toStepStatus,
            string actionType,
            string? comment,
            bool isSystemGenerated,
            DateTime now)
        {
            var nextSequence = instance.StatusHistories.Count == 0
                ? 1
                : instance.StatusHistories.Max(x => x.SequenceNumber) + 1;

            var history = new TrxWorkflowStatusHistory
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                WorkflowStepInstanceId = step?.Id,
                ChangedByUserId = isSystemGenerated ? null : actorContext?.UserId,
                ChangedByWorkforceProfileId = isSystemGenerated
                    ? null
                    : actorContext?.WorkforceProfileId,
                SequenceNumber = nextSequence,
                FromWorkflowStatus = fromWorkflowStatus,
                ToWorkflowStatus = toWorkflowStatus,
                FromStepStatus = fromStepStatus,
                ToStepStatus = toStepStatus,
                ActionType = actionType,
                ChangedAt = now,
                Comment = NormalizeOptionalText(comment),
                IsSystemGenerated = isSystemGenerated,
                StatusSnapshotJson = JsonSerializer.Serialize(new
                {
                    workflowStatus = toWorkflowStatus,
                    workflowStepInstanceId = step?.Id,
                    stepCode = step?.StepCodeSnapshot,
                    stepStatus = toStepStatus,
                    currentStepOrder = instance.CurrentStepOrder,
                    currentStepCode = instance.CurrentStepCode
                }),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = isSystemGenerated
                    ? Guid.Empty
                    : actorContext?.UserId ?? Guid.Empty,
                IsDelete = false,
                IsCancel = false
            };

            instance.StatusHistories.Add(history);
        }

        private static void AddComment(
            TrxWorkflowInstance instance,
            TrxWorkflowStepInstance? step,
            HumanResourceUserContextDto actorContext,
            string commentType,
            string commentText,
            DateTime now)
        {
            instance.Comments.Add(new TrxWorkflowComment
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                WorkflowStepInstanceId = step?.Id,
                CommentByUserId = actorContext.UserId,
                CommentByWorkforceProfileId = actorContext.WorkforceProfileId,
                CommentType = commentType,
                CommentText = commentText.Trim(),
                CommentedAt = now,
                IsRequesterVisible = true,
                IsInternalComment = false,
                IsSystemGenerated = false,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorContext.UserId,
                IsDelete = false,
                IsCancel = false
            });
        }

        private static List<string> BuildInstanceAvailableActions(
            TrxWorkflowInstance instance,
            HumanResourceUserContextDto? actorContext)
        {
            var actions = new List<string>();

            if (actorContext == null || instance.RequestedByUserId != actorContext.UserId)
            {
                return actions;
            }

            var isDraft = string.Equals(
                instance.WorkflowStatus,
                WorkflowValueConstants.WorkflowStatus.Draft,
                StringComparison.OrdinalIgnoreCase);

            var isRevisionRequested = string.Equals(
                instance.WorkflowStatus,
                WorkflowValueConstants.WorkflowStatus.RevisionRequested,
                StringComparison.OrdinalIgnoreCase);

            var isReturned = string.Equals(
                instance.WorkflowStatus,
                WorkflowValueConstants.WorkflowStatus.Returned,
                StringComparison.OrdinalIgnoreCase);

            var isSubmitted = string.Equals(
                instance.WorkflowStatus,
                WorkflowValueConstants.WorkflowStatus.Submitted,
                StringComparison.OrdinalIgnoreCase);

            var isInProgress = string.Equals(
                instance.WorkflowStatus,
                WorkflowValueConstants.WorkflowStatus.InProgress,
                StringComparison.OrdinalIgnoreCase);

            if (isDraft || isRevisionRequested)
            {
                actions.Add(WorkflowValueConstants.ActionType.Submit);
            }

            if (instance.WorkflowDefinition?.AllowRequesterCancel == true &&
                (isDraft || isRevisionRequested || isReturned || isSubmitted))
            {
                actions.Add(WorkflowValueConstants.ActionType.Cancel);
            }

            if (instance.WorkflowDefinition?.AllowRequesterWithdraw == true && isInProgress)
            {
                actions.Add(WorkflowValueConstants.ActionType.Withdraw);
            }

            return actions.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static List<string> BuildAssignmentAvailableActions(
            TrxWorkflowInstance instance,
            TrxWorkflowStepInstance step,
            TrxWorkflowApproverAssignment assignment,
            HumanResourceUserContextDto? actorContext)
        {
            var actions = new List<string>();

            if (actorContext == null ||
                assignment.AssignedApproverUserId != actorContext.UserId ||
                !step.IsCurrentStep ||
                !assignment.IsCurrentAssignment ||
                !string.Equals(
                    instance.WorkflowStatus,
                    WorkflowValueConstants.WorkflowStatus.InProgress,
                    StringComparison.OrdinalIgnoreCase) ||
                !IsAvailableAssignmentStatus(assignment.AssignmentStatus))
            {
                return actions;
            }

            actions.Add(ResolveApprovalActionType(step.StepTypeSnapshot));
            actions.Add(WorkflowValueConstants.ActionType.Reject);
            actions.Add(WorkflowValueConstants.ActionType.RequestRevision);

            if (instance.StepInstances.Any(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.StepOrder < step.StepOrder))
            {
                actions.Add(WorkflowValueConstants.ActionType.Return);
            }

            return actions.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static string? ValidateCreateRequest(
            CreateWorkflowInstanceRequest request)
        {
            if (request == null)
            {
                return "Request workflow wajib diisi.";
            }

            if (string.IsNullOrWhiteSpace(request.WorkflowDefinitionCode))
            {
                return "WorkflowDefinitionCode wajib diisi.";
            }

            if (string.IsNullOrWhiteSpace(request.ReferenceType))
            {
                return "ReferenceType wajib diisi.";
            }

            if (request.ReferenceId == Guid.Empty)
            {
                return "ReferenceId wajib diisi.";
            }

            return null;
        }

        private static string BuildDefinitionSnapshot(
            MstWorkflowDefinition definition,
            IReadOnlyCollection<MstWorkflowStep> steps)
        {
            return JsonSerializer.Serialize(new
            {
                definitionId = definition.Id,
                workflowCode = definition.WorkflowCode,
                workflowName = definition.WorkflowName,
                requestType = definition.RequestType,
                workflowCategory = definition.WorkflowCategory,
                version = definition.Version,
                effectiveStartDate = definition.EffectiveStartDate,
                effectiveEndDate = definition.EffectiveEndDate,
                allowRequesterCancel = definition.AllowRequesterCancel,
                allowRequesterWithdraw = definition.AllowRequesterWithdraw,
                allowParallelApproval = definition.AllowParallelApproval,
                allowStepSkip = definition.AllowStepSkip,
                stopOnRejection = definition.StopOnRejection,
                steps = steps
                    .OrderBy(x => x.StepOrder)
                    .ThenBy(x => x.StepCode)
                    .Select(x => new
                    {
                        id = x.Id,
                        x.StepCode,
                        x.StepName,
                        x.StepOrder,
                        x.StepType,
                        x.ApprovalMode,
                        x.RequiredApprovalCount,
                        x.RequiredApprovalPercentage,
                        x.ApproverSourceType,
                        x.IsRequired,
                        x.IsParallel,
                        x.AllowDelegation,
                        x.AllowSelfApproval,
                        x.OnApproveNextStepCode,
                        x.OnRejectStepCode
                    })
            });
        }

        private static string BuildRequestNumber()
        {
            var value = $"WF-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"
                .ToUpperInvariant();

            return value.Length <= 60
                ? value
                : value.Substring(0, 60);
        }

        private static string NormalizeSourceChannel(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return WorkflowValueConstants.SourceChannel.Web;
            }

            var normalized = value.Trim();

            if (string.Equals(normalized, WorkflowValueConstants.SourceChannel.Mobile, StringComparison.OrdinalIgnoreCase))
                return WorkflowValueConstants.SourceChannel.Mobile;
            if (string.Equals(normalized, WorkflowValueConstants.SourceChannel.Api, StringComparison.OrdinalIgnoreCase))
                return WorkflowValueConstants.SourceChannel.Api;
            if (string.Equals(normalized, WorkflowValueConstants.SourceChannel.Integration, StringComparison.OrdinalIgnoreCase))
                return WorkflowValueConstants.SourceChannel.Integration;
            if (string.Equals(normalized, WorkflowValueConstants.SourceChannel.System, StringComparison.OrdinalIgnoreCase))
                return WorkflowValueConstants.SourceChannel.System;

            return WorkflowValueConstants.SourceChannel.Web;
        }

        private static string ResolveActionSource(string? sourceChannel)
        {
            return NormalizeSourceChannel(sourceChannel);
        }

        private static string NormalizeApproverSource(string? value)
        {
            if (string.Equals(
                value,
                WorkflowValueConstants.ApproverSource.DirectManager,
                StringComparison.OrdinalIgnoreCase))
            {
                return WorkflowValueConstants.ApproverSource.RequesterManager;
            }

            return string.IsNullOrWhiteSpace(value)
                ? WorkflowValueConstants.ApproverSource.RequesterManager
                : value.Trim();
        }

        private static string ResolveApprovalActionType(string? stepType)
        {
            if (string.Equals(
                stepType,
                WorkflowValueConstants.StepType.Verification,
                StringComparison.OrdinalIgnoreCase))
            {
                return WorkflowValueConstants.ActionType.Verify;
            }

            if (string.Equals(
                stepType,
                WorkflowValueConstants.StepType.Acknowledgement,
                StringComparison.OrdinalIgnoreCase))
            {
                return WorkflowValueConstants.ActionType.Acknowledge;
            }

            return WorkflowValueConstants.ActionType.Approve;
        }

        private static bool IsSystemOnlyStep(string? stepType)
        {
            return string.Equals(
                       stepType,
                       WorkflowValueConstants.StepType.SystemAction,
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       stepType,
                       WorkflowValueConstants.StepType.Notification,
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAvailableAssignmentStatus(string? status)
        {
            return string.Equals(
                       status,
                       WorkflowValueConstants.AssignmentStatus.Available,
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       status,
                       WorkflowValueConstants.AssignmentStatus.InProgress,
                       StringComparison.OrdinalIgnoreCase);
        }

        private static string? SerializeJsonElement(JsonElement? element)
        {
            if (!element.HasValue ||
                element.Value.ValueKind == JsonValueKind.Null ||
                element.Value.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            return element.Value.GetRawText();
        }

        private static string? NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private string? GetIpAddress()
        {
            return _httpContextAccessor.HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();
        }

        private string? GetUserAgent()
        {
            var value = _httpContextAccessor.HttpContext?
                .Request
                .Headers["User-Agent"]
                .ToString();

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Length <= 500
                ? value
                : value.Substring(0, 500);
        }

        private static string GetUserDisplayName(ApplicationUser? user)
        {
            if (user == null)
            {
                return string.Empty;
            }

            return user.DisplayName ??
                   user.UserName ??
                   user.Email ??
                   user.UserCode;
        }

        private static List<WorkflowStringOptionResponse> BuildOptions(
            IEnumerable<(string Value, string Label)> source)
        {
            return source
                .Select(x => new WorkflowStringOptionResponse
                {
                    Value = x.Value,
                    Label = x.Label
                })
                .ToList();
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 25;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            return (pageNumber, pageSize);
        }

        private static DateRangeResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? period)
        {
            if (!string.IsNullOrWhiteSpace(period) &&
                !string.Equals(period, "custom", StringComparison.OrdinalIgnoreCase))
            {
                var today = DateTime.UtcNow.Date;
                var normalized = period.Trim().ToLowerInvariant();

                return normalized switch
                {
                    "today" => DateRangeResult.Valid(today, today.AddDays(1)),
                    "last7days" => DateRangeResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "last30days" => DateRangeResult.Valid(today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => DateRangeResult.Valid(
                        new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                    "lastmonth" => DateRangeResult.Valid(
                        new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1),
                        new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
                    _ => DateRangeResult.Invalid("Period filter tidak dikenali.")
                };
            }

            var start = startDate.HasValue
                ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc)
                : (DateTime?)null;
            var endExclusive = endDate.HasValue
                ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc)
                : (DateTime?)null;

            if (start.HasValue &&
                endExclusive.HasValue &&
                start.Value >= endExclusive.Value)
            {
                return DateRangeResult.Invalid(
                    "StartDate tidak boleh lebih besar atau sama dengan EndDate.");
            }

            return DateRangeResult.Valid(start, endExclusive);
        }

        private static async Task SafeRollbackAsync(
            IDbContextTransaction transaction,
            CancellationToken cancellationToken)
        {
            try
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            catch
            {
                // Menjaga exception utama agar tidak tertutup exception rollback.
            }
        }

        private sealed class ResolvedApprover
        {
            public Guid UserId { get; set; }

            public Guid? WorkforceProfileId { get; set; }

            public string DisplayName { get; set; } = string.Empty;

            public string? RoleCode { get; set; }

            public string Source { get; set; } = string.Empty;
        }

        private sealed class WorkflowRequestContext
        {
            public decimal? Amount { get; private set; }

            public decimal? DurationHours { get; private set; }

            public int? DurationDays { get; private set; }

            public Guid? EmployeeCategoryId { get; private set; }

            public Guid? EmploymentTypeId { get; private set; }

            public static WorkflowRequestContext Parse(JsonElement? element)
            {
                var result = new WorkflowRequestContext();

                if (!element.HasValue || element.Value.ValueKind != JsonValueKind.Object)
                {
                    return result;
                }

                result.Amount = ReadDecimal(
                    element.Value,
                    "amount",
                    "totalAmount",
                    "requestedAmount",
                    "estimatedCost");
                result.DurationHours = ReadDecimal(
                    element.Value,
                    "durationHours",
                    "requestedHours",
                    "totalHours");
                result.DurationDays = ReadInt(
                    element.Value,
                    "durationDays",
                    "requestedDays",
                    "totalDays");
                result.EmployeeCategoryId = ReadGuid(
                    element.Value,
                    "employeeCategoryId");
                result.EmploymentTypeId = ReadGuid(
                    element.Value,
                    "employmentTypeId");

                return result;
            }

            private static decimal? ReadDecimal(
                JsonElement element,
                params string[] names)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (!names.Any(x => string.Equals(
                        x,
                        property.Name,
                        StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    if (property.Value.ValueKind == JsonValueKind.Number &&
                        property.Value.TryGetDecimal(out var number))
                    {
                        return number;
                    }

                    if (property.Value.ValueKind == JsonValueKind.String &&
                        decimal.TryParse(property.Value.GetString(), out number))
                    {
                        return number;
                    }
                }

                return null;
            }

            private static int? ReadInt(
                JsonElement element,
                params string[] names)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (!names.Any(x => string.Equals(
                        x,
                        property.Name,
                        StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    if (property.Value.ValueKind == JsonValueKind.Number &&
                        property.Value.TryGetInt32(out var number))
                    {
                        return number;
                    }

                    if (property.Value.ValueKind == JsonValueKind.String &&
                        int.TryParse(property.Value.GetString(), out number))
                    {
                        return number;
                    }
                }

                return null;
            }

            private static Guid? ReadGuid(
                JsonElement element,
                params string[] names)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (!names.Any(x => string.Equals(
                        x,
                        property.Name,
                        StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    if (property.Value.ValueKind == JsonValueKind.String &&
                        Guid.TryParse(property.Value.GetString(), out var value) &&
                        value != Guid.Empty)
                    {
                        return value;
                    }
                }

                return null;
            }
        }

        private sealed class DateRangeResult
        {
            public bool IsValid { get; private set; }

            public DateTime? Start { get; private set; }

            public DateTime? EndExclusive { get; private set; }

            public string? ErrorMessage { get; private set; }

            public static DateRangeResult Valid(
                DateTime? start,
                DateTime? endExclusive)
            {
                return new DateRangeResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResult Invalid(string message)
            {
                return new DateRangeResult
                {
                    IsValid = false,
                    ErrorMessage = message
                };
            }
        }
    }
}
