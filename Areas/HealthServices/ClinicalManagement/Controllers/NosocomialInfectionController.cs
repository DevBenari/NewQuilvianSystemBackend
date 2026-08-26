using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseNosocomialInfectionPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.NosocomialInfectionResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    /// <summary>
    /// Surveilans infeksi terkait pelayanan kesehatan (nosokomial).
    /// </summary>
    /// <remarks>
    /// Berada di Clinical Management karena surveilans berlaku untuk seluruh unit pelayanan,
    /// bukan hanya IGD. Layar pengkajian IGD memakainya dengan menyaring menurut
    /// <c>emergencyVisitId</c>.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/nosocomial-infections")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Nosocomial Infection",
        AreaName = "HealthServices",
        ControllerName = "NosocomialInfection",
        Description = "Pencatatan dan surveilans infeksi terkait pelayanan kesehatan",
        SortOrder = 16
    )]
    [Tags("Health Services / Clinical Management / Nosocomial Infection")]
    public class NosocomialInfectionController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public NosocomialInfectionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        // ==========================================================
        // METADATA
        // ==========================================================

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<NosocomialInfectionFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nosocomial Infection", Description = "Melihat metadata filter infeksi nosokomial", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NosocomialInfection", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new NosocomialInfectionFilterMetadataResponse
            {
                InfectionTypes = BuildOptions<NosocomialInfectionType>(),
                Statuses = BuildOptions<NosocomialInfectionStatus>(),
                OnsetCategories = BuildOptions<NosocomialInfectionOnsetCategory>()
            };

            return Ok(ApiResponse<NosocomialInfectionFilterMetadataResponse>.Ok(
                result,
                "Metadata filter infeksi nosokomial berhasil diambil."));
        }

        // ==========================================================
        // LIST
        // ==========================================================

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseNosocomialInfectionPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nosocomial Infection List", Description = "Melihat daftar catatan infeksi nosokomial", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("NosocomialInfection", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? emergencyVisitId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] NosocomialInfectionType? infectionType,
            [FromQuery] NosocomialInfectionStatus? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "onsetDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<TrxNosocomialInfection>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (patientId.HasValue)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (emergencyVisitId.HasValue)
                query = query.Where(x => x.EmergencyVisitId == emergencyVisitId.Value);

            if (encounterId.HasValue)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (serviceUnitId.HasValue)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (infectionType.HasValue)
                query = query.Where(x => x.InfectionType == infectionType.Value);

            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.OnsetDateTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.OnsetDateTime <= endDate.Value);

            var totalData = await query.CountAsync(cancellationToken);

            query = ApplySorting(query, sortBy, sortDirection);

            var safePageSize = pageSize < 1 ? 10 : pageSize;
            var safePageNumber = pageNumber < 1 ? 1 : pageNumber;

            var entities = await query
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync(cancellationToken);

            var items = await ToResponsesAsync(entities, cancellationToken);

            var result = new ResponseNosocomialInfectionPagedResult
            {
                Items = items,
                TotalData = totalData,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalPage = (int)Math.Ceiling(totalData / (double)safePageSize)
            };

            return Ok(ApiResponse<ResponseNosocomialInfectionPagedResult>.Ok(
                result,
                "Daftar infeksi nosokomial berhasil diambil."));
        }

        // ==========================================================
        // DETAIL
        // ==========================================================

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NosocomialInfectionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nosocomial Infection Detail", Description = "Melihat detail catatan infeksi nosokomial", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("NosocomialInfection", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxNosocomialInfection>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<string>.Fail(StatusCodes.Status404NotFound, "Catatan infeksi nosokomial tidak ditemukan."));

            var responses = await ToResponsesAsync(new List<TrxNosocomialInfection> { entity }, cancellationToken);

            return Ok(ApiResponse<NosocomialInfectionResponse>.Ok(
                responses[0],
                "Detail infeksi nosokomial berhasil diambil."));
        }

        // ==========================================================
        // CREATE
        // ==========================================================

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NosocomialInfectionResponse>), StatusCodes.Status201Created)]
        [AccessAction("Create", "Create Nosocomial Infection", Description = "Mencatat kejadian infeksi nosokomial", AccessType = AccessTypes.Create, SortOrder = 4)]
        [AccessPermission("NosocomialInfection", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateNosocomialInfectionRequest request,
            CancellationToken cancellationToken = default)
        {
            var validationMessage = await ValidateAsync(request, cancellationToken);

            if (validationMessage != null)
                return BadRequest(ApiResponse<string>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var admissionAt = request.AdmissionDateTimeSnapshot;

            var entity = new TrxNosocomialInfection
            {
                Id = Guid.NewGuid(),
                NosocomialRecordNumber = await GenerateRecordNumberAsync(now, cancellationToken),
                PatientId = request.PatientId,
                EncounterId = request.EncounterId,
                EmergencyVisitId = request.EmergencyVisitId,
                AssessmentId = request.AssessmentId,
                ServiceUnitId = request.ServiceUnitId,
                DoctorId = request.DoctorId,

                InfectionType = request.InfectionType,
                InfectionTypeOther = request.InfectionTypeOther,
                Status = NosocomialInfectionStatus.Suspected,
                OnsetCategory = ResolveOnsetCategory(request.OnsetCategory, admissionAt, request.OnsetDateTime),
                OnsetDateTime = request.OnsetDateTime,
                AdmissionDateTimeSnapshot = admissionAt,
                HoursSinceAdmission = CalculateHoursSinceAdmission(admissionAt, request.OnsetDateTime),

                IsDeviceAssociated = request.IsDeviceAssociated,
                DeviceName = request.IsDeviceAssociated ? request.DeviceName : null,
                DeviceInsertedAt = request.IsDeviceAssociated ? request.DeviceInsertedAt : null,
                DeviceUsageDays = request.IsDeviceAssociated ? request.DeviceUsageDays : null,

                CriteriaMet = request.CriteriaMet,
                CultureSpecimenType = request.CultureSpecimenType,
                CultureTakenAt = request.CultureTakenAt,
                CultureResult = request.CultureResult,
                CausativeOrganism = request.CausativeOrganism,
                AntibioticTherapy = request.AntibioticTherapy,

                ReportedAt = now,
                ReportedByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                ReportedByNameSnapshot = User.Identity?.Name,

                Notes = request.Notes,
                IsActive = request.IsActive,

                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<TrxNosocomialInfection>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var responses = await ToResponsesAsync(new List<TrxNosocomialInfection> { entity }, cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "NosocomialInfection.Create",
                "Mencatat kejadian infeksi nosokomial.",
                responses[0]);

            return CreatedAtAction(
                nameof(GetById),
                new { id = entity.Id },
                ApiResponse<NosocomialInfectionResponse>.Ok(
                    responses[0],
                    "Catatan infeksi nosokomial berhasil dibuat."));
        }

        // ==========================================================
        // UPDATE
        // ==========================================================

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NosocomialInfectionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Nosocomial Infection", Description = "Mengubah catatan infeksi nosokomial", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("NosocomialInfection", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateNosocomialInfectionRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxNosocomialInfection>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<string>.Fail(StatusCodes.Status404NotFound, "Catatan infeksi nosokomial tidak ditemukan."));

            // Kejadian yang sudah ditutup tidak diubah isinya. Menutup lalu mengubah berarti
            // hasil verifikasi tim PPI dapat bergeser tanpa jejak.
            if (entity.Status is NosocomialInfectionStatus.Resolved
                or NosocomialInfectionStatus.RuledOut
                or NosocomialInfectionStatus.Cancelled)
            {
                return Conflict(ApiResponse<string>.Fail(StatusCodes.Status409Conflict, 
                    "Catatan yang sudah ditutup tidak dapat diubah. Buka kembali statusnya lebih dulu."));
            }

            var validationMessage = await ValidateAsync(request, cancellationToken);

            if (validationMessage != null)
                return BadRequest(ApiResponse<string>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var now = DateTime.UtcNow;
            var admissionAt = request.AdmissionDateTimeSnapshot ?? entity.AdmissionDateTimeSnapshot;

            entity.EncounterId = request.EncounterId;
            entity.EmergencyVisitId = request.EmergencyVisitId;
            entity.AssessmentId = request.AssessmentId;
            entity.ServiceUnitId = request.ServiceUnitId;
            entity.DoctorId = request.DoctorId;

            entity.InfectionType = request.InfectionType;
            entity.InfectionTypeOther = request.InfectionTypeOther;
            entity.OnsetCategory = ResolveOnsetCategory(request.OnsetCategory, admissionAt, request.OnsetDateTime);
            entity.OnsetDateTime = request.OnsetDateTime;
            entity.AdmissionDateTimeSnapshot = admissionAt;
            entity.HoursSinceAdmission = CalculateHoursSinceAdmission(admissionAt, request.OnsetDateTime);

            entity.IsDeviceAssociated = request.IsDeviceAssociated;
            entity.DeviceName = request.IsDeviceAssociated ? request.DeviceName : null;
            entity.DeviceInsertedAt = request.IsDeviceAssociated ? request.DeviceInsertedAt : null;
            entity.DeviceUsageDays = request.IsDeviceAssociated ? request.DeviceUsageDays : null;

            entity.CriteriaMet = request.CriteriaMet;
            entity.CultureSpecimenType = request.CultureSpecimenType;
            entity.CultureTakenAt = request.CultureTakenAt;
            entity.CultureResult = request.CultureResult;
            entity.CausativeOrganism = request.CausativeOrganism;
            entity.AntibioticTherapy = request.AntibioticTherapy;

            entity.Notes = request.Notes;
            entity.IsActive = request.IsActive;

            entity.UpdateDateTime = now;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            var responses = await ToResponsesAsync(new List<TrxNosocomialInfection> { entity }, cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "NosocomialInfection.Update",
                "Mengubah catatan infeksi nosokomial.",
                responses[0]);

            return Ok(ApiResponse<NosocomialInfectionResponse>.Ok(
                responses[0],
                "Catatan infeksi nosokomial berhasil diperbarui."));
        }

        // ==========================================================
        // STATUS
        // ==========================================================

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<NosocomialInfectionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Nosocomial Infection Status", Description = "Menetapkan status kejadian infeksi nosokomial", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("NosocomialInfection", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateNosocomialInfectionStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxNosocomialInfection>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<string>.Fail(StatusCodes.Status404NotFound, "Catatan infeksi nosokomial tidak ditemukan."));

            if (entity.Status == request.Status)
            {
                return Conflict(ApiResponse<string>.Fail(StatusCodes.Status409Conflict, 
                    "Status kejadian sudah bernilai sama, tidak ada yang perlu diubah."));
            }

            // Menyatakan bukan infeksi terkait pelayanan berarti kejadian ini keluar dari
            // hitungan indikator mutu. Alasannya wajib supaya keputusan itu dapat ditinjau.
            if (request.Status == NosocomialInfectionStatus.RuledOut
                && string.IsNullOrWhiteSpace(request.RuledOutReason))
            {
                return BadRequest(ApiResponse<string>.Fail(StatusCodes.Status400BadRequest, 
                    "Alasan wajib diisi ketika kejadian dinyatakan bukan infeksi terkait pelayanan."));
            }

            // Hanya kejadian yang sudah terkonfirmasi yang dapat dinyatakan teratasi.
            if (request.Status == NosocomialInfectionStatus.Resolved
                && entity.Status != NosocomialInfectionStatus.Confirmed)
            {
                return Conflict(ApiResponse<string>.Fail(StatusCodes.Status409Conflict, 
                    "Kejadian hanya dapat dinyatakan teratasi setelah dikonfirmasi lebih dulu."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.Status = request.Status;

            if (request.Status == NosocomialInfectionStatus.Confirmed)
            {
                entity.VerifiedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
                entity.VerifiedByNameSnapshot = User.Identity?.Name;
                entity.VerifiedAt = now;
            }

            if (request.Status == NosocomialInfectionStatus.RuledOut)
                entity.RuledOutReason = request.RuledOutReason;

            if (request.Status == NosocomialInfectionStatus.Resolved)
                entity.ResolvedAt = now;

            if (!string.IsNullOrWhiteSpace(request.Notes))
                entity.Notes = request.Notes;

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var responses = await ToResponsesAsync(new List<TrxNosocomialInfection> { entity }, cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "NosocomialInfection.UpdateStatus",
                "Menetapkan status kejadian infeksi nosokomial.",
                responses[0]);

            return Ok(ApiResponse<NosocomialInfectionResponse>.Ok(
                responses[0],
                "Status kejadian infeksi nosokomial berhasil diperbarui."));
        }

        // ==========================================================
        // DELETE
        // ==========================================================

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Nosocomial Infection", Description = "Menghapus catatan infeksi nosokomial", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("NosocomialInfection", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxNosocomialInfection>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<string>.Fail(StatusCodes.Status404NotFound, "Catatan infeksi nosokomial tidak ditemukan."));

            var now = DateTime.UtcNow;

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "NosocomialInfection.Delete",
                "Menghapus catatan infeksi nosokomial.",
                new { entity.Id, entity.NosocomialRecordNumber });

            return Ok(ApiResponse<string>.Ok(
                entity.Id.ToString(),
                "Catatan infeksi nosokomial berhasil dihapus."));
        }

        // ==========================================================
        // HELPERS
        // ==========================================================

        private async Task<string?> ValidateAsync(
            CreateNosocomialInfectionRequest request,
            CancellationToken cancellationToken)
        {
            if (request.PatientId == Guid.Empty)
                return "PatientId wajib diisi.";

            var patientExists = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.PatientId && !x.IsDelete, cancellationToken);

            if (!patientExists)
                return "Pasien tidak ditemukan.";

            if (request.OnsetDateTime == default)
                return "Waktu munculnya tanda atau gejala wajib diisi.";

            if (request.OnsetDateTime > DateTime.UtcNow.AddDays(1))
                return "Waktu munculnya tanda atau gejala tidak boleh di masa depan.";

            if (request.InfectionType == NosocomialInfectionType.Other
                && string.IsNullOrWhiteSpace(request.InfectionTypeOther))
            {
                return "Sebutkan jenis infeksinya ketika memilih Lainnya.";
            }

            if (request.IsDeviceAssociated && string.IsNullOrWhiteSpace(request.DeviceName))
                return "Nama alat wajib diisi ketika infeksi dikaitkan dengan pemakaian alat.";

            if (request.AdmissionDateTimeSnapshot.HasValue
                && request.AdmissionDateTimeSnapshot.Value > request.OnsetDateTime)
            {
                return "Waktu mulai dirawat tidak boleh lebih akhir daripada waktu munculnya gejala.";
            }

            return null;
        }

        /// <summary>
        /// Menetapkan asal infeksi bila petugas belum memilihnya sendiri.
        /// </summary>
        /// <remarks>
        /// Batas 48 jam hanya dipakai sebagai usulan ketika petugas membiarkannya
        /// <c>Unknown</c>. Pilihan petugas selalu menang: mereka mengetahui hal yang tidak
        /// terbaca dari tanggal, misalnya pasien rujukan yang sudah terinfeksi sejak di
        /// fasilitas sebelumnya.
        /// </remarks>
        private static NosocomialInfectionOnsetCategory ResolveOnsetCategory(
            NosocomialInfectionOnsetCategory requested,
            DateTime? admissionAt,
            DateTime onsetAt)
        {
            if (requested != NosocomialInfectionOnsetCategory.Unknown)
                return requested;

            if (!admissionAt.HasValue)
                return NosocomialInfectionOnsetCategory.Unknown;

            return (onsetAt - admissionAt.Value).TotalHours > 48
                ? NosocomialInfectionOnsetCategory.HealthcareAssociated
                : NosocomialInfectionOnsetCategory.PresentOnAdmission;
        }

        private static int? CalculateHoursSinceAdmission(DateTime? admissionAt, DateTime onsetAt)
        {
            if (!admissionAt.HasValue)
                return null;

            var hours = (onsetAt - admissionAt.Value).TotalHours;

            return hours < 0 ? null : (int)Math.Floor(hours);
        }

        private async Task<string> GenerateRecordNumberAsync(DateTime now, CancellationToken cancellationToken)
        {
            var prefix = $"NOS-{now:yyyyMMdd}";

            var countToday = await _dbContext.Set<TrxNosocomialInfection>()
                .CountAsync(x => x.NosocomialRecordNumber.StartsWith(prefix), cancellationToken);

            return $"{prefix}-{countToday + 1:0000}";
        }

        /// <summary>
        /// Melengkapi respons dengan nama pasien dan nama unit.
        /// </summary>
        /// <remarks>
        /// Nama diambil di sini, bukan lewat navigation property, supaya query daftar tidak
        /// menarik seluruh entitas pasien dan unit hanya untuk mengambil dua kolom teks.
        /// </remarks>
        private async Task<List<NosocomialInfectionResponse>> ToResponsesAsync(
            List<TrxNosocomialInfection> entities,
            CancellationToken cancellationToken)
        {
            var patientIds = entities.Select(x => x.PatientId).Distinct().ToList();

            var serviceUnitIds = entities
                .Where(x => x.ServiceUnitId.HasValue)
                .Select(x => x.ServiceUnitId!.Value)
                .Distinct()
                .ToList();

            var patients = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => patientIds.Contains(x.Id))
                .Select(x => new { x.Id, x.FullName, x.MedicalRecordNumber })
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var serviceUnits = serviceUnitIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .Where(x => serviceUnitIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.ServiceUnitName, cancellationToken);

            return entities.Select(x =>
            {
                patients.TryGetValue(x.PatientId, out var patient);

                string? serviceUnitName = null;

                if (x.ServiceUnitId.HasValue)
                    serviceUnits.TryGetValue(x.ServiceUnitId.Value, out serviceUnitName);

                return new NosocomialInfectionResponse
                {
                    Id = x.Id,
                    NosocomialRecordNumber = x.NosocomialRecordNumber,
                    PatientId = x.PatientId,
                    PatientName = patient?.FullName,
                    MedicalRecordNumber = patient?.MedicalRecordNumber,
                    EncounterId = x.EncounterId,
                    EmergencyVisitId = x.EmergencyVisitId,
                    AssessmentId = x.AssessmentId,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = serviceUnitName,
                    DoctorId = x.DoctorId,

                    InfectionType = x.InfectionType,
                    InfectionTypeOther = x.InfectionTypeOther,
                    Status = x.Status,
                    OnsetCategory = x.OnsetCategory,

                    OnsetDateTime = x.OnsetDateTime,
                    AdmissionDateTimeSnapshot = x.AdmissionDateTimeSnapshot,
                    HoursSinceAdmission = x.HoursSinceAdmission,

                    IsDeviceAssociated = x.IsDeviceAssociated,
                    DeviceName = x.DeviceName,
                    DeviceInsertedAt = x.DeviceInsertedAt,
                    DeviceUsageDays = x.DeviceUsageDays,

                    CriteriaMet = x.CriteriaMet,
                    CultureSpecimenType = x.CultureSpecimenType,
                    CultureTakenAt = x.CultureTakenAt,
                    CultureResult = x.CultureResult,
                    CausativeOrganism = x.CausativeOrganism,
                    AntibioticTherapy = x.AntibioticTherapy,

                    ReportedAt = x.ReportedAt,
                    ReportedByUserId = x.ReportedByUserId,
                    ReportedByNameSnapshot = x.ReportedByNameSnapshot,
                    VerifiedByUserId = x.VerifiedByUserId,
                    VerifiedByNameSnapshot = x.VerifiedByNameSnapshot,
                    VerifiedAt = x.VerifiedAt,
                    RuledOutReason = x.RuledOutReason,
                    ResolvedAt = x.ResolvedAt,

                    Notes = x.Notes,
                    IsActive = x.IsActive,

                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                };
            }).ToList();
        }

        private static IQueryable<TrxNosocomialInfection> ApplySorting(
            IQueryable<TrxNosocomialInfection> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "onsetdatetime").ToLowerInvariant() switch
            {
                "reportedat" => isDesc ? query.OrderByDescending(x => x.ReportedAt) : query.OrderBy(x => x.ReportedAt),
                "infectiontype" => isDesc ? query.OrderByDescending(x => x.InfectionType) : query.OrderBy(x => x.InfectionType),
                "status" => isDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc ? query.OrderByDescending(x => x.OnsetDateTime) : query.OrderBy(x => x.OnsetDateTime)
            };
        }

        private static List<NosocomialInfectionOptionResponse> BuildOptions<TEnum>() where TEnum : struct, Enum =>
            Enum.GetValues<TEnum>()
                .Select(value => new NosocomialInfectionOptionResponse
                {
                    Value = Convert.ToInt32(value),
                    Label = value.ToString() ?? string.Empty
                })
                .ToList();

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
