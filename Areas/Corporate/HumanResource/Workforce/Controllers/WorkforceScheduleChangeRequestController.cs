using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/schedule-change-requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Schedule Change Request",
        AreaName = "Corporate",
        ControllerName = "WorkforceScheduleChangeRequest",
        Description = "Corporate human resource workforce schedule change request",
        SortOrder = 16
    )]
    [Tags("Corporate / Human Resource / Workforce / Schedule Change Request")]
    public class WorkforceScheduleChangeRequestController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceScheduleChangeRequestController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceScheduleChangeRequestListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Schedule Change Request",
            Description = "Melihat data schedule change request workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceScheduleChangeRequest", "Read")]
        public async Task<IActionResult> GetScheduleChangeRequests(
            Guid workforceProfileId,
            [FromQuery] ScheduleRequestApprovalStatus? approvalStatus,
            [FromQuery] ScheduleChangeRequestType? requestType,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool? isActive)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var query = _dbContext.WfpScheduleChangeRequests
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (approvalStatus.HasValue)
            {
                query = query.Where(x => x.ApprovalStatus == approvalStatus.Value);
            }

            if (requestType.HasValue)
            {
                query = query.Where(x => x.RequestType == requestType.Value);
            }

            if (startDate.HasValue)
            {
                var start = DateOnly.FromDateTime(startDate.Value.Date);
                query = query.Where(x => x.RequestedScheduleDate >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateOnly.FromDateTime(endDate.Value.Date);
                query = query.Where(x => x.RequestedScheduleDate <= end);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var entities = await query
                .Include(x => x.CurrentWorkScheduleAssignment)
                    .ThenInclude(x => x.WorkSchedule)
                .Include(x => x.RequestedWorkSchedule)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.RejectedByUser)
                .OrderByDescending(x => x.RequestedAt)
                .ThenByDescending(x => x.CreateDateTime)
                .ToListAsync();

            var items = entities
                .Select(x => MapScheduleChangeResponse(x, profile))
                .ToList();

            var result = new WorkforceScheduleChangeRequestListResponse
            {
                WorkforceProfileId = workforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                PendingData = items.Count(x => x.ApprovalStatus == ScheduleRequestApprovalStatus.Pending),
                ApprovedData = items.Count(x => x.ApprovalStatus == ScheduleRequestApprovalStatus.Approved),
                RejectedData = items.Count(x => x.ApprovalStatus == ScheduleRequestApprovalStatus.Rejected),
                CancelledData = items.Count(x => x.ApprovalStatus == ScheduleRequestApprovalStatus.Cancelled),
                Items = items
            };

            return Ok(ApiResponse<WorkforceScheduleChangeRequestListResponse>.Ok(
                result,
                "Data schedule change request workforce berhasil diambil."
            ));
        }

        [HttpGet("{scheduleChangeRequestId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceScheduleChangeRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Schedule Change Request Detail",
            Description = "Melihat detail schedule change request workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceScheduleChangeRequest", "Read")]
        public async Task<IActionResult> GetScheduleChangeRequestById(
            Guid workforceProfileId,
            Guid scheduleChangeRequestId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.WfpScheduleChangeRequests
                .AsNoTracking()
                .Include(x => x.CurrentWorkScheduleAssignment)
                    .ThenInclude(x => x.WorkSchedule)
                .Include(x => x.RequestedWorkSchedule)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.RejectedByUser)
                .FirstOrDefaultAsync(x =>
                    x.Id == scheduleChangeRequestId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            var data = entity == null
                ? null
                : MapScheduleChangeResponse(entity, profile);

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Schedule change request tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceScheduleChangeRequestResponse>.Ok(
                data,
                "Detail schedule change request workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceScheduleChangeRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Schedule Change Request",
            Description = "Membuat schedule change request workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceScheduleChangeRequest", "Create")]
        public async Task<IActionResult> CreateScheduleChangeRequest(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceScheduleChangeRequest request)
        {
            var validation = await ValidateScheduleChangeRequestAsync(
                workforceProfileId,
                request.CurrentWorkScheduleAssignmentId,
                request.RequestedScheduleDate,
                request.RequestedWorkScheduleId,
                request.RequestType,
                request.Reason
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var requestedDate = DateOnly.FromDateTime(request.RequestedScheduleDate.Date);

            var duplicatePending = await _dbContext.WfpScheduleChangeRequests
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.RequestedScheduleDate == requestedDate &&
                    x.RequestType == request.RequestType &&
                    x.ApprovalStatus == ScheduleRequestApprovalStatus.Pending &&
                    !x.IsDelete);

            if (duplicatePending)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Schedule change request pending dengan tanggal dan tipe yang sama sudah ada."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new WfpScheduleChangeRequest
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CurrentWorkScheduleAssignmentId = request.CurrentWorkScheduleAssignmentId,
                RequestedScheduleDate = requestedDate,
                RequestedWorkScheduleId = request.RequestedWorkScheduleId,
                RequestType = request.RequestType,
                Reason = request.Reason.Trim(),
                ApprovalStatus = ScheduleRequestApprovalStatus.Pending,
                RequestedAt = now,
                ApprovedByUserId = null,
                ApprovedAt = null,
                RejectedByUserId = null,
                RejectedAt = null,
                RejectedReason = null,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.WfpScheduleChangeRequests.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceScheduleChangeRequest.CreateScheduleChangeRequest",
                "Schedule change request workforce berhasil dibuat.",
                new { entity.Id, entity.WorkforceProfileId, entity.RequestType }
            );

            return await GetScheduleChangeRequestById(workforceProfileId, entity.Id);
        }

        [HttpPut("{scheduleChangeRequestId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceScheduleChangeRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Schedule Change Request",
            Description = "Mengubah schedule change request workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceScheduleChangeRequest", "Update")]
        public async Task<IActionResult> UpdateScheduleChangeRequest(
            Guid workforceProfileId,
            Guid scheduleChangeRequestId,
            [FromBody] UpdateWorkforceScheduleChangeRequest request)
        {
            var entity = await _dbContext.WfpScheduleChangeRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == scheduleChangeRequestId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Schedule change request tidak ditemukan."
                ));
            }

            if (entity.ApprovalStatus != ScheduleRequestApprovalStatus.Pending)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Schedule change request hanya bisa diubah saat status Pending."
                ));
            }

            var validation = await ValidateScheduleChangeRequestAsync(
                workforceProfileId,
                request.CurrentWorkScheduleAssignmentId,
                request.RequestedScheduleDate,
                request.RequestedWorkScheduleId,
                request.RequestType,
                request.Reason
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var now = DateTime.UtcNow;

            entity.CurrentWorkScheduleAssignmentId = request.CurrentWorkScheduleAssignmentId;
            entity.RequestedScheduleDate = DateOnly.FromDateTime(request.RequestedScheduleDate.Date);
            entity.RequestedWorkScheduleId = request.RequestedWorkScheduleId;
            entity.RequestType = request.RequestType;
            entity.Reason = request.Reason.Trim();
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return await GetScheduleChangeRequestById(workforceProfileId, entity.Id);
        }

        [HttpPatch("{scheduleChangeRequestId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceScheduleChangeRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Schedule Change Request Status",
            Description = "Mengubah status schedule change request workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceScheduleChangeRequest", "Update")]
        public async Task<IActionResult> UpdateScheduleChangeRequestStatus(
            Guid workforceProfileId,
            Guid scheduleChangeRequestId,
            [FromBody] UpdateWorkforceScheduleChangeStatusRequest request)
        {
            if (request.ApprovalStatus == ScheduleRequestApprovalStatus.Approved)
            {
                return await ApproveScheduleChangeRequest(
                    workforceProfileId,
                    scheduleChangeRequestId,
                    new ApproveWorkforceScheduleChangeRequest()
                );
            }

            if (request.ApprovalStatus == ScheduleRequestApprovalStatus.Rejected)
            {
                return await RejectScheduleChangeRequest(
                    workforceProfileId,
                    scheduleChangeRequestId,
                    new RejectWorkforceScheduleChangeRequest
                    {
                        RejectedReason = request.RejectedReason ?? string.Empty
                    }
                );
            }

            if (request.ApprovalStatus == ScheduleRequestApprovalStatus.Cancelled)
            {
                return await CancelScheduleChangeRequest(
                    workforceProfileId,
                    scheduleChangeRequestId,
                    new CancelWorkforceScheduleChangeRequest
                    {
                        CancelReason = request.RejectedReason
                    }
                );
            }

            var entity = await _dbContext.WfpScheduleChangeRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == scheduleChangeRequestId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Schedule change request tidak ditemukan."
                ));
            }

            entity.ApprovalStatus = request.ApprovalStatus;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return await GetScheduleChangeRequestById(workforceProfileId, entity.Id);
        }

        [HttpPatch("{scheduleChangeRequestId:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceScheduleChangeRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Approve Workforce Schedule Change Request",
            Description = "Menyetujui schedule change request workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 5
        )]
        [AccessPermission("WorkforceScheduleChangeRequest", "Update")]
        public async Task<IActionResult> ApproveScheduleChangeRequest(
            Guid workforceProfileId,
            Guid scheduleChangeRequestId,
            [FromBody] ApproveWorkforceScheduleChangeRequest request)
        {
            var entity = await _dbContext.WfpScheduleChangeRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == scheduleChangeRequestId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Schedule change request tidak ditemukan."
                ));
            }

            if (entity.ApprovalStatus != ScheduleRequestApprovalStatus.Pending)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya schedule change request Pending yang bisa disetujui."
                ));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;
                var actorUserId = GetCurrentUserId();

                if (request.ApplyToScheduleAssignment)
                {
                    var applyResult = await ApplyScheduleChangeAsync(entity, now, actorUserId, request.Notes);

                    if (!applyResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();

                        return BadRequest(ApiResponse<object>.Fail(
                            StatusCodes.Status400BadRequest,
                            applyResult.Message
                        ));
                    }
                }

                entity.ApprovalStatus = ScheduleRequestApprovalStatus.Approved;
                entity.ApprovedByUserId = actorUserId;
                entity.ApprovedAt = now;
                entity.RejectedByUserId = null;
                entity.RejectedAt = null;
                entity.RejectedReason = null;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceScheduleChangeRequest.ApproveScheduleChangeRequest",
                    "Schedule change request workforce berhasil disetujui.",
                    new { entity.Id, entity.WorkforceProfileId, request.ApplyToScheduleAssignment }
                );

                return await GetScheduleChangeRequestById(workforceProfileId, entity.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceScheduleChangeRequest.ApproveScheduleChangeRequest",
                    "Gagal menyetujui schedule change request workforce.",
                    ex,
                    new { workforceProfileId, scheduleChangeRequestId }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Gagal menyetujui schedule change request workforce: {ex.Message}"
                    )
                );
            }
        }

        [HttpPatch("{scheduleChangeRequestId:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceScheduleChangeRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Reject Workforce Schedule Change Request",
            Description = "Menolak schedule change request workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 6
        )]
        [AccessPermission("WorkforceScheduleChangeRequest", "Update")]
        public async Task<IActionResult> RejectScheduleChangeRequest(
            Guid workforceProfileId,
            Guid scheduleChangeRequestId,
            [FromBody] RejectWorkforceScheduleChangeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RejectedReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "RejectedReason wajib diisi."
                ));
            }

            var entity = await _dbContext.WfpScheduleChangeRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == scheduleChangeRequestId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Schedule change request tidak ditemukan."
                ));
            }

            if (entity.ApprovalStatus != ScheduleRequestApprovalStatus.Pending)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya schedule change request Pending yang bisa ditolak."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ApprovalStatus = ScheduleRequestApprovalStatus.Rejected;
            entity.RejectedByUserId = actorUserId;
            entity.RejectedAt = now;
            entity.RejectedReason = request.RejectedReason.Trim();
            entity.ApprovedByUserId = null;
            entity.ApprovedAt = null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return await GetScheduleChangeRequestById(workforceProfileId, entity.Id);
        }

        [HttpPatch("{scheduleChangeRequestId:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceScheduleChangeRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Cancel Workforce Schedule Change Request",
            Description = "Membatalkan schedule change request workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 7
        )]
        [AccessPermission("WorkforceScheduleChangeRequest", "Update")]
        public async Task<IActionResult> CancelScheduleChangeRequest(
            Guid workforceProfileId,
            Guid scheduleChangeRequestId,
            [FromBody] CancelWorkforceScheduleChangeRequest request)
        {
            var entity = await _dbContext.WfpScheduleChangeRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == scheduleChangeRequestId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Schedule change request tidak ditemukan."
                ));
            }

            if (entity.ApprovalStatus != ScheduleRequestApprovalStatus.Pending)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya schedule change request Pending yang bisa dibatalkan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ApprovalStatus = ScheduleRequestApprovalStatus.Cancelled;
            entity.RejectedByUserId = actorUserId;
            entity.RejectedAt = now;
            entity.RejectedReason = NormalizeNullableText(request.CancelReason) ?? "Request dibatalkan.";
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return await GetScheduleChangeRequestById(workforceProfileId, entity.Id);
        }

        [HttpDelete("{scheduleChangeRequestId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Schedule Change Request",
            Description = "Menghapus schedule change request workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 8
        )]
        [AccessPermission("WorkforceScheduleChangeRequest", "Delete")]
        public async Task<IActionResult> DeleteScheduleChangeRequest(
            Guid workforceProfileId,
            Guid scheduleChangeRequestId)
        {
            var entity = await _dbContext.WfpScheduleChangeRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == scheduleChangeRequestId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Schedule change request tidak ditemukan."
                ));
            }

            if (entity.ApprovalStatus == ScheduleRequestApprovalStatus.Approved)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Schedule change request yang sudah Approved tidak boleh dihapus."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id },
                "Schedule change request workforce berhasil dihapus."
            ));
        }


        private static WorkforceScheduleChangeRequestResponse MapScheduleChangeResponse(
            WfpScheduleChangeRequest x,
            WorkforceProfileHeader profile)
        {
            return new WorkforceScheduleChangeRequestResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserType = profile.UserType,
                CurrentWorkScheduleAssignmentId = x.CurrentWorkScheduleAssignmentId,
                CurrentScheduleDate = x.CurrentWorkScheduleAssignment != null
                    ? x.CurrentWorkScheduleAssignment.ScheduleDate.ToDateTime(TimeOnly.MinValue)
                    : null,
                CurrentWorkScheduleId = x.CurrentWorkScheduleAssignment?.WorkScheduleId,
                CurrentWorkScheduleCode = x.CurrentWorkScheduleAssignment?.WorkSchedule?.ScheduleCode,
                CurrentWorkScheduleName = x.CurrentWorkScheduleAssignment?.WorkSchedule?.ScheduleName,
                RequestedScheduleDate = x.RequestedScheduleDate.ToDateTime(TimeOnly.MinValue),
                RequestedWorkScheduleId = x.RequestedWorkScheduleId,
                RequestedWorkScheduleCode = x.RequestedWorkSchedule?.ScheduleCode,
                RequestedWorkScheduleName = x.RequestedWorkSchedule?.ScheduleName,
                RequestType = x.RequestType,
                Reason = x.Reason,
                ApprovalStatus = x.ApprovalStatus,
                RequestedAt = x.RequestedAt,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByUserName = x.ApprovedByUser?.DisplayName,
                ApprovedAt = x.ApprovedAt,
                RejectedByUserId = x.RejectedByUserId,
                RejectedByUserName = x.RejectedByUser?.DisplayName,
                RejectedAt = x.RejectedAt,
                RejectedReason = x.RejectedReason,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private IQueryable<WorkforceScheduleChangeRequestResponse> BuildScheduleChangeResponseQuery(
            WorkforceProfileHeader profile)
        {
            return _dbContext.WfpScheduleChangeRequests
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == profile.Id && !x.IsDelete)
                .Select(x => new WorkforceScheduleChangeRequestResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    CurrentWorkScheduleAssignmentId = x.CurrentWorkScheduleAssignmentId,
                    CurrentScheduleDate = x.CurrentWorkScheduleAssignment != null
                        ? x.CurrentWorkScheduleAssignment.ScheduleDate.ToDateTime(TimeOnly.MinValue)
                        : null,
                    CurrentWorkScheduleId = x.CurrentWorkScheduleAssignment != null
                        ? x.CurrentWorkScheduleAssignment.WorkScheduleId
                        : null,
                    CurrentWorkScheduleCode = x.CurrentWorkScheduleAssignment != null && x.CurrentWorkScheduleAssignment.WorkSchedule != null
                        ? x.CurrentWorkScheduleAssignment.WorkSchedule.ScheduleCode
                        : null,
                    CurrentWorkScheduleName = x.CurrentWorkScheduleAssignment != null && x.CurrentWorkScheduleAssignment.WorkSchedule != null
                        ? x.CurrentWorkScheduleAssignment.WorkSchedule.ScheduleName
                        : null,
                    RequestedScheduleDate = x.RequestedScheduleDate.ToDateTime(TimeOnly.MinValue),
                    RequestedWorkScheduleId = x.RequestedWorkScheduleId,
                    RequestedWorkScheduleCode = x.RequestedWorkSchedule != null ? x.RequestedWorkSchedule.ScheduleCode : null,
                    RequestedWorkScheduleName = x.RequestedWorkSchedule != null ? x.RequestedWorkSchedule.ScheduleName : null,
                    RequestType = x.RequestType,
                    Reason = x.Reason,
                    ApprovalStatus = x.ApprovalStatus,
                    RequestedAt = x.RequestedAt,
                    ApprovedByUserId = x.ApprovedByUserId,
                    ApprovedByUserName = x.ApprovedByUser != null ? x.ApprovedByUser.DisplayName : null,
                    ApprovedAt = x.ApprovedAt,
                    RejectedByUserId = x.RejectedByUserId,
                    RejectedByUserName = x.RejectedByUser != null ? x.RejectedByUser.DisplayName : null,
                    RejectedAt = x.RejectedAt,
                    RejectedReason = x.RejectedReason,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                });
        }

        private async Task<(bool IsValid, string Message)> ValidateScheduleChangeRequestAsync(
            Guid workforceProfileId,
            Guid? currentWorkScheduleAssignmentId,
            DateTime requestedScheduleDate,
            Guid? requestedWorkScheduleId,
            ScheduleChangeRequestType requestType,
            string reason)
        {
            var profileExists = await _dbContext.MstWorkforceProfiles
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (!profileExists)
            {
                return (false, "Workforce profile tidak ditemukan.");
            }

            if (requestedScheduleDate == default)
            {
                return (false, "RequestedScheduleDate wajib diisi.");
            }

            if (requestType == ScheduleChangeRequestType.Unknown)
            {
                return (false, "RequestType wajib dipilih.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return (false, "Reason wajib diisi.");
            }

            if (currentWorkScheduleAssignmentId.HasValue && currentWorkScheduleAssignmentId.Value != Guid.Empty)
            {
                var assignmentExists = await _dbContext.WfpWorkScheduleAssignments
                    .AnyAsync(x =>
                        x.Id == currentWorkScheduleAssignmentId.Value &&
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

                if (!assignmentExists)
                {
                    return (false, "CurrentWorkScheduleAssignmentId tidak ditemukan untuk workforce profile ini.");
                }
            }

            if (requestedWorkScheduleId.HasValue && requestedWorkScheduleId.Value != Guid.Empty)
            {
                var scheduleExists = await _dbContext.MstWorkSchedules
                    .AnyAsync(x =>
                        x.Id == requestedWorkScheduleId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!scheduleExists)
                {
                    return (false, "RequestedWorkScheduleId tidak ditemukan atau tidak aktif.");
                }
            }

            if ((requestType == ScheduleChangeRequestType.ChangeShift ||
                 requestType == ScheduleChangeRequestType.AdditionalShift ||
                 requestType == ScheduleChangeRequestType.ScheduleCorrection) &&
                (!requestedWorkScheduleId.HasValue || requestedWorkScheduleId.Value == Guid.Empty))
            {
                return (false, "RequestedWorkScheduleId wajib diisi untuk ChangeShift, AdditionalShift, atau ScheduleCorrection.");
            }

            return (true, string.Empty);
        }

        private async Task<(bool IsSuccess, string Message)> ApplyScheduleChangeAsync(
            WfpScheduleChangeRequest entity,
            DateTime now,
            Guid actorUserId,
            string? notes)
        {
            var assignment = await ResolveCurrentAssignmentAsync(entity);

            if (entity.RequestType == ScheduleChangeRequestType.ChangeShift ||
                entity.RequestType == ScheduleChangeRequestType.ScheduleCorrection)
            {
                if (!entity.RequestedWorkScheduleId.HasValue)
                {
                    return (false, "RequestedWorkScheduleId wajib diisi untuk apply perubahan shift.");
                }

                if (assignment == null)
                {
                    assignment = BuildScheduleAssignment(
                        entity.WorkforceProfileId,
                        entity.RequestedWorkScheduleId.Value,
                        entity.RequestedScheduleDate,
                        isOffDay: false,
                        isOnCall: false,
                        notes,
                        now,
                        actorUserId
                    );

                    _dbContext.WfpWorkScheduleAssignments.Add(assignment);
                }
                else
                {
                    assignment.WorkScheduleId = entity.RequestedWorkScheduleId.Value;
                    assignment.ScheduleDate = entity.RequestedScheduleDate;
                    assignment.IsOffDay = false;
                    assignment.UpdateDateTime = now;
                    assignment.UpdateBy = actorUserId;
                    assignment.Description = NormalizeNullableText(notes) ?? assignment.Description;
                }

                return (true, string.Empty);
            }

            if (entity.RequestType == ScheduleChangeRequestType.ChangeOffDay)
            {
                if (assignment == null)
                {
                    var fallbackScheduleId = await ResolveFallbackScheduleIdAsync(entity.RequestedWorkScheduleId);

                    if (!fallbackScheduleId.HasValue)
                    {
                        return (false, "Tidak ada work schedule aktif/default untuk membuat off day assignment.");
                    }

                    assignment = BuildScheduleAssignment(
                        entity.WorkforceProfileId,
                        fallbackScheduleId.Value,
                        entity.RequestedScheduleDate,
                        isOffDay: true,
                        isOnCall: false,
                        notes,
                        now,
                        actorUserId
                    );

                    _dbContext.WfpWorkScheduleAssignments.Add(assignment);
                }
                else
                {
                    assignment.IsOffDay = true;
                    assignment.UpdateDateTime = now;
                    assignment.UpdateBy = actorUserId;
                    assignment.Description = NormalizeNullableText(notes) ?? assignment.Description;
                }

                return (true, string.Empty);
            }

            if (entity.RequestType == ScheduleChangeRequestType.OnCallRequest)
            {
                if (assignment == null)
                {
                    var fallbackScheduleId = await ResolveFallbackScheduleIdAsync(entity.RequestedWorkScheduleId);

                    if (!fallbackScheduleId.HasValue)
                    {
                        return (false, "Tidak ada work schedule aktif/default untuk membuat on call assignment.");
                    }

                    assignment = BuildScheduleAssignment(
                        entity.WorkforceProfileId,
                        fallbackScheduleId.Value,
                        entity.RequestedScheduleDate,
                        isOffDay: false,
                        isOnCall: true,
                        notes,
                        now,
                        actorUserId
                    );

                    _dbContext.WfpWorkScheduleAssignments.Add(assignment);
                }
                else
                {
                    assignment.IsOnCall = true;
                    assignment.UpdateDateTime = now;
                    assignment.UpdateBy = actorUserId;
                    assignment.Description = NormalizeNullableText(notes) ?? assignment.Description;
                }

                return (true, string.Empty);
            }

            if (entity.RequestType == ScheduleChangeRequestType.AdditionalShift)
            {
                if (!entity.RequestedWorkScheduleId.HasValue)
                {
                    return (false, "RequestedWorkScheduleId wajib diisi untuk additional shift.");
                }

                var duplicateAssignment = await _dbContext.WfpWorkScheduleAssignments
                    .AnyAsync(x =>
                        x.WorkforceProfileId == entity.WorkforceProfileId &&
                        x.ScheduleDate == entity.RequestedScheduleDate &&
                        x.WorkScheduleId == entity.RequestedWorkScheduleId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (duplicateAssignment)
                {
                    return (false, "Assignment dengan tanggal dan work schedule yang sama sudah ada.");
                }

                var newAssignment = BuildScheduleAssignment(
                    entity.WorkforceProfileId,
                    entity.RequestedWorkScheduleId.Value,
                    entity.RequestedScheduleDate,
                    isOffDay: false,
                    isOnCall: false,
                    notes,
                    now,
                    actorUserId
                );

                _dbContext.WfpWorkScheduleAssignments.Add(newAssignment);

                return (true, string.Empty);
            }

            return (false, "RequestType belum didukung untuk apply ke schedule assignment.");
        }

        private async Task<WfpWorkScheduleAssignment?> ResolveCurrentAssignmentAsync(WfpScheduleChangeRequest entity)
        {
            if (entity.CurrentWorkScheduleAssignmentId.HasValue && entity.CurrentWorkScheduleAssignmentId.Value != Guid.Empty)
            {
                return await _dbContext.WfpWorkScheduleAssignments
                    .FirstOrDefaultAsync(x =>
                        x.Id == entity.CurrentWorkScheduleAssignmentId.Value &&
                        x.WorkforceProfileId == entity.WorkforceProfileId &&
                        !x.IsDelete);
            }

            return await _dbContext.WfpWorkScheduleAssignments
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(x =>
                    x.WorkforceProfileId == entity.WorkforceProfileId &&
                    x.ScheduleDate == entity.RequestedScheduleDate &&
                    x.IsActive &&
                    !x.IsDelete);
        }

        private async Task<Guid?> ResolveFallbackScheduleIdAsync(Guid? requestedWorkScheduleId)
        {
            if (requestedWorkScheduleId.HasValue && requestedWorkScheduleId.Value != Guid.Empty)
            {
                return requestedWorkScheduleId.Value;
            }

            var defaultSchedule = await _dbContext.MstWorkSchedules
                .AsNoTracking()
                .Where(x => x.IsDefault && x.IsActive && !x.IsDelete)
                .OrderByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .FirstOrDefaultAsync();

            return defaultSchedule?.Id;
        }

        private static WfpWorkScheduleAssignment BuildScheduleAssignment(
            Guid workforceProfileId,
            Guid workScheduleId,
            DateOnly scheduleDate,
            bool isOffDay,
            bool isOnCall,
            string? notes,
            DateTime now,
            Guid actorUserId)
        {
            return new WfpWorkScheduleAssignment
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                WorkScheduleId = workScheduleId,
                ScheduleDate = scheduleDate,
                IsOffDay = isOffDay,
                IsOvertimePlanned = false,
                IsOnCall = isOnCall,
                Description = NormalizeNullableText(notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };
        }

        private async Task<WorkforceProfileHeader?> GetWorkforceProfileHeaderAsync(Guid workforceProfileId)
        {
            return await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => x.Id == workforceProfileId && !x.IsDelete)
                .Select(x => new WorkforceProfileHeader
                {
                    Id = x.Id,
                    ProfileCode = x.ProfileCode,
                    DisplayName = x.DisplayName,
                    UserType = x.UserType
                })
                .FirstOrDefaultAsync();
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private class WorkforceProfileHeader
        {
            public Guid Id { get; set; }
            public string ProfileCode { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public UserType UserType { get; set; }
        }
    }
}
