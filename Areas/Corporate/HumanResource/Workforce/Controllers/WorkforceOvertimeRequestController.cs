using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models;
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
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/overtime-requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Overtime Request",
        AreaName = "Corporate",
        ControllerName = "WorkforceOvertimeRequest",
        Description = "Workforce overtime request management",
        SortOrder = 36
    )]
    [Tags("Corporate / Human Resource / Workforce / Overtime Request")]
    public class WorkforceOvertimeRequestController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.OvertimeRequest";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceOvertimeRequestController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOvertimeRequestListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Overtime Request",
            Description = "Melihat data pengajuan lembur workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceOvertimeRequest", "Read")]
        public async Task<IActionResult> GetOvertimeRequests(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] OvertimeApprovalStatus? approvalStatus,
            [FromQuery] bool? isPayrollProcessed,
            [FromQuery] Guid? attendanceId,
            [FromQuery] Guid? workScheduleId)
        {
            var profile = await GetWorkforceProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var query = _dbContext.Set<WfpOvertimeRequest>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (startDate.HasValue)
            {
                query = query.Where(x => x.OvertimeDate >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.OvertimeDate <= endDate.Value.Date);
            }

            if (approvalStatus.HasValue)
            {
                query = query.Where(x => x.ApprovalStatus == approvalStatus.Value);
            }

            if (isPayrollProcessed.HasValue)
            {
                query = query.Where(x => x.IsPayrollProcessed == isPayrollProcessed.Value);
            }

            if (attendanceId.HasValue && attendanceId.Value != Guid.Empty)
            {
                query = query.Where(x => x.AttendanceId == attendanceId.Value);
            }

            if (workScheduleId.HasValue && workScheduleId.Value != Guid.Empty)
            {
                query = query.Where(x => x.WorkScheduleId == workScheduleId.Value);
            }

            var rawItems = await query
                .OrderByDescending(x => x.OvertimeDate)
                .ThenByDescending(x => x.RequestedAt)
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId,
                    x.AttendanceId,
                    x.WorkScheduleAssignmentId,
                    x.WorkScheduleId,
                    WorkScheduleCode = x.WorkSchedule != null ? x.WorkSchedule.ScheduleCode : null,
                    WorkScheduleName = x.WorkSchedule != null ? x.WorkSchedule.ScheduleName : null,
                    x.OvertimeDate,
                    x.ScheduledStartTime,
                    x.ScheduledEndTime,
                    x.ActualCheckInAt,
                    x.ActualCheckOutAt,
                    x.StartTime,
                    x.EndTime,
                    x.IsOvernight,
                    x.TotalMinutes,
                    x.Reason,
                    x.ApprovalStatus,
                    x.RequestedAt,
                    x.ApprovedByUserId,
                    ApprovedByUserName = x.ApprovedByUser != null ? x.ApprovedByUser.DisplayName : null,
                    x.ApprovedAt,
                    x.ApprovalNote,
                    x.RejectedByUserId,
                    RejectedByUserName = x.RejectedByUser != null ? x.RejectedByUser.DisplayName : null,
                    x.RejectedAt,
                    x.RejectedReason,
                    x.CancelledByUserId,
                    CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName : null,
                    x.CancelledAt,
                    x.CancelReason,
                    x.IsPayrollProcessed,
                    x.PayrollProcessedAt,
                    x.PayrollProcessedByUserId,
                    PayrollProcessedByUserName = x.PayrollProcessedByUser != null ? x.PayrollProcessedByUser.DisplayName : null,
                    x.PayrollPeriodCode,
                    x.AttachmentPath,
                    x.AttachmentContentType,
                    x.IsActive,
                    x.CreateDateTime
                })
                .ToListAsync();

            var items = rawItems.Select(x => new WorkforceOvertimeRequestResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                AttendanceId = x.AttendanceId,
                WorkScheduleAssignmentId = x.WorkScheduleAssignmentId,
                WorkScheduleId = x.WorkScheduleId,
                WorkScheduleCode = x.WorkScheduleCode,
                WorkScheduleName = x.WorkScheduleName,
                OvertimeDate = x.OvertimeDate,
                ScheduledStartTime = x.ScheduledStartTime,
                ScheduledEndTime = x.ScheduledEndTime,
                ActualCheckInAt = x.ActualCheckInAt,
                ActualCheckOutAt = x.ActualCheckOutAt,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                IsOvernight = x.IsOvernight,
                TotalMinutes = x.TotalMinutes,
                TotalHours = Math.Round(x.TotalMinutes / 60m, 2),
                Reason = x.Reason,
                ApprovalStatus = x.ApprovalStatus,
                RequestedAt = x.RequestedAt,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByUserName = x.ApprovedByUserName,
                ApprovedAt = x.ApprovedAt,
                ApprovalNote = x.ApprovalNote,
                RejectedByUserId = x.RejectedByUserId,
                RejectedByUserName = x.RejectedByUserName,
                RejectedAt = x.RejectedAt,
                RejectedReason = x.RejectedReason,
                CancelledByUserId = x.CancelledByUserId,
                CancelledByUserName = x.CancelledByUserName,
                CancelledAt = x.CancelledAt,
                CancelReason = x.CancelReason,
                IsPayrollProcessed = x.IsPayrollProcessed,
                PayrollProcessedAt = x.PayrollProcessedAt,
                PayrollProcessedByUserId = x.PayrollProcessedByUserId,
                PayrollProcessedByUserName = x.PayrollProcessedByUserName,
                PayrollPeriodCode = x.PayrollPeriodCode,
                AttachmentPath = x.AttachmentPath,
                AttachmentContentType = x.AttachmentContentType,
                HasAttachment = !string.IsNullOrWhiteSpace(x.AttachmentPath),
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            }).ToList();

            var result = new WorkforceOvertimeRequestListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                PendingData = items.Count(x => x.ApprovalStatus == OvertimeApprovalStatus.PendingApproval),
                ApprovedData = items.Count(x => x.ApprovalStatus == OvertimeApprovalStatus.Approved),
                RejectedData = items.Count(x => x.ApprovalStatus == OvertimeApprovalStatus.Rejected),
                CancelledData = items.Count(x => x.ApprovalStatus == OvertimeApprovalStatus.Cancelled),
                PayrollProcessedData = items.Count(x => x.IsPayrollProcessed),
                TotalMinutes = items.Sum(x => x.TotalMinutes),
                ApprovedMinutes = items
                    .Where(x => x.ApprovalStatus == OvertimeApprovalStatus.Approved)
                    .Sum(x => x.TotalMinutes),
                PendingMinutes = items
                    .Where(x => x.ApprovalStatus == OvertimeApprovalStatus.PendingApproval)
                    .Sum(x => x.TotalMinutes),
                Items = items
            };

            return Ok(ApiResponse<WorkforceOvertimeRequestListResponse>.Ok(
                result,
                "Data pengajuan lembur workforce berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOvertimeRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Overtime Request",
            Description = "Melihat detail pengajuan lembur workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceOvertimeRequest", "Read")]
        public async Task<IActionResult> GetOvertimeRequestById(Guid workforceProfileId, Guid id)
        {
            var response = await BuildOvertimeRequestResponseAsync(id, workforceProfileId);

            if (response == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan lembur workforce tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceOvertimeRequestResponse>.Ok(
                response,
                "Detail pengajuan lembur workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOvertimeRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Overtime Request",
            Description = "Membuat pengajuan lembur workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceOvertimeRequest", "Create")]
        public async Task<IActionResult> CreateOvertimeRequest(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceOvertimeRequestRequest request)
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

            var contextResult = await ResolveOvertimeContextAsync(
                workforceProfileId,
                request.AttendanceId,
                request.WorkScheduleAssignmentId,
                request.WorkScheduleId);

            if (!contextResult.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    contextResult.ErrorMessage ?? "Konteks lembur tidak valid."
                ));
            }

            var totalMinutes = ResolveTotalMinutes(
                request.StartTime,
                request.EndTime,
                request.IsOvernight,
                request.TotalMinutes);

            var validation = await ValidateOvertimeRequestAsync(
                workforceProfileId,
                request.OvertimeDate,
                request.StartTime,
                request.EndTime,
                request.IsOvernight,
                totalMinutes,
                request.Reason,
                excludeOvertimeRequestId: null);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data pengajuan lembur tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new WfpOvertimeRequest
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                AttendanceId = contextResult.AttendanceId,
                WorkScheduleAssignmentId = contextResult.WorkScheduleAssignmentId,
                WorkScheduleId = contextResult.WorkScheduleId,
                OvertimeDate = request.OvertimeDate.Date,
                ScheduledStartTime = contextResult.ScheduledStartTime ?? request.ScheduledStartTime,
                ScheduledEndTime = contextResult.ScheduledEndTime ?? request.ScheduledEndTime,
                ActualCheckInAt = contextResult.ActualCheckInAt ?? request.ActualCheckInAt,
                ActualCheckOutAt = contextResult.ActualCheckOutAt ?? request.ActualCheckOutAt,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsOvernight = request.IsOvernight,
                TotalMinutes = totalMinutes,
                Reason = request.Reason.Trim(),
                ApprovalStatus = OvertimeApprovalStatus.PendingApproval,
                RequestedAt = now,
                IsPayrollProcessed = false,
                PayrollPeriodCode = null,
                AttachmentPath = NormalizeNullableText(request.AttachmentPath),
                AttachmentContentType = NormalizeNullableText(request.AttachmentContentType),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpOvertimeRequest>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildOvertimeRequestResponseAsync(entity.Id, workforceProfileId);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceOvertimeRequest.CreateOvertimeRequest",
                "Pengajuan lembur workforce berhasil dibuat.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.OvertimeDate,
                    entity.StartTime,
                    entity.EndTime,
                    entity.TotalMinutes,
                    entity.ApprovalStatus
                }
            );

            return Ok(ApiResponse<WorkforceOvertimeRequestResponse>.Ok(
                response!,
                "Pengajuan lembur workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOvertimeRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Overtime Request",
            Description = "Mengubah pengajuan lembur workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceOvertimeRequest", "Update")]
        public async Task<IActionResult> UpdateOvertimeRequest(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceOvertimeRequestRequest request)
        {
            var entity = await _dbContext.Set<WfpOvertimeRequest>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan lembur workforce tidak ditemukan."
                ));
            }

            if (entity.IsPayrollProcessed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pengajuan lembur yang sudah diproses payroll tidak boleh diubah."
                ));
            }

            if (entity.ApprovalStatus != OvertimeApprovalStatus.PendingApproval &&
                entity.ApprovalStatus != OvertimeApprovalStatus.Draft)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya pengajuan lembur draft/pending yang bisa diubah."
                ));
            }

            var contextResult = await ResolveOvertimeContextAsync(
                workforceProfileId,
                request.AttendanceId,
                request.WorkScheduleAssignmentId,
                request.WorkScheduleId);

            if (!contextResult.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    contextResult.ErrorMessage ?? "Konteks lembur tidak valid."
                ));
            }

            var totalMinutes = ResolveTotalMinutes(
                request.StartTime,
                request.EndTime,
                request.IsOvernight,
                request.TotalMinutes);

            var validation = await ValidateOvertimeRequestAsync(
                workforceProfileId,
                request.OvertimeDate,
                request.StartTime,
                request.EndTime,
                request.IsOvernight,
                totalMinutes,
                request.Reason,
                excludeOvertimeRequestId: id);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data pengajuan lembur tidak valid."
                ));
            }

            entity.AttendanceId = contextResult.AttendanceId;
            entity.WorkScheduleAssignmentId = contextResult.WorkScheduleAssignmentId;
            entity.WorkScheduleId = contextResult.WorkScheduleId;
            entity.OvertimeDate = request.OvertimeDate.Date;
            entity.ScheduledStartTime = contextResult.ScheduledStartTime ?? request.ScheduledStartTime;
            entity.ScheduledEndTime = contextResult.ScheduledEndTime ?? request.ScheduledEndTime;
            entity.ActualCheckInAt = contextResult.ActualCheckInAt ?? request.ActualCheckInAt;
            entity.ActualCheckOutAt = contextResult.ActualCheckOutAt ?? request.ActualCheckOutAt;
            entity.StartTime = request.StartTime;
            entity.EndTime = request.EndTime;
            entity.IsOvernight = request.IsOvernight;
            entity.TotalMinutes = totalMinutes;
            entity.Reason = request.Reason.Trim();
            entity.AttachmentPath = NormalizeNullableText(request.AttachmentPath);
            entity.AttachmentContentType = NormalizeNullableText(request.AttachmentContentType);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildOvertimeRequestResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceOvertimeRequestResponse>.Ok(
                response!,
                "Pengajuan lembur workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOvertimeRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Overtime Request",
            Description = "Approve pengajuan lembur workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceOvertimeRequest", "Update")]
        public async Task<IActionResult> ApproveOvertimeRequest(
            Guid workforceProfileId,
            Guid id,
            [FromBody] ApproveWorkforceOvertimeRequestRequest request)
        {
            var entity = await _dbContext.Set<WfpOvertimeRequest>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan lembur workforce tidak ditemukan."
                ));
            }

            if (entity.IsPayrollProcessed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pengajuan lembur yang sudah diproses payroll tidak boleh diubah."
                ));
            }

            if (entity.ApprovalStatus != OvertimeApprovalStatus.PendingApproval)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya pengajuan lembur pending yang bisa di-approve."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ApprovalStatus = OvertimeApprovalStatus.Approved;
            entity.ApprovedByUserId = actorUserId;
            entity.ApprovedAt = now;
            entity.ApprovalNote = NormalizeNullableText(request.ApprovalNote);
            entity.IsActive = true;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildOvertimeRequestResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceOvertimeRequestResponse>.Ok(
                response!,
                "Pengajuan lembur workforce berhasil di-approve."
            ));
        }

        [HttpPatch("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOvertimeRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Overtime Request",
            Description = "Reject pengajuan lembur workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceOvertimeRequest", "Update")]
        public async Task<IActionResult> RejectOvertimeRequest(
            Guid workforceProfileId,
            Guid id,
            [FromBody] RejectWorkforceOvertimeRequestRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RejectedReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan reject wajib diisi."
                ));
            }

            var entity = await _dbContext.Set<WfpOvertimeRequest>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan lembur workforce tidak ditemukan."
                ));
            }

            if (entity.IsPayrollProcessed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pengajuan lembur yang sudah diproses payroll tidak boleh diubah."
                ));
            }

            if (entity.ApprovalStatus != OvertimeApprovalStatus.PendingApproval)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya pengajuan lembur pending yang bisa di-reject."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ApprovalStatus = OvertimeApprovalStatus.Rejected;
            entity.RejectedByUserId = actorUserId;
            entity.RejectedAt = now;
            entity.RejectedReason = request.RejectedReason.Trim();
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildOvertimeRequestResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceOvertimeRequestResponse>.Ok(
                response!,
                "Pengajuan lembur workforce berhasil di-reject."
            ));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOvertimeRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Overtime Request",
            Description = "Cancel pengajuan lembur workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceOvertimeRequest", "Update")]
        public async Task<IActionResult> CancelOvertimeRequest(
            Guid workforceProfileId,
            Guid id,
            [FromBody] CancelWorkforceOvertimeRequestRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CancelReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan cancel wajib diisi."
                ));
            }

            var entity = await _dbContext.Set<WfpOvertimeRequest>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan lembur workforce tidak ditemukan."
                ));
            }

            if (entity.IsPayrollProcessed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pengajuan lembur yang sudah diproses payroll tidak boleh di-cancel."
                ));
            }

            if (entity.ApprovalStatus == OvertimeApprovalStatus.Cancelled ||
                entity.ApprovalStatus == OvertimeApprovalStatus.Rejected)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pengajuan lembur yang sudah cancelled/rejected tidak bisa di-cancel ulang."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ApprovalStatus = OvertimeApprovalStatus.Cancelled;
            entity.CancelledByUserId = actorUserId;
            entity.CancelledAt = now;
            entity.CancelReason = request.CancelReason.Trim();
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildOvertimeRequestResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceOvertimeRequestResponse>.Ok(
                response!,
                "Pengajuan lembur workforce berhasil di-cancel."
            ));
        }

        [HttpPatch("{id:guid}/payroll-status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOvertimeRequestResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Overtime Request",
            Description = "Mengubah status proses payroll lembur workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceOvertimeRequest", "Update")]
        public async Task<IActionResult> UpdatePayrollStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceOvertimePayrollStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpOvertimeRequest>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan lembur workforce tidak ditemukan."
                ));
            }

            if (request.IsPayrollProcessed && entity.ApprovalStatus != OvertimeApprovalStatus.Approved)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya lembur approved yang bisa diproses payroll."
                ));
            }

            if (request.IsPayrollProcessed && string.IsNullOrWhiteSpace(request.PayrollPeriodCode))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "PayrollPeriodCode wajib diisi jika lembur diproses payroll."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsPayrollProcessed = request.IsPayrollProcessed;
            entity.PayrollPeriodCode = request.IsPayrollProcessed
                ? NormalizeNullableText(request.PayrollPeriodCode)
                : null;
            entity.PayrollProcessedAt = request.IsPayrollProcessed ? now : null;
            entity.PayrollProcessedByUserId = request.IsPayrollProcessed ? actorUserId : null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildOvertimeRequestResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceOvertimeRequestResponse>.Ok(
                response!,
                "Status payroll lembur workforce berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Overtime Request",
            Description = "Menghapus pengajuan lembur workforce profile",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceOvertimeRequest", "Delete")]
        public async Task<IActionResult> DeleteOvertimeRequest(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpOvertimeRequest>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan lembur workforce tidak ditemukan."
                ));
            }

            if (entity.IsPayrollProcessed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pengajuan lembur yang sudah diproses payroll tidak boleh dihapus."
                ));
            }

            if (entity.ApprovalStatus == OvertimeApprovalStatus.Approved)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pengajuan lembur approved tidak boleh dihapus. Gunakan cancel agar audit trail tetap aman."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Pengajuan lembur workforce berhasil dihapus."
            ));
        }

        private async Task<MstWorkforceProfile?> GetWorkforceProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private async Task<WorkforceOvertimeRequestResponse?> BuildOvertimeRequestResponseAsync(
            Guid id,
            Guid workforceProfileId)
        {
            var item = await _dbContext.Set<WfpOvertimeRequest>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    DisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    x.AttendanceId,
                    x.WorkScheduleAssignmentId,
                    x.WorkScheduleId,
                    WorkScheduleCode = x.WorkSchedule != null ? x.WorkSchedule.ScheduleCode : null,
                    WorkScheduleName = x.WorkSchedule != null ? x.WorkSchedule.ScheduleName : null,
                    x.OvertimeDate,
                    x.ScheduledStartTime,
                    x.ScheduledEndTime,
                    x.ActualCheckInAt,
                    x.ActualCheckOutAt,
                    x.StartTime,
                    x.EndTime,
                    x.IsOvernight,
                    x.TotalMinutes,
                    x.Reason,
                    x.ApprovalStatus,
                    x.RequestedAt,
                    x.ApprovedByUserId,
                    ApprovedByUserName = x.ApprovedByUser != null ? x.ApprovedByUser.DisplayName : null,
                    x.ApprovedAt,
                    x.ApprovalNote,
                    x.RejectedByUserId,
                    RejectedByUserName = x.RejectedByUser != null ? x.RejectedByUser.DisplayName : null,
                    x.RejectedAt,
                    x.RejectedReason,
                    x.CancelledByUserId,
                    CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName : null,
                    x.CancelledAt,
                    x.CancelReason,
                    x.IsPayrollProcessed,
                    x.PayrollProcessedAt,
                    x.PayrollProcessedByUserId,
                    PayrollProcessedByUserName = x.PayrollProcessedByUser != null ? x.PayrollProcessedByUser.DisplayName : null,
                    x.PayrollPeriodCode,
                    x.AttachmentPath,
                    x.AttachmentContentType,
                    x.IsActive,
                    x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return null;
            }

            return new WorkforceOvertimeRequestResponse
            {
                Id = item.Id,
                WorkforceProfileId = item.WorkforceProfileId,
                ProfileCode = item.ProfileCode,
                DisplayName = item.DisplayName,
                AttendanceId = item.AttendanceId,
                WorkScheduleAssignmentId = item.WorkScheduleAssignmentId,
                WorkScheduleId = item.WorkScheduleId,
                WorkScheduleCode = item.WorkScheduleCode,
                WorkScheduleName = item.WorkScheduleName,
                OvertimeDate = item.OvertimeDate,
                ScheduledStartTime = item.ScheduledStartTime,
                ScheduledEndTime = item.ScheduledEndTime,
                ActualCheckInAt = item.ActualCheckInAt,
                ActualCheckOutAt = item.ActualCheckOutAt,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                IsOvernight = item.IsOvernight,
                TotalMinutes = item.TotalMinutes,
                TotalHours = Math.Round(item.TotalMinutes / 60m, 2),
                Reason = item.Reason,
                ApprovalStatus = item.ApprovalStatus,
                RequestedAt = item.RequestedAt,
                ApprovedByUserId = item.ApprovedByUserId,
                ApprovedByUserName = item.ApprovedByUserName,
                ApprovedAt = item.ApprovedAt,
                ApprovalNote = item.ApprovalNote,
                RejectedByUserId = item.RejectedByUserId,
                RejectedByUserName = item.RejectedByUserName,
                RejectedAt = item.RejectedAt,
                RejectedReason = item.RejectedReason,
                CancelledByUserId = item.CancelledByUserId,
                CancelledByUserName = item.CancelledByUserName,
                CancelledAt = item.CancelledAt,
                CancelReason = item.CancelReason,
                IsPayrollProcessed = item.IsPayrollProcessed,
                PayrollProcessedAt = item.PayrollProcessedAt,
                PayrollProcessedByUserId = item.PayrollProcessedByUserId,
                PayrollProcessedByUserName = item.PayrollProcessedByUserName,
                PayrollPeriodCode = item.PayrollPeriodCode,
                AttachmentPath = item.AttachmentPath,
                AttachmentContentType = item.AttachmentContentType,
                HasAttachment = !string.IsNullOrWhiteSpace(item.AttachmentPath),
                IsActive = item.IsActive,
                CreateDateTime = item.CreateDateTime
            };
        }

        private async Task<OvertimeContextResolveResult> ResolveOvertimeContextAsync(
            Guid workforceProfileId,
            Guid? attendanceId,
            Guid? workScheduleAssignmentId,
            Guid? workScheduleId)
        {
            var result = new OvertimeContextResolveResult
            {
                IsValid = true,
                AttendanceId = NormalizeNullableGuid(attendanceId),
                WorkScheduleAssignmentId = NormalizeNullableGuid(workScheduleAssignmentId),
                WorkScheduleId = NormalizeNullableGuid(workScheduleId)
            };

            if (result.AttendanceId.HasValue)
            {
                var attendance = await _dbContext.Set<EmpAttendance>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == result.AttendanceId.Value &&
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

                if (attendance == null)
                {
                    return OvertimeContextResolveResult.Invalid("Attendance tidak ditemukan atau tidak sesuai workforce profile.");
                }

                result.WorkScheduleAssignmentId ??= attendance.WorkScheduleAssignmentId;
                result.WorkScheduleId ??= attendance.WorkScheduleId;
                result.ScheduledStartTime = attendance.WorkStartTime;
                result.ScheduledEndTime = attendance.WorkEndTime;
                result.ActualCheckInAt = attendance.CheckInAt;
                result.ActualCheckOutAt = attendance.CheckOutAt;
            }

            if (result.WorkScheduleAssignmentId.HasValue)
            {
                var assignment = await _dbContext.Set<WfpWorkScheduleAssignment>()
                    .AsNoTracking()
                    .Include(x => x.WorkSchedule)
                    .FirstOrDefaultAsync(x =>
                        x.Id == result.WorkScheduleAssignmentId.Value &&
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

                if (assignment == null)
                {
                    return OvertimeContextResolveResult.Invalid("Work schedule assignment tidak ditemukan atau tidak sesuai workforce profile.");
                }

                result.WorkScheduleId ??= assignment.WorkScheduleId;

                if (assignment.WorkSchedule != null)
                {
                    result.ScheduledStartTime ??= assignment.WorkSchedule.WorkStartTime;
                    result.ScheduledEndTime ??= assignment.WorkSchedule.WorkEndTime;
                }
            }

            if (result.WorkScheduleId.HasValue)
            {
                var schedule = await _dbContext.Set<MstWorkSchedule>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.WorkScheduleId.Value && !x.IsDelete);

                if (schedule == null)
                {
                    return OvertimeContextResolveResult.Invalid("Work schedule tidak ditemukan.");
                }

                result.ScheduledStartTime ??= schedule.WorkStartTime;
                result.ScheduledEndTime ??= schedule.WorkEndTime;
            }

            return result;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateOvertimeRequestAsync(
            Guid workforceProfileId,
            DateTime overtimeDate,
            TimeOnly startTime,
            TimeOnly endTime,
            bool isOvernight,
            int totalMinutes,
            string? reason,
            Guid? excludeOvertimeRequestId)
        {
            if (overtimeDate == default)
            {
                return (false, "OvertimeDate wajib diisi.");
            }

            if (startTime == endTime)
            {
                return (false, "StartTime dan EndTime tidak boleh sama.");
            }

            if (!isOvernight && startTime > endTime)
            {
                return (false, "StartTime tidak boleh lebih besar dari EndTime jika IsOvernight = false.");
            }

            if (totalMinutes <= 0)
            {
                return (false, "TotalMinutes harus lebih besar dari 0.");
            }

            if (totalMinutes > 24 * 60)
            {
                return (false, "TotalMinutes tidak boleh lebih dari 24 jam.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return (false, "Reason wajib diisi.");
            }

            var overtimeDateOnly = overtimeDate.Date;

            var duplicateExactTimeExists = await _dbContext.Set<WfpOvertimeRequest>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.Id != excludeOvertimeRequestId &&
                    x.OvertimeDate == overtimeDateOnly &&
                    x.StartTime == startTime &&
                    x.EndTime == endTime &&
                    !x.IsDelete &&
                    (x.ApprovalStatus == OvertimeApprovalStatus.PendingApproval ||
                     x.ApprovalStatus == OvertimeApprovalStatus.Approved));

            if (duplicateExactTimeExists)
            {
                return (false, "Sudah ada pengajuan lembur pending/approved pada tanggal dan jam yang sama.");
            }

            return (true, null);
        }

        private static int ResolveTotalMinutes(
            TimeOnly startTime,
            TimeOnly endTime,
            bool isOvernight,
            int? totalMinutes)
        {
            if (totalMinutes.HasValue && totalMinutes.Value > 0)
            {
                return totalMinutes.Value;
            }

            var startDateTime = DateTime.Today.Add(startTime.ToTimeSpan());
            var endDateTime = DateTime.Today.Add(endTime.ToTimeSpan());

            if (isOvernight && endDateTime <= startDateTime)
            {
                endDateTime = endDateTime.AddDays(1);
            }

            return Convert.ToInt32((endDateTime - startDateTime).TotalMinutes);
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

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
            {
                return null;
            }

            return value.Value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private sealed class OvertimeContextResolveResult
        {
            public bool IsValid { get; set; }

            public string? ErrorMessage { get; set; }

            public Guid? AttendanceId { get; set; }

            public Guid? WorkScheduleAssignmentId { get; set; }

            public Guid? WorkScheduleId { get; set; }

            public TimeOnly? ScheduledStartTime { get; set; }

            public TimeOnly? ScheduledEndTime { get; set; }

            public DateTime? ActualCheckInAt { get; set; }

            public DateTime? ActualCheckOutAt { get; set; }

            public static OvertimeContextResolveResult Invalid(string errorMessage)
            {
                return new OvertimeContextResolveResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}