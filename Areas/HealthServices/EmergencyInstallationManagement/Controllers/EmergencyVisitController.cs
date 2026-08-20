using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
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
    [Route("api/v1/health-services/emergency-installation-management/emergency-visits")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Visit",
        AreaName = "HealthServices",
        ControllerName = "EmergencyVisit",
        Description = "Mengelola kunjungan pasien Instalasi Gawat Darurat",
        SortOrder = 1
    )]
    [Tags("Health Services / Emergency Installation Management / Emergency Visit")]
    public class EmergencyVisitController : ControllerBase
    {
        private const string LogCategory = "HealthServices.EmergencyInstallation";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly EmergencyVisitService _emergencyVisitService;
        private readonly EmergencyDispositionService _emergencyDispositionService;

        public EmergencyVisitController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            EmergencyVisitService emergencyService,
            EmergencyDispositionService emergencyDispositionService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _emergencyVisitService = emergencyService;
            _emergencyDispositionService = emergencyDispositionService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencyVisitResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Visit", Description = "Melihat data kunjungan IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyVisit", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? arrivalModeId,
            [FromQuery] Guid? caseTypeId,
            [FromQuery] EmergencyRegistrationStatus? registrationStatus,
            [FromQuery] EmergencyVisitStatus? visitStatus,
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
            IQueryable<TrxEmergencyVisit> query = _dbContext.Set<TrxEmergencyVisit>().AsNoTracking().Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.EmergencyVisitNumber.ToLower().Contains(keyword) ||
                    (x.ChiefComplaint != null && x.ChiefComplaint.ToLower().Contains(keyword)) ||
                    (x.ArrivalLocation != null && x.ArrivalLocation.ToLower().Contains(keyword)) ||
                    (x.FoundLocation != null && x.FoundLocation.ToLower().Contains(keyword)) ||
                    (x.TraumaLocation != null && x.TraumaLocation.ToLower().Contains(keyword)) ||
                    (x.TemporaryPatientAlias != null && x.TemporaryPatientAlias.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (arrivalModeId.HasValue && arrivalModeId.Value != Guid.Empty)
                query = query.Where(x => x.ArrivalModeId == arrivalModeId.Value);

            if (caseTypeId.HasValue && caseTypeId.Value != Guid.Empty)
                query = query.Where(x => x.CaseTypeId == caseTypeId.Value);

            if (registrationStatus.HasValue)
                query = query.Where(x => x.RegistrationStatus == registrationStatus.Value);

            if (visitStatus.HasValue)
                query = query.Where(x => x.VisitStatus == visitStatus.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.ArrivalDateTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.ArrivalDateTime < endDate.Value.Date.AddDays(1));

            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "arrivaldatetime" => descending ? query.OrderByDescending(x => x.ArrivalDateTime) : query.OrderBy(x => x.ArrivalDateTime),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.ArrivalDateTime) : query.OrderBy(x => x.ArrivalDateTime)
            };

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<EmergencyVisitResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<EmergencyVisitResponse>>.Ok(result, "Data kunjungan IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyVisitResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Visit", Description = "Melihat detail kunjungan IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyVisit", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyVisit>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data kunjungan IGD tidak ditemukan."));

            return Ok(ApiResponse<EmergencyVisitResponse>.Ok(ToResponse(entity), "Detail kunjungan IGD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyVisitResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Visit", Description = "Membuat kunjungan IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyVisit", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyVisitRequest request, CancellationToken cancellationToken = default)
        {
            var validationMessage = await _emergencyVisitService.ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedNumber = NormalizeText(request.EmergencyVisitNumber);
            if (!string.IsNullOrWhiteSpace(normalizedNumber) && await _dbContext.Set<TrxEmergencyVisit>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.EmergencyVisitNumber == normalizedNumber, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "EmergencyVisitNumber sudah digunakan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new TrxEmergencyVisit
            {
                Id = Guid.NewGuid(),
                EmergencyVisitNumber = string.IsNullOrWhiteSpace(request.EmergencyVisitNumber) ? await _emergencyVisitService.GenerateVisitNumberAsync(now, cancellationToken) : request.EmergencyVisitNumber.Trim(),
                EncounterId = request.EncounterId,
                PatientId = request.PatientId,
                ServiceUnitId = request.ServiceUnitId,
                ArrivalModeId = request.ArrivalModeId,
                CaseTypeId = request.CaseTypeId,
                ArrivalDateTime = request.ArrivalDateTime == default ? now : request.ArrivalDateTime,
                ChiefComplaint = NormalizeText(request.ChiefComplaint),
                ArrivalLocation = NormalizeText(request.ArrivalLocation),
                FoundLocation = NormalizeText(request.FoundLocation),
                TraumaLocation = NormalizeText(request.TraumaLocation),
                TraumaDateTime = request.TraumaDateTime,
                IsUnknownPatient = request.IsUnknownPatient,
                TemporaryPatientAlias = NormalizeText(request.TemporaryPatientAlias),
                IsImmediateCareAllowed = request.IsImmediateCareAllowed,
                RegistrationStatus = request.RegistrationStatus,
                VisitStatus = request.VisitStatus,
                RegistrationCompletedAt = request.RegistrationCompletedAt,
                RegistrationCompletedByUserId = request.RegistrationCompletedByUserId ?? actorUserId,
                TreatmentStartedAt = request.TreatmentStartedAt,
                VisitCompletedAt = request.VisitCompletedAt,
                Notes = NormalizeText(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxEmergencyVisit>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data kunjungan IGD gagal disimpan karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyVisit.Create",
                "Membuat data Emergency Visit.",
                new { EntityId = entity.Id, Controller = "EmergencyVisit", Action = "Create" }
            );

            return Ok(ApiResponse<EmergencyVisitResponse>.Ok(ToResponse(entity), "Data kunjungan IGD berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyVisitResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Visit", Description = "Mengubah kunjungan IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyVisit", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmergencyVisitRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyVisit>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data kunjungan IGD tidak ditemukan."));

            var validationMessage = await _emergencyVisitService.ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedNumber = NormalizeText(request.EmergencyVisitNumber);
            if (!string.IsNullOrWhiteSpace(normalizedNumber) && await _dbContext.Set<TrxEmergencyVisit>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.EmergencyVisitNumber == normalizedNumber && x.Id != id, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "EmergencyVisitNumber sudah digunakan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.EmergencyVisitNumber = string.IsNullOrWhiteSpace(request.EmergencyVisitNumber) ? entity.EmergencyVisitNumber : request.EmergencyVisitNumber.Trim();
            entity.EncounterId = request.EncounterId;
            entity.PatientId = request.PatientId;
            entity.ServiceUnitId = request.ServiceUnitId;
            entity.ArrivalModeId = request.ArrivalModeId;
            entity.CaseTypeId = request.CaseTypeId;
            entity.ArrivalDateTime = request.ArrivalDateTime;
            entity.ChiefComplaint = NormalizeText(request.ChiefComplaint);
            entity.ArrivalLocation = NormalizeText(request.ArrivalLocation);
            entity.FoundLocation = NormalizeText(request.FoundLocation);
            entity.TraumaLocation = NormalizeText(request.TraumaLocation);
            entity.TraumaDateTime = request.TraumaDateTime;
            entity.IsUnknownPatient = request.IsUnknownPatient;
            entity.TemporaryPatientAlias = NormalizeText(request.TemporaryPatientAlias);
            entity.IsImmediateCareAllowed = request.IsImmediateCareAllowed;
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
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data kunjungan IGD gagal diubah karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyVisit.Update",
                "Mengubah data Emergency Visit.",
                new { EntityId = id, Controller = "EmergencyVisit", Action = "Update" }
            );

            return Ok(ApiResponse<EmergencyVisitResponse>.Ok(ToResponse(entity), "Data kunjungan IGD berhasil diubah."));
        }

        [HttpPatch("{id:guid}/registration-status")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyVisitResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Emergency Visit RegistrationStatus", Description = "Mengubah status kunjungan IGD", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyVisit", "Update")]
        public async Task<IActionResult> UpdateRegistrationStatus(Guid id, [FromBody] UpdateEmergencyVisitRegistrationStatusRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyVisit>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data kunjungan IGD tidak ditemukan."));

            if (!_emergencyVisitService.CanTransition(entity.RegistrationStatus, request.RegistrationStatus))
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, $"Perubahan status dari {entity.RegistrationStatus} ke {request.RegistrationStatus} tidak diperbolehkan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.RegistrationStatus = request.RegistrationStatus;
            if (request.RegistrationStatus is EmergencyRegistrationStatus.Registered or EmergencyRegistrationStatus.Completed)
            {
                entity.RegistrationCompletedAt ??= now;
                entity.RegistrationCompletedByUserId = actorUserId;
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
                "EmergencyVisit.UpdateRegistrationStatus",
                "Memperbarui proses Emergency Visit melalui aksi UpdateRegistrationStatus.",
                new { EntityId = id, Controller = "EmergencyVisit", Action = "UpdateRegistrationStatus" }
            );

            return Ok(ApiResponse<EmergencyVisitResponse>.Ok(ToResponse(entity), "Status kunjungan IGD berhasil diubah."));
        }

        [HttpPatch("{id:guid}/visit-status")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyVisitResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Emergency Visit VisitStatus", Description = "Mengubah status kunjungan IGD", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyVisit", "Update")]
        public async Task<IActionResult> UpdateVisitStatus(Guid id, [FromBody] UpdateEmergencyVisitVisitStatusRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyVisit>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data kunjungan IGD tidak ditemukan."));

            // Penyelesaian klinis punya closure gate sendiri. Bila ditetapkan lewat endpoint
            // status umum, gate itu terlewati sepenuhnya, jadi jalurnya ditutup di sini.
            if (request.VisitStatus == EmergencyVisitStatus.Completed)
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Penyelesaian kunjungan hanya dapat dilakukan melalui aksi selesaikan kunjungan."));

            if (!_emergencyVisitService.CanTransition(entity.VisitStatus, request.VisitStatus))
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, $"Perubahan status dari {entity.VisitStatus} ke {request.VisitStatus} tidak diperbolehkan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.VisitStatus = request.VisitStatus;
            if (request.VisitStatus == EmergencyVisitStatus.InTreatment)
                entity.TreatmentStartedAt ??= now;
            // VisitCompletedAt sengaja tidak diisi di sini. "Keputusan tindak lanjut sudah
            // ditetapkan" (Disposed) dan "kunjungan dibatalkan" (Cancelled) bukan penyelesaian
            // klinis. Kolom ini kini hanya diisi oleh PATCH /{id}/complete. Baris lama yang
            // terlanjur terisi dibiarkan apa adanya karena mengubahnya memalsukan riwayat.
            if (!string.IsNullOrWhiteSpace(request.Notes) && entity.GetType().GetProperty("Notes") != null)
            {
                entity.GetType().GetProperty("Notes")?.SetValue(entity, NormalizeText(request.Notes));
            }
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyVisit.UpdateVisitStatus",
                "Memperbarui proses Emergency Visit melalui aksi UpdateVisitStatus.",
                new { EntityId = id, Controller = "EmergencyVisit", Action = "UpdateVisitStatus" }
            );

            return Ok(ApiResponse<EmergencyVisitResponse>.Ok(ToResponse(entity), "Status kunjungan IGD berhasil diubah."));
        }

        /// <summary>
        /// Menyelesaikan kunjungan secara klinis. Route dan hak aksesnya milik resource
        /// EmergencyVisit sesuai api contract dan permission matrix, sedangkan pemeriksaan
        /// closure gate-nya dipegang EmergencyDispositionService sesuai arsitektur bagian 3.3.
        /// </summary>
        [HttpPatch("{id:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyVisitResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Complete Emergency Visit", Description = "Menyelesaikan kunjungan IGD setelah seluruh kewajiban tuntas", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("EmergencyVisit", "Update")]
        public async Task<IActionResult> Complete(
            Guid id,
            [FromBody] CompleteVisitRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyVisit>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data kunjungan IGD tidak ditemukan."));

            var penolakan = await _emergencyDispositionService.ValidateVisitClosureAsync(entity, cancellationToken);
            if (penolakan != null)
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, penolakan));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.VisitStatus = EmergencyVisitStatus.Completed;
            entity.VisitCompletedAt = now;

            if (!string.IsNullOrWhiteSpace(request?.Notes) && entity.GetType().GetProperty("Notes") != null)
            {
                entity.GetType().GetProperty("Notes")?.SetValue(entity, NormalizeText(request.Notes));
            }

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyVisit.Complete",
                "Memperbarui proses Emergency Visit melalui aksi Complete.",
                new { EntityId = id, Controller = "EmergencyVisit", Action = "Complete" }
            );

            return Ok(ApiResponse<EmergencyVisitResponse>.Ok(ToResponse(entity), "Kunjungan IGD berhasil diselesaikan."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Emergency Visit", Description = "Menghapus kunjungan IGD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmergencyVisit", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyVisit>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data kunjungan IGD tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyVisit.Delete",
                "Menghapus data Emergency Visit.",
                new { EntityId = id, Controller = "EmergencyVisit", Action = "Delete" }
            );

            return Ok(ApiResponse<object>.Ok(null, "Data kunjungan IGD berhasil dihapus."));
        }

        private async Task<string?> ValidateRequestAsync(CreateEmergencyVisitRequest request, CancellationToken cancellationToken)
        {
            if (request.ServiceUnitId == Guid.Empty)
                return "ServiceUnitId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyRegistrationStatus), request.RegistrationStatus))
                return "Nilai RegistrationStatus tidak valid.";

            if (!Enum.IsDefined(typeof(EmergencyVisitStatus), request.VisitStatus))
                return "Nilai VisitStatus tidak valid.";

            if (!request.IsUnknownPatient && (!request.PatientId.HasValue || request.PatientId.Value == Guid.Empty))
                return "PatientId wajib diisi untuk pasien yang sudah dikenal.";

            if ((request.RegistrationStatus == EmergencyRegistrationStatus.Registered || request.RegistrationStatus == EmergencyRegistrationStatus.Completed) &&
                (!request.EncounterId.HasValue || request.EncounterId.Value == Guid.Empty))
                return "EncounterId wajib tersedia ketika registrasi IGD sudah terdaftar atau selesai.";

            var emergencySetting = await _dbContext.Set<MstEmergencySetting>()
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (emergencySetting == null)
                return "Setting IGD aktif belum tersedia.";

            if (request.ServiceUnitId != emergencySetting.DefaultEmergencyServiceUnitId)
                return "Asal kunjungan harus IGD. ServiceUnitId harus sama dengan DefaultEmergencyServiceUnitId pada setting IGD aktif.";

            if (request.EncounterId.HasValue && request.EncounterId.Value != Guid.Empty)
            {
                var encounter = await _dbContext.Set<TrxPatientEncounter>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.EncounterId.Value && !x.IsDelete, cancellationToken);

                if (encounter == null)
                    return "EncounterId tidak ditemukan.";

                if (encounter.EncounterType != EncounterType.Outpatient)
                    return "Jenis kunjungan pasien IGD harus OP (EncounterType.Outpatient).";

                if (encounter.ServiceUnitId != request.ServiceUnitId)
                    return "ServiceUnitId kunjungan IGD harus sama dengan ServiceUnitId pada encounter.";

                if (request.PatientId.HasValue && request.PatientId.Value != Guid.Empty && encounter.PatientId != request.PatientId.Value)
                    return "PatientId tidak sesuai dengan pasien pada encounter.";
            }

            if (request.PatientId.HasValue && request.PatientId.Value != Guid.Empty &&
                !await _dbContext.Set<MstPatient>().AsNoTracking().AnyAsync(x => x.Id == request.PatientId.Value && !x.IsDelete, cancellationToken))
                return "PatientId tidak ditemukan.";

            if (!await _dbContext.Set<MstServiceUnit>().AsNoTracking().AnyAsync(x => x.Id == request.ServiceUnitId && !x.IsDelete, cancellationToken))
                return "ServiceUnitId tidak ditemukan.";

            if (request.ArrivalModeId.HasValue && request.ArrivalModeId.Value != Guid.Empty &&
                !await _dbContext.Set<MstEmergencyArrivalMode>().AsNoTracking().AnyAsync(x => x.Id == request.ArrivalModeId.Value && !x.IsDelete, cancellationToken))
                return "ArrivalModeId tidak ditemukan.";

            if (request.CaseTypeId.HasValue && request.CaseTypeId.Value != Guid.Empty &&
                !await _dbContext.Set<MstEmergencyCaseType>().AsNoTracking().AnyAsync(x => x.Id == request.CaseTypeId.Value && !x.IsDelete, cancellationToken))
                return "CaseTypeId tidak ditemukan.";

            return null;
        }

        private Task<string?> ValidateRequestAsync(UpdateEmergencyVisitRequest request, CancellationToken cancellationToken)
            => ValidateRequestAsync((CreateEmergencyVisitRequest)request, cancellationToken);

        private static bool CanTransition(EmergencyRegistrationStatus current, EmergencyRegistrationStatus target)
        {
            if (current == target) return true;

            return current switch
            {
                EmergencyRegistrationStatus.Pending => target is EmergencyRegistrationStatus.Provisional or EmergencyRegistrationStatus.Registered or EmergencyRegistrationStatus.Cancelled,
                EmergencyRegistrationStatus.Provisional => target is EmergencyRegistrationStatus.Registered or EmergencyRegistrationStatus.Completed or EmergencyRegistrationStatus.Cancelled,
                EmergencyRegistrationStatus.Registered => target is EmergencyRegistrationStatus.Completed or EmergencyRegistrationStatus.Cancelled,
                EmergencyRegistrationStatus.Completed => false,
                EmergencyRegistrationStatus.Cancelled => false,
                _ => false
            };
        }

        private static bool CanTransition(EmergencyVisitStatus current, EmergencyVisitStatus target)
        {
            if (current == target) return true;

            return current switch
            {
                EmergencyVisitStatus.Arrived => target is EmergencyVisitStatus.WaitingForTriage or EmergencyVisitStatus.InTreatment or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.WaitingForTriage => target is EmergencyVisitStatus.Triaged or EmergencyVisitStatus.InTreatment or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.Triaged => target is EmergencyVisitStatus.InTreatment or EmergencyVisitStatus.UnderObservation or EmergencyVisitStatus.AwaitingDisposition or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.InTreatment => target is EmergencyVisitStatus.UnderObservation or EmergencyVisitStatus.AwaitingDisposition or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.UnderObservation => target is EmergencyVisitStatus.InTreatment or EmergencyVisitStatus.AwaitingDisposition or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.AwaitingDisposition => target is EmergencyVisitStatus.Disposed or EmergencyVisitStatus.InTreatment or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.Disposed => false,
                EmergencyVisitStatus.Cancelled => false,
                _ => false
            };
        }

        private static EmergencyVisitResponse ToResponse(TrxEmergencyVisit x)
        {
            return new EmergencyVisitResponse
            {
                Id = x.Id,
                EmergencyVisitNumber = x.EmergencyVisitNumber,
                EncounterId = x.EncounterId,
                PatientId = x.PatientId,
                ServiceUnitId = x.ServiceUnitId,
                ArrivalModeId = x.ArrivalModeId,
                CaseTypeId = x.CaseTypeId,
                ArrivalDateTime = x.ArrivalDateTime,
                ChiefComplaint = x.ChiefComplaint,
                ArrivalLocation = x.ArrivalLocation,
                FoundLocation = x.FoundLocation,
                TraumaLocation = x.TraumaLocation,
                TraumaDateTime = x.TraumaDateTime,
                IsUnknownPatient = x.IsUnknownPatient,
                TemporaryPatientAlias = x.TemporaryPatientAlias,
                IsImmediateCareAllowed = x.IsImmediateCareAllowed,
                RegistrationStatus = x.RegistrationStatus,
                VisitStatus = x.VisitStatus,
                RegistrationCompletedAt = x.RegistrationCompletedAt,
                RegistrationCompletedByUserId = x.RegistrationCompletedByUserId,
                TreatmentStartedAt = x.TreatmentStartedAt,
                VisitCompletedAt = x.VisitCompletedAt,
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
