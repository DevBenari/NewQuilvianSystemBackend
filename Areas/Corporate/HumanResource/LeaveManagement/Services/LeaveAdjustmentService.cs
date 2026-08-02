using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveAdjustmentService
    {
        private static readonly string[] WorkflowReferenceAliases =
        {
            LeaveValueConstants.WorkflowReferenceType.LeaveAdjustment,
            "LeaveAdjustment",
            "TrxLeaveAdjustment",
            "LEAVE_BALANCE_ADJUSTMENT"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly WorkflowService _workflowService;
        private readonly WorkflowReferenceLifecycleService _workflowLifecycleService;
        private readonly LeaveAdjustmentPostingService _postingService;

        public LeaveAdjustmentService(
            ApplicationDbContext dbContext,
            WorkflowService workflowService,
            WorkflowReferenceLifecycleService workflowLifecycleService,
            LeaveAdjustmentPostingService postingService)
        {
            _dbContext = dbContext;
            _workflowService = workflowService;
            _workflowLifecycleService = workflowLifecycleService;
            _postingService = postingService;
        }

        public LeaveAdjustmentFilterMetadataResponse GetFilterMetadata()
        {
            return new LeaveAdjustmentFilterMetadataResponse
            {
                DefaultFilter = new LeaveAdjustmentDefaultFilterResponse(),
                AdjustmentTypes = new()
                {
                    Option(LeaveValueConstants.AdjustmentType.OpeningBalance, "Opening balance"),
                    Option(LeaveValueConstants.AdjustmentType.ManualAdjustment, "Manual adjustment"),
                    Option(LeaveValueConstants.AdjustmentType.Correction, "Koreksi saldo"),
                    Option(LeaveValueConstants.AdjustmentType.Reversal, "Reversal")
                },
                Directions = new()
                {
                    Option(LeaveValueConstants.TransactionDirection.Credit, "Penambahan / credit"),
                    Option(LeaveValueConstants.TransactionDirection.Debit, "Pengurangan / debit")
                },
                AdjustmentStatuses = new()
                {
                    Option(LeaveValueConstants.AdjustmentStatus.Draft, "Draft"),
                    Option(LeaveValueConstants.AdjustmentStatus.Submitted, "Submitted"),
                    Option(LeaveValueConstants.AdjustmentStatus.UnderReview, "Dalam review"),
                    Option(LeaveValueConstants.AdjustmentStatus.NeedRevision, "Perlu revisi"),
                    Option(LeaveValueConstants.AdjustmentStatus.Approved, "Disetujui, belum diposting"),
                    Option(LeaveValueConstants.AdjustmentStatus.Posted, "Posted"),
                    Option(LeaveValueConstants.AdjustmentStatus.Rejected, "Ditolak"),
                    Option(LeaveValueConstants.AdjustmentStatus.Cancelled, "Dibatalkan"),
                    Option(LeaveValueConstants.AdjustmentStatus.Reversed, "Direversal")
                },
                CustomPeriods = new()
                {
                    Option("today", "Hari ini"),
                    Option("last7days", "7 hari terakhir"),
                    Option("thismonth", "Bulan ini"),
                    Option("lastmonth", "Bulan lalu"),
                    Option("thisyear", "Tahun ini")
                },
                SortOptions = new()
                {
                    Option("requestedAt", "Tanggal pengajuan"),
                    Option("adjustmentNumber", "Nomor adjustment"),
                    Option("workforceDisplayName", "Nama workforce"),
                    Option("leaveTypeName", "Jenis cuti"),
                    Option("requestedDays", "Jumlah hari"),
                    Option("effectiveDate", "Tanggal efektif"),
                    Option("adjustmentStatus", "Status")
                }
            };
        }

        public async Task<List<LeaveAdjustmentReasonOptionResponse>> GetReasonOptionsAsync(
            Guid? leaveTypeId,
            string? adjustmentType,
            string? direction,
            bool onlyActive,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var today = DateTime.UtcNow.Date;
            var query = _dbContext.Set<MstLeaveAdjustmentReason>()
                .AsNoTracking()
                .Include(x => x.LeaveType)
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x =>
                    x.IsActive &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today));
            }

            if (leaveTypeId.HasValue && leaveTypeId.Value != Guid.Empty)
            {
                query = query.Where(x =>
                    !x.LeaveTypeId.HasValue || x.LeaveTypeId == leaveTypeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(adjustmentType))
            {
                var type = adjustmentType.Trim();
                if (type.Equals(LeaveValueConstants.AdjustmentType.OpeningBalance, StringComparison.OrdinalIgnoreCase))
                    query = query.Where(x => x.AllowOpeningBalance);
                else if (type.Equals(LeaveValueConstants.AdjustmentType.ManualAdjustment, StringComparison.OrdinalIgnoreCase))
                    query = query.Where(x => x.AllowManualAdjustment);
                else if (type.Equals(LeaveValueConstants.AdjustmentType.Correction, StringComparison.OrdinalIgnoreCase))
                    query = query.Where(x => x.AllowCorrection);
                else if (type.Equals(LeaveValueConstants.AdjustmentType.Reversal, StringComparison.OrdinalIgnoreCase))
                    query = query.Where(x => x.AllowReversal);
            }

            if (!string.IsNullOrWhiteSpace(direction))
            {
                var normalizedDirection = direction.Trim();
                query = query.Where(x =>
                    x.AllowedDirection == LeaveValueConstants.AdjustmentAllowedDirection.Both ||
                    x.AllowedDirection == normalizedDirection);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ReasonCode.ToLower().Contains(keyword) ||
                    x.ReasonName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ReasonName)
                .Select(x => new LeaveAdjustmentReasonOptionResponse
                {
                    Id = x.Id,
                    LeaveTypeId = x.LeaveTypeId,
                    LeaveTypeCode = x.LeaveType != null ? x.LeaveType.LeaveTypeCode : null,
                    LeaveTypeName = x.LeaveType != null ? x.LeaveType.LeaveTypeName : null,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    ReasonCategory = x.ReasonCategory,
                    AllowedDirection = x.AllowedDirection,
                    AllowOpeningBalance = x.AllowOpeningBalance,
                    AllowManualAdjustment = x.AllowManualAdjustment,
                    AllowCorrection = x.AllowCorrection,
                    AllowReversal = x.AllowReversal,
                    MaximumAdjustmentDays = x.MaximumAdjustmentDays,
                    RequiresComment = x.RequiresComment,
                    RequiresAttachment = x.RequiresAttachment,
                    RequiresApproval = x.RequiresApproval,
                    ApprovalWorkflowCode = x.ApprovalWorkflowCode
                })
                .Take(100)
                .ToListAsync(cancellationToken);
        }

        public async Task<LeaveAdjustmentSummaryResponse> GetSummaryAsync(
            LeaveAdjustmentQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilters(BuildQuery(), request);
            return new LeaveAdjustmentSummaryResponse
            {
                TotalAdjustment = await query.CountAsync(cancellationToken),
                Draft = await query.CountAsync(x => x.AdjustmentStatus == LeaveValueConstants.AdjustmentStatus.Draft, cancellationToken),
                WaitingApproval = await query.CountAsync(x =>
                    x.AdjustmentStatus == LeaveValueConstants.AdjustmentStatus.Submitted ||
                    x.AdjustmentStatus == LeaveValueConstants.AdjustmentStatus.UnderReview,
                    cancellationToken),
                NeedRevision = await query.CountAsync(x => x.AdjustmentStatus == LeaveValueConstants.AdjustmentStatus.NeedRevision, cancellationToken),
                ApprovedPendingPost = await query.CountAsync(x => x.AdjustmentStatus == LeaveValueConstants.AdjustmentStatus.Approved, cancellationToken),
                Posted = await query.CountAsync(x => x.AdjustmentStatus == LeaveValueConstants.AdjustmentStatus.Posted, cancellationToken),
                Rejected = await query.CountAsync(x => x.AdjustmentStatus == LeaveValueConstants.AdjustmentStatus.Rejected, cancellationToken),
                Cancelled = await query.CountAsync(x => x.AdjustmentStatus == LeaveValueConstants.AdjustmentStatus.Cancelled, cancellationToken),
                Reversed = await query.CountAsync(x => x.AdjustmentStatus == LeaveValueConstants.AdjustmentStatus.Reversed, cancellationToken),
                OpeningBalance = await query.CountAsync(x => x.AdjustmentType == LeaveValueConstants.AdjustmentType.OpeningBalance, cancellationToken),
                ManualAdjustment = await query.CountAsync(x => x.AdjustmentType == LeaveValueConstants.AdjustmentType.ManualAdjustment, cancellationToken),
                CreditCount = await query.CountAsync(x => x.Direction == LeaveValueConstants.TransactionDirection.Credit, cancellationToken),
                DebitCount = await query.CountAsync(x => x.Direction == LeaveValueConstants.TransactionDirection.Debit, cancellationToken),
                TotalRequestedCreditDays = await query.Where(x => x.Direction == LeaveValueConstants.TransactionDirection.Credit).SumAsync(x => x.RequestedDays, cancellationToken),
                TotalRequestedDebitDays = await query.Where(x => x.Direction == LeaveValueConstants.TransactionDirection.Debit).SumAsync(x => x.RequestedDays, cancellationToken),
                TotalPostedCreditDays = await query.Where(x => x.Direction == LeaveValueConstants.TransactionDirection.Credit).SumAsync(x => x.PostedDays, cancellationToken),
                TotalPostedDebitDays = await query.Where(x => x.Direction == LeaveValueConstants.TransactionDirection.Debit).SumAsync(x => x.PostedDays, cancellationToken)
            };
        }

        public async Task<LeaveAdjustmentPagedResponse> GetPagedAsync(
            LeaveAdjustmentQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            NormalizePaging(request);
            var query = ApplyFilters(BuildQuery(), request);
            var total = await query.CountAsync(cancellationToken);
            var entities = await ApplySorting(query, request.SortBy, request.SortDirection)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var items = new List<LeaveAdjustmentResponse>();
            foreach (var entity in entities)
                items.Add(await MapResponseAsync(entity, cancellationToken));

            return new LeaveAdjustmentPagedResponse
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
                Items = items
            };
        }

        public async Task<LeaveAdjustmentServiceResult<LeaveAdjustmentDetailResponse>> GetByIdAsync(
            Guid id,
            bool includeWorkflow = true,
            CancellationToken cancellationToken = default)
        {
            var entity = await BuildQuery()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity == null)
            {
                return LeaveAdjustmentServiceResult<LeaveAdjustmentDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave adjustment tidak ditemukan.");
            }

            var detail = await MapDetailAsync(entity, includeWorkflow, cancellationToken);
            return LeaveAdjustmentServiceResult<LeaveAdjustmentDetailResponse>.Ok(
                detail,
                "Detail leave adjustment berhasil diambil.");
        }

        public async Task<LeaveAdjustmentServiceResult<WorkflowInstanceDetailResponse>> GetWorkflowAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var adjustment = await _dbContext.Set<TrxLeaveAdjustment>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (adjustment == null)
                return LeaveAdjustmentServiceResult<WorkflowInstanceDetailResponse>.Fail(StatusCodes.Status404NotFound, "Leave adjustment tidak ditemukan.");

            var workflow = await FindWorkflowAsync(adjustment, cancellationToken);
            if (workflow == null)
                return LeaveAdjustmentServiceResult<WorkflowInstanceDetailResponse>.Fail(StatusCodes.Status404NotFound, "Workflow leave adjustment belum tersedia.");

            var result = await _workflowService.GetByIdAsync(workflow.Id, cancellationToken);
            return result.Success && result.Data != null
                ? LeaveAdjustmentServiceResult<WorkflowInstanceDetailResponse>.Ok(result.Data, result.Message)
                : LeaveAdjustmentServiceResult<WorkflowInstanceDetailResponse>.Fail(result.StatusCode, result.Message);
        }

        public async Task<LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>> CreateAsync(
            CreateLeaveAdjustmentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return FailAction(StatusCodes.Status401Unauthorized, "User login tidak valid.");

            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var existing = await _dbContext.Set<TrxLeaveAdjustment>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => !x.IsDelete && x.IdempotencyKey == request.IdempotencyKey.Trim(), cancellationToken);
                if (existing != null)
                {
                    var existingDetail = await GetByIdAsync(existing.Id, true, cancellationToken);
                    return existingDetail.Success
                        ? LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>.Ok(
                            new LeaveAdjustmentActionResponse { Adjustment = existingDetail.Data! },
                            "Leave adjustment dengan idempotency key yang sama sudah tersedia.")
                        : FailAction(existingDetail.StatusCode, existingDetail.Message);
                }
            }

            var context = await LoadValidationContextAsync(
                request.LeaveBalanceId,
                request.LeaveAdjustmentReasonId,
                cancellationToken);
            if (!context.Success)
                return FailAction(context.StatusCode, context.Message);

            var validation = await ValidateRequestAsync(
                null,
                context.Balance!,
                context.Reason!,
                request.AdjustmentType,
                request.Direction,
                request.RequestedDays,
                request.EffectiveDate,
                request.Reason,
                cancellationToken);
            if (validation != null)
                return FailAction(StatusCodes.Status400BadRequest, validation);

            var now = DateTime.UtcNow;
            var entity = new TrxLeaveAdjustment
            {
                Id = Guid.NewGuid(),
                AdjustmentNumber = GenerateAdjustmentNumber(),
                WorkforceProfileId = context.Balance!.WorkforceProfileId,
                LeaveBalanceId = context.Balance.Id,
                LeaveTypeId = context.Balance.LeaveTypeId,
                LeaveEntitlementPeriodId = context.Balance.LeaveEntitlementPeriodId!.Value,
                LeaveAdjustmentReasonId = context.Reason!.Id,
                AdjustmentType = NormalizeAdjustmentType(request.AdjustmentType),
                Direction = NormalizeDirection(request.Direction),
                RequestedDays = request.RequestedDays,
                EffectiveDate = request.EffectiveDate,
                AdjustmentStatus = LeaveValueConstants.AdjustmentStatus.Draft,
                IdempotencyKey = NormalizeText(request.IdempotencyKey),
                Reason = request.Reason.Trim(),
                RequestNote = NormalizeText(request.RequestNote),
                SourceType = NormalizeText(request.SourceType) ?? LeaveValueConstants.AdjustmentSourceType.HrManual,
                SourceReferenceId = request.SourceReferenceId,
                SourceReferenceNumber = NormalizeText(request.SourceReferenceNumber),
                RequestedAt = now,
                RequestedByUserId = actorUserId,
                RequestSnapshotJson = JsonSerializer.Serialize(new
                {
                    request.LeaveBalanceId,
                    request.LeaveAdjustmentReasonId,
                    adjustmentType = NormalizeAdjustmentType(request.AdjustmentType),
                    direction = NormalizeDirection(request.Direction),
                    request.RequestedDays,
                    request.EffectiveDate,
                    reason = request.Reason.Trim(),
                    sourceType = NormalizeText(request.SourceType) ?? LeaveValueConstants.AdjustmentSourceType.HrManual,
                    createdAt = now,
                    createdByUserId = actorUserId
                }),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxLeaveAdjustment>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                return FailAction(StatusCodes.Status409Conflict, $"Leave adjustment gagal dibuat: {ex.InnerException?.Message ?? ex.Message}");
            }

            string? warning = null;
            if (RequiresWorkflow(context.Reason))
            {
                var prepareResult = await EnsureWorkflowAsync(
                    entity,
                    context.Reason,
                    request.SourceChannel,
                    request.RequestCorrelationId,
                    request.SelectedApproverUserIds,
                    cancellationToken);
                if (!prepareResult.Success)
                    warning = $"Draft berhasil dibuat, tetapi workflow belum berhasil disiapkan: {prepareResult.Message}";
            }

            var detailResult = await GetByIdAsync(entity.Id, true, cancellationToken);
            if (!detailResult.Success)
                return FailAction(detailResult.StatusCode, detailResult.Message);

            return LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>.Ok(
                new LeaveAdjustmentActionResponse
                {
                    Adjustment = detailResult.Data!,
                    WarningMessage = warning
                },
                warning == null
                    ? "Draft leave adjustment berhasil dibuat."
                    : "Draft leave adjustment berhasil dibuat dengan peringatan workflow.",
                StatusCodes.Status201Created);
        }

        public async Task<LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>> UpdateAsync(
            Guid id,
            UpdateLeaveAdjustmentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxLeaveAdjustment>()
                .Include(x => x.LeaveBalance)
                    .ThenInclude(x => x!.LeaveEntitlementPeriod)
                .Include(x => x.LeaveBalance)
                    .ThenInclude(x => x!.LeavePolicy)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return FailAction(StatusCodes.Status404NotFound, "Leave adjustment tidak ditemukan.");

            if (!CanEdit(entity.AdjustmentStatus))
                return FailAction(StatusCodes.Status409Conflict, "Leave adjustment hanya dapat diubah pada status Draft atau NeedRevision.");

            if (entity.WorkflowInstanceId.HasValue &&
                entity.LeaveAdjustmentReasonId != request.LeaveAdjustmentReasonId)
            {
                return FailAction(
                    StatusCodes.Status409Conflict,
                    "Adjustment reason tidak dapat diganti setelah workflow disiapkan. Batalkan draft dan buat pengajuan baru.");
            }

            var reason = await LoadReasonAsync(request.LeaveAdjustmentReasonId, entity.LeaveTypeId, cancellationToken);
            if (reason == null)
                return FailAction(StatusCodes.Status400BadRequest, "Adjustment reason tidak ditemukan, tidak aktif, atau tidak berlaku untuk leave type tersebut.");

            var validation = await ValidateRequestAsync(
                entity.Id,
                entity.LeaveBalance!,
                reason,
                request.AdjustmentType,
                request.Direction,
                request.RequestedDays,
                request.EffectiveDate,
                request.Reason,
                cancellationToken);
            if (validation != null)
                return FailAction(StatusCodes.Status400BadRequest, validation);

            var now = DateTime.UtcNow;
            entity.LeaveAdjustmentReasonId = reason.Id;
            entity.AdjustmentType = NormalizeAdjustmentType(request.AdjustmentType);
            entity.Direction = NormalizeDirection(request.Direction);
            entity.RequestedDays = request.RequestedDays;
            entity.ApprovedDays = null;
            entity.EffectiveDate = request.EffectiveDate;
            entity.Reason = request.Reason.Trim();
            entity.RequestNote = NormalizeText(request.RequestNote);
            entity.AdjustmentStatus = LeaveValueConstants.AdjustmentStatus.Draft;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var detail = await GetByIdAsync(entity.Id, true, cancellationToken);
            return detail.Success
                ? LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>.Ok(
                    new LeaveAdjustmentActionResponse { Adjustment = detail.Data! },
                    "Leave adjustment berhasil diperbarui.")
                : FailAction(detail.StatusCode, detail.Message);
        }

        public async Task<LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>> PrepareWorkflowAsync(
            Guid id,
            PrepareLeaveAdjustmentWorkflowRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxLeaveAdjustment>()
                .Include(x => x.LeaveAdjustmentReason)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return FailAction(StatusCodes.Status404NotFound, "Leave adjustment tidak ditemukan.");
            if (!CanEdit(entity.AdjustmentStatus))
                return FailAction(StatusCodes.Status409Conflict, "Workflow hanya dapat disiapkan pada status Draft atau NeedRevision.");
            if (entity.LeaveAdjustmentReason == null)
                return FailAction(StatusCodes.Status409Conflict, "Adjustment reason tidak tersedia.");

            var workflowResult = await EnsureWorkflowAsync(
                entity,
                entity.LeaveAdjustmentReason,
                request.SourceChannel,
                request.RequestCorrelationId,
                request.SelectedApproverUserIds,
                cancellationToken);
            if (!workflowResult.Success)
                return FailAction(workflowResult.StatusCode, workflowResult.Message);

            var detail = await GetByIdAsync(entity.Id, true, cancellationToken);
            return detail.Success
                ? LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>.Ok(
                    new LeaveAdjustmentActionResponse { Adjustment = detail.Data! },
                    "Workflow leave adjustment berhasil disiapkan.")
                : FailAction(detail.StatusCode, detail.Message);
        }

        public async Task<LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>> SubmitAsync(
            Guid id,
            SubmitLeaveAdjustmentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxLeaveAdjustment>()
                .Include(x => x.LeaveAdjustmentReason)
                .Include(x => x.LeaveBalance)
                    .ThenInclude(x => x!.LeaveEntitlementPeriod)
                .Include(x => x.LeaveBalance)
                    .ThenInclude(x => x!.LeavePolicy)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return FailAction(StatusCodes.Status404NotFound, "Leave adjustment tidak ditemukan.");
            if (!CanEdit(entity.AdjustmentStatus))
                return FailAction(StatusCodes.Status409Conflict, "Leave adjustment hanya dapat di-submit dari status Draft atau NeedRevision.");
            if (entity.LeaveAdjustmentReason == null || entity.LeaveBalance == null)
                return FailAction(StatusCodes.Status409Conflict, "Data reason atau balance leave adjustment tidak lengkap.");

            var validation = await ValidateRequestAsync(
                entity.Id,
                entity.LeaveBalance,
                entity.LeaveAdjustmentReason,
                entity.AdjustmentType,
                entity.Direction,
                entity.RequestedDays,
                entity.EffectiveDate,
                entity.Reason,
                cancellationToken);
            if (validation != null)
                return FailAction(StatusCodes.Status400BadRequest, validation);

            if (RequiresWorkflow(entity.LeaveAdjustmentReason) || entity.WorkflowInstanceId.HasValue)
            {
                var workflowResult = await EnsureWorkflowAsync(
                    entity,
                    entity.LeaveAdjustmentReason,
                    request.SourceChannel,
                    request.RequestCorrelationId,
                    request.SelectedApproverUserIds,
                    cancellationToken);
                if (!workflowResult.Success || workflowResult.Data == null)
                    return FailAction(workflowResult.StatusCode, workflowResult.Message);

                if (entity.LeaveAdjustmentReason.RequiresAttachment)
                {
                    var attachmentExists = await _dbContext.Set<TrxWorkflowAttachment>()
                        .AsNoTracking()
                        .AnyAsync(x =>
                            !x.IsDelete &&
                            x.IsActive &&
                            x.WorkflowInstanceId == workflowResult.Data.Id,
                            cancellationToken);
                    if (!attachmentExists)
                        return FailAction(StatusCodes.Status400BadRequest, "Adjustment reason mewajibkan minimal satu attachment pada workflow.");
                }

                entity.SubmittedAt = DateTime.UtcNow;
                entity.SubmittedByUserId = actorUserId;
                entity.AdjustmentStatus = LeaveValueConstants.AdjustmentStatus.Submitted;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                var submitResult = await _workflowService.SubmitAsync(
                    workflowResult.Data.Id,
                    new WorkflowSubmitRequest
                    {
                        Comment = NormalizeText(request.Note),
                        IdempotencyKey = NormalizeText(request.IdempotencyKey)
                    },
                    cancellationToken);
                if (!submitResult.Success)
                    return FailAction(submitResult.StatusCode, submitResult.Message);

                await _workflowLifecycleService.SynchronizeAsync(
                    workflowResult.Data.Id,
                    actorUserId,
                    allowAutoApply: true,
                    cancellationToken: cancellationToken);

                var detail = await GetByIdAsync(entity.Id, true, cancellationToken);
                return detail.Success
                    ? LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>.Ok(
                        new LeaveAdjustmentActionResponse { Adjustment = detail.Data! },
                        "Leave adjustment berhasil di-submit ke workflow.")
                    : FailAction(detail.StatusCode, detail.Message);
            }

            entity.SubmittedAt = DateTime.UtcNow;
            entity.SubmittedByUserId = actorUserId;
            entity.ApprovedDays = entity.RequestedDays;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.ApprovedByUserId = actorUserId;
            entity.ApprovalNote = NormalizeText(request.Note) ?? "Direct approval karena adjustment reason tidak memerlukan workflow.";
            entity.AdjustmentStatus = LeaveValueConstants.AdjustmentStatus.Approved;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var postResult = await _postingService.PostAsync(
                entity.Id,
                actorUserId,
                request.Note,
                request.IdempotencyKey,
                cancellationToken);
            if (!postResult.Success)
                return FailAction(postResult.StatusCode, postResult.Message);

            var postedDetail = await GetByIdAsync(entity.Id, true, cancellationToken);
            return postedDetail.Success
                ? LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>.Ok(
                    new LeaveAdjustmentActionResponse { Adjustment = postedDetail.Data! },
                    "Leave adjustment disetujui langsung dan berhasil diposting.")
                : FailAction(postedDetail.StatusCode, postedDetail.Message);
        }

        public async Task<LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>> CancelAsync(
            Guid id,
            CancelLeaveAdjustmentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxLeaveAdjustment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return FailAction(StatusCodes.Status404NotFound, "Leave adjustment tidak ditemukan.");
            if (entity.RequestedByUserId != actorUserId)
                return FailAction(StatusCodes.Status403Forbidden, "Hanya pembuat adjustment yang dapat membatalkan pengajuan.");
            if (!CanCancel(entity.AdjustmentStatus))
                return FailAction(StatusCodes.Status409Conflict, "Leave adjustment tidak berada pada status yang dapat dibatalkan.");

            var workflow = await FindWorkflowAsync(entity, cancellationToken);
            if (workflow != null)
            {
                WorkflowServiceResult<WorkflowInstanceDetailResponse> workflowAction;
                if (string.Equals(workflow.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.InProgress, StringComparison.OrdinalIgnoreCase))
                {
                    workflowAction = await _workflowService.WithdrawAsync(
                        workflow.Id,
                        new WorkflowWithdrawRequest
                        {
                            Reason = request.Reason.Trim(),
                            IdempotencyKey = NormalizeText(request.IdempotencyKey)
                        },
                        cancellationToken);
                }
                else
                {
                    workflowAction = await _workflowService.CancelAsync(
                        workflow.Id,
                        new WorkflowCancelRequest
                        {
                            Reason = request.Reason.Trim(),
                            IdempotencyKey = NormalizeText(request.IdempotencyKey)
                        },
                        cancellationToken);
                }

                if (!workflowAction.Success)
                    return FailAction(workflowAction.StatusCode, workflowAction.Message);

                await _workflowLifecycleService.SynchronizeAsync(
                    workflow.Id,
                    actorUserId,
                    allowAutoApply: false,
                    cancellationToken: cancellationToken);
            }
            else
            {
                entity.AdjustmentStatus = LeaveValueConstants.AdjustmentStatus.Cancelled;
                entity.IsCancel = true;
                entity.CancelDateTime = DateTime.UtcNow;
                entity.CancelBy = actorUserId;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var detail = await GetByIdAsync(id, true, cancellationToken);
            return detail.Success
                ? LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>.Ok(
                    new LeaveAdjustmentActionResponse { Adjustment = detail.Data! },
                    "Leave adjustment berhasil dibatalkan.")
                : FailAction(detail.StatusCode, detail.Message);
        }

        public async Task<LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>> SynchronizeAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var adjustment = await _dbContext.Set<TrxLeaveAdjustment>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (adjustment == null)
                return FailAction(StatusCodes.Status404NotFound, "Leave adjustment tidak ditemukan.");

            var workflow = await FindWorkflowAsync(adjustment, cancellationToken);
            if (workflow == null)
                return FailAction(StatusCodes.Status404NotFound, "Workflow leave adjustment belum tersedia.");

            var sync = await _workflowLifecycleService.SynchronizeAsync(
                workflow.Id,
                actorUserId,
                allowAutoApply: true,
                cancellationToken: cancellationToken);

            var detail = await GetByIdAsync(id, true, cancellationToken);
            return detail.Success
                ? LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>.Ok(
                    new LeaveAdjustmentActionResponse
                    {
                        Adjustment = detail.Data!,
                        WarningMessage = sync.WarningMessage
                    },
                    sync.WarningMessage == null
                        ? "Status leave adjustment berhasil disinkronkan."
                        : "Status berhasil disinkronkan dengan peringatan auto-post.")
                : FailAction(detail.StatusCode, detail.Message);
        }

        public async Task<LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>> RetryPostAsync(
            Guid id,
            PostLeaveAdjustmentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var post = await _postingService.PostAsync(
                id,
                actorUserId,
                request.Note,
                request.IdempotencyKey,
                cancellationToken);
            if (!post.Success)
                return FailAction(post.StatusCode, post.Message);

            var detail = await GetByIdAsync(id, true, cancellationToken);
            return detail.Success
                ? LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>.Ok(
                    new LeaveAdjustmentActionResponse { Adjustment = detail.Data! },
                    post.Message)
                : FailAction(detail.StatusCode, detail.Message);
        }

        public async Task<LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>> ReverseAsync(
            Guid id,
            ReverseLeaveAdjustmentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var original = await _dbContext.Set<TrxLeaveAdjustment>()
                .Include(x => x.ReversalAdjustment)
                .Include(x => x.LeaveBalance)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (original == null)
                return FailAction(StatusCodes.Status404NotFound, "Original leave adjustment tidak ditemukan.");
            if (!string.Equals(original.AdjustmentStatus, LeaveValueConstants.AdjustmentStatus.Posted, StringComparison.OrdinalIgnoreCase))
                return FailAction(StatusCodes.Status409Conflict, "Hanya leave adjustment berstatus Posted yang dapat direversal.");
            if (original.ReversalAdjustment != null && !original.ReversalAdjustment.IsDelete)
                return FailAction(StatusCodes.Status409Conflict, "Leave adjustment sudah mempunyai reversal adjustment.");

            var reason = await LoadReasonAsync(request.LeaveAdjustmentReasonId, original.LeaveTypeId, cancellationToken);
            if (reason == null || !reason.AllowReversal)
                return FailAction(StatusCodes.Status400BadRequest, "Adjustment reason reversal tidak ditemukan atau tidak mengizinkan reversal.");

            var reversalDirection = string.Equals(
                original.Direction,
                LeaveValueConstants.TransactionDirection.Credit,
                StringComparison.OrdinalIgnoreCase)
                ? LeaveValueConstants.TransactionDirection.Debit
                : LeaveValueConstants.TransactionDirection.Credit;

            if (reason.AllowedDirection != LeaveValueConstants.AdjustmentAllowedDirection.Both &&
                reason.AllowedDirection != reversalDirection)
            {
                return FailAction(
                    StatusCodes.Status400BadRequest,
                    $"Adjustment reason reversal tidak mengizinkan direction {reversalDirection}.");
            }

            if (original.LeaveBalance == null ||
                request.EffectiveDate < original.LeaveBalance.PeriodStartDate ||
                request.EffectiveDate > original.LeaveBalance.PeriodEndDate)
            {
                return FailAction(
                    StatusCodes.Status400BadRequest,
                    "Tanggal efektif reversal harus berada dalam periode leave balance.");
            }

            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var existing = await _dbContext.Set<TrxLeaveAdjustment>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => !x.IsDelete && x.IdempotencyKey == request.IdempotencyKey.Trim(), cancellationToken);
                if (existing != null)
                {
                    var existingDetail = await GetByIdAsync(existing.Id, true, cancellationToken);
                    return existingDetail.Success
                        ? LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>.Ok(
                            new LeaveAdjustmentActionResponse { Adjustment = existingDetail.Data! },
                            "Reversal dengan idempotency key yang sama sudah tersedia.")
                        : FailAction(existingDetail.StatusCode, existingDetail.Message);
                }
            }

            var now = DateTime.UtcNow;
            var reversal = new TrxLeaveAdjustment
            {
                Id = Guid.NewGuid(),
                AdjustmentNumber = GenerateAdjustmentNumber(),
                WorkforceProfileId = original.WorkforceProfileId,
                LeaveBalanceId = original.LeaveBalanceId,
                LeaveTypeId = original.LeaveTypeId,
                LeaveEntitlementPeriodId = original.LeaveEntitlementPeriodId,
                LeaveAdjustmentReasonId = reason.Id,
                OriginalAdjustmentId = original.Id,
                AdjustmentType = LeaveValueConstants.AdjustmentType.Reversal,
                Direction = reversalDirection,
                RequestedDays = original.PostedDays,
                ApprovedDays = original.PostedDays,
                EffectiveDate = request.EffectiveDate,
                AdjustmentStatus = LeaveValueConstants.AdjustmentStatus.Approved,
                IdempotencyKey = NormalizeText(request.IdempotencyKey),
                Reason = request.Reason.Trim(),
                RequestNote = $"Reversal untuk {original.AdjustmentNumber}",
                SourceType = LeaveValueConstants.AdjustmentSourceType.HrManual,
                SourceReferenceId = original.Id,
                SourceReferenceNumber = original.AdjustmentNumber,
                RequestedAt = now,
                RequestedByUserId = actorUserId,
                SubmittedAt = now,
                SubmittedByUserId = actorUserId,
                ApprovedAt = now,
                ApprovedByUserId = actorUserId,
                ApprovalNote = "Reversal adjustment disetujui langsung dan diposting sebagai transaksi lawan.",
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxLeaveAdjustment>().Add(reversal);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var post = await _postingService.PostAsync(
                reversal.Id,
                actorUserId,
                request.Reason,
                request.IdempotencyKey,
                cancellationToken);
            if (!post.Success)
                return FailAction(post.StatusCode, post.Message);

            var detail = await GetByIdAsync(reversal.Id, true, cancellationToken);
            return detail.Success
                ? LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>.Ok(
                    new LeaveAdjustmentActionResponse { Adjustment = detail.Data! },
                    "Leave adjustment berhasil direversal.",
                    StatusCodes.Status201Created)
                : FailAction(detail.StatusCode, detail.Message);
        }

        public async Task<LeaveAdjustmentServiceResult<object>> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxLeaveAdjustment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return LeaveAdjustmentServiceResult<object>.Fail(StatusCodes.Status404NotFound, "Leave adjustment tidak ditemukan.");
            if (!string.Equals(entity.AdjustmentStatus, LeaveValueConstants.AdjustmentStatus.Draft, StringComparison.OrdinalIgnoreCase))
                return LeaveAdjustmentServiceResult<object>.Fail(StatusCodes.Status409Conflict, "Hanya draft leave adjustment yang dapat dihapus.");

            var workflow = await FindWorkflowAsync(entity, cancellationToken);
            if (workflow != null)
            {
                if (!string.Equals(workflow.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.Draft, StringComparison.OrdinalIgnoreCase))
                {
                    return LeaveAdjustmentServiceResult<object>.Fail(
                        StatusCodes.Status409Conflict,
                        "Draft tidak dapat dihapus karena workflow sudah berjalan.");
                }

                var cancelResult = await _workflowService.CancelAsync(
                    workflow.Id,
                    new WorkflowCancelRequest
                    {
                        Reason = "Draft leave adjustment dihapus.",
                        IdempotencyKey = $"leave-adjustment:{entity.Id:N}:delete-cancel"
                    },
                    cancellationToken);

                if (!cancelResult.Success)
                {
                    return LeaveAdjustmentServiceResult<object>.Fail(
                        cancelResult.StatusCode,
                        $"Draft tidak dapat dihapus karena workflow gagal dibatalkan: {cancelResult.Message}");
                }
            }

            var now = DateTime.UtcNow;
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return LeaveAdjustmentServiceResult<object>.Ok(null, "Draft leave adjustment berhasil dihapus.");
        }

        private IQueryable<TrxLeaveAdjustment> BuildQuery()
        {
            return _dbContext.Set<TrxLeaveAdjustment>()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.LeaveBalance)
                .Include(x => x.LeaveType)
                .Include(x => x.LeaveEntitlementPeriod)
                .Include(x => x.LeaveAdjustmentReason)
                .Include(x => x.OriginalAdjustment)
                .Include(x => x.ReversalAdjustment)
                .Include(x => x.RequestedByUser)
                .Include(x => x.SubmittedByUser)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.RejectedByUser)
                .Include(x => x.PostedByUser)
                .Include(x => x.ReversedByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxLeaveAdjustment> ApplyFilters(
            IQueryable<TrxLeaveAdjustment> query,
            LeaveAdjustmentQueryRequest request)
        {
            var range = ResolveDateRange(request.StartDate, request.EndDate, request.CustomPeriod);
            if (range.Start.HasValue) query = query.Where(x => x.RequestedAt >= range.Start.Value);
            if (range.EndExclusive.HasValue) query = query.Where(x => x.RequestedAt < range.EndExclusive.Value);
            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty) query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            if (request.LeaveBalanceId.HasValue && request.LeaveBalanceId.Value != Guid.Empty) query = query.Where(x => x.LeaveBalanceId == request.LeaveBalanceId.Value);
            if (request.LeaveTypeId.HasValue && request.LeaveTypeId.Value != Guid.Empty) query = query.Where(x => x.LeaveTypeId == request.LeaveTypeId.Value);
            if (request.LeaveEntitlementPeriodId.HasValue && request.LeaveEntitlementPeriodId.Value != Guid.Empty) query = query.Where(x => x.LeaveEntitlementPeriodId == request.LeaveEntitlementPeriodId.Value);
            if (request.LeaveAdjustmentReasonId.HasValue && request.LeaveAdjustmentReasonId.Value != Guid.Empty) query = query.Where(x => x.LeaveAdjustmentReasonId == request.LeaveAdjustmentReasonId.Value);
            if (!string.IsNullOrWhiteSpace(request.AdjustmentType)) query = query.Where(x => x.AdjustmentType == request.AdjustmentType.Trim());
            if (!string.IsNullOrWhiteSpace(request.Direction)) query = query.Where(x => x.Direction == request.Direction.Trim());
            if (!string.IsNullOrWhiteSpace(request.AdjustmentStatus)) query = query.Where(x => x.AdjustmentStatus == request.AdjustmentStatus.Trim());
            if (request.HasWorkflow.HasValue) query = request.HasWorkflow.Value ? query.Where(x => x.WorkflowInstanceId.HasValue) : query.Where(x => !x.WorkflowInstanceId.HasValue);
            if (request.RequiresApproval.HasValue) query = query.Where(x => x.LeaveAdjustmentReason != null && x.LeaveAdjustmentReason.RequiresApproval == request.RequiresApproval.Value);
            if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.AdjustmentNumber.ToLower().Contains(keyword) ||
                    x.Reason.ToLower().Contains(keyword) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.DisplayName.ToLower().Contains(keyword)) ||
                    (x.LeaveType != null && x.LeaveType.LeaveTypeName.ToLower().Contains(keyword)) ||
                    (x.LeaveAdjustmentReason != null && x.LeaveAdjustmentReason.ReasonName.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<TrxLeaveAdjustment> ApplySorting(
            IQueryable<TrxLeaveAdjustment> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "requestedAt").Trim().ToLowerInvariant() switch
            {
                "adjustmentnumber" => desc ? query.OrderByDescending(x => x.AdjustmentNumber) : query.OrderBy(x => x.AdjustmentNumber),
                "workforcedisplayname" => desc ? query.OrderByDescending(x => x.WorkforceProfile!.DisplayName) : query.OrderBy(x => x.WorkforceProfile!.DisplayName),
                "leavetypename" => desc ? query.OrderByDescending(x => x.LeaveType!.LeaveTypeName) : query.OrderBy(x => x.LeaveType!.LeaveTypeName),
                "requesteddays" => desc ? query.OrderByDescending(x => x.RequestedDays) : query.OrderBy(x => x.RequestedDays),
                "effectivedate" => desc ? query.OrderByDescending(x => x.EffectiveDate) : query.OrderBy(x => x.EffectiveDate),
                "adjustmentstatus" => desc ? query.OrderByDescending(x => x.AdjustmentStatus) : query.OrderBy(x => x.AdjustmentStatus),
                _ => desc ? query.OrderByDescending(x => x.RequestedAt) : query.OrderBy(x => x.RequestedAt)
            };
        }

        private async Task<LeaveAdjustmentResponse> MapResponseAsync(
            TrxLeaveAdjustment entity,
            CancellationToken cancellationToken)
        {
            var attachmentCount = entity.WorkflowInstanceId.HasValue
                ? await _dbContext.Set<TrxWorkflowAttachment>().AsNoTracking().CountAsync(x => !x.IsDelete && x.IsActive && x.WorkflowInstanceId == entity.WorkflowInstanceId.Value, cancellationToken)
                : 0;
            var response = new LeaveAdjustmentResponse
            {
                Id = entity.Id,
                AdjustmentNumber = entity.AdjustmentNumber,
                WorkforceProfileId = entity.WorkforceProfileId,
                WorkforceProfileCode = entity.WorkforceProfile?.ProfileCode,
                WorkforceDisplayName = entity.WorkforceProfile?.DisplayName ?? string.Empty,
                LeaveBalanceId = entity.LeaveBalanceId,
                LeaveTypeId = entity.LeaveTypeId,
                LeaveTypeCode = entity.LeaveType?.LeaveTypeCode ?? string.Empty,
                LeaveTypeName = entity.LeaveType?.LeaveTypeName ?? string.Empty,
                LeaveEntitlementPeriodId = entity.LeaveEntitlementPeriodId,
                PeriodCode = entity.LeaveEntitlementPeriod?.PeriodCode,
                PeriodName = entity.LeaveEntitlementPeriod?.PeriodName,
                LeaveAdjustmentReasonId = entity.LeaveAdjustmentReasonId,
                ReasonCode = entity.LeaveAdjustmentReason?.ReasonCode ?? string.Empty,
                ReasonName = entity.LeaveAdjustmentReason?.ReasonName ?? string.Empty,
                AdjustmentType = entity.AdjustmentType,
                Direction = entity.Direction,
                RequestedDays = entity.RequestedDays,
                ApprovedDays = entity.ApprovedDays,
                PostedDays = entity.PostedDays,
                EffectiveDate = entity.EffectiveDate,
                AdjustmentStatus = entity.AdjustmentStatus,
                Reason = entity.Reason,
                WorkflowInstanceId = entity.WorkflowInstanceId,
                HasWorkflow = entity.WorkflowInstanceId.HasValue,
                RequiresWorkflow = RequiresWorkflow(entity.LeaveAdjustmentReason),
                RequiresAttachment = entity.LeaveAdjustmentReason?.RequiresAttachment == true,
                RequiresApproval = entity.LeaveAdjustmentReason?.RequiresApproval == true,
                AttachmentCount = attachmentCount,
                RequestedAt = entity.RequestedAt,
                RequestedByUserId = entity.RequestedByUserId,
                RequestedByName = GetUserName(entity.RequestedByUser),
                SubmittedAt = entity.SubmittedAt,
                ApprovedAt = entity.ApprovedAt,
                PostedAt = entity.PostedAt,
                ReversedAt = entity.ReversedAt,
                IsActive = entity.IsActive
            };
            response.AvailableActions = ResolveAvailableActions(entity, response.RequiresWorkflow);
            return response;
        }

        private async Task<LeaveAdjustmentDetailResponse> MapDetailAsync(
            TrxLeaveAdjustment entity,
            bool includeWorkflow,
            CancellationToken cancellationToken)
        {
            var baseResponse = await MapResponseAsync(entity, cancellationToken);
            var detail = new LeaveAdjustmentDetailResponse
            {
                Id = baseResponse.Id,
                AdjustmentNumber = baseResponse.AdjustmentNumber,
                WorkforceProfileId = baseResponse.WorkforceProfileId,
                WorkforceProfileCode = baseResponse.WorkforceProfileCode,
                WorkforceDisplayName = baseResponse.WorkforceDisplayName,
                LeaveBalanceId = baseResponse.LeaveBalanceId,
                LeaveTypeId = baseResponse.LeaveTypeId,
                LeaveTypeCode = baseResponse.LeaveTypeCode,
                LeaveTypeName = baseResponse.LeaveTypeName,
                LeaveEntitlementPeriodId = baseResponse.LeaveEntitlementPeriodId,
                PeriodCode = baseResponse.PeriodCode,
                PeriodName = baseResponse.PeriodName,
                LeaveAdjustmentReasonId = baseResponse.LeaveAdjustmentReasonId,
                ReasonCode = baseResponse.ReasonCode,
                ReasonName = baseResponse.ReasonName,
                AdjustmentType = baseResponse.AdjustmentType,
                Direction = baseResponse.Direction,
                RequestedDays = baseResponse.RequestedDays,
                ApprovedDays = baseResponse.ApprovedDays,
                PostedDays = baseResponse.PostedDays,
                EffectiveDate = baseResponse.EffectiveDate,
                AdjustmentStatus = baseResponse.AdjustmentStatus,
                Reason = baseResponse.Reason,
                WorkflowInstanceId = baseResponse.WorkflowInstanceId,
                HasWorkflow = baseResponse.HasWorkflow,
                RequiresWorkflow = baseResponse.RequiresWorkflow,
                RequiresAttachment = baseResponse.RequiresAttachment,
                RequiresApproval = baseResponse.RequiresApproval,
                AttachmentCount = baseResponse.AttachmentCount,
                RequestedAt = baseResponse.RequestedAt,
                RequestedByUserId = baseResponse.RequestedByUserId,
                RequestedByName = baseResponse.RequestedByName,
                SubmittedAt = baseResponse.SubmittedAt,
                ApprovedAt = baseResponse.ApprovedAt,
                PostedAt = baseResponse.PostedAt,
                ReversedAt = baseResponse.ReversedAt,
                IsActive = baseResponse.IsActive,
                AvailableActions = baseResponse.AvailableActions,
                OriginalAdjustmentId = entity.OriginalAdjustmentId,
                OriginalAdjustmentNumber = entity.OriginalAdjustment?.AdjustmentNumber,
                ReversalAdjustmentId = entity.ReversalAdjustment?.Id,
                ReversalAdjustmentNumber = entity.ReversalAdjustment?.AdjustmentNumber,
                RequestNote = entity.RequestNote,
                SourceType = entity.SourceType,
                SourceReferenceId = entity.SourceReferenceId,
                SourceReferenceNumber = entity.SourceReferenceNumber,
                SubmittedByUserId = entity.SubmittedByUserId,
                SubmittedByName = GetUserName(entity.SubmittedByUser),
                ApprovedByUserId = entity.ApprovedByUserId,
                ApprovedByName = GetUserName(entity.ApprovedByUser),
                ApprovalNote = entity.ApprovalNote,
                RejectedAt = entity.RejectedAt,
                RejectedByUserId = entity.RejectedByUserId,
                RejectedByName = GetUserName(entity.RejectedByUser),
                RejectionReason = entity.RejectionReason,
                PostedByUserId = entity.PostedByUserId,
                PostedByName = GetUserName(entity.PostedByUser),
                ReversedByUserId = entity.ReversedByUserId,
                ReversedByName = GetUserName(entity.ReversedByUser),
                ReversalReason = entity.ReversalReason,
                RequestSnapshotJson = entity.RequestSnapshotJson,
                ApprovalSnapshotJson = entity.ApprovalSnapshotJson,
                PostingSnapshotJson = entity.PostingSnapshotJson,
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime,
                Balance = new LeaveAdjustmentBalanceSnapshotResponse
                {
                    OpeningBalanceDays = entity.LeaveBalance?.OpeningBalanceDays ?? 0,
                    AdjustmentDays = entity.LeaveBalance?.AdjustmentDays ?? 0,
                    ReservedDays = entity.LeaveBalance?.ReservedDays ?? 0,
                    UsedDays = entity.LeaveBalance?.UsedDays ?? 0,
                    RemainingDays = entity.LeaveBalance?.RemainingDays ?? 0,
                    AvailableDays = entity.LeaveBalance?.AvailableDays ?? 0,
                    BalanceStatus = entity.LeaveBalance?.BalanceStatus ?? string.Empty,
                    IsLocked = entity.LeaveBalance?.IsLocked ?? false,
                    BalanceVersion = entity.LeaveBalance?.BalanceVersion ?? 0,
                    LastTransactionSequence = entity.LeaveBalance?.LastTransactionSequence ?? 0
                }
            };

            var posting = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDelete && x.LeaveAdjustmentId == entity.Id, cancellationToken);
            if (posting != null && entity.LeaveBalance != null)
            {
                detail.Posting = new LeaveAdjustmentPostingResponse
                {
                    LeaveAdjustmentId = entity.Id,
                    AdjustmentNumber = entity.AdjustmentNumber,
                    LeaveBalanceId = entity.LeaveBalanceId,
                    BalanceTransactionId = posting.Id,
                    TransactionNumber = posting.TransactionNumber,
                    TransactionSequence = posting.TransactionSequence,
                    TransactionType = posting.TransactionType,
                    Direction = posting.Direction,
                    PostedDays = entity.PostedDays,
                    PreviousAvailableDays = posting.PreviousAvailableDays,
                    NewAvailableDays = posting.NewAvailableDays,
                    NewRemainingDays = entity.LeaveBalance.RemainingDays,
                    BalanceVersion = entity.LeaveBalance.BalanceVersion,
                    IsIdempotent = false,
                    PostedAt = posting.PostedAt ?? posting.TransactionDateTime
                };
            }

            if (includeWorkflow)
            {
                var workflow = await FindWorkflowAsync(entity, cancellationToken);
                if (workflow != null)
                {
                    var workflowResult = await _workflowService.GetByIdAsync(workflow.Id, cancellationToken);
                    if (workflowResult.Success)
                        detail.Workflow = workflowResult.Data;
                }
            }
            return detail;
        }

        private async Task<LeaveAdjustmentServiceResult<WorkflowInstanceDetailResponse>> EnsureWorkflowAsync(
            TrxLeaveAdjustment entity,
            MstLeaveAdjustmentReason reason,
            string? sourceChannel,
            string? correlationId,
            IEnumerable<Guid>? selectedApproverUserIds,
            CancellationToken cancellationToken)
        {
            var existing = await FindWorkflowAsync(entity, cancellationToken);
            if (existing != null)
            {
                if (!entity.WorkflowInstanceId.HasValue)
                {
                    var tracked = await _dbContext.Set<TrxLeaveAdjustment>().FirstAsync(x => x.Id == entity.Id, cancellationToken);
                    tracked.WorkflowInstanceId = existing.Id;
                    tracked.UpdateDateTime = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    entity.WorkflowInstanceId = existing.Id;
                }
                var current = await _workflowService.GetByIdAsync(existing.Id, cancellationToken);
                return current.Success && current.Data != null
                    ? LeaveAdjustmentServiceResult<WorkflowInstanceDetailResponse>.Ok(current.Data, "Workflow leave adjustment sudah tersedia.")
                    : LeaveAdjustmentServiceResult<WorkflowInstanceDetailResponse>.Fail(current.StatusCode, current.Message);
            }

            var workflowCode = NormalizeText(reason.ApprovalWorkflowCode) ?? "LEAVE_ADJUSTMENT";
            var create = await _workflowService.CreateAsync(
                new CreateWorkflowInstanceRequest
                {
                    WorkflowDefinitionCode = workflowCode,
                    ReferenceType = LeaveValueConstants.WorkflowReferenceType.LeaveAdjustment,
                    ReferenceId = entity.Id,
                    ExternalReferenceNumber = entity.AdjustmentNumber,
                    SourceChannel = NormalizeText(sourceChannel) ?? "Web",
                    RequestCorrelationId = NormalizeText(correlationId),
                    IdempotencyKey = $"leave-adjustment:{entity.Id:N}:workflow",
                    RequestContext = JsonSerializer.SerializeToElement(new
                    {
                        leaveAdjustmentId = entity.Id,
                        entity.AdjustmentNumber,
                        entity.WorkforceProfileId,
                        entity.LeaveBalanceId,
                        entity.LeaveTypeId,
                        entity.LeaveEntitlementPeriodId,
                        entity.LeaveAdjustmentReasonId,
                        entity.AdjustmentType,
                        entity.Direction,
                        entity.RequestedDays,
                        entity.EffectiveDate,
                        entity.Reason,
                        reason.RequiresApproval,
                        reason.RequiresAttachment
                    }),
                    SelectedApproverUserIds = selectedApproverUserIds?
                        .Where(x => x != Guid.Empty)
                        .Distinct()
                        .ToList() ?? new List<Guid>()
                },
                cancellationToken);

            if (!create.Success || create.Data == null)
                return LeaveAdjustmentServiceResult<WorkflowInstanceDetailResponse>.Fail(create.StatusCode, create.Message);

            var adjustment = await _dbContext.Set<TrxLeaveAdjustment>().FirstAsync(x => x.Id == entity.Id, cancellationToken);
            adjustment.WorkflowInstanceId = create.Data.Id;
            adjustment.UpdateDateTime = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            entity.WorkflowInstanceId = create.Data.Id;
            return LeaveAdjustmentServiceResult<WorkflowInstanceDetailResponse>.Ok(create.Data, "Workflow leave adjustment berhasil dibuat.", StatusCodes.Status201Created);
        }

        private async Task<TrxWorkflowInstance?> FindWorkflowAsync(
            TrxLeaveAdjustment adjustment,
            CancellationToken cancellationToken)
        {
            if (adjustment.WorkflowInstanceId.HasValue)
            {
                var direct = await _dbContext.Set<TrxWorkflowInstance>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == adjustment.WorkflowInstanceId.Value && !x.IsDelete, cancellationToken);
                if (direct != null) return direct;
            }

            return await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ReferenceId == adjustment.Id && WorkflowReferenceAliases.Contains(x.ReferenceType))
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<(bool Success, int StatusCode, string Message, WfpLeaveBalance? Balance, MstLeaveAdjustmentReason? Reason)> LoadValidationContextAsync(
            Guid balanceId,
            Guid reasonId,
            CancellationToken cancellationToken)
        {
            var balance = await _dbContext.Set<WfpLeaveBalance>()
                .Include(x => x.LeavePolicy)
                .Include(x => x.LeaveEntitlementPeriod)
                .Include(x => x.LeaveType)
                .Include(x => x.WorkforceProfile)
                .FirstOrDefaultAsync(x => x.Id == balanceId && !x.IsDelete, cancellationToken);
            if (balance == null)
                return (false, StatusCodes.Status404NotFound, "Leave balance tidak ditemukan.", null, null);
            if (!balance.LeaveEntitlementPeriodId.HasValue)
                return (false, StatusCodes.Status409Conflict, "Leave balance belum terhubung dengan entitlement period.", null, null);

            var reason = await LoadReasonAsync(reasonId, balance.LeaveTypeId, cancellationToken);
            if (reason == null)
                return (false, StatusCodes.Status400BadRequest, "Adjustment reason tidak ditemukan, tidak aktif, atau tidak berlaku untuk leave type tersebut.", null, null);
            return (true, StatusCodes.Status200OK, string.Empty, balance, reason);
        }

        private async Task<MstLeaveAdjustmentReason?> LoadReasonAsync(
            Guid reasonId,
            Guid leaveTypeId,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            return await _dbContext.Set<MstLeaveAdjustmentReason>()
                .FirstOrDefaultAsync(x =>
                    x.Id == reasonId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    (!x.LeaveTypeId.HasValue || x.LeaveTypeId == leaveTypeId) &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate <= now) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate >= now),
                    cancellationToken);
        }

        private async Task<string?> ValidateRequestAsync(
            Guid? excludeId,
            WfpLeaveBalance balance,
            MstLeaveAdjustmentReason reason,
            string adjustmentType,
            string direction,
            decimal requestedDays,
            DateOnly effectiveDate,
            string requestReason,
            CancellationToken cancellationToken)
        {
            if (balance.IsLocked || !balance.IsActive || balance.IsDelete)
                return "Leave balance sedang terkunci atau tidak aktif.";
            if (balance.LeaveEntitlementPeriod?.IsLocked == true ||
                string.Equals(balance.LeaveEntitlementPeriod?.PeriodStatus, LeaveValueConstants.PeriodStatus.Closed, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(balance.LeaveEntitlementPeriod?.PeriodStatus, LeaveValueConstants.PeriodStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
                return "Entitlement period sedang terkunci, sudah ditutup, atau dibatalkan.";
            if (effectiveDate < balance.PeriodStartDate || effectiveDate > balance.PeriodEndDate)
                return "Tanggal efektif harus berada dalam periode leave balance.";
            if (requestedDays <= 0)
                return "Jumlah hari adjustment harus lebih besar dari 0.";
            if (reason.RequiresComment && string.IsNullOrWhiteSpace(requestReason))
                return "Adjustment reason mewajibkan komentar/alasan.";
            if (reason.MaximumAdjustmentDays.HasValue && requestedDays > reason.MaximumAdjustmentDays.Value)
                return $"Jumlah adjustment melebihi batas {reason.MaximumAdjustmentDays.Value:0.####} hari.";

            var type = NormalizeAdjustmentType(adjustmentType);
            var normalizedDirection = NormalizeDirection(direction);
            if (!IsAllowedType(type)) return "Adjustment type tidak valid.";
            if (!IsAllowedDirection(normalizedDirection)) return "Direction adjustment tidak valid.";
            if (type == LeaveValueConstants.AdjustmentType.Reversal)
                return "Reversal harus dibuat melalui endpoint reversal.";
            if (type == LeaveValueConstants.AdjustmentType.OpeningBalance && !reason.AllowOpeningBalance)
                return "Adjustment reason tidak mengizinkan opening balance.";
            if (type == LeaveValueConstants.AdjustmentType.ManualAdjustment && !reason.AllowManualAdjustment)
                return "Adjustment reason tidak mengizinkan manual adjustment.";
            if (type == LeaveValueConstants.AdjustmentType.Correction && !reason.AllowCorrection)
                return "Adjustment reason tidak mengizinkan correction.";
            if (type == LeaveValueConstants.AdjustmentType.OpeningBalance && normalizedDirection != LeaveValueConstants.TransactionDirection.Credit)
                return "Opening balance hanya dapat menggunakan direction Credit.";
            if (reason.AllowedDirection != LeaveValueConstants.AdjustmentAllowedDirection.Both && reason.AllowedDirection != normalizedDirection)
                return $"Adjustment reason hanya mengizinkan direction {reason.AllowedDirection}.";

            if (type == LeaveValueConstants.AdjustmentType.OpeningBalance)
            {
                var duplicateQuery = _dbContext.Set<TrxLeaveAdjustment>().AsNoTracking().Where(x =>
                    !x.IsDelete &&
                    x.LeaveBalanceId == balance.Id &&
                    x.AdjustmentType == LeaveValueConstants.AdjustmentType.OpeningBalance &&
                    x.AdjustmentStatus != LeaveValueConstants.AdjustmentStatus.Rejected &&
                    x.AdjustmentStatus != LeaveValueConstants.AdjustmentStatus.Cancelled &&
                    x.AdjustmentStatus != LeaveValueConstants.AdjustmentStatus.Reversed);
                if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
                if (await duplicateQuery.AnyAsync(cancellationToken))
                    return "Leave balance sudah mempunyai opening balance aktif.";
            }

            if (normalizedDirection == LeaveValueConstants.TransactionDirection.Debit)
            {
                var projectedAvailable = balance.AvailableDays - requestedDays;
                if (projectedAvailable < 0 && balance.LeavePolicy?.AllowNegativeBalance != true)
                    return "Debit adjustment akan menyebabkan saldo negatif, sedangkan policy tidak mengizinkannya.";
                if (balance.LeavePolicy?.NegativeBalanceLimitDays.HasValue == true &&
                    projectedAvailable < -balance.LeavePolicy.NegativeBalanceLimitDays.Value)
                    return $"Debit adjustment melebihi batas saldo negatif {balance.LeavePolicy.NegativeBalanceLimitDays.Value:0.####} hari.";
            }

            return null;
        }

        private static bool RequiresWorkflow(MstLeaveAdjustmentReason? reason)
        {
            return reason?.RequiresApproval == true || reason?.RequiresAttachment == true;
        }

        private static bool CanEdit(string status)
        {
            return status == LeaveValueConstants.AdjustmentStatus.Draft || status == LeaveValueConstants.AdjustmentStatus.NeedRevision;
        }

        private static bool CanCancel(string status)
        {
            return status == LeaveValueConstants.AdjustmentStatus.Draft ||
                   status == LeaveValueConstants.AdjustmentStatus.Submitted ||
                   status == LeaveValueConstants.AdjustmentStatus.UnderReview ||
                   status == LeaveValueConstants.AdjustmentStatus.NeedRevision;
        }

        private static List<string> ResolveAvailableActions(TrxLeaveAdjustment entity, bool requiresWorkflow)
        {
            var actions = new List<string> { "View" };
            if (CanEdit(entity.AdjustmentStatus))
            {
                actions.Add("Update");
                actions.Add("Submit");
                actions.Add("Cancel");
                if (requiresWorkflow && !entity.WorkflowInstanceId.HasValue) actions.Add("PrepareWorkflow");
                if (entity.AdjustmentStatus == LeaveValueConstants.AdjustmentStatus.Draft) actions.Add("Delete");
            }
            if (entity.AdjustmentStatus == LeaveValueConstants.AdjustmentStatus.Submitted || entity.AdjustmentStatus == LeaveValueConstants.AdjustmentStatus.UnderReview)
                actions.Add("Cancel");
            if (entity.WorkflowInstanceId.HasValue) actions.Add("OpenWorkflow");
            if (entity.AdjustmentStatus == LeaveValueConstants.AdjustmentStatus.Approved) actions.Add("RetryPost");
            if (entity.AdjustmentStatus == LeaveValueConstants.AdjustmentStatus.Posted && entity.ReversalAdjustment == null) actions.Add("Reverse");
            return actions.Distinct().ToList();
        }

        private static string NormalizeAdjustmentType(string value)
        {
            var input = value.Trim();
            if (input.Equals(LeaveValueConstants.AdjustmentType.OpeningBalance, StringComparison.OrdinalIgnoreCase)) return LeaveValueConstants.AdjustmentType.OpeningBalance;
            if (input.Equals(LeaveValueConstants.AdjustmentType.ManualAdjustment, StringComparison.OrdinalIgnoreCase)) return LeaveValueConstants.AdjustmentType.ManualAdjustment;
            if (input.Equals(LeaveValueConstants.AdjustmentType.Correction, StringComparison.OrdinalIgnoreCase)) return LeaveValueConstants.AdjustmentType.Correction;
            if (input.Equals(LeaveValueConstants.AdjustmentType.Reversal, StringComparison.OrdinalIgnoreCase)) return LeaveValueConstants.AdjustmentType.Reversal;
            return input;
        }

        private static string NormalizeDirection(string value)
        {
            var input = value.Trim();
            if (input.Equals(LeaveValueConstants.TransactionDirection.Debit, StringComparison.OrdinalIgnoreCase)) return LeaveValueConstants.TransactionDirection.Debit;
            if (input.Equals(LeaveValueConstants.TransactionDirection.Credit, StringComparison.OrdinalIgnoreCase)) return LeaveValueConstants.TransactionDirection.Credit;
            return input;
        }

        private static bool IsAllowedType(string value) =>
            value == LeaveValueConstants.AdjustmentType.OpeningBalance ||
            value == LeaveValueConstants.AdjustmentType.ManualAdjustment ||
            value == LeaveValueConstants.AdjustmentType.Correction ||
            value == LeaveValueConstants.AdjustmentType.Reversal;

        private static bool IsAllowedDirection(string value) =>
            value == LeaveValueConstants.TransactionDirection.Credit ||
            value == LeaveValueConstants.TransactionDirection.Debit;

        private static string GenerateAdjustmentNumber()
        {
            return $"LADJ-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..34].ToUpperInvariant();
        }

        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static string? GetUserName(ApplicationUser? user) => user == null ? null : user.DisplayName ?? user.UserName ?? user.Email ?? user.UserCode;
        private static LeaveAdjustmentOptionResponse Option(string value, string label) => new() { Value = value, Label = label };
        private static LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse> FailAction(int statusCode, string message) => LeaveAdjustmentServiceResult<LeaveAdjustmentActionResponse>.Fail(statusCode, message);

        private static void NormalizePaging(LeaveAdjustmentQueryRequest request)
        {
            request.PageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 100);
        }

        private static (DateTime? Start, DateTime? EndExclusive) ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            if (startDate.HasValue || endDate.HasValue)
            {
                return (
                    startDate.HasValue ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc) : null,
                    endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc) : null);
            }

            var today = DateTime.UtcNow.Date;
            return customPeriod?.Trim().ToLowerInvariant() switch
            {
                "today" => (today, today.AddDays(1)),
                "last7days" => (today.AddDays(-6), today.AddDays(1)),
                "thismonth" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                "lastmonth" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
                "thisyear" => (new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                _ => (null, null)
            };
        }
    }
}
