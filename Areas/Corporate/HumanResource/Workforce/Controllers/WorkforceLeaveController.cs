using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/leaves")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Leave",
        AreaName = "Corporate",
        ControllerName = "WorkforceLeave",
        Description = "Workforce leave balance and leave request management",
        SortOrder = 35
    )]
    [Tags("Corporate / Human Resource / Workforce / Leave")]
    public class WorkforceLeaveController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.Leave";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceLeaveController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("balances")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceLeaveBalanceListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Leave",
            Description = "Melihat saldo cuti workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceLeave", "Read")]
        public async Task<IActionResult> GetLeaveBalances(
            Guid workforceProfileId,
            [FromQuery] int? leaveYear,
            [FromQuery] LeaveType? leaveType,
            [FromQuery] bool? isActive)
        {
            var profile = await GetWorkforceProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var query = _dbContext.Set<WfpLeaveBalance>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (leaveYear.HasValue)
            {
                query = query.Where(x => x.LeaveYear == leaveYear.Value);
            }

            if (leaveType.HasValue)
            {
                query = query.Where(x => x.LeaveType == leaveType.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var items = await query
                .OrderByDescending(x => x.LeaveYear)
                .ThenBy(x => x.LeaveType)
                .Select(x => new WorkforceLeaveBalanceResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    LeaveYear = x.LeaveYear,
                    LeaveType = x.LeaveType,
                    OpeningBalance = x.OpeningBalance,
                    EntitledDays = x.EntitledDays,
                    UsedDays = x.UsedDays,
                    PendingDays = x.PendingDays,
                    RemainingDays = x.RemainingDays,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceLeaveBalanceListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                TotalEntitledDays = items.Sum(x => x.EntitledDays),
                TotalUsedDays = items.Sum(x => x.UsedDays),
                TotalPendingDays = items.Sum(x => x.PendingDays),
                TotalRemainingDays = items.Sum(x => x.RemainingDays),
                Items = items
            };

            return Ok(ApiResponse<WorkforceLeaveBalanceListResponse>.Ok(
                result,
                "Data saldo cuti workforce berhasil diambil."
            ));
        }

        [HttpGet("requests")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceLeaveRequestListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Leave",
            Description = "Melihat pengajuan cuti workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceLeave", "Read")]
        public async Task<IActionResult> GetLeaveRequests(
            Guid workforceProfileId,
            [FromQuery] int? leaveYear,
            [FromQuery] LeaveType? leaveType,
            [FromQuery] LeaveApprovalStatus? approvalStatus,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var profile = await GetWorkforceProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var query = _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (leaveYear.HasValue)
            {
                query = query.Where(x => x.StartDate.Year == leaveYear.Value);
            }

            if (leaveType.HasValue)
            {
                query = query.Where(x => x.LeaveType == leaveType.Value);
            }

            if (approvalStatus.HasValue)
            {
                query = query.Where(x => x.ApprovalStatus == approvalStatus.Value);
            }

            if (startDate.HasValue)
            {
                var date = startDate.Value.Date;
                query = query.Where(x => x.EndDate.Date >= date);
            }

            if (endDate.HasValue)
            {
                var date = endDate.Value.Date;
                query = query.Where(x => x.StartDate.Date <= date);
            }

            var items = await query
                .OrderByDescending(x => x.RequestedAt)
                .ThenByDescending(x => x.StartDate)
                .Select(x => new WorkforceLeaveRequestResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    LeaveBalanceId = x.LeaveBalanceId,
                    LeaveType = x.LeaveType,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    TotalDays = x.TotalDays,
                    IsHalfDay = x.IsHalfDay,
                    IsDeductBalance = x.IsDeductBalance,
                    Reason = x.Reason,
                    ApprovalStatus = x.ApprovalStatus,
                    RequestedAt = x.RequestedAt,                    
                    ApprovedAt = x.ApprovedAt,
                    ApprovalNote = x.ApprovalNote,                    
                    RejectedAt = x.RejectedAt,
                    RejectedReason = x.RejectedReason,                    
                    CancelledAt = x.CancelledAt,
                    CancelReason = x.CancelReason,
                    AttachmentPath = x.AttachmentPath,
                    AttachmentContentType = x.AttachmentContentType,
                    HasAttachment = !string.IsNullOrWhiteSpace(x.AttachmentPath),
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceLeaveRequestListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                PendingData = items.Count(x => x.ApprovalStatus == LeaveApprovalStatus.PendingApproval),
                ApprovedData = items.Count(x => x.ApprovalStatus == LeaveApprovalStatus.Approved),
                RejectedData = items.Count(x => x.ApprovalStatus == LeaveApprovalStatus.Rejected),
                CancelledData = items.Count(x => x.ApprovalStatus == LeaveApprovalStatus.Cancelled),
                TotalRequestedDays = items.Sum(x => x.TotalDays),
                ApprovedDays = items
                    .Where(x => x.ApprovalStatus == LeaveApprovalStatus.Approved)
                    .Sum(x => x.TotalDays),
                PendingDays = items
                    .Where(x => x.ApprovalStatus == LeaveApprovalStatus.PendingApproval)
                    .Sum(x => x.TotalDays),
                Items = items
            };

            return Ok(ApiResponse<WorkforceLeaveRequestListResponse>.Ok(
                result,
                "Data pengajuan cuti workforce berhasil diambil."
            ));
        }

        [HttpPost("balances")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceLeaveBalanceResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Leave",
            Description = "Membuat saldo cuti workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceLeave", "Create")]
        public async Task<IActionResult> CreateLeaveBalance(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceLeaveBalanceRequest request)
        {
            var profileExists = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (!profileExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = await ValidateLeaveBalanceRequestAsync(
                workforceProfileId,
                request.LeaveYear,
                request.LeaveType,
                request.OpeningBalance,
                request.EntitledDays,
                request.UsedDays,
                request.PendingDays,
                request.EffectiveStartDate,
                request.EffectiveEndDate,
                excludeLeaveBalanceId: null);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data saldo cuti tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var remainingDays = CalculateRemainingDays(
                request.OpeningBalance,
                request.EntitledDays,
                request.UsedDays,
                request.PendingDays);

            var entity = new WfpLeaveBalance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                LeaveYear = request.LeaveYear,
                LeaveType = request.LeaveType,
                OpeningBalance = request.OpeningBalance,
                EntitledDays = request.EntitledDays,
                UsedDays = request.UsedDays,
                PendingDays = request.PendingDays,
                RemainingDays = remainingDays,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                IsActive = request.IsActive,
                Description = NormalizeNullableText(request.Description),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpLeaveBalance>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildLeaveBalanceResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceLeave.CreateLeaveBalance",
                "Saldo cuti workforce berhasil dibuat.",
                new
                {
                    workforceProfileId,
                    entity.Id,
                    entity.LeaveYear,
                    entity.LeaveType,
                    entity.EntitledDays,
                    entity.RemainingDays
                }
            );

            return Ok(ApiResponse<WorkforceLeaveBalanceResponse>.Ok(
                response!,
                "Saldo cuti workforce berhasil dibuat."
            ));
        }

        [HttpPut("balances/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceLeaveBalanceResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Leave",
            Description = "Mengubah saldo cuti workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceLeave", "Update")]
        public async Task<IActionResult> UpdateLeaveBalance(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceLeaveBalanceRequest request)
        {
            var entity = await _dbContext.Set<WfpLeaveBalance>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Saldo cuti workforce tidak ditemukan."
                ));
            }

            var hasActiveRequests = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.LeaveBalanceId == id &&
                    !x.IsDelete &&
                    (x.ApprovalStatus == LeaveApprovalStatus.PendingApproval ||
                     x.ApprovalStatus == LeaveApprovalStatus.Approved));

            if (hasActiveRequests &&
                (entity.LeaveYear != request.LeaveYear || entity.LeaveType != request.LeaveType))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "LeaveYear dan LeaveType tidak boleh diubah karena saldo cuti sudah memiliki transaksi aktif."
                ));
            }

            var validation = await ValidateLeaveBalanceRequestAsync(
                workforceProfileId,
                request.LeaveYear,
                request.LeaveType,
                request.OpeningBalance,
                request.EntitledDays,
                request.UsedDays,
                request.PendingDays,
                request.EffectiveStartDate,
                request.EffectiveEndDate,
                excludeLeaveBalanceId: id);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data saldo cuti tidak valid."
                ));
            }

            var remainingDays = CalculateRemainingDays(
                request.OpeningBalance,
                request.EntitledDays,
                request.UsedDays,
                request.PendingDays);

            entity.LeaveYear = request.LeaveYear;
            entity.LeaveType = request.LeaveType;
            entity.OpeningBalance = request.OpeningBalance;
            entity.EntitledDays = request.EntitledDays;
            entity.UsedDays = request.UsedDays;
            entity.PendingDays = request.PendingDays;
            entity.RemainingDays = remainingDays;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildLeaveBalanceResponseAsync(entity.Id);

            return Ok(ApiResponse<WorkforceLeaveBalanceResponse>.Ok(
                response!,
                "Saldo cuti workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("balances/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Leave",
            Description = "Mengubah status saldo cuti workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceLeave", "Update")]
        public async Task<IActionResult> UpdateLeaveBalanceStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceLeaveBalanceStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpLeaveBalance>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Saldo cuti workforce tidak ditemukan."
                ));
            }

            if (!request.IsActive)
            {
                var hasPendingRequest = await _dbContext.Set<WfpLeaveRequest>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.LeaveBalanceId == id &&
                        !x.IsDelete &&
                        x.ApprovalStatus == LeaveApprovalStatus.PendingApproval);

                if (hasPendingRequest)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Saldo cuti tidak boleh dinonaktifkan karena masih memiliki pengajuan pending."
                    ));
                }
            }

            entity.IsActive = request.IsActive;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status saldo cuti workforce berhasil diperbarui."
            ));
        }

        [HttpDelete("balances/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Leave",
            Description = "Menghapus saldo cuti workforce profile",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceLeave", "Delete")]
        public async Task<IActionResult> DeleteLeaveBalance(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpLeaveBalance>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Saldo cuti workforce tidak ditemukan."
                ));
            }

            var hasRequest = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .AnyAsync(x => x.LeaveBalanceId == id && !x.IsDelete);

            if (hasRequest)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Saldo cuti tidak boleh dihapus karena sudah memiliki transaksi pengajuan cuti."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Saldo cuti workforce berhasil dihapus."
            ));
        }

        [HttpPost("requests")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceLeaveRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Leave",
            Description = "Membuat pengajuan cuti workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceLeave", "Create")]
        public async Task<IActionResult> CreateLeaveRequest(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceLeaveRequestRequest request)
        {
            var profileExists = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (!profileExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var totalDays = ResolveTotalDays(request.StartDate, request.EndDate, request.TotalDays, request.IsHalfDay);

            var validation = await ValidateCreateLeaveRequestAsync(
                workforceProfileId,
                request.LeaveType,
                request.StartDate,
                request.EndDate,
                totalDays,
                request.IsDeductBalance,
                request.Reason);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data pengajuan cuti tidak valid."
                ));
            }

            var leaveBalance = validation.LeaveBalance;
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsDeductBalance && leaveBalance != null)
                {
                    leaveBalance.PendingDays += totalDays;
                    RecalculateLeaveBalance(leaveBalance);
                    leaveBalance.UpdateDateTime = now;
                    leaveBalance.UpdateBy = actorUserId;
                }

                var entity = new WfpLeaveRequest
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    LeaveBalanceId = leaveBalance?.Id,
                    LeaveType = request.LeaveType,
                    StartDate = request.StartDate.Date,
                    EndDate = request.EndDate.Date,
                    TotalDays = totalDays,
                    IsHalfDay = request.IsHalfDay,
                    IsDeductBalance = request.IsDeductBalance,
                    Reason = request.Reason.Trim(),
                    ApprovalStatus = LeaveApprovalStatus.PendingApproval,
                    RequestedAt = now,
                    AttachmentPath = NormalizeNullableText(request.AttachmentPath),
                    AttachmentContentType = NormalizeNullableText(request.AttachmentContentType),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpLeaveRequest>().Add(entity);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await BuildLeaveRequestResponseAsync(entity.Id);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceLeave.CreateLeaveRequest",
                    "Pengajuan cuti workforce berhasil dibuat.",
                    new
                    {
                        workforceProfileId,
                        entity.Id,
                        entity.LeaveType,
                        entity.StartDate,
                        entity.EndDate,
                        entity.TotalDays,
                        entity.ApprovalStatus
                    }
                );

                return Ok(ApiResponse<WorkforceLeaveRequestResponse>.Ok(
                    response!,
                    "Pengajuan cuti workforce berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceLeave.CreateLeaveRequest",
                    "Gagal membuat pengajuan cuti workforce.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat pengajuan cuti workforce."
                    )
                );
            }
        }

        [HttpPatch("requests/{id:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceLeaveRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Leave",
            Description = "Approve pengajuan cuti workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceLeave", "Update")]
        public async Task<IActionResult> ApproveLeaveRequest(
            Guid workforceProfileId,
            Guid id,
            [FromBody] ApproveWorkforceLeaveRequestRequest request)
        {
            var entity = await _dbContext.Set<WfpLeaveRequest>()
                .Include(x => x.LeaveBalance)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan cuti workforce tidak ditemukan."
                ));
            }

            if (entity.ApprovalStatus != LeaveApprovalStatus.PendingApproval)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya pengajuan cuti pending yang bisa di-approve."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (entity.IsDeductBalance)
                {
                    if (entity.LeaveBalance == null)
                    {
                        return BadRequest(ApiResponse<object>.Fail(
                            StatusCodes.Status400BadRequest,
                            "Saldo cuti tidak ditemukan untuk pengajuan ini."
                        ));
                    }

                    entity.LeaveBalance.PendingDays = Math.Max(0, entity.LeaveBalance.PendingDays - entity.TotalDays);
                    entity.LeaveBalance.UsedDays += entity.TotalDays;
                    RecalculateLeaveBalance(entity.LeaveBalance);
                    entity.LeaveBalance.UpdateDateTime = now;
                    entity.LeaveBalance.UpdateBy = actorUserId;
                }

                entity.ApprovalStatus = LeaveApprovalStatus.Approved;
                entity.ApprovedByUserId = actorUserId;
                entity.ApprovedAt = now;
                entity.ApprovalNote = NormalizeNullableText(request.ApprovalNote);
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await BuildLeaveRequestResponseAsync(entity.Id);

                return Ok(ApiResponse<WorkforceLeaveRequestResponse>.Ok(
                    response!,
                    "Pengajuan cuti workforce berhasil di-approve."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceLeave.ApproveLeaveRequest",
                    "Gagal approve pengajuan cuti workforce.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat approve pengajuan cuti workforce."
                    )
                );
            }
        }

        [HttpPatch("requests/{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceLeaveRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Leave",
            Description = "Reject pengajuan cuti workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceLeave", "Update")]
        public async Task<IActionResult> RejectLeaveRequest(
            Guid workforceProfileId,
            Guid id,
            [FromBody] RejectWorkforceLeaveRequestRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RejectedReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan reject wajib diisi."
                ));
            }

            var entity = await _dbContext.Set<WfpLeaveRequest>()
                .Include(x => x.LeaveBalance)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan cuti workforce tidak ditemukan."
                ));
            }

            if (entity.ApprovalStatus != LeaveApprovalStatus.PendingApproval)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya pengajuan cuti pending yang bisa di-reject."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                RestorePendingLeaveBalance(entity, now, actorUserId);

                entity.ApprovalStatus = LeaveApprovalStatus.Rejected;
                entity.RejectedByUserId = actorUserId;
                entity.RejectedAt = now;
                entity.RejectedReason = request.RejectedReason.Trim();
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await BuildLeaveRequestResponseAsync(entity.Id);

                return Ok(ApiResponse<WorkforceLeaveRequestResponse>.Ok(
                    response!,
                    "Pengajuan cuti workforce berhasil di-reject."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceLeave.RejectLeaveRequest",
                    "Gagal reject pengajuan cuti workforce.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat reject pengajuan cuti workforce."
                    )
                );
            }
        }

        [HttpPatch("requests/{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceLeaveRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Leave",
            Description = "Cancel pengajuan cuti workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceLeave", "Update")]
        public async Task<IActionResult> CancelLeaveRequest(
            Guid workforceProfileId,
            Guid id,
            [FromBody] CancelWorkforceLeaveRequestRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CancelReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan cancel wajib diisi."
                ));
            }

            var entity = await _dbContext.Set<WfpLeaveRequest>()
                .Include(x => x.LeaveBalance)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan cuti workforce tidak ditemukan."
                ));
            }

            if (entity.ApprovalStatus == LeaveApprovalStatus.Cancelled ||
                entity.ApprovalStatus == LeaveApprovalStatus.Rejected)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pengajuan cuti yang sudah cancelled/rejected tidak bisa di-cancel ulang."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (entity.ApprovalStatus == LeaveApprovalStatus.PendingApproval)
                {
                    RestorePendingLeaveBalance(entity, now, actorUserId);
                }
                else if (entity.ApprovalStatus == LeaveApprovalStatus.Approved && entity.IsDeductBalance)
                {
                    if (entity.LeaveBalance == null)
                    {
                        return BadRequest(ApiResponse<object>.Fail(
                            StatusCodes.Status400BadRequest,
                            "Saldo cuti tidak ditemukan untuk pengajuan ini."
                        ));
                    }

                    entity.LeaveBalance.UsedDays = Math.Max(0, entity.LeaveBalance.UsedDays - entity.TotalDays);
                    RecalculateLeaveBalance(entity.LeaveBalance);
                    entity.LeaveBalance.UpdateDateTime = now;
                    entity.LeaveBalance.UpdateBy = actorUserId;
                }

                entity.ApprovalStatus = LeaveApprovalStatus.Cancelled;
                entity.CancelledByUserId = actorUserId;
                entity.CancelledAt = now;
                entity.CancelReason = request.CancelReason.Trim();
                entity.IsActive = false;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await BuildLeaveRequestResponseAsync(entity.Id);

                return Ok(ApiResponse<WorkforceLeaveRequestResponse>.Ok(
                    response!,
                    "Pengajuan cuti workforce berhasil di-cancel."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceLeave.CancelLeaveRequest",
                    "Gagal cancel pengajuan cuti workforce.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat cancel pengajuan cuti workforce."
                    )
                );
            }
        }

        [HttpDelete("requests/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Leave",
            Description = "Menghapus pengajuan cuti workforce profile",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceLeave", "Delete")]
        public async Task<IActionResult> DeleteLeaveRequest(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpLeaveRequest>()
                .Include(x => x.LeaveBalance)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan cuti workforce tidak ditemukan."
                ));
            }

            if (entity.ApprovalStatus == LeaveApprovalStatus.Approved)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pengajuan cuti approved tidak boleh dihapus. Gunakan cancel agar saldo cuti dikembalikan dengan audit trail."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (entity.ApprovalStatus == LeaveApprovalStatus.PendingApproval)
                {
                    RestorePendingLeaveBalance(entity, now, actorUserId);
                }

                entity.IsActive = false;
                entity.IsDelete = true;
                entity.DeleteDateTime = now;
                entity.DeleteBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Pengajuan cuti workforce berhasil dihapus."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceLeave.DeleteLeaveRequest",
                    "Gagal menghapus pengajuan cuti workforce.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat menghapus pengajuan cuti workforce."
                    )
                );
            }
        }

        private async Task<MstWorkforceProfile?> GetWorkforceProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private async Task<WorkforceLeaveBalanceResponse?> BuildLeaveBalanceResponseAsync(Guid id)
        {
            return await _dbContext.Set<WfpLeaveBalance>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new WorkforceLeaveBalanceResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    DisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    LeaveYear = x.LeaveYear,
                    LeaveType = x.LeaveType,
                    OpeningBalance = x.OpeningBalance,
                    EntitledDays = x.EntitledDays,
                    UsedDays = x.UsedDays,
                    PendingDays = x.PendingDays,
                    RemainingDays = x.RemainingDays,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();
        }

        private async Task<WorkforceLeaveRequestResponse?> BuildLeaveRequestResponseAsync(Guid id)
        {
            return await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new WorkforceLeaveRequestResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    DisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    LeaveBalanceId = x.LeaveBalanceId,
                    LeaveType = x.LeaveType,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    TotalDays = x.TotalDays,
                    IsHalfDay = x.IsHalfDay,
                    IsDeductBalance = x.IsDeductBalance,
                    Reason = x.Reason,
                    ApprovalStatus = x.ApprovalStatus,
                    RequestedAt = x.RequestedAt,                    
                    ApprovedAt = x.ApprovedAt,
                    ApprovalNote = x.ApprovalNote,                    
                    RejectedAt = x.RejectedAt,
                    RejectedReason = x.RejectedReason,                    
                    CancelledAt = x.CancelledAt,
                    CancelReason = x.CancelReason,
                    AttachmentPath = x.AttachmentPath,
                    AttachmentContentType = x.AttachmentContentType,
                    HasAttachment = !string.IsNullOrWhiteSpace(x.AttachmentPath),
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateLeaveBalanceRequestAsync(
            Guid workforceProfileId,
            int leaveYear,
            LeaveType leaveType,
            decimal openingBalance,
            decimal entitledDays,
            decimal usedDays,
            decimal pendingDays,
            DateTime? effectiveStartDate,
            DateTime? effectiveEndDate,
            Guid? excludeLeaveBalanceId)
        {
            if (leaveYear < 2000 || leaveYear > 2100)
            {
                return (false, "LeaveYear tidak valid.");
            }

            if (leaveType == LeaveType.Unknown)
            {
                return (false, "LeaveType wajib dipilih."
                );
            }

            if (openingBalance < 0 || entitledDays < 0 || usedDays < 0 || pendingDays < 0)
            {
                return (false, "Saldo cuti tidak boleh bernilai negatif.");
            }

            var remainingDays = CalculateRemainingDays(openingBalance, entitledDays, usedDays, pendingDays);

            if (remainingDays < 0)
            {
                return (false, "RemainingDays tidak boleh negatif. Periksa OpeningBalance, EntitledDays, UsedDays, dan PendingDays.");
            }

            if (effectiveStartDate.HasValue && effectiveEndDate.HasValue &&
                effectiveStartDate.Value.Date > effectiveEndDate.Value.Date)
            {
                return (false, "EffectiveStartDate tidak boleh lebih besar dari EffectiveEndDate.");
            }

            var duplicateExists = await _dbContext.Set<WfpLeaveBalance>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.Id != excludeLeaveBalanceId &&
                    x.LeaveYear == leaveYear &&
                    x.LeaveType == leaveType &&
                    !x.IsDelete);

            if (duplicateExists)
            {
                return (false, "Saldo cuti dengan tahun dan tipe cuti tersebut sudah tersedia untuk workforce profile ini.");
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage, WfpLeaveBalance? LeaveBalance)> ValidateCreateLeaveRequestAsync(
            Guid workforceProfileId,
            LeaveType leaveType,
            DateTime startDate,
            DateTime endDate,
            decimal totalDays,
            bool isDeductBalance,
            string? reason)
        {
            if (leaveType == LeaveType.Unknown)
            {
                return (false, "LeaveType wajib dipilih.", null);
            }

            if (startDate.Date > endDate.Date)
            {
                return (false, "StartDate tidak boleh lebih besar dari EndDate.", null);
            }

            if (startDate.Year != endDate.Year)
            {
                return (false, "Pengajuan cuti sementara belum mendukung lintas tahun. Buat request terpisah per tahun.", null);
            }

            if (totalDays <= 0)
            {
                return (false, "TotalDays harus lebih besar dari 0.", null);
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return (false, "Reason wajib diisi.", null);
            }

            var overlapExists = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete &&
                    (x.ApprovalStatus == LeaveApprovalStatus.PendingApproval ||
                     x.ApprovalStatus == LeaveApprovalStatus.Approved) &&
                    x.StartDate.Date <= endDate.Date &&
                    x.EndDate.Date >= startDate.Date);

            if (overlapExists)
            {
                return (false, "Sudah ada pengajuan cuti pending/approved pada rentang tanggal tersebut.", null);
            }

            WfpLeaveBalance? leaveBalance = null;

            if (isDeductBalance)
            {
                leaveBalance = await _dbContext.Set<WfpLeaveBalance>()
                    .FirstOrDefaultAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.LeaveYear == startDate.Year &&
                        x.LeaveType == leaveType &&
                        x.IsActive &&
                        !x.IsDelete);

                if (leaveBalance == null)
                {
                    return (false, "Saldo cuti tidak ditemukan untuk tahun dan tipe cuti tersebut.", null);
                }

                if (leaveBalance.RemainingDays < totalDays)
                {
                    return (false, "Saldo cuti tidak mencukupi.", null);
                }
            }

            return (true, null, leaveBalance);
        }

        private static decimal ResolveTotalDays(
            DateTime startDate,
            DateTime endDate,
            decimal? totalDays,
            bool isHalfDay)
        {
            if (totalDays.HasValue && totalDays.Value > 0)
            {
                return totalDays.Value;
            }

            if (isHalfDay)
            {
                return 0.5m;
            }

            return Convert.ToDecimal((endDate.Date - startDate.Date).TotalDays + 1);
        }

        private static decimal CalculateRemainingDays(
            decimal openingBalance,
            decimal entitledDays,
            decimal usedDays,
            decimal pendingDays)
        {
            return openingBalance + entitledDays - usedDays - pendingDays;
        }

        private static void RecalculateLeaveBalance(WfpLeaveBalance leaveBalance)
        {
            leaveBalance.RemainingDays = CalculateRemainingDays(
                leaveBalance.OpeningBalance,
                leaveBalance.EntitledDays,
                leaveBalance.UsedDays,
                leaveBalance.PendingDays);
        }

        private static void RestorePendingLeaveBalance(
            WfpLeaveRequest leaveRequest,
            DateTime now,
            Guid actorUserId)
        {
            if (!leaveRequest.IsDeductBalance || leaveRequest.LeaveBalance == null)
            {
                return;
            }

            leaveRequest.LeaveBalance.PendingDays = Math.Max(0, leaveRequest.LeaveBalance.PendingDays - leaveRequest.TotalDays);
            RecalculateLeaveBalance(leaveRequest.LeaveBalance);
            leaveRequest.LeaveBalance.UpdateDateTime = now;
            leaveRequest.LeaveBalance.UpdateBy = actorUserId;
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

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