using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientDiagnosisPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.PatientDiagnosisResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/patient-diagnoses")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Patient Diagnosis",
        AreaName = "HealthServices",
        ControllerName = "PatientDiagnosis",
        Description = "Diagnosis ICD pasien berdasarkan konsultasi dokter",
        SortOrder = 3
    )]
    [Tags("Health Services / Clinical Management / Patient Diagnosis")]
    public class PatientDiagnosisController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        // =====================================================================
        // BE-RWI-068 / INT-DOK-10 - kalimat penolakan yang terkunci kontrak 0.4.0
        // =====================================================================

        /// <summary>
        /// <c>VAL-DOK-38</c>. Kalimat penolakan jalur lama, dikutip <b>apa adanya</b> dari
        /// perilaku sebelum <c>0.4.0</c>.
        /// </summary>
        /// <remarks>
        /// Kunjungan rawat jalan dan medical check-up tidak ikut dilonggarkan, dan buktinya
        /// bukan sekadar kode <c>400</c> melainkan kalimat yang sama persis — cara yang sama
        /// dipakai <c>BE-RWI-043</c> membuktikan <c>RWI-AC-143</c>. Konstanta ini ada supaya
        /// kalimatnya punya satu tempat dan tidak dapat bergeser tanpa terlihat.
        /// </remarks>
        internal const string PenolakanKonsultasiTidakDitemukan =
            "Konsultasi dokter tidak ditemukan atau tidak sesuai encounter.";

        /// <summary><c>VAL-DOK-36</c> — kedua konteks sama-sama kosong.</summary>
        internal const string PenolakanTanpaKonteks =
            "Diagnosis harus melekat pada catatan dokter atau pada perawatan pasien yang " +
            "sedang berjalan.";

        /// <summary>
        /// <c>VAL-DOK-37</c> dan <c>VAL-DOK-40</c> — konteks menunjuk pasien yang berbeda.
        /// </summary>
        /// <remarks>
        /// Keduanya sengaja berbagi satu kalimat, sama seperti <c>VAL-DOK-26</c>. Bagi dokter
        /// yang sedang membuka layar keduanya adalah kesalahan yang sama: ia menulis untuk
        /// pasien yang keliru. Membedakan kalimatnya hanya memberi tahu bagian mana yang tidak
        /// cocok.
        /// </remarks>
        internal const string PenolakanKonteksPasienBerbeda =
            "Diagnosis ini tidak cocok dengan perawatan pasien. Periksa kembali pasien yang " +
            "sedang Anda buka.";

        /// <summary><c>VAL-DOK-05</c> — pengguna tidak terhubung ke dokter mana pun.</summary>
        internal const string PenolakanBukanDokter =
            "Catatan ini hanya dapat ditulis dokter.";

        /// <summary><c>VAL-DOK-39</c>, memakai penjaga yang sama dengan <c>VAL-DOK-06</c>.</summary>
        internal const string PenolakanBukanDpjpPasien =
            "Anda bukan DPJP pasien ini. Hubungi DPJP atau supervisor klinis.";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly InpatientClinicalContextService _inpatientClinicalContextService;

        public PatientDiagnosisController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            InpatientClinicalContextService inpatientClinicalContextService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _inpatientClinicalContextService = inpatientClinicalContextService;
        }

        /// <summary>
        /// Hasil pemeriksaan konteks pembuatan diagnosis: jalur mana yang dipakai, atau kenapa
        /// permintaannya ditolak.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-068</c>. Bentuknya mengikuti <c>CreateGuard</c> pada
        /// <c>PatientAssessmentController</c>: satu nilai membawa kode dan kalimatnya sekaligus,
        /// sehingga pemanggil tidak perlu menerjemahkan sebab penolakan sendiri-sendiri.
        /// </remarks>
        private sealed class DiagnosisContextResolution
        {
            public bool IsValid { get; init; }

            public int StatusCode { get; init; } = StatusCodes.Status400BadRequest;

            public string? ErrorMessage { get; init; }

            /// <summary>Catatan dokter yang menaungi diagnosis. Kosong pada jalur kajian medis.</summary>
            public TrxDoctorConsultation? Consultation { get; init; }

            /// <summary>Konteks perawatan. Kosong pada jalur catatan dokter murni.</summary>
            public InpatientClinicalContext? InpatientContext { get; init; }

            /// <summary>Dokter penulis pada jalur kajian medis.</summary>
            public Guid? DoctorId { get; init; }

            public static DiagnosisContextResolution Ok(
                TrxDoctorConsultation? consultation,
                InpatientClinicalContext? inpatientContext,
                Guid? doctorId) => new()
                {
                    IsValid = true,
                    StatusCode = StatusCodes.Status200OK,
                    Consultation = consultation,
                    InpatientContext = inpatientContext,
                    DoctorId = doctorId
                };

            public static DiagnosisContextResolution Fail(
                string message,
                int statusCode = StatusCodes.Status400BadRequest) => new()
                {
                    IsValid = false,
                    StatusCode = statusCode,
                    ErrorMessage = message
                };
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientDiagnosisFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Diagnosis", Description = "Melihat metadata filter diagnosis pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientDiagnosis", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientDiagnosisFilterMetadataResponse
            {
                DefaultFilter = new PatientDiagnosisDefaultFilterResponse(),
                SortOptions = new List<PatientDiagnosisSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "diagnosisCode", Label = "Kode diagnosis" },
                    new() { Value = "diagnosisName", Label = "Nama diagnosis" },
                    new() { Value = "diagnosisDateTime", Label = "Tanggal diagnosis" },
                    new() { Value = "diagnosisType", Label = "Tipe diagnosis" },
                    new() { Value = "diagnosisStatus", Label = "Status diagnosis" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                DiagnosisTypeOptions = BuildEnumOptions<PatientDiagnosisType>(),
                DiagnosisStatusOptions = BuildEnumOptions<PatientDiagnosisStatus>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientDiagnosis.GetFilterMetadata",
                "Mengambil metadata filter diagnosis pasien.",
                result
            );

            return Ok(ApiResponse<PatientDiagnosisFilterMetadataResponse>.Ok(
                result,
                "Metadata filter diagnosis pasien berhasil diambil."
            ));
        }

        [HttpGet("master-options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientDiagnosisMasterOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Diagnosis", Description = "Melihat pilihan master diagnosis ICD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientDiagnosis", "Read")]
        public async Task<IActionResult> GetMasterDiagnosisOptions(
            [FromQuery] string? search,
            [FromQuery] Guid? diagnosisChapterId,
            [FromQuery] bool onlySelectable = true,
            [FromQuery] bool onlyActive = true,
            [FromQuery] int take = 50)
        {
            if (take <= 0) take = 50;
            if (take > 100) take = 100;

            var query = _dbContext.Set<MstDiagnosis>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (onlySelectable)
                query = query.Where(x => x.IsSelectableForClinicalUse);

            if (diagnosisChapterId.HasValue && diagnosisChapterId.Value != Guid.Empty)
                query = query.Where(x => x.DiagnosisChapterId == diagnosisChapterId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DiagnosisCode.ToLower().Contains(keyword) ||
                    x.DiagnosisName.ToLower().Contains(keyword));
            }

            var data = await query
                .OrderBy(x => x.DiagnosisCode)
                .Take(take)
                .Select(x => new PatientDiagnosisMasterOptionResponse
                {
                    Id = x.Id,
                    DiagnosisCode = x.DiagnosisCode,
                    DiagnosisName = x.DiagnosisName,
                    DiagnosisType = x.DiagnosisType,
                    IcdVersion = x.IcdVersion,
                    IsSelectableForClinicalUse = x.IsSelectableForClinicalUse,
                    IsPrimaryDiagnosisAllowed = x.IsPrimaryDiagnosisAllowed,
                    IsSecondaryDiagnosisAllowed = x.IsSecondaryDiagnosisAllowed
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientDiagnosisMasterOptionResponse>>.Ok(
                data,
                "Data pilihan master diagnosis berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientDiagnosisPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Diagnosis", Description = "Melihat data diagnosis pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientDiagnosis", "Read")]
        public async Task<IActionResult> GetDiagnoses(
            [FromQuery] string? search,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? inpEpisodeId,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? diagnosisId,
            [FromQuery] PatientDiagnosisType? diagnosisType,
            [FromQuery] PatientDiagnosisStatus? diagnosisStatus,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isConfirmed,
            [FromQuery] bool? isFromMasterDiagnosis,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyFilters(
                query,
                search,
                encounterId,
                consultationId,
                inpEpisodeId,
                patientId,
                doctorId,
                serviceUnitId,
                clinicId,
                diagnosisId,
                diagnosisType,
                diagnosisStatus,
                isPrimary,
                isConfirmed,
                isFromMasterDiagnosis,
                startDate,
                endDate
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(ToResponse).ToList();

            return Ok(ApiResponse<ResponsePatientDiagnosisPagedResult>.Ok(
                new ResponsePatientDiagnosisPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data diagnosis pasien berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientDiagnosisOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Diagnosis", Description = "Melihat pilihan diagnosis pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientDiagnosis", "Read")]
        public async Task<IActionResult> GetDiagnosisOptions(
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? inpEpisodeId,
            [FromQuery] Guid? patientId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<TrxPatientDiagnosis>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive && x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled);

            if (consultationId.HasValue && consultationId.Value != Guid.Empty)
                query = query.Where(x => x.ConsultationId == consultationId.Value);

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            // BE-RWI-068. Inilah penyaring yang dipakai daftar masalah pada layar kajian medis.
            if (inpEpisodeId.HasValue && inpEpisodeId.Value != Guid.Empty)
                query = query.Where(x => x.InpEpisodeId == inpEpisodeId.Value);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DiagnosisCode.ToLower().Contains(keyword) ||
                    x.DiagnosisName.ToLower().Contains(keyword));
            }

            var data = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.DiagnosisCode)
                .Take(100)
                .Select(x => new PatientDiagnosisOptionResponse
                {
                    Id = x.Id,
                    DiagnosisId = x.DiagnosisId,
                    DiagnosisCode = x.DiagnosisCode,
                    DiagnosisName = x.DiagnosisName,
                    DiagnosisMasterType = x.DiagnosisMasterType,
                    IcdVersion = x.IcdVersion,
                    DiagnosisType = x.DiagnosisType,
                    DiagnosisStatus = x.DiagnosisStatus,
                    IsPrimary = x.IsPrimary,
                    IsConfirmed = x.IsConfirmed,
                    IsFromMasterDiagnosis = x.IsFromMasterDiagnosis
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientDiagnosisOptionResponse>>.Ok(
                data,
                "Data pilihan diagnosis pasien berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientDiagnosisDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Diagnosis", Description = "Melihat detail diagnosis pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientDiagnosis", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis pasien tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientDiagnosisDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Detail diagnosis pasien berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientDiagnosisCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Diagnosis", Description = "Membuat diagnosis pasien", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientDiagnosis", "Create")]
        public async Task<IActionResult> CreateDiagnosis([FromBody] CreatePatientDiagnosisRequest request)
        {
            // BE-RWI-068 / INT-DOK-10. Konteks diperiksa lebih dulu dan menentukan jalur mana
            // yang dipakai: catatan dokter seperti sebelumnya, atau perawatan rawat inap bagi
            // diagnosis yang lahir dari kajian medis awal.
            var konteks = await ResolveCreateContextAsync(request);

            if (!konteks.IsValid)
            {
                return StatusCode(konteks.StatusCode, ApiResponse<object>.Fail(
                    konteks.StatusCode,
                    konteks.ErrorMessage ?? "Data diagnosis pasien tidak valid."
                ));
            }

            var validation = await ValidateCreateRequestAsync(request, konteks);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data diagnosis pasien tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var consultation = konteks.Consultation;
            var inpatientContext = konteks.InpatientContext;

            var diagnosisSnapshot = await BuildDiagnosisSnapshotAsync(
                request.DiagnosisId,
                request.DiagnosisCode,
                request.DiagnosisName,
                request.DiagnosisMasterType,
                request.IcdVersion,
                request.IsPrimary ? PatientDiagnosisType.Primary : request.DiagnosisType
            );

            if (!diagnosisSnapshot.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    diagnosisSnapshot.ErrorMessage ?? "Diagnosis tidak valid."
                ));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsPrimary)
            {
                await ClearPrimaryDiagnosisAsync(
                    consultation?.Id,
                    inpatientContext?.EpisodeId,
                    actorUserId,
                    now);
            }

            var entity = new TrxPatientDiagnosis
            {
                Id = Guid.NewGuid(),

                // Jalur catatan dokter mengambil seluruh identitasnya dari konsultasi, persis
                // seperti sebelum 0.4.0. Jalur kajian medis mengambilnya dari konteks
                // perawatan, dan dokternya adalah penulis yang sedang masuk.
                EncounterId = consultation?.EncounterId ?? inpatientContext!.EncounterId,
                ConsultationId = consultation?.Id,
                InpEpisodeId = inpatientContext?.EpisodeId,
                PatientId = consultation?.PatientId ?? inpatientContext!.PatientId,
                DoctorId = consultation?.DoctorId ?? konteks.DoctorId!.Value,
                ServiceUnitId = consultation != null
                    ? consultation.ServiceUnitId
                    : inpatientContext!.ServiceUnitId,
                ClinicId = consultation?.ClinicId,
                DiagnosisId = diagnosisSnapshot.DiagnosisId,
                DiagnosisCode = diagnosisSnapshot.DiagnosisCode,
                DiagnosisName = diagnosisSnapshot.DiagnosisName,
                DiagnosisMasterType = diagnosisSnapshot.DiagnosisMasterType,
                IcdVersion = diagnosisSnapshot.IcdVersion,
                DiagnosisType = request.IsPrimary ? PatientDiagnosisType.Primary : request.DiagnosisType,
                DiagnosisStatus = PatientDiagnosisStatus.Active,
                IsPrimary = request.IsPrimary,
                IsChronic = request.IsChronic,
                IsNewCase = request.IsNewCase,
                IsConfirmed = request.IsConfirmed,
                IsFromMasterDiagnosis = diagnosisSnapshot.IsFromMasterDiagnosis,
                DiagnosisDateTime = now,
                OnsetDate = request.OnsetDate,
                ClinicalNote = NormalizeNullableText(request.ClinicalNote),
                AssessmentNote = NormalizeNullableText(request.AssessmentNote),
                PlanNote = NormalizeNullableText(request.PlanNote),
                DifferentialDiagnosisNote = NormalizeNullableText(request.DifferentialDiagnosisNote),
                SupportingFindingNote = NormalizeNullableText(request.SupportingFindingNote),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxPatientDiagnosis>().Add(entity);

            await _dbContext.SaveChangesAsync();

            // Ringkasan diagnosis milik catatan dokter hanya diperbarui ketika catatannya
            // memang ada. Jalur kajian medis menghitung ringkasannya per perawatan dan tidak
            // menyentuh satu baris konsultasi pun - bukti "nol konsultasi bayangan".
            var summary = consultation != null
                ? await UpdateConsultationDiagnosisSummaryAsync(consultation.Id, actorUserId, now)
                : await BuildEpisodeDiagnosisSummaryAsync(inpatientContext!.EpisodeId);

            await transaction.CommitAsync();

            var response = new PatientDiagnosisCreateResponse
            {
                Id = entity.Id,
                EncounterId = entity.EncounterId,
                ConsultationId = entity.ConsultationId,
                InpEpisodeId = entity.InpEpisodeId,
                DiagnosisId = entity.DiagnosisId,
                DiagnosisCode = entity.DiagnosisCode,
                DiagnosisName = entity.DiagnosisName,
                DiagnosisMasterType = entity.DiagnosisMasterType,
                IcdVersion = entity.IcdVersion,
                DiagnosisType = entity.DiagnosisType,
                DiagnosisStatus = entity.DiagnosisStatus,
                IsPrimary = entity.IsPrimary,
                IsConfirmed = entity.IsConfirmed,
                IsFromMasterDiagnosis = entity.IsFromMasterDiagnosis,
                DiagnosisCount = summary.DiagnosisCount,
                HasPrimaryDiagnosis = summary.HasPrimaryDiagnosis,
                DiagnosisText = summary.DiagnosisText,
                PrimaryDiagnosisText = summary.PrimaryDiagnosisText,
                SecondaryDiagnosisText = summary.SecondaryDiagnosisText
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientDiagnosis.CreateDiagnosis",
                "Membuat diagnosis pasien.",
                response
            );

            return Ok(ApiResponse<PatientDiagnosisCreateResponse>.Ok(
                response,
                "Diagnosis pasien berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientDiagnosisUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Diagnosis", Description = "Mengubah diagnosis pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientDiagnosis", "Update")]
        public async Task<IActionResult> UpdateDiagnosis(Guid id, [FromBody] UpdatePatientDiagnosisRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientDiagnosis>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis pasien tidak ditemukan."
                ));
            }

            if (entity.DiagnosisStatus == PatientDiagnosisStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Diagnosis yang sudah cancelled tidak dapat diubah."
                ));
            }

            // BE-RWI-068. Konteks asal diagnosis tersimpan apa adanya dan tidak dapat
            // dipindahkan lewat penyuntingan - permission-audit-matrix.md bagian 4. Penanda
            // yang ikut dikirim hanya diperiksa kecocokannya.
            var inpEpisodeIdPermintaan = NormalizeNullableGuid(request.InpEpisodeId);

            if (inpEpisodeIdPermintaan.HasValue &&
                inpEpisodeIdPermintaan != entity.InpEpisodeId)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    PenolakanKonteksPasienBerbeda
                ));
            }

            var diagnosisSnapshot = await BuildDiagnosisSnapshotAsync(
                request.DiagnosisId,
                request.DiagnosisCode,
                request.DiagnosisName,
                request.DiagnosisMasterType,
                request.IcdVersion,
                request.IsPrimary ? PatientDiagnosisType.Primary : request.DiagnosisType
            );

            if (!diagnosisSnapshot.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    diagnosisSnapshot.ErrorMessage ?? "Diagnosis tidak valid."
                ));
            }

            var duplicateValidation = await ValidateDuplicateDiagnosisAsync(
                entity.ConsultationId,
                entity.InpEpisodeId,
                diagnosisSnapshot.DiagnosisCode,
                diagnosisSnapshot.DiagnosisId,
                id
            );

            if (!duplicateValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    duplicateValidation.ErrorMessage ?? "Diagnosis pasien duplikat."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsPrimary)
            {
                await ClearPrimaryDiagnosisAsync(
                    entity.ConsultationId,
                    entity.InpEpisodeId,
                    actorUserId,
                    now,
                    entity.Id);
            }

            entity.DiagnosisId = diagnosisSnapshot.DiagnosisId;
            entity.DiagnosisCode = diagnosisSnapshot.DiagnosisCode;
            entity.DiagnosisName = diagnosisSnapshot.DiagnosisName;
            entity.DiagnosisMasterType = diagnosisSnapshot.DiagnosisMasterType;
            entity.IcdVersion = diagnosisSnapshot.IcdVersion;
            entity.DiagnosisType = request.IsPrimary ? PatientDiagnosisType.Primary : request.DiagnosisType;
            entity.IsPrimary = request.IsPrimary;
            entity.IsChronic = request.IsChronic;
            entity.IsNewCase = request.IsNewCase;
            entity.IsConfirmed = request.IsConfirmed;
            entity.IsFromMasterDiagnosis = diagnosisSnapshot.IsFromMasterDiagnosis;
            entity.OnsetDate = request.OnsetDate;
            entity.ClinicalNote = NormalizeNullableText(request.ClinicalNote);
            entity.AssessmentNote = NormalizeNullableText(request.AssessmentNote);
            entity.PlanNote = NormalizeNullableText(request.PlanNote);
            entity.DifferentialDiagnosisNote = NormalizeNullableText(request.DifferentialDiagnosisNote);
            entity.SupportingFindingNote = NormalizeNullableText(request.SupportingFindingNote);
            entity.SortOrder = request.SortOrder;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var summary = await RefreshDiagnosisSummaryAsync(entity, actorUserId, now);

            await transaction.CommitAsync();

            var response = new PatientDiagnosisUpdateResponse
            {
                Id = entity.Id,
                EncounterId = entity.EncounterId,
                ConsultationId = entity.ConsultationId,
                InpEpisodeId = entity.InpEpisodeId,
                DiagnosisId = entity.DiagnosisId,
                DiagnosisCode = entity.DiagnosisCode,
                DiagnosisName = entity.DiagnosisName,
                DiagnosisMasterType = entity.DiagnosisMasterType,
                IcdVersion = entity.IcdVersion,
                DiagnosisType = entity.DiagnosisType,
                DiagnosisStatus = entity.DiagnosisStatus,
                IsPrimary = entity.IsPrimary,
                IsConfirmed = entity.IsConfirmed,
                IsFromMasterDiagnosis = entity.IsFromMasterDiagnosis,
                DiagnosisCount = summary.DiagnosisCount,
                HasPrimaryDiagnosis = summary.HasPrimaryDiagnosis,
                DiagnosisText = summary.DiagnosisText,
                PrimaryDiagnosisText = summary.PrimaryDiagnosisText,
                SecondaryDiagnosisText = summary.SecondaryDiagnosisText
            };

            return Ok(ApiResponse<PatientDiagnosisUpdateResponse>.Ok(
                response,
                "Diagnosis pasien berhasil diubah."
            ));
        }

        [HttpPatch("{id:guid}/set-primary")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Set Primary Patient Diagnosis", Description = "Menandai diagnosis utama pasien", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PatientDiagnosis", "Update")]
        public async Task<IActionResult> SetPrimary(Guid id, [FromBody] SetPrimaryPatientDiagnosisRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientDiagnosis>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis pasien tidak ditemukan."
                ));
            }

            if (entity.DiagnosisStatus == PatientDiagnosisStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Diagnosis yang sudah cancelled tidak dapat dijadikan diagnosis utama."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsPrimary)
            {
                await ClearPrimaryDiagnosisAsync(
                    entity.ConsultationId,
                    entity.InpEpisodeId,
                    actorUserId,
                    now,
                    entity.Id);

                entity.IsPrimary = true;
                entity.DiagnosisType = PatientDiagnosisType.Primary;
            }
            else
            {
                entity.IsPrimary = false;

                if (entity.DiagnosisType == PatientDiagnosisType.Primary)
                    entity.DiagnosisType = PatientDiagnosisType.Secondary;
            }

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await RefreshDiagnosisSummaryAsync(entity, actorUserId, now);

            await transaction.CommitAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsPrimary
                    ? "Diagnosis berhasil ditandai sebagai diagnosis utama."
                    : "Diagnosis utama berhasil dilepas."
            ));
        }

        [HttpPatch("{id:guid}/resolve")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Resolve Patient Diagnosis", Description = "Menandai diagnosis pasien sudah resolved", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PatientDiagnosis", "Update")]
        public async Task<IActionResult> ResolveDiagnosis(Guid id, [FromBody] ResolvePatientDiagnosisRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientDiagnosis>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis pasien tidak ditemukan."
                ));
            }

            if (entity.DiagnosisStatus == PatientDiagnosisStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Diagnosis yang sudah cancelled tidak dapat di-resolve."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.DiagnosisStatus = PatientDiagnosisStatus.Resolved;
            entity.ResolvedAt = now;
            entity.ResolvedByUserId = actorUserId;
            entity.ResolvedReason = request.ResolvedReason.Trim();
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Diagnosis pasien berhasil ditandai resolved."
            ));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Patient Diagnosis", Description = "Membatalkan diagnosis pasien", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PatientDiagnosis", "Update")]
        public async Task<IActionResult> CancelDiagnosis(Guid id, [FromBody] CancelPatientDiagnosisRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientDiagnosis>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Diagnosis pasien tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            entity.DiagnosisStatus = PatientDiagnosisStatus.Cancelled;
            entity.CancelledAt = now;
            entity.CancelledByUserId = actorUserId;
            entity.CancelReason = request.CancelReason.Trim();
            entity.IsActive = false;
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await RefreshDiagnosisSummaryAsync(entity, actorUserId, now);

            await transaction.CommitAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Diagnosis pasien berhasil dibatalkan."
            ));
        }

        private IQueryable<TrxPatientDiagnosis> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPatientDiagnosis>()
                .Include(x => x.Encounter)
                .Include(x => x.Consultation)
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Diagnosis)
                .Include(x => x.ResolvedByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPatientDiagnosis> ApplyFilters(
            IQueryable<TrxPatientDiagnosis> query,
            string? search,
            Guid? encounterId,
            Guid? consultationId,
            Guid? inpEpisodeId,
            Guid? patientId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? diagnosisId,
            PatientDiagnosisType? diagnosisType,
            PatientDiagnosisStatus? diagnosisStatus,
            bool? isPrimary,
            bool? isConfirmed,
            bool? isFromMasterDiagnosis,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DiagnosisCode.ToLower().Contains(keyword) ||
                    x.DiagnosisName.ToLower().Contains(keyword) ||
                    (x.DiagnosisMasterType != null && x.DiagnosisMasterType.ToLower().Contains(keyword)) ||
                    (x.IcdVersion != null && x.IcdVersion.ToLower().Contains(keyword)) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)) ||
                    (x.Consultation != null && x.Consultation.ConsultationNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)));
            }

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (consultationId.HasValue && consultationId.Value != Guid.Empty)
                query = query.Where(x => x.ConsultationId == consultationId.Value);

            // BE-RWI-068. Daftar masalah kajian medis dibaca per perawatan - CAP-022 aturan 2.
            if (inpEpisodeId.HasValue && inpEpisodeId.Value != Guid.Empty)
                query = query.Where(x => x.InpEpisodeId == inpEpisodeId.Value);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (diagnosisId.HasValue && diagnosisId.Value != Guid.Empty)
                query = query.Where(x => x.DiagnosisId == diagnosisId.Value);

            if (diagnosisType.HasValue)
                query = query.Where(x => x.DiagnosisType == diagnosisType.Value);

            if (diagnosisStatus.HasValue)
                query = query.Where(x => x.DiagnosisStatus == diagnosisStatus.Value);

            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);

            if (isConfirmed.HasValue)
                query = query.Where(x => x.IsConfirmed == isConfirmed.Value);

            if (isFromMasterDiagnosis.HasValue)
                query = query.Where(x => x.IsFromMasterDiagnosis == isFromMasterDiagnosis.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.DiagnosisDateTime >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.DiagnosisDateTime < endDate.Value.Date.AddDays(1));

            return query;
        }

        /// <summary>
        /// Menentukan konteks pembuatan diagnosis, dan menolak permintaan yang konteksnya tidak
        /// sah - <c>VAL-DOK-36</c> s.d. <c>VAL-DOK-40</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-068</c>, <c>INT-DOK-10</c>. Urutan pemeriksaannya menentukan arti, jadi
        /// ditulis eksplisit:
        /// </para>
        /// <list type="number">
        /// <item>
        /// Nomor konsultasi disebut - <b>jalur lama</b>, tidak berubah satu langkah pun. Bila
        /// penanda perawatan ikut dikirim, keduanya wajib menunjuk kunjungan dan pasien yang
        /// sama - <c>VAL-DOK-37</c>.
        /// </item>
        /// <item>
        /// Nomor konsultasi kosong pada kunjungan yang <b>bukan</b> <c>Inpatient</c> - jalur
        /// lama juga, beserta kalimat penolakannya yang sama persis. Rawat jalan, medical
        /// check-up, dan IGD tidak ikut dilonggarkan - <c>VAL-DOK-38</c>,
        /// <c>integration-contract.md</c> bagian 10.2.
        /// </item>
        /// <item>
        /// Nomor konsultasi kosong pada kunjungan <c>Inpatient</c> - <b>jalur kajian medis</b>.
        /// Penanda perawatan wajib disebut; keduanya kosong ditolak <c>VAL-DOK-36</c>.
        /// </item>
        /// </list>
        /// <para>
        /// <b>Jalur ketiga tidak pernah membuat baris konsultasi.</b> Membuatkan konsultasi
        /// bayangan demi mengisi kolom adalah cara termudah membuat jalur ini terlihat
        /// berhasil, dan justru itulah yang dilarang -
        /// <c>testing/acceptance-test-matrix.md</c> bagian 11.
        /// </para>
        /// </remarks>
        private async Task<DiagnosisContextResolution> ResolveCreateContextAsync(
            CreatePatientDiagnosisRequest request)
        {
            var consultationId = NormalizeNullableGuid(request.ConsultationId);
            var inpEpisodeId = NormalizeNullableGuid(request.InpEpisodeId);

            if (consultationId.HasValue)
            {
                var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                    .FirstOrDefaultAsync(x =>
                        x.Id == consultationId.Value &&
                        x.EncounterId == request.EncounterId &&
                        !x.IsDelete);

                if (consultation == null)
                    return DiagnosisContextResolution.Fail(PenolakanKonsultasiTidakDitemukan);

                if (!inpEpisodeId.HasValue)
                    return DiagnosisContextResolution.Ok(consultation, null, null);

                // VAL-DOK-37. Keduanya terisi, sehingga keduanya wajib menunjuk kunjungan dan
                // pasien yang sama. Penanda perawatan milik kunjungan lain menempelkan
                // diagnosis pada pasien yang keliru.
                var konteksBersama = await _inpatientClinicalContextService.ResolveAsync(
                    consultation.EncounterId,
                    expectedPatientId: consultation.PatientId,
                    expectedEpisodeId: inpEpisodeId,
                    forNewDocument: true);

                if (!konteksBersama.IsResolved)
                {
                    return konteksBersama.StatusCode == StatusCodes.Status400BadRequest
                        ? DiagnosisContextResolution.Fail(PenolakanKonteksPasienBerbeda)
                        : DiagnosisContextResolution.Fail(
                            konteksBersama.ErrorMessage ?? PenolakanKonteksPasienBerbeda,
                            konteksBersama.StatusCode);
                }

                return DiagnosisContextResolution.Ok(consultation, konteksBersama.Context, null);
            }

            // VAL-DOK-38. Jenis kunjungan menentukan apakah pelonggaran berlaku sama sekali.
            // Kunjungan yang tidak ditemukan pun jatuh ke sini, dan jawabannya tetap kalimat
            // lama - persis seperti sebelum 0.4.0.
            var jenisKunjungan = await _dbContext.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .Where(x => x.Id == request.EncounterId && !x.IsDelete)
                .Select(x => x.EncounterType)
                .FirstOrDefaultAsync();

            if (jenisKunjungan != EncounterType.Inpatient)
                return DiagnosisContextResolution.Fail(PenolakanKonsultasiTidakDitemukan);

            // VAL-DOK-36. Basis data tidak lagi menolak diagnosis tanpa konteks, sehingga
            // penjagaannya sepenuhnya di sini.
            if (!inpEpisodeId.HasValue)
                return DiagnosisContextResolution.Fail(PenolakanTanpaKonteks);

            var konteks = await _inpatientClinicalContextService.ResolveAsync(
                request.EncounterId,
                expectedEpisodeId: inpEpisodeId,
                forNewDocument: true);

            if (!konteks.IsResolved)
            {
                // VAL-DOK-40. Perawatan milik pasien lain terbaca sebagai penanda yang tidak
                // cocok dengan kunjungannya, dan kalimatnya dipakai bersama VAL-DOK-37.
                return konteks.StatusCode == StatusCodes.Status400BadRequest
                    ? DiagnosisContextResolution.Fail(PenolakanKonteksPasienBerbeda)
                    : DiagnosisContextResolution.Fail(
                        konteks.ErrorMessage ?? "Konteks perawatan rawat inap tidak dapat dibentuk.",
                        konteks.StatusCode);
            }

            // VAL-DOK-05 lalu VAL-DOK-39. Kewenangan menulis diagnosis dari kajian medis
            // mengikuti kewenangan menulis kajian medis pasien itu: pengguna wajib terhubung ke
            // satu baris dokter aktif, dan dokter itu wajib memegang penugasan yang berlaku
            // pada perawatan tersebut. Mesin hak akses tidak dapat menjaga ini - ia tahu peran
            // dan tidak tahu pasien.
            var doctorId = await ResolveCurrentDoctorIdAsync();

            if (!doctorId.HasValue)
            {
                return DiagnosisContextResolution.Fail(
                    PenolakanBukanDokter,
                    StatusCodes.Status403Forbidden);
            }

            var berwenang = await _inpatientClinicalContextService.IsDoctorAssignedAsync(
                konteks.Context!.EpisodeId,
                doctorId.Value,
                DateTime.UtcNow);

            if (!berwenang)
            {
                return DiagnosisContextResolution.Fail(
                    PenolakanBukanDpjpPasien,
                    StatusCodes.Status403Forbidden);
            }

            return DiagnosisContextResolution.Ok(null, konteks.Context, doctorId);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(
            CreatePatientDiagnosisRequest request,
            DiagnosisContextResolution konteks)
        {
            var consultation = konteks.Consultation;

            if (consultation != null &&
                consultation.ConsultationStatus == DoctorConsultationStatus.Completed)
                return (false, "Konsultasi yang sudah completed tidak dapat ditambahkan diagnosis.");

            var snapshot = await BuildDiagnosisSnapshotAsync(
                request.DiagnosisId,
                request.DiagnosisCode,
                request.DiagnosisName,
                request.DiagnosisMasterType,
                request.IcdVersion,
                request.IsPrimary ? PatientDiagnosisType.Primary : request.DiagnosisType
            );

            if (!snapshot.IsValid)
                return (false, snapshot.ErrorMessage);

            var duplicateValidation = await ValidateDuplicateDiagnosisAsync(
                consultation?.Id,
                konteks.InpatientContext?.EpisodeId,
                snapshot.DiagnosisCode,
                snapshot.DiagnosisId,
                excludeId: null
            );

            if (!duplicateValidation.IsValid)
                return duplicateValidation;

            return (true, null);
        }

        /// <summary>
        /// Menolak diagnosis berkode sama di dalam satu konteks yang sama.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-068</c>. Sebelum <c>0.4.0</c> konteksnya selalu catatan dokter. Sejak
        /// diagnosis dapat lahir dari kajian medis, cakupan pemeriksaannya mengikuti konteks
        /// tempat diagnosis itu tinggal: catatan dokter bila ada, perawatan bila tidak.
        /// Memeriksa jalur kajian medis dengan cakupan konsultasi akan meloloskan kode ganda,
        /// sebab kolom konsultasinya sama-sama kosong.
        /// </remarks>
        private async Task<(bool IsValid, string? ErrorMessage)> ValidateDuplicateDiagnosisAsync(
            Guid? consultationId,
            Guid? inpEpisodeId,
            string diagnosisCode,
            Guid? diagnosisId,
            Guid? excludeId)
        {
            var normalizedCode = diagnosisCode.Trim().ToUpperInvariant();

            var lingkup = BuildContextScopedQuery(consultationId, inpEpisodeId);

            if (lingkup == null)
                return (true, null);

            var penanda = consultationId.HasValue ? "konsultasi" : "perawatan";

            var duplicateByCode = await lingkup
                .AnyAsync(x =>
                    x.DiagnosisCode.ToUpper() == normalizedCode &&
                    !x.IsDelete &&
                    x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateByCode)
                return (false, $"Diagnosis dengan kode yang sama sudah ada pada {penanda} ini.");

            if (diagnosisId.HasValue)
            {
                var duplicateByMaster = await lingkup
                    .AnyAsync(x =>
                        x.DiagnosisId == diagnosisId.Value &&
                        !x.IsDelete &&
                        x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateByMaster)
                    return (false, $"Diagnosis master yang sama sudah ada pada {penanda} ini.");
            }

            return (true, null);
        }

        /// <summary>
        /// Cakupan baris yang berbagi konteks dengan sebuah diagnosis: catatan dokter bila ada,
        /// perawatan rawat inap bila tidak. Kosong bila diagnosis itu tidak punya keduanya.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-068</c>. Baris tanpa konteks tidak dapat dibuat lagi lewat
        /// <c>VAL-DOK-36</c>, tetapi cakupan ini tetap menolak menebak: mengembalikan kosong
        /// membuat pemanggilnya berhenti dengan aman alih-alih memindai seluruh tabel.
        /// </remarks>
        private IQueryable<TrxPatientDiagnosis>? BuildContextScopedQuery(
            Guid? consultationId,
            Guid? inpEpisodeId)
        {
            if (consultationId.HasValue)
            {
                return _dbContext.Set<TrxPatientDiagnosis>()
                    .Where(x => x.ConsultationId == consultationId.Value);
            }

            if (inpEpisodeId.HasValue)
            {
                return _dbContext.Set<TrxPatientDiagnosis>()
                    .Where(x => x.InpEpisodeId == inpEpisodeId.Value);
            }

            return null;
        }

        private async Task<DiagnosisSnapshotResult> BuildDiagnosisSnapshotAsync(
            Guid? diagnosisId,
            string? diagnosisCode,
            string? diagnosisName,
            string? diagnosisMasterType,
            string? icdVersion,
            PatientDiagnosisType requestedDiagnosisType)
        {
            var normalizedDiagnosisId = NormalizeNullableGuid(diagnosisId);

            if (normalizedDiagnosisId.HasValue)
            {
                var master = await _dbContext.Set<MstDiagnosis>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == normalizedDiagnosisId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (master == null)
                {
                    return DiagnosisSnapshotResult.Fail("Master diagnosis tidak ditemukan atau tidak aktif.");
                }

                if (!master.IsSelectableForClinicalUse)
                {
                    return DiagnosisSnapshotResult.Fail("Master diagnosis ini tidak dapat dipilih untuk penggunaan klinis.");
                }

                if (requestedDiagnosisType == PatientDiagnosisType.Primary && !master.IsPrimaryDiagnosisAllowed)
                {
                    return DiagnosisSnapshotResult.Fail("Diagnosis ini tidak diizinkan sebagai diagnosis utama.");
                }

                if (requestedDiagnosisType != PatientDiagnosisType.Primary && !master.IsSecondaryDiagnosisAllowed)
                {
                    return DiagnosisSnapshotResult.Fail("Diagnosis ini tidak diizinkan sebagai diagnosis sekunder.");
                }

                return DiagnosisSnapshotResult.Ok(
                    diagnosisId: master.Id,
                    diagnosisCode: master.DiagnosisCode.Trim().ToUpperInvariant(),
                    diagnosisName: master.DiagnosisName.Trim(),
                    diagnosisMasterType: master.DiagnosisType,
                    icdVersion: master.IcdVersion,
                    isFromMasterDiagnosis: true
                );
            }

            if (string.IsNullOrWhiteSpace(diagnosisCode))
                return DiagnosisSnapshotResult.Fail("Kode diagnosis wajib diisi jika tidak memilih master diagnosis.");

            if (string.IsNullOrWhiteSpace(diagnosisName))
                return DiagnosisSnapshotResult.Fail("Nama diagnosis wajib diisi jika tidak memilih master diagnosis.");

            return DiagnosisSnapshotResult.Ok(
                diagnosisId: null,
                diagnosisCode: diagnosisCode.Trim().ToUpperInvariant(),
                diagnosisName: diagnosisName.Trim(),
                diagnosisMasterType: string.IsNullOrWhiteSpace(diagnosisMasterType)
                    ? "Manual"
                    : diagnosisMasterType.Trim(),
                icdVersion: NormalizeNullableText(icdVersion),
                isFromMasterDiagnosis: false
            );
        }

        /// <summary>
        /// Melepas penanda diagnosis utama dari baris lain di dalam konteks yang sama.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-068</c>. Cakupannya mengikuti konteks diagnosis, sama seperti pemeriksaan
        /// duplikat: satu daftar masalah hanya boleh punya satu diagnosis utama, dan pada jalur
        /// kajian medis daftar itu dipegang perawatan, bukan catatan dokter.
        /// </remarks>
        private async Task ClearPrimaryDiagnosisAsync(
            Guid? consultationId,
            Guid? inpEpisodeId,
            Guid actorUserId,
            DateTime now,
            Guid? exceptDiagnosisId = null)
        {
            var lingkup = BuildContextScopedQuery(consultationId, inpEpisodeId);

            if (lingkup == null)
                return;

            var query = lingkup
                .Where(x =>
                    x.IsPrimary &&
                    !x.IsDelete &&
                    x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled);

            if (exceptDiagnosisId.HasValue)
                query = query.Where(x => x.Id != exceptDiagnosisId.Value);

            var existingPrimaryDiagnoses = await query.ToListAsync();

            foreach (var diagnosis in existingPrimaryDiagnoses)
            {
                diagnosis.IsPrimary = false;

                if (diagnosis.DiagnosisType == PatientDiagnosisType.Primary)
                    diagnosis.DiagnosisType = PatientDiagnosisType.Secondary;

                diagnosis.UpdateDateTime = now;
                diagnosis.UpdateBy = actorUserId;
            }
        }

        private async Task<DiagnosisSummaryResult> UpdateConsultationDiagnosisSummaryAsync(
            Guid consultationId,
            Guid actorUserId,
            DateTime now)
        {
            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .FirstAsync(x => x.Id == consultationId && !x.IsDelete);

            var diagnoses = await _dbContext.Set<TrxPatientDiagnosis>()
                .AsNoTracking()
                .Where(x =>
                    x.ConsultationId == consultationId &&
                    !x.IsDelete &&
                    x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled)
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.DiagnosisDateTime)
                .ToListAsync();

            var primaryDiagnosis = diagnoses.FirstOrDefault(x => x.IsPrimary);
            var secondaryDiagnoses = diagnoses.Where(x => !x.IsPrimary).ToList();

            var diagnosisText = diagnoses.Count == 0
                ? null
                : string.Join("; ", diagnoses.Select(x => $"{x.DiagnosisCode} - {x.DiagnosisName}"));

            var primaryDiagnosisText = primaryDiagnosis == null
                ? null
                : $"{primaryDiagnosis.DiagnosisCode} - {primaryDiagnosis.DiagnosisName}";

            var secondaryDiagnosisText = secondaryDiagnoses.Count == 0
                ? null
                : string.Join("; ", secondaryDiagnoses.Select(x => $"{x.DiagnosisCode} - {x.DiagnosisName}"));

            consultation.DiagnosisText = diagnosisText;
            consultation.PrimaryDiagnosisText = primaryDiagnosisText;
            consultation.SecondaryDiagnosisText = secondaryDiagnosisText;
            consultation.DiagnosisCount = diagnoses.Count;
            consultation.HasPrimaryDiagnosis = primaryDiagnosis != null;
            consultation.UpdateDateTime = now;
            consultation.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return new DiagnosisSummaryResult
            {
                DiagnosisText = diagnosisText,
                PrimaryDiagnosisText = primaryDiagnosisText,
                SecondaryDiagnosisText = secondaryDiagnosisText,
                DiagnosisCount = diagnoses.Count,
                HasPrimaryDiagnosis = primaryDiagnosis != null
            };
        }

        /// <summary>
        /// Menghitung ringkasan daftar masalah satu perawatan, <b>tanpa menulis apa pun</b>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-068</c>. Jalur kajian medis tidak memiliki catatan dokter, sehingga tidak
        /// ada tempat menyimpan ringkasan dan tidak ada yang perlu disimpan. Ringkasan ini
        /// hanya mengisi balasan supaya layar kajian medis menerima bentuk yang sama dengan
        /// jalur lama.
        /// </para>
        /// <para>
        /// <b>Nol baris konsultasi disentuh.</b> Inilah setengah dari bukti "tidak ada
        /// konsultasi bayangan"; setengah lainnya adalah pembuatan entity yang memang tidak
        /// pernah membuat <c>TrxDoctorConsultation</c>.
        /// </para>
        /// </remarks>
        private async Task<DiagnosisSummaryResult> BuildEpisodeDiagnosisSummaryAsync(
            Guid inpEpisodeId)
        {
            var diagnoses = await _dbContext.Set<TrxPatientDiagnosis>()
                .AsNoTracking()
                .Where(x =>
                    x.InpEpisodeId == inpEpisodeId &&
                    !x.IsDelete &&
                    x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled)
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.DiagnosisDateTime)
                .ToListAsync();

            var primaryDiagnosis = diagnoses.FirstOrDefault(x => x.IsPrimary);
            var secondaryDiagnoses = diagnoses.Where(x => !x.IsPrimary).ToList();

            return new DiagnosisSummaryResult
            {
                DiagnosisText = diagnoses.Count == 0
                    ? null
                    : string.Join("; ", diagnoses.Select(x => $"{x.DiagnosisCode} - {x.DiagnosisName}")),
                PrimaryDiagnosisText = primaryDiagnosis == null
                    ? null
                    : $"{primaryDiagnosis.DiagnosisCode} - {primaryDiagnosis.DiagnosisName}",
                SecondaryDiagnosisText = secondaryDiagnoses.Count == 0
                    ? null
                    : string.Join("; ", secondaryDiagnoses.Select(x => $"{x.DiagnosisCode} - {x.DiagnosisName}")),
                DiagnosisCount = diagnoses.Count,
                HasPrimaryDiagnosis = primaryDiagnosis != null
            };
        }

        /// <summary>
        /// Ringkasan bagi sebuah baris diagnosis, menurut konteks tempat ia tinggal.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-068</c>. Dipakai jalur sunting, penetapan diagnosis utama, dan pembatalan
        /// - ketiganya dulu selalu memanggil pembaruan ringkasan konsultasi, yang kini akan
        /// melempar bila barisnya lahir dari kajian medis.
        /// </remarks>
        private async Task<DiagnosisSummaryResult> RefreshDiagnosisSummaryAsync(
            TrxPatientDiagnosis entity,
            Guid actorUserId,
            DateTime now)
        {
            if (entity.ConsultationId.HasValue)
            {
                return await UpdateConsultationDiagnosisSummaryAsync(
                    entity.ConsultationId.Value,
                    actorUserId,
                    now);
            }

            if (entity.InpEpisodeId.HasValue)
                return await BuildEpisodeDiagnosisSummaryAsync(entity.InpEpisodeId.Value);

            return new DiagnosisSummaryResult();
        }

        private static IQueryable<TrxPatientDiagnosis> ApplySorting(
            IQueryable<TrxPatientDiagnosis> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "diagnosiscode" => isDesc ? query.OrderByDescending(x => x.DiagnosisCode) : query.OrderBy(x => x.DiagnosisCode),
                "diagnosisname" => isDesc ? query.OrderByDescending(x => x.DiagnosisName) : query.OrderBy(x => x.DiagnosisName),
                "diagnosisdatetime" => isDesc ? query.OrderByDescending(x => x.DiagnosisDateTime) : query.OrderBy(x => x.DiagnosisDateTime),
                "diagnosistype" => isDesc ? query.OrderByDescending(x => x.DiagnosisType) : query.OrderBy(x => x.DiagnosisType),
                "diagnosisstatus" => isDesc ? query.OrderByDescending(x => x.DiagnosisStatus) : query.OrderBy(x => x.DiagnosisStatus),
                "isprimary" => isDesc ? query.OrderByDescending(x => x.IsPrimary) : query.OrderBy(x => x.IsPrimary),
                "isconfirmed" => isDesc ? query.OrderByDescending(x => x.IsConfirmed) : query.OrderBy(x => x.IsConfirmed),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.DiagnosisDateTime)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.DiagnosisDateTime)
            };
        }

        private static PatientDiagnosisResponse ToResponse(TrxPatientDiagnosis x)
        {
            return new PatientDiagnosisResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                ConsultationId = x.ConsultationId,
                ConsultationNumber = x.Consultation != null ? x.Consultation.ConsultationNumber : string.Empty,
                InpEpisodeId = x.InpEpisodeId,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DiagnosisId = x.DiagnosisId,
                DiagnosisCode = x.DiagnosisCode,
                DiagnosisName = x.DiagnosisName,
                DiagnosisMasterType = x.DiagnosisMasterType,
                IcdVersion = x.IcdVersion,
                DiagnosisType = x.DiagnosisType,
                DiagnosisStatus = x.DiagnosisStatus,
                IsPrimary = x.IsPrimary,
                IsChronic = x.IsChronic,
                IsNewCase = x.IsNewCase,
                IsConfirmed = x.IsConfirmed,
                IsFromMasterDiagnosis = x.IsFromMasterDiagnosis,
                DiagnosisDateTime = x.DiagnosisDateTime,
                OnsetDate = x.OnsetDate,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientDiagnosisDetailResponse ToDetailResponse(TrxPatientDiagnosis x)
        {
            return new PatientDiagnosisDetailResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                ConsultationId = x.ConsultationId,
                ConsultationNumber = x.Consultation != null ? x.Consultation.ConsultationNumber : string.Empty,
                InpEpisodeId = x.InpEpisodeId,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DiagnosisId = x.DiagnosisId,
                DiagnosisCode = x.DiagnosisCode,
                DiagnosisName = x.DiagnosisName,
                DiagnosisMasterType = x.DiagnosisMasterType,
                IcdVersion = x.IcdVersion,
                DiagnosisType = x.DiagnosisType,
                DiagnosisStatus = x.DiagnosisStatus,
                IsPrimary = x.IsPrimary,
                IsChronic = x.IsChronic,
                IsNewCase = x.IsNewCase,
                IsConfirmed = x.IsConfirmed,
                IsFromMasterDiagnosis = x.IsFromMasterDiagnosis,
                DiagnosisDateTime = x.DiagnosisDateTime,
                OnsetDate = x.OnsetDate,
                SortOrder = x.SortOrder,
                ClinicalNote = x.ClinicalNote,
                AssessmentNote = x.AssessmentNote,
                PlanNote = x.PlanNote,
                DifferentialDiagnosisNote = x.DifferentialDiagnosisNote,
                SupportingFindingNote = x.SupportingFindingNote,
                ResolvedAt = x.ResolvedAt,
                ResolvedByUserId = x.ResolvedByUserId,
                ResolvedByUserName = x.ResolvedByUser != null ? x.ResolvedByUser.DisplayName : null,
                ResolvedReason = x.ResolvedReason,
                CancelledAt = x.CancelledAt,
                CancelledByUserId = x.CancelledByUserId,
                CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName : null,
                CancelReason = x.CancelReason,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<PatientDiagnosisEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientDiagnosisEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = SplitPascalCase(x.ToString())
                })
                .ToList();
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
                return null;

            return value.Value;
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var id)
                ? id
                : Guid.Empty;
        }

        /// <summary>
        /// Menemukan baris dokter yang melekat pada pengguna yang sedang masuk.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-068</c>, <c>VAL-DOK-05</c>. Urutannya sama persis dengan
        /// <c>PatientAssessmentController.ResolveCurrentDoctorIdAsync</c>: klaim identitas
        /// dokter lebih dulu, lalu penautan lewat profil tenaga kerja, lalu surel. Ketiganya
        /// bersandar pada <b>data</b> - tidak satu pun membaca nama peran, nama jabatan, maupun
        /// <c>UserType</c>.
        /// </para>
        /// <para>
        /// Mengembalikan kosong bila pengguna tidak terhubung ke dokter mana pun. Itulah yang
        /// menolak perawat menulis diagnosis dari kajian medis, tanpa satu baris pun kode yang
        /// menyebut kata perawat.
        /// </para>
        /// </remarks>
        private async Task<Guid?> ResolveCurrentDoctorIdAsync()
        {
            var doctorIdClaim = User.FindFirstValue("doctor_id") ?? User.FindFirstValue("DoctorId");

            if (Guid.TryParse(doctorIdClaim, out var dariKlaimDokter) && dariKlaimDokter != Guid.Empty)
            {
                var adaDokter = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == dariKlaimDokter && !x.IsDelete && x.IsActive);

                if (adaDokter)
                    return dariKlaimDokter;
            }

            var workforceClaim = User.FindFirstValue("workforce_profile_id")
                                 ?? User.FindFirstValue("WorkforceProfileId");

            Guid? workforceProfileId =
                Guid.TryParse(workforceClaim, out var dariKlaimProfil) && dariKlaimProfil != Guid.Empty
                    ? dariKlaimProfil
                    : null;

            var currentUserId = GetCurrentUserId();

            var pengguna = currentUserId == Guid.Empty
                ? null
                : await _dbContext.Users
                    .AsNoTracking()
                    .Where(x => x.Id == currentUserId)
                    .Select(x => new { x.WorkforceProfileId, x.Email })
                    .FirstOrDefaultAsync();

            workforceProfileId ??= pengguna?.WorkforceProfileId;

            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
            {
                var dokter = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == workforceProfileId.Value &&
                        !x.IsDelete &&
                        x.IsActive)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync();

                if (dokter.HasValue)
                    return dokter;
            }

            if (!string.IsNullOrWhiteSpace(pengguna?.Email))
            {
                var surel = pengguna.Email.ToLower();

                var dokter = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .Where(x =>
                        x.Email != null &&
                        x.Email.ToLower() == surel &&
                        !x.IsDelete &&
                        x.IsActive)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync();

                if (dokter.HasValue)
                    return dokter;
            }

            return null;
        }

        private class DiagnosisSummaryResult
        {
            public string? DiagnosisText { get; set; }
            public string? PrimaryDiagnosisText { get; set; }
            public string? SecondaryDiagnosisText { get; set; }
            public int DiagnosisCount { get; set; }
            public bool HasPrimaryDiagnosis { get; set; }
        }

        private class DiagnosisSnapshotResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public Guid? DiagnosisId { get; set; }
            public string DiagnosisCode { get; set; } = string.Empty;
            public string DiagnosisName { get; set; } = string.Empty;
            public string DiagnosisMasterType { get; set; } = string.Empty;
            public string? IcdVersion { get; set; }
            public bool IsFromMasterDiagnosis { get; set; }
            public static DiagnosisSnapshotResult Ok(
                Guid? diagnosisId,
                string diagnosisCode,
                string diagnosisName,
                string diagnosisMasterType,
                string? icdVersion,
                bool isFromMasterDiagnosis)
            {
                return new DiagnosisSnapshotResult
                {
                    IsValid = true,
                    DiagnosisId = diagnosisId,
                    DiagnosisCode = diagnosisCode,
                    DiagnosisName = diagnosisName,
                    DiagnosisMasterType = diagnosisMasterType,
                    IcdVersion = icdVersion,
                    IsFromMasterDiagnosis = isFromMasterDiagnosis
                };
            }

            public static DiagnosisSnapshotResult Fail(string errorMessage)
            {
                return new DiagnosisSnapshotResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}