using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveCancellationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveRequestCalculationService _calculationService;
        private readonly WorkflowService _workflowService;
        private readonly LeaveCancellationWorkflowLifecycleService _lifecycleService;

        public LeaveCancellationService(
            ApplicationDbContext dbContext,
            LeaveRequestCalculationService calculationService,
            WorkflowService workflowService,
            LeaveCancellationWorkflowLifecycleService lifecycleService)
        {
            _dbContext = dbContext;
            _calculationService = calculationService;
            _workflowService = workflowService;
            _lifecycleService = lifecycleService;
        }

        public async Task<LeaveRequestServiceResult<LeaveLifecyclePagedResponse<LeaveCancellationResponse>>> GetMyPagedAsync(
            Guid actorUserId,
            LeaveLifecycleQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<LeaveLifecyclePagedResponse<LeaveCancellationResponse>>.Fail(actor.StatusCode, actor.Message);

            return await GetPagedCoreAsync(actor.Data.WorkforceProfileId, request, cancellationToken);
        }

        public async Task<LeaveRequestServiceResult<LeaveLifecyclePagedResponse<LeaveCancellationResponse>>> GetPagedAsync(
            LeaveLifecycleQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            return await GetPagedCoreAsync(request.WorkforceProfileId, request, cancellationToken);
        }

        public async Task<LeaveRequestServiceResult<LeaveCancellationResponse>> GetByIdAsync(
            Guid id,
            Guid? ownerWorkforceProfileId,
            CancellationToken cancellationToken = default)
        {
            var entity = await BuildQuery()
                .FirstOrDefaultAsync(x => x.Id == id &&
                    (!ownerWorkforceProfileId.HasValue || x.WorkforceProfileId == ownerWorkforceProfileId.Value),
                    cancellationToken);

            if (entity == null)
                return LeaveRequestServiceResult<LeaveCancellationResponse>.Fail(StatusCodes.Status404NotFound, "Leave cancellation tidak ditemukan.");

            return LeaveRequestServiceResult<LeaveCancellationResponse>.Ok(await MapAsync(entity, cancellationToken), "Detail leave cancellation berhasil diambil.");
        }

        public async Task<LeaveRequestServiceResult<LeaveLifecycleActionResponse>> CreateAsync(
            Guid actorUserId,
            CreateLeaveCancellationRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(actor.StatusCode, actor.Message);

            var leave = await _dbContext.Set<WfpLeaveRequest>()
                .Include(x => x.LeaveType)
                .FirstOrDefaultAsync(x => x.Id == request.LeaveRequestId &&
                    x.WorkforceProfileId == actor.Data.WorkforceProfileId && !x.IsDelete, cancellationToken);

            if (leave == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status404NotFound, "Leave request tidak ditemukan.");

            if (leave.LeaveRequestStatus != LeaveRequestValueConstants.Status.Approved &&
                leave.LeaveRequestStatus != LeaveRequestValueConstants.Status.Taken)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status409Conflict, "Cancellation setelah approval hanya dapat dibuat untuk leave Approved atau Taken.");

            var effectiveDate = request.EffectiveCancellationDate ?? leave.StartDate;
            if (effectiveDate < leave.StartDate || effectiveDate > leave.EndDate)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status400BadRequest, "EffectiveCancellationDate harus berada dalam periode leave.");

            var activeExists = await _dbContext.Set<TrxLeaveCancellationRequest>()
                .AnyAsync(x => x.LeaveRequestId == leave.Id &&
                    x.CancellationStatus != LeaveLifecycleValueConstants.CancellationStatus.Rejected &&
                    x.CancellationStatus != LeaveLifecycleValueConstants.CancellationStatus.Cancelled &&
                    x.CancellationStatus != LeaveLifecycleValueConstants.CancellationStatus.Applied &&
                    !x.IsDelete, cancellationToken);

            if (activeExists)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status409Conflict, "Masih terdapat cancellation request aktif untuk leave tersebut.");

            var restoredDays = await CalculateRestoredDaysAsync(leave, effectiveDate, cancellationToken);
            var now = DateTime.UtcNow;
            var entity = new TrxLeaveCancellationRequest
            {
                Id = Guid.NewGuid(),
                CancellationNumber = GenerateNumber("LCN"),
                LeaveRequestId = leave.Id,
                WorkforceProfileId = leave.WorkforceProfileId,
                RequestReasonId = request.RequestReasonId,
                RequestedAt = now,
                EffectiveCancellationDate = effectiveDate,
                RestoredDays = restoredDays,
                CancellationReason = request.CancellationReason.Trim(),
                CancellationStatus = LeaveLifecycleValueConstants.CancellationStatus.Draft,
                RequestedByUserId = actorUserId,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Ok(
                MapAction(entity, null, false, "Draft leave cancellation berhasil dibuat."),
                "Draft leave cancellation berhasil dibuat.",
                StatusCodes.Status201Created);
        }

        public async Task<LeaveRequestServiceResult<LeaveLifecycleActionResponse>> PrepareWorkflowAsync(
            Guid id,
            Guid actorUserId,
            PrepareLeaveLifecycleWorkflowRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(actor.StatusCode, actor.Message);

            var entity = await _dbContext.Set<TrxLeaveCancellationRequest>()
                .Include(x => x.LeaveRequest)
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == actor.Data.WorkforceProfileId && !x.IsDelete, cancellationToken);

            if (entity?.LeaveRequest == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status404NotFound, "Leave cancellation tidak ditemukan.");

            if (entity.WorkflowInstanceId.HasValue)
            {
                var existing = await _dbContext.Set<TrxWorkflowInstance>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == entity.WorkflowInstanceId.Value && !x.IsDelete, cancellationToken);
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Ok(
                    MapAction(entity, existing?.WorkflowStatus, true, "Workflow cancellation sudah tersedia."),
                    "Workflow cancellation sudah tersedia.");
            }

            if (entity.CancellationStatus != LeaveLifecycleValueConstants.CancellationStatus.Draft &&
                entity.CancellationStatus != LeaveLifecycleValueConstants.CancellationStatus.NeedRevision)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status409Conflict, "Workflow hanya dapat disiapkan pada status Draft atau NeedRevision.");

            var context = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                cancellationId = entity.Id,
                entity.CancellationNumber,
                entity.LeaveRequestId,
                entity.WorkforceProfileId,
                entity.EffectiveCancellationDate,
                entity.RestoredDays,
                entity.CancellationReason
            })).RootElement.Clone();

            var result = await _workflowService.CreateAsync(new CreateWorkflowInstanceRequest
            {
                WorkflowDefinitionCode = "LEAVE_CANCELLATION",
                ReferenceType = LeaveLifecycleValueConstants.ReferenceType.LeaveCancellation,
                ReferenceId = entity.Id,
                ExternalReferenceNumber = entity.CancellationNumber,
                SourceChannel = string.IsNullOrWhiteSpace(request.SourceChannel) ? "Web" : request.SourceChannel.Trim(),
                RequestCorrelationId = NullIfWhiteSpace(request.CorrelationId),
                IdempotencyKey = NullIfWhiteSpace(request.IdempotencyKey) ?? $"LEAVE-CANCELLATION-WF:{entity.Id:N}",
                RequestContext = context,
                SelectedApproverUserIds = request.SelectedApproverUserIds
            }, cancellationToken);

            if (!result.Success || result.Data == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(result.StatusCode, result.Message);

            entity.WorkflowDefinitionId = result.Data.WorkflowDefinitionId;
            entity.WorkflowInstanceId = result.Data.Id;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Ok(
                MapAction(entity, result.Data.WorkflowStatus, false, "Workflow cancellation berhasil disiapkan."),
                "Workflow cancellation berhasil disiapkan.");
        }

        public async Task<LeaveRequestServiceResult<LeaveLifecycleActionResponse>> SubmitAsync(
            Guid id,
            Guid actorUserId,
            SubmitLeaveLifecycleWorkflowRequest request,
            CancellationToken cancellationToken = default)
        {
            var prepare = await PrepareWorkflowAsync(id, actorUserId, request, cancellationToken);
            if (!prepare.Success) return prepare;

            var entity = await _dbContext.Set<TrxLeaveCancellationRequest>()
                .FirstAsync(x => x.Id == id, cancellationToken);

            if (!entity.WorkflowInstanceId.HasValue)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status500InternalServerError, "Workflow cancellation belum tersedia.");

            var result = await _workflowService.SubmitAsync(entity.WorkflowInstanceId.Value, new WorkflowSubmitRequest
            {
                Comment = request.Comment,
                IdempotencyKey = NullIfWhiteSpace(request.IdempotencyKey) ?? $"LEAVE-CANCELLATION-SUBMIT:{entity.Id:N}"
            }, cancellationToken);

            if (!result.Success || result.Data == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(result.StatusCode, result.Message);

            entity.CancellationStatus = LeaveLifecycleValueConstants.CancellationStatus.Submitted;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Ok(
                MapAction(entity, result.Data.WorkflowStatus, false, "Leave cancellation berhasil disubmit."),
                "Leave cancellation berhasil disubmit.");
        }

        public async Task<LeaveRequestServiceResult<LeaveLifecycleActionResponse>> SynchronizeAsync(
            Guid id,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxLeaveCancellationRequest>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity?.WorkflowInstanceId == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status404NotFound, "Workflow cancellation belum tersedia.");

            var workflow = await _dbContext.Set<TrxWorkflowInstance>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == entity.WorkflowInstanceId.Value && !x.IsDelete, cancellationToken);

            if (workflow == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status404NotFound, "Workflow cancellation tidak ditemukan.");

            var sync = await _lifecycleService.SynchronizeAsync(workflow, actorUserId, allowAutoApply, cancellationToken);
            var refreshed = await _dbContext.Set<TrxLeaveCancellationRequest>().AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);

            return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Ok(
                MapAction(refreshed, workflow.WorkflowStatus, false, sync.WarningMessage ?? "Leave cancellation berhasil disinkronkan."),
                sync.WarningMessage ?? "Leave cancellation berhasil disinkronkan.");
        }

        private async Task<LeaveRequestServiceResult<LeaveLifecyclePagedResponse<LeaveCancellationResponse>>> GetPagedCoreAsync(
            Guid? workforceProfileId,
            LeaveLifecycleQueryRequest request,
            CancellationToken cancellationToken)
        {
            request.PageNumber = Math.Max(1, request.PageNumber);
            request.PageSize = Math.Clamp(request.PageSize, 1, 100);

            var query = BuildQuery();
            if (workforceProfileId.HasValue) query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);
            if (request.StartDate.HasValue) query = query.Where(x => x.RequestedAt.Date >= request.StartDate.Value.ToDateTime(TimeOnly.MinValue).Date);
            if (request.EndDate.HasValue) query = query.Where(x => x.RequestedAt.Date <= request.EndDate.Value.ToDateTime(TimeOnly.MinValue).Date);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.CancellationStatus == request.Status);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x => x.CancellationNumber.ToLower().Contains(keyword) ||
                    x.CancellationReason.ToLower().Contains(keyword) ||
                    (x.LeaveRequest != null && x.LeaveRequest.RequestNumber.ToLower().Contains(keyword)));
            }

            var total = await query.CountAsync(cancellationToken);
            query = request.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.CreateDateTime)
                : query.OrderByDescending(x => x.CreateDateTime);

            var rows = await query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
            var items = new List<LeaveCancellationResponse>();
            foreach (var row in rows) items.Add(await MapAsync(row, cancellationToken));

            return LeaveRequestServiceResult<LeaveLifecyclePagedResponse<LeaveCancellationResponse>>.Ok(new LeaveLifecyclePagedResponse<LeaveCancellationResponse>
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
                Items = items
            }, "Daftar leave cancellation berhasil diambil.");
        }

        private IQueryable<TrxLeaveCancellationRequest> BuildQuery()
        {
            return _dbContext.Set<TrxLeaveCancellationRequest>().AsNoTracking()
                .Include(x => x.LeaveRequest)!.ThenInclude(x => x!.LeaveType)
                .Include(x => x.WorkforceProfile)
                .Where(x => x.IsActive && !x.IsDelete);
        }

        private async Task<LeaveCancellationResponse> MapAsync(TrxLeaveCancellationRequest x, CancellationToken cancellationToken)
        {
            string? workflowStatus = null;
            if (x.WorkflowInstanceId.HasValue)
            {
                workflowStatus = await _dbContext.Set<TrxWorkflowInstance>().AsNoTracking()
                    .Where(w => w.Id == x.WorkflowInstanceId.Value && !w.IsDelete)
                    .Select(w => w.WorkflowStatus)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return new LeaveCancellationResponse
            {
                Id = x.Id,
                CancellationNumber = x.CancellationNumber,
                LeaveRequestId = x.LeaveRequestId,
                LeaveRequestNumber = x.LeaveRequest?.RequestNumber,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName,
                LeaveTypeName = x.LeaveRequest?.LeaveType?.LeaveTypeName,
                LeaveStartDate = x.LeaveRequest?.StartDate ?? default,
                LeaveEndDate = x.LeaveRequest?.EndDate ?? default,
                EffectiveCancellationDate = x.EffectiveCancellationDate,
                RestoredDays = x.RestoredDays,
                CancellationReason = x.CancellationReason,
                CancellationStatus = x.CancellationStatus,
                WorkflowInstanceId = x.WorkflowInstanceId,
                WorkflowStatus = workflowStatus,
                RequestedAt = x.RequestedAt,
                ApprovedAt = x.ApprovedAt,
                AppliedAt = x.AppliedAt,
                ApprovalNotes = x.ApprovalNotes,
                AvailableActions = ResolveActions(x.CancellationStatus, x.WorkflowInstanceId)
            };
        }

        private async Task<decimal> CalculateRestoredDaysAsync(WfpLeaveRequest leave, DateOnly effectiveDate, CancellationToken cancellationToken)
        {
            var integrationDays = await _dbContext.Set<TrxLeaveAttendanceIntegration>().AsNoTracking()
                .Where(x => x.LeaveRequestId == leave.Id && x.LeaveDate >= effectiveDate &&
                    x.IntegrationStatus != LeaveExecutionValueConstants.AttendanceIntegrationStatus.Reversed && !x.IsDelete)
                .SumAsync(x => (decimal?)x.RequestedLeaveDays, cancellationToken) ?? 0;

            if (integrationDays > 0) return integrationDays;
            if (effectiveDate <= leave.StartDate) return leave.ActualBalanceDeduction > 0 ? leave.ActualBalanceDeduction : leave.EstimatedBalanceDeduction;

            var totalCalendarDays = Math.Max(1, leave.EndDate.DayNumber - leave.StartDate.DayNumber + 1);
            var remainingCalendarDays = Math.Max(0, leave.EndDate.DayNumber - effectiveDate.DayNumber + 1);
            return Math.Round(leave.EstimatedBalanceDeduction * remainingCalendarDays / totalCalendarDays, 4);
        }

        private static LeaveLifecycleActionResponse MapAction(TrxLeaveCancellationRequest x, string? workflowStatus, bool idempotent, string message)
            => new()
            {
                Id = x.Id,
                LeaveRequestId = x.LeaveRequestId,
                ReferenceNumber = x.CancellationNumber,
                ReferenceStatus = x.CancellationStatus,
                WorkflowInstanceId = x.WorkflowInstanceId,
                WorkflowStatus = workflowStatus,
                IsIdempotent = idempotent,
                Message = message
            };

        private static List<string> ResolveActions(string status, Guid? workflowId)
        {
            var actions = new List<string> { "View" };
            if (status == LeaveLifecycleValueConstants.CancellationStatus.Draft || status == LeaveLifecycleValueConstants.CancellationStatus.NeedRevision)
            {
                actions.Add("Submit");
                actions.Add("Cancel");
            }
            if (workflowId.HasValue) actions.Add("ViewWorkflow");
            if (status == LeaveLifecycleValueConstants.CancellationStatus.Approved) actions.Add("Apply");
            return actions;
        }

        private static string GenerateNumber(string prefix) => $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
