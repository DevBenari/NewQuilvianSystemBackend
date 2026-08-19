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
    [Route("api/v1/health-services/emergency-installation-management/emergency-resuscitations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Resuscitation",
        AreaName = "HealthServices",
        ControllerName = "EmergencyResuscitation",
        Description = "Mengelola episode resusitasi pasien IGD",
        SortOrder = 4
    )]
    [Tags("Health Services / Emergency Installation Management / Emergency Resuscitation")]
    public class EmergencyResuscitationController : ControllerBase
    {
        private const string LogCategory = "HealthServices.EmergencyInstallation";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly EmergencyResuscitationService _emergencyResuscitationService;

        public EmergencyResuscitationController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            EmergencyResuscitationService emergencyService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _emergencyResuscitationService = emergencyService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencyResuscitationResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Resuscitation", Description = "Melihat data resusitasi IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyResuscitation", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? emergencyVisitId,
            [FromQuery] EmergencyResuscitationStatus? resuscitationStatus,
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
            IQueryable<TrxEmergencyResuscitation> query = _dbContext.Set<TrxEmergencyResuscitation>().AsNoTracking().Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ResuscitationNumber.ToLower().Contains(keyword) ||
                    (x.Location != null && x.Location.ToLower().Contains(keyword)) ||
                    (x.TriggerCondition != null && x.TriggerCondition.ToLower().Contains(keyword)) ||
                    (x.AirwayManagementSummary != null && x.AirwayManagementSummary.ToLower().Contains(keyword)) ||
                    (x.BreathingManagementSummary != null && x.BreathingManagementSummary.ToLower().Contains(keyword)) ||
                    (x.CirculationManagementSummary != null && x.CirculationManagementSummary.ToLower().Contains(keyword)) ||
                    (x.NeurologicalManagementSummary != null && x.NeurologicalManagementSummary.ToLower().Contains(keyword)) ||
                    (x.OutcomeSummary != null && x.OutcomeSummary.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }

            if (emergencyVisitId.HasValue && emergencyVisitId.Value != Guid.Empty)
                query = query.Where(x => x.EmergencyVisitId == emergencyVisitId.Value);

            if (resuscitationStatus.HasValue)
                query = query.Where(x => x.ResuscitationStatus == resuscitationStatus.Value);

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

            var result = new PagedResult<EmergencyResuscitationResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<EmergencyResuscitationResponse>>.Ok(result, "Data resusitasi IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyResuscitationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Resuscitation", Description = "Melihat detail resusitasi IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyResuscitation", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyResuscitation>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data resusitasi IGD tidak ditemukan."));

            return Ok(ApiResponse<EmergencyResuscitationResponse>.Ok(ToResponse(entity), "Detail resusitasi IGD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyResuscitationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Resuscitation", Description = "Membuat resusitasi IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyResuscitation", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyResuscitationRequest request, CancellationToken cancellationToken = default)
        {
            var validationMessage = await _emergencyResuscitationService.ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedNumber = NormalizeText(request.ResuscitationNumber);
            if (!string.IsNullOrWhiteSpace(normalizedNumber) && await _dbContext.Set<TrxEmergencyResuscitation>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.ResuscitationNumber == normalizedNumber, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "ResuscitationNumber sudah digunakan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new TrxEmergencyResuscitation
            {
                Id = Guid.NewGuid(),
                EmergencyVisitId = request.EmergencyVisitId,
                ResuscitationNumber = string.IsNullOrWhiteSpace(request.ResuscitationNumber) ? _emergencyResuscitationService.GenerateNumber(now) : request.ResuscitationNumber.Trim(),
                ResuscitationStatus = request.ResuscitationStatus,
                StartedAt = request.StartedAt == default ? now : request.StartedAt,
                CompletedAt = request.CompletedAt,
                Location = NormalizeText(request.Location),
                TriggerCondition = NormalizeText(request.TriggerCondition),
                TeamLeaderDoctorId = request.TeamLeaderDoctorId,
                RecordedByUserId = request.RecordedByUserId ?? actorUserId,
                WasCardiopulmonaryResuscitationPerformed = request.WasCardiopulmonaryResuscitationPerformed,
                CardiopulmonaryResuscitationStartedAt = request.CardiopulmonaryResuscitationStartedAt,
                ReturnOfSpontaneousCirculationAt = request.ReturnOfSpontaneousCirculationAt,
                DefibrillationCount = request.DefibrillationCount,
                AirwayManagementSummary = NormalizeText(request.AirwayManagementSummary),
                BreathingManagementSummary = NormalizeText(request.BreathingManagementSummary),
                CirculationManagementSummary = NormalizeText(request.CirculationManagementSummary),
                NeurologicalManagementSummary = NormalizeText(request.NeurologicalManagementSummary),
                OutcomeSummary = NormalizeText(request.OutcomeSummary),
                Notes = NormalizeText(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxEmergencyResuscitation>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data resusitasi IGD gagal disimpan karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyResuscitation.Create",
                "Membuat data Emergency Resuscitation.",
                new { EntityId = entity.Id, Controller = "EmergencyResuscitation", Action = "Create" }
            );

            return Ok(ApiResponse<EmergencyResuscitationResponse>.Ok(ToResponse(entity), "Data resusitasi IGD berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyResuscitationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Resuscitation", Description = "Mengubah resusitasi IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyResuscitation", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmergencyResuscitationRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyResuscitation>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data resusitasi IGD tidak ditemukan."));

            var validationMessage = await _emergencyResuscitationService.ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedNumber = NormalizeText(request.ResuscitationNumber);
            if (!string.IsNullOrWhiteSpace(normalizedNumber) && await _dbContext.Set<TrxEmergencyResuscitation>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.ResuscitationNumber == normalizedNumber && x.Id != id, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "ResuscitationNumber sudah digunakan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.EmergencyVisitId = request.EmergencyVisitId;
            entity.ResuscitationNumber = string.IsNullOrWhiteSpace(request.ResuscitationNumber) ? entity.ResuscitationNumber : request.ResuscitationNumber.Trim();
            entity.StartedAt = request.StartedAt;
            entity.Location = NormalizeText(request.Location);
            entity.TriggerCondition = NormalizeText(request.TriggerCondition);
            entity.TeamLeaderDoctorId = request.TeamLeaderDoctorId;
            entity.RecordedByUserId = request.RecordedByUserId;
            entity.WasCardiopulmonaryResuscitationPerformed = request.WasCardiopulmonaryResuscitationPerformed;
            entity.CardiopulmonaryResuscitationStartedAt = request.CardiopulmonaryResuscitationStartedAt;
            entity.ReturnOfSpontaneousCirculationAt = request.ReturnOfSpontaneousCirculationAt;
            entity.DefibrillationCount = request.DefibrillationCount;
            entity.AirwayManagementSummary = NormalizeText(request.AirwayManagementSummary);
            entity.BreathingManagementSummary = NormalizeText(request.BreathingManagementSummary);
            entity.CirculationManagementSummary = NormalizeText(request.CirculationManagementSummary);
            entity.NeurologicalManagementSummary = NormalizeText(request.NeurologicalManagementSummary);
            entity.OutcomeSummary = NormalizeText(request.OutcomeSummary);
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
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data resusitasi IGD gagal diubah karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyResuscitation.Update",
                "Mengubah data Emergency Resuscitation.",
                new { EntityId = id, Controller = "EmergencyResuscitation", Action = "Update" }
            );

            return Ok(ApiResponse<EmergencyResuscitationResponse>.Ok(ToResponse(entity), "Data resusitasi IGD berhasil diubah."));
        }

        [HttpPatch("{id:guid}/resuscitation-status")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyResuscitationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Emergency Resuscitation ResuscitationStatus", Description = "Mengubah status resusitasi IGD", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyResuscitation", "Update")]
        public async Task<IActionResult> UpdateResuscitationStatus(Guid id, [FromBody] UpdateEmergencyResuscitationResuscitationStatusRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyResuscitation>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data resusitasi IGD tidak ditemukan."));

            if (!_emergencyResuscitationService.CanTransition(entity.ResuscitationStatus, request.ResuscitationStatus))
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, $"Perubahan status dari {entity.ResuscitationStatus} ke {request.ResuscitationStatus} tidak diperbolehkan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.ResuscitationStatus = request.ResuscitationStatus;
            if (request.ResuscitationStatus != EmergencyResuscitationStatus.InProgress && request.ResuscitationStatus != EmergencyResuscitationStatus.Planned)
                entity.CompletedAt ??= now;
            if (request.ResuscitationStatus == EmergencyResuscitationStatus.InProgress)
            {
                var visit = await _dbContext.Set<TrxEmergencyVisit>().FirstAsync(x => x.Id == entity.EmergencyVisitId && !x.IsDelete, cancellationToken);
                visit.VisitStatus = EmergencyVisitStatus.InTreatment;
                visit.TreatmentStartedAt ??= now;
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
                "EmergencyResuscitation.UpdateResuscitationStatus",
                "Memperbarui proses Emergency Resuscitation melalui aksi UpdateResuscitationStatus.",
                new { EntityId = id, Controller = "EmergencyResuscitation", Action = "UpdateResuscitationStatus" }
            );

            return Ok(ApiResponse<EmergencyResuscitationResponse>.Ok(ToResponse(entity), "Status resusitasi IGD berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Emergency Resuscitation", Description = "Menghapus resusitasi IGD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmergencyResuscitation", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyResuscitation>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data resusitasi IGD tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyResuscitation.Delete",
                "Menghapus data Emergency Resuscitation.",
                new { EntityId = id, Controller = "EmergencyResuscitation", Action = "Delete" }
            );

            return Ok(ApiResponse<object>.Ok(null, "Data resusitasi IGD berhasil dihapus."));
        }

        private async Task<string?> ValidateRequestAsync(CreateEmergencyResuscitationRequest request, CancellationToken cancellationToken)
        {
            if (request.EmergencyVisitId == Guid.Empty)
                return "EmergencyVisitId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyResuscitationStatus), request.ResuscitationStatus))
                return "Nilai ResuscitationStatus tidak valid.";

            if (!await _dbContext.Set<TrxEmergencyVisit>().AsNoTracking().AnyAsync(x => x.Id == request.EmergencyVisitId && !x.IsDelete, cancellationToken))
                return "EmergencyVisitId tidak ditemukan.";

            return null;
        }

        private Task<string?> ValidateRequestAsync(UpdateEmergencyResuscitationRequest request, CancellationToken cancellationToken)
            => ValidateRequestAsync((CreateEmergencyResuscitationRequest)request, cancellationToken);

        private static bool CanTransition(EmergencyResuscitationStatus current, EmergencyResuscitationStatus target)
        {
            if (current == target) return true;

            return current switch
            {
                EmergencyResuscitationStatus.Planned => target is EmergencyResuscitationStatus.InProgress or EmergencyResuscitationStatus.Cancelled,
                EmergencyResuscitationStatus.InProgress => target is EmergencyResuscitationStatus.Completed or EmergencyResuscitationStatus.Stopped or EmergencyResuscitationStatus.Cancelled,
                EmergencyResuscitationStatus.Completed => false,
                EmergencyResuscitationStatus.Stopped => false,
                EmergencyResuscitationStatus.Cancelled => false,
                _ => false
            };
        }

        private static EmergencyResuscitationResponse ToResponse(TrxEmergencyResuscitation x)
        {
            return new EmergencyResuscitationResponse
            {
                Id = x.Id,
                EmergencyVisitId = x.EmergencyVisitId,
                ResuscitationNumber = x.ResuscitationNumber,
                ResuscitationStatus = x.ResuscitationStatus,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                Location = x.Location,
                TriggerCondition = x.TriggerCondition,
                TeamLeaderDoctorId = x.TeamLeaderDoctorId,
                RecordedByUserId = x.RecordedByUserId,
                WasCardiopulmonaryResuscitationPerformed = x.WasCardiopulmonaryResuscitationPerformed,
                CardiopulmonaryResuscitationStartedAt = x.CardiopulmonaryResuscitationStartedAt,
                ReturnOfSpontaneousCirculationAt = x.ReturnOfSpontaneousCirculationAt,
                DefibrillationCount = x.DefibrillationCount,
                AirwayManagementSummary = x.AirwayManagementSummary,
                BreathingManagementSummary = x.BreathingManagementSummary,
                CirculationManagementSummary = x.CirculationManagementSummary,
                NeurologicalManagementSummary = x.NeurologicalManagementSummary,
                OutcomeSummary = x.OutcomeSummary,
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
