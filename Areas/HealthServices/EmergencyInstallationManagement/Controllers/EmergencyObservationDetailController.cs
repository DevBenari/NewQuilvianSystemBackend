using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
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
    [Route("api/v1/health-services/emergency-installation-management/emergency-observation-details")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Observation Detail",
        AreaName = "HealthServices",
        ControllerName = "EmergencyObservationDetail",
        Description = "Mengelola catatan berkala selama observasi pasien IGD",
        SortOrder = 6
    )]
    [Tags("Health Services / Emergency Installation Management / Emergency Observation Detail")]
    public class EmergencyObservationDetailController : ControllerBase
    {
        private const string LogCategory = "HealthServices.EmergencyInstallation";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public EmergencyObservationDetailController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencyObservationDetailResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Observation Detail", Description = "Melihat data detail observasi IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyObservationDetail", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? emergencyObservationId,
            [FromQuery] Guid? patientVitalSignId,
            [FromQuery] Guid? progressNoteId,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default
        )
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);
            IQueryable<EmgObservationDetail> query = _dbContext.Set<EmgObservationDetail>().AsNoTracking().Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.ClinicalConditionSummary != null && x.ClinicalConditionSummary.ToLower().Contains(keyword)) ||
                    (x.InterventionSummary != null && x.InterventionSummary.ToLower().Contains(keyword)) ||
                    (x.PatientResponseSummary != null && x.PatientResponseSummary.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }

            if (emergencyObservationId.HasValue && emergencyObservationId.Value != Guid.Empty)
                query = query.Where(x => x.EmergencyObservationId == emergencyObservationId.Value);

            if (patientVitalSignId.HasValue && patientVitalSignId.Value != Guid.Empty)
                query = query.Where(x => x.PatientVitalSignId == patientVitalSignId.Value);

            if (progressNoteId.HasValue && progressNoteId.Value != Guid.Empty)
                query = query.Where(x => x.ProgressNoteId == progressNoteId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.RecordedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.RecordedAt < endDate.Value.Date.AddDays(1));

            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "recordedat" => descending ? query.OrderByDescending(x => x.RecordedAt) : query.OrderBy(x => x.RecordedAt),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.RecordedAt) : query.OrderBy(x => x.RecordedAt)
            };

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<EmergencyObservationDetailResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<EmergencyObservationDetailResponse>>.Ok(result, "Data detail observasi IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyObservationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Observation Detail", Description = "Melihat detail detail observasi IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyObservationDetail", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<EmgObservationDetail>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data detail observasi IGD tidak ditemukan."));

            return Ok(ApiResponse<EmergencyObservationDetailResponse>.Ok(ToResponse(entity), "Detail detail observasi IGD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyObservationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Observation Detail", Description = "Membuat detail observasi IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyObservationDetail", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyObservationDetailRequest request, CancellationToken cancellationToken = default)
        {
            var validationMessage = await ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new EmgObservationDetail
            {
                Id = Guid.NewGuid(),
                EmergencyObservationId = request.EmergencyObservationId,
                PatientVitalSignId = request.PatientVitalSignId,
                ProgressNoteId = request.ProgressNoteId,
                RecordedAt = request.RecordedAt == default ? now : request.RecordedAt,
                RecordedByUserId = request.RecordedByUserId == Guid.Empty ? actorUserId : request.RecordedByUserId,
                ClinicalConditionSummary = NormalizeText(request.ClinicalConditionSummary),
                InterventionSummary = NormalizeText(request.InterventionSummary),
                PatientResponseSummary = NormalizeText(request.PatientResponseSummary),
                FluidIntakeMl = request.FluidIntakeMl,
                UrineOutputMl = request.UrineOutputMl,
                OtherOutputMl = request.OtherOutputMl,
                BleedingEstimatedMl = request.BleedingEstimatedMl,
                VomitEstimatedMl = request.VomitEstimatedMl,
                Notes = NormalizeText(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<EmgObservationDetail>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data detail observasi IGD gagal disimpan karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyObservationDetail.Create",
                "Membuat data Emergency Observation Detail.",
                new { EntityId = entity.Id, Controller = "EmergencyObservationDetail", Action = "Create" }
            );

            return Ok(ApiResponse<EmergencyObservationDetailResponse>.Ok(ToResponse(entity), "Data detail observasi IGD berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyObservationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Observation Detail", Description = "Mengubah detail observasi IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyObservationDetail", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmergencyObservationDetailRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<EmgObservationDetail>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data detail observasi IGD tidak ditemukan."));

            var validationMessage = await ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.EmergencyObservationId = request.EmergencyObservationId;
            entity.PatientVitalSignId = request.PatientVitalSignId;
            entity.ProgressNoteId = request.ProgressNoteId;
            entity.RecordedAt = request.RecordedAt;
            entity.RecordedByUserId = request.RecordedByUserId;
            entity.ClinicalConditionSummary = NormalizeText(request.ClinicalConditionSummary);
            entity.InterventionSummary = NormalizeText(request.InterventionSummary);
            entity.PatientResponseSummary = NormalizeText(request.PatientResponseSummary);
            entity.FluidIntakeMl = request.FluidIntakeMl;
            entity.UrineOutputMl = request.UrineOutputMl;
            entity.OtherOutputMl = request.OtherOutputMl;
            entity.BleedingEstimatedMl = request.BleedingEstimatedMl;
            entity.VomitEstimatedMl = request.VomitEstimatedMl;
            entity.Notes = NormalizeText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data detail observasi IGD gagal diubah karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyObservationDetail.Update",
                "Mengubah data Emergency Observation Detail.",
                new { EntityId = id, Controller = "EmergencyObservationDetail", Action = "Update" }
            );

            return Ok(ApiResponse<EmergencyObservationDetailResponse>.Ok(ToResponse(entity), "Data detail observasi IGD berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Emergency Observation Detail", Description = "Menghapus detail observasi IGD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmergencyObservationDetail", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<EmgObservationDetail>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data detail observasi IGD tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyObservationDetail.Delete",
                "Menghapus data Emergency Observation Detail.",
                new { EntityId = id, Controller = "EmergencyObservationDetail", Action = "Delete" }
            );

            return Ok(ApiResponse<object>.Ok(null, "Data detail observasi IGD berhasil dihapus."));
        }

        private async Task<string?> ValidateRequestAsync(CreateEmergencyObservationDetailRequest request, CancellationToken cancellationToken)
        {
            if (request.EmergencyObservationId == Guid.Empty)
                return "EmergencyObservationId wajib diisi.";

            if (!await _dbContext.Set<EmgObservation>().AsNoTracking().AnyAsync(x => x.Id == request.EmergencyObservationId && !x.IsDelete, cancellationToken))
                return "EmergencyObservationId tidak ditemukan.";

            if (request.PatientVitalSignId.HasValue && request.PatientVitalSignId.Value != Guid.Empty &&
                !await _dbContext.Set<TrxPatientVitalSign>().AsNoTracking().AnyAsync(x => x.Id == request.PatientVitalSignId.Value && !x.IsDelete, cancellationToken))
                return "PatientVitalSignId tidak ditemukan.";

            if (request.ProgressNoteId.HasValue && request.ProgressNoteId.Value != Guid.Empty &&
                !await _dbContext.Set<TrxPatientIntegratedProgressNote>().AsNoTracking().AnyAsync(x => x.Id == request.ProgressNoteId.Value && !x.IsDelete, cancellationToken))
                return "ProgressNoteId tidak ditemukan.";

            return null;
        }

        private Task<string?> ValidateRequestAsync(UpdateEmergencyObservationDetailRequest request, CancellationToken cancellationToken)
            => ValidateRequestAsync((CreateEmergencyObservationDetailRequest)request, cancellationToken);

        private static EmergencyObservationDetailResponse ToResponse(EmgObservationDetail x)
        {
            return new EmergencyObservationDetailResponse
            {
                Id = x.Id,
                EmergencyObservationId = x.EmergencyObservationId,
                PatientVitalSignId = x.PatientVitalSignId,
                ProgressNoteId = x.ProgressNoteId,
                RecordedAt = x.RecordedAt,
                RecordedByUserId = x.RecordedByUserId,
                ClinicalConditionSummary = x.ClinicalConditionSummary,
                InterventionSummary = x.InterventionSummary,
                PatientResponseSummary = x.PatientResponseSummary,
                FluidIntakeMl = x.FluidIntakeMl,
                UrineOutputMl = x.UrineOutputMl,
                OtherOutputMl = x.OtherOutputMl,
                BleedingEstimatedMl = x.BleedingEstimatedMl,
                VomitEstimatedMl = x.VomitEstimatedMl,
                Notes = x.Notes,
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
