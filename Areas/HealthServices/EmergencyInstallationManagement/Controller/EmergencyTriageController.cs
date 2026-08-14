using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models;
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
    [Route("api/v1/health-services/emergency-installation-management/emergency-triages")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Triage",
        AreaName = "HealthServices",
        ControllerName = "EmergencyTriage",
        Description = "Mengelola proses triage dan retriage pasien IGD",
        SortOrder = 2
    )]
    [Tags("Health Services / Emergency Installation Management / Emergency Triage")]
    public class EmergencyTriageController : ControllerBase
    {
        private const string LogCategory = "HealthServices.EmergencyInstallation";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly EmergencyTriageService _emergencyTriageService;

        public EmergencyTriageController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            EmergencyTriageService emergencyService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _emergencyTriageService = emergencyService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencyTriageResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Triage", Description = "Melihat data triage IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyTriage", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? emergencyVisitId,
            [FromQuery] Guid? triageLevelId,
            [FromQuery] Guid? patientVitalSignId,
            [FromQuery] Guid? previousTriageId,
            [FromQuery] EmergencyTriageSystem? triageSystem,
            [FromQuery] EmergencyTriageStatus? triageStatus,
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
            IQueryable<TrxEmergencyTriage> query = _dbContext.Set<TrxEmergencyTriage>().AsNoTracking().Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.TriageReason != null && x.TriageReason.ToLower().Contains(keyword)) ||
                    (x.AirwaySummary != null && x.AirwaySummary.ToLower().Contains(keyword)) ||
                    (x.BreathingSummary != null && x.BreathingSummary.ToLower().Contains(keyword)) ||
                    (x.CirculationSummary != null && x.CirculationSummary.ToLower().Contains(keyword)) ||
                    (x.DisabilitySummary != null && x.DisabilitySummary.ToLower().Contains(keyword)) ||
                    (x.ExposureSummary != null && x.ExposureSummary.ToLower().Contains(keyword)) ||
                    (x.RedFlagSummary != null && x.RedFlagSummary.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }

            if (emergencyVisitId.HasValue && emergencyVisitId.Value != Guid.Empty)
                query = query.Where(x => x.EmergencyVisitId == emergencyVisitId.Value);

            if (triageLevelId.HasValue && triageLevelId.Value != Guid.Empty)
                query = query.Where(x => x.TriageLevelId == triageLevelId.Value);

            if (patientVitalSignId.HasValue && patientVitalSignId.Value != Guid.Empty)
                query = query.Where(x => x.PatientVitalSignId == patientVitalSignId.Value);

            if (previousTriageId.HasValue && previousTriageId.Value != Guid.Empty)
                query = query.Where(x => x.PreviousTriageId == previousTriageId.Value);

            if (triageSystem.HasValue)
                query = query.Where(x => x.TriageSystem == triageSystem.Value);

            if (triageStatus.HasValue)
                query = query.Where(x => x.TriageStatus == triageStatus.Value);

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
                "sequence" => descending ? query.OrderByDescending(x => x.Sequence) : query.OrderBy(x => x.Sequence),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.StartedAt) : query.OrderBy(x => x.StartedAt)
            };

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<EmergencyTriageResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<EmergencyTriageResponse>>.Ok(result, "Data triage IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTriageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Triage", Description = "Melihat detail triage IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyTriage", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyTriage>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data triage IGD tidak ditemukan."));

            return Ok(ApiResponse<EmergencyTriageResponse>.Ok(ToResponse(entity), "Detail triage IGD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTriageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Triage", Description = "Membuat triage IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyTriage", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyTriageRequest request, CancellationToken cancellationToken = default)
        {
            var validationMessage = await _emergencyTriageService.ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var triageLevel = await _emergencyTriageService.GetTriageLevelAsync(request.TriageLevelId, cancellationToken);

            var nextSequence = request.Sequence > 0
                ? request.Sequence
                : (await _dbContext.Set<TrxEmergencyTriage>()
                    .Where(x => x.EmergencyVisitId == request.EmergencyVisitId && !x.IsDelete)
                    .Select(x => (int?)x.Sequence)
                    .MaxAsync(cancellationToken) ?? 0) + 1;

            var entity = new TrxEmergencyTriage
            {
                Id = Guid.NewGuid(),
                EmergencyVisitId = request.EmergencyVisitId,
                TriageLevelId = request.TriageLevelId,
                PatientVitalSignId = request.PatientVitalSignId,
                Sequence = request.Sequence,
                IsRetriage = request.IsRetriage,
                PreviousTriageId = request.PreviousTriageId,
                TriageSystem = request.TriageSystem,
                TriageStatus = request.TriageStatus,
                StartedAt = request.StartedAt == default ? now : request.StartedAt,
                CompletedAt = request.CompletedAt,
                MaxWaitingMinutesSnapshot = request.MaxWaitingMinutesSnapshot,
                ResponseDueAt = request.ResponseDueAt,
                ImmediateCareAllowed = request.ImmediateCareAllowed,
                TriageReason = NormalizeText(request.TriageReason),
                AirwaySummary = NormalizeText(request.AirwaySummary),
                BreathingSummary = NormalizeText(request.BreathingSummary),
                CirculationSummary = NormalizeText(request.CirculationSummary),
                DisabilitySummary = NormalizeText(request.DisabilitySummary),
                ExposureSummary = NormalizeText(request.ExposureSummary),
                RedFlagSummary = NormalizeText(request.RedFlagSummary),
                PerformedByUserId = request.PerformedByUserId == Guid.Empty ? actorUserId : request.PerformedByUserId,
                ReviewedByUserId = request.ReviewedByUserId ?? actorUserId,
                ReviewedAt = request.ReviewedAt,
                Notes = NormalizeText(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            entity.Sequence = nextSequence;
            entity.TriageSystem = triageLevel.TriageSystem;
            entity.MaxWaitingMinutesSnapshot = triageLevel.MaxWaitingMinutes;
            entity.ImmediateCareAllowed = triageLevel.AllowsTreatmentBeforeRegistration;

            // Target waktu yang belum ditetapkan SOP dibiarkan kosong, bukan dianggap 0 menit.
            // Level dengan target 0 menit tetap menghasilkan batas waktu sama dengan StartedAt.
            entity.ResponseDueAt = triageLevel.MaxWaitingMinutes.HasValue
                ? entity.StartedAt.AddMinutes(triageLevel.MaxWaitingMinutes.Value)
                : null;

            _dbContext.Set<TrxEmergencyTriage>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data triage IGD gagal disimpan karena melanggar relasi atau data unik."));
            }

            if (entity.TriageStatus == EmergencyTriageStatus.Completed)
            {
                var visit = await _dbContext.Set<TrxEmergencyVisit>()
                    .FirstAsync(x => x.Id == entity.EmergencyVisitId && !x.IsDelete, cancellationToken);
                visit.VisitStatus = EmergencyVisitStatus.Triaged;
                visit.IsImmediateCareAllowed = entity.ImmediateCareAllowed;
                visit.UpdateDateTime = now;
                visit.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTriage.Create",
                "Membuat data Emergency Triage.",
                new { EntityId = entity.Id, Controller = "EmergencyTriage", Action = "Create" }
            );

            return Ok(ApiResponse<EmergencyTriageResponse>.Ok(ToResponse(entity), "Data triage IGD berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTriageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Triage", Description = "Mengubah triage IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyTriage", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmergencyTriageRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyTriage>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data triage IGD tidak ditemukan."));

            var validationMessage = await _emergencyTriageService.ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.EmergencyVisitId = request.EmergencyVisitId;
            entity.TriageLevelId = request.TriageLevelId;
            entity.PatientVitalSignId = request.PatientVitalSignId;
            entity.Sequence = request.Sequence;
            entity.IsRetriage = request.IsRetriage;
            entity.PreviousTriageId = request.PreviousTriageId;
            entity.StartedAt = request.StartedAt;
            entity.TriageReason = NormalizeText(request.TriageReason);
            entity.AirwaySummary = NormalizeText(request.AirwaySummary);
            entity.BreathingSummary = NormalizeText(request.BreathingSummary);
            entity.CirculationSummary = NormalizeText(request.CirculationSummary);
            entity.DisabilitySummary = NormalizeText(request.DisabilitySummary);
            entity.ExposureSummary = NormalizeText(request.ExposureSummary);
            entity.RedFlagSummary = NormalizeText(request.RedFlagSummary);
            entity.PerformedByUserId = request.PerformedByUserId;
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
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data triage IGD gagal diubah karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTriage.Update",
                "Mengubah data Emergency Triage.",
                new { EntityId = id, Controller = "EmergencyTriage", Action = "Update" }
            );

            return Ok(ApiResponse<EmergencyTriageResponse>.Ok(ToResponse(entity), "Data triage IGD berhasil diubah."));
        }

        [HttpPatch("{id:guid}/triage-status")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTriageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Emergency Triage TriageStatus", Description = "Mengubah status triage IGD", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyTriage", "Update")]
        public async Task<IActionResult> UpdateTriageStatus(Guid id, [FromBody] UpdateEmergencyTriageTriageStatusRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyTriage>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data triage IGD tidak ditemukan."));

            if (!_emergencyTriageService.CanTransition(entity.TriageStatus, request.TriageStatus))
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, $"Perubahan status dari {entity.TriageStatus} ke {request.TriageStatus} tidak diperbolehkan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.TriageStatus = request.TriageStatus;
            if (request.TriageStatus == EmergencyTriageStatus.Completed)
            {
                entity.CompletedAt ??= now;
                var visit = await _dbContext.Set<TrxEmergencyVisit>().FirstAsync(x => x.Id == entity.EmergencyVisitId && !x.IsDelete, cancellationToken);
                visit.VisitStatus = EmergencyVisitStatus.Triaged;
                visit.IsImmediateCareAllowed = entity.ImmediateCareAllowed;
                visit.UpdateDateTime = now;
                visit.UpdateBy = actorUserId;
            }
            if (!string.IsNullOrWhiteSpace(request.Notes) && entity.GetType().GetProperty("Notes") != null)
            {
                entity.GetType().GetProperty("Notes")?.SetValue(entity, NormalizeText(request.Notes));
            }
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTriage.UpdateTriageStatus",
                "Memperbarui proses Emergency Triage melalui aksi UpdateTriageStatus.",
                new { EntityId = id, Controller = "EmergencyTriage", Action = "UpdateTriageStatus" }
            );

            return Ok(ApiResponse<EmergencyTriageResponse>.Ok(ToResponse(entity), "Status triage IGD berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Emergency Triage", Description = "Menghapus triage IGD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmergencyTriage", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyTriage>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data triage IGD tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTriage.Delete",
                "Menghapus data Emergency Triage.",
                new { EntityId = id, Controller = "EmergencyTriage", Action = "Delete" }
            );

            return Ok(ApiResponse<object>.Ok(null, "Data triage IGD berhasil dihapus."));
        }

        private async Task<string?> ValidateRequestAsync(CreateEmergencyTriageRequest request, CancellationToken cancellationToken)
        {
            if (request.EmergencyVisitId == Guid.Empty)
                return "EmergencyVisitId wajib diisi.";

            if (request.TriageLevelId == Guid.Empty)
                return "TriageLevelId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyTriageSystem), request.TriageSystem))
                return "Nilai TriageSystem tidak valid.";

            if (!Enum.IsDefined(typeof(EmergencyTriageStatus), request.TriageStatus))
                return "Nilai TriageStatus tidak valid.";

            if (!await _dbContext.Set<TrxEmergencyVisit>().AsNoTracking().AnyAsync(x => x.Id == request.EmergencyVisitId && !x.IsDelete, cancellationToken))
                return "EmergencyVisitId tidak ditemukan.";

            if (!await _dbContext.Set<MstEmergencyTriageLevel>().AsNoTracking().AnyAsync(x => x.Id == request.TriageLevelId && !x.IsDelete, cancellationToken))
                return "TriageLevelId tidak ditemukan.";

            if (request.PatientVitalSignId.HasValue && request.PatientVitalSignId.Value != Guid.Empty &&
                !await _dbContext.Set<TrxPatientVitalSign>().AsNoTracking().AnyAsync(x => x.Id == request.PatientVitalSignId.Value && !x.IsDelete, cancellationToken))
                return "PatientVitalSignId tidak ditemukan.";

            if (request.PreviousTriageId.HasValue && request.PreviousTriageId.Value != Guid.Empty &&
                !await _dbContext.Set<TrxEmergencyTriage>().AsNoTracking().AnyAsync(x => x.Id == request.PreviousTriageId.Value && !x.IsDelete, cancellationToken))
                return "PreviousTriageId tidak ditemukan.";

            return null;
        }

        private Task<string?> ValidateRequestAsync(UpdateEmergencyTriageRequest request, CancellationToken cancellationToken)
            => ValidateRequestAsync((CreateEmergencyTriageRequest)request, cancellationToken);

        private static bool CanTransition(EmergencyTriageStatus current, EmergencyTriageStatus target)
        {
            if (current == target) return true;

            return current switch
            {
                EmergencyTriageStatus.Draft => target is EmergencyTriageStatus.InProgress or EmergencyTriageStatus.Completed or EmergencyTriageStatus.Cancelled,
                EmergencyTriageStatus.InProgress => target is EmergencyTriageStatus.Completed or EmergencyTriageStatus.Cancelled,
                EmergencyTriageStatus.Completed => target is EmergencyTriageStatus.Superseded,
                EmergencyTriageStatus.Superseded => false,
                EmergencyTriageStatus.Cancelled => false,
                _ => false
            };
        }

        private static EmergencyTriageResponse ToResponse(TrxEmergencyTriage x)
        {
            return new EmergencyTriageResponse
            {
                Id = x.Id,
                EmergencyVisitId = x.EmergencyVisitId,
                TriageLevelId = x.TriageLevelId,
                PatientVitalSignId = x.PatientVitalSignId,
                Sequence = x.Sequence,
                IsRetriage = x.IsRetriage,
                PreviousTriageId = x.PreviousTriageId,
                TriageSystem = x.TriageSystem,
                TriageStatus = x.TriageStatus,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                MaxWaitingMinutesSnapshot = x.MaxWaitingMinutesSnapshot,
                ResponseDueAt = x.ResponseDueAt,
                ImmediateCareAllowed = x.ImmediateCareAllowed,
                TriageReason = x.TriageReason,
                AirwaySummary = x.AirwaySummary,
                BreathingSummary = x.BreathingSummary,
                CirculationSummary = x.CirculationSummary,
                DisabilitySummary = x.DisabilitySummary,
                ExposureSummary = x.ExposureSummary,
                RedFlagSummary = x.RedFlagSummary,
                PerformedByUserId = x.PerformedByUserId,
                ReviewedByUserId = x.ReviewedByUserId,
                ReviewedAt = x.ReviewedAt,
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
