using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/emergency-installation-management/emergency-triage-details")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Triage Detail",
        AreaName = "HealthServices",
        ControllerName = "EmergencyTriageDetail",
        Description = "Mengelola indikator klinis yang digunakan pada triage pasien IGD",
        SortOrder = 3
    )]
    [Tags("Health Services / Emergency Installation Management / Emergency Triage Detail")]
    public class EmergencyTriageDetailController : ControllerBase
    {
        private const string LogCategory = "HealthServices.EmergencyInstallation";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public EmergencyTriageDetailController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencyTriageDetailResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Triage Detail", Description = "Melihat data detail triage IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyTriageDetail", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? emergencyTriageId,
            [FromQuery] Guid? triageIndicatorId,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default
        )
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);
            IQueryable<TrxEmergencyTriageDetail> query = _dbContext.Set<TrxEmergencyTriageDetail>().AsNoTracking().Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.IndicatorCodeSnapshot.ToLower().Contains(keyword) ||
                    x.IndicatorNameSnapshot.ToLower().Contains(keyword) ||
                    (x.IndicatorGroupSnapshot != null && x.IndicatorGroupSnapshot.ToLower().Contains(keyword)) ||
                    (x.ObservedValue != null && x.ObservedValue.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }

            if (emergencyTriageId.HasValue && emergencyTriageId.Value != Guid.Empty)
                query = query.Where(x => x.EmergencyTriageId == emergencyTriageId.Value);

            if (triageIndicatorId.HasValue && triageIndicatorId.Value != Guid.Empty)
                query = query.Where(x => x.TriageIndicatorId == triageIndicatorId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "sequence" => descending ? query.OrderByDescending(x => x.Sequence) : query.OrderBy(x => x.Sequence),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.Sequence) : query.OrderBy(x => x.Sequence)
            };

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<EmergencyTriageDetailResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<EmergencyTriageDetailResponse>>.Ok(result, "Data detail triage IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTriageDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Triage Detail", Description = "Melihat detail detail triage IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyTriageDetail", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyTriageDetail>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data detail triage IGD tidak ditemukan."));

            return Ok(ApiResponse<EmergencyTriageDetailResponse>.Ok(ToResponse(entity), "Detail detail triage IGD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTriageDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Triage Detail", Description = "Membuat detail triage IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyTriageDetail", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyTriageDetailRequest request, CancellationToken cancellationToken = default)
        {
            var validationMessage = await ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new TrxEmergencyTriageDetail
            {
                Id = Guid.NewGuid(),
                EmergencyTriageId = request.EmergencyTriageId,
                TriageIndicatorId = request.TriageIndicatorId,
                IndicatorCodeSnapshot = NormalizeText(request.IndicatorCodeSnapshot) ?? string.Empty,
                IndicatorNameSnapshot = NormalizeText(request.IndicatorNameSnapshot) ?? string.Empty,
                IndicatorGroupSnapshot = NormalizeText(request.IndicatorGroupSnapshot),
                ObservedValue = NormalizeText(request.ObservedValue),
                Score = request.Score,
                IsMatched = request.IsMatched,
                Notes = NormalizeText(request.Notes),
                Sequence = request.Sequence,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxEmergencyTriageDetail>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data detail triage IGD gagal disimpan karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTriageDetail.Create",
                "Membuat data Emergency Triage Detail.",
                new { EntityId = entity.Id, Controller = "EmergencyTriageDetail", Action = "Create" }
            );

            return Ok(ApiResponse<EmergencyTriageDetailResponse>.Ok(ToResponse(entity), "Data detail triage IGD berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTriageDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Triage Detail", Description = "Mengubah detail triage IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyTriageDetail", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmergencyTriageDetailRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyTriageDetail>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data detail triage IGD tidak ditemukan."));

            var validationMessage = await ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.EmergencyTriageId = request.EmergencyTriageId;
            entity.TriageIndicatorId = request.TriageIndicatorId;
            entity.IndicatorCodeSnapshot = NormalizeText(request.IndicatorCodeSnapshot) ?? string.Empty;
            entity.IndicatorNameSnapshot = NormalizeText(request.IndicatorNameSnapshot) ?? string.Empty;
            entity.IndicatorGroupSnapshot = NormalizeText(request.IndicatorGroupSnapshot);
            entity.ObservedValue = NormalizeText(request.ObservedValue);
            entity.Score = request.Score;
            entity.IsMatched = request.IsMatched;
            entity.Notes = NormalizeText(request.Notes);
            entity.Sequence = request.Sequence;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data detail triage IGD gagal diubah karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTriageDetail.Update",
                "Mengubah data Emergency Triage Detail.",
                new { EntityId = id, Controller = "EmergencyTriageDetail", Action = "Update" }
            );

            return Ok(ApiResponse<EmergencyTriageDetailResponse>.Ok(ToResponse(entity), "Data detail triage IGD berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Emergency Triage Detail", Description = "Menghapus detail triage IGD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmergencyTriageDetail", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyTriageDetail>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data detail triage IGD tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTriageDetail.Delete",
                "Menghapus data Emergency Triage Detail.",
                new { EntityId = id, Controller = "EmergencyTriageDetail", Action = "Delete" }
            );

            return Ok(ApiResponse<object>.Ok(null, "Data detail triage IGD berhasil dihapus."));
        }

        private async Task<string?> ValidateRequestAsync(CreateEmergencyTriageDetailRequest request, CancellationToken cancellationToken)
        {
            if (request.EmergencyTriageId == Guid.Empty)
                return "EmergencyTriageId wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.IndicatorCodeSnapshot))
                return "IndicatorCodeSnapshot wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.IndicatorNameSnapshot))
                return "IndicatorNameSnapshot wajib diisi.";

            if (!await _dbContext.Set<TrxEmergencyTriage>().AsNoTracking().AnyAsync(x => x.Id == request.EmergencyTriageId && !x.IsDelete, cancellationToken))
                return "EmergencyTriageId tidak ditemukan.";

            if (request.TriageIndicatorId.HasValue && request.TriageIndicatorId.Value != Guid.Empty &&
                !await _dbContext.Set<EmgTriageIndicator>().AsNoTracking().AnyAsync(x => x.Id == request.TriageIndicatorId.Value && !x.IsDelete, cancellationToken))
                return "TriageIndicatorId tidak ditemukan.";

            return null;
        }

        private Task<string?> ValidateRequestAsync(UpdateEmergencyTriageDetailRequest request, CancellationToken cancellationToken)
            => ValidateRequestAsync((CreateEmergencyTriageDetailRequest)request, cancellationToken);

        private static EmergencyTriageDetailResponse ToResponse(TrxEmergencyTriageDetail x)
        {
            return new EmergencyTriageDetailResponse
            {
                Id = x.Id,
                EmergencyTriageId = x.EmergencyTriageId,
                TriageIndicatorId = x.TriageIndicatorId,
                IndicatorCodeSnapshot = x.IndicatorCodeSnapshot,
                IndicatorNameSnapshot = x.IndicatorNameSnapshot,
                IndicatorGroupSnapshot = x.IndicatorGroupSnapshot,
                ObservedValue = x.ObservedValue,
                Score = x.Score,
                IsMatched = x.IsMatched,
                Notes = x.Notes,
                Sequence = x.Sequence,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);
            return (pageNumber, pageSize);
        }

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string GenerateDocumentNumber(string prefix, DateTime now)
            => $"{prefix}-{now:yyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
