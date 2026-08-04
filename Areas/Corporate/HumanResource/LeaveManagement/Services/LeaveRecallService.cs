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
    public class LeaveRecallService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveRequestCalculationService _calculationService;
        private readonly WorkflowService _workflowService;
        private readonly LeaveRecallWorkflowLifecycleService _lifecycleService;

        public LeaveRecallService(
            ApplicationDbContext dbContext,
            LeaveRequestCalculationService calculationService,
            WorkflowService workflowService,
            LeaveRecallWorkflowLifecycleService lifecycleService)
        {
            _dbContext = dbContext;
            _calculationService = calculationService;
            _workflowService = workflowService;
            _lifecycleService = lifecycleService;
        }

        public async Task<LeaveRequestServiceResult<LeaveLifecyclePagedResponse<LeaveRecallResponse>>> GetPagedAsync(
            LeaveLifecycleQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            request.PageNumber = Math.Max(1, request.PageNumber);
            request.PageSize = Math.Clamp(request.PageSize, 1, 100);
            var query = BuildQuery();

            if (request.WorkforceProfileId.HasValue) query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            if (request.StartDate.HasValue) query = query.Where(x => x.RecallEffectiveDate >= request.StartDate.Value);
            if (request.EndDate.HasValue) query = query.Where(x => x.RecallEffectiveDate <= request.EndDate.Value);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.RecallStatus == request.Status);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x => x.RecallNumber.ToLower().Contains(keyword) ||
                    x.RecallReason.ToLower().Contains(keyword) ||
                    (x.LeaveRequest != null && x.LeaveRequest.RequestNumber.ToLower().Contains(keyword)));
            }

            var total = await query.CountAsync(cancellationToken);
            query = request.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.CreateDateTime)
                : query.OrderByDescending(x => x.CreateDateTime);

            var rows = await query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
            var items = new List<LeaveRecallResponse>();
            foreach (var row in rows) items.Add(await MapAsync(row, cancellationToken));

            return LeaveRequestServiceResult<LeaveLifecyclePagedResponse<LeaveRecallResponse>>.Ok(new LeaveLifecyclePagedResponse<LeaveRecallResponse>
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
                Items = items
            }, "Daftar leave recall berhasil diambil.");
        }

        public async Task<LeaveRequestServiceResult<LeaveLifecyclePagedResponse<LeaveRecallResponse>>> GetMyReturnToWorkAsync(
            Guid actorUserId,
            LeaveLifecycleQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<LeaveLifecyclePagedResponse<LeaveRecallResponse>>.Fail(actor.StatusCode, actor.Message);

            request.WorkforceProfileId = actor.Data.WorkforceProfileId;
            return await GetPagedAsync(request, cancellationToken);
        }

        public async Task<LeaveRequestServiceResult<LeaveRecallResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await BuildQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity == null)
                return LeaveRequestServiceResult<LeaveRecallResponse>.Fail(StatusCodes.Status404NotFound, "Leave recall tidak ditemukan.");
            return LeaveRequestServiceResult<LeaveRecallResponse>.Ok(await MapAsync(entity, cancellationToken), "Detail leave recall berhasil diambil.");
        }

        public async Task<LeaveRequestServiceResult<LeaveLifecycleActionResponse>> CreateAsync(
            Guid actorUserId,
            CreateLeaveRecallRequest request,
            CancellationToken cancellationToken = default)
        {
            var leave = await _dbContext.Set<WfpLeaveRequest>()
                .Include(x => x.LeaveType)
                .FirstOrDefaultAsync(x => x.Id == request.LeaveRequestId && !x.IsDelete, cancellationToken);

            if (leave == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status404NotFound, "Leave request tidak ditemukan.");

            if (leave.LeaveRequestStatus != LeaveRequestValueConstants.Status.Approved &&
                leave.LeaveRequestStatus != LeaveRequestValueConstants.Status.Taken)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status409Conflict, "Recall hanya dapat dibuat untuk leave Approved atau Taken.");

            if (request.RecallEffectiveDate < leave.StartDate || request.RecallEffectiveDate > leave.EndDate)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status400BadRequest, "RecallEffectiveDate harus berada dalam periode leave.");

            var activeExists = await _dbContext.Set<TrxLeaveRecall>().AnyAsync(x => x.LeaveRequestId == leave.Id &&
                x.RecallStatus != LeaveLifecycleValueConstants.RecallStatus.Rejected &&
                x.RecallStatus != LeaveLifecycleValueConstants.RecallStatus.Cancelled &&
                x.RecallStatus != LeaveLifecycleValueConstants.RecallStatus.Applied && !x.IsDelete, cancellationToken);

            if (activeExists)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status409Conflict, "Masih terdapat recall aktif untuk leave tersebut.");

            var recalledDays = await CalculateRecalledDaysAsync(leave, request.RecallEffectiveDate, cancellationToken);
            var now = DateTime.UtcNow;
            var entity = new TrxLeaveRecall
            {
                Id = Guid.NewGuid(),
                RecallNumber = GenerateNumber("LRC"),
                LeaveRequestId = leave.Id,
                WorkforceProfileId = leave.WorkforceProfileId,
                ReplacementWorkforceProfileId = request.ReplacementWorkforceProfileId,
                OriginalLeaveEndDate = leave.EndDate,
                RecallEffectiveDate = request.RecallEffectiveDate,
                RecalledLeaveDays = recalledDays,
                RestoredBalanceDays = recalledDays,
                RecallReason = request.RecallReason.Trim(),
                RecallStatus = LeaveLifecycleValueConstants.RecallStatus.Draft,
                InitiatedByUserId = actorUserId,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Ok(
                MapAction(entity, null, false, "Draft leave recall berhasil dibuat."),
                "Draft leave recall berhasil dibuat.",
                StatusCodes.Status201Created);
        }

        public async Task<LeaveRequestServiceResult<LeaveLifecycleActionResponse>> PrepareWorkflowAsync(
            Guid id,
            Guid actorUserId,
            PrepareLeaveLifecycleWorkflowRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxLeaveRecall>()
                .Include(x => x.LeaveRequest)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity?.LeaveRequest == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status404NotFound, "Leave recall tidak ditemukan.");

            if (entity.WorkflowInstanceId.HasValue)
            {
                var existing = await _dbContext.Set<TrxWorkflowInstance>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == entity.WorkflowInstanceId.Value && !x.IsDelete, cancellationToken);
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Ok(
                    MapAction(entity, existing?.WorkflowStatus, true, "Workflow recall sudah tersedia."),
                    "Workflow recall sudah tersedia.");
            }

            var context = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                recallId = entity.Id,
                entity.RecallNumber,
                entity.LeaveRequestId,
                entity.WorkforceProfileId,
                entity.RecallEffectiveDate,
                entity.RecalledLeaveDays,
                entity.RecallReason
            })).RootElement.Clone();

            var result = await _workflowService.CreateAsync(new CreateWorkflowInstanceRequest
            {
                WorkflowDefinitionCode = "LEAVE_RECALL",
                ReferenceType = LeaveLifecycleValueConstants.ReferenceType.LeaveRecall,
                ReferenceId = entity.Id,
                ExternalReferenceNumber = entity.RecallNumber,
                SourceChannel = string.IsNullOrWhiteSpace(request.SourceChannel) ? "Web" : request.SourceChannel.Trim(),
                RequestCorrelationId = NullIfWhiteSpace(request.CorrelationId),
                IdempotencyKey = NullIfWhiteSpace(request.IdempotencyKey) ?? $"LEAVE-RECALL-WF:{entity.Id:N}",
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
                MapAction(entity, result.Data.WorkflowStatus, false, "Workflow recall berhasil disiapkan."),
                "Workflow recall berhasil disiapkan.");
        }

        public async Task<LeaveRequestServiceResult<LeaveLifecycleActionResponse>> SubmitAsync(
            Guid id,
            Guid actorUserId,
            SubmitLeaveLifecycleWorkflowRequest request,
            CancellationToken cancellationToken = default)
        {
            var prepare = await PrepareWorkflowAsync(id, actorUserId, request, cancellationToken);
            if (!prepare.Success) return prepare;

            var entity = await _dbContext.Set<TrxLeaveRecall>().FirstAsync(x => x.Id == id, cancellationToken);
            if (!entity.WorkflowInstanceId.HasValue)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status500InternalServerError, "Workflow recall belum tersedia.");

            var result = await _workflowService.SubmitAsync(entity.WorkflowInstanceId.Value, new WorkflowSubmitRequest
            {
                Comment = request.Comment,
                IdempotencyKey = NullIfWhiteSpace(request.IdempotencyKey) ?? $"LEAVE-RECALL-SUBMIT:{entity.Id:N}"
            }, cancellationToken);

            if (!result.Success || result.Data == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(result.StatusCode, result.Message);

            entity.RecallStatus = LeaveLifecycleValueConstants.RecallStatus.Submitted;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Ok(
                MapAction(entity, result.Data.WorkflowStatus, false, "Leave recall berhasil disubmit."),
                "Leave recall berhasil disubmit.");
        }

        public async Task<LeaveRequestServiceResult<LeaveLifecycleActionResponse>> AcknowledgeReturnToWorkAsync(
            Guid recallId,
            Guid actorUserId,
            AcknowledgeReturnToWorkRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(actor.StatusCode, actor.Message);

            var entity = await _dbContext.Set<TrxLeaveRecall>()
                .Include(x => x.LeaveRequest)
                .FirstOrDefaultAsync(x => x.Id == recallId && x.WorkforceProfileId == actor.Data.WorkforceProfileId && !x.IsDelete, cancellationToken);

            if (entity?.LeaveRequest == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status404NotFound, "Leave recall tidak ditemukan atau bukan milik employee login.");

            if (request.ActualReturnToWorkDate < entity.RecallEffectiveDate ||
                request.ActualReturnToWorkDate > entity.OriginalLeaveEndDate)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status400BadRequest, "ActualReturnToWorkDate harus berada antara recall effective date dan original leave end date.");

            entity.ActualReturnToWorkDate = request.ActualReturnToWorkDate;
            entity.AcknowledgedByUserId = actorUserId;
            entity.AcknowledgedAt = DateTime.UtcNow;
            entity.Notes = AppendNote(entity.Notes, request.Notes);

            if (entity.RecallStatus == LeaveLifecycleValueConstants.RecallStatus.Submitted ||
                entity.RecallStatus == LeaveLifecycleValueConstants.RecallStatus.WaitingApproval)
                entity.RecallStatus = LeaveLifecycleValueConstants.RecallStatus.Acknowledged;

            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Ok(
                MapAction(entity, null, false, "Return-to-work berhasil dikonfirmasi."),
                "Return-to-work berhasil dikonfirmasi.");
        }

        public async Task<LeaveRequestServiceResult<LeaveLifecycleActionResponse>> SynchronizeAsync(
            Guid id,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxLeaveRecall>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity?.WorkflowInstanceId == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status404NotFound, "Workflow recall belum tersedia.");

            var workflow = await _dbContext.Set<TrxWorkflowInstance>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == entity.WorkflowInstanceId.Value && !x.IsDelete, cancellationToken);
            if (workflow == null)
                return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Fail(StatusCodes.Status404NotFound, "Workflow recall tidak ditemukan.");

            var sync = await _lifecycleService.SynchronizeAsync(workflow, actorUserId, allowAutoApply, cancellationToken);
            var refreshed = await _dbContext.Set<TrxLeaveRecall>().AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);

            return LeaveRequestServiceResult<LeaveLifecycleActionResponse>.Ok(
                MapAction(refreshed, workflow.WorkflowStatus, false, sync.WarningMessage ?? "Leave recall berhasil disinkronkan."),
                sync.WarningMessage ?? "Leave recall berhasil disinkronkan.");
        }

        private IQueryable<TrxLeaveRecall> BuildQuery()
        {
            return _dbContext.Set<TrxLeaveRecall>().AsNoTracking()
                .Include(x => x.LeaveRequest)!.ThenInclude(x => x!.LeaveType)
                .Include(x => x.WorkforceProfile)
                .Where(x => x.IsActive && !x.IsDelete);
        }

        private async Task<LeaveRecallResponse> MapAsync(TrxLeaveRecall x, CancellationToken cancellationToken)
        {
            string? workflowStatus = null;
            if (x.WorkflowInstanceId.HasValue)
            {
                workflowStatus = await _dbContext.Set<TrxWorkflowInstance>().AsNoTracking()
                    .Where(w => w.Id == x.WorkflowInstanceId.Value && !w.IsDelete)
                    .Select(w => w.WorkflowStatus)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return new LeaveRecallResponse
            {
                Id = x.Id,
                RecallNumber = x.RecallNumber,
                LeaveRequestId = x.LeaveRequestId,
                LeaveRequestNumber = x.LeaveRequest?.RequestNumber,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName,
                LeaveTypeName = x.LeaveRequest?.LeaveType?.LeaveTypeName,
                OriginalLeaveEndDate = x.OriginalLeaveEndDate,
                RecallEffectiveDate = x.RecallEffectiveDate,
                ActualReturnToWorkDate = x.ActualReturnToWorkDate,
                RecalledLeaveDays = x.RecalledLeaveDays,
                RestoredBalanceDays = x.RestoredBalanceDays,
                RecallReason = x.RecallReason,
                RecallStatus = x.RecallStatus,
                WorkflowInstanceId = x.WorkflowInstanceId,
                WorkflowStatus = workflowStatus,
                AcknowledgedAt = x.AcknowledgedAt,
                ApprovedAt = x.ApprovedAt,
                AppliedAt = x.AppliedAt,
                Notes = x.Notes,
                AvailableActions = ResolveActions(x)
            };
        }

        private async Task<decimal> CalculateRecalledDaysAsync(WfpLeaveRequest leave, DateOnly effectiveDate, CancellationToken cancellationToken)
        {
            var integrationDays = await _dbContext.Set<TrxLeaveAttendanceIntegration>().AsNoTracking()
                .Where(x => x.LeaveRequestId == leave.Id && x.LeaveDate >= effectiveDate &&
                    x.IntegrationStatus != LeaveExecutionValueConstants.AttendanceIntegrationStatus.Reversed && !x.IsDelete)
                .SumAsync(x => (decimal?)x.RequestedLeaveDays, cancellationToken) ?? 0;

            if (integrationDays > 0) return integrationDays;
            var totalCalendarDays = Math.Max(1, leave.EndDate.DayNumber - leave.StartDate.DayNumber + 1);
            var remainingCalendarDays = Math.Max(0, leave.EndDate.DayNumber - effectiveDate.DayNumber + 1);
            return Math.Round(leave.EstimatedBalanceDeduction * remainingCalendarDays / totalCalendarDays, 4);
        }

        private static LeaveLifecycleActionResponse MapAction(TrxLeaveRecall x, string? workflowStatus, bool idempotent, string message)
            => new()
            {
                Id = x.Id,
                LeaveRequestId = x.LeaveRequestId,
                ReferenceNumber = x.RecallNumber,
                ReferenceStatus = x.RecallStatus,
                WorkflowInstanceId = x.WorkflowInstanceId,
                WorkflowStatus = workflowStatus,
                IsIdempotent = idempotent,
                Message = message
            };

        private static List<string> ResolveActions(TrxLeaveRecall x)
        {
            var actions = new List<string> { "View" };
            if (x.RecallStatus == LeaveLifecycleValueConstants.RecallStatus.Draft || x.RecallStatus == LeaveLifecycleValueConstants.RecallStatus.NeedRevision)
                actions.Add("Submit");
            if (!x.ActualReturnToWorkDate.HasValue &&
                x.RecallStatus != LeaveLifecycleValueConstants.RecallStatus.Rejected &&
                x.RecallStatus != LeaveLifecycleValueConstants.RecallStatus.Cancelled)
                actions.Add("AcknowledgeReturnToWork");
            if (x.WorkflowInstanceId.HasValue) actions.Add("ViewWorkflow");
            if (x.RecallStatus == LeaveLifecycleValueConstants.RecallStatus.Approved) actions.Add("Apply");
            return actions;
        }

        private static string GenerateNumber(string prefix) => $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static string? AppendNote(string? existing, string? value) => string.IsNullOrWhiteSpace(value) ? existing : string.IsNullOrWhiteSpace(existing) ? value.Trim() : $"{existing}\n[{DateTime.UtcNow:O}] {value.Trim()}";
    }
}
