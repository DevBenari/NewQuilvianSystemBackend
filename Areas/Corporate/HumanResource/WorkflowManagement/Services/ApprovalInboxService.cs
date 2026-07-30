using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public class ApprovalInboxService
    {
        private static readonly string[] OpenAssignmentStatuses =
        {
            WorkflowValueConstants.AssignmentStatus.Pending,
            WorkflowValueConstants.AssignmentStatus.Available,
            WorkflowValueConstants.AssignmentStatus.InProgress,
            WorkflowValueConstants.AssignmentStatus.Delegated
        };

        private static readonly string[] CompletedAssignmentStatuses =
        {
            WorkflowValueConstants.AssignmentStatus.Approved,
            WorkflowValueConstants.AssignmentStatus.Rejected,
            WorkflowValueConstants.AssignmentStatus.RevisionRequested,
            WorkflowValueConstants.AssignmentStatus.Returned,
            WorkflowValueConstants.AssignmentStatus.Completed,
            WorkflowValueConstants.AssignmentStatus.Skipped,
            WorkflowValueConstants.AssignmentStatus.Cancelled
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly HumanResourceContextService _humanResourceContextService;
        private readonly WorkflowService _workflowService;

        public ApprovalInboxService(
            ApplicationDbContext dbContext,
            HumanResourceContextService humanResourceContextService,
            WorkflowService workflowService)
        {
            _dbContext = dbContext;
            _humanResourceContextService = humanResourceContextService;
            _workflowService = workflowService;
        }

        public ApprovalInboxFilterMetadataResponse GetFilterMetadata()
        {
            return new ApprovalInboxFilterMetadataResponse
            {
                DefaultFilter = new ApprovalInboxDefaultFilterResponse(),
                PeriodOptions = BuildOptions(new[]
                {
                    ("today", "Hari Ini"),
                    ("last7days", "7 Hari Terakhir"),
                    ("last30days", "30 Hari Terakhir"),
                    ("thismonth", "Bulan Ini"),
                    ("lastmonth", "Bulan Lalu"),
                    ("custom", "Rentang Tanggal")
                }),
                ViewOptions = BuildOptions(new[]
                {
                    ("open", "Perlu Tindakan"),
                    ("completed", "Sudah Diproses"),
                    ("all", "Semua Assignment")
                }),
                AssignmentStatusOptions = BuildOptions(new[]
                {
                    (WorkflowValueConstants.AssignmentStatus.Pending, "Menunggu"),
                    (WorkflowValueConstants.AssignmentStatus.Available, "Tersedia"),
                    (WorkflowValueConstants.AssignmentStatus.InProgress, "Sedang Diproses"),
                    (WorkflowValueConstants.AssignmentStatus.Approved, "Disetujui"),
                    (WorkflowValueConstants.AssignmentStatus.Rejected, "Ditolak"),
                    (WorkflowValueConstants.AssignmentStatus.RevisionRequested, "Meminta Revisi"),
                    (WorkflowValueConstants.AssignmentStatus.Returned, "Dikembalikan"),
                    (WorkflowValueConstants.AssignmentStatus.Delegated, "Didelegasikan"),
                    (WorkflowValueConstants.AssignmentStatus.Completed, "Selesai"),
                    (WorkflowValueConstants.AssignmentStatus.Skipped, "Dilewati"),
                    (WorkflowValueConstants.AssignmentStatus.Cancelled, "Dibatalkan")
                }),
                StepTypeOptions = BuildOptions(new[]
                {
                    (WorkflowValueConstants.StepType.Approval, "Persetujuan"),
                    (WorkflowValueConstants.StepType.Review, "Review"),
                    (WorkflowValueConstants.StepType.Verification, "Verifikasi"),
                    (WorkflowValueConstants.StepType.Acknowledgement, "Acknowledgement"),
                    (WorkflowValueConstants.StepType.Notification, "Notifikasi"),
                    (WorkflowValueConstants.StepType.SystemAction, "Aksi Sistem")
                }),
                DueStatusOptions = BuildOptions(new[]
                {
                    ("overdue", "Terlambat"),
                    ("dueToday", "Jatuh Tempo Hari Ini"),
                    ("upcoming", "Akan Datang"),
                    ("noDueDate", "Tanpa Jatuh Tempo")
                }),
                SortOptions = new List<ApprovalInboxSortOptionResponse>
                {
                    new() { Value = "dueAt", Label = "Jatuh Tempo" },
                    new() { Value = "availableAt", Label = "Mulai Tersedia" },
                    new() { Value = "assignedAt", Label = "Tanggal Ditugaskan" },
                    new() { Value = "requestNumber", Label = "Nomor Permintaan" },
                    new() { Value = "workflowName", Label = "Nama Workflow" },
                    new() { Value = "requesterName", Label = "Nama Pemohon" },
                    new() { Value = "stepOrder", Label = "Urutan Step" },
                    new() { Value = "assignmentStatus", Label = "Status Assignment" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
        }

        public async Task<WorkflowServiceResult<ApprovalInboxSummaryResponse>> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<ApprovalInboxSummaryResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var now = DateTime.UtcNow;
            var today = now.Date;
            var tomorrow = today.AddDays(1);
            var query = BuildBaseQuery(actorResult.Data!.UserId);

            var result = new ApprovalInboxSummaryResponse
            {
                TotalAssigned = await query.CountAsync(cancellationToken),
                Open = await query.CountAsync(
                    x => OpenAssignmentStatuses.Contains(x.AssignmentStatus),
                    cancellationToken),
                Pending = await query.CountAsync(
                    x => x.AssignmentStatus == WorkflowValueConstants.AssignmentStatus.Pending,
                    cancellationToken),
                Available = await query.CountAsync(
                    x => x.AssignmentStatus == WorkflowValueConstants.AssignmentStatus.Available,
                    cancellationToken),
                InProgress = await query.CountAsync(
                    x => x.AssignmentStatus == WorkflowValueConstants.AssignmentStatus.InProgress,
                    cancellationToken),
                DueToday = await query.CountAsync(
                    x => OpenAssignmentStatuses.Contains(x.AssignmentStatus) &&
                         x.DueAt.HasValue &&
                         x.DueAt.Value >= today &&
                         x.DueAt.Value < tomorrow,
                    cancellationToken),
                Overdue = await query.CountAsync(
                    x => OpenAssignmentStatuses.Contains(x.AssignmentStatus) &&
                         x.DueAt.HasValue &&
                         x.DueAt.Value < now,
                    cancellationToken),
                DelegatedToMe = await query.CountAsync(
                    x => x.IsDelegated &&
                         x.OriginalApproverUserId.HasValue &&
                         x.OriginalApproverUserId.Value != x.AssignedApproverUserId,
                    cancellationToken),
                CompletedToday = await query.CountAsync(
                    x => CompletedAssignmentStatuses.Contains(x.AssignmentStatus) &&
                         x.CompletedAt.HasValue &&
                         x.CompletedAt.Value >= today &&
                         x.CompletedAt.Value < tomorrow,
                    cancellationToken),
                ApprovedToday = await query.CountAsync(
                    x => x.AssignmentStatus == WorkflowValueConstants.AssignmentStatus.Approved &&
                         x.CompletedAt.HasValue &&
                         x.CompletedAt.Value >= today &&
                         x.CompletedAt.Value < tomorrow,
                    cancellationToken),
                RejectedToday = await query.CountAsync(
                    x => x.AssignmentStatus == WorkflowValueConstants.AssignmentStatus.Rejected &&
                         x.CompletedAt.HasValue &&
                         x.CompletedAt.Value >= today &&
                         x.CompletedAt.Value < tomorrow,
                    cancellationToken)
            };

            return WorkflowServiceResult<ApprovalInboxSummaryResponse>.Ok(
                result,
                "Ringkasan approval inbox berhasil diambil.");
        }

        public async Task<WorkflowServiceResult<PagedResult<ApprovalInboxItemResponse>>> GetPagedAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? period,
            string? view,
            Guid? workflowDefinitionId,
            string? workflowCode,
            string? referenceType,
            string? assignmentStatus,
            string? stepType,
            string? dueStatus,
            bool? isDelegated,
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<PagedResult<ApprovalInboxItemResponse>>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var dateRange = ResolveDateRange(startDate, endDate, period);
            if (!dateRange.IsValid)
            {
                return WorkflowServiceResult<PagedResult<ApprovalInboxItemResponse>>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage!);
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            var now = DateTime.UtcNow;
            var query = ApplyFilters(
                BuildBaseQuery(actorResult.Data!.UserId),
                dateRange,
                view,
                workflowDefinitionId,
                workflowCode,
                referenceType,
                assignmentStatus,
                stepType,
                dueStatus,
                isDelegated,
                search,
                now);

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .Select(x => new ApprovalInboxProjection
                {
                    AssignmentId = x.Id,
                    WorkflowInstanceId = x.WorkflowInstanceId,
                    WorkflowStepInstanceId = x.WorkflowStepInstanceId,
                    ApprovalMatrixId = x.ApprovalMatrixId,
                    ApprovalDelegationId = x.ApprovalDelegationId,
                    RequestNumber = x.WorkflowInstance != null ? x.WorkflowInstance.RequestNumber : string.Empty,
                    WorkflowDefinitionId = x.WorkflowInstance != null ? x.WorkflowInstance.WorkflowDefinitionId : Guid.Empty,
                    WorkflowCode = x.WorkflowInstance != null && x.WorkflowInstance.WorkflowDefinition != null
                        ? x.WorkflowInstance.WorkflowDefinition.WorkflowCode
                        : string.Empty,
                    WorkflowName = x.WorkflowInstance != null && x.WorkflowInstance.WorkflowDefinition != null
                        ? x.WorkflowInstance.WorkflowDefinition.WorkflowName
                        : string.Empty,
                    WorkflowVersion = x.WorkflowInstance != null && x.WorkflowInstance.WorkflowDefinition != null
                        ? x.WorkflowInstance.WorkflowDefinition.Version
                        : 0,
                    ReferenceType = x.WorkflowInstance != null ? x.WorkflowInstance.ReferenceType : string.Empty,
                    ReferenceId = x.WorkflowInstance != null ? x.WorkflowInstance.ReferenceId : Guid.Empty,
                    ExternalReferenceNumber = x.WorkflowInstance != null ? x.WorkflowInstance.ExternalReferenceNumber : null,
                    WorkflowStatus = x.WorkflowInstance != null ? x.WorkflowInstance.WorkflowStatus : string.Empty,
                    SourceChannel = x.WorkflowInstance != null ? x.WorkflowInstance.SourceChannel : string.Empty,
                    RequestedByUserId = x.WorkflowInstance != null ? x.WorkflowInstance.RequestedByUserId : Guid.Empty,
                    RequestedByWorkforceProfileId = x.WorkflowInstance != null
                        ? x.WorkflowInstance.RequestedByWorkforceProfileId
                        : null,
                    RequestedByProfileCode = x.WorkflowInstance != null && x.WorkflowInstance.RequestedByWorkforceProfile != null
                        ? x.WorkflowInstance.RequestedByWorkforceProfile.ProfileCode
                        : null,
                    RequestedByName = x.WorkflowInstance != null && x.WorkflowInstance.RequestedByWorkforceProfile != null
                        ? x.WorkflowInstance.RequestedByWorkforceProfile.DisplayName
                        : x.WorkflowInstance != null && x.WorkflowInstance.RequestedByUser != null
                            ? x.WorkflowInstance.RequestedByUser.DisplayName ??
                              x.WorkflowInstance.RequestedByUser.UserName ??
                              x.WorkflowInstance.RequestedByUser.Email ??
                              x.WorkflowInstance.RequestedByUser.UserCode
                            : string.Empty,
                    StepOrder = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepOrder : 0,
                    StepCode = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepCodeSnapshot : string.Empty,
                    StepName = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepNameSnapshot : string.Empty,
                    StepType = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepTypeSnapshot : string.Empty,
                    ApprovalMode = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.ApprovalModeSnapshot : string.Empty,
                    StepStatus = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepStatus : string.Empty,
                    IsCurrentStep = x.WorkflowStepInstance != null && x.WorkflowStepInstance.IsCurrentStep,
                    ApproverSource = x.ApproverSourceSnapshot,
                    AssignedApproverRoleCode = x.AssignedApproverRoleCode,
                    AssignmentOrder = x.AssignmentOrder,
                    AssignmentStatus = x.AssignmentStatus,
                    IsRequired = x.IsRequired,
                    IsCurrentAssignment = x.IsCurrentAssignment,
                    IsDelegated = x.IsDelegated,
                    AssignedApproverUserId = x.AssignedApproverUserId,
                    AssignedApproverWorkforceProfileId = x.AssignedApproverWorkforceProfileId,
                    AssignedApproverProfileCode = x.AssignedApproverWorkforceProfile != null
                        ? x.AssignedApproverWorkforceProfile.ProfileCode
                        : null,
                    AssignedApproverName = x.AssignedApproverWorkforceProfile != null
                        ? x.AssignedApproverWorkforceProfile.DisplayName
                        : x.AssignedApproverUser != null
                            ? x.AssignedApproverUser.DisplayName ??
                              x.AssignedApproverUser.UserName ??
                              x.AssignedApproverUser.Email ??
                              x.AssignedApproverUser.UserCode
                            : string.Empty,
                    OriginalApproverUserId = x.OriginalApproverUserId,
                    OriginalApproverWorkforceProfileId = x.OriginalApproverWorkforceProfileId,
                    OriginalApproverName = x.OriginalApproverWorkforceProfile != null
                        ? x.OriginalApproverWorkforceProfile.DisplayName
                        : x.OriginalApproverUser != null
                            ? x.OriginalApproverUser.DisplayName ??
                              x.OriginalApproverUser.UserName ??
                              x.OriginalApproverUser.Email ??
                              x.OriginalApproverUser.UserCode
                            : null,
                    AssignedAt = x.AssignedAt,
                    AvailableAt = x.AvailableAt,
                    StartedAt = x.StartedAt,
                    DueAt = x.DueAt,
                    CompletedAt = x.CompletedAt,
                    SubmittedAt = x.WorkflowInstance != null ? x.WorkflowInstance.SubmittedAt : null,
                    LastActionAt = x.WorkflowInstance != null ? x.WorkflowInstance.LastActionAt : null
                })
                .ToListAsync(cancellationToken);

            var items = entities
                .Select(x => MapItem(x, now))
                .ToList();

            var result = new PagedResult<ApprovalInboxItemResponse>
            {
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)paging.PageSize),
                Items = items
            };

            return WorkflowServiceResult<PagedResult<ApprovalInboxItemResponse>>.Ok(
                result,
                "Data approval inbox berhasil diambil.");
        }

        public Task<WorkflowServiceResult<PagedResult<ApprovalInboxItemResponse>>> GetDelegatedToMeAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? period,
            string? view,
            string? assignmentStatus,
            string? stepType,
            string? dueStatus,
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return GetPagedAsync(
                startDate,
                endDate,
                period,
                view,
                null,
                null,
                null,
                assignmentStatus,
                stepType,
                dueStatus,
                true,
                search,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);
        }

        public async Task<WorkflowServiceResult<ApprovalInboxDetailResponse>> GetByIdAsync(
            Guid assignmentId,
            CancellationToken cancellationToken = default)
        {
            if (assignmentId == Guid.Empty)
            {
                return WorkflowServiceResult<ApprovalInboxDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Assignment id tidak valid.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<ApprovalInboxDetailResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var actor = actorResult.Data!;
            var now = DateTime.UtcNow;
            var projection = await BuildBaseQuery(actor.UserId)
                .Where(x => x.Id == assignmentId)
                .Select(x => new ApprovalInboxDetailProjection
                {
                    Item = new ApprovalInboxProjection
                    {
                        AssignmentId = x.Id,
                        WorkflowInstanceId = x.WorkflowInstanceId,
                        WorkflowStepInstanceId = x.WorkflowStepInstanceId,
                        ApprovalMatrixId = x.ApprovalMatrixId,
                        ApprovalDelegationId = x.ApprovalDelegationId,
                        RequestNumber = x.WorkflowInstance != null ? x.WorkflowInstance.RequestNumber : string.Empty,
                        WorkflowDefinitionId = x.WorkflowInstance != null ? x.WorkflowInstance.WorkflowDefinitionId : Guid.Empty,
                        WorkflowCode = x.WorkflowInstance != null && x.WorkflowInstance.WorkflowDefinition != null
                            ? x.WorkflowInstance.WorkflowDefinition.WorkflowCode
                            : string.Empty,
                        WorkflowName = x.WorkflowInstance != null && x.WorkflowInstance.WorkflowDefinition != null
                            ? x.WorkflowInstance.WorkflowDefinition.WorkflowName
                            : string.Empty,
                        WorkflowVersion = x.WorkflowInstance != null && x.WorkflowInstance.WorkflowDefinition != null
                            ? x.WorkflowInstance.WorkflowDefinition.Version
                            : 0,
                        ReferenceType = x.WorkflowInstance != null ? x.WorkflowInstance.ReferenceType : string.Empty,
                        ReferenceId = x.WorkflowInstance != null ? x.WorkflowInstance.ReferenceId : Guid.Empty,
                        ExternalReferenceNumber = x.WorkflowInstance != null ? x.WorkflowInstance.ExternalReferenceNumber : null,
                        WorkflowStatus = x.WorkflowInstance != null ? x.WorkflowInstance.WorkflowStatus : string.Empty,
                        SourceChannel = x.WorkflowInstance != null ? x.WorkflowInstance.SourceChannel : string.Empty,
                        RequestedByUserId = x.WorkflowInstance != null ? x.WorkflowInstance.RequestedByUserId : Guid.Empty,
                        RequestedByWorkforceProfileId = x.WorkflowInstance != null
                            ? x.WorkflowInstance.RequestedByWorkforceProfileId
                            : null,
                        RequestedByProfileCode = x.WorkflowInstance != null && x.WorkflowInstance.RequestedByWorkforceProfile != null
                            ? x.WorkflowInstance.RequestedByWorkforceProfile.ProfileCode
                            : null,
                        RequestedByName = x.WorkflowInstance != null && x.WorkflowInstance.RequestedByWorkforceProfile != null
                            ? x.WorkflowInstance.RequestedByWorkforceProfile.DisplayName
                            : x.WorkflowInstance != null && x.WorkflowInstance.RequestedByUser != null
                                ? x.WorkflowInstance.RequestedByUser.DisplayName ??
                                  x.WorkflowInstance.RequestedByUser.UserName ??
                                  x.WorkflowInstance.RequestedByUser.Email ??
                                  x.WorkflowInstance.RequestedByUser.UserCode
                                : string.Empty,
                        StepOrder = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepOrder : 0,
                        StepCode = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepCodeSnapshot : string.Empty,
                        StepName = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepNameSnapshot : string.Empty,
                        StepType = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepTypeSnapshot : string.Empty,
                        ApprovalMode = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.ApprovalModeSnapshot : string.Empty,
                        StepStatus = x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepStatus : string.Empty,
                        IsCurrentStep = x.WorkflowStepInstance != null && x.WorkflowStepInstance.IsCurrentStep,
                        ApproverSource = x.ApproverSourceSnapshot,
                        AssignedApproverRoleCode = x.AssignedApproverRoleCode,
                        AssignmentOrder = x.AssignmentOrder,
                        AssignmentStatus = x.AssignmentStatus,
                        IsRequired = x.IsRequired,
                        IsCurrentAssignment = x.IsCurrentAssignment,
                        IsDelegated = x.IsDelegated,
                        AssignedApproverUserId = x.AssignedApproverUserId,
                        AssignedApproverWorkforceProfileId = x.AssignedApproverWorkforceProfileId,
                        AssignedApproverProfileCode = x.AssignedApproverWorkforceProfile != null
                            ? x.AssignedApproverWorkforceProfile.ProfileCode
                            : null,
                        AssignedApproverName = x.AssignedApproverWorkforceProfile != null
                            ? x.AssignedApproverWorkforceProfile.DisplayName
                            : x.AssignedApproverUser != null
                                ? x.AssignedApproverUser.DisplayName ??
                                  x.AssignedApproverUser.UserName ??
                                  x.AssignedApproverUser.Email ??
                                  x.AssignedApproverUser.UserCode
                                : string.Empty,
                        OriginalApproverUserId = x.OriginalApproverUserId,
                        OriginalApproverWorkforceProfileId = x.OriginalApproverWorkforceProfileId,
                        OriginalApproverName = x.OriginalApproverWorkforceProfile != null
                            ? x.OriginalApproverWorkforceProfile.DisplayName
                            : x.OriginalApproverUser != null
                                ? x.OriginalApproverUser.DisplayName ??
                                  x.OriginalApproverUser.UserName ??
                                  x.OriginalApproverUser.Email ??
                                  x.OriginalApproverUser.UserCode
                                : null,
                        AssignedAt = x.AssignedAt,
                        AvailableAt = x.AvailableAt,
                        StartedAt = x.StartedAt,
                        DueAt = x.DueAt,
                        CompletedAt = x.CompletedAt,
                        SubmittedAt = x.WorkflowInstance != null ? x.WorkflowInstance.SubmittedAt : null,
                        LastActionAt = x.WorkflowInstance != null ? x.WorkflowInstance.LastActionAt : null
                    },
                    StepInstructions = x.WorkflowStepInstance != null
                        ? x.WorkflowStepInstance.InstructionsSnapshot
                        : null,
                    ApprovalMatrixCode = x.ApprovalMatrix != null
                        ? x.ApprovalMatrix.ApprovalMatrixCode
                        : null,
                    ApprovalMatrixName = x.ApprovalMatrix != null
                        ? x.ApprovalMatrix.ApprovalMatrixName
                        : null,
                    RequestType = x.WorkflowInstance != null && x.WorkflowInstance.WorkflowDefinition != null
                        ? x.WorkflowInstance.WorkflowDefinition.RequestType
                        : string.Empty
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (projection == null)
            {
                return WorkflowServiceResult<ApprovalInboxDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Approval assignment tidak ditemukan pada inbox user login.");
            }

            var workflowResult = await _workflowService.GetByIdAsync(
                projection.Item.WorkflowInstanceId,
                cancellationToken);

            if (!workflowResult.Success)
            {
                return WorkflowServiceResult<ApprovalInboxDetailResponse>.Fail(
                    workflowResult.StatusCode,
                    workflowResult.Message);
            }

            var actionHistory = await _dbContext.Set<TrxApprovalAction>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.WorkflowApproverAssignmentId == assignmentId)
                .OrderByDescending(x => x.ActionAt)
                .Select(x => new ApprovalInboxActionHistoryResponse
                {
                    Id = x.Id,
                    ActionType = x.ActionType,
                    ActionAt = x.ActionAt,
                    ActualActionByUserId = x.ActualActionByUserId,
                    ActualActionByWorkforceProfileId = x.ActualActionByWorkforceProfileId,
                    ActualActionByName = x.ActualActionByWorkforceProfile != null
                        ? x.ActualActionByWorkforceProfile.DisplayName
                        : x.ActualActionByUser != null
                            ? x.ActualActionByUser.DisplayName ??
                              x.ActualActionByUser.UserName ??
                              x.ActualActionByUser.Email ??
                              x.ActualActionByUser.UserCode
                            : x.IsSystemAction ? "System" : string.Empty,
                    Comment = x.Comment,
                    IsDelegated = x.IsDelegated,
                    IsSystemAction = x.IsSystemAction,
                    ActionSource = x.ActionSource,
                    ActionReasonId = x.ActionReasonId,
                    ActionReasonCode = x.ActionReasonCodeSnapshot,
                    ActionReasonName = x.ActionReasonNameSnapshot,
                    PreviousWorkflowStatus = x.PreviousWorkflowStatus,
                    ResultingWorkflowStatus = x.ResultingWorkflowStatus,
                    PreviousStepStatus = x.PreviousStepStatus,
                    ResultingStepStatus = x.ResultingStepStatus
                })
                .ToListAsync(cancellationToken);

            var today = DateTime.UtcNow.Date;
            var workflowStepId = workflowResult.Data!.Steps
                .Where(x => x.Id == projection.Item.WorkflowStepInstanceId)
                .Select(x => (Guid?)x.WorkflowStepId)
                .FirstOrDefault();

            var rejectionReasons = await _dbContext.Set<MstRejectionReason>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.RequestType == projection.RequestType &&
                    (!x.WorkflowDefinitionId.HasValue ||
                     x.WorkflowDefinitionId == projection.Item.WorkflowDefinitionId) &&
                    (!x.WorkflowStepId.HasValue ||
                     x.WorkflowStepId == workflowStepId) &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today))
                .OrderByDescending(x => x.WorkflowStepId.HasValue)
                .ThenByDescending(x => x.WorkflowDefinitionId.HasValue)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.ReasonName)
                .Select(x => new ApprovalInboxRejectionReasonOptionResponse
                {
                    Id = x.Id,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    ReasonCategory = x.ReasonCategory,
                    RejectAction = x.RejectAction,
                    ReturnToStepCode = x.ReturnToStepCode,
                    IsCommentRequired = x.IsCommentRequired,
                    IsAttachmentRequired = x.IsAttachmentRequired,
                    AllowResubmit = x.AllowResubmit
                })
                .ToListAsync(cancellationToken);

            var result = new ApprovalInboxDetailResponse
            {
                Assignment = MapItem(projection.Item, now),
                StepInstructions = projection.StepInstructions,
                ApprovalMatrixCode = projection.ApprovalMatrixCode,
                ApprovalMatrixName = projection.ApprovalMatrixName,
                Workflow = workflowResult.Data!,
                ActionHistory = actionHistory,
                RejectionReasons = rejectionReasons
            };

            return WorkflowServiceResult<ApprovalInboxDetailResponse>.Ok(
                result,
                "Detail approval inbox berhasil diambil.");
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> ApproveAsync(
            Guid assignmentId,
            WorkflowApproveRequest? request,
            CancellationToken cancellationToken = default)
        {
            var target = await GetActionTargetAsync(assignmentId, cancellationToken);
            if (!target.Success)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    target.StatusCode,
                    target.Message);
            }

            return await _workflowService.ApproveAsync(
                target.Data!.WorkflowInstanceId,
                assignmentId,
                request,
                cancellationToken);
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> RejectAsync(
            Guid assignmentId,
            WorkflowRejectRequest request,
            CancellationToken cancellationToken = default)
        {
            var target = await GetActionTargetAsync(assignmentId, cancellationToken);
            if (!target.Success)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    target.StatusCode,
                    target.Message);
            }

            return await _workflowService.RejectAsync(
                target.Data!.WorkflowInstanceId,
                assignmentId,
                request,
                cancellationToken);
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> RequestRevisionAsync(
            Guid assignmentId,
            WorkflowRequestRevisionRequest request,
            CancellationToken cancellationToken = default)
        {
            var target = await GetActionTargetAsync(assignmentId, cancellationToken);
            if (!target.Success)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    target.StatusCode,
                    target.Message);
            }

            return await _workflowService.RequestRevisionAsync(
                target.Data!.WorkflowInstanceId,
                assignmentId,
                request,
                cancellationToken);
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> ReturnAsync(
            Guid assignmentId,
            WorkflowReturnRequest request,
            CancellationToken cancellationToken = default)
        {
            var target = await GetActionTargetAsync(assignmentId, cancellationToken);
            if (!target.Success)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    target.StatusCode,
                    target.Message);
            }

            return await _workflowService.ReturnAsync(
                target.Data!.WorkflowInstanceId,
                assignmentId,
                request,
                cancellationToken);
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> VerifyAsync(
            Guid assignmentId,
            WorkflowVerifyRequest? request,
            CancellationToken cancellationToken = default)
        {
            var target = await GetActionTargetAsync(assignmentId, cancellationToken);
            if (!target.Success)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    target.StatusCode,
                    target.Message);
            }

            return await _workflowService.VerifyAsync(
                target.Data!.WorkflowInstanceId,
                assignmentId,
                request,
                cancellationToken);
        }

        public async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> AcknowledgeAsync(
            Guid assignmentId,
            WorkflowAcknowledgeRequest? request,
            CancellationToken cancellationToken = default)
        {
            var target = await GetActionTargetAsync(assignmentId, cancellationToken);
            if (!target.Success)
            {
                return WorkflowServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    target.StatusCode,
                    target.Message);
            }

            return await _workflowService.AcknowledgeAsync(
                target.Data!.WorkflowInstanceId,
                assignmentId,
                request,
                cancellationToken);
        }

        private IQueryable<TrxWorkflowApproverAssignment> BuildBaseQuery(Guid userId)
        {
            return _dbContext.Set<TrxWorkflowApproverAssignment>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.AssignedApproverUserId == userId &&
                    x.WorkflowInstance != null &&
                    !x.WorkflowInstance.IsDelete &&
                    !x.WorkflowInstance.IsCancel &&
                    x.WorkflowInstance.IsActive &&
                    x.WorkflowStepInstance != null &&
                    !x.WorkflowStepInstance.IsDelete &&
                    !x.WorkflowStepInstance.IsCancel &&
                    x.WorkflowStepInstance.IsActive);
        }

        private static IQueryable<TrxWorkflowApproverAssignment> ApplyFilters(
            IQueryable<TrxWorkflowApproverAssignment> query,
            DateRangeResult dateRange,
            string? view,
            Guid? workflowDefinitionId,
            string? workflowCode,
            string? referenceType,
            string? assignmentStatus,
            string? stepType,
            string? dueStatus,
            bool? isDelegated,
            string? search,
            DateTime now)
        {
            if (dateRange.Start.HasValue)
            {
                query = query.Where(x => x.AssignedAt >= dateRange.Start.Value);
            }

            if (dateRange.EndExclusive.HasValue)
            {
                query = query.Where(x => x.AssignedAt < dateRange.EndExclusive.Value);
            }

            var normalizedView = string.IsNullOrWhiteSpace(view)
                ? "open"
                : view.Trim().ToLowerInvariant();

            if (normalizedView == "open")
            {
                query = query.Where(x => OpenAssignmentStatuses.Contains(x.AssignmentStatus));
            }
            else if (normalizedView == "completed")
            {
                query = query.Where(x => CompletedAssignmentStatuses.Contains(x.AssignmentStatus));
            }
            else if (normalizedView != "all")
            {
                query = query.Where(x => false);
            }

            if (workflowDefinitionId.HasValue && workflowDefinitionId.Value != Guid.Empty)
            {
                query = query.Where(x =>
                    x.WorkflowInstance != null &&
                    x.WorkflowInstance.WorkflowDefinitionId == workflowDefinitionId.Value);
            }

            if (!string.IsNullOrWhiteSpace(workflowCode))
            {
                var value = workflowCode.Trim().ToLower();
                query = query.Where(x =>
                    x.WorkflowInstance != null &&
                    x.WorkflowInstance.WorkflowDefinition != null &&
                    x.WorkflowInstance.WorkflowDefinition.WorkflowCode.ToLower() == value);
            }

            if (!string.IsNullOrWhiteSpace(referenceType))
            {
                var value = referenceType.Trim().ToLower();
                query = query.Where(x =>
                    x.WorkflowInstance != null &&
                    x.WorkflowInstance.ReferenceType.ToLower() == value);
            }

            if (!string.IsNullOrWhiteSpace(assignmentStatus))
            {
                var value = assignmentStatus.Trim().ToLower();
                query = query.Where(x => x.AssignmentStatus.ToLower() == value);
            }

            if (!string.IsNullOrWhiteSpace(stepType))
            {
                var value = stepType.Trim().ToLower();
                query = query.Where(x =>
                    x.WorkflowStepInstance != null &&
                    x.WorkflowStepInstance.StepTypeSnapshot.ToLower() == value);
            }

            if (isDelegated.HasValue)
            {
                query = query.Where(x => x.IsDelegated == isDelegated.Value);
            }

            if (!string.IsNullOrWhiteSpace(dueStatus))
            {
                var today = now.Date;
                var tomorrow = today.AddDays(1);

                query = dueStatus.Trim().ToLowerInvariant() switch
                {
                    "overdue" => query.Where(x =>
                        OpenAssignmentStatuses.Contains(x.AssignmentStatus) &&
                        x.DueAt.HasValue &&
                        x.DueAt.Value < now),
                    "duetoday" => query.Where(x =>
                        OpenAssignmentStatuses.Contains(x.AssignmentStatus) &&
                        x.DueAt.HasValue &&
                        x.DueAt.Value >= today &&
                        x.DueAt.Value < tomorrow),
                    "upcoming" => query.Where(x =>
                        OpenAssignmentStatuses.Contains(x.AssignmentStatus) &&
                        x.DueAt.HasValue &&
                        x.DueAt.Value >= tomorrow),
                    "noduedate" => query.Where(x => !x.DueAt.HasValue),
                    _ => query.Where(x => false)
                };
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.WorkflowInstance != null &&
                    (
                        x.WorkflowInstance.RequestNumber.ToLower().Contains(keyword) ||
                        x.WorkflowInstance.ReferenceType.ToLower().Contains(keyword) ||
                        (x.WorkflowInstance.ExternalReferenceNumber != null &&
                         x.WorkflowInstance.ExternalReferenceNumber.ToLower().Contains(keyword)) ||
                        (x.WorkflowInstance.WorkflowDefinition != null &&
                         (x.WorkflowInstance.WorkflowDefinition.WorkflowCode.ToLower().Contains(keyword) ||
                          x.WorkflowInstance.WorkflowDefinition.WorkflowName.ToLower().Contains(keyword))) ||
                        (x.WorkflowInstance.RequestedByWorkforceProfile != null &&
                         (x.WorkflowInstance.RequestedByWorkforceProfile.ProfileCode.ToLower().Contains(keyword) ||
                          x.WorkflowInstance.RequestedByWorkforceProfile.DisplayName.ToLower().Contains(keyword))) ||
                        (x.WorkflowStepInstance != null &&
                         (x.WorkflowStepInstance.StepCodeSnapshot.ToLower().Contains(keyword) ||
                          x.WorkflowStepInstance.StepNameSnapshot.ToLower().Contains(keyword)))
                    ));
            }

            return query;
        }

        private static IOrderedQueryable<TrxWorkflowApproverAssignment> ApplySorting(
            IQueryable<TrxWorkflowApproverAssignment> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            var normalizedSort = string.IsNullOrWhiteSpace(sortBy)
                ? "dueat"
                : sortBy.Trim().ToLowerInvariant();

            return normalizedSort switch
            {
                "availableat" => desc
                    ? query.OrderByDescending(x => x.AvailableAt).ThenByDescending(x => x.AssignedAt)
                    : query.OrderBy(x => x.AvailableAt == null).ThenBy(x => x.AvailableAt).ThenBy(x => x.AssignedAt),
                "assignedat" => desc
                    ? query.OrderByDescending(x => x.AssignedAt)
                    : query.OrderBy(x => x.AssignedAt),
                "requestnumber" => desc
                    ? query.OrderByDescending(x => x.WorkflowInstance != null ? x.WorkflowInstance.RequestNumber : string.Empty)
                    : query.OrderBy(x => x.WorkflowInstance != null ? x.WorkflowInstance.RequestNumber : string.Empty),
                "workflowname" => desc
                    ? query.OrderByDescending(x => x.WorkflowInstance != null && x.WorkflowInstance.WorkflowDefinition != null
                        ? x.WorkflowInstance.WorkflowDefinition.WorkflowName
                        : string.Empty)
                    : query.OrderBy(x => x.WorkflowInstance != null && x.WorkflowInstance.WorkflowDefinition != null
                        ? x.WorkflowInstance.WorkflowDefinition.WorkflowName
                        : string.Empty),
                "requestername" => desc
                    ? query.OrderByDescending(x => x.WorkflowInstance != null && x.WorkflowInstance.RequestedByWorkforceProfile != null
                        ? x.WorkflowInstance.RequestedByWorkforceProfile.DisplayName
                        : string.Empty)
                    : query.OrderBy(x => x.WorkflowInstance != null && x.WorkflowInstance.RequestedByWorkforceProfile != null
                        ? x.WorkflowInstance.RequestedByWorkforceProfile.DisplayName
                        : string.Empty),
                "steporder" => desc
                    ? query.OrderByDescending(x => x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepOrder : 0)
                    : query.OrderBy(x => x.WorkflowStepInstance != null ? x.WorkflowStepInstance.StepOrder : 0),
                "assignmentstatus" => desc
                    ? query.OrderByDescending(x => x.AssignmentStatus).ThenByDescending(x => x.AssignedAt)
                    : query.OrderBy(x => x.AssignmentStatus).ThenBy(x => x.AssignedAt),
                _ => desc
                    ? query.OrderByDescending(x => x.DueAt).ThenByDescending(x => x.AssignedAt)
                    : query.OrderBy(x => x.DueAt == null).ThenBy(x => x.DueAt).ThenBy(x => x.AssignedAt)
            };
        }

        private async Task<WorkflowServiceResult<ActionTarget>> GetActionTargetAsync(
            Guid assignmentId,
            CancellationToken cancellationToken)
        {
            if (assignmentId == Guid.Empty)
            {
                return WorkflowServiceResult<ActionTarget>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Assignment id tidak valid.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<ActionTarget>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var target = await BuildBaseQuery(actorResult.Data!.UserId)
                .Where(x => x.Id == assignmentId)
                .Select(x => new ActionTarget
                {
                    AssignmentId = x.Id,
                    WorkflowInstanceId = x.WorkflowInstanceId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (target == null)
            {
                return WorkflowServiceResult<ActionTarget>.Fail(
                    StatusCodes.Status404NotFound,
                    "Approval assignment tidak ditemukan pada inbox user login.");
            }

            return WorkflowServiceResult<ActionTarget>.Ok(
                target,
                "Target approval assignment berhasil ditemukan.");
        }

        private async Task<WorkflowServiceResult<HumanResourceUserContextDto>> GetActorContextAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                var actor = await _humanResourceContextService.GetCurrentAsync(cancellationToken);
                return WorkflowServiceResult<HumanResourceUserContextDto>.Ok(
                    actor,
                    "Konteks user berhasil diambil.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return WorkflowServiceResult<HumanResourceUserContextDto>.Fail(
                    StatusCodes.Status401Unauthorized,
                    ex.Message);
            }
        }

        private static ApprovalInboxItemResponse MapItem(
            ApprovalInboxProjection source,
            DateTime now)
        {
            return new ApprovalInboxItemResponse
            {
                AssignmentId = source.AssignmentId,
                WorkflowInstanceId = source.WorkflowInstanceId,
                WorkflowStepInstanceId = source.WorkflowStepInstanceId,
                ApprovalMatrixId = source.ApprovalMatrixId,
                ApprovalDelegationId = source.ApprovalDelegationId,
                RequestNumber = source.RequestNumber,
                WorkflowDefinitionId = source.WorkflowDefinitionId,
                WorkflowCode = source.WorkflowCode,
                WorkflowName = source.WorkflowName,
                WorkflowVersion = source.WorkflowVersion,
                ReferenceType = source.ReferenceType,
                ReferenceId = source.ReferenceId,
                ExternalReferenceNumber = source.ExternalReferenceNumber,
                WorkflowStatus = source.WorkflowStatus,
                SourceChannel = source.SourceChannel,
                RequestedByUserId = source.RequestedByUserId,
                RequestedByWorkforceProfileId = source.RequestedByWorkforceProfileId,
                RequestedByProfileCode = source.RequestedByProfileCode,
                RequestedByName = source.RequestedByName,
                StepOrder = source.StepOrder,
                StepCode = source.StepCode,
                StepName = source.StepName,
                StepType = source.StepType,
                ApprovalMode = source.ApprovalMode,
                StepStatus = source.StepStatus,
                IsCurrentStep = source.IsCurrentStep,
                ApproverSource = source.ApproverSource,
                AssignedApproverRoleCode = source.AssignedApproverRoleCode,
                AssignmentOrder = source.AssignmentOrder,
                AssignmentStatus = source.AssignmentStatus,
                IsRequired = source.IsRequired,
                IsCurrentAssignment = source.IsCurrentAssignment,
                IsDelegated = source.IsDelegated,
                AssignedApproverUserId = source.AssignedApproverUserId,
                AssignedApproverWorkforceProfileId = source.AssignedApproverWorkforceProfileId,
                AssignedApproverProfileCode = source.AssignedApproverProfileCode,
                AssignedApproverName = source.AssignedApproverName,
                OriginalApproverUserId = source.OriginalApproverUserId,
                OriginalApproverWorkforceProfileId = source.OriginalApproverWorkforceProfileId,
                OriginalApproverName = source.OriginalApproverName,
                AssignedAt = source.AssignedAt,
                AvailableAt = source.AvailableAt,
                StartedAt = source.StartedAt,
                DueAt = source.DueAt,
                CompletedAt = source.CompletedAt,
                SubmittedAt = source.SubmittedAt,
                LastActionAt = source.LastActionAt,
                DueStatus = ResolveDueStatus(source.DueAt, source.AssignmentStatus, now),
                DueInHours = source.DueAt.HasValue
                    ? Math.Round((source.DueAt.Value - now).TotalHours, 2)
                    : null,
                AvailableActions = BuildAvailableActions(source)
            };
        }

        private static List<string> BuildAvailableActions(ApprovalInboxProjection source)
        {
            var actions = new List<string>();

            if (!source.IsCurrentStep ||
                !source.IsCurrentAssignment ||
                !string.Equals(
                    source.WorkflowStatus,
                    WorkflowValueConstants.WorkflowStatus.InProgress,
                    StringComparison.OrdinalIgnoreCase) ||
                !IsOpenAssignmentStatus(source.AssignmentStatus))
            {
                return actions;
            }

            if (string.Equals(
                source.StepType,
                WorkflowValueConstants.StepType.Verification,
                StringComparison.OrdinalIgnoreCase))
            {
                actions.Add(WorkflowValueConstants.ActionType.Verify);
            }
            else if (string.Equals(
                source.StepType,
                WorkflowValueConstants.StepType.Acknowledgement,
                StringComparison.OrdinalIgnoreCase))
            {
                actions.Add(WorkflowValueConstants.ActionType.Acknowledge);
            }
            else
            {
                actions.Add(WorkflowValueConstants.ActionType.Approve);
            }

            actions.Add(WorkflowValueConstants.ActionType.Reject);
            actions.Add(WorkflowValueConstants.ActionType.RequestRevision);

            if (source.StepOrder > 1)
            {
                actions.Add(WorkflowValueConstants.ActionType.Return);
            }

            return actions
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool IsOpenAssignmentStatus(string status)
        {
            return OpenAssignmentStatuses.Contains(
                status,
                StringComparer.OrdinalIgnoreCase);
        }

        private static string ResolveDueStatus(
            DateTime? dueAt,
            string assignmentStatus,
            DateTime now)
        {
            if (!dueAt.HasValue)
            {
                return "NoDueDate";
            }

            if (!IsOpenAssignmentStatus(assignmentStatus))
            {
                return "Completed";
            }

            if (dueAt.Value < now)
            {
                return "Overdue";
            }

            var today = now.Date;
            var tomorrow = today.AddDays(1);

            if (dueAt.Value >= today && dueAt.Value < tomorrow)
            {
                return "DueToday";
            }

            return "Upcoming";
        }

        private static List<ApprovalInboxStringOptionResponse> BuildOptions(
            IEnumerable<(string Value, string Label)> values)
        {
            return values
                .Select(x => new ApprovalInboxStringOptionResponse
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
            return (
                pageNumber < 1 ? 1 : pageNumber,
                pageSize < 1 ? 25 : Math.Min(pageSize, 100));
        }

        private static DateRangeResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? period)
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
            {
                return DateRangeResult.Fail("Tanggal mulai tidak boleh lebih besar dari tanggal selesai.");
            }

            if (startDate.HasValue || endDate.HasValue)
            {
                return DateRangeResult.Ok(
                    startDate.HasValue
                        ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc)
                        : null,
                    endDate.HasValue
                        ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc)
                        : null);
            }

            var today = DateTime.UtcNow.Date;
            var normalized = string.IsNullOrWhiteSpace(period)
                ? "last30days"
                : period.Trim().ToLowerInvariant();

            return normalized switch
            {
                "today" => DateRangeResult.Ok(today, today.AddDays(1)),
                "last7days" => DateRangeResult.Ok(today.AddDays(-6), today.AddDays(1)),
                "last30days" => DateRangeResult.Ok(today.AddDays(-29), today.AddDays(1)),
                "thismonth" => DateRangeResult.Ok(
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                "lastmonth" => DateRangeResult.Ok(
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1),
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
                "custom" => DateRangeResult.Ok(null, null),
                "all" => DateRangeResult.Ok(null, null),
                _ => DateRangeResult.Fail("Period approval inbox tidak valid.")
            };
        }

        private sealed class ApprovalInboxProjection
        {
            public Guid AssignmentId { get; set; }
            public Guid WorkflowInstanceId { get; set; }
            public Guid WorkflowStepInstanceId { get; set; }
            public Guid? ApprovalMatrixId { get; set; }
            public Guid? ApprovalDelegationId { get; set; }
            public string RequestNumber { get; set; } = string.Empty;
            public Guid WorkflowDefinitionId { get; set; }
            public string WorkflowCode { get; set; } = string.Empty;
            public string WorkflowName { get; set; } = string.Empty;
            public int WorkflowVersion { get; set; }
            public string ReferenceType { get; set; } = string.Empty;
            public Guid ReferenceId { get; set; }
            public string? ExternalReferenceNumber { get; set; }
            public string WorkflowStatus { get; set; } = string.Empty;
            public string SourceChannel { get; set; } = string.Empty;
            public Guid RequestedByUserId { get; set; }
            public Guid? RequestedByWorkforceProfileId { get; set; }
            public string? RequestedByProfileCode { get; set; }
            public string RequestedByName { get; set; } = string.Empty;
            public int StepOrder { get; set; }
            public string StepCode { get; set; } = string.Empty;
            public string StepName { get; set; } = string.Empty;
            public string StepType { get; set; } = string.Empty;
            public string ApprovalMode { get; set; } = string.Empty;
            public string StepStatus { get; set; } = string.Empty;
            public bool IsCurrentStep { get; set; }
            public string ApproverSource { get; set; } = string.Empty;
            public string? AssignedApproverRoleCode { get; set; }
            public int AssignmentOrder { get; set; }
            public string AssignmentStatus { get; set; } = string.Empty;
            public bool IsRequired { get; set; }
            public bool IsCurrentAssignment { get; set; }
            public bool IsDelegated { get; set; }
            public Guid AssignedApproverUserId { get; set; }
            public Guid? AssignedApproverWorkforceProfileId { get; set; }
            public string? AssignedApproverProfileCode { get; set; }
            public string AssignedApproverName { get; set; } = string.Empty;
            public Guid? OriginalApproverUserId { get; set; }
            public Guid? OriginalApproverWorkforceProfileId { get; set; }
            public string? OriginalApproverName { get; set; }
            public DateTime AssignedAt { get; set; }
            public DateTime? AvailableAt { get; set; }
            public DateTime? StartedAt { get; set; }
            public DateTime? DueAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public DateTime? SubmittedAt { get; set; }
            public DateTime? LastActionAt { get; set; }
        }

        private sealed class ApprovalInboxDetailProjection
        {
            public ApprovalInboxProjection Item { get; set; } = new();
            public string? StepInstructions { get; set; }
            public string? ApprovalMatrixCode { get; set; }
            public string? ApprovalMatrixName { get; set; }
            public string RequestType { get; set; } = string.Empty;
        }

        private sealed class ActionTarget
        {
            public Guid AssignmentId { get; set; }
            public Guid WorkflowInstanceId { get; set; }
        }

        private sealed class DateRangeResult
        {
            public bool IsValid { get; private set; }
            public DateTime? Start { get; private set; }
            public DateTime? EndExclusive { get; private set; }
            public string? ErrorMessage { get; private set; }

            public static DateRangeResult Ok(DateTime? start, DateTime? endExclusive)
            {
                return new DateRangeResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResult Fail(string message)
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
