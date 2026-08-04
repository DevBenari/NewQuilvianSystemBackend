using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
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
    [Route("api/v1/health-services/master-data/emergency-installation-management/emergency-settings")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Emergency Setting",
        AreaName = "HealthServices",
        ControllerName = "EmergencySetting",
        Description = "Mengelola pengaturan operasional dan alur pelayanan IGD",
        SortOrder = 6
    )]
    [Tags("Health Services / Master Data / Emergency Installation Management / Emergency Setting")]
    public class EmergencySettingController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.EmergencyInstallation";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly EmergencySettingService _emergencySettingService;

        public EmergencySettingController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            EmergencySettingService emergencySettingService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _emergencySettingService = emergencySettingService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencySettingResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Setting", Description = "Melihat data pengaturan IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencySetting", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? defaultEmergencyServiceUnitId,
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
            IQueryable<MstEmergencySetting> query = _dbContext.Set<MstEmergencySetting>().AsNoTracking().Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.Code.ToLower().Contains(keyword) ||
                    x.Name.ToLower().Contains(keyword) ||
                    x.TemporaryPatientNumberPrefix.ToLower().Contains(keyword) ||
                    x.EmergencyVisitNumberPrefix.ToLower().Contains(keyword) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }

            if (defaultEmergencyServiceUnitId.HasValue && defaultEmergencyServiceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.DefaultEmergencyServiceUnitId == defaultEmergencyServiceUnitId.Value);

            if (triageSystem.HasValue)
                query = query.Where(x => x.TriageSystem == triageSystem.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "code" => descending ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
                "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
            };

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<EmergencySettingResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<EmergencySettingResponse>>.Ok(result, "Data pengaturan IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencySettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Setting", Description = "Melihat detail pengaturan IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencySetting", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstEmergencySetting>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data pengaturan IGD tidak ditemukan."));

            return Ok(ApiResponse<EmergencySettingResponse>.Ok(ToResponse(entity), "Detail pengaturan IGD berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<EmergencySettingOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Setting Options", Description = "Melihat pilihan aktif pengaturan IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencySetting", "Read")]
        public async Task<IActionResult> GetOptions(CancellationToken cancellationToken = default)
        {
            var entities = await _dbContext.Set<MstEmergencySetting>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var options = entities.Select(x => new EmergencySettingOptionResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                TriageSystem = x.TriageSystem
            }).ToList();

            return Ok(ApiResponse<List<EmergencySettingOptionResponse>>.Ok(options, "Pilihan pengaturan IGD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencySettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Setting", Description = "Membuat pengaturan IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencySetting", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencySettingRequest request, CancellationToken cancellationToken = default)
        {
            var validationMessage = await _emergencySettingService.ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedCode = NormalizeText(request.Code) ?? string.Empty;
            if (await _dbContext.Set<MstEmergencySetting>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.Code.ToLower() == normalizedCode.ToLower(), cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Kode sudah digunakan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            if (request.IsDefault)
            {
                await _emergencySettingService.ClearOtherDefaultsAsync(
                    exceptId: null,
                    actorUserId: actorUserId,
                    now: now,
                    cancellationToken: cancellationToken);
            }

            var entity = new MstEmergencySetting
            {
                Id = Guid.NewGuid(),
                Code = NormalizeText(request.Code) ?? string.Empty,
                Name = NormalizeText(request.Name) ?? string.Empty,
                DefaultEmergencyServiceUnitId = request.DefaultEmergencyServiceUnitId,
                TriageSystem = request.TriageSystem,
                AllowProvisionalRegistration = request.AllowProvisionalRegistration,
                AllowUnknownPatient = request.AllowUnknownPatient,
                AutoCreateProvisionalEncounter = request.AutoCreateProvisionalEncounter,
                ImmediateCareLevelThreshold = request.ImmediateCareLevelThreshold,
                RequireRegistrationBeforeTreatmentFromLevel = request.RequireRegistrationBeforeTreatmentFromLevel,
                RequireTriageBeforeStandardRegistration = request.RequireTriageBeforeStandardRegistration,
                RequireRegistrationCompletionBeforeDisposition = request.RequireRegistrationCompletionBeforeDisposition,
                TemporaryPatientNumberPrefix = NormalizeText(request.TemporaryPatientNumberPrefix) ?? string.Empty,
                EmergencyVisitNumberPrefix = NormalizeText(request.EmergencyVisitNumberPrefix) ?? string.Empty,
                IsDefault = request.IsDefault,
                IsActive = request.IsActive,
                Notes = NormalizeText(request.Notes),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstEmergencySetting>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data pengaturan IGD gagal disimpan karena melanggar relasi atau data unik."));
            }

            await transaction.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencySetting.Create",
                "Membuat data Emergency Setting.",
                new { EntityId = entity.Id, Controller = "EmergencySetting", Action = "Create" }
            );

            return Ok(ApiResponse<EmergencySettingResponse>.Ok(ToResponse(entity), "Data pengaturan IGD berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencySettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Setting", Description = "Mengubah pengaturan IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencySetting", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmergencySettingRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstEmergencySetting>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data pengaturan IGD tidak ditemukan."));

            var validationMessage = await _emergencySettingService.ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedCode = NormalizeText(request.Code) ?? string.Empty;
            if (await _dbContext.Set<MstEmergencySetting>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.Code.ToLower() == normalizedCode.ToLower() && x.Id != id, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Kode sudah digunakan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            if (request.IsDefault)
            {
                await _emergencySettingService.ClearOtherDefaultsAsync(
                    exceptId: id,
                    actorUserId: actorUserId,
                    now: now,
                    cancellationToken: cancellationToken);
            }

            entity.Code = NormalizeText(request.Code) ?? string.Empty;
            entity.Name = NormalizeText(request.Name) ?? string.Empty;
            entity.DefaultEmergencyServiceUnitId = request.DefaultEmergencyServiceUnitId;
            entity.TriageSystem = request.TriageSystem;
            entity.AllowProvisionalRegistration = request.AllowProvisionalRegistration;
            entity.AllowUnknownPatient = request.AllowUnknownPatient;
            entity.AutoCreateProvisionalEncounter = request.AutoCreateProvisionalEncounter;
            entity.ImmediateCareLevelThreshold = request.ImmediateCareLevelThreshold;
            entity.RequireRegistrationBeforeTreatmentFromLevel = request.RequireRegistrationBeforeTreatmentFromLevel;
            entity.RequireTriageBeforeStandardRegistration = request.RequireTriageBeforeStandardRegistration;
            entity.RequireRegistrationCompletionBeforeDisposition = request.RequireRegistrationCompletionBeforeDisposition;
            entity.TemporaryPatientNumberPrefix = NormalizeText(request.TemporaryPatientNumberPrefix) ?? string.Empty;
            entity.EmergencyVisitNumberPrefix = NormalizeText(request.EmergencyVisitNumberPrefix) ?? string.Empty;
            entity.IsDefault = request.IsDefault;
            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeText(request.Notes);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data pengaturan IGD gagal diubah karena melanggar relasi atau data unik."));
            }

            await transaction.CommitAsync(cancellationToken);
            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencySetting.Update",
                "Mengubah data Emergency Setting.",
                new { EntityId = id, Controller = "EmergencySetting", Action = "Update" }
            );

            return Ok(ApiResponse<EmergencySettingResponse>.Ok(ToResponse(entity), "Data pengaturan IGD berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Emergency Setting", Description = "Menghapus pengaturan IGD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmergencySetting", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstEmergencySetting>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data pengaturan IGD tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencySetting.Delete",
                "Menghapus data Emergency Setting.",
                new { EntityId = id, Controller = "EmergencySetting", Action = "Delete" }
            );

            return Ok(ApiResponse<object>.Ok(null, "Data pengaturan IGD berhasil dihapus."));
        }

        private async Task<string?> ValidateRequestAsync(CreateEmergencySettingRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                return "Code wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.Name))
                return "Name wajib diisi.";

            if (request.DefaultEmergencyServiceUnitId == Guid.Empty)
                return "DefaultEmergencyServiceUnitId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyTriageSystem), request.TriageSystem))
                return "Nilai TriageSystem tidak valid.";

            if (string.IsNullOrWhiteSpace(request.TemporaryPatientNumberPrefix))
                return "TemporaryPatientNumberPrefix wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.EmergencyVisitNumberPrefix))
                return "EmergencyVisitNumberPrefix wajib diisi.";

            if (request.ImmediateCareLevelThreshold < 1 || request.ImmediateCareLevelThreshold > 5)
                return "ImmediateCareLevelThreshold harus berada pada level 1 sampai 5.";

            if (request.RequireRegistrationBeforeTreatmentFromLevel < 1 || request.RequireRegistrationBeforeTreatmentFromLevel > 5)
                return "RequireRegistrationBeforeTreatmentFromLevel harus berada pada level 1 sampai 5.";

            if (!await _dbContext.Set<MstServiceUnit>().AsNoTracking().AnyAsync(x => x.Id == request.DefaultEmergencyServiceUnitId && !x.IsDelete, cancellationToken))
                return "DefaultEmergencyServiceUnitId tidak ditemukan.";

            return null;
        }

        private Task<string?> ValidateRequestAsync(UpdateEmergencySettingRequest request, CancellationToken cancellationToken)
            => ValidateRequestAsync((CreateEmergencySettingRequest)request, cancellationToken);

        private static EmergencySettingResponse ToResponse(MstEmergencySetting x)
        {
            return new EmergencySettingResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                DefaultEmergencyServiceUnitId = x.DefaultEmergencyServiceUnitId,
                TriageSystem = x.TriageSystem,
                AllowProvisionalRegistration = x.AllowProvisionalRegistration,
                AllowUnknownPatient = x.AllowUnknownPatient,
                AutoCreateProvisionalEncounter = x.AutoCreateProvisionalEncounter,
                ImmediateCareLevelThreshold = x.ImmediateCareLevelThreshold,
                RequireRegistrationBeforeTreatmentFromLevel = x.RequireRegistrationBeforeTreatmentFromLevel,
                RequireTriageBeforeStandardRegistration = x.RequireTriageBeforeStandardRegistration,
                RequireRegistrationCompletionBeforeDisposition = x.RequireRegistrationCompletionBeforeDisposition,
                TemporaryPatientNumberPrefix = x.TemporaryPatientNumberPrefix,
                EmergencyVisitNumberPrefix = x.EmergencyVisitNumberPrefix,
                IsDefault = x.IsDefault,
                IsActive = x.IsActive,
                Notes = x.Notes,
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
