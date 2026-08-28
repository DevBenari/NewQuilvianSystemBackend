using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;
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
    [Route("api/v1/health-services/emergency-installation-management/emergency-observations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Observation",
        AreaName = "HealthServices",
        ControllerName = "EmergencyObservation",
        Description = "Mengelola periode observasi pasien IGD",
        SortOrder = 5
    )]
    [Tags("Health Services / Emergency Installation Management / Emergency Observation")]
    public class EmergencyObservationController : ControllerBase
    {
        private const string LogCategory = "HealthServices.EmergencyInstallation";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly EmergencyObservationService _emergencyObservationService;
        private readonly EmergencyVisitService _emergencyVisitService;

        public EmergencyObservationController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            EmergencyObservationService emergencyService,
            EmergencyVisitService emergencyVisitService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _emergencyObservationService = emergencyService;
            _emergencyVisitService = emergencyVisitService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencyObservationResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Observation", Description = "Melihat data observasi IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyObservation", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? emergencyVisitId,
            [FromQuery] EmergencyObservationStatus? observationStatus,
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
            IQueryable<EmgObservation> query = _dbContext.Set<EmgObservation>().AsNoTracking().Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ObservationNumber.ToLower().Contains(keyword) ||
                    (x.ObservationLocation != null && x.ObservationLocation.ToLower().Contains(keyword)) ||
                    (x.Indication != null && x.Indication.ToLower().Contains(keyword)) ||
                    (x.ObservationPlan != null && x.ObservationPlan.ToLower().Contains(keyword)) ||
                    (x.CompletionSummary != null && x.CompletionSummary.ToLower().Contains(keyword)) ||
                    (x.EscalationReason != null && x.EscalationReason.ToLower().Contains(keyword)));
            }

            if (emergencyVisitId.HasValue && emergencyVisitId.Value != Guid.Empty)
                query = query.Where(x => x.EmergencyVisitId == emergencyVisitId.Value);

            if (observationStatus.HasValue)
                query = query.Where(x => x.ObservationStatus == observationStatus.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.StartedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.StartedAt < endDate.Value.Date.AddDays(1));

            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "startedat" => descending ? query.OrderByDescending(x => x.StartedAt) : query.OrderBy(x => x.StartedAt),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.StartedAt) : query.OrderBy(x => x.StartedAt)
            };

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<EmergencyObservationResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<EmergencyObservationResponse>>.Ok(result, "Data observasi IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyObservationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Observation", Description = "Melihat detail observasi IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyObservation", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<EmgObservation>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data observasi IGD tidak ditemukan."));

            return Ok(ApiResponse<EmergencyObservationResponse>.Ok(ToResponse(entity), "Detail observasi IGD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyObservationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Observation", Description = "Membuat observasi IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyObservation", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyObservationRequest request, CancellationToken cancellationToken = default)
        {
            var validationMessage = await _emergencyObservationService.ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedNumber = NormalizeText(request.ObservationNumber);
            if (!string.IsNullOrWhiteSpace(normalizedNumber) && await _dbContext.Set<EmgObservation>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.ObservationNumber == normalizedNumber, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "ObservationNumber sudah digunakan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new EmgObservation
            {
                Id = Guid.NewGuid(),
                EmergencyVisitId = request.EmergencyVisitId,
                ObservationNumber = string.IsNullOrWhiteSpace(request.ObservationNumber) ? _emergencyObservationService.GenerateNumber(now) : request.ObservationNumber.Trim(),
                ObservationStatus = request.ObservationStatus,
                StartedAt = request.StartedAt == default ? now : request.StartedAt,
                EndedAt = request.EndedAt,
                ObservationLocation = NormalizeText(request.ObservationLocation),
                Indication = NormalizeText(request.Indication),
                ObservationPlan = NormalizeText(request.ObservationPlan),
                ResponsibleDoctorId = request.ResponsibleDoctorId,
                ResponsibleNurseUserId = request.ResponsibleNurseUserId,
                CompletionSummary = NormalizeText(request.CompletionSummary),
                EscalationReason = NormalizeText(request.EscalationReason),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<EmgObservation>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data observasi IGD gagal disimpan karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyObservation.Create",
                "Membuat data Emergency Observation.",
                new { EntityId = entity.Id, Controller = "EmergencyObservation", Action = "Create" }
            );

            return Ok(ApiResponse<EmergencyObservationResponse>.Ok(ToResponse(entity), "Data observasi IGD berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyObservationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Observation", Description = "Mengubah observasi IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyObservation", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmergencyObservationRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<EmgObservation>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data observasi IGD tidak ditemukan."));

            var validationMessage = await _emergencyObservationService.ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedNumber = NormalizeText(request.ObservationNumber);
            if (!string.IsNullOrWhiteSpace(normalizedNumber) && await _dbContext.Set<EmgObservation>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.ObservationNumber == normalizedNumber && x.Id != id, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "ObservationNumber sudah digunakan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.EmergencyVisitId = request.EmergencyVisitId;
            entity.ObservationNumber = string.IsNullOrWhiteSpace(request.ObservationNumber) ? entity.ObservationNumber : request.ObservationNumber.Trim();
            entity.StartedAt = request.StartedAt;
            entity.ObservationLocation = NormalizeText(request.ObservationLocation);
            entity.Indication = NormalizeText(request.Indication);
            entity.ObservationPlan = NormalizeText(request.ObservationPlan);
            entity.ResponsibleDoctorId = request.ResponsibleDoctorId;
            entity.ResponsibleNurseUserId = request.ResponsibleNurseUserId;
            entity.CompletionSummary = NormalizeText(request.CompletionSummary);
            entity.EscalationReason = NormalizeText(request.EscalationReason);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data observasi IGD gagal diubah karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyObservation.Update",
                "Mengubah data Emergency Observation.",
                new { EntityId = id, Controller = "EmergencyObservation", Action = "Update" }
            );

            return Ok(ApiResponse<EmergencyObservationResponse>.Ok(ToResponse(entity), "Data observasi IGD berhasil diubah."));
        }

        [HttpPatch("{id:guid}/observation-status")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyObservationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Observation ObservationStatus", Description = "Mengubah status observasi IGD", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyObservation", "Update")]
        public async Task<IActionResult> UpdateObservationStatus(Guid id, [FromBody] UpdateEmergencyObservationObservationStatusRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<EmgObservation>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data observasi IGD tidak ditemukan."));

            if (!_emergencyObservationService.CanTransition(entity.ObservationStatus, request.ObservationStatus))
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, $"Perubahan status dari {entity.ObservationStatus} ke {request.ObservationStatus} tidak diperbolehkan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var visit = await _dbContext.Set<EmgVisit>().FirstAsync(x => x.Id == entity.EmergencyVisitId && !x.IsDelete, cancellationToken);

            // BE-IGD-021 — tiga dari lima titik tulis VisitStatus yang tersisa ada di
            // percabangan ini. Targetnya ditentukan lebih dulu, lalu penjaga BE-IGD-018
            // yang memutuskan boleh atau tidak. Diperiksa SEBELUM entity diubah, supaya
            // penolakan 409 tidak meninggalkan observasi yang terlanjur berpindah status.
            // ObservationStatus.Cancelled sengaja tidak memetakan ke status kunjungan mana
            // pun: membatalkan observasi tidak dengan sendirinya memindahkan pasien.
            var targetVisitStatus = request.ObservationStatus switch
            {
                EmergencyObservationStatus.Active => EmergencyVisitStatus.UnderObservation,
                EmergencyObservationStatus.Completed => EmergencyVisitStatus.AwaitingDisposition,
                EmergencyObservationStatus.Escalated => EmergencyVisitStatus.InTreatment,
                _ => (EmergencyVisitStatus?)null
            };

            if (targetVisitStatus.HasValue &&
                !_emergencyVisitService.TryApplyVisitStatus(
                    visit, targetVisitStatus.Value, actorUserId, now, out var penolakanStatusKunjungan))
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, penolakanStatusKunjungan!));
            }

            entity.ObservationStatus = request.ObservationStatus;
            if (request.ObservationStatus != EmergencyObservationStatus.Active)
                entity.EndedAt ??= now;
            if (request.ObservationStatus == EmergencyObservationStatus.Escalated)
                entity.EscalationReason = NormalizeText(request.Notes) ?? entity.EscalationReason;
            if (!string.IsNullOrWhiteSpace(request.Notes) && entity.GetType().GetProperty("Notes") != null)
            {
                entity.GetType().GetProperty("Notes")?.SetValue(entity, NormalizeText(request.Notes));
            }
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyObservation.UpdateObservationStatus",
                "Memperbarui proses Emergency Observation melalui aksi UpdateObservationStatus.",
                new { EntityId = id, Controller = "EmergencyObservation", Action = "UpdateObservationStatus" }
            );

            return Ok(ApiResponse<EmergencyObservationResponse>.Ok(ToResponse(entity), "Status observasi IGD berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Emergency Observation", Description = "Menghapus observasi IGD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmergencyObservation", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<EmgObservation>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data observasi IGD tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyObservation.Delete",
                "Menghapus data Emergency Observation.",
                new { EntityId = id, Controller = "EmergencyObservation", Action = "Delete" }
            );

            return Ok(ApiResponse<object>.Ok(null, "Data observasi IGD berhasil dihapus."));
        }

        private async Task<string?> ValidateRequestAsync(CreateEmergencyObservationRequest request, CancellationToken cancellationToken)
        {
            if (request.EmergencyVisitId == Guid.Empty)
                return "EmergencyVisitId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyObservationStatus), request.ObservationStatus))
                return "Nilai ObservationStatus tidak valid.";

            if (!await _dbContext.Set<EmgVisit>().AsNoTracking().AnyAsync(x => x.Id == request.EmergencyVisitId && !x.IsDelete, cancellationToken))
                return "EmergencyVisitId tidak ditemukan.";

            return null;
        }

        private Task<string?> ValidateRequestAsync(UpdateEmergencyObservationRequest request, CancellationToken cancellationToken)
            => ValidateRequestAsync((CreateEmergencyObservationRequest)request, cancellationToken);

        private static bool CanTransition(EmergencyObservationStatus current, EmergencyObservationStatus target)
        {
            if (current == target) return true;

            return current switch
            {
                EmergencyObservationStatus.Active => target is EmergencyObservationStatus.Completed or EmergencyObservationStatus.Escalated or EmergencyObservationStatus.Cancelled,
                EmergencyObservationStatus.Completed => false,
                EmergencyObservationStatus.Escalated => target is EmergencyObservationStatus.Completed or EmergencyObservationStatus.Cancelled,
                EmergencyObservationStatus.Cancelled => false,
                _ => false
            };
        }

        private static EmergencyObservationResponse ToResponse(EmgObservation x)
        {
            return new EmergencyObservationResponse
            {
                Id = x.Id,
                EmergencyVisitId = x.EmergencyVisitId,
                ObservationNumber = x.ObservationNumber,
                ObservationStatus = x.ObservationStatus,
                StartedAt = x.StartedAt,
                EndedAt = x.EndedAt,
                ObservationLocation = x.ObservationLocation,
                Indication = x.Indication,
                ObservationPlan = x.ObservationPlan,
                ResponsibleDoctorId = x.ResponsibleDoctorId,
                ResponsibleNurseUserId = x.ResponsibleNurseUserId,
                CompletionSummary = x.CompletionSummary,
                EscalationReason = x.EscalationReason,
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
