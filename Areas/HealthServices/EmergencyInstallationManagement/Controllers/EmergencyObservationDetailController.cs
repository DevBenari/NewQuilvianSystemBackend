using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Linq.Expressions;
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
        private readonly EmergencyObservationService _emergencyObservationService;

        public EmergencyObservationDetailController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            EmergencyObservationService emergencyObservationService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _emergencyObservationService = emergencyObservationService;
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

            // BE-IGD-046 - angka tanda vital dan nama pencatat ikut terbawa oleh kueri
            // daftar yang sama. Layar riwayat pemantauan tidak lagi perlu memanggil endpoint
            // tanda vital sekali per baris hanya untuk menampilkan angkanya.
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ProyeksiResponse)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<EmergencyObservationDetailResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
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
            var response = await BacaResponseAsync(id, cancellationToken);
            if (response == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data detail observasi IGD tidak ditemukan."));

            return Ok(ApiResponse<EmergencyObservationDetailResponse>.Ok(response, "Detail detail observasi IGD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyObservationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Observation Detail", Description = "Membuat detail observasi IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyObservationDetail", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyObservationDetailRequest request, CancellationToken cancellationToken = default)
        {
            // BE-IGD-046 - seluruh pemeriksaan selesai sebelum satu pun data berubah, dengan
            // urutan yang dikunci validation 0.6.0 bagian 9.1: periode ada, periode belum
            // ditutup, baru tautannya. Pencatatan baru menolak periode Completed dan
            // Cancelled (IGD-DEC-126).
            var pemeriksaan = await _emergencyObservationService.ValidateDetailScopeAsync(
                request.EmergencyObservationId,
                request.PatientVitalSignId,
                request.ProgressNoteId,
                tolakPeriodeTertutup: true,
                cancellationToken);

            if (!pemeriksaan.Lolos)
                return Failure(pemeriksaan.StatusCode, pemeriksaan.Penolakan!);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new EmgObservationDetail
            {
                Id = Guid.NewGuid(),
                EmergencyObservationId = request.EmergencyObservationId,
                PatientVitalSignId = request.PatientVitalSignId,
                ProgressNoteId = request.ProgressNoteId,
                RecordedAt = request.RecordedAt == default ? now : request.RecordedAt,
                // BE-IGD-046 - pelaku pencatat selalu dari pengguna yang terautentikasi.
                // Nilai RecordedByUserId yang dikirim pemanggil sengaja diabaikan, bukan
                // ditolak, supaya pemanggil lama tetap dilayani (validation 0.6.0 bagian 9
                // aturan 9, IGD-DEC-057).
                RecordedByUserId = actorUserId,
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

            return Ok(ApiResponse<EmergencyObservationDetailResponse>.Ok(
                (await BacaResponseAsync(entity.Id, cancellationToken))!,
                "Data detail observasi IGD berhasil dibuat."));
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

            // BE-IGD-046 - lingkup tautan diperiksa pada perubahan juga (validation 0.6.0
            // bagian 9 berlaku untuk POST dan PUT). Penolakan periode tertutup sengaja TIDAK
            // dinyalakan di sini: aturan 2 menolak "pemantauan baru", dan memperluasnya ke
            // PUT berarti mengubah perilaku yang tidak diperintahkan kontrak. Perbaikan PUT
            // menjadi tambah-saja (IGD-DEC-080) tetap di luar lingkup task ini.
            var pemeriksaan = await _emergencyObservationService.ValidateDetailScopeAsync(
                request.EmergencyObservationId,
                request.PatientVitalSignId,
                request.ProgressNoteId,
                tolakPeriodeTertutup: false,
                cancellationToken);

            if (!pemeriksaan.Lolos)
                return Failure(pemeriksaan.StatusCode, pemeriksaan.Penolakan!);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.EmergencyObservationId = request.EmergencyObservationId;
            entity.PatientVitalSignId = request.PatientVitalSignId;
            entity.ProgressNoteId = request.ProgressNoteId;
            entity.RecordedAt = request.RecordedAt;
            // BE-IGD-046 - RecordedByUserId tidak lagi diambil dari badan permintaan.
            // Pencatat aslinya dipertahankan apa adanya; pelaku perubahan tercatat pada
            // UpdateBy di bawah. Sebelumnya nilai dari pemanggil menimpa pencatat asli, dan
            // badan tanpa field itu bahkan mengosongkannya menjadi GUID nol.
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

            return Ok(ApiResponse<EmergencyObservationDetailResponse>.Ok(
                (await BacaResponseAsync(id, cancellationToken))!,
                "Data detail observasi IGD berhasil diubah."));
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

        /// <summary>
        /// Proyeksi baca satu baris pemantauan beserta angka tanda vital yang ditautkan dan
        /// nama petugas pencatatnya.
        /// </summary>
        /// <remarks>
        /// <c>BE-IGD-046</c>, API 0.6.0 bagian 7.2. Ditulis sebagai satu expression supaya
        /// daftar, detail, pembuatan, dan perubahan memakai bentuk balasan yang sama persis,
        /// dan supaya seluruhnya terbawa satu kueri lewat relasi <c>PatientVitalSign</c> dan
        /// <c>RecordedByUser</c> yang sudah dikonfigurasi. Baris lama yang
        /// <c>PatientVitalSignId</c>-nya kosong tetap terbaca dengan <c>VitalSign</c>
        /// bernilai <c>null</c>; tidak ada pengisian mundur.
        /// </remarks>
        private static readonly Expression<Func<EmgObservationDetail, EmergencyObservationDetailResponse>> ProyeksiResponse =
            x => new EmergencyObservationDetailResponse
            {
                Id = x.Id,
                EmergencyObservationId = x.EmergencyObservationId,
                PatientVitalSignId = x.PatientVitalSignId,
                VitalSign = x.PatientVitalSign == null
                    ? null
                    : new EmergencyObservationDetailVitalSignResponse
                    {
                        Id = x.PatientVitalSign.Id,
                        ObservationDateTime = x.PatientVitalSign.ObservationDateTime,
                        BloodPressureSystolic = x.PatientVitalSign.BloodPressureSystolic,
                        BloodPressureDiastolic = x.PatientVitalSign.BloodPressureDiastolic,
                        PulseRate = x.PatientVitalSign.PulseRate,
                        RespiratoryRate = x.PatientVitalSign.RespiratoryRate,
                        Temperature = x.PatientVitalSign.Temperature,
                        OxygenSaturation = x.PatientVitalSign.OxygenSaturation,
                        GcsEye = x.PatientVitalSign.GcsEye,
                        GcsVerbal = x.PatientVitalSign.GcsVerbal,
                        GcsMotor = x.PatientVitalSign.GcsMotor,
                        GcsTotal = x.PatientVitalSign.GcsTotal,
                        ConsciousnessStatus = x.PatientVitalSign.ConsciousnessStatus,
                        IsUsingOxygen = x.PatientVitalSign.IsUsingOxygen,
                        OxygenSupportType = x.PatientVitalSign.OxygenSupportType,
                        OxygenFlowRate = x.PatientVitalSign.OxygenFlowRate,
                        OxygenSupportNote = x.PatientVitalSign.OxygenSupportNote,
                        VitalSignStatus = x.PatientVitalSign.VitalSignStatus,
                        IsAbnormal = x.PatientVitalSign.IsAbnormal,
                        IsCritical = x.PatientVitalSign.IsCritical
                    },
                ProgressNoteId = x.ProgressNoteId,
                RecordedAt = x.RecordedAt,
                RecordedByUserId = x.RecordedByUserId,
                // Nama pencatat memakai urutan yang sudah dipakai backend lain:
                // DisplayName, lalu UserName, Email, dan UserCode. Pengguna yang tidak
                // ditemukan menghasilkan null, bukan GUID.
                RecordedByName = x.RecordedByUser == null
                    ? null
                    : x.RecordedByUser.DisplayName ?? x.RecordedByUser.UserName ?? x.RecordedByUser.Email ?? x.RecordedByUser.UserCode,
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

        private Task<EmergencyObservationDetailResponse?> BacaResponseAsync(Guid id, CancellationToken cancellationToken)
            => _dbContext.Set<EmgObservationDetail>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(ProyeksiResponse)
                .FirstOrDefaultAsync(cancellationToken);

        private IActionResult Failure(int statusCode, string message)
            => StatusCode(statusCode, ApiResponse<object>.Fail(statusCode, message));

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
