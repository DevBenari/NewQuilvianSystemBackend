using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveRequestService
    {
        private static readonly string[] WaitingStatuses =
        {
            LeaveRequestValueConstants.Status.Submitted,
            LeaveRequestValueConstants.Status.WaitingApproval
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveRequestCalculationService _calculationService;
        private readonly LeaveRequestReservationService _reservationService;
        private readonly WorkflowService _workflowService;
        private readonly WorkflowFileStorageService _fileStorageService;

        public LeaveRequestService(
            ApplicationDbContext dbContext,
            LeaveRequestCalculationService calculationService,
            LeaveRequestReservationService reservationService,
            WorkflowService workflowService,
            WorkflowFileStorageService fileStorageService)
        {
            _dbContext = dbContext;
            _calculationService = calculationService;
            _reservationService = reservationService;
            _workflowService = workflowService;
            _fileStorageService = fileStorageService;
        }

        public LeaveRequestFilterMetadataResponse GetMetadata() =>
            new()
            {
                Statuses = new()
                {
                    Option(LeaveRequestValueConstants.Status.Draft, "Draft"),
                    Option(LeaveRequestValueConstants.Status.Submitted, "Diajukan"),
                    Option(LeaveRequestValueConstants.Status.WaitingApproval, "Menunggu persetujuan"),
                    Option(LeaveRequestValueConstants.Status.NeedRevision, "Perlu revisi"),
                    Option(LeaveRequestValueConstants.Status.Approved, "Disetujui"),
                    Option(LeaveRequestValueConstants.Status.Rejected, "Ditolak"),
                    Option(LeaveRequestValueConstants.Status.Cancelled, "Dibatalkan"),
                    Option(LeaveRequestValueConstants.Status.Taken, "Sedang diambil"),
                    Option(LeaveRequestValueConstants.Status.Completed, "Selesai")
                },
                HalfDayPeriods = new()
                {
                    Option(LeaveRequestValueConstants.HalfDayPeriod.FirstHalf, "Setengah hari pertama"),
                    Option(LeaveRequestValueConstants.HalfDayPeriod.SecondHalf, "Setengah hari kedua")
                },
                AttachmentTypes = new()
                {
                    Option(LeaveRequestValueConstants.AttachmentType.SupportingDocument, "Dokumen pendukung"),
                    Option(LeaveRequestValueConstants.AttachmentType.MedicalCertificate, "Surat keterangan medis"),
                    Option(LeaveRequestValueConstants.AttachmentType.HandoverDocument, "Dokumen serah terima"),
                    Option(LeaveRequestValueConstants.AttachmentType.Other, "Lainnya")
                },
                SourceChannels = new()
                {
                    Option(LeaveRequestValueConstants.SourceChannel.Web, "Web"),
                    Option(LeaveRequestValueConstants.SourceChannel.Mobile, "Mobile"),
                    Option(LeaveRequestValueConstants.SourceChannel.Api, "API")
                },
                SortOptions = new()
                {
                    Option("createDateTime", "Tanggal dibuat"),
                    Option("startDate", "Tanggal mulai"),
                    Option("requestNumber", "Nomor pengajuan"),
                    Option("leaveTypeName", "Jenis cuti"),
                    Option("requestedDays", "Jumlah hari"),
                    Option("status", "Status")
                },
                MaximumAttachmentSizeBytes = _fileStorageService.MaximumFileSizeBytes,
                AllowedAttachmentExtensions = _fileStorageService.AllowedExtensions.ToList()
            };

        public async Task<LeaveRequestServiceResult<List<LeaveRequestBalanceOptionResponse>>> GetBalanceOptionsAsync(
            Guid actorUserId,
            DateOnly? asOfDate,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
            {
                return LeaveRequestServiceResult<List<LeaveRequestBalanceOptionResponse>>.Fail(
                    actor.StatusCode,
                    actor.Message);
            }

            var date = asOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var balances = await _dbContext.Set<WfpLeaveBalance>()
                .AsNoTracking()
                .Include(x => x.LeaveType)
                .Include(x => x.LeavePolicy)
                .Include(x => x.LeaveEntitlementPeriod)
                .Where(x =>
                    x.WorkforceProfileId == actor.Data.WorkforceProfileId &&
                    x.PeriodStartDate <= date &&
                    x.PeriodEndDate >= date &&
                    x.IsActive &&
                    !x.IsDelete)
                .OrderBy(x => x.LeaveType!.LeaveTypeName)
                .ToListAsync(cancellationToken);

            var data = balances.Select(balance =>
            {
                var policy = balance.LeavePolicy;
                var minimum = policy?.AllowNegativeBalance == true
                    ? -(policy.NegativeBalanceLimitDays ?? decimal.MaxValue)
                    : 0;
                var canRequest =
                    !balance.IsLocked &&
                    balance.BalanceStatus == LeaveValueConstants.BalanceStatus.Active &&
                    balance.AvailableDays > minimum;

                return new LeaveRequestBalanceOptionResponse
                {
                    LeaveBalanceId = balance.Id,
                    LeaveTypeId = balance.LeaveTypeId,
                    LeaveTypeCode = balance.LeaveType?.LeaveTypeCode ?? string.Empty,
                    LeaveTypeName = balance.LeaveType?.LeaveTypeName ?? string.Empty,
                    LeaveCategory = balance.LeaveType?.LeaveCategory ?? string.Empty,
                    ColorCode = balance.LeaveType?.ColorCode,
                    LeavePolicyId = balance.LeavePolicyId,
                    LeavePolicyCode = policy?.LeavePolicyCode,
                    LeavePolicyName = policy?.LeavePolicyName,
                    LeaveEntitlementPeriodId = balance.LeaveEntitlementPeriodId,
                    EntitlementPeriodCode = balance.LeaveEntitlementPeriod?.PeriodCode,
                    EntitlementPeriodName = balance.LeaveEntitlementPeriod?.PeriodName,
                    PeriodStartDate = balance.PeriodStartDate,
                    PeriodEndDate = balance.PeriodEndDate,
                    RemainingDays = balance.RemainingDays,
                    AvailableDays = balance.AvailableDays,
                    ReservedDays = balance.ReservedDays,
                    PendingDays = balance.PendingDays,
                    IsPaidLeave = balance.LeaveType?.IsPaidLeave ?? false,
                    IsBalanceDeducted = balance.LeaveType?.IsBalanceDeducted ?? true,
                    AllowHalfDay = balance.LeaveType?.AllowHalfDay ?? false,
                    AllowHourly = balance.LeaveType?.AllowHourly ?? false,
                    RequiresAttachment =
                        (balance.LeaveType?.RequiresAttachment ?? false) ||
                        (policy?.RequireAttachment ?? false),
                    RequiresMedicalCertificate = balance.LeaveType?.RequiresMedicalCertificate ?? false,
                    IsLocked = balance.IsLocked,
                    CanRequest = canRequest,
                    RestrictionReason = canRequest
                        ? null
                        : balance.IsLocked
                            ? "Saldo sedang dikunci."
                            : "Saldo tidak tersedia atau periodenya sudah ditutup."
                };
            }).ToList();

            var existingLeaveTypeIds = data.Select(x => x.LeaveTypeId).ToHashSet();
            var nonBalanceTypes = await _dbContext.Set<MstLeaveType>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsBalanceDeducted &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !existingLeaveTypeIds.Contains(x.Id))
                .OrderBy(x => x.LeaveTypeName)
                .ToListAsync(cancellationToken);

            foreach (var leaveType in nonBalanceTypes)
            {
                var policy = await _calculationService.ResolvePolicyForActorAsync(
                    actor.Data,
                    leaveType.Id,
                    date,
                    cancellationToken);
                if (policy == null) continue;

                data.Add(new LeaveRequestBalanceOptionResponse
                {
                    LeaveTypeId = leaveType.Id,
                    LeaveTypeCode = leaveType.LeaveTypeCode,
                    LeaveTypeName = leaveType.LeaveTypeName,
                    LeaveCategory = leaveType.LeaveCategory,
                    ColorCode = leaveType.ColorCode,
                    LeavePolicyId = policy.Id,
                    LeavePolicyCode = policy.LeavePolicyCode,
                    LeavePolicyName = policy.LeavePolicyName,
                    IsPaidLeave = leaveType.IsPaidLeave,
                    IsBalanceDeducted = false,
                    AllowHalfDay = leaveType.AllowHalfDay,
                    AllowHourly = leaveType.AllowHourly,
                    RequiresAttachment = leaveType.RequiresAttachment || policy.RequireAttachment,
                    RequiresMedicalCertificate = leaveType.RequiresMedicalCertificate,
                    CanRequest = true
                });
            }

            return LeaveRequestServiceResult<List<LeaveRequestBalanceOptionResponse>>.Ok(
                data.OrderBy(x => x.LeaveTypeName).ToList(),
                "Pilihan saldo dan jenis cuti berhasil diambil.");
        }

        public async Task<LeaveRequestServiceResult<List<LeaveRequestReasonOptionResponse>>> GetReasonOptionsAsync(
            string? search,
            CancellationToken cancellationToken = default)
        {
            var today = DateTime.UtcNow.Date;
            var query = _dbContext.Set<MstRequestReason>()
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.IsEmployeeSelectable &&
                    !x.IsDelete &&
                    (x.RequestType == "LeaveRequest" || x.RequestType == "LEAVE_REQUEST") &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ReasonCode.ToLower().Contains(keyword) ||
                    x.ReasonName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ReasonName)
                .Take(100)
                .Select(x => new LeaveRequestReasonOptionResponse
                {
                    Id = x.Id,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    ReasonCategory = x.ReasonCategory,
                    IsCommentRequired = x.IsCommentRequired,
                    IsAttachmentRequired = x.IsAttachmentRequired,
                    Description = x.Description
                })
                .ToListAsync(cancellationToken);

            return LeaveRequestServiceResult<List<LeaveRequestReasonOptionResponse>>.Ok(
                data,
                "Pilihan alasan pengajuan cuti berhasil diambil.");
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestSummaryResponse>> GetSummaryAsync(
            Guid actorUserId,
            LeaveRequestQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<LeaveRequestSummaryResponse>.Fail(actor.StatusCode, actor.Message);

            var query = ApplyFilters(BuildMyQuery(actor.Data.WorkforceProfileId), request);
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var response = new LeaveRequestSummaryResponse
            {
                TotalRequest = await query.CountAsync(cancellationToken),
                Draft = await query.CountAsync(x => x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Draft, cancellationToken),
                WaitingApproval = await query.CountAsync(x => WaitingStatuses.Contains(x.LeaveRequestStatus), cancellationToken),
                NeedRevision = await query.CountAsync(x => x.LeaveRequestStatus == LeaveRequestValueConstants.Status.NeedRevision, cancellationToken),
                Approved = await query.CountAsync(x => x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Approved, cancellationToken),
                Rejected = await query.CountAsync(x => x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Rejected, cancellationToken),
                Cancelled = await query.CountAsync(x => x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Cancelled, cancellationToken),
                UpcomingLeave = await query.CountAsync(x =>
                    x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Approved &&
                    x.StartDate >= today,
                    cancellationToken),
                TotalRequestedDays = await query.SumAsync(x => (decimal?)x.RequestedDays, cancellationToken) ?? 0,
                TotalApprovedDays = await query
                    .Where(x =>
                        x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Approved ||
                        x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Taken ||
                        x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Completed)
                    .SumAsync(x => (decimal?)x.ActualBalanceDeduction, cancellationToken) ?? 0
            };

            return LeaveRequestServiceResult<LeaveRequestSummaryResponse>.Ok(
                response,
                "Ringkasan pengajuan cuti berhasil diambil.");
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestPagedResponse>> GetPagedAsync(
            Guid actorUserId,
            LeaveRequestQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<LeaveRequestPagedResponse>.Fail(actor.StatusCode, actor.Message);

            request.PageNumber = Math.Max(1, request.PageNumber);
            request.PageSize = Math.Clamp(request.PageSize, 1, 100);
            IQueryable<WfpLeaveRequest> query = ApplyFilters(
                BuildMyQuery(actor.Data.WorkforceProfileId),
                request);
            query = ApplySort(query, request.SortBy, request.SortDirection);

            var total = await query.CountAsync(cancellationToken);
            var rows = await query
                .Include(x => x.LeaveType)
                .Include(x => x.Attachments)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var workflowIds = rows
                .Where(x => x.WorkflowInstanceId.HasValue)
                .Select(x => x.WorkflowInstanceId!.Value)
                .Distinct()
                .ToList();

            var workflowStatuses = workflowIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.Set<TrxWorkflowInstance>()
                    .AsNoTracking()
                    .Where(x => workflowIds.Contains(x.Id) && !x.IsDelete)
                    .ToDictionaryAsync(x => x.Id, x => x.WorkflowStatus, cancellationToken);

            var items = new List<LeaveRequestListResponse>();
            foreach (var row in rows)
            {
                var workflowStatus = row.WorkflowInstanceId.HasValue &&
                                     workflowStatuses.TryGetValue(row.WorkflowInstanceId.Value, out var status)
                    ? status
                    : null;
                var reserved = await _reservationService.HasActiveReservationAsync(row.Id, cancellationToken);
                items.Add(MapList(row, workflowStatus, reserved));
            }

            return LeaveRequestServiceResult<LeaveRequestPagedResponse>.Ok(
                new LeaveRequestPagedResponse
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalData = total,
                    TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
                    Items = items
                },
                "Daftar pengajuan cuti berhasil diambil.");
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestDetailResponse>> GetByIdAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<LeaveRequestDetailResponse>.Fail(actor.StatusCode, actor.Message);

            var entity = await LoadOwnedRequestAsync(id, actor.Data.WorkforceProfileId, true, cancellationToken);
            if (entity == null)
                return LeaveRequestServiceResult<LeaveRequestDetailResponse>.Fail(StatusCodes.Status404NotFound, "Pengajuan cuti tidak ditemukan.");

            var workflow = entity.WorkflowInstanceId.HasValue
                ? await _dbContext.Set<TrxWorkflowInstance>()
                    .AsNoTracking()
                    .Include(x => x.StatusHistories)
                    .FirstOrDefaultAsync(x =>
                        x.Id == entity.WorkflowInstanceId.Value &&
                        !x.IsDelete,
                        cancellationToken)
                : null;

            var reserved = await _reservationService.HasActiveReservationAsync(entity.Id, cancellationToken);
            var response = MapDetail(entity, workflow?.WorkflowStatus, reserved);
            if (workflow != null)
            {
                response.Timeline.AddRange(workflow.StatusHistories
                    .OrderBy(x => x.ChangedAt)
                    .Select(x => new LeaveRequestTimelineResponse
                    {
                        At = x.ChangedAt,
                        EventType = x.ActionType,
                        Status = x.ToWorkflowStatus,
                        Description = x.Comment,
                        IsWorkflowEvent = true
                    }));
            }
            response.Timeline = response.Timeline.OrderBy(x => x.At).ToList();

            return LeaveRequestServiceResult<LeaveRequestDetailResponse>.Ok(
                response,
                "Detail pengajuan cuti berhasil diambil.");
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestActionResponse>> CreateAsync(
            Guid actorUserId,
            CreateLeaveRequestRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(actor.StatusCode, actor.Message);

            var calculation = await _calculationService.CalculateForActorAsync(actor.Data, request, cancellationToken);
            if (!calculation.Success || calculation.Data == null)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(calculation.StatusCode, calculation.Message);
            if (!calculation.Data.IsValid)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(StatusCodes.Status400BadRequest, string.Join(" | ", calculation.Data.Errors));

            var entity = new WfpLeaveRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = GenerateRequestNumber(),
                WorkforceProfileId = actor.Data.WorkforceProfileId,
                EmployeeId = actor.Data.EmployeeId,
                LeaveTypeId = request.LeaveTypeId,
                LeavePolicyId = calculation.Data.LeavePolicyId,
                LeaveBalanceId = calculation.Data.LeaveBalanceId,
                OrganizationAssignmentId = actor.Data.OrganizationAssignmentId,
                HospitalSiteId = actor.Data.HospitalSiteId,
                OrganizationUnitId = actor.Data.OrganizationUnitId,
                DepartmentId = actor.Data.DepartmentId,
                PositionId = actor.Data.PositionId,
                ReplacementWorkforceProfileId = request.ReplacementWorkforceProfileId,
                RequestReasonId = request.RequestReasonId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsHalfDay = request.IsHalfDay,
                HalfDayPeriod = request.HalfDayPeriod,
                IsHourly = request.IsHourly,
                RequestedMinutes = request.RequestedMinutes,
                RequestedDays = calculation.Data.RequestedDays,
                CalculatedWorkingDays = calculation.Data.CalculatedWorkingDays,
                ExcludedHolidayDays = calculation.Data.ExcludedHolidayDays,
                ExcludedWeeklyOffDays = calculation.Data.ExcludedWeeklyOffDays,
                BalanceBeforeRequest = calculation.Data.BalanceBeforeRequest,
                EstimatedBalanceDeduction = calculation.Data.EstimatedBalanceDeduction,
                EstimatedBalanceAfterRequest = calculation.Data.EstimatedBalanceAfterRequest,
                Reason = request.Reason.Trim(),
                ContactAddressDuringLeave = NullIfWhiteSpace(request.ContactAddressDuringLeave),
                ContactNumberDuringLeave = NullIfWhiteSpace(request.ContactNumberDuringLeave),
                HandoverNotes = NullIfWhiteSpace(request.HandoverNotes),
                RequiresReplacement = calculation.Data.RequiresReplacement,
                HasRosterConflict = calculation.Data.HasRosterConflict,
                BalanceSimulationJson = calculation.Data.CalculationSnapshotJson,
                RosterImpactJson = JsonSerializer.Serialize(calculation.Data.Days),
                ValidationResultJson = SerializeValidation(calculation.Data),
                LeaveRequestStatus = LeaveRequestValueConstants.Status.Draft,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveRequestServiceResult<LeaveRequestActionResponse>.Ok(
                MapAction(entity, null, false, false, "Draft pengajuan cuti berhasil dibuat."),
                "Draft pengajuan cuti berhasil dibuat.",
                StatusCodes.Status201Created);
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestActionResponse>> UpdateAsync(
            Guid id,
            Guid actorUserId,
            UpdateLeaveRequestRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(actor.StatusCode, actor.Message);

            var entity = await LoadOwnedRequestAsync(id, actor.Data.WorkforceProfileId, false, cancellationToken);
            if (entity == null)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(StatusCodes.Status404NotFound, "Pengajuan cuti tidak ditemukan.");
            if (!CanEdit(entity.LeaveRequestStatus))
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(StatusCodes.Status409Conflict, "Pengajuan hanya dapat diubah pada status Draft atau NeedRevision.");

            request.ExcludeLeaveRequestId = entity.Id;
            var calculation = await _calculationService.CalculateForActorAsync(actor.Data, request, cancellationToken);
            if (!calculation.Success || calculation.Data == null)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(calculation.StatusCode, calculation.Message);
            if (!calculation.Data.IsValid)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(StatusCodes.Status400BadRequest, string.Join(" | ", calculation.Data.Errors));

            ApplyCalculation(entity, request, calculation.Data);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveRequestServiceResult<LeaveRequestActionResponse>.Ok(
                MapAction(
                    entity,
                    null,
                    await _reservationService.HasActiveReservationAsync(entity.Id, cancellationToken),
                    false,
                    "Draft pengajuan cuti berhasil diperbarui."),
                "Draft pengajuan cuti berhasil diperbarui.");
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestActionResponse>> PrepareWorkflowAsync(
            Guid id,
            Guid actorUserId,
            PrepareLeaveRequestWorkflowRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(actor.StatusCode, actor.Message);

            var entity = await LoadOwnedRequestAsync(id, actor.Data.WorkforceProfileId, true, cancellationToken);
            if (entity == null)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(StatusCodes.Status404NotFound, "Pengajuan cuti tidak ditemukan.");
            if (!CanEdit(entity.LeaveRequestStatus))
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(StatusCodes.Status409Conflict, "Workflow hanya dapat disiapkan pada status Draft atau NeedRevision.");

            if (entity.WorkflowInstanceId.HasValue)
            {
                var existingWorkflow = await _dbContext.Set<TrxWorkflowInstance>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == entity.WorkflowInstanceId.Value && !x.IsDelete, cancellationToken);
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Ok(
                    MapAction(
                        entity,
                        existingWorkflow?.WorkflowStatus,
                        await _reservationService.HasActiveReservationAsync(entity.Id, cancellationToken),
                        true,
                        "Workflow pengajuan sudah tersedia."),
                    "Workflow pengajuan sudah tersedia.");
            }

            var workflowCode = string.IsNullOrWhiteSpace(entity.LeavePolicy?.ApprovalWorkflowCode)
                ? LeaveRequestValueConstants.DefaultWorkflowCode
                : entity.LeavePolicy!.ApprovalWorkflowCode!.Trim();

            var create = await _workflowService.CreateAsync(
                new CreateWorkflowInstanceRequest
                {
                    WorkflowDefinitionCode = workflowCode,
                    ReferenceType = LeaveRequestValueConstants.WorkflowReferenceType,
                    ReferenceId = entity.Id,
                    ExternalReferenceNumber = entity.RequestNumber,
                    SourceChannel = NormalizeSourceChannel(request.SourceChannel),
                    RequestCorrelationId = NullIfWhiteSpace(request.RequestCorrelationId),
                    IdempotencyKey = NullIfWhiteSpace(request.IdempotencyKey) ?? $"LEAVE-WF:{entity.Id:N}",
                    RequestContext = JsonSerializer.SerializeToElement(new
                    {
                        leaveRequestId = entity.Id,
                        entity.RequestNumber,
                        entity.WorkforceProfileId,
                        entity.LeaveTypeId,
                        leaveTypeCode = entity.LeaveType?.LeaveTypeCode,
                        leaveTypeName = entity.LeaveType?.LeaveTypeName,
                        entity.StartDate,
                        entity.EndDate,
                        entity.RequestedDays,
                        entity.EstimatedBalanceDeduction,
                        entity.Reason,
                        attachmentIds = entity.Attachments
                            .Where(x => x.IsActive && !x.IsDelete)
                            .Select(x => x.Id)
                            .ToList(),
                        entity.ReplacementWorkforceProfileId
                    }),
                    SelectedApproverUserIds = request.SelectedApproverUserIds
                        .Where(x => x != Guid.Empty)
                        .Distinct()
                        .ToList()
                },
                cancellationToken);

            if (!create.Success || create.Data == null)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(create.StatusCode, create.Message);

            entity.WorkflowDefinitionId = create.Data.WorkflowDefinitionId;
            entity.WorkflowInstanceId = create.Data.Id;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveRequestServiceResult<LeaveRequestActionResponse>.Ok(
                MapAction(entity, create.Data.WorkflowStatus, false, false, "Workflow pengajuan cuti berhasil disiapkan."),
                "Workflow pengajuan cuti berhasil disiapkan.");
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestActionResponse>> SubmitAsync(
            Guid id,
            Guid actorUserId,
            SubmitLeaveRequestRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(actor.StatusCode, actor.Message);

            var entity = await LoadOwnedRequestAsync(id, actor.Data.WorkforceProfileId, true, cancellationToken);
            if (entity == null)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(StatusCodes.Status404NotFound, "Pengajuan cuti tidak ditemukan.");
            if (!CanEdit(entity.LeaveRequestStatus))
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(StatusCodes.Status409Conflict, "Pengajuan tidak berada pada status yang dapat disubmit.");

            var calculationRequest = new LeaveRequestCalculationRequest
            {
                LeaveTypeId = entity.LeaveTypeId,
                LeaveBalanceId = entity.LeaveBalanceId,
                RequestReasonId = entity.RequestReasonId,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                IsHalfDay = entity.IsHalfDay,
                HalfDayPeriod = entity.HalfDayPeriod,
                IsHourly = entity.IsHourly,
                RequestedMinutes = entity.RequestedMinutes,
                ReplacementWorkforceProfileId = entity.ReplacementWorkforceProfileId,
                ExcludeLeaveRequestId = entity.Id
            };

            var calculation = await _calculationService.CalculateForActorAsync(
                actor.Data,
                calculationRequest,
                cancellationToken);

            if (!calculation.Success || calculation.Data == null)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(calculation.StatusCode, calculation.Message);
            if (!calculation.Data.IsValid)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(StatusCodes.Status400BadRequest, string.Join(" | ", calculation.Data.Errors));

            var activeAttachments = entity.Attachments
                .Where(x => x.IsActive && !x.IsDelete)
                .ToList();

            if (calculation.Data.RequiresAttachment && activeAttachments.Count == 0)
            {
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Dokumen pendukung wajib diunggah sebelum submit.");
            }

            if (calculation.Data.RequiresMedicalCertificate &&
                !activeAttachments.Any(x =>
                    x.AttachmentType == LeaveRequestValueConstants.AttachmentType.MedicalCertificate))
            {
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Surat keterangan medis wajib diunggah sebelum submit.");
            }

            if (!entity.WorkflowInstanceId.HasValue)
            {
                var prepare = await PrepareWorkflowAsync(id, actorUserId, request, cancellationToken);
                if (!prepare.Success) return prepare;

                entity = await LoadOwnedRequestAsync(id, actor.Data.WorkforceProfileId, true, cancellationToken);
                if (entity?.WorkflowInstanceId == null)
                {
                    return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Workflow instance belum tersedia setelah proses preparation.");
                }
            }

            if (entity.LeavePolicy == null)
            {
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Leave policy pengajuan tidak tersedia.");
            }

            entity.RequestedDays = calculation.Data.RequestedDays;
            entity.CalculatedWorkingDays = calculation.Data.CalculatedWorkingDays;
            entity.ExcludedHolidayDays = calculation.Data.ExcludedHolidayDays;
            entity.ExcludedWeeklyOffDays = calculation.Data.ExcludedWeeklyOffDays;
            entity.BalanceBeforeRequest = calculation.Data.BalanceBeforeRequest;
            entity.EstimatedBalanceDeduction = calculation.Data.EstimatedBalanceDeduction;
            entity.EstimatedBalanceAfterRequest = calculation.Data.EstimatedBalanceAfterRequest;
            entity.BalanceSimulationJson = calculation.Data.CalculationSnapshotJson;
            entity.RosterImpactJson = JsonSerializer.Serialize(calculation.Data.Days);
            entity.ValidationResultJson = SerializeValidation(calculation.Data);

            var reservation = await _reservationService.ReserveAsync(
                entity,
                entity.LeavePolicy,
                actorUserId,
                cancellationToken);

            if (!reservation.Success)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(reservation.StatusCode, reservation.Message);

            var workflowSubmit = await _workflowService.SubmitAsync(
                entity.WorkflowInstanceId.Value,
                new WorkflowSubmitRequest
                {
                    Comment = request.Comment,
                    IdempotencyKey = NullIfWhiteSpace(request.IdempotencyKey) ?? $"LEAVE-SUBMIT:{entity.Id:N}"
                },
                cancellationToken);

            if (!workflowSubmit.Success || workflowSubmit.Data == null)
            {
                await _reservationService.ReleaseAsync(
                    entity,
                    actorUserId,
                    "Reservasi dilepas karena submit workflow gagal.",
                    cancellationToken);
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(workflowSubmit.StatusCode, workflowSubmit.Message);
            }

            entity.LeaveRequestStatus = LeaveRequestValueConstants.Status.Submitted;
            entity.SubmittedAt = DateTime.UtcNow;
            entity.SubmittedByUserId = actorUserId;
            entity.CurrentApprovalStep = workflowSubmit.Data.CurrentStepOrder;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveRequestServiceResult<LeaveRequestActionResponse>.Ok(
                MapAction(
                    entity,
                    workflowSubmit.Data.WorkflowStatus,
                    await _reservationService.HasActiveReservationAsync(entity.Id, cancellationToken),
                    false,
                    "Pengajuan cuti berhasil disubmit."),
                "Pengajuan cuti berhasil disubmit.");
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestActionResponse>> CancelAsync(
            Guid id,
            Guid actorUserId,
            CancelLeaveRequestRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(actor.StatusCode, actor.Message);

            var entity = await LoadOwnedRequestAsync(id, actor.Data.WorkforceProfileId, true, cancellationToken);
            if (entity == null)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(StatusCodes.Status404NotFound, "Pengajuan cuti tidak ditemukan.");

            if (entity.LeaveRequestStatus == LeaveRequestValueConstants.Status.Cancelled)
            {
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Ok(
                    MapAction(entity, null, false, true, "Pengajuan sudah dibatalkan."),
                    "Pengajuan sudah dibatalkan.");
            }

            if (entity.LeaveRequestStatus == LeaveRequestValueConstants.Status.Approved ||
                entity.LeaveRequestStatus == LeaveRequestValueConstants.Status.Taken ||
                entity.LeaveRequestStatus == LeaveRequestValueConstants.Status.Completed)
            {
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Pengajuan yang sudah disetujui atau diambil harus menggunakan leave cancellation request.");
            }

            string? workflowStatus = null;
            if (entity.WorkflowInstanceId.HasValue)
            {
                var workflow = await _dbContext.Set<TrxWorkflowInstance>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == entity.WorkflowInstanceId.Value && !x.IsDelete, cancellationToken);

                workflowStatus = workflow?.WorkflowStatus;
                if (workflow != null)
                {
                    if (workflow.WorkflowStatus == "InProgress")
                    {
                        var withdraw = await _workflowService.WithdrawAsync(
                            workflow.Id,
                            new WorkflowWithdrawRequest
                            {
                                Reason = request.Reason,
                                IdempotencyKey = request.IdempotencyKey ?? $"LEAVE-WITHDRAW:{entity.Id:N}"
                            },
                            cancellationToken);
                        if (!withdraw.Success)
                            return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(withdraw.StatusCode, withdraw.Message);
                        workflowStatus = withdraw.Data?.WorkflowStatus;
                    }
                    else if (workflow.WorkflowStatus != "Cancelled" && workflow.WorkflowStatus != "Withdrawn")
                    {
                        var cancel = await _workflowService.CancelAsync(
                            workflow.Id,
                            new WorkflowCancelRequest
                            {
                                Reason = request.Reason,
                                IdempotencyKey = request.IdempotencyKey ?? $"LEAVE-CANCEL:{entity.Id:N}"
                            },
                            cancellationToken);
                        if (!cancel.Success)
                            return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(cancel.StatusCode, cancel.Message);
                        workflowStatus = cancel.Data?.WorkflowStatus;
                    }
                }
            }

            var release = await _reservationService.ReleaseAsync(
                entity,
                actorUserId,
                $"Reservasi dilepas karena pengajuan dibatalkan: {request.Reason}",
                cancellationToken);

            if (!release.Success)
                return LeaveRequestServiceResult<LeaveRequestActionResponse>.Fail(release.StatusCode, release.Message);

            entity.LeaveRequestStatus = LeaveRequestValueConstants.Status.Cancelled;
            entity.CancelledAt = DateTime.UtcNow;
            entity.CancelledByUserId = actorUserId;
            entity.ApprovalNotes = request.Reason;
            entity.IsCancel = true;
            entity.CancelDateTime = DateTime.UtcNow;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveRequestServiceResult<LeaveRequestActionResponse>.Ok(
                MapAction(entity, workflowStatus, false, false, "Pengajuan cuti berhasil dibatalkan."),
                "Pengajuan cuti berhasil dibatalkan.");
        }

        public async Task<LeaveRequestServiceResult<WorkflowInstanceDetailResponse>> GetWorkflowAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<WorkflowInstanceDetailResponse>.Fail(actor.StatusCode, actor.Message);

            var request = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == actor.Data.WorkforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (request?.WorkflowInstanceId == null)
            {
                return LeaveRequestServiceResult<WorkflowInstanceDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow pengajuan cuti belum tersedia.");
            }

            var workflow = await _workflowService.GetByIdAsync(request.WorkflowInstanceId.Value, cancellationToken);
            return workflow.Success
                ? LeaveRequestServiceResult<WorkflowInstanceDetailResponse>.Ok(workflow.Data, workflow.Message)
                : LeaveRequestServiceResult<WorkflowInstanceDetailResponse>.Fail(workflow.StatusCode, workflow.Message);
        }

        public async Task<LeaveRequestServiceResult<bool>> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
                return LeaveRequestServiceResult<bool>.Fail(actor.StatusCode, actor.Message);

            var entity = await LoadOwnedRequestAsync(id, actor.Data.WorkforceProfileId, true, cancellationToken);
            if (entity == null)
                return LeaveRequestServiceResult<bool>.Fail(StatusCodes.Status404NotFound, "Pengajuan cuti tidak ditemukan.");
            if (entity.LeaveRequestStatus != LeaveRequestValueConstants.Status.Draft || entity.WorkflowInstanceId.HasValue)
            {
                return LeaveRequestServiceResult<bool>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya draft yang belum memiliki workflow yang dapat dihapus.");
            }

            foreach (var attachment in entity.Attachments.Where(x => !x.IsDelete))
            {
                attachment.IsActive = false;
                attachment.IsDelete = true;
                attachment.DeleteDateTime = DateTime.UtcNow;
                attachment.DeleteBy = actorUserId;
                await _fileStorageService.DeletePhysicalFileAsync(attachment.FilePath, cancellationToken);
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveRequestServiceResult<bool>.Ok(true, "Draft pengajuan cuti berhasil dihapus.");
        }

        private IQueryable<WfpLeaveRequest> BuildMyQuery(Guid workforceProfileId) =>
            _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete);

        private static IQueryable<WfpLeaveRequest> ApplyFilters(
            IQueryable<WfpLeaveRequest> query,
            LeaveRequestQueryRequest request)
        {
            if (request.StartDate.HasValue) query = query.Where(x => x.EndDate >= request.StartDate.Value);
            if (request.EndDate.HasValue) query = query.Where(x => x.StartDate <= request.EndDate.Value);
            if (request.LeaveTypeId.HasValue) query = query.Where(x => x.LeaveTypeId == request.LeaveTypeId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.LeaveRequestStatus == request.Status);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.RequestNumber.ToLower().Contains(keyword) ||
                    x.Reason.ToLower().Contains(keyword) ||
                    (x.LeaveType != null && x.LeaveType.LeaveTypeName.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IQueryable<WfpLeaveRequest> ApplySort(
            IQueryable<WfpLeaveRequest> query,
            string? sortBy,
            string? direction)
        {
            var desc = !string.Equals(direction, "asc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "startdate" => desc ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate),
                "requestnumber" => desc ? query.OrderByDescending(x => x.RequestNumber) : query.OrderBy(x => x.RequestNumber),
                "leavetypename" => desc ? query.OrderByDescending(x => x.LeaveType!.LeaveTypeName) : query.OrderBy(x => x.LeaveType!.LeaveTypeName),
                "requesteddays" => desc ? query.OrderByDescending(x => x.RequestedDays) : query.OrderBy(x => x.RequestedDays),
                "status" => desc ? query.OrderByDescending(x => x.LeaveRequestStatus) : query.OrderBy(x => x.LeaveRequestStatus),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private async Task<WfpLeaveRequest?> LoadOwnedRequestAsync(
            Guid id,
            Guid workforceProfileId,
            bool includeDetails,
            CancellationToken cancellationToken)
        {
            IQueryable<WfpLeaveRequest> query = _dbContext.Set<WfpLeaveRequest>();
            if (includeDetails)
            {
                query = query
                    .Include(x => x.WorkforceProfile)
                    .Include(x => x.Employee)
                    .Include(x => x.LeaveType)
                    .Include(x => x.LeavePolicy)
                    .Include(x => x.LeaveBalance)
                    .Include(x => x.RequestReason)
                    .Include(x => x.ReplacementWorkforceProfile)
                    .Include(x => x.Attachments);
            }

            return await query.FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.WorkforceProfileId == workforceProfileId &&
                !x.IsDelete,
                cancellationToken);
        }

        private static LeaveRequestListResponse MapList(
            WfpLeaveRequest x,
            string? workflowStatus,
            bool isReserved) =>
            new()
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                LeaveTypeId = x.LeaveTypeId,
                LeaveTypeCode = x.LeaveType?.LeaveTypeCode ?? string.Empty,
                LeaveTypeName = x.LeaveType?.LeaveTypeName ?? string.Empty,
                ColorCode = x.LeaveType?.ColorCode,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                IsHalfDay = x.IsHalfDay,
                IsHourly = x.IsHourly,
                RequestedMinutes = x.RequestedMinutes,
                RequestedDays = x.RequestedDays,
                EstimatedBalanceDeduction = x.EstimatedBalanceDeduction,
                LeaveRequestStatus = x.LeaveRequestStatus,
                WorkflowStatus = workflowStatus,
                HasAttachment = x.Attachments.Any(a => a.IsActive && !a.IsDelete),
                AttachmentCount = x.Attachments.Count(a => a.IsActive && !a.IsDelete),
                IsReserved = isReserved,
                SubmittedAt = x.SubmittedAt,
                CreateDateTime = x.CreateDateTime,
                AvailableActions = ResolveActions(x, workflowStatus)
            };

        private static LeaveRequestDetailResponse MapDetail(
            WfpLeaveRequest x,
            string? workflowStatus,
            bool isReserved)
        {
            var list = MapList(x, workflowStatus, isReserved);
            var response = new LeaveRequestDetailResponse
            {
                Id = list.Id,
                RequestNumber = list.RequestNumber,
                LeaveTypeId = list.LeaveTypeId,
                LeaveTypeCode = list.LeaveTypeCode,
                LeaveTypeName = list.LeaveTypeName,
                ColorCode = list.ColorCode,
                StartDate = list.StartDate,
                EndDate = list.EndDate,
                IsHalfDay = list.IsHalfDay,
                IsHourly = list.IsHourly,
                RequestedMinutes = list.RequestedMinutes,
                RequestedDays = list.RequestedDays,
                EstimatedBalanceDeduction = list.EstimatedBalanceDeduction,
                LeaveRequestStatus = list.LeaveRequestStatus,
                WorkflowStatus = list.WorkflowStatus,
                HasAttachment = list.HasAttachment,
                AttachmentCount = list.AttachmentCount,
                IsReserved = list.IsReserved,
                SubmittedAt = list.SubmittedAt,
                CreateDateTime = list.CreateDateTime,
                AvailableActions = list.AvailableActions,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName,
                EmployeeId = x.EmployeeId,
                LeavePolicyId = x.LeavePolicyId,
                LeavePolicyCode = x.LeavePolicy?.LeavePolicyCode,
                LeavePolicyName = x.LeavePolicy?.LeavePolicyName,
                LeaveBalanceId = x.LeaveBalanceId,
                BalanceBeforeRequest = x.BalanceBeforeRequest,
                EstimatedBalanceAfterRequest = x.EstimatedBalanceAfterRequest,
                ActualBalanceDeduction = x.ActualBalanceDeduction,
                CalculatedWorkingDays = x.CalculatedWorkingDays,
                ExcludedHolidayDays = x.ExcludedHolidayDays,
                ExcludedWeeklyOffDays = x.ExcludedWeeklyOffDays,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                HalfDayPeriod = x.HalfDayPeriod,
                Reason = x.Reason,
                RequestReasonId = x.RequestReasonId,
                RequestReasonName = x.RequestReason?.ReasonName,
                ContactAddressDuringLeave = x.ContactAddressDuringLeave,
                ContactNumberDuringLeave = x.ContactNumberDuringLeave,
                HandoverNotes = x.HandoverNotes,
                RequiresReplacement = x.RequiresReplacement,
                ReplacementWorkforceProfileId = x.ReplacementWorkforceProfileId,
                ReplacementWorkforceName = x.ReplacementWorkforceProfile?.DisplayName,
                HasRosterConflict = x.HasRosterConflict,
                HasTrainingConflict = x.HasTrainingConflict,
                HasCriticalStaffingImpact = x.HasCriticalStaffingImpact,
                BalanceSimulationJson = x.BalanceSimulationJson,
                RosterImpactJson = x.RosterImpactJson,
                ValidationResultJson = x.ValidationResultJson,
                WorkflowDefinitionId = x.WorkflowDefinitionId,
                WorkflowInstanceId = x.WorkflowInstanceId,
                ApprovedAt = x.ApprovedAt,
                RejectedAt = x.RejectedAt,
                CancelledAt = x.CancelledAt,
                Attachments = x.Attachments
                    .Where(a => a.IsActive && !a.IsDelete)
                    .OrderBy(a => a.CreateDateTime)
                    .Select(LeaveRequestAttachmentService.Map)
                    .ToList()
            };

            if (!string.IsNullOrWhiteSpace(x.RosterImpactJson))
            {
                try
                {
                    response.CalculationDays = JsonSerializer.Deserialize<List<LeaveRequestCalculationDayResponse>>(x.RosterImpactJson) ?? new();
                }
                catch
                {
                    response.CalculationDays = new();
                }
            }

            response.Timeline.Add(new LeaveRequestTimelineResponse
            {
                At = x.CreateDateTime,
                EventType = "Created",
                Status = LeaveRequestValueConstants.Status.Draft,
                Description = "Draft pengajuan dibuat."
            });
            if (x.SubmittedAt.HasValue)
                response.Timeline.Add(new LeaveRequestTimelineResponse { At = x.SubmittedAt.Value, EventType = "Submitted", Status = LeaveRequestValueConstants.Status.Submitted, Description = "Pengajuan disubmit ke workflow." });
            if (x.ApprovedAt.HasValue)
                response.Timeline.Add(new LeaveRequestTimelineResponse { At = x.ApprovedAt.Value, EventType = "Approved", Status = LeaveRequestValueConstants.Status.Approved, Description = x.ApprovalNotes });
            if (x.RejectedAt.HasValue)
                response.Timeline.Add(new LeaveRequestTimelineResponse { At = x.RejectedAt.Value, EventType = "Rejected", Status = LeaveRequestValueConstants.Status.Rejected, Description = x.ApprovalNotes });
            if (x.CancelledAt.HasValue)
                response.Timeline.Add(new LeaveRequestTimelineResponse { At = x.CancelledAt.Value, EventType = "Cancelled", Status = LeaveRequestValueConstants.Status.Cancelled, Description = x.ApprovalNotes });

            return response;
        }

        private static List<string> ResolveActions(WfpLeaveRequest x, string? workflowStatus)
        {
            var actions = new List<string> { "View" };
            if (CanEdit(x.LeaveRequestStatus))
            {
                actions.AddRange(new[] { "Update", "UploadAttachment", "Calculate", "Submit" });
            }
            if (x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Draft && !x.WorkflowInstanceId.HasValue)
                actions.Add("Delete");
            if (x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Draft ||
                x.LeaveRequestStatus == LeaveRequestValueConstants.Status.NeedRevision ||
                x.LeaveRequestStatus == LeaveRequestValueConstants.Status.Submitted ||
                x.LeaveRequestStatus == LeaveRequestValueConstants.Status.WaitingApproval)
                actions.Add(workflowStatus == "InProgress" ? "Withdraw" : "Cancel");
            if (x.WorkflowInstanceId.HasValue) actions.Add("ViewWorkflow");
            return actions.Distinct().ToList();
        }

        private static void ApplyCalculation(
            WfpLeaveRequest entity,
            CreateLeaveRequestRequest request,
            LeaveRequestCalculationResponse calculation)
        {
            entity.LeaveTypeId = request.LeaveTypeId;
            entity.LeavePolicyId = calculation.LeavePolicyId;
            entity.LeaveBalanceId = calculation.LeaveBalanceId;
            entity.ReplacementWorkforceProfileId = request.ReplacementWorkforceProfileId;
            entity.RequestReasonId = request.RequestReasonId;
            entity.StartDate = request.StartDate;
            entity.EndDate = request.EndDate;
            entity.StartTime = request.StartTime;
            entity.EndTime = request.EndTime;
            entity.IsHalfDay = request.IsHalfDay;
            entity.HalfDayPeriod = request.HalfDayPeriod;
            entity.IsHourly = request.IsHourly;
            entity.RequestedMinutes = request.RequestedMinutes;
            entity.RequestedDays = calculation.RequestedDays;
            entity.CalculatedWorkingDays = calculation.CalculatedWorkingDays;
            entity.ExcludedHolidayDays = calculation.ExcludedHolidayDays;
            entity.ExcludedWeeklyOffDays = calculation.ExcludedWeeklyOffDays;
            entity.BalanceBeforeRequest = calculation.BalanceBeforeRequest;
            entity.EstimatedBalanceDeduction = calculation.EstimatedBalanceDeduction;
            entity.EstimatedBalanceAfterRequest = calculation.EstimatedBalanceAfterRequest;
            entity.Reason = request.Reason.Trim();
            entity.ContactAddressDuringLeave = NullIfWhiteSpace(request.ContactAddressDuringLeave);
            entity.ContactNumberDuringLeave = NullIfWhiteSpace(request.ContactNumberDuringLeave);
            entity.HandoverNotes = NullIfWhiteSpace(request.HandoverNotes);
            entity.RequiresReplacement = calculation.RequiresReplacement;
            entity.HasRosterConflict = calculation.HasRosterConflict;
            entity.BalanceSimulationJson = calculation.CalculationSnapshotJson;
            entity.RosterImpactJson = JsonSerializer.Serialize(calculation.Days);
            entity.ValidationResultJson = SerializeValidation(calculation);
        }

        private static string SerializeValidation(LeaveRequestCalculationResponse data) =>
            JsonSerializer.Serialize(new
            {
                data.IsValid,
                data.Errors,
                data.Warnings,
                data.RequiresAttachment,
                data.RequiresMedicalCertificate,
                data.RequiresReplacement
            });

        private static LeaveRequestActionResponse MapAction(
            WfpLeaveRequest x,
            string? workflowStatus,
            bool reserved,
            bool idempotent,
            string message) =>
            new()
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                LeaveRequestStatus = x.LeaveRequestStatus,
                WorkflowInstanceId = x.WorkflowInstanceId,
                WorkflowStatus = workflowStatus,
                IsReserved = reserved,
                IsIdempotent = idempotent,
                Message = message
            };

        private static bool CanEdit(string status) =>
            status == LeaveRequestValueConstants.Status.Draft ||
            status == LeaveRequestValueConstants.Status.NeedRevision;

        private static LeaveRequestOptionResponse Option(string value, string label) =>
            new() { Value = value, Label = label };

        private static string GenerateRequestNumber() =>
            $"LR-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        private static string NormalizeSourceChannel(string? value) => value switch
        {
            LeaveRequestValueConstants.SourceChannel.Mobile => value,
            LeaveRequestValueConstants.SourceChannel.Api => value,
            _ => LeaveRequestValueConstants.SourceChannel.Web
        };

        private static string? NullIfWhiteSpace(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
