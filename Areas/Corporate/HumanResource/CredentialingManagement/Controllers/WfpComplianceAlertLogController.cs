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
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/compliance-alerts/{complianceAlertId:guid}/logs")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_CREDENTIALING_MANAGEMENT",
        moduleName: "Human Resource Credentialing Management",
        displayName: "Workforce Compliance Alert Log",
        AreaName = "Corporate",
        ControllerName = "WorkforceComplianceAlertLog",
        Description = "Corporate human resource workforce compliance alert log",
        SortOrder = 21
    )]
    [Tags("Corporate / Human Resource / Credentialing Management / Compliance Alert Log")]
    public class WfpComplianceAlertLogController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public WfpComplianceAlertLogController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpComplianceAlertLogResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Compliance Alert Log", Description = "Melihat log compliance alert workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceComplianceAlertLog", "Read")]
        public async Task<IActionResult> GetLogs(
            Guid workforceProfileId,
            Guid complianceAlertId,
            [FromQuery] ComplianceAlertLogType? logType,
            [FromQuery] string? search,
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await AlertExistsAsync(workforceProfileId, complianceAlertId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert workforce tidak ditemukan."));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(complianceAlertId);

            if (logType.HasValue)
                query = query.Where(x => x.LogType == logType.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.LogMessage.ToLower().Contains(keyword) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync(cancellationToken);

            var ordered = string.Equals(
                sortDirection,
                "asc",
                StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.PerformedAt)
                : query.OrderByDescending(x => x.PerformedAt);

            var rows = await ordered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = rows.Select(MapResponse).ToList();

            var result = new PagedResult<WfpComplianceAlertLogResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<WfpComplianceAlertLogResponse>>.Ok(
                result,
                "Log compliance alert workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpComplianceAlertLogDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Compliance Alert Log", Description = "Melihat detail log compliance alert workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceComplianceAlertLog", "Read")]
        public async Task<IActionResult> GetLogById(
            Guid workforceProfileId,
            Guid complianceAlertId,
            Guid id,
            CancellationToken cancellationToken)
        {
            if (!await AlertExistsAsync(workforceProfileId, complianceAlertId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert workforce tidak ditemukan."));
            }

            var entity = await BuildBaseQuery(complianceAlertId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Log compliance alert workforce tidak ditemukan."));
            }

            var response = MapResponse(entity);

            var result = new WfpComplianceAlertLogDetailResponse
            {
                Id = response.Id,
                ComplianceAlertId = response.ComplianceAlertId,
                LogType = response.LogType,
                LogTypeName = response.LogTypeName,
                OldStatus = response.OldStatus,
                OldStatusName = response.OldStatusName,
                NewStatus = response.NewStatus,
                NewStatusName = response.NewStatusName,
                LogMessage = response.LogMessage,
                PerformedByUserId = response.PerformedByUserId,
                PerformedByUserName = response.PerformedByUserName,
                PerformedAt = response.PerformedAt,
                Notes = response.Notes,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetUserDisplayName(entity.UpdateBy)
            };

            return Ok(ApiResponse<WfpComplianceAlertLogDetailResponse>.Ok(
                result,
                "Detail log compliance alert workforce berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpComplianceAlertLogDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Workforce Compliance Alert Log", Description = "Menambahkan catatan compliance alert workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceComplianceAlertLog", "Create")]
        public async Task<IActionResult> CreateLog(
            Guid workforceProfileId,
            Guid complianceAlertId,
            [FromBody] CreateWfpComplianceAlertLogRequest request,
            CancellationToken cancellationToken)
        {
            var alert = await _dbContext.Set<WfpComplianceAlert>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == complianceAlertId &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);

            if (alert == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Compliance alert workforce tidak ditemukan."));
            }

            if (!Enum.IsDefined(typeof(ComplianceAlertLogType), request.LogType))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "LogType tidak valid."));
            }

            if (request.LogType == ComplianceAlertLogType.StatusChanged ||
                request.LogType == ComplianceAlertLogType.Resolved ||
                request.LogType == ComplianceAlertLogType.Reopened ||
                request.LogType == ComplianceAlertLogType.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Log perubahan status dibuat otomatis melalui endpoint status compliance alert."));
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            var entity = new WfpComplianceAlertLog
            {
                Id = Guid.NewGuid(),
                ComplianceAlertId = complianceAlertId,
                LogType = request.LogType,
                OldStatus = alert.AlertStatus,
                NewStatus = alert.AlertStatus,
                LogMessage = request.LogMessage.Trim(),
                PerformedByUserId = actor == Guid.Empty ? null : actor,
                PerformedAt = now,
                Notes = NormalizeText(request.Notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpComplianceAlertLog>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetLogById(
                workforceProfileId,
                complianceAlertId,
                entity.Id,
                cancellationToken);
        }

        private IQueryable<WfpComplianceAlertLog> BuildBaseQuery(Guid complianceAlertId)
        {
            return _dbContext.Set<WfpComplianceAlertLog>()
                .AsNoTracking()
                .Include(x => x.PerformedByUser)
                .Where(x =>
                    x.ComplianceAlertId == complianceAlertId &&
                    !x.IsDelete);
        }

        private WfpComplianceAlertLogResponse MapResponse(WfpComplianceAlertLog entity)
        {
            return new WfpComplianceAlertLogResponse
            {
                Id = entity.Id,
                ComplianceAlertId = entity.ComplianceAlertId,
                LogType = entity.LogType,
                LogTypeName = entity.LogType.ToString(),
                OldStatus = entity.OldStatus,
                OldStatusName = entity.OldStatus?.ToString(),
                NewStatus = entity.NewStatus,
                NewStatusName = entity.NewStatus?.ToString(),
                LogMessage = entity.LogMessage,
                PerformedByUserId = entity.PerformedByUserId,
                PerformedByUserName = entity.PerformedByUser?.DisplayName ??
                                      entity.PerformedByUser?.UserName ??
                                      entity.PerformedByUser?.Email ??
                                      entity.PerformedByUser?.UserCode,
                PerformedAt = entity.PerformedAt,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime
            };
        }

        private async Task<bool> AlertExistsAsync(
            Guid workforceProfileId,
            Guid complianceAlertId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<WfpComplianceAlert>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == complianceAlertId &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);
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

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
