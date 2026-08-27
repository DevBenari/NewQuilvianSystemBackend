using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
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
    [Route("api/v1/health-services/emergency-installation-management/emergency-dispositions")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Disposition",
        AreaName = "HealthServices",
        ControllerName = "EmergencyDisposition",
        Description = "Mengelola keputusan klinis akhir pelayanan pasien IGD",
        SortOrder = 8
    )]
    [Tags("Health Services / Emergency Installation Management / Emergency Disposition")]
    public class EmergencyDispositionController : ControllerBase
    {
        private const string LogCategory = "HealthServices.EmergencyInstallation";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly EmergencyDispositionService _emergencyDispositionService;
        private readonly EmergencyVisitService _emergencyVisitService;

        public EmergencyDispositionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            EmergencyDispositionService emergencyService,
            EmergencyVisitService emergencyVisitService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _emergencyDispositionService = emergencyService;
            _emergencyVisitService = emergencyVisitService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencyDispositionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Disposition", Description = "Melihat data tindak lanjut IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyDisposition", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? emergencyVisitId,
            [FromQuery] Guid? dispositionTypeId,
            [FromQuery] Guid? destinationServiceUnitId,
            [FromQuery] EmergencyDispositionStatus? dispositionStatus,
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
            IQueryable<TrxEmergencyDisposition> query = _dbContext.Set<TrxEmergencyDisposition>()
                .AsNoTracking()
                .Include(x => x.DispositionType)
                .Include(x => x.DestinationServiceUnit)
                .Include(x => x.DecidedByDoctor)
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.DestinationFacilityName != null && x.DestinationFacilityName.ToLower().Contains(keyword)) ||
                    (x.ReferralNumber != null && x.ReferralNumber.ToLower().Contains(keyword)) ||
                    (x.DispositionReason != null && x.DispositionReason.ToLower().Contains(keyword)) ||
                    (x.PatientConditionAtDisposition != null && x.PatientConditionAtDisposition.ToLower().Contains(keyword)) ||
                    (x.FollowUpInstruction != null && x.FollowUpInstruction.ToLower().Contains(keyword)) ||
                    (x.RefusalReason != null && x.RefusalReason.ToLower().Contains(keyword)) ||
                    (x.DeathLocation != null && x.DeathLocation.ToLower().Contains(keyword)) ||
                    (x.SuspectedCauseOfDeath != null && x.SuspectedCauseOfDeath.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }

            if (emergencyVisitId.HasValue && emergencyVisitId.Value != Guid.Empty)
                query = query.Where(x => x.EmergencyVisitId == emergencyVisitId.Value);

            if (dispositionTypeId.HasValue && dispositionTypeId.Value != Guid.Empty)
                query = query.Where(x => x.DispositionTypeId == dispositionTypeId.Value);

            if (destinationServiceUnitId.HasValue && destinationServiceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.DestinationServiceUnitId == destinationServiceUnitId.Value);

            if (dispositionStatus.HasValue)
                query = query.Where(x => x.DispositionStatus == dispositionStatus.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.DecidedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.DecidedAt < endDate.Value.Date.AddDays(1));

            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "decidedat" => descending ? query.OrderByDescending(x => x.DecidedAt) : query.OrderBy(x => x.DecidedAt),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.DecidedAt) : query.OrderBy(x => x.DecidedAt)
            };

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<EmergencyDispositionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<EmergencyDispositionResponse>>.Ok(result, "Data tindak lanjut IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDispositionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Disposition", Description = "Melihat detail tindak lanjut IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyDisposition", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyDisposition>()
                .AsNoTracking()
                .Include(x => x.DispositionType)
                .Include(x => x.DestinationServiceUnit)
                .Include(x => x.DecidedByDoctor)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data tindak lanjut IGD tidak ditemukan."));

            return Ok(ApiResponse<EmergencyDispositionResponse>.Ok(ToResponse(entity), "Detail tindak lanjut IGD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDispositionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Disposition", Description = "Membuat tindak lanjut IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyDisposition", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyDispositionRequest request, CancellationToken cancellationToken = default)
        {
            var validationMessage = await _emergencyDispositionService.ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new TrxEmergencyDisposition
            {
                Id = Guid.NewGuid(),
                EmergencyVisitId = request.EmergencyVisitId,
                DispositionTypeId = request.DispositionTypeId,
                DispositionStatus = request.DispositionStatus,
                DecidedAt = request.DecidedAt == default ? now : request.DecidedAt,
                DecidedByDoctorId = request.DecidedByDoctorId,
                ConfirmedByUserId = request.ConfirmedByUserId ?? actorUserId,
                ConfirmedAt = request.ConfirmedAt,
                ExecutedAt = request.ExecutedAt,
                DestinationServiceUnitId = request.DestinationServiceUnitId,
                DestinationFacilityName = NormalizeText(request.DestinationFacilityName),
                ReferralNumber = NormalizeText(request.ReferralNumber),
                DispositionReason = NormalizeText(request.DispositionReason),
                PatientConditionAtDisposition = NormalizeText(request.PatientConditionAtDisposition),
                FollowUpInstruction = NormalizeText(request.FollowUpInstruction),
                RefusalReason = NormalizeText(request.RefusalReason),
                IsPatientDeceased = request.IsPatientDeceased,
                DeathDateTime = request.DeathDateTime,
                DeathLocation = NormalizeText(request.DeathLocation),
                SuspectedCauseOfDeath = NormalizeText(request.SuspectedCauseOfDeath),
                IsVisumRequested = request.IsVisumRequested,
                Notes = NormalizeText(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxEmergencyDisposition>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data tindak lanjut IGD gagal disimpan karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyDisposition.Create",
                "Membuat data Emergency Disposition.",
                new { EntityId = entity.Id, Controller = "EmergencyDisposition", Action = "Create" }
            );

            await LoadDispositionNamesAsync(entity, cancellationToken);

            return Ok(ApiResponse<EmergencyDispositionResponse>.Ok(ToResponse(entity), "Data tindak lanjut IGD berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDispositionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Disposition", Description = "Mengubah tindak lanjut IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyDisposition", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmergencyDispositionRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyDisposition>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data tindak lanjut IGD tidak ditemukan."));

            if (entity.DispositionStatus != EmergencyDispositionStatus.Draft)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Tindak lanjut yang sudah diproses tidak dapat ditimpa. Gunakan aksi status atau jalur koreksi yang tercatat."));
            }

            var validationMessage = await _emergencyDispositionService.ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.EmergencyVisitId = request.EmergencyVisitId;
            entity.DispositionTypeId = request.DispositionTypeId;
            entity.DecidedAt = request.DecidedAt;
            entity.DecidedByDoctorId = request.DecidedByDoctorId;
            entity.DestinationServiceUnitId = request.DestinationServiceUnitId;
            entity.DestinationFacilityName = NormalizeText(request.DestinationFacilityName);
            entity.ReferralNumber = NormalizeText(request.ReferralNumber);
            entity.DispositionReason = NormalizeText(request.DispositionReason);
            entity.PatientConditionAtDisposition = NormalizeText(request.PatientConditionAtDisposition);
            entity.FollowUpInstruction = NormalizeText(request.FollowUpInstruction);
            entity.RefusalReason = NormalizeText(request.RefusalReason);
            entity.IsPatientDeceased = request.IsPatientDeceased;
            entity.DeathDateTime = request.DeathDateTime;
            entity.DeathLocation = NormalizeText(request.DeathLocation);
            entity.SuspectedCauseOfDeath = NormalizeText(request.SuspectedCauseOfDeath);
            entity.IsVisumRequested = request.IsVisumRequested;
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
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data tindak lanjut IGD gagal diubah karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyDisposition.Update",
                "Mengubah data Emergency Disposition.",
                new { EntityId = id, Controller = "EmergencyDisposition", Action = "Update" }
            );

            await LoadDispositionNamesAsync(entity, cancellationToken);

            return Ok(ApiResponse<EmergencyDispositionResponse>.Ok(ToResponse(entity), "Data tindak lanjut IGD berhasil diubah."));
        }

        [HttpPatch("{id:guid}/disposition-status")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDispositionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Disposition DispositionStatus", Description = "Mengubah status tindak lanjut IGD", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyDisposition", "Update")]
        public async Task<IActionResult> UpdateDispositionStatus(Guid id, [FromBody] UpdateEmergencyDispositionDispositionStatusRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyDisposition>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data tindak lanjut IGD tidak ditemukan."));

            if (!_emergencyDispositionService.CanTransition(entity.DispositionStatus, request.DispositionStatus))
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, $"Perubahan status dari {entity.DispositionStatus} ke {request.DispositionStatus} tidak diperbolehkan."));

            // Membatalkan keputusan tindak lanjut berarti mencabut penentuan ke mana pasien
            // pergi setelah meninggalkan IGD. Tanpa alasan tertulis, pencabutan itu tidak
            // dapat ditinjau siapa pun sesudahnya.
            if (request.DispositionStatus == EmergencyDispositionStatus.Cancelled &&
                string.IsNullOrWhiteSpace(request.Notes))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan pembatalan wajib diisi ketika tindak lanjut dibatalkan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            // BE-IGD-021 — titik tulis VisitStatus kelima. Penjaga BE-IGD-018 dipanggil
            // sebelum entity diubah, supaya penolakan 409 tidak meninggalkan tindak lanjut
            // yang terlanjur berstatus Executed sementara kunjungannya tidak ikut pindah.
            if (request.DispositionStatus == EmergencyDispositionStatus.Executed)
            {
                var visit = await _dbContext.Set<TrxEmergencyVisit>().FirstAsync(x => x.Id == entity.EmergencyVisitId && !x.IsDelete, cancellationToken);

                if (!_emergencyVisitService.TryApplyVisitStatus(
                        visit, EmergencyVisitStatus.Disposed, actorUserId, now, out var penolakanStatusKunjungan))
                {
                    return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, penolakanStatusKunjungan!));
                }

                // VisitCompletedAt sengaja TIDAK diisi di sini, sejalan dengan BE-IGD-008.
                // "Keputusan tindak lanjut sudah ditetapkan" bukan berarti "urusan pasien di
                // IGD sudah tuntas": pasien masih dapat menunggu observasi selesai atau
                // menunggu proses kepergian. Waktu selesai hanya diisi oleh
                // PATCH /emergency-visits/{id}/complete setelah closure gate lulus.
            }

            entity.DispositionStatus = request.DispositionStatus;
            if (request.DispositionStatus == EmergencyDispositionStatus.Confirmed)
            {
                entity.ConfirmedAt ??= now;
                entity.ConfirmedByUserId = actorUserId;
            }
            if (request.DispositionStatus == EmergencyDispositionStatus.Executed)
            {
                entity.ExecutedAt ??= now;
            }
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                entity.Notes = NormalizeText(request.Notes);
            }
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyDisposition.UpdateDispositionStatus",
                "Memperbarui proses Emergency Disposition melalui aksi UpdateDispositionStatus.",
                new { EntityId = id, Controller = "EmergencyDisposition", Action = "UpdateDispositionStatus" }
            );

            await LoadDispositionNamesAsync(entity, cancellationToken);

            return Ok(ApiResponse<EmergencyDispositionResponse>.Ok(ToResponse(entity), "Status tindak lanjut IGD berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Emergency Disposition", Description = "Menghapus tindak lanjut IGD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmergencyDisposition", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyDisposition>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data tindak lanjut IGD tidak ditemukan."));

            if (entity.DispositionStatus != EmergencyDispositionStatus.Draft)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Tindak lanjut yang sudah diproses tidak dapat dihapus karena merupakan riwayat klinis."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyDisposition.Delete",
                "Menghapus data Emergency Disposition.",
                new { EntityId = id, Controller = "EmergencyDisposition", Action = "Delete" }
            );

            return Ok(ApiResponse<object>.Ok(null, "Data tindak lanjut IGD berhasil dihapus."));
        }

        private async Task<string?> ValidateRequestAsync(CreateEmergencyDispositionRequest request, CancellationToken cancellationToken)
        {
            if (request.EmergencyVisitId == Guid.Empty)
                return "EmergencyVisitId wajib diisi.";

            if (request.DispositionTypeId == Guid.Empty)
                return "DispositionTypeId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyDispositionStatus), request.DispositionStatus))
                return "Nilai DispositionStatus tidak valid.";

            if (request.IsPatientDeceased && !request.DeathDateTime.HasValue)
                return "DeathDateTime wajib diisi ketika pasien dinyatakan meninggal.";

            if (!await _dbContext.Set<TrxEmergencyVisit>().AsNoTracking().AnyAsync(x => x.Id == request.EmergencyVisitId && !x.IsDelete, cancellationToken))
                return "EmergencyVisitId tidak ditemukan.";

            if (!await _dbContext.Set<MstEmergencyDispositionType>().AsNoTracking().AnyAsync(x => x.Id == request.DispositionTypeId && !x.IsDelete, cancellationToken))
                return "DispositionTypeId tidak ditemukan.";

            if (request.DestinationServiceUnitId.HasValue && request.DestinationServiceUnitId.Value != Guid.Empty &&
                !await _dbContext.Set<MstServiceUnit>().AsNoTracking().AnyAsync(x => x.Id == request.DestinationServiceUnitId.Value && !x.IsDelete, cancellationToken))
                return "DestinationServiceUnitId tidak ditemukan.";

            return null;
        }

        private Task<string?> ValidateRequestAsync(UpdateEmergencyDispositionRequest request, CancellationToken cancellationToken)
            => ValidateRequestAsync((CreateEmergencyDispositionRequest)request, cancellationToken);

        private static bool CanTransition(EmergencyDispositionStatus current, EmergencyDispositionStatus target)
        {
            if (current == target) return true;

            return current switch
            {
                EmergencyDispositionStatus.Draft => target is EmergencyDispositionStatus.Confirmed or EmergencyDispositionStatus.Cancelled,
                EmergencyDispositionStatus.Confirmed => target is EmergencyDispositionStatus.Executed or EmergencyDispositionStatus.Cancelled,
                EmergencyDispositionStatus.Executed => false,
                EmergencyDispositionStatus.Cancelled => false,
                _ => false
            };
        }

        /// <summary>
        /// Memuat relasi jenis tindak lanjut, unit tujuan, dan dokter pemutus untuk entity
        /// yang baru saja ditulis, supaya balasan aksi tulis memuat nama sama seperti balasan
        /// aksi baca. Tanpa ini layar harus memuat ulang daftar hanya untuk memperoleh nama
        /// dari data yang baru saja dikirimnya sendiri.
        /// </summary>
        private async Task LoadDispositionNamesAsync(
            TrxEmergencyDisposition entity,
            CancellationToken cancellationToken)
        {
            var entry = _dbContext.Entry(entity);

            if (!entry.Reference(x => x.DispositionType).IsLoaded)
                await entry.Reference(x => x.DispositionType).LoadAsync(cancellationToken);

            if (entity.DestinationServiceUnitId.HasValue &&
                !entry.Reference(x => x.DestinationServiceUnit).IsLoaded)
                await entry.Reference(x => x.DestinationServiceUnit).LoadAsync(cancellationToken);

            if (entity.DecidedByDoctorId.HasValue &&
                !entry.Reference(x => x.DecidedByDoctor).IsLoaded)
                await entry.Reference(x => x.DecidedByDoctor).LoadAsync(cancellationToken);
        }

        private static EmergencyDispositionResponse ToResponse(TrxEmergencyDisposition x)
        {
            return new EmergencyDispositionResponse
            {
                Id = x.Id,
                EmergencyVisitId = x.EmergencyVisitId,
                DispositionTypeId = x.DispositionTypeId,
                DispositionTypeCode = x.DispositionType?.Code,
                DispositionTypeName = x.DispositionType?.Name,
                RequiresDestinationServiceUnit = x.DispositionType?.RequiresDestinationServiceUnit ?? false,
                RequiresReferralFacility = x.DispositionType?.RequiresReferralFacility ?? false,
                ClosesEmergencyVisit = x.DispositionType?.ClosesEmergencyVisit ?? false,
                DispositionStatus = x.DispositionStatus,
                DecidedAt = x.DecidedAt,
                DecidedByDoctorId = x.DecidedByDoctorId,
                DecidedByDoctorName = x.DecidedByDoctor?.FullName,
                ConfirmedByUserId = x.ConfirmedByUserId,
                ConfirmedAt = x.ConfirmedAt,
                ExecutedAt = x.ExecutedAt,
                DestinationServiceUnitId = x.DestinationServiceUnitId,
                DestinationServiceUnitName = x.DestinationServiceUnit?.ServiceUnitName,
                DestinationFacilityName = x.DestinationFacilityName,
                ReferralNumber = x.ReferralNumber,
                DispositionReason = x.DispositionReason,
                PatientConditionAtDisposition = x.PatientConditionAtDisposition,
                FollowUpInstruction = x.FollowUpInstruction,
                RefusalReason = x.RefusalReason,
                IsPatientDeceased = x.IsPatientDeceased,
                DeathDateTime = x.DeathDateTime,
                DeathLocation = x.DeathLocation,
                SuspectedCauseOfDeath = x.SuspectedCauseOfDeath,
                IsVisumRequested = x.IsVisumRequested,
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
