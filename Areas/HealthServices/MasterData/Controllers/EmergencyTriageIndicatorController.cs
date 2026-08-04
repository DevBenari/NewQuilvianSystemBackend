using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/emergency-installation-management/emergency-triage-indicators")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Emergency Triage Indicator",
        AreaName = "HealthServices",
        ControllerName = "EmergencyTriageIndicator",
        Description = "Mengelola master indikator klinis triage IGD",
        SortOrder = 2
    )]
    [Tags("Health Services / Master Data / Emergency Installation Management / Emergency Triage Indicator")]
    public class EmergencyTriageIndicatorController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.EmergencyInstallation";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public EmergencyTriageIndicatorController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencyTriageIndicatorResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Triage Indicator", Description = "Melihat data master indikator triage IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyTriageIndicator", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? triageLevelId,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default
        )
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);
            IQueryable<MstEmergencyTriageIndicator> query = _dbContext.Set<MstEmergencyTriageIndicator>().AsNoTracking().Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.Code.ToLower().Contains(keyword) ||
                    x.Name.ToLower().Contains(keyword) ||
                    (x.IndicatorGroup != null && x.IndicatorGroup.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (triageLevelId.HasValue && triageLevelId.Value != Guid.Empty)
                query = query.Where(x => x.TriageLevelId == triageLevelId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "code" => descending ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
                "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                "sequence" => descending ? query.OrderByDescending(x => x.Sequence) : query.OrderBy(x => x.Sequence),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.Sequence) : query.OrderBy(x => x.Sequence)
            };

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<EmergencyTriageIndicatorResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<EmergencyTriageIndicatorResponse>>.Ok(result, "Data master indikator triage IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTriageIndicatorResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Triage Indicator", Description = "Melihat detail master indikator triage IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyTriageIndicator", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstEmergencyTriageIndicator>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data master indikator triage IGD tidak ditemukan."));

            return Ok(ApiResponse<EmergencyTriageIndicatorResponse>.Ok(ToResponse(entity), "Detail master indikator triage IGD berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<EmergencyTriageIndicatorOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Triage Indicator Options", Description = "Melihat pilihan aktif master indikator triage IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyTriageIndicator", "Read")]
        public async Task<IActionResult> GetOptions(CancellationToken cancellationToken = default)
        {
            var entities = await _dbContext.Set<MstEmergencyTriageIndicator>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.Sequence)
                .ToListAsync(cancellationToken);

            var options = entities.Select(x => new EmergencyTriageIndicatorOptionResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Sequence = x.Sequence
            }).ToList();

            return Ok(ApiResponse<List<EmergencyTriageIndicatorOptionResponse>>.Ok(options, "Pilihan master indikator triage IGD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTriageIndicatorResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Triage Indicator", Description = "Membuat master indikator triage IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyTriageIndicator", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyTriageIndicatorRequest request, CancellationToken cancellationToken = default)
        {
            var validationMessage = await ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedCode = NormalizeText(request.Code) ?? string.Empty;
            if (await _dbContext.Set<MstEmergencyTriageIndicator>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.Code.ToLower() == normalizedCode.ToLower(), cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Kode sudah digunakan."));

            if (await _dbContext.Set<MstEmergencyTriageIndicator>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.TriageLevelId == request.TriageLevelId && x.Code.ToLower() == normalizedCode.ToLower(), cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Kode indikator pada level triage tersebut sudah digunakan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstEmergencyTriageIndicator
            {
                Id = Guid.NewGuid(),
                TriageLevelId = request.TriageLevelId,
                Code = NormalizeText(request.Code) ?? string.Empty,
                Name = NormalizeText(request.Name) ?? string.Empty,
                IndicatorGroup = NormalizeText(request.IndicatorGroup),
                Sequence = request.Sequence,
                Description = NormalizeText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstEmergencyTriageIndicator>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data master indikator triage IGD gagal disimpan karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTriageIndicator.Create",
                "Membuat data Emergency Triage Indicator.",
                new { EntityId = entity.Id, Controller = "EmergencyTriageIndicator", Action = "Create" }
            );

            return Ok(ApiResponse<EmergencyTriageIndicatorResponse>.Ok(ToResponse(entity), "Data master indikator triage IGD berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTriageIndicatorResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Triage Indicator", Description = "Mengubah master indikator triage IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyTriageIndicator", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmergencyTriageIndicatorRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstEmergencyTriageIndicator>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data master indikator triage IGD tidak ditemukan."));

            var validationMessage = await ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedCode = NormalizeText(request.Code) ?? string.Empty;
            if (await _dbContext.Set<MstEmergencyTriageIndicator>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.Code.ToLower() == normalizedCode.ToLower() && x.Id != id, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Kode sudah digunakan."));

            if (await _dbContext.Set<MstEmergencyTriageIndicator>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.TriageLevelId == request.TriageLevelId && x.Code.ToLower() == normalizedCode.ToLower() && x.Id != id, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Kode indikator pada level triage tersebut sudah digunakan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.TriageLevelId = request.TriageLevelId;
            entity.Code = NormalizeText(request.Code) ?? string.Empty;
            entity.Name = NormalizeText(request.Name) ?? string.Empty;
            entity.IndicatorGroup = NormalizeText(request.IndicatorGroup);
            entity.Sequence = request.Sequence;
            entity.Description = NormalizeText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data master indikator triage IGD gagal diubah karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTriageIndicator.Update",
                "Mengubah data Emergency Triage Indicator.",
                new { EntityId = id, Controller = "EmergencyTriageIndicator", Action = "Update" }
            );

            return Ok(ApiResponse<EmergencyTriageIndicatorResponse>.Ok(ToResponse(entity), "Data master indikator triage IGD berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Emergency Triage Indicator", Description = "Menghapus master indikator triage IGD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmergencyTriageIndicator", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstEmergencyTriageIndicator>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data master indikator triage IGD tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTriageIndicator.Delete",
                "Menghapus data Emergency Triage Indicator.",
                new { EntityId = id, Controller = "EmergencyTriageIndicator", Action = "Delete" }
            );

            return Ok(ApiResponse<object>.Ok(null, "Data master indikator triage IGD berhasil dihapus."));
        }

        private async Task<string?> ValidateRequestAsync(CreateEmergencyTriageIndicatorRequest request, CancellationToken cancellationToken)
        {
            if (request.TriageLevelId == Guid.Empty)
                return "TriageLevelId wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.Code))
                return "Code wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.Name))
                return "Name wajib diisi.";

            if (!await _dbContext.Set<MstEmergencyTriageLevel>().AsNoTracking().AnyAsync(x => x.Id == request.TriageLevelId && !x.IsDelete, cancellationToken))
                return "TriageLevelId tidak ditemukan.";

            return null;
        }

        private Task<string?> ValidateRequestAsync(UpdateEmergencyTriageIndicatorRequest request, CancellationToken cancellationToken)
            => ValidateRequestAsync((CreateEmergencyTriageIndicatorRequest)request, cancellationToken);

        private static EmergencyTriageIndicatorResponse ToResponse(MstEmergencyTriageIndicator x)
        {
            return new EmergencyTriageIndicatorResponse
            {
                Id = x.Id,
                TriageLevelId = x.TriageLevelId,
                Code = x.Code,
                Name = x.Name,
                IndicatorGroup = x.IndicatorGroup,
                Sequence = x.Sequence,
                Description = x.Description,
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
