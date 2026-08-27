using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/emergency-installation-management/master-data/emergency-disposition-types")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Disposition Type",
        AreaName = "HealthServices",
        ControllerName = "EmergencyDispositionType",
        Description = "Mengelola master jenis tindak lanjut pelayanan IGD",
        SortOrder = 14
    )]
    [Tags("Health Services / Emergency Installation Management / Master Data / Emergency Disposition Type")]
    public class EmergencyDispositionTypeController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.EmergencyInstallation";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public EmergencyDispositionTypeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencyDispositionTypeResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Disposition Type", Description = "Melihat data master tindak lanjut IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyDispositionType", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default
        )
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);
            IQueryable<MstEmergencyDispositionType> query = _dbContext.Set<MstEmergencyDispositionType>().AsNoTracking().Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.Code.ToLower().Contains(keyword) ||
                    x.Name.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

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

            var result = new PagedResult<EmergencyDispositionTypeResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<EmergencyDispositionTypeResponse>>.Ok(result, "Data master tindak lanjut IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDispositionTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Disposition Type", Description = "Melihat detail master tindak lanjut IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyDispositionType", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstEmergencyDispositionType>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data master tindak lanjut IGD tidak ditemukan."));

            return Ok(ApiResponse<EmergencyDispositionTypeResponse>.Ok(ToResponse(entity), "Detail master tindak lanjut IGD berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<EmergencyDispositionTypeOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Disposition Type Options", Description = "Melihat pilihan aktif master tindak lanjut IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyDispositionType", "Read")]
        public async Task<IActionResult> GetOptions(CancellationToken cancellationToken = default)
        {
            var entities = await _dbContext.Set<MstEmergencyDispositionType>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.Sequence)
                .ToListAsync(cancellationToken);

            var options = entities.Select(x => new EmergencyDispositionTypeOptionResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Sequence = x.Sequence
            }).ToList();

            return Ok(ApiResponse<List<EmergencyDispositionTypeOptionResponse>>.Ok(options, "Pilihan master tindak lanjut IGD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDispositionTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Disposition Type", Description = "Membuat master tindak lanjut IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyDispositionType", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyDispositionTypeRequest request, CancellationToken cancellationToken = default)
        {
            var validationMessage = await ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedCode = NormalizeText(request.Code) ?? string.Empty;
            if (await _dbContext.Set<MstEmergencyDispositionType>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.Code.ToLower() == normalizedCode.ToLower(), cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Kode sudah digunakan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstEmergencyDispositionType
            {
                Id = Guid.NewGuid(),
                Code = NormalizeText(request.Code) ?? string.Empty,
                Name = NormalizeText(request.Name) ?? string.Empty,
                RequiresDestinationServiceUnit = request.RequiresDestinationServiceUnit,
                RequiresReferralFacility = request.RequiresReferralFacility,
                ClosesEmergencyVisit = request.ClosesEmergencyVisit,
                Sequence = request.Sequence,
                Description = NormalizeText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstEmergencyDispositionType>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data master tindak lanjut IGD gagal disimpan karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyDispositionType.Create",
                "Membuat data Emergency Disposition Type.",
                new { EntityId = entity.Id, Controller = "EmergencyDispositionType", Action = "Create" }
            );

            return Ok(ApiResponse<EmergencyDispositionTypeResponse>.Ok(ToResponse(entity), "Data master tindak lanjut IGD berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDispositionTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Disposition Type", Description = "Mengubah master tindak lanjut IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyDispositionType", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmergencyDispositionTypeRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstEmergencyDispositionType>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data master tindak lanjut IGD tidak ditemukan."));

            var validationMessage = await ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedCode = NormalizeText(request.Code) ?? string.Empty;
            if (await _dbContext.Set<MstEmergencyDispositionType>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.Code.ToLower() == normalizedCode.ToLower() && x.Id != id, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Kode sudah digunakan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.Code = NormalizeText(request.Code) ?? string.Empty;
            entity.Name = NormalizeText(request.Name) ?? string.Empty;
            entity.RequiresDestinationServiceUnit = request.RequiresDestinationServiceUnit;
            entity.RequiresReferralFacility = request.RequiresReferralFacility;
            entity.ClosesEmergencyVisit = request.ClosesEmergencyVisit;
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
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data master tindak lanjut IGD gagal diubah karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyDispositionType.Update",
                "Mengubah data Emergency Disposition Type.",
                new { EntityId = id, Controller = "EmergencyDispositionType", Action = "Update" }
            );

            return Ok(ApiResponse<EmergencyDispositionTypeResponse>.Ok(ToResponse(entity), "Data master tindak lanjut IGD berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Emergency Disposition Type", Description = "Menghapus master tindak lanjut IGD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmergencyDispositionType", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstEmergencyDispositionType>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data master tindak lanjut IGD tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyDispositionType.Delete",
                "Menghapus data Emergency Disposition Type.",
                new { EntityId = id, Controller = "EmergencyDispositionType", Action = "Delete" }
            );

            return Ok(ApiResponse<object>.Ok(null, "Data master tindak lanjut IGD berhasil dihapus."));
        }

        private async Task<string?> ValidateRequestAsync(CreateEmergencyDispositionTypeRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                return "Code wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.Name))
                return "Name wajib diisi.";

            return null;
        }

        private Task<string?> ValidateRequestAsync(UpdateEmergencyDispositionTypeRequest request, CancellationToken cancellationToken)
            => ValidateRequestAsync((CreateEmergencyDispositionTypeRequest)request, cancellationToken);

        private static EmergencyDispositionTypeResponse ToResponse(MstEmergencyDispositionType x)
        {
            return new EmergencyDispositionTypeResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                RequiresDestinationServiceUnit = x.RequiresDestinationServiceUnit,
                RequiresReferralFacility = x.RequiresReferralFacility,
                ClosesEmergencyVisit = x.ClosesEmergencyVisit,
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
