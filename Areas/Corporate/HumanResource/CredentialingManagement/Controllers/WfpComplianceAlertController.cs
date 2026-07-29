using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/compliance-alerts")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_CREDENTIALING_MANAGEMENT",
        moduleName: "Human Resource Credentialing Management",
        displayName: "Workforce Compliance Alert",
        AreaName = "Corporate",
        ControllerName = "WorkforceComplianceAlert",
        Description = "Corporate human resource workforce compliance alert",
        SortOrder = 20
    )]
    [Tags("Corporate / Human Resource / Credentialing Management / Compliance Alert")]
    public class WfpComplianceAlertController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.CredentialingManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpComplianceAlertController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpComplianceAlertFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Compliance Alert", Description = "Melihat metadata filter compliance alert workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceComplianceAlert", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpComplianceAlertFilterMetadataResponse
            {
                DefaultFilter = new WfpComplianceAlertDefaultFilterResponse(),
                AlertTypeOptions = BuildEnumOptions<ComplianceAlertType>(),
                AlertStatusOptions = BuildEnumOptions<ComplianceAlertStatus>(),
                SeverityLevelOptions = BuildEnumOptions<ComplianceAlertSeverityLevel>(),
                LogTypeOptions = BuildEnumOptions<ComplianceAlertLogType>(),
                SortOptions = new List<WfpComplianceAlertSortOptionResponse>
                {
                    new() { Value = "dueDate", Label = "Tanggal jatuh tempo" },
                    new() { Value = "severityLevel", Label = "Tingkat keparahan" },
                    new() { Value = "alertStatus", Label = "Status alert" },
                    new() { Value = "alertType", Label = "Jenis alert" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpComplianceAlertFilterMetadataResponse>.Ok(
                result,
                "Metadata filter compliance alert workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpComplianceAlertSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Compliance Alert", Description = "Melihat ringkasan compliance alert workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceComplianceAlert", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var query = _dbContext.Set<WfpComplianceAlert>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpComplianceAlertSummaryResponse
            {
                TotalAlert = await query.CountAsync(cancellationToken),
                OpenAlert = await query.CountAsync(x => x.AlertStatus == ComplianceAlertStatus.Open, cancellationToken),
                InProgressAlert = await query.CountAsync(x => x.AlertStatus == ComplianceAlertStatus.InProgress, cancellationToken),
                ResolvedAlert = await query.CountAsync(x => x.AlertStatus == ComplianceAlertStatus.Resolved, cancellationToken),
                IgnoredAlert = await query.CountAsync(x => x.AlertStatus == ComplianceAlertStatus.Ignored, cancellationToken),
                ExpiredAlert = await query.CountAsync(x => x.AlertStatus == ComplianceAlertStatus.Expired, cancellationToken),
                CriticalAlert = await query.CountAsync(x => x.SeverityLevel == ComplianceAlertSeverityLevel.Critical, cancellationToken),
                HighAlert = await query.CountAsync(x => x.SeverityLevel == ComplianceAlertSeverityLevel.High, cancellationToken),
                OverdueAlert = await query.CountAsync(x => !x.IsResolved && x.DueDate < now, cancellationToken),
                SchedulingBlockedAlert = await query.CountAsync(x => x.BlocksScheduling && !x.IsResolved, cancellationToken),
                ClinicalServiceBlockedAlert = await query.CountAsync(x => x.BlocksClinicalService && !x.IsResolved, cancellationToken)
            };

            return Ok(ApiResponse<WfpComplianceAlertSummaryResponse>.Ok(
                result,
                "Ringkasan compliance alert workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpComplianceAlertResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Compliance Alert", Description = "Melihat compliance alert workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceComplianceAlert", "Read")]
        public async Task<IActionResult> GetAlerts(
            Guid workforceProfileId,
            [FromQuery] ComplianceAlertType? alertType,
            [FromQuery] ComplianceAlertStatus? alertStatus,
            [FromQuery] ComplianceAlertSeverityLevel? severityLevel,
            [FromQuery] bool? isResolved,
            [FromQuery] bool? isOverdue,
            [FromQuery] bool? blocksScheduling,
            [FromQuery] bool? blocksClinicalService,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "dueDate",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyFilter(
                query,
                alertType,
                alertStatus,
                severityLevel,
                isResolved,
                isOverdue,
                blocksScheduling,
                blocksClinicalService,
                isActive,
                search);

            var totalData = await query.CountAsync(cancellationToken);

            var rows = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = rows.Select(MapResponse).ToList();

            var result = new PagedResult<WfpComplianceAlertResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<WfpComplianceAlertResponse>>.Ok(
                result,
                "Data compliance alert workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpComplianceAlertDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Compliance Alert", Description = "Melihat detail compliance alert workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceComplianceAlert", "Read")]
        public async Task<IActionResult> GetAlertById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert workforce tidak ditemukan."));
            }

            var result = MapDetailResponse(entity);

            return Ok(ApiResponse<WfpComplianceAlertDetailResponse>.Ok(
                result,
                "Detail compliance alert workforce berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpComplianceAlertDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Workforce Compliance Alert", Description = "Membuat compliance alert workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceComplianceAlert", "Create")]
        public async Task<IActionResult> CreateAlert(
            Guid workforceProfileId,
            [FromBody] CreateWfpComplianceAlertRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(request, null, cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data compliance alert tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            var entity = new WfpComplianceAlert
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                SourceEntityName = request.SourceEntityName.Trim(),
                SourceEntityId = request.SourceEntityId,
                AlertType = request.AlertType,
                AlertTitle = request.AlertTitle.Trim(),
                AlertMessage = request.AlertMessage.Trim(),
                DueDate = EnsureUtc(request.DueDate),
                AlertStatus = ComplianceAlertStatus.Open,
                SeverityLevel = request.SeverityLevel,
                IsResolved = false,
                BlocksScheduling = request.BlocksScheduling,
                BlocksClinicalService = request.BlocksClinicalService,
                Notes = NormalizeText(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpComplianceAlert>().Add(entity);

            AddLog(
                entity.Id,
                ComplianceAlertLogType.Created,
                null,
                ComplianceAlertStatus.Open,
                "Compliance alert dibuat.",
                actor,
                request.Notes,
                now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceComplianceAlert.Create",
                "Membuat compliance alert workforce.",
                new { entity.Id, entity.WorkforceProfileId, entity.AlertType, entity.SeverityLevel });

            return await GetAlertById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpComplianceAlertDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Workforce Compliance Alert", Description = "Mengubah compliance alert workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceComplianceAlert", "Update")]
        public async Task<IActionResult> UpdateAlert(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpComplianceAlertRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpComplianceAlert>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert workforce tidak ditemukan."));
            }

            if (entity.AlertStatus == ComplianceAlertStatus.Resolved ||
                entity.AlertStatus == ComplianceAlertStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Compliance alert yang sudah resolved atau cancelled tidak dapat diubah."));
            }

            var validation = await ValidateRequestAsync(request, id, cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data compliance alert tidak valid."));
            }

            entity.SourceEntityName = request.SourceEntityName.Trim();
            entity.SourceEntityId = request.SourceEntityId;
            entity.AlertType = request.AlertType;
            entity.AlertTitle = request.AlertTitle.Trim();
            entity.AlertMessage = request.AlertMessage.Trim();
            entity.DueDate = EnsureUtc(request.DueDate);
            entity.SeverityLevel = request.SeverityLevel;
            entity.BlocksScheduling = request.BlocksScheduling;
            entity.BlocksClinicalService = request.BlocksClinicalService;
            entity.Notes = NormalizeText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetAlertById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Workforce Compliance Alert Status", Description = "Mengubah status compliance alert workforce", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceComplianceAlert", "Update")]
        public async Task<IActionResult> UpdateAlertStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpComplianceAlertStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await GetTrackedAlertAsync(workforceProfileId, id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert workforce tidak ditemukan."));
            }

            if (!IsValidTransition(entity.AlertStatus, request.AlertStatus))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Perubahan status {entity.AlertStatus} ke {request.AlertStatus} tidak diizinkan."));
            }

            var oldStatus = entity.AlertStatus;
            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            ApplyStatus(entity, request.AlertStatus, actor, now);
            entity.Notes = NormalizeText(request.Notes) ?? entity.Notes;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            AddLog(
                entity.Id,
                ComplianceAlertLogType.StatusChanged,
                oldStatus,
                request.AlertStatus,
                $"Status compliance alert berubah dari {oldStatus} menjadi {request.AlertStatus}.",
                actor,
                request.Notes,
                now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status compliance alert workforce berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/resolve")]
        [AccessAction("Update", "Resolve Workforce Compliance Alert", Description = "Menyelesaikan compliance alert workforce", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("WorkforceComplianceAlert", "Update")]
        public async Task<IActionResult> ResolveAlert(
            Guid workforceProfileId,
            Guid id,
            [FromBody] ResolveWfpComplianceAlertRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await GetTrackedAlertAsync(workforceProfileId, id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert workforce tidak ditemukan."));
            }

            if (!IsValidTransition(entity.AlertStatus, ComplianceAlertStatus.Resolved))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Compliance alert tidak dapat diselesaikan dari status saat ini."));
            }

            var oldStatus = entity.AlertStatus;
            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            ApplyStatus(entity, ComplianceAlertStatus.Resolved, actor, now);
            entity.Notes = NormalizeText(request.ResolutionNotes) ?? entity.Notes;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            AddLog(
                entity.Id,
                ComplianceAlertLogType.Resolved,
                oldStatus,
                ComplianceAlertStatus.Resolved,
                "Compliance alert diselesaikan.",
                actor,
                request.ResolutionNotes,
                now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Compliance alert workforce berhasil diselesaikan."));
        }

        [HttpPatch("{id:guid}/reopen")]
        [AccessAction("Update", "Reopen Workforce Compliance Alert", Description = "Membuka kembali compliance alert workforce", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("WorkforceComplianceAlert", "Update")]
        public async Task<IActionResult> ReopenAlert(
            Guid workforceProfileId,
            Guid id,
            [FromBody] ReopenWfpComplianceAlertRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await GetTrackedAlertAsync(workforceProfileId, id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert workforce tidak ditemukan."));
            }

            if (entity.AlertStatus != ComplianceAlertStatus.Resolved &&
                entity.AlertStatus != ComplianceAlertStatus.Ignored &&
                entity.AlertStatus != ComplianceAlertStatus.Expired)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya alert Resolved, Ignored, atau Expired yang dapat dibuka kembali."));
            }

            var oldStatus = entity.AlertStatus;
            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            ApplyStatus(entity, ComplianceAlertStatus.Open, actor, now);
            entity.Notes = NormalizeText(request.Notes) ?? entity.Notes;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            AddLog(
                entity.Id,
                ComplianceAlertLogType.Reopened,
                oldStatus,
                ComplianceAlertStatus.Open,
                "Compliance alert dibuka kembali.",
                actor,
                request.Notes,
                now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Compliance alert workforce berhasil dibuka kembali."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Workforce Compliance Alert", Description = "Menghapus compliance alert workforce", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("WorkforceComplianceAlert", "Delete")]
        public async Task<IActionResult> DeleteAlert(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await GetTrackedAlertAsync(workforceProfileId, id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert workforce tidak ditemukan."));
            }

            if (entity.AlertStatus == ComplianceAlertStatus.InProgress)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Compliance alert yang sedang diproses tidak dapat dihapus."));
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Compliance alert workforce berhasil dihapus."));
        }

        private IQueryable<WfpComplianceAlert> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpComplianceAlert>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.ResolvedByUser)
                .Include(x => x.Logs)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IQueryable<WfpComplianceAlert> ApplyFilter(
            IQueryable<WfpComplianceAlert> query,
            ComplianceAlertType? alertType,
            ComplianceAlertStatus? alertStatus,
            ComplianceAlertSeverityLevel? severityLevel,
            bool? isResolved,
            bool? isOverdue,
            bool? blocksScheduling,
            bool? blocksClinicalService,
            bool? isActive,
            string? search)
        {
            if (alertType.HasValue)
                query = query.Where(x => x.AlertType == alertType.Value);

            if (alertStatus.HasValue)
                query = query.Where(x => x.AlertStatus == alertStatus.Value);

            if (severityLevel.HasValue)
                query = query.Where(x => x.SeverityLevel == severityLevel.Value);

            if (isResolved.HasValue)
                query = query.Where(x => x.IsResolved == isResolved.Value);

            if (isOverdue.HasValue)
            {
                var now = DateTime.UtcNow;
                query = isOverdue.Value
                    ? query.Where(x => !x.IsResolved && x.DueDate < now)
                    : query.Where(x => x.IsResolved || x.DueDate >= now);
            }

            if (blocksScheduling.HasValue)
                query = query.Where(x => x.BlocksScheduling == blocksScheduling.Value);

            if (blocksClinicalService.HasValue)
                query = query.Where(x => x.BlocksClinicalService == blocksClinicalService.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.AlertTitle.ToLower().Contains(keyword) ||
                    x.AlertMessage.ToLower().Contains(keyword) ||
                    x.SourceEntityName.ToLower().Contains(keyword) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpComplianceAlert> ApplySorting(
            IQueryable<WfpComplianceAlert> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "dueDate").Trim().ToLowerInvariant() switch
            {
                "severitylevel" => isDescending
                    ? query.OrderByDescending(x => x.SeverityLevel)
                    : query.OrderBy(x => x.SeverityLevel),

                "alertstatus" => isDescending
                    ? query.OrderByDescending(x => x.AlertStatus)
                    : query.OrderBy(x => x.AlertStatus),

                "alerttype" => isDescending
                    ? query.OrderByDescending(x => x.AlertType)
                    : query.OrderBy(x => x.AlertType),

                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDescending
                    ? query.OrderByDescending(x => x.DueDate)
                    : query.OrderBy(x => x.DueDate)
            };
        }

        private WfpComplianceAlertResponse MapResponse(WfpComplianceAlert entity)
        {
            return new WfpComplianceAlertResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                WorkforceProfileCode = entity.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = entity.WorkforceProfile?.DisplayName ?? string.Empty,
                SourceEntityName = entity.SourceEntityName,
                SourceEntityId = entity.SourceEntityId,
                AlertType = entity.AlertType,
                AlertTypeName = entity.AlertType.ToString(),
                AlertTitle = entity.AlertTitle,
                AlertMessage = entity.AlertMessage,
                DueDate = entity.DueDate,
                AlertStatus = entity.AlertStatus,
                AlertStatusName = entity.AlertStatus.ToString(),
                SeverityLevel = entity.SeverityLevel,
                SeverityLevelName = entity.SeverityLevel.ToString(),
                IsResolved = entity.IsResolved,
                ResolvedAt = entity.ResolvedAt,
                ResolvedByUserId = entity.ResolvedByUserId,
                ResolvedByUserName = entity.ResolvedByUser?.DisplayName ??
                                     entity.ResolvedByUser?.UserName ??
                                     entity.ResolvedByUser?.Email ??
                                     entity.ResolvedByUser?.UserCode,
                BlocksScheduling = entity.BlocksScheduling,
                BlocksClinicalService = entity.BlocksClinicalService,
                IsOverdue = !entity.IsResolved && entity.DueDate < DateTime.UtcNow,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                LogCount = entity.Logs.Count(x => !x.IsDelete),
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetUserDisplayName(entity.CreateBy)
            };
        }

        private WfpComplianceAlertDetailResponse MapDetailResponse(WfpComplianceAlert entity)
        {
            var response = MapResponse(entity);

            return new WfpComplianceAlertDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                WorkforceProfileCode = response.WorkforceProfileCode,
                WorkforceDisplayName = response.WorkforceDisplayName,
                SourceEntityName = response.SourceEntityName,
                SourceEntityId = response.SourceEntityId,
                AlertType = response.AlertType,
                AlertTypeName = response.AlertTypeName,
                AlertTitle = response.AlertTitle,
                AlertMessage = response.AlertMessage,
                DueDate = response.DueDate,
                AlertStatus = response.AlertStatus,
                AlertStatusName = response.AlertStatusName,
                SeverityLevel = response.SeverityLevel,
                SeverityLevelName = response.SeverityLevelName,
                IsResolved = response.IsResolved,
                ResolvedAt = response.ResolvedAt,
                ResolvedByUserId = response.ResolvedByUserId,
                ResolvedByUserName = response.ResolvedByUserName,
                BlocksScheduling = response.BlocksScheduling,
                BlocksClinicalService = response.BlocksClinicalService,
                IsOverdue = response.IsOverdue,
                Notes = response.Notes,
                IsActive = response.IsActive,
                LogCount = response.LogCount,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetUserDisplayName(entity.UpdateBy)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            CreateWfpComplianceAlertRequest request,
            Guid? currentId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.SourceEntityName))
                return (false, "SourceEntityName wajib diisi.");

            if (request.SourceEntityId == Guid.Empty)
                return (false, "SourceEntityId wajib diisi.");

            if (!Enum.IsDefined(typeof(ComplianceAlertType), request.AlertType) ||
                request.AlertType == ComplianceAlertType.Unknown)
            {
                return (false, "AlertType tidak valid.");
            }

            if (!Enum.IsDefined(typeof(ComplianceAlertSeverityLevel), request.SeverityLevel))
                return (false, "SeverityLevel tidak valid.");

            if (string.IsNullOrWhiteSpace(request.AlertTitle))
                return (false, "AlertTitle wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.AlertMessage))
                return (false, "AlertMessage wajib diisi.");

            var sourceEntityName = request.SourceEntityName.Trim();
            var dueDate = EnsureUtc(request.DueDate);

            var duplicateQuery = _dbContext.Set<WfpComplianceAlert>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.SourceEntityName == sourceEntityName &&
                    x.SourceEntityId == request.SourceEntityId &&
                    x.AlertType == request.AlertType &&
                    x.DueDate == dueDate);

            if (currentId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != currentId.Value);

            if (await duplicateQuery.AnyAsync(cancellationToken))
                return (false, "Compliance alert dengan sumber, jenis, dan due date yang sama sudah tersedia.");

            return (true, null);
        }

        private async Task<WfpComplianceAlert?> GetTrackedAlertAsync(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<WfpComplianceAlert>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);
        }

        private async Task<bool> WorkforceProfileExistsAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            return workforceProfileId != Guid.Empty &&
                   await _dbContext.MstWorkforceProfiles
                       .AsNoTracking()
                       .AnyAsync(
                           x => x.Id == workforceProfileId &&
                                x.IsActive &&
                                !x.IsDelete,
                           cancellationToken);
        }

        private void AddLog(
            Guid complianceAlertId,
            ComplianceAlertLogType logType,
            ComplianceAlertStatus? oldStatus,
            ComplianceAlertStatus? newStatus,
            string logMessage,
            Guid actor,
            string? notes,
            DateTime performedAt)
        {
            _dbContext.Set<WfpComplianceAlertLog>().Add(
                new WfpComplianceAlertLog
                {
                    Id = Guid.NewGuid(),
                    ComplianceAlertId = complianceAlertId,
                    LogType = logType,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    LogMessage = logMessage,
                    PerformedByUserId = actor == Guid.Empty ? null : actor,
                    PerformedAt = performedAt,
                    Notes = NormalizeText(notes),
                    IsActive = true,
                    CreateDateTime = performedAt,
                    CreateBy = actor,
                    IsDelete = false,
                    IsCancel = false
                });
        }

        private static bool IsValidTransition(
            ComplianceAlertStatus currentStatus,
            ComplianceAlertStatus targetStatus)
        {
            if (currentStatus == targetStatus)
                return true;

            return currentStatus switch
            {
                ComplianceAlertStatus.Open =>
                    targetStatus == ComplianceAlertStatus.InProgress ||
                    targetStatus == ComplianceAlertStatus.Resolved ||
                    targetStatus == ComplianceAlertStatus.Ignored ||
                    targetStatus == ComplianceAlertStatus.Cancelled ||
                    targetStatus == ComplianceAlertStatus.Expired,

                ComplianceAlertStatus.InProgress =>
                    targetStatus == ComplianceAlertStatus.Open ||
                    targetStatus == ComplianceAlertStatus.Resolved ||
                    targetStatus == ComplianceAlertStatus.Ignored ||
                    targetStatus == ComplianceAlertStatus.Cancelled ||
                    targetStatus == ComplianceAlertStatus.Expired,

                ComplianceAlertStatus.Resolved =>
                    targetStatus == ComplianceAlertStatus.Open,

                ComplianceAlertStatus.Ignored =>
                    targetStatus == ComplianceAlertStatus.Open,

                ComplianceAlertStatus.Expired =>
                    targetStatus == ComplianceAlertStatus.Open ||
                    targetStatus == ComplianceAlertStatus.Resolved,

                ComplianceAlertStatus.Cancelled => false,
                _ => false
            };
        }

        private static void ApplyStatus(
            WfpComplianceAlert entity,
            ComplianceAlertStatus status,
            Guid actor,
            DateTime now)
        {
            entity.AlertStatus = status;
            entity.IsResolved = status == ComplianceAlertStatus.Resolved;

            if (status == ComplianceAlertStatus.Resolved)
            {
                entity.ResolvedAt = now;
                entity.ResolvedByUserId = actor == Guid.Empty ? null : actor;
            }
            else
            {
                entity.ResolvedAt = null;
                entity.ResolvedByUserId = null;
            }

            if (status == ComplianceAlertStatus.Cancelled)
            {
                entity.IsCancel = true;
                entity.CancelDateTime = now;
                entity.CancelBy = actor;
            }
            else
            {
                entity.IsCancel = false;
                entity.CancelDateTime = null;
                entity.CancelBy = Guid.Empty;
            }
        }

        private static List<WfpComplianceAlertEnumOptionResponse> BuildEnumOptions<TEnum>()
            where TEnum : struct, Enum
        {
            return Enum.GetValues<TEnum>()
                .Select(value => new WfpComplianceAlertEnumOptionResponse
                {
                    Value = Convert.ToInt32(value),
                    Name = value.ToString(),
                    Label = BuildLabel(value.ToString())
                })
                .ToList();
        }

        private static string BuildLabel(string value)
        {
            return string.Concat(
                value.Select((character, index) =>
                    index > 0 && char.IsUpper(character)
                        ? " " + character
                        : character.ToString()));
        }

        private string? GetUserDisplayName(Guid userId)
        {
            if (userId == Guid.Empty)
                return null;

            return _dbContext.Users
                .Where(x => x.Id == userId)
                .Select(x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode)
                .FirstOrDefault();
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

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;

            return (pageNumber, pageSize);
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
