using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce/compliance-alerts")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Compliance Alert",
        AreaName = "Corporate",
        ControllerName = "WorkforceComplianceAlert",
        Description = "Corporate human resource workforce compliance alert and expiry reminder",
        SortOrder = 15
    )]
    [Tags("Corporate / Human Resource / Workforce / Compliance Alert")]
    public class WorkforceComplianceAlertController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceComplianceAlertController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceComplianceAlertListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Compliance Alert",
            Description = "Melihat data compliance alert workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceComplianceAlert", "Read")]
        public async Task<IActionResult> GetComplianceAlerts(
            [FromQuery] Guid? workforceProfileId,
            [FromQuery] ComplianceAlertType? alertType,
            [FromQuery] ComplianceAlertStatus? alertStatus,
            [FromQuery] ComplianceAlertSeverityLevel? severityLevel,
            [FromQuery] bool? isResolved,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? dueFrom,
            [FromQuery] DateTime? dueTo,
            [FromQuery] string? sourceEntityName,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "dueDate",
            [FromQuery] string? sortDirection = "asc")
        {
            var query = _dbContext.WfpComplianceAlerts
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.ResolvedByUser)
                .Include(x => x.Logs)
                .Where(x => !x.IsDelete);

            if (workforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);
            }

            if (alertType.HasValue)
            {
                query = query.Where(x => x.AlertType == alertType.Value);
            }

            if (alertStatus.HasValue)
            {
                query = query.Where(x => x.AlertStatus == alertStatus.Value);
            }

            if (severityLevel.HasValue)
            {
                query = query.Where(x => x.SeverityLevel == severityLevel.Value);
            }

            if (isResolved.HasValue)
            {
                query = query.Where(x => x.IsResolved == isResolved.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (dueFrom.HasValue)
            {
                query = query.Where(x => x.DueDate >= dueFrom.Value.Date);
            }

            if (dueTo.HasValue)
            {
                var dueToExclusive = dueTo.Value.Date.AddDays(1);
                query = query.Where(x => x.DueDate < dueToExclusive);
            }

            if (!string.IsNullOrWhiteSpace(sourceEntityName))
            {
                var source = sourceEntityName.Trim().ToLower();
                query = query.Where(x => x.SourceEntityName.ToLower() == source);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.AlertTitle.ToLower().Contains(keyword) ||
                    x.AlertMessage.ToLower().Contains(keyword) ||
                    x.SourceEntityName.ToLower().Contains(keyword) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.DisplayName.ToLower().Contains(keyword))
                );
            }

            query = ApplySorting(query, sortBy, sortDirection);

            var today = DateTime.UtcNow.Date;
            var entities = await query.ToListAsync();
            var items = entities.Select(x => MapAlertResponse(x, today, includeLogs: false)).ToList();

            var result = new WorkforceComplianceAlertListResponse
            {
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                OpenData = items.Count(x => x.AlertStatus == ComplianceAlertStatus.Open),
                InProgressData = items.Count(x => x.AlertStatus == ComplianceAlertStatus.InProgress),
                ResolvedData = items.Count(x => x.IsResolved || x.AlertStatus == ComplianceAlertStatus.Resolved),
                IgnoredData = items.Count(x => x.AlertStatus == ComplianceAlertStatus.Ignored),
                CancelledData = items.Count(x => x.AlertStatus == ComplianceAlertStatus.Cancelled),
                ExpiredData = items.Count(x => x.AlertStatus == ComplianceAlertStatus.Expired),
                OverdueData = items.Count(x => x.IsOverdue && !x.IsResolved),
                DueTodayData = items.Count(x => x.DaysRemaining == 0 && !x.IsResolved),
                DueInSevenDaysData = items.Count(x => x.DaysRemaining >= 0 && x.DaysRemaining <= 7 && !x.IsResolved),
                DueInThirtyDaysData = items.Count(x => x.DaysRemaining >= 0 && x.DaysRemaining <= 30 && !x.IsResolved),
                CriticalData = items.Count(x => x.SeverityLevel == ComplianceAlertSeverityLevel.Critical && !x.IsResolved),
                HighData = items.Count(x => x.SeverityLevel == ComplianceAlertSeverityLevel.High && !x.IsResolved),
                Items = items
            };

            return Ok(ApiResponse<WorkforceComplianceAlertListResponse>.Ok(
                result,
                "Data compliance alert workforce berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceComplianceAlertSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Compliance Alert Summary",
            Description = "Melihat ringkasan compliance alert workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceComplianceAlert", "Read")]
        public async Task<IActionResult> GetSummary([FromQuery] Guid? workforceProfileId)
        {
            var today = DateTime.UtcNow.Date;
            var sevenDays = today.AddDays(7);
            var thirtyDays = today.AddDays(30);

            var query = _dbContext.WfpComplianceAlerts
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (workforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);
            }

            var result = new WorkforceComplianceAlertSummaryResponse
            {
                TotalAlert = await query.CountAsync(),
                OpenAlert = await query.CountAsync(x => x.AlertStatus == ComplianceAlertStatus.Open && !x.IsResolved),
                InProgressAlert = await query.CountAsync(x => x.AlertStatus == ComplianceAlertStatus.InProgress && !x.IsResolved),
                ResolvedAlert = await query.CountAsync(x => x.IsResolved || x.AlertStatus == ComplianceAlertStatus.Resolved),
                OverdueAlert = await query.CountAsync(x => x.DueDate < today && !x.IsResolved),
                DueTodayAlert = await query.CountAsync(x => x.DueDate == today && !x.IsResolved),
                DueInSevenDaysAlert = await query.CountAsync(x => x.DueDate >= today && x.DueDate <= sevenDays && !x.IsResolved),
                DueInThirtyDaysAlert = await query.CountAsync(x => x.DueDate >= today && x.DueDate <= thirtyDays && !x.IsResolved),
                CriticalAlert = await query.CountAsync(x => x.SeverityLevel == ComplianceAlertSeverityLevel.Critical && !x.IsResolved),
                HighAlert = await query.CountAsync(x => x.SeverityLevel == ComplianceAlertSeverityLevel.High && !x.IsResolved),
                DocumentAlert = await query.CountAsync(x => x.SourceEntityName == "WfpDocument" && !x.IsResolved),
                LicenseAlert = await query.CountAsync(x => x.SourceEntityName == "WfpCredentialLicense" && !x.IsResolved),
                CertificationAlert = await query.CountAsync(x => x.SourceEntityName == "WfpCertification" && !x.IsResolved),
                HealthRecordAlert = await query.CountAsync(x => x.SourceEntityName == "WfpHealthRecord" && !x.IsResolved),
                ContractAlert = await query.CountAsync(x =>
                    (x.SourceEntityName == "MstEmployee" ||
                     x.SourceEntityName == "MstDoctor" ||
                     x.SourceEntityName == "WfpContractHistory") &&
                    !x.IsResolved),
                ClinicalPrivilegeAlert = await query.CountAsync(x => x.SourceEntityName == "WfpClinicalPrivilege" && !x.IsResolved),
                ExternalAccessAlert = await query.CountAsync(x => x.SourceEntityName == "MstExternalUser" && !x.IsResolved)
            };

            return Ok(ApiResponse<WorkforceComplianceAlertSummaryResponse>.Ok(
                result,
                "Ringkasan compliance alert workforce berhasil diambil."
            ));
        }

        [HttpGet("{complianceAlertId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceComplianceAlertResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Compliance Alert Detail",
            Description = "Melihat detail compliance alert workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceComplianceAlert", "Read")]
        public async Task<IActionResult> GetComplianceAlertById(Guid complianceAlertId)
        {
            var today = DateTime.UtcNow.Date;

            var entity = await _dbContext.WfpComplianceAlerts
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.ResolvedByUser)
                .Include(x => x.Logs.Where(l => !l.IsDelete))
                    .ThenInclude(x => x.PerformedByUser)
                .FirstOrDefaultAsync(x => x.Id == complianceAlertId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceComplianceAlertResponse>.Ok(
                MapAlertResponse(entity, today, includeLogs: true),
                "Detail compliance alert workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceComplianceAlertResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Compliance Alert",
            Description = "Membuat compliance alert workforce manual",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceComplianceAlert", "Create")]
        public async Task<IActionResult> CreateComplianceAlert([FromBody] CreateWorkforceComplianceAlertRequest request)
        {
            var validation = await ValidateRequestAsync(
                request.WorkforceProfileId,
                request.SourceEntityName,
                request.SourceEntityId,
                request.AlertType,
                request.AlertTitle,
                request.AlertMessage,
                request.DueDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var duplicate = await _dbContext.WfpComplianceAlerts.AnyAsync(x =>
                x.SourceEntityName == request.SourceEntityName.Trim() &&
                x.SourceEntityId == request.SourceEntityId &&
                x.AlertType == request.AlertType &&
                x.DueDate == request.DueDate.Date &&
                !x.IsDelete);

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Compliance alert untuk source dan due date yang sama sudah ada."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var isResolved = request.AlertStatus == ComplianceAlertStatus.Resolved;

            var entity = new WfpComplianceAlert
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = request.WorkforceProfileId,
                SourceEntityName = request.SourceEntityName.Trim(),
                SourceEntityId = request.SourceEntityId,
                AlertType = request.AlertType,
                AlertTitle = request.AlertTitle.Trim(),
                AlertMessage = request.AlertMessage.Trim(),
                DueDate = request.DueDate.Date,
                AlertStatus = request.AlertStatus,
                SeverityLevel = request.SeverityLevel,
                IsResolved = isResolved,
                ResolvedAt = isResolved ? now : null,
                ResolvedByUserId = isResolved ? actorUserId : null,
                Notes = NormalizeNullableText(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.WfpComplianceAlerts.Add(entity);
            _dbContext.WfpComplianceAlertLogs.Add(BuildLogEntity(
                entity.Id,
                ComplianceAlertLogType.Created,
                null,
                entity.AlertStatus,
                "Compliance alert dibuat manual.",
                actorUserId,
                now,
                entity.Notes
            ));

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceComplianceAlert.CreateComplianceAlert",
                "Compliance alert workforce berhasil dibuat.",
                new { entity.Id, entity.WorkforceProfileId, entity.SourceEntityName, entity.SourceEntityId }
            );

            return await GetComplianceAlertById(entity.Id);
        }

        [HttpPut("{complianceAlertId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceComplianceAlertResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Compliance Alert",
            Description = "Mengubah compliance alert workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceComplianceAlert", "Update")]
        public async Task<IActionResult> UpdateComplianceAlert(
            Guid complianceAlertId,
            [FromBody] UpdateWorkforceComplianceAlertRequest request)
        {
            var entity = await _dbContext.WfpComplianceAlerts
                .FirstOrDefaultAsync(x => x.Id == complianceAlertId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                entity.WorkforceProfileId,
                entity.SourceEntityName,
                entity.SourceEntityId,
                request.AlertType,
                request.AlertTitle,
                request.AlertMessage,
                request.DueDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var oldStatus = entity.AlertStatus;
            var isResolved = request.AlertStatus == ComplianceAlertStatus.Resolved;

            entity.AlertType = request.AlertType;
            entity.AlertTitle = request.AlertTitle.Trim();
            entity.AlertMessage = request.AlertMessage.Trim();
            entity.DueDate = request.DueDate.Date;
            entity.AlertStatus = request.AlertStatus;
            entity.SeverityLevel = request.SeverityLevel;
            entity.IsResolved = isResolved;
            entity.ResolvedAt = isResolved ? entity.ResolvedAt ?? now : null;
            entity.ResolvedByUserId = isResolved ? entity.ResolvedByUserId ?? actorUserId : null;
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (oldStatus != entity.AlertStatus)
            {
                _dbContext.WfpComplianceAlertLogs.Add(BuildLogEntity(
                    entity.Id,
                    ComplianceAlertLogType.StatusChanged,
                    oldStatus,
                    entity.AlertStatus,
                    "Status compliance alert diubah.",
                    actorUserId,
                    now,
                    entity.Notes
                ));
            }

            await _dbContext.SaveChangesAsync();

            return await GetComplianceAlertById(entity.Id);
        }

        [HttpPatch("{complianceAlertId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceComplianceAlertResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Compliance Alert Status",
            Description = "Mengubah status compliance alert workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceComplianceAlert", "Update")]
        public async Task<IActionResult> UpdateComplianceAlertStatus(
            Guid complianceAlertId,
            [FromBody] UpdateWorkforceComplianceAlertStatusRequest request)
        {
            var entity = await _dbContext.WfpComplianceAlerts
                .FirstOrDefaultAsync(x => x.Id == complianceAlertId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var oldStatus = entity.AlertStatus;
            var isResolved = request.AlertStatus == ComplianceAlertStatus.Resolved;

            entity.AlertStatus = request.AlertStatus;
            entity.IsResolved = isResolved;
            entity.ResolvedAt = isResolved ? now : null;
            entity.ResolvedByUserId = isResolved ? actorUserId : null;
            entity.Notes = NormalizeNullableText(request.Notes) ?? entity.Notes;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            _dbContext.WfpComplianceAlertLogs.Add(BuildLogEntity(
                entity.Id,
                ComplianceAlertLogType.StatusChanged,
                oldStatus,
                entity.AlertStatus,
                "Status compliance alert diubah.",
                actorUserId,
                now,
                entity.Notes
            ));

            await _dbContext.SaveChangesAsync();

            return await GetComplianceAlertById(entity.Id);
        }

        [HttpPatch("{complianceAlertId:guid}/resolve")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceComplianceAlertResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Resolve Workforce Compliance Alert",
            Description = "Menyelesaikan compliance alert workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 5
        )]
        [AccessPermission("WorkforceComplianceAlert", "Update")]
        public async Task<IActionResult> ResolveComplianceAlert(
            Guid complianceAlertId,
            [FromBody] ResolveWorkforceComplianceAlertRequest request)
        {
            var entity = await _dbContext.WfpComplianceAlerts
                .FirstOrDefaultAsync(x => x.Id == complianceAlertId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert tidak ditemukan."
                ));
            }

            if (entity.IsResolved)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Compliance alert sudah resolved."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var oldStatus = entity.AlertStatus;

            entity.AlertStatus = ComplianceAlertStatus.Resolved;
            entity.IsResolved = true;
            entity.ResolvedAt = now;
            entity.ResolvedByUserId = actorUserId;
            entity.Notes = NormalizeNullableText(request.Notes) ?? entity.Notes;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            _dbContext.WfpComplianceAlertLogs.Add(BuildLogEntity(
                entity.Id,
                ComplianceAlertLogType.Resolved,
                oldStatus,
                entity.AlertStatus,
                "Compliance alert ditandai resolved.",
                actorUserId,
                now,
                entity.Notes
            ));

            await _dbContext.SaveChangesAsync();

            return await GetComplianceAlertById(entity.Id);
        }

        [HttpPatch("{complianceAlertId:guid}/reopen")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceComplianceAlertResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Reopen Workforce Compliance Alert",
            Description = "Membuka kembali compliance alert workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 6
        )]
        [AccessPermission("WorkforceComplianceAlert", "Update")]
        public async Task<IActionResult> ReopenComplianceAlert(
            Guid complianceAlertId,
            [FromBody] ReopenWorkforceComplianceAlertRequest request)
        {
            var entity = await _dbContext.WfpComplianceAlerts
                .FirstOrDefaultAsync(x => x.Id == complianceAlertId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var oldStatus = entity.AlertStatus;

            entity.AlertStatus = ComplianceAlertStatus.Open;
            entity.IsResolved = false;
            entity.ResolvedAt = null;
            entity.ResolvedByUserId = null;
            entity.Notes = NormalizeNullableText(request.Notes) ?? entity.Notes;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            _dbContext.WfpComplianceAlertLogs.Add(BuildLogEntity(
                entity.Id,
                ComplianceAlertLogType.Reopened,
                oldStatus,
                entity.AlertStatus,
                "Compliance alert dibuka kembali.",
                actorUserId,
                now,
                entity.Notes
            ));

            await _dbContext.SaveChangesAsync();

            return await GetComplianceAlertById(entity.Id);
        }

        [HttpPost("{complianceAlertId:guid}/logs")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceComplianceAlertResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Compliance Alert Log",
            Description = "Menambah log compliance alert workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 7
        )]
        [AccessPermission("WorkforceComplianceAlert", "Create")]
        public async Task<IActionResult> AddComplianceAlertLog(
            Guid complianceAlertId,
            [FromBody] AddWorkforceComplianceAlertLogRequest request)
        {
            var exists = await _dbContext.WfpComplianceAlerts
                .AnyAsync(x => x.Id == complianceAlertId && !x.IsDelete);

            if (!exists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            _dbContext.WfpComplianceAlertLogs.Add(BuildLogEntity(
                complianceAlertId,
                request.LogType,
                null,
                null,
                NormalizeNullableText(request.LogMessage) ?? "Log compliance alert ditambahkan.",
                actorUserId,
                now,
                request.Notes
            ));

            await _dbContext.SaveChangesAsync();

            return await GetComplianceAlertById(complianceAlertId);
        }

        [HttpDelete("{complianceAlertId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Compliance Alert",
            Description = "Menghapus compliance alert workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 8
        )]
        [AccessPermission("WorkforceComplianceAlert", "Delete")]
        public async Task<IActionResult> DeleteComplianceAlert(Guid complianceAlertId)
        {
            var entity = await _dbContext.WfpComplianceAlerts
                .Include(x => x.Logs)
                .FirstOrDefaultAsync(x => x.Id == complianceAlertId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;

            foreach (var log in entity.Logs.Where(x => !x.IsDelete))
            {
                log.IsDelete = true;
                log.DeleteDateTime = now;
                log.DeleteBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Compliance alert berhasil dihapus."
            ));
        }

        [HttpPost("generate")]
        [ProducesResponseType(typeof(ApiResponse<GenerateWorkforceComplianceAlertResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Generate Workforce Compliance Alert",
            Description = "Generate compliance alert dari data expired workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 9
        )]
        [AccessPermission("WorkforceComplianceAlert", "Create")]
        public async Task<IActionResult> GenerateComplianceAlerts([FromBody] GenerateWorkforceComplianceAlertRequest request)
        {
            if (!request.IncludeExpired && !request.IncludeWillExpire)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Minimal IncludeExpired atau IncludeWillExpire harus true."
                ));
            }

            if (request.WorkforceProfileId.HasValue)
            {
                var profileExists = await _dbContext.MstWorkforceProfiles
                    .AnyAsync(x => x.Id == request.WorkforceProfileId.Value && !x.IsDelete);

                if (!profileExists)
                {
                    return NotFound(ApiResponse<object>.Fail(
                        StatusCodes.Status404NotFound,
                        "Workforce profile tidak ditemukan."
                    ));
                }
            }

            var today = DateTime.UtcNow.Date;
            var untilDate = today.AddDays(request.DaysBeforeDue);
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var candidates = new List<AlertCandidate>();

            if (request.IncludeDocument)
            {
                await AddDocumentCandidatesAsync(candidates, request, today, untilDate);
            }

            if (request.IncludeCertification)
            {
                await AddCertificationCandidatesAsync(candidates, request, today, untilDate);
            }

            if (request.IncludeCredentialLicense)
            {
                await AddCredentialLicenseCandidatesAsync(candidates, request, today, untilDate);
            }

            if (request.IncludeClinicalPrivilege)
            {
                await AddClinicalPrivilegeCandidatesAsync(candidates, request, today, untilDate);
            }

            if (request.IncludeHealthRecord)
            {
                await AddHealthRecordCandidatesAsync(candidates, request, today, untilDate);
            }

            if (request.IncludeEmployeeContract)
            {
                await AddEmployeeContractCandidatesAsync(candidates, request, today, untilDate);
            }

            if (request.IncludeDoctorContract)
            {
                await AddDoctorContractCandidatesAsync(candidates, request, today, untilDate);
            }

            if (request.IncludeExternalAccess)
            {
                await AddExternalAccessCandidatesAsync(candidates, request, today, untilDate);
            }

            if (request.IncludeContractHistory)
            {
                await AddContractHistoryCandidatesAsync(candidates, request, today, untilDate);
            }

            var existingAlerts = await _dbContext.WfpComplianceAlerts
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => new ExistingAlertKey
                {
                    SourceEntityName = x.SourceEntityName,
                    SourceEntityId = x.SourceEntityId,
                    AlertType = x.AlertType,
                    DueDate = x.DueDate,
                    IsResolved = x.IsResolved
                })
                .ToListAsync();

            var existingKeys = existingAlerts
                .GroupBy(x => BuildKey(x.SourceEntityName, x.SourceEntityId, x.AlertType, x.DueDate))
                .ToDictionary(x => x.Key, x => x.Any(v => v.IsResolved));

            var createdEntities = new List<WfpComplianceAlert>();
            var skippedDuplicate = 0;
            var skippedResolved = 0;

            foreach (var candidate in candidates)
            {
                var key = BuildKey(candidate.SourceEntityName, candidate.SourceEntityId, candidate.AlertType, candidate.DueDate);

                if (existingKeys.TryGetValue(key, out var existingIsResolved))
                {
                    if (existingIsResolved)
                    {
                        skippedResolved++;
                    }
                    else
                    {
                        skippedDuplicate++;
                    }

                    continue;
                }

                var entity = new WfpComplianceAlert
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = candidate.WorkforceProfileId,
                    SourceEntityName = candidate.SourceEntityName,
                    SourceEntityId = candidate.SourceEntityId,
                    AlertType = candidate.AlertType,
                    AlertTitle = candidate.AlertTitle,
                    AlertMessage = candidate.AlertMessage,
                    DueDate = candidate.DueDate.Date,
                    AlertStatus = candidate.AlertStatus,
                    SeverityLevel = candidate.SeverityLevel,
                    IsResolved = false,
                    ResolvedAt = null,
                    ResolvedByUserId = null,
                    Notes = candidate.Notes,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                createdEntities.Add(entity);
                existingKeys[key] = false;

                _dbContext.WfpComplianceAlerts.Add(entity);
                _dbContext.WfpComplianceAlertLogs.Add(BuildLogEntity(
                    entity.Id,
                    ComplianceAlertLogType.Generated,
                    null,
                    entity.AlertStatus,
                    "Compliance alert digenerate otomatis dari data expiry.",
                    actorUserId,
                    now,
                    candidate.Notes
                ));
            }

            if (createdEntities.Any())
            {
                await _dbContext.SaveChangesAsync();
            }

            var createdIds = createdEntities.Select(x => x.Id).ToList();
            var createdItems = await _dbContext.WfpComplianceAlerts
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.ResolvedByUser)
                .Include(x => x.Logs)
                .Where(x => createdIds.Contains(x.Id))
                .OrderBy(x => x.DueDate)
                .ToListAsync();

            var result = new GenerateWorkforceComplianceAlertResponse
            {
                DaysBeforeDue = request.DaysBeforeDue,
                WorkforceProfileId = request.WorkforceProfileId,
                CandidateData = candidates.Count,
                CreatedData = createdEntities.Count,
                SkippedDuplicateData = skippedDuplicate,
                SkippedResolvedData = skippedResolved,
                DocumentCreatedData = createdEntities.Count(x => x.SourceEntityName == "WfpDocument"),
                CertificationCreatedData = createdEntities.Count(x => x.SourceEntityName == "WfpCertification"),
                CredentialLicenseCreatedData = createdEntities.Count(x => x.SourceEntityName == "WfpCredentialLicense"),
                ClinicalPrivilegeCreatedData = createdEntities.Count(x => x.SourceEntityName == "WfpClinicalPrivilege"),
                HealthRecordCreatedData = createdEntities.Count(x => x.SourceEntityName == "WfpHealthRecord"),
                EmployeeContractCreatedData = createdEntities.Count(x => x.SourceEntityName == "MstEmployee"),
                DoctorContractCreatedData = createdEntities.Count(x => x.SourceEntityName == "MstDoctor"),
                ExternalAccessCreatedData = createdEntities.Count(x => x.SourceEntityName == "MstExternalUser"),
                ContractHistoryCreatedData = createdEntities.Count(x => x.SourceEntityName == "WfpContractHistory"),
                CreatedItems = createdItems.Select(x => MapAlertResponse(x, today, includeLogs: false)).ToList()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceComplianceAlert.GenerateComplianceAlerts",
                "Generate compliance alert workforce selesai.",
                new
                {
                    result.CandidateData,
                    result.CreatedData,
                    result.SkippedDuplicateData,
                    result.SkippedResolvedData,
                    request.WorkforceProfileId,
                    request.DaysBeforeDue
                }
            );

            return Ok(ApiResponse<GenerateWorkforceComplianceAlertResponse>.Ok(
                result,
                "Generate compliance alert workforce berhasil diproses."
            ));
        }

        private static IQueryable<WfpComplianceAlert> ApplySorting(
            IQueryable<WfpComplianceAlert> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            var normalizedSortBy = string.IsNullOrWhiteSpace(sortBy)
                ? "dueDate"
                : sortBy.Trim().ToLower();

            return normalizedSortBy switch
            {
                "createdatetime" or "created" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                "severity" or "severitylevel" => isDesc
                    ? query.OrderByDescending(x => x.SeverityLevel).ThenBy(x => x.DueDate)
                    : query.OrderBy(x => x.SeverityLevel).ThenBy(x => x.DueDate),
                "status" or "alertstatus" => isDesc
                    ? query.OrderByDescending(x => x.AlertStatus).ThenBy(x => x.DueDate)
                    : query.OrderBy(x => x.AlertStatus).ThenBy(x => x.DueDate),
                "type" or "alerttype" => isDesc
                    ? query.OrderByDescending(x => x.AlertType).ThenBy(x => x.DueDate)
                    : query.OrderBy(x => x.AlertType).ThenBy(x => x.DueDate),
                "title" or "alerttitle" => isDesc
                    ? query.OrderByDescending(x => x.AlertTitle).ThenBy(x => x.DueDate)
                    : query.OrderBy(x => x.AlertTitle).ThenBy(x => x.DueDate),
                _ => isDesc
                    ? query.OrderByDescending(x => x.DueDate).ThenByDescending(x => x.SeverityLevel)
                    : query.OrderBy(x => x.DueDate).ThenByDescending(x => x.SeverityLevel)
            };
        }

        private static WorkforceComplianceAlertResponse MapAlertResponse(
            WfpComplianceAlert entity,
            DateTime today,
            bool includeLogs)
        {
            var daysRemaining = (entity.DueDate.Date - today).Days;

            return new WorkforceComplianceAlertResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                ProfileCode = entity.WorkforceProfile?.ProfileCode ?? string.Empty,
                DisplayName = entity.WorkforceProfile?.DisplayName ?? string.Empty,
                UserType = entity.WorkforceProfile?.UserType ?? UserType.Employee,
                SourceEntityName = entity.SourceEntityName,
                SourceEntityId = entity.SourceEntityId,
                SourceDisplayName = entity.AlertTitle,
                AlertType = entity.AlertType,
                AlertTitle = entity.AlertTitle,
                AlertMessage = entity.AlertMessage,
                DueDate = entity.DueDate,
                DaysRemaining = daysRemaining,
                IsOverdue = daysRemaining < 0,
                AlertStatus = entity.AlertStatus,
                SeverityLevel = entity.SeverityLevel,
                IsResolved = entity.IsResolved,
                ResolvedAt = entity.ResolvedAt,
                ResolvedByUserId = entity.ResolvedByUserId,
                ResolvedByUserName = entity.ResolvedByUser?.DisplayName,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                LogCount = entity.Logs.Count(x => !x.IsDelete),
                CreateDateTime = entity.CreateDateTime,
                Logs = includeLogs
                    ? entity.Logs
                        .Where(x => !x.IsDelete)
                        .OrderByDescending(x => x.PerformedAt)
                        .Select(x => new WorkforceComplianceAlertLogResponse
                        {
                            Id = x.Id,
                            ComplianceAlertId = x.ComplianceAlertId,
                            LogType = x.LogType,
                            OldStatus = x.OldStatus,
                            NewStatus = x.NewStatus,
                            LogMessage = x.LogMessage,
                            PerformedByUserId = x.PerformedByUserId,
                            PerformedByUserName = x.PerformedByUser?.DisplayName,
                            PerformedAt = x.PerformedAt,
                            Notes = x.Notes,
                            IsActive = x.IsActive,
                            CreateDateTime = x.CreateDateTime
                        })
                        .ToList()
                    : new List<WorkforceComplianceAlertLogResponse>()
            };
        }

        private async Task<(bool IsValid, string Message)> ValidateRequestAsync(
            Guid workforceProfileId,
            string sourceEntityName,
            Guid sourceEntityId,
            ComplianceAlertType alertType,
            string alertTitle,
            string alertMessage,
            DateTime dueDate)
        {
            if (workforceProfileId == Guid.Empty)
            {
                return (false, "WorkforceProfileId wajib diisi.");
            }

            var profileExists = await _dbContext.MstWorkforceProfiles
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (!profileExists)
            {
                return (false, "Workforce profile tidak ditemukan.");
            }

            if (string.IsNullOrWhiteSpace(sourceEntityName))
            {
                return (false, "SourceEntityName wajib diisi.");
            }

            if (sourceEntityId == Guid.Empty)
            {
                return (false, "SourceEntityId wajib diisi.");
            }

            if (alertType == ComplianceAlertType.Unknown)
            {
                return (false, "AlertType wajib dipilih.");
            }

            if (string.IsNullOrWhiteSpace(alertTitle))
            {
                return (false, "AlertTitle wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(alertMessage))
            {
                return (false, "AlertMessage wajib diisi.");
            }

            if (dueDate == default)
            {
                return (false, "DueDate wajib diisi.");
            }

            return (true, string.Empty);
        }

        private async Task AddDocumentCandidatesAsync(
            List<AlertCandidate> candidates,
            GenerateWorkforceComplianceAlertRequest request,
            DateTime today,
            DateTime untilDate)
        {
            var query = _dbContext.WfpDocuments
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.ExpiredDate.HasValue);

            if (request.WorkforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }

            var rows = await query.ToListAsync();

            foreach (var row in rows)
            {
                var dueDate = row.ExpiredDate.Value.Date;

                if (!ShouldIncludeDueDate(dueDate, today, untilDate, request))
                {
                    continue;
                }

                var isExpired = dueDate < today;

                candidates.Add(new AlertCandidate
                {
                    WorkforceProfileId = row.WorkforceProfileId,
                    SourceEntityName = "WfpDocument",
                    SourceEntityId = row.Id,
                    AlertType = isExpired ? ComplianceAlertType.DocumentExpired : ComplianceAlertType.DocumentWillExpire,
                    AlertTitle = isExpired
                        ? $"Dokumen expired: {row.DocumentName}"
                        : $"Dokumen akan expired: {row.DocumentName}",
                    AlertMessage = $"Dokumen {row.DocumentName} ({row.DocumentType}) memiliki due date {dueDate:yyyy-MM-dd}.",
                    DueDate = dueDate,
                    AlertStatus = isExpired ? ComplianceAlertStatus.Expired : ComplianceAlertStatus.Open,
                    SeverityLevel = ResolveSeverity(dueDate, today),
                    Notes = row.DocumentNumber
                });
            }
        }

        private async Task AddCertificationCandidatesAsync(
            List<AlertCandidate> candidates,
            GenerateWorkforceComplianceAlertRequest request,
            DateTime today,
            DateTime untilDate)
        {
            var query = _dbContext.WfpCertifications
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && !x.IsLifetime && x.ExpiredDate.HasValue);

            if (request.WorkforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }

            var rows = await query.ToListAsync();

            foreach (var row in rows)
            {
                var dueDate = row.ExpiredDate.Value.Date;

                if (!ShouldIncludeDueDate(dueDate, today, untilDate, request))
                {
                    continue;
                }

                var isExpired = dueDate < today;

                candidates.Add(new AlertCandidate
                {
                    WorkforceProfileId = row.WorkforceProfileId,
                    SourceEntityName = "WfpCertification",
                    SourceEntityId = row.Id,
                    AlertType = isExpired ? ComplianceAlertType.CertificationExpired : ComplianceAlertType.CertificationWillExpire,
                    AlertTitle = isExpired
                        ? $"Sertifikasi expired: {row.CertificationName}"
                        : $"Sertifikasi akan expired: {row.CertificationName}",
                    AlertMessage = $"Sertifikasi {row.CertificationName} ({row.CertificationType}) memiliki due date {dueDate:yyyy-MM-dd}.",
                    DueDate = dueDate,
                    AlertStatus = isExpired ? ComplianceAlertStatus.Expired : ComplianceAlertStatus.Open,
                    SeverityLevel = ResolveSeverity(dueDate, today),
                    Notes = row.CertificateNumber
                });
            }
        }

        private async Task AddCredentialLicenseCandidatesAsync(
            List<AlertCandidate> candidates,
            GenerateWorkforceComplianceAlertRequest request,
            DateTime today,
            DateTime untilDate)
        {
            var query = _dbContext.WfpCredentialLicenses
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            if (request.WorkforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }

            var rows = await query.ToListAsync();

            foreach (var row in rows)
            {
                var dueDate = row.ExpiredDate.Date;

                if (!ShouldIncludeDueDate(dueDate, today, untilDate, request))
                {
                    continue;
                }

                var isExpired = dueDate < today;

                candidates.Add(new AlertCandidate
                {
                    WorkforceProfileId = row.WorkforceProfileId,
                    SourceEntityName = "WfpCredentialLicense",
                    SourceEntityId = row.Id,
                    AlertType = isExpired ? ComplianceAlertType.LicenseExpired : ComplianceAlertType.LicenseWillExpire,
                    AlertTitle = isExpired
                        ? $"License expired: {row.LicenseType}"
                        : $"License akan expired: {row.LicenseType}",
                    AlertMessage = $"License {row.LicenseType} nomor {row.LicenseNumber} memiliki due date {dueDate:yyyy-MM-dd}.",
                    DueDate = dueDate,
                    AlertStatus = isExpired ? ComplianceAlertStatus.Expired : ComplianceAlertStatus.Open,
                    SeverityLevel = ResolveSeverity(dueDate, today),
                    Notes = row.LicenseNumber
                });
            }
        }

        private async Task AddClinicalPrivilegeCandidatesAsync(
            List<AlertCandidate> candidates,
            GenerateWorkforceComplianceAlertRequest request,
            DateTime today,
            DateTime untilDate)
        {
            var query = _dbContext.WfpClinicalPrivileges
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.EffectiveEndDate.HasValue);

            if (request.WorkforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }

            var rows = await query.ToListAsync();

            foreach (var row in rows)
            {
                var dueDate = row.EffectiveEndDate.Value.Date;

                if (!ShouldIncludeDueDate(dueDate, today, untilDate, request))
                {
                    continue;
                }

                var isExpired = dueDate < today;

                candidates.Add(new AlertCandidate
                {
                    WorkforceProfileId = row.WorkforceProfileId,
                    SourceEntityName = "WfpClinicalPrivilege",
                    SourceEntityId = row.Id,
                    AlertType = isExpired ? ComplianceAlertType.ClinicalPrivilegeExpired : ComplianceAlertType.ClinicalPrivilegeReview,
                    AlertTitle = isExpired
                        ? $"Clinical privilege expired: {row.PrivilegeName}"
                        : $"Clinical privilege perlu review: {row.PrivilegeName}",
                    AlertMessage = $"Clinical privilege {row.PrivilegeName} memiliki effective end date {dueDate:yyyy-MM-dd}.",
                    DueDate = dueDate,
                    AlertStatus = isExpired ? ComplianceAlertStatus.Expired : ComplianceAlertStatus.Open,
                    SeverityLevel = ResolveSeverity(dueDate, today),
                    Notes = row.PrivilegeCode
                });
            }
        }

        private async Task AddHealthRecordCandidatesAsync(
            List<AlertCandidate> candidates,
            GenerateWorkforceComplianceAlertRequest request,
            DateTime today,
            DateTime untilDate)
        {
            var query = _dbContext.WfpHealthRecords
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.ExpiredDate.HasValue);

            if (request.WorkforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }

            var rows = await query.ToListAsync();

            foreach (var row in rows)
            {
                var dueDate = row.ExpiredDate.Value.Date;

                if (!ShouldIncludeDueDate(dueDate, today, untilDate, request))
                {
                    continue;
                }

                var isExpired = dueDate < today;

                candidates.Add(new AlertCandidate
                {
                    WorkforceProfileId = row.WorkforceProfileId,
                    SourceEntityName = "WfpHealthRecord",
                    SourceEntityId = row.Id,
                    AlertType = isExpired ? ComplianceAlertType.HealthRecordExpired : ComplianceAlertType.HealthRecordWillExpire,
                    AlertTitle = isExpired
                        ? $"Health record expired: {row.HealthRecordType}"
                        : $"Health record akan expired: {row.HealthRecordType}",
                    AlertMessage = $"Health record {row.HealthRecordType} dari provider {row.ProviderName ?? "-"} memiliki due date {dueDate:yyyy-MM-dd}.",
                    DueDate = dueDate,
                    AlertStatus = isExpired ? ComplianceAlertStatus.Expired : ComplianceAlertStatus.Open,
                    SeverityLevel = ResolveSeverity(dueDate, today),
                    Notes = row.RequirementCode
                });
            }
        }

        private async Task AddEmployeeContractCandidatesAsync(
            List<AlertCandidate> candidates,
            GenerateWorkforceComplianceAlertRequest request,
            DateTime today,
            DateTime untilDate)
        {
            var query = _dbContext.MstEmployees
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.ContractEndDate.HasValue);

            if (request.WorkforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }

            var rows = await query.ToListAsync();

            foreach (var row in rows)
            {
                var dueDate = row.ContractEndDate.Value.Date;

                if (!ShouldIncludeDueDate(dueDate, today, untilDate, request))
                {
                    continue;
                }

                var isExpired = dueDate < today;

                candidates.Add(new AlertCandidate
                {
                    WorkforceProfileId = row.WorkforceProfileId,
                    SourceEntityName = "MstEmployee",
                    SourceEntityId = row.Id,
                    AlertType = isExpired ? ComplianceAlertType.ContractExpired : ComplianceAlertType.ContractWillEnd,
                    AlertTitle = isExpired
                        ? $"Kontrak employee expired: {row.FullName}"
                        : $"Kontrak employee akan berakhir: {row.FullName}",
                    AlertMessage = $"Kontrak employee {row.FullName} ({row.EmployeeNumber}) memiliki tanggal akhir {dueDate:yyyy-MM-dd}.",
                    DueDate = dueDate,
                    AlertStatus = isExpired ? ComplianceAlertStatus.Expired : ComplianceAlertStatus.Open,
                    SeverityLevel = ResolveSeverity(dueDate, today),
                    Notes = row.EmployeeCode
                });
            }
        }

        private async Task AddDoctorContractCandidatesAsync(
            List<AlertCandidate> candidates,
            GenerateWorkforceComplianceAlertRequest request,
            DateTime today,
            DateTime untilDate)
        {
            var query = _dbContext.MstDoctors
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.ContractEndDate.HasValue);

            if (request.WorkforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }

            var rows = await query.ToListAsync();

            foreach (var row in rows)
            {
                var dueDate = row.ContractEndDate.Value.Date;

                if (!ShouldIncludeDueDate(dueDate, today, untilDate, request))
                {
                    continue;
                }

                var isExpired = dueDate < today;

                candidates.Add(new AlertCandidate
                {
                    WorkforceProfileId = row.WorkforceProfileId,
                    SourceEntityName = "MstDoctor",
                    SourceEntityId = row.Id,
                    AlertType = isExpired ? ComplianceAlertType.ContractExpired : ComplianceAlertType.ContractWillEnd,
                    AlertTitle = isExpired
                        ? $"Kontrak doctor expired: {row.FullName}"
                        : $"Kontrak doctor akan berakhir: {row.FullName}",
                    AlertMessage = $"Kontrak doctor {row.FullName} ({row.DoctorNumber}) memiliki tanggal akhir {dueDate:yyyy-MM-dd}.",
                    DueDate = dueDate,
                    AlertStatus = isExpired ? ComplianceAlertStatus.Expired : ComplianceAlertStatus.Open,
                    SeverityLevel = ResolveSeverity(dueDate, today),
                    Notes = row.DoctorCode
                });
            }
        }

        private async Task AddExternalAccessCandidatesAsync(
            List<AlertCandidate> candidates,
            GenerateWorkforceComplianceAlertRequest request,
            DateTime today,
            DateTime untilDate)
        {
            var query = _dbContext.MstExternalUsers
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.AccessEndDate.HasValue);

            if (request.WorkforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }

            var rows = await query.ToListAsync();

            foreach (var row in rows)
            {
                var dueDate = row.AccessEndDate.Value.Date;

                if (!ShouldIncludeDueDate(dueDate, today, untilDate, request))
                {
                    continue;
                }

                var isExpired = dueDate < today;

                candidates.Add(new AlertCandidate
                {
                    WorkforceProfileId = row.WorkforceProfileId,
                    SourceEntityName = "MstExternalUser",
                    SourceEntityId = row.Id,
                    AlertType = isExpired ? ComplianceAlertType.ExternalAccessExpired : ComplianceAlertType.ExternalAccessWillEnd,
                    AlertTitle = isExpired
                        ? $"Akses external user expired: {row.FullName}"
                        : $"Akses external user akan berakhir: {row.FullName}",
                    AlertMessage = $"Akses external user {row.FullName} dari {row.CompanyName ?? "-"} memiliki tanggal akhir {dueDate:yyyy-MM-dd}.",
                    DueDate = dueDate,
                    AlertStatus = isExpired ? ComplianceAlertStatus.Expired : ComplianceAlertStatus.Open,
                    SeverityLevel = ResolveSeverity(dueDate, today),
                    Notes = row.ExternalCode
                });
            }
        }

        private async Task AddContractHistoryCandidatesAsync(
            List<AlertCandidate> candidates,
            GenerateWorkforceComplianceAlertRequest request,
            DateTime today,
            DateTime untilDate)
        {
            var query = _dbContext.WfpContractHistories
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.EndDate.HasValue);

            if (request.WorkforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            }

            var rows = await query.ToListAsync();

            foreach (var row in rows)
            {
                var dueDate = row.EndDate.Value.Date;

                if (!ShouldIncludeDueDate(dueDate, today, untilDate, request))
                {
                    continue;
                }

                var isExpired = dueDate < today;

                candidates.Add(new AlertCandidate
                {
                    WorkforceProfileId = row.WorkforceProfileId,
                    SourceEntityName = "WfpContractHistory",
                    SourceEntityId = row.Id,
                    AlertType = isExpired ? ComplianceAlertType.ContractExpired : ComplianceAlertType.ContractWillEnd,
                    AlertTitle = isExpired
                        ? $"Riwayat kontrak expired: {row.ContractNumber}"
                        : $"Riwayat kontrak akan berakhir: {row.ContractNumber}",
                    AlertMessage = $"Riwayat kontrak nomor {row.ContractNumber} memiliki tanggal akhir {dueDate:yyyy-MM-dd}.",
                    DueDate = dueDate,
                    AlertStatus = isExpired ? ComplianceAlertStatus.Expired : ComplianceAlertStatus.Open,
                    SeverityLevel = ResolveSeverity(dueDate, today),
                    Notes = row.ContractNumber
                });
            }
        }

        private static bool ShouldIncludeDueDate(
            DateTime dueDate,
            DateTime today,
            DateTime untilDate,
            GenerateWorkforceComplianceAlertRequest request)
        {
            if (dueDate < today)
            {
                return request.IncludeExpired;
            }

            if (dueDate >= today && dueDate <= untilDate)
            {
                return request.IncludeWillExpire;
            }

            return false;
        }

        private static ComplianceAlertSeverityLevel ResolveSeverity(DateTime dueDate, DateTime today)
        {
            var daysRemaining = (dueDate.Date - today.Date).Days;

            if (daysRemaining < 0)
            {
                return ComplianceAlertSeverityLevel.Critical;
            }

            if (daysRemaining <= 7)
            {
                return ComplianceAlertSeverityLevel.High;
            }

            if (daysRemaining <= 14)
            {
                return ComplianceAlertSeverityLevel.Medium;
            }

            return ComplianceAlertSeverityLevel.Low;
        }

        private static WfpComplianceAlertLog BuildLogEntity(
            Guid complianceAlertId,
            ComplianceAlertLogType logType,
            ComplianceAlertStatus? oldStatus,
            ComplianceAlertStatus? newStatus,
            string logMessage,
            Guid actorUserId,
            DateTime now,
            string? notes)
        {
            return new WfpComplianceAlertLog
            {
                Id = Guid.NewGuid(),
                ComplianceAlertId = complianceAlertId,
                LogType = logType,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                LogMessage = logMessage,
                PerformedByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                PerformedAt = now,
                Notes = NormalizeNullableText(notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };
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

        private static string BuildKey(
            string sourceEntityName,
            Guid sourceEntityId,
            ComplianceAlertType alertType,
            DateTime dueDate)
        {
            return $"{sourceEntityName.Trim().ToLowerInvariant()}|{sourceEntityId}|{(int)alertType}|{dueDate.Date:yyyyMMdd}";
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private class AlertCandidate
        {
            public Guid WorkforceProfileId { get; set; }

            public string SourceEntityName { get; set; } = string.Empty;

            public Guid SourceEntityId { get; set; }

            public ComplianceAlertType AlertType { get; set; }

            public string AlertTitle { get; set; } = string.Empty;

            public string AlertMessage { get; set; } = string.Empty;

            public DateTime DueDate { get; set; }

            public ComplianceAlertStatus AlertStatus { get; set; }

            public ComplianceAlertSeverityLevel SeverityLevel { get; set; }

            public string? Notes { get; set; }
        }

        private class ExistingAlertKey
        {
            public string SourceEntityName { get; set; } = string.Empty;

            public Guid SourceEntityId { get; set; }

            public ComplianceAlertType AlertType { get; set; }

            public DateTime DueDate { get; set; }

            public bool IsResolved { get; set; }
        }
    }
}
