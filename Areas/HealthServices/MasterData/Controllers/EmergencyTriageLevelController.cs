using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
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
    [Route("api/v1/health-services/master-data/emergency-installation-management/emergency-triage-levels")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Emergency Triage Level",
        AreaName = "HealthServices",
        ControllerName = "EmergencyTriageLevel",
        Description = "Mengelola master level triage ATS atau ESI",
        SortOrder = 1
    )]
    [Tags("Health Services / Master Data / Emergency Installation Management / Emergency Triage Level")]
    public class EmergencyTriageLevelController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.EmergencyInstallation";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public EmergencyTriageLevelController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencyTriageLevelResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Triage Level", Description = "Melihat data master level triage IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyTriageLevel", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] EmergencyTriageSystem? triageSystem,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default
        )
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);
            IQueryable<MstEmergencyTriageLevel> query = _dbContext.Set<MstEmergencyTriageLevel>().AsNoTracking().Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.Code.ToLower().Contains(keyword) ||
                    x.Name.ToLower().Contains(keyword) ||
                    x.ColorName.ToLower().Contains(keyword) ||
                    (x.ColorHex != null && x.ColorHex.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (triageSystem.HasValue)
                query = query.Where(x => x.TriageSystem == triageSystem.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "code" => descending ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
                "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                "sequence" => descending ? query.OrderByDescending(x => x.Sequence) : query.OrderBy(x => x.Sequence),
                "level" => descending ? query.OrderByDescending(x => x.Level) : query.OrderBy(x => x.Level),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.Sequence) : query.OrderBy(x => x.Sequence)
            };

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<EmergencyTriageLevelResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<EmergencyTriageLevelResponse>>.Ok(result, "Data master level triage IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTriageLevelResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Triage Level", Description = "Melihat detail master level triage IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyTriageLevel", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstEmergencyTriageLevel>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data master level triage IGD tidak ditemukan."));

            return Ok(ApiResponse<EmergencyTriageLevelResponse>.Ok(ToResponse(entity), "Detail master level triage IGD berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<EmergencyTriageLevelOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Triage Level Options", Description = "Melihat pilihan aktif master level triage IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyTriageLevel", "Read")]
        public async Task<IActionResult> GetOptions(CancellationToken cancellationToken = default)
        {
            var entities = await _dbContext.Set<MstEmergencyTriageLevel>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.Sequence)
                .ToListAsync(cancellationToken);

            var options = entities.Select(x => new EmergencyTriageLevelOptionResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Level = x.Level,
                ColorName = x.ColorName,
                ColorHex = x.ColorHex,
                TriageSystem = x.TriageSystem,
                Sequence = x.Sequence
            }).ToList();

            return Ok(ApiResponse<List<EmergencyTriageLevelOptionResponse>>.Ok(options, "Pilihan master level triage IGD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTriageLevelResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Triage Level", Description = "Membuat master level triage IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyTriageLevel", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyTriageLevelRequest request, CancellationToken cancellationToken = default)
        {
            var validationMessage = await ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedCode = NormalizeText(request.Code) ?? string.Empty;
            if (await _dbContext.Set<MstEmergencyTriageLevel>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.Code.ToLower() == normalizedCode.ToLower(), cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Kode sudah digunakan."));

            if (await _dbContext.Set<MstEmergencyTriageLevel>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.TriageSystem == request.TriageSystem && x.Level == request.Level, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Level pada sistem triage tersebut sudah tersedia."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstEmergencyTriageLevel
            {
                Id = Guid.NewGuid(),
                TriageSystem = request.TriageSystem,
                Level = request.Level,
                Code = NormalizeText(request.Code) ?? string.Empty,
                Name = NormalizeText(request.Name) ?? string.Empty,
                ColorName = NormalizeText(request.ColorName) ?? string.Empty,
                ColorHex = NormalizeText(request.ColorHex),
                MaxWaitingMinutes = request.MaxWaitingMinutes,
                AllowsTreatmentBeforeRegistration = request.AllowsTreatmentBeforeRegistration,
                Sequence = request.Sequence,
                Description = NormalizeText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstEmergencyTriageLevel>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data master level triage IGD gagal disimpan karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTriageLevel.Create",
                "Membuat data Emergency Triage Level.",
                new { EntityId = entity.Id, Controller = "EmergencyTriageLevel", Action = "Create" }
            );

            return Ok(ApiResponse<EmergencyTriageLevelResponse>.Ok(ToResponse(entity), "Data master level triage IGD berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTriageLevelResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Triage Level", Description = "Mengubah master level triage IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyTriageLevel", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmergencyTriageLevelRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstEmergencyTriageLevel>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data master level triage IGD tidak ditemukan."));

            var validationMessage = await ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedCode = NormalizeText(request.Code) ?? string.Empty;
            if (await _dbContext.Set<MstEmergencyTriageLevel>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.Code.ToLower() == normalizedCode.ToLower() && x.Id != id, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Kode sudah digunakan."));

            if (await _dbContext.Set<MstEmergencyTriageLevel>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.TriageSystem == request.TriageSystem && x.Level == request.Level && x.Id != id, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Level pada sistem triage tersebut sudah tersedia."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.TriageSystem = request.TriageSystem;
            entity.Level = request.Level;
            entity.Code = NormalizeText(request.Code) ?? string.Empty;
            entity.Name = NormalizeText(request.Name) ?? string.Empty;
            entity.ColorName = NormalizeText(request.ColorName) ?? string.Empty;
            entity.ColorHex = NormalizeText(request.ColorHex);
            entity.MaxWaitingMinutes = request.MaxWaitingMinutes;
            entity.AllowsTreatmentBeforeRegistration = request.AllowsTreatmentBeforeRegistration;
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
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data master level triage IGD gagal diubah karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTriageLevel.Update",
                "Mengubah data Emergency Triage Level.",
                new { EntityId = id, Controller = "EmergencyTriageLevel", Action = "Update" }
            );

            return Ok(ApiResponse<EmergencyTriageLevelResponse>.Ok(ToResponse(entity), "Data master level triage IGD berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Emergency Triage Level", Description = "Menghapus master level triage IGD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmergencyTriageLevel", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstEmergencyTriageLevel>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data master level triage IGD tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTriageLevel.Delete",
                "Menghapus data Emergency Triage Level.",
                new { EntityId = id, Controller = "EmergencyTriageLevel", Action = "Delete" }
            );

            return Ok(ApiResponse<object>.Ok(null, "Data master level triage IGD berhasil dihapus."));
        }

        private async Task<string?> ValidateRequestAsync(CreateEmergencyTriageLevelRequest request, CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(typeof(EmergencyTriageSystem), request.TriageSystem))
                return "Nilai TriageSystem tidak valid.";

            if (string.IsNullOrWhiteSpace(request.Code))
                return "Code wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.Name))
                return "Name wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.ColorName))
                return "ColorName wajib diisi.";

            // Kosong diperbolehkan dan berarti "target belum ditetapkan SOP". Yang dilarang
            // hanya angka negatif. Nol tetap sah dan berarti "harus dilayani seketika".
            if (request.MaxWaitingMinutes.HasValue && request.MaxWaitingMinutes.Value < 0)
                return "MaxWaitingMinutes tidak boleh negatif.";

            return null;
        }

        private Task<string?> ValidateRequestAsync(UpdateEmergencyTriageLevelRequest request, CancellationToken cancellationToken)
            => ValidateRequestAsync((CreateEmergencyTriageLevelRequest)request, cancellationToken);

        private static EmergencyTriageLevelResponse ToResponse(MstEmergencyTriageLevel x)
        {
            return new EmergencyTriageLevelResponse
            {
                Id = x.Id,
                TriageSystem = x.TriageSystem,
                Level = x.Level,
                Code = x.Code,
                Name = x.Name,
                ColorName = x.ColorName,
                ColorHex = x.ColorHex,
                MaxWaitingMinutes = x.MaxWaitingMinutes,
                AllowsTreatmentBeforeRegistration = x.AllowsTreatmentBeforeRegistration,
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
