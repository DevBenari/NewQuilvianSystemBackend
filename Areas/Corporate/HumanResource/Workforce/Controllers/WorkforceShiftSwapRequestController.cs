using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Route("api/v1/corporate/human-resource/workforce/shift-swap-requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Shift Swap Request",
        AreaName = "Corporate",
        ControllerName = "WorkforceShiftSwapRequest",
        Description = "Corporate human resource workforce shift swap request",
        SortOrder = 17
    )]
    [Tags("Corporate / Human Resource / Workforce / Shift Swap Request")]
    public class WorkforceShiftSwapRequestController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceShiftSwapRequestController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceShiftSwapRequestListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Shift Swap Request",
            Description = "Melihat data shift swap request workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceShiftSwapRequest", "Read")]
        public async Task<IActionResult> GetShiftSwapRequests(
            [FromQuery] Guid? workforceProfileId,
            [FromQuery] ScheduleRequestApprovalStatus? approvalStatus,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool? isActive)
        {
            var query = _dbContext.WfpShiftSwapRequests
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
            {
                query = query.Where(x =>
                    x.RequesterWorkforceProfileId == workforceProfileId.Value ||
                    x.TargetWorkforceProfileId == workforceProfileId.Value);
            }

            if (approvalStatus.HasValue)
            {
                query = query.Where(x => x.ApprovalStatus == approvalStatus.Value);
            }

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(x => x.RequestedAt >= start);
            }

            if (endDate.HasValue)
            {
                var endExclusive = endDate.Value.Date.AddDays(1);
                query = query.Where(x => x.RequestedAt < endExclusive);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var entities = await query
                .Include(x => x.RequesterWorkforceProfile)
                .Include(x => x.TargetWorkforceProfile)
                .Include(x => x.RequesterScheduleAssignment)
                    .ThenInclude(x => x.WorkSchedule)
                .Include(x => x.TargetScheduleAssignment)
                    .ThenInclude(x => x.WorkSchedule)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.RejectedByUser)
                .OrderByDescending(x => x.RequestedAt)
                .ThenByDescending(x => x.CreateDateTime)
                .ToListAsync();

            var items = entities
                .Select(MapShiftSwapResponse)
                .ToList();

            var result = new WorkforceShiftSwapRequestListResponse
            {
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                PendingData = items.Count(x => x.ApprovalStatus == ScheduleRequestApprovalStatus.Pending),
                ApprovedData = items.Count(x => x.ApprovalStatus == ScheduleRequestApprovalStatus.Approved),
                RejectedData = items.Count(x => x.ApprovalStatus == ScheduleRequestApprovalStatus.Rejected),
                CancelledData = items.Count(x => x.ApprovalStatus == ScheduleRequestApprovalStatus.Cancelled),
                Items = items
            };

            return Ok(ApiResponse<WorkforceShiftSwapRequestListResponse>.Ok(
                result,
                "Data shift swap request workforce berhasil diambil."
            ));
        }

        [HttpGet("{shiftSwapRequestId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceShiftSwapRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Shift Swap Request Detail",
            Description = "Melihat detail shift swap request workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceShiftSwapRequest", "Read")]
        public async Task<IActionResult> GetShiftSwapRequestById(Guid shiftSwapRequestId)
        {
            var entity = await _dbContext.WfpShiftSwapRequests
                .AsNoTracking()
                .Include(x => x.RequesterWorkforceProfile)
                .Include(x => x.TargetWorkforceProfile)
                .Include(x => x.RequesterScheduleAssignment)
                    .ThenInclude(x => x.WorkSchedule)
                .Include(x => x.TargetScheduleAssignment)
                    .ThenInclude(x => x.WorkSchedule)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.RejectedByUser)
                .FirstOrDefaultAsync(x => x.Id == shiftSwapRequestId && !x.IsDelete);

            var data = entity == null
                ? null
                : MapShiftSwapResponse(entity);

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Shift swap request tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceShiftSwapRequestResponse>.Ok(
                data,
                "Detail shift swap request workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceShiftSwapRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Shift Swap Request",
            Description = "Membuat shift swap request workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceShiftSwapRequest", "Create")]
        public async Task<IActionResult> CreateShiftSwapRequest([FromBody] CreateWorkforceShiftSwapRequest request)
        {
            var validation = await ValidateShiftSwapRequestAsync(
                request.RequesterWorkforceProfileId,
                request.TargetWorkforceProfileId,
                request.RequesterScheduleAssignmentId,
                request.TargetScheduleAssignmentId,
                request.Reason
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var duplicatePending = await _dbContext.WfpShiftSwapRequests
                .AnyAsync(x =>
                    x.RequesterScheduleAssignmentId == request.RequesterScheduleAssignmentId &&
                    x.TargetScheduleAssignmentId == request.TargetScheduleAssignmentId &&
                    x.ApprovalStatus == ScheduleRequestApprovalStatus.Pending &&
                    !x.IsDelete);

            if (duplicatePending)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Shift swap request pending untuk assignment yang sama sudah ada."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new WfpShiftSwapRequest
            {
                Id = Guid.NewGuid(),
                RequesterWorkforceProfileId = request.RequesterWorkforceProfileId,
                TargetWorkforceProfileId = request.TargetWorkforceProfileId,
                RequesterScheduleAssignmentId = request.RequesterScheduleAssignmentId,
                TargetScheduleAssignmentId = request.TargetScheduleAssignmentId,
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

            _dbContext.WfpShiftSwapRequests.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceShiftSwapRequest.CreateShiftSwapRequest",
                "Shift swap request workforce berhasil dibuat.",
                new { entity.Id, entity.RequesterWorkforceProfileId, entity.TargetWorkforceProfileId }
            );

            return await GetShiftSwapRequestById(entity.Id);
        }

        [HttpPut("{shiftSwapRequestId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceShiftSwapRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Shift Swap Request",
            Description = "Mengubah shift swap request workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceShiftSwapRequest", "Update")]
        public async Task<IActionResult> UpdateShiftSwapRequest(
            Guid shiftSwapRequestId,
            [FromBody] UpdateWorkforceShiftSwapRequest request)
        {
            var entity = await _dbContext.WfpShiftSwapRequests
                .FirstOrDefaultAsync(x => x.Id == shiftSwapRequestId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Shift swap request tidak ditemukan."
                ));
            }

            if (entity.ApprovalStatus != ScheduleRequestApprovalStatus.Pending)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Shift swap request hanya bisa diubah saat status Pending."
                ));
            }

            var validation = await ValidateShiftSwapRequestAsync(
                request.RequesterWorkforceProfileId,
                request.TargetWorkforceProfileId,
                request.RequesterScheduleAssignmentId,
                request.TargetScheduleAssignmentId,
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

            entity.RequesterWorkforceProfileId = request.RequesterWorkforceProfileId;
            entity.TargetWorkforceProfileId = request.TargetWorkforceProfileId;
            entity.RequesterScheduleAssignmentId = request.RequesterScheduleAssignmentId;
            entity.TargetScheduleAssignmentId = request.TargetScheduleAssignmentId;
            entity.Reason = request.Reason.Trim();
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return await GetShiftSwapRequestById(entity.Id);
        }

        [HttpPatch("{shiftSwapRequestId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceShiftSwapRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Shift Swap Request Status",
            Description = "Mengubah status shift swap request workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceShiftSwapRequest", "Update")]
        public async Task<IActionResult> UpdateShiftSwapRequestStatus(
            Guid shiftSwapRequestId,
            [FromBody] UpdateWorkforceShiftSwapStatusRequest request)
        {
            if (request.ApprovalStatus == ScheduleRequestApprovalStatus.Approved)
            {
                return await ApproveShiftSwapRequest(
                    shiftSwapRequestId,
                    new ApproveWorkforceShiftSwapRequest()
                );
            }

            if (request.ApprovalStatus == ScheduleRequestApprovalStatus.Rejected)
            {
                return await RejectShiftSwapRequest(
                    shiftSwapRequestId,
                    new RejectWorkforceShiftSwapRequest
                    {
                        RejectedReason = request.RejectedReason ?? string.Empty
                    }
                );
            }

            if (request.ApprovalStatus == ScheduleRequestApprovalStatus.Cancelled)
            {
                return await CancelShiftSwapRequest(
                    shiftSwapRequestId,
                    new CancelWorkforceShiftSwapRequest
                    {
                        CancelReason = request.RejectedReason
                    }
                );
            }

            var entity = await _dbContext.WfpShiftSwapRequests
                .FirstOrDefaultAsync(x => x.Id == shiftSwapRequestId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Shift swap request tidak ditemukan."
                ));
            }

            entity.ApprovalStatus = request.ApprovalStatus;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return await GetShiftSwapRequestById(entity.Id);
        }

        [HttpPatch("{shiftSwapRequestId:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceShiftSwapRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Approve Workforce Shift Swap Request",
            Description = "Menyetujui shift swap request workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 5
        )]
        [AccessPermission("WorkforceShiftSwapRequest", "Update")]
        public async Task<IActionResult> ApproveShiftSwapRequest(
            Guid shiftSwapRequestId,
            [FromBody] ApproveWorkforceShiftSwapRequest request)
        {
            var entity = await _dbContext.WfpShiftSwapRequests
                .FirstOrDefaultAsync(x => x.Id == shiftSwapRequestId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Shift swap request tidak ditemukan."
                ));
            }

            if (entity.ApprovalStatus != ScheduleRequestApprovalStatus.Pending)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya shift swap request Pending yang bisa disetujui."
                ));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;
                var actorUserId = GetCurrentUserId();

                if (request.ApplyToScheduleAssignment)
                {
                    var applyResult = await ApplyShiftSwapAsync(entity, now, actorUserId, request.Notes);

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
                    "WorkforceShiftSwapRequest.ApproveShiftSwapRequest",
                    "Shift swap request workforce berhasil disetujui.",
                    new { entity.Id, request.ApplyToScheduleAssignment }
                );

                return await GetShiftSwapRequestById(entity.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceShiftSwapRequest.ApproveShiftSwapRequest",
                    "Gagal menyetujui shift swap request workforce.",
                    ex,
                    new { shiftSwapRequestId }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Gagal menyetujui shift swap request workforce: {ex.Message}"
                    )
                );
            }
        }

        [HttpPatch("{shiftSwapRequestId:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceShiftSwapRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Reject Workforce Shift Swap Request",
            Description = "Menolak shift swap request workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 6
        )]
        [AccessPermission("WorkforceShiftSwapRequest", "Update")]
        public async Task<IActionResult> RejectShiftSwapRequest(
            Guid shiftSwapRequestId,
            [FromBody] RejectWorkforceShiftSwapRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RejectedReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "RejectedReason wajib diisi."
                ));
            }

            var entity = await _dbContext.WfpShiftSwapRequests
                .FirstOrDefaultAsync(x => x.Id == shiftSwapRequestId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Shift swap request tidak ditemukan."
                ));
            }

            if (entity.ApprovalStatus != ScheduleRequestApprovalStatus.Pending)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya shift swap request Pending yang bisa ditolak."
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

            return await GetShiftSwapRequestById(entity.Id);
        }

        [HttpPatch("{shiftSwapRequestId:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceShiftSwapRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Cancel Workforce Shift Swap Request",
            Description = "Membatalkan shift swap request workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 7
        )]
        [AccessPermission("WorkforceShiftSwapRequest", "Update")]
        public async Task<IActionResult> CancelShiftSwapRequest(
            Guid shiftSwapRequestId,
            [FromBody] CancelWorkforceShiftSwapRequest request)
        {
            var entity = await _dbContext.WfpShiftSwapRequests
                .FirstOrDefaultAsync(x => x.Id == shiftSwapRequestId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Shift swap request tidak ditemukan."
                ));
            }

            if (entity.ApprovalStatus != ScheduleRequestApprovalStatus.Pending)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya shift swap request Pending yang bisa dibatalkan."
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

            return await GetShiftSwapRequestById(entity.Id);
        }

        [HttpDelete("{shiftSwapRequestId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Shift Swap Request",
            Description = "Menghapus shift swap request workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 8
        )]
        [AccessPermission("WorkforceShiftSwapRequest", "Delete")]
        public async Task<IActionResult> DeleteShiftSwapRequest(Guid shiftSwapRequestId)
        {
            var entity = await _dbContext.WfpShiftSwapRequests
                .FirstOrDefaultAsync(x => x.Id == shiftSwapRequestId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Shift swap request tidak ditemukan."
                ));
            }

            if (entity.ApprovalStatus == ScheduleRequestApprovalStatus.Approved)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Shift swap request yang sudah Approved tidak boleh dihapus."
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
                "Shift swap request workforce berhasil dihapus."
            ));
        }


        private static WorkforceShiftSwapRequestResponse MapShiftSwapResponse(WfpShiftSwapRequest x)
        {
            return new WorkforceShiftSwapRequestResponse
            {
                Id = x.Id,
                RequesterWorkforceProfileId = x.RequesterWorkforceProfileId,
                RequesterProfileCode = x.RequesterWorkforceProfile?.ProfileCode ?? string.Empty,
                RequesterDisplayName = x.RequesterWorkforceProfile?.DisplayName ?? string.Empty,
                RequesterUserType = x.RequesterWorkforceProfile?.UserType ?? UserType.Employee,
                TargetWorkforceProfileId = x.TargetWorkforceProfileId,
                TargetProfileCode = x.TargetWorkforceProfile?.ProfileCode ?? string.Empty,
                TargetDisplayName = x.TargetWorkforceProfile?.DisplayName ?? string.Empty,
                TargetUserType = x.TargetWorkforceProfile?.UserType ?? UserType.Employee,
                RequesterScheduleAssignmentId = x.RequesterScheduleAssignmentId,
                RequesterScheduleDate = x.RequesterScheduleAssignment != null
                    ? x.RequesterScheduleAssignment.ScheduleDate.ToDateTime(TimeOnly.MinValue)
                    : null,
                RequesterWorkScheduleId = x.RequesterScheduleAssignment?.WorkScheduleId,
                RequesterWorkScheduleCode = x.RequesterScheduleAssignment?.WorkSchedule?.ScheduleCode,
                RequesterWorkScheduleName = x.RequesterScheduleAssignment?.WorkSchedule?.ScheduleName,
                TargetScheduleAssignmentId = x.TargetScheduleAssignmentId,
                TargetScheduleDate = x.TargetScheduleAssignment != null
                    ? x.TargetScheduleAssignment.ScheduleDate.ToDateTime(TimeOnly.MinValue)
                    : null,
                TargetWorkScheduleId = x.TargetScheduleAssignment?.WorkScheduleId,
                TargetWorkScheduleCode = x.TargetScheduleAssignment?.WorkSchedule?.ScheduleCode,
                TargetWorkScheduleName = x.TargetScheduleAssignment?.WorkSchedule?.ScheduleName,
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

        private IQueryable<WorkforceShiftSwapRequestResponse> BuildShiftSwapResponseQuery()
        {
            return _dbContext.WfpShiftSwapRequests
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => new WorkforceShiftSwapRequestResponse
                {
                    Id = x.Id,
                    RequesterWorkforceProfileId = x.RequesterWorkforceProfileId,
                    RequesterProfileCode = x.RequesterWorkforceProfile != null ? x.RequesterWorkforceProfile.ProfileCode : string.Empty,
                    RequesterDisplayName = x.RequesterWorkforceProfile != null ? x.RequesterWorkforceProfile.DisplayName : string.Empty,
                    RequesterUserType = x.RequesterWorkforceProfile != null ? x.RequesterWorkforceProfile.UserType : UserType.Employee,
                    TargetWorkforceProfileId = x.TargetWorkforceProfileId,
                    TargetProfileCode = x.TargetWorkforceProfile != null ? x.TargetWorkforceProfile.ProfileCode : string.Empty,
                    TargetDisplayName = x.TargetWorkforceProfile != null ? x.TargetWorkforceProfile.DisplayName : string.Empty,
                    TargetUserType = x.TargetWorkforceProfile != null ? x.TargetWorkforceProfile.UserType : UserType.Employee,
                    RequesterScheduleAssignmentId = x.RequesterScheduleAssignmentId,
                    RequesterScheduleDate = x.RequesterScheduleAssignment != null
                        ? x.RequesterScheduleAssignment.ScheduleDate.ToDateTime(TimeOnly.MinValue)
                        : null,
                    RequesterWorkScheduleId = x.RequesterScheduleAssignment != null
                        ? x.RequesterScheduleAssignment.WorkScheduleId
                        : null,
                    RequesterWorkScheduleCode = x.RequesterScheduleAssignment != null && x.RequesterScheduleAssignment.WorkSchedule != null
                        ? x.RequesterScheduleAssignment.WorkSchedule.ScheduleCode
                        : null,
                    RequesterWorkScheduleName = x.RequesterScheduleAssignment != null && x.RequesterScheduleAssignment.WorkSchedule != null
                        ? x.RequesterScheduleAssignment.WorkSchedule.ScheduleName
                        : null,
                    TargetScheduleAssignmentId = x.TargetScheduleAssignmentId,
                    TargetScheduleDate = x.TargetScheduleAssignment != null
                        ? x.TargetScheduleAssignment.ScheduleDate.ToDateTime(TimeOnly.MinValue)
                        : null,
                    TargetWorkScheduleId = x.TargetScheduleAssignment != null
                        ? x.TargetScheduleAssignment.WorkScheduleId
                        : null,
                    TargetWorkScheduleCode = x.TargetScheduleAssignment != null && x.TargetScheduleAssignment.WorkSchedule != null
                        ? x.TargetScheduleAssignment.WorkSchedule.ScheduleCode
                        : null,
                    TargetWorkScheduleName = x.TargetScheduleAssignment != null && x.TargetScheduleAssignment.WorkSchedule != null
                        ? x.TargetScheduleAssignment.WorkSchedule.ScheduleName
                        : null,
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

        private async Task<(bool IsValid, string Message)> ValidateShiftSwapRequestAsync(
            Guid requesterWorkforceProfileId,
            Guid targetWorkforceProfileId,
            Guid requesterScheduleAssignmentId,
            Guid targetScheduleAssignmentId,
            string reason)
        {
            if (requesterWorkforceProfileId == Guid.Empty)
            {
                return (false, "RequesterWorkforceProfileId wajib diisi.");
            }

            if (targetWorkforceProfileId == Guid.Empty)
            {
                return (false, "TargetWorkforceProfileId wajib diisi.");
            }

            if (requesterWorkforceProfileId == targetWorkforceProfileId)
            {
                return (false, "Requester dan target workforce tidak boleh sama.");
            }

            if (requesterScheduleAssignmentId == Guid.Empty)
            {
                return (false, "RequesterScheduleAssignmentId wajib diisi.");
            }

            if (targetScheduleAssignmentId == Guid.Empty)
            {
                return (false, "TargetScheduleAssignmentId wajib diisi.");
            }

            if (requesterScheduleAssignmentId == targetScheduleAssignmentId)
            {
                return (false, "Requester dan target schedule assignment tidak boleh sama.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return (false, "Reason wajib diisi.");
            }

            var requesterExists = await _dbContext.MstWorkforceProfiles
                .AnyAsync(x => x.Id == requesterWorkforceProfileId && !x.IsDelete);

            if (!requesterExists)
            {
                return (false, "Requester workforce profile tidak ditemukan.");
            }

            var targetExists = await _dbContext.MstWorkforceProfiles
                .AnyAsync(x => x.Id == targetWorkforceProfileId && !x.IsDelete);

            if (!targetExists)
            {
                return (false, "Target workforce profile tidak ditemukan.");
            }

            var requesterAssignmentExists = await _dbContext.WfpWorkScheduleAssignments
                .AnyAsync(x =>
                    x.Id == requesterScheduleAssignmentId &&
                    x.WorkforceProfileId == requesterWorkforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!requesterAssignmentExists)
            {
                return (false, "RequesterScheduleAssignmentId tidak ditemukan atau tidak sesuai requester workforce.");
            }

            var targetAssignmentExists = await _dbContext.WfpWorkScheduleAssignments
                .AnyAsync(x =>
                    x.Id == targetScheduleAssignmentId &&
                    x.WorkforceProfileId == targetWorkforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!targetAssignmentExists)
            {
                return (false, "TargetScheduleAssignmentId tidak ditemukan atau tidak sesuai target workforce.");
            }

            return (true, string.Empty);
        }

        private async Task<(bool IsSuccess, string Message)> ApplyShiftSwapAsync(
            WfpShiftSwapRequest entity,
            DateTime now,
            Guid actorUserId,
            string? notes)
        {
            var requesterAssignment = await _dbContext.WfpWorkScheduleAssignments
                .FirstOrDefaultAsync(x =>
                    x.Id == entity.RequesterScheduleAssignmentId &&
                    x.WorkforceProfileId == entity.RequesterWorkforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (requesterAssignment == null)
            {
                return (false, "Requester schedule assignment tidak ditemukan atau tidak aktif.");
            }

            var targetAssignment = await _dbContext.WfpWorkScheduleAssignments
                .FirstOrDefaultAsync(x =>
                    x.Id == entity.TargetScheduleAssignmentId &&
                    x.WorkforceProfileId == entity.TargetWorkforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (targetAssignment == null)
            {
                return (false, "Target schedule assignment tidak ditemukan atau tidak aktif.");
            }

            if (requesterAssignment.ScheduleDate != targetAssignment.ScheduleDate)
            {
                return (false, "Apply shift swap otomatis hanya didukung untuk assignment pada tanggal yang sama. Jika beda tanggal, approve tanpa apply lalu lakukan koreksi jadwal manual.");
            }

            var requesterWorkScheduleId = requesterAssignment.WorkScheduleId;
            var requesterIsOffDay = requesterAssignment.IsOffDay;
            var requesterIsOvertimePlanned = requesterAssignment.IsOvertimePlanned;
            var requesterIsOnCall = requesterAssignment.IsOnCall;

            requesterAssignment.WorkScheduleId = targetAssignment.WorkScheduleId;
            requesterAssignment.IsOffDay = targetAssignment.IsOffDay;
            requesterAssignment.IsOvertimePlanned = targetAssignment.IsOvertimePlanned;
            requesterAssignment.IsOnCall = targetAssignment.IsOnCall;
            requesterAssignment.Description = NormalizeNullableText(notes) ?? requesterAssignment.Description;
            requesterAssignment.UpdateDateTime = now;
            requesterAssignment.UpdateBy = actorUserId;

            targetAssignment.WorkScheduleId = requesterWorkScheduleId;
            targetAssignment.IsOffDay = requesterIsOffDay;
            targetAssignment.IsOvertimePlanned = requesterIsOvertimePlanned;
            targetAssignment.IsOnCall = requesterIsOnCall;
            targetAssignment.Description = NormalizeNullableText(notes) ?? targetAssignment.Description;
            targetAssignment.UpdateDateTime = now;
            targetAssignment.UpdateBy = actorUserId;

            return (true, string.Empty);
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
    }
}