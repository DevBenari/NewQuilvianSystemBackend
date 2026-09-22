using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientAssessmentPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.PatientAssessmentResponse>;

using ResponseInitialAssessmentCompliancePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.InitialAssessmentComplianceItemResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/patient-assessments")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Patient Assessment",
        AreaName = "HealthServices",
        ControllerName = "PatientAssessment",
        Description = "Screening awal pasien oleh perawat",
        SortOrder = 1
    )]
    [Tags("Health Services / Clinical Management / Patient Assessment")]
    public class PatientAssessmentController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        /// <summary>
        /// Kalimat penolakan pembuatan pengkajian tanpa antrean bagi kunjungan yang bukan rawat
        /// inap maupun IGD.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-044</c>. Sejalan dengan <c>VAL-DOK-04</c> pada catatan dokter. Kalimat
        /// sebelumnya berbunyi "hanya untuk pasien IGD" dan berhenti benar begitu pintu rawat
        /// inap dibuka; kode penolakannya tetap <c>400</c> dan jalur poliklinik tetap tertutup.
        /// </remarks>
        private const string PenolakanTanpaAntreanBukanRawatInap =
            "Pengkajian untuk pasien poliklinik tetap harus lewat antrean.";

        /// <summary>Kalimat penolakan <c>VAL-DOK-05</c>, apa adanya dari validation matrix.</summary>
        private const string PenolakanKajianMedisBukanDokter =
            "Catatan ini hanya dapat ditulis dokter.";

        /// <summary>
        /// Kalimat penolakan <c>VAL-KEP-01</c>, apa adanya dari validation matrix keperawatan.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-054</c>. Dipakai hanya untuk kunjungan yang memang bertipe rawat inap
        /// tetapi belum punya perawatan. Kunjungan poliklinik tetap memakai
        /// <see cref="PenolakanTanpaAntreanBukanRawatInap"/> beserta kode <c>400</c>-nya.
        /// </remarks>
        private const string PenolakanTanpaPerawatanRawatInap =
            "Pasien ini tidak sedang dirawat inap. Pengkajian rawat inap hanya untuk pasien " +
            "yang sudah masuk kamar.";

        /// <summary>
        /// Kalimat penolakan <c>VAL-KEP-11</c>, apa adanya dari validation matrix keperawatan.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-056</c>. Ditulis untuk perawat, bukan untuk programmer: ia menyebut apa yang
        /// harus dilakukan berikutnya, bukan aturan teknis yang dilanggar.
        /// </remarks>
        private const string PenolakanPengkajianAwalKedua =
            "Pengkajian awal untuk pasien ini sudah ada. Gunakan pengkajian ulang.";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly InpatientClinicalContextService _inpatientClinicalContextService;
        private readonly ClinicalDocumentIntegrityService _integrityService;

        /// <summary>
        /// Pemilik kebijakan batas waktu pengkajian - <c>BE-RWI-055</c>. Dipakai saat pengkajian
        /// lahir untuk menetapkan tenggatnya beserta kebijakan yang dipakai menghitungnya.
        /// </summary>
        private readonly ClinicalAssessmentPolicyService _assessmentPolicyService;

        /// <summary>
        /// Mesin koreksi dokumen klinis milik <c>MedicalRecordManagement</c> - <c>BE-RWI-057</c>.
        /// Endpoint koreksi pada grup ini <b>meneruskan</b> ke sini; tidak satu baris koreksi pun
        /// disimpan pada tabel milik <c>ClinicalManagement</c> (<c>RWI-DEC-087</c>).
        /// </summary>
        private readonly ClinicalNoteAddendumService _addendumService;

        /// <summary>
        /// Pemilik pembacaan lini masa, keadaan tenggat, dan daftar pantau kepatuhan -
        /// <c>BE-RWI-058</c>, <c>BE-RWI-064</c>.
        /// </summary>
        private readonly NursingAssessmentMonitoringService _monitoringService;

        /// <summary>
        /// Penemu pegawai di balik pengguna yang sedang masuk - <c>BE-RWI-078</c>,
        /// <c>GUARD-INP-07</c>. Dipakai hanya oleh jalur pengkajian keperawatan rawat inap.
        /// </summary>
        private readonly NursingActorService _nursingActorService;

        /// <summary>Dokumen Pengkajian Pasien V2 rawat inap — <c>BE-RWI-109</c> s.d. <c>BE-RWI-111</c>.</summary>
        private readonly NursingAssessmentDocumentService _documentService;

        /// <summary>Progres Pengkajian Pasien lima bagian — <c>BE-RWI-112</c>.</summary>
        private readonly NursingAssessmentProgressService _progressService;

        /// <summary>Resolusi instrumen berversi bagi formulir — <c>INT-KEP-16</c>.</summary>
        private readonly ClinicalInstrumentService _instrumentService;

        public PatientAssessmentController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            InpatientClinicalContextService inpatientClinicalContextService,
            ClinicalDocumentIntegrityService integrityService,
            ClinicalAssessmentPolicyService assessmentPolicyService,
            ClinicalNoteAddendumService addendumService,
            NursingAssessmentMonitoringService monitoringService,
            NursingActorService nursingActorService,
            NursingAssessmentDocumentService documentService,
            NursingAssessmentProgressService progressService,
            ClinicalInstrumentService instrumentService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _inpatientClinicalContextService = inpatientClinicalContextService;
            _integrityService = integrityService;
            _assessmentPolicyService = assessmentPolicyService;
            _addendumService = addendumService;
            _monitoringService = monitoringService;
            _nursingActorService = nursingActorService;
            _documentService = documentService;
            _progressService = progressService;
            _instrumentService = instrumentService;
        }

        /// <summary>
        /// Hasil penjagaan pembuatan pengkajian, beserta kode HTTP yang menyertainya.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-044</c>. Bentuknya sama dengan penjagaan pada catatan dokter, dan dengan
        /// alasan yang sama: konteks rawat inap menambahkan sebab penolakan yang menurut
        /// <c>api-contract.md</c> bagian 1.2 dijawab <c>422</c>, sedangkan kewenangan menulis
        /// kajian medis dijawab <c>403</c>. Menyamakan seluruhnya dengan <c>400</c> membuat
        /// layar tidak dapat membedakan isian yang salah dari kewenangan yang tidak ada.
        /// </remarks>
        private readonly record struct CreateGuard(
            bool IsValid,
            int StatusCode,
            string? ErrorMessage,
            string? ErrorCode = null)
        {
            public static CreateGuard Ok() => new(true, StatusCodes.Status200OK, null);

            public static CreateGuard Fail(
                string message,
                int statusCode = StatusCodes.Status400BadRequest,
                string? errorCode = null) => new(false, statusCode, message, errorCode);

            /// <summary>
            /// Isian <c>errors</c> pada <c>ApiResponse</c>: kode alasan penolakan yang dapat dibaca
            /// mesin — <c>BE-RWI-106</c> kriteria 5.
            /// </summary>
            public object? Errors => ErrorCode == null ? null : new { code = ErrorCode };
        }

        // =====================================================================
        // BE-RWI-106 / RLN3-CAP-23 — kode alasan penolakan 403
        // =====================================================================
        //
        // 401 dan 403 sudah dibedakan AccessPermissionFilter: 401 berarti sesi tidak ada, 403
        // berarti butir hak akses tidak dicentang. Yang belum dapat dibedakan layar adalah
        // DUA sebab 403 yang sama sekali lain penanganannya: hak akses yang tidak diberikan admin
        // (arahkan ke admin), dan kewenangan yang melekat pada data — perawat tidak ditempatkan
        // di unit pasien, akun tanpa pegawai, atau bukan penulis konsep (arahkan ke kepala
        // ruangan atau kepegawaian). Kode di bawah dikirim pada isian errors.code.

        /// <summary>Akun belum tertaut ke data pegawai.</summary>
        private const string KodeTanpaPegawai = "NURSE_NOT_LINKED_TO_EMPLOYEE";

        /// <summary>Perawat tidak ditempatkan di unit tempat perawatan berada.</summary>
        private const string KodeUnitLain = "NURSE_UNIT_NOT_ASSIGNED";

        /// <summary>Pengguna bukan penulis konsep.</summary>
        private const string KodeBukanPenulis = "NOT_DOCUMENT_AUTHOR";

        /// <summary>
        /// Apakah jenis ini kajian medis, yaitu dokumen milik dokter dan bukan pengkajian
        /// keperawatan - <c>BE-RWI-045</c>.
        /// </summary>
        private static bool IsKajianMedis(PatientAssessmentType assessmentType) =>
            assessmentType == PatientAssessmentType.MedicalInitial ||
            assessmentType == PatientAssessmentType.MedicalReassessment;

        /// <summary>
        /// Daftar nilai sah seluruh enum pengkajian beserta labelnya.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-106</c> kriteria 1, <c>RLN3-CAP-18</c>, <c>RLN3-CAP-29</c>. Frontend membaca
        /// pilihan dari sini, bukan menulis angkanya sendiri. Server tetap menolak angka di luar
        /// daftar ini pada jalur buat dan ubah, sehingga salah pemetaan di layar tidak pernah
        /// tersimpan diam-diam dengan arti yang lain.
        /// </para>
        /// <para>
        /// <b>Contoh yang ditutup.</b> Layar lama menganggap status <c>1</c> berarti selesai,
        /// padahal <c>1</c> adalah <c>InProgress</c> dan selesai bernilai <c>2</c>. Layar lama juga
        /// mengirim status fungsional <c>4</c> untuk "Ketergantungan Berat", sedangkan backend hanya
        /// mengenal <c>0</c> s.d. <c>3</c>. Dari daftar ini keduanya terbaca jelas.
        /// </para>
        /// </remarks>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientAssessmentMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat daftar nilai sah isian pengkajian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public IActionResult GetMetadata()
        {
            var result = new PatientAssessmentMetadataResponse
            {
                AssessmentTypes = OpsiEnum<PatientAssessmentType>(LabelJenisPengkajian),
                AssessmentStatuses = OpsiEnum<PatientAssessmentStatus>(x => x switch
                {
                    PatientAssessmentStatus.Draft => "Konsep",
                    PatientAssessmentStatus.InProgress => "Sedang dikerjakan",
                    PatientAssessmentStatus.Completed => "Selesai",
                    PatientAssessmentStatus.Cancelled => "Dibatalkan",
                    _ => x.ToString()
                }),
                ConsciousnessStatuses = OpsiEnum<ConsciousnessStatus>(x => x switch
                {
                    ConsciousnessStatus.Unknown => "Belum dikaji",
                    ConsciousnessStatus.ComposMentis => "Compos Mentis",
                    _ => x.ToString()
                }),
                AppetiteStatuses = OpsiEnum<AppetiteStatus>(x => x switch
                {
                    AppetiteStatus.Unknown => "Belum dikaji",
                    AppetiteStatus.Normal => "Normal",
                    AppetiteStatus.Decreased => "Menurun",
                    AppetiteStatus.Increased => "Meningkat",
                    AppetiteStatus.Poor => "Sangat buruk",
                    _ => x.ToString()
                }),
                FunctionalStatuses = OpsiEnum<FunctionalStatus>(x => x switch
                {
                    FunctionalStatus.Unknown => "Belum dikaji",
                    FunctionalStatus.Independent => "Mandiri",
                    FunctionalStatus.NeedPartialAssistance => "Butuh bantuan sebagian",
                    FunctionalStatus.FullyDependent => "Tergantung penuh",
                    _ => x.ToString()
                }),
                NutritionRiskStatuses = OpsiEnum<NutritionRiskStatus>(LabelRisiko),
                FallRiskStatuses = OpsiEnum<FallRiskStatus>(x => x switch
                {
                    FallRiskStatus.Unknown => "Belum dikaji",
                    FallRiskStatus.NoRisk => "Tidak berisiko",
                    FallRiskStatus.LowRisk => "Risiko rendah",
                    FallRiskStatus.MediumRisk => "Risiko sedang",
                    FallRiskStatus.HighRisk => "Risiko tinggi",
                    _ => x.ToString()
                }),
                OxygenSupportTypes = OpsiEnum<OxygenSupportType>(x => x switch
                {
                    OxygenSupportType.None => "Tanpa oksigen",
                    OxygenSupportType.NasalCannula => "Nasal kanul",
                    OxygenSupportType.SimpleMask => "Simple mask",
                    OxygenSupportType.NonRebreathingMask => "Non-rebreathing mask",
                    OxygenSupportType.VenturiMask => "Venturi mask",
                    OxygenSupportType.Other => "Lainnya",
                    _ => x.ToString()
                }),
                PainAssessmentStates = OpsiEnum<PainAssessmentState>(x => x switch
                {
                    PainAssessmentState.NotAssessed => "Belum dinilai",
                    PainAssessmentState.NoPain => "Tidak nyeri",
                    PainAssessmentState.HasPain => "Nyeri",
                    PainAssessmentState.UnableToAssess => "Tidak dapat dinilai",
                    _ => x.ToString()
                }),
                NotAssessedValue = 0
            };

            return Ok(ApiResponse<PatientAssessmentMetadataResponse>.Ok(
                result,
                "Daftar nilai isian pengkajian berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientAssessmentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat data assessment pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> GetAssessments(
            [FromQuery] string? search,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? queueId,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? doctorId,
            [FromQuery] PatientAssessmentStatus? assessmentStatus,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "assessmentDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.AssessmentNumber.ToLower().Contains(keyword) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)) ||
                    (x.Queue != null && x.Queue.QueueCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.ChiefComplaint != null && x.ChiefComplaint.ToLower().Contains(keyword)));
            }

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (queueId.HasValue && queueId.Value != Guid.Empty)
                query = query.Where(x => x.QueueId == queueId.Value);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            if (assessmentStatus.HasValue)
                query = query.Where(x => x.AssessmentStatus == assessmentStatus.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.AssessmentDateTime >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.AssessmentDateTime < endDate.Value.Date.AddDays(1));

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(ToResponse).ToList();

            var result = new ResponsePatientAssessmentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientAssessmentPagedResult>.Ok(
                result,
                "Data assessment pasien berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat detail assessment pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            var result = entity != null ? ToDetailResponse(entity) : null;

            // BE-RWI-109 / BE-RWI-110 — hasil instrumen dan tanda vital yang ditunjuk, dibaca langsung.
            if (result != null)
                await _documentService.EnrichDetailAsync(result);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Assessment pasien tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientAssessmentDetailResponse>.Ok(
                result,
                "Detail assessment pasien berhasil diambil."
            ));
        }

        [HttpGet("active-by-encounter/{encounterId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat draft/assessment aktif berdasarkan encounter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> GetActiveByEncounter(Guid encounterId, [FromQuery] Guid? queueId = null)
        {
            var query = BuildBaseQuery()
                .AsNoTracking()
                .Where(x =>
                    x.EncounterId == encounterId &&
                    x.IsActive &&
                    x.AssessmentStatus != PatientAssessmentStatus.Cancelled);

            if (queueId.HasValue && queueId.Value != Guid.Empty)
            {
                query = query.Where(x => x.QueueId == queueId.Value);
            }

            var entity = await query
                .OrderByDescending(x => x.AssessmentStatus == PatientAssessmentStatus.InProgress)
                .ThenByDescending(x => x.AssessmentStatus == PatientAssessmentStatus.Draft)
                .ThenByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .ThenByDescending(x => x.AssessmentDateTime)
                .FirstOrDefaultAsync();

            var result = entity != null ? ToDetailResponse(entity) : null;

            // BE-RWI-109 / BE-RWI-110 — hasil instrumen dan tanda vital yang ditunjuk, dibaca langsung.
            if (result != null)
                await _documentService.EnrichDetailAsync(result);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Draft/assessment aktif untuk encounter ini tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientAssessmentDetailResponse>.Ok(
                result,
                "Draft/assessment aktif berhasil diambil."
            ));
        }

        /// <summary>
        /// Kajian satu perawatan rawat inap, dapat disaring jenisnya.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-045</c>, <c>api-contract.md</c> bagian 2. Ruang kerja dokter membuka pasien
        /// dari konteks perawatan, bukan dari kunjungan maupun antrean; tanpa endpoint ini
        /// kajian medis satu perawatan hanya dapat ditemukan dengan menebak kunjungannya lebih
        /// dulu.
        /// </para>
        /// <para>
        /// Penyaring jenis membuat satu tabel yang dipakai dua profesi tetap terbaca terpisah:
        /// layar dokter meminta kajian medis, layar perawat meminta pengkajian keperawatan, dan
        /// keduanya tidak pernah saling menimpa di layar - <c>AC-CAP022-02</c>.
        /// </para>
        /// </remarks>
        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientAssessmentPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat kajian satu perawatan rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> GetByEpisode(
            Guid episodeId,
            [FromQuery] PatientAssessmentType? assessmentType = null,
            [FromQuery] PatientAssessmentStatus? assessmentStatus = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (episodeId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Perawatan rawat inap wajib disebut."
                ));
            }

            var episodeExists = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (!episodeExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Perawatan rawat inap tidak ditemukan."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery()
                .AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId);

            if (assessmentType.HasValue)
                query = query.Where(x => x.AssessmentType == assessmentType.Value);

            // BE-RWI-106 / RLN3-CAP-17, api-contract 0.5.0 bagian 7.1. Daftar tetap berpaginasi —
            // paling banyak 100 baris sekali baca — dan kini dapat disaring statusnya, sehingga
            // layar yang mencari konsep berjalan tidak perlu menarik seluruh riwayat perawatan.
            if (assessmentStatus.HasValue)
                query = query.Where(x => x.AssessmentStatus == assessmentStatus.Value);

            var totalData = await query.CountAsync(cancellationToken);

            var entities = await query
                .OrderByDescending(x => x.AssessmentDateTime)
                .ThenByDescending(x => x.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new ResponsePatientAssessmentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<ResponsePatientAssessmentPagedResult>.Ok(
                result,
                "Kajian pada perawatan rawat inap berhasil diambil."
            ));
        }

        [HttpGet("active-by-queue/{queueId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat draft/assessment aktif berdasarkan antrean", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> GetActiveByQueue(Guid queueId)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .Where(x =>
                    x.QueueId == queueId &&
                    x.IsActive &&
                    x.AssessmentStatus != PatientAssessmentStatus.Cancelled)
                .OrderByDescending(x => x.AssessmentStatus == PatientAssessmentStatus.InProgress)
                .ThenByDescending(x => x.AssessmentStatus == PatientAssessmentStatus.Draft)
                .ThenByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .ThenByDescending(x => x.AssessmentDateTime)
                .FirstOrDefaultAsync();

            var result = entity != null ? ToDetailResponse(entity) : null;

            // BE-RWI-109 / BE-RWI-110 — hasil instrumen dan tanda vital yang ditunjuk, dibaca langsung.
            if (result != null)
                await _documentService.EnrichDetailAsync(result);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Draft/assessment aktif untuk antrean ini tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientAssessmentDetailResponse>.Ok(
                result,
                "Draft/assessment aktif berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientAssessmentCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("Create", "Create Patient Assessment", Description = "Membuat assessment pasien", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientAssessment", "Create")]
        public async Task<IActionResult> CreateAssessment([FromBody] CreatePatientAssessmentRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                var pesan = validation.ErrorMessage ?? "Data assessment pasien tidak valid.";

                // Penolakan 400 tetap lewat BadRequest, apa adanya seperti sebelum BE-RWI-044.
                // Hanya 403 kewenangan kajian medis dan 422 konteks perawatan yang memerlukan
                // jalur StatusCode.
                if (validation.StatusCode == StatusCodes.Status400BadRequest)
                    return BadRequest(ApiResponse<object>.Fail(validation.StatusCode, pesan));

                return StatusCode(validation.StatusCode, ApiResponse<object>.Fail(
                    validation.StatusCode,
                    pesan,
                    validation.Errors
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            // Dua asal data yang setara. Pasien poli membawa baris antrean; pasien IGD tidak
            // pernah punya satu pun, sehingga identitas klinisnya diambil dari encounter.
            // BE-IGD-027, FR-IGD-061.
            var queue = request.QueueId.HasValue
                ? await _dbContext.Set<TrxQueue>()
                    .Include(x => x.Encounter)
                    .FirstAsync(x =>
                        x.Id == request.QueueId.Value &&
                        x.EncounterId == request.EncounterId &&
                        !x.IsDelete)
                : null;

            var encounter = queue?.Encounter ?? await _dbContext.Set<RegPatientEncounter>()
                .FirstAsync(x => x.Id == request.EncounterId && !x.IsDelete);

            // BE-RWI-044. Konteks perawatan distempel saat dokumen lahir, sehingga pertanyaan
            // "pengkajian ini milik perawatan yang mana" terjawab tanpa penelusuran berlapis -
            // INV-DOK-01. Kosong bagi kunjungan yang memang bukan rawat inap.
            var inpEpisodeId = await _inpatientClinicalContextService
                .FindOpenEpisodeIdAsync(encounter.Id);

            // BE-RWI-056. Perhitungan dijalankan SESUDAH perawatan diketahui, karena hanya
            // pengkajian rawat inap yang membedakan "risiko jatuh belum diisi" dari "tidak
            // berisiko". Urutan dua baris ini karena itu menentukan, bukan sekadar selera.
            // BE-RWI-109 s.d. BE-RWI-111. Dokumen keperawatan rawat inap V2: keadaan nyeri menentukan
            // HasPain, dan dua centang risiko jatuh lama diabaikan (api-contract 0.5.0 bagian 7.0).
            var dokumenV2 = !IsKajianMedis(request.AssessmentType) &&
                            NursingAssessmentDocumentService.IsInpatientV2(inpEpisodeId, request.AssessmentType);

            if (dokumenV2)
            {
                request.HasPain = request.PainAssessmentState == PainAssessmentState.HasPain;
                request.HasFallRisk = false;
                request.HasAtaxia = false;
                request.HasPosturalInstability = false;
            }

            var calculated = CalculateAssessmentValues(request, inpEpisodeId.HasValue);

            // BE-RWI-045. Kajian medis dituliskan atas nama dokter yang benar-benar terhubung
            // ke pengguna yang masuk, bukan atas nama dokter yang kebetulan tertulis pada
            // antrean atau kunjungan - VAL-DOK-05.
            var kajianMedisDoctorId = IsKajianMedis(request.AssessmentType)
                ? await ResolveCurrentDoctorIdAsync()
                : null;

            // BE-RWI-055 / FR-KEP-010. Tenggat distempel saat pengkajian lahir, memakai
            // kebijakan yang berlaku pada saat itu. Dua kolom, satu makna: DueAt menjawab
            // "kapan seharusnya selesai", PolicyId menjawab "menurut aturan yang mana".
            //
            // Master kosong berarti kedua kolom kosong, dan pengkajian tetap tersimpan penuh -
            // VAL-KEP-17. Tidak ada satu pun jalur yang tertahan karena kebijakan belum diisi.
            var serviceUnitIdPengkajian = queue?.ServiceUnitId ?? encounter.ServiceUnitId;

            var jenisPelayanan = await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .Where(x => x.Id == serviceUnitIdPengkajian)
                .Select(x => (QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums.ServiceUnitType?)x.ServiceUnitType)
                .FirstOrDefaultAsync();

            var tenggat = await _assessmentPolicyService.CalculateDueAsync(
                request.AssessmentType,
                jenisPelayanan,
                now);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var entity = new TrxPatientAssessment
            {
                Id = Guid.NewGuid(),
                AssessmentNumber = await GenerateAssessmentNumberAsync(now),
                EncounterId = encounter.Id,
                QueueId = queue?.Id,
                PatientId = queue?.PatientId ?? encounter.PatientId,
                ServiceUnitId = queue?.ServiceUnitId ?? encounter.ServiceUnitId,
                ClinicId = queue?.ClinicId ?? encounter.ClinicId,
                DoctorId = kajianMedisDoctorId ?? queue?.DoctorId ?? encounter.DoctorId,
                InpEpisodeId = inpEpisodeId,
                AssessmentType = request.AssessmentType,
                DueAt = tenggat.DueAt,
                PolicyId = tenggat.PolicyId,
                AssessmentDateTime = now,
                AssessmentStatus = request.CompleteImmediately
                    ? PatientAssessmentStatus.Completed
                    : PatientAssessmentStatus.InProgress,
                AssessmentByUserId = actorUserId,

                ChiefComplaint = NormalizeNullableText(request.ChiefComplaint),
                CurrentIllnessHistory = NormalizeNullableText(request.CurrentIllnessHistory),
                MedicationHistory = NormalizeNullableText(request.MedicationHistory),
                PhysicalExamination = NormalizeNullableText(request.PhysicalExamination),
                WorkingDiagnosis = NormalizeNullableText(request.WorkingDiagnosis),
                TherapyPlan = NormalizeNullableText(request.TherapyPlan),

                BloodPressureSystolic = request.BloodPressureSystolic,
                BloodPressureDiastolic = request.BloodPressureDiastolic,
                PulseRate = request.PulseRate,
                IsPulseReadable = request.IsPulseReadable,
                RespiratoryRate = request.RespiratoryRate,
                Temperature = request.Temperature,
                OxygenSaturation = request.OxygenSaturation,
                IsUsingOxygen = request.IsUsingOxygen,
                OxygenSupportType = request.OxygenSupportType,
                OxygenFlowRate = request.OxygenFlowRate,
                OxygenSupportNote = NormalizeNullableText(request.OxygenSupportNote),
                ConsciousnessStatus = request.ConsciousnessStatus,
                Weight = request.Weight,
                Height = request.Height,
                BMI = calculated.BMI,
                MeanArterialPressure = calculated.MeanArterialPressure,
                MapStatus = calculated.MapStatus,
                EarlyWarningScore = calculated.EarlyWarningScore,
                EwsRiskLevel = calculated.EwsRiskLevel,
                EwsMonitoringRecommendation = calculated.EwsMonitoringRecommendation,

                HasPain = request.HasPain,
                PainScale = request.PainScale,
                PainTrigger = NormalizeNullableText(request.PainTrigger),
                PainQuality = NormalizeNullableText(request.PainQuality),
                PainLocation = NormalizeNullableText(request.PainLocation),
                PainFrequency = NormalizeNullableText(request.PainFrequency),
                PainManagement = NormalizeNullableText(request.PainManagement),
                PainNote = NormalizeNullableText(request.PainNote),

                HasHereditaryDisease = request.HasHereditaryDisease,
                HereditaryDiseaseNote = NormalizeNullableText(request.HereditaryDiseaseNote),

                HasAllergy = request.HasAllergy,
                AllergyType = NormalizeNullableText(request.AllergyType),
                AllergyNote = NormalizeNullableText(request.AllergyNote),

                HasBcgImmunization = request.HasBcgImmunization,
                HasHepatitisBImmunization = request.HasHepatitisBImmunization,
                HasPolioImmunization = request.HasPolioImmunization,
                HasDptImmunization = request.HasDptImmunization,
                HasMeaslesImmunization = request.HasMeaslesImmunization,
                ImmunizationNote = NormalizeNullableText(request.ImmunizationNote),

                AppetiteStatus = request.AppetiteStatus,
                HasNausea = request.HasNausea,
                HasVomiting = request.HasVomiting,
                NutritionRiskStatus = request.NutritionRiskStatus,
                NutritionRiskScore = request.NutritionRiskScore,
                NutritionNote = NormalizeNullableText(request.NutritionNote),

                HasFallRisk = request.HasFallRisk || request.HasAtaxia || request.HasPosturalInstability,
                HasAtaxia = request.HasAtaxia,
                HasPosturalInstability = request.HasPosturalInstability,
                FallRiskStatus = calculated.FallRiskStatus,
                FallRiskScore = calculated.FallRiskScore,
                FallRiskNote = NormalizeNullableText(request.FallRiskNote),

                FunctionalStatus = request.FunctionalStatus,
                FunctionalNote = NormalizeNullableText(request.FunctionalNote),
                PsychosocialNote = NormalizeNullableText(request.PsychosocialNote),
                EducationNote = NormalizeNullableText(request.EducationNote),
                NurseNote = NormalizeNullableText(request.NurseNote),

                StartedAt = now,
                CompletedAt = request.CompleteImmediately ? now : null,
                CompletedByUserId = request.CompleteImmediately ? actorUserId : null,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            NormalizeAssessmentData(entity);

            var hasilInstrumen = new List<AssessmentInstrumentResultResponse>();

            if (dokumenV2)
            {
                var penerapan = await _documentService.ApplyDraftAsync(
                    entity,
                    request.InstrumentResponses,
                    request.VitalSignId,
                    request.PainAssessmentState,
                    actorUserId);

                if (!penerapan.IsSuccess)
                {
                    return StatusCode(penerapan.StatusCode, ApiResponse<object>.Fail(
                        penerapan.StatusCode,
                        penerapan.ErrorMessage ?? "Pengkajian tidak dapat disimpan.",
                        penerapan.Errors));
                }

                hasilInstrumen = penerapan.Value ?? hasilInstrumen;
            }

            // Penyelesaian punya DUA pintu: PATCH /{id}/complete dan completeImmediately pada
            // pembuatan. Keduanya dijaga aturan yang sama.
            //
            // BE-RWI-109 kriteria 3 / FR-KEP-043. Daftar isian wajib tetap BE-RWI-056 (risiko jatuh
            // dan skrining gizi) dicabut dari jalur rawat inap. Isian wajib kini dibaca dari versi
            // instrumen yang disahkan (VAL-KEP-21a, VAL-KEP-21b, VAL-KEP-22a). Poliklinik, medical
            // check-up, dan IGD tidak pernah punya pemeriksaan ini dan tetap tidak tersentuh.
            if (dokumenV2 && entity.AssessmentStatus == PatientAssessmentStatus.Completed)
            {
                var penjagaSelesai = await _documentService.EnsureCanCompleteAsync(entity);

                if (!penjagaSelesai.IsSuccess)
                {
                    return StatusCode(penjagaSelesai.StatusCode, ApiResponse<object>.Fail(
                        penjagaSelesai.StatusCode,
                        penjagaSelesai.ErrorMessage ?? "Pengkajian belum dapat diselesaikan.",
                        penjagaSelesai.Errors));
                }
            }

            _dbContext.Set<TrxPatientAssessment>().Add(entity);

            // Assessment adalah dokumen klinis. Perubahan status antrean/encounter
            // dikendalikan oleh workflow NurseStationQueueController.FinishScreening
            // agar tidak terjadi double finalization antara assessment dan queue.

            // =================================================================================
            // BE-RWI-091 / FR-DOK-074, INV-DOK-18, RWI-DEC-144, RWI-DEC-151, migration R2
            // =================================================================================
            //
            // KONSEP KAJIAN MEDIS MENDAPAT IDENTITAS SEJAK SIMPAN PERTAMA, dengan alasan yang
            // sama seperti pada catatan dokter: konsep tanpa registrasi tidak dapat ditunjuk
            // saat kunjungan ditutup, dan tidak dapat ditemukan dokter pada "Catatan Saya".
            //
            // BATASNYA: hanya kajian yang menempel pada perawatan rawat inap. Kajian
            // poliklinik, medical check-up, dan IGD tetap didaftarkan pada saat finalisasi
            // seperti sebelumnya (BE-RWI-038, BE-RWI-065) — perilakunya tidak bergeser.
            //
            // Kajian yang langsung diselesaikan lewat completeImmediately tidak dibedakan:
            // RegisterAsync idempoten terhadap baris yang sudah ditambahkan pada unit kerja
            // yang sama, sehingga jalur finalisasi hanya menaikkan statusnya.
            if (entity.InpEpisodeId.HasValue &&
                entity.InpEpisodeId.Value != Guid.Empty &&
                entity.EncounterId != Guid.Empty)
            {
                var penulisKonsep = entity.AssessmentByUserId.HasValue &&
                                    entity.AssessmentByUserId.Value != Guid.Empty
                    ? entity.AssessmentByUserId.Value
                    : actorUserId;

                try
                {
                    await _integrityService.RegisterAsync(
                        ClinicalDocumentKind.Assessment,
                        entity.Id,
                        entity.PatientId,
                        entity.EncounterId,
                        authorUserId: penulisKonsep);
                }
                catch (InvalidOperationException pendaftaranGagal)
                {
                    // Keluar sebelum SaveChanges dan sebelum Commit: nol baris tersimpan.
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Pengkajian tidak dapat dibuat karena pendaftaran pada rekam medis " +
                        $"gagal: {pendaftaranGagal.Message}"
                    ));
                }
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = new PatientAssessmentCreateResponse
            {
                Id = entity.Id,
                AssessmentNumber = entity.AssessmentNumber,
                EncounterId = entity.EncounterId,
                QueueId = entity.QueueId,
                InpEpisodeId = entity.InpEpisodeId,
                AssessmentType = entity.AssessmentType,
                AssessmentStatus = entity.AssessmentStatus,
                AssessmentDateTime = entity.AssessmentDateTime,
                CompletedAt = entity.CompletedAt,
                DueAt = entity.DueAt,
                PolicyId = entity.PolicyId,
                BMI = entity.BMI,
                MeanArterialPressure = entity.MeanArterialPressure,
                MapStatus = entity.MapStatus,
                EarlyWarningScore = entity.EarlyWarningScore,
                EwsRiskLevel = entity.EwsRiskLevel,
                EwsMonitoringRecommendation = entity.EwsMonitoringRecommendation,
                InstrumentResults = hasilInstrumen
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientAssessment.CreateAssessment",
                "Membuat assessment pasien.",
                response
            );

            return Ok(ApiResponse<PatientAssessmentCreateResponse>.Ok(
                response,
                "Assessment pasien berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Assessment", Description = "Mengubah assessment pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientAssessment", "Update")]
        public async Task<IActionResult> UpdateAssessment(Guid id, [FromBody] UpdatePatientAssessmentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientAssessment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Assessment pasien tidak ditemukan."
                ));
            }

            var kewenanganPerawat = await EnsureNursingUnitAuthorityAsync(
                entity.AssessmentType, entity.InpEpisodeId);

            if (!kewenanganPerawat.IsValid)
            {
                return StatusCode(kewenanganPerawat.StatusCode, ApiResponse<object>.Fail(
                    kewenanganPerawat.StatusCode,
                    kewenanganPerawat.ErrorMessage ?? "Assessment ini tidak dapat diubah.",
                    kewenanganPerawat.Errors
                ));
            }

            // BE-RWI-088 titik panggil 5 dari 9. INV-DOK-14: konsep kajian hanya disunting
            // penulisnya, termasuk oleh DPJP pasien itu.
            var penjagaPenulis = await EnsureSoleAuthorAsync(entity);

            if (!penjagaPenulis.IsValid)
            {
                return StatusCode(penjagaPenulis.StatusCode, ApiResponse<object>.Fail(
                    penjagaPenulis.StatusCode,
                    penjagaPenulis.ErrorMessage ?? "Assessment ini tidak dapat diubah.",
                    penjagaPenulis.Errors
                ));
            }

            // BE-RWI-065 kriteria 3. Dokumen yang sudah terkunci pada mesin keutuhan menolak
            // penyuntingan langsung, beserta arahan memakai koreksi. Diperiksa lebih dulu supaya
            // pengguna menerima arahan yang benar, bukan kalimat teknis "tidak dapat diubah".
            //
            // Jenis dokumen yang belum ditegakkan dan dokumen yang belum terdaftar dilewatkan
            // apa adanya oleh EnsureMutableAsync, sehingga jalur poliklinik, medical check-up,
            // dan IGD tidak berubah sedikit pun - kriteria 5.
            //
            // BE-RWI-091 / VAL-DOK-43. Konsep kajian yang terkunci "Tidak Ditandatangani" karena
            // perawatannya ditutup dijawab 409 beserta arahan addendum; keadaan lain diteruskan
            // ke EnsureMutableAsync apa adanya.
            var keutuhan = await _integrityService.EnsureDraftStillOpenAsync(
                ClinicalDocumentKind.Assessment, id);

            if (!keutuhan.IsAllowed)
            {
                return StatusCode(keutuhan.StatusCode, ApiResponse<object>.Fail(
                    keutuhan.StatusCode,
                    keutuhan.ErrorMessage ?? "Assessment ini tidak dapat diubah."
                ));
            }

            if (entity.AssessmentStatus == PatientAssessmentStatus.Completed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Assessment yang sudah completed tidak dapat diubah."
                ));
            }

            // BE-RWI-106 kriteria 1 / RLN3-CAP-29.
            var enumTidakSah = TemukanEnumTidakSah(
                null,
                request.OxygenSupportType,
                request.ConsciousnessStatus,
                request.AppetiteStatus,
                request.NutritionRiskStatus,
                request.FallRiskStatus,
                request.FunctionalStatus);

            if (enumTidakSah != null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    enumTidakSah));
            }

            // BE-RWI-106 kriteria 4 / RLN3-CAP-21. PUT menimpa SELURUH isian. Layar yang mengisi
            // formulir dari baris daftar — yang memang tidak memuat catatan nyeri, riwayat obat,
            // dan belasan isian lain — akan mengosongkan isian itu tanpa perawat sadar. Kiriman
            // karena itu wajib membawa jejak waktu dari pembacaan detail terakhir.
            //
            // Contoh: Ns. Siti membuka detail pengkajian Budi pukul 09.00 (UpdateDateTime 08.55).
            // Ns. Rina menyimpan perubahan pukul 09.02. Kiriman Siti pukul 09.05 masih membawa
            // 08.55 → 409, bukan menimpa perubahan Rina.
            var penjagaBacaDetail = PeriksaBacaDetailSebelumSunting(entity, request.ExpectedUpdateDate);

            if (!penjagaBacaDetail.IsValid)
            {
                return StatusCode(penjagaBacaDetail.StatusCode, ApiResponse<object>.Fail(
                    penjagaBacaDetail.StatusCode,
                    penjagaBacaDetail.ErrorMessage ?? "Assessment ini tidak dapat diubah.",
                    penjagaBacaDetail.Errors
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var dokumenV2 = !IsKajianMedis(entity.AssessmentType) &&
                            NursingAssessmentDocumentService.IsInpatientV2(entity.InpEpisodeId, entity.AssessmentType);

            if (dokumenV2)
            {
                request.HasPain = request.PainAssessmentState == PainAssessmentState.HasPain;
                request.HasFallRisk = false;
                request.HasAtaxia = false;
                request.HasPosturalInstability = false;
            }

            var calculated = CalculateAssessmentValues(request, entity.InpEpisodeId.HasValue);

            entity.ChiefComplaint = NormalizeNullableText(request.ChiefComplaint);
            entity.CurrentIllnessHistory = NormalizeNullableText(request.CurrentIllnessHistory);
            entity.MedicationHistory = NormalizeNullableText(request.MedicationHistory);
            entity.PhysicalExamination = NormalizeNullableText(request.PhysicalExamination);
            entity.WorkingDiagnosis = NormalizeNullableText(request.WorkingDiagnosis);
            entity.TherapyPlan = NormalizeNullableText(request.TherapyPlan);

            entity.BloodPressureSystolic = request.BloodPressureSystolic;
            entity.BloodPressureDiastolic = request.BloodPressureDiastolic;
            entity.PulseRate = request.PulseRate;
            entity.IsPulseReadable = request.IsPulseReadable;
            entity.RespiratoryRate = request.RespiratoryRate;
            entity.Temperature = request.Temperature;
            entity.OxygenSaturation = request.OxygenSaturation;
            entity.IsUsingOxygen = request.IsUsingOxygen;
            entity.OxygenSupportType = request.OxygenSupportType;
            entity.OxygenFlowRate = request.OxygenFlowRate;
            entity.OxygenSupportNote = NormalizeNullableText(request.OxygenSupportNote);
            entity.ConsciousnessStatus = request.ConsciousnessStatus;
            entity.Weight = request.Weight;
            entity.Height = request.Height;
            entity.BMI = calculated.BMI;
            entity.MeanArterialPressure = calculated.MeanArterialPressure;
            entity.MapStatus = calculated.MapStatus;
            entity.EarlyWarningScore = calculated.EarlyWarningScore;
            entity.EwsRiskLevel = calculated.EwsRiskLevel;
            entity.EwsMonitoringRecommendation = calculated.EwsMonitoringRecommendation;

            entity.HasPain = request.HasPain;
            entity.PainScale = request.PainScale;
            entity.PainTrigger = NormalizeNullableText(request.PainTrigger);
            entity.PainQuality = NormalizeNullableText(request.PainQuality);
            entity.PainLocation = NormalizeNullableText(request.PainLocation);
            entity.PainFrequency = NormalizeNullableText(request.PainFrequency);
            entity.PainManagement = NormalizeNullableText(request.PainManagement);
            entity.PainNote = NormalizeNullableText(request.PainNote);

            entity.HasHereditaryDisease = request.HasHereditaryDisease;
            entity.HereditaryDiseaseNote = NormalizeNullableText(request.HereditaryDiseaseNote);

            entity.HasAllergy = request.HasAllergy;
            entity.AllergyType = NormalizeNullableText(request.AllergyType);
            entity.AllergyNote = NormalizeNullableText(request.AllergyNote);

            entity.HasBcgImmunization = request.HasBcgImmunization;
            entity.HasHepatitisBImmunization = request.HasHepatitisBImmunization;
            entity.HasPolioImmunization = request.HasPolioImmunization;
            entity.HasDptImmunization = request.HasDptImmunization;
            entity.HasMeaslesImmunization = request.HasMeaslesImmunization;
            entity.ImmunizationNote = NormalizeNullableText(request.ImmunizationNote);

            entity.AppetiteStatus = request.AppetiteStatus;
            entity.HasNausea = request.HasNausea;
            entity.HasVomiting = request.HasVomiting;
            entity.NutritionRiskStatus = request.NutritionRiskStatus;
            entity.NutritionRiskScore = request.NutritionRiskScore;
            entity.NutritionNote = NormalizeNullableText(request.NutritionNote);

            entity.HasFallRisk = request.HasFallRisk || request.HasAtaxia || request.HasPosturalInstability;
            entity.HasAtaxia = request.HasAtaxia;
            entity.HasPosturalInstability = request.HasPosturalInstability;
            entity.FallRiskStatus = calculated.FallRiskStatus;
            entity.FallRiskScore = calculated.FallRiskScore;
            entity.FallRiskNote = NormalizeNullableText(request.FallRiskNote);

            entity.FunctionalStatus = request.FunctionalStatus;
            entity.FunctionalNote = NormalizeNullableText(request.FunctionalNote);
            entity.PsychosocialNote = NormalizeNullableText(request.PsychosocialNote);
            entity.EducationNote = NormalizeNullableText(request.EducationNote);
            entity.NurseNote = NormalizeNullableText(request.NurseNote);

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            NormalizeAssessmentData(entity);

            if (dokumenV2)
            {
                // BE-RWI-109. Skor dihitung ulang dari versi yang berlaku saat konsep disimpan.
                var penerapan = await _documentService.ApplyDraftAsync(
                    entity,
                    request.InstrumentResponses,
                    request.VitalSignId,
                    request.PainAssessmentState,
                    actorUserId);

                if (!penerapan.IsSuccess)
                {
                    return StatusCode(penerapan.StatusCode, ApiResponse<object>.Fail(
                        penerapan.StatusCode,
                        penerapan.ErrorMessage ?? "Assessment ini tidak dapat diubah.",
                        penerapan.Errors));
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Assessment pasien berhasil diubah."
            ));
        }

        [HttpPatch("{id:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<PatientAssessmentCompleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Complete Patient Assessment", Description = "Menyelesaikan dokumen assessment pasien tanpa mengubah status antrean", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PatientAssessment", "Update")]
        public async Task<IActionResult> CompleteAssessment(Guid id, [FromBody] CompletePatientAssessmentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientAssessment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Assessment pasien tidak ditemukan."
                ));
            }

            if (entity.AssessmentStatus == PatientAssessmentStatus.Completed)
            {
                var completedResponse = new PatientAssessmentCompleteResponse
                {
                    Id = entity.Id,
                    AssessmentNumber = entity.AssessmentNumber,
                    EncounterId = entity.EncounterId,
                    QueueId = entity.QueueId,
                    InpEpisodeId = entity.InpEpisodeId,
                    AssessmentType = entity.AssessmentType,
                    AssessmentStatus = entity.AssessmentStatus,
                    CompletedAt = entity.CompletedAt,
                    CompletedByUserId = entity.CompletedByUserId,
                    IsAlreadyCompleted = true
                };

                return Ok(ApiResponse<PatientAssessmentCompleteResponse>.Ok(
                    completedResponse,
                    "Assessment pasien sudah completed."
                ));
            }

            // BE-RWI-078 / GUARD-INP-08. Diperiksa setelah jawaban idempotent di atas, sehingga
            // pengkajian yang memang sudah selesai tetap dijawab sama seperti sebelumnya.
            var kewenanganPerawat = await EnsureNursingUnitAuthorityAsync(
                entity.AssessmentType, entity.InpEpisodeId);

            if (!kewenanganPerawat.IsValid)
            {
                return StatusCode(kewenanganPerawat.StatusCode, ApiResponse<object>.Fail(
                    kewenanganPerawat.StatusCode,
                    kewenanganPerawat.ErrorMessage ?? "Assessment ini tidak dapat diselesaikan.",
                    kewenanganPerawat.Errors
                ));
            }

            // BE-RWI-088 titik panggil 6 dari 9. Menyelesaikan kajian sama dengan
            // menandatanganinya — api-contract.md 0.6.0 bagian 12.3. Dengan penjaga ini,
            // penanda tangan yang tercatat di bawah dijamin orang yang sama dengan penulisnya,
            // bukan siapa pun yang kebetulan menekan tombolnya.
            var penjagaPenulis = await EnsureSoleAuthorAsync(entity);

            if (!penjagaPenulis.IsValid)
            {
                return StatusCode(penjagaPenulis.StatusCode, ApiResponse<object>.Fail(
                    penjagaPenulis.StatusCode,
                    penjagaPenulis.ErrorMessage ?? "Assessment ini tidak dapat diselesaikan.",
                    penjagaPenulis.Errors
                ));
            }

            // BE-RWI-091 / VAL-DOK-43, RWI-DEC-138 butir (2). Konsep yang sudah terkunci
            // "Tidak Ditandatangani" tidak dapat diselesaikan belakangan: menyelesaikannya sama
            // dengan menandatangani mundur. Tanpa penjaga ini kajian menjadi Completed sementara
            // registrasinya tetap LockedUnsigned. Kajian yang belum terdaftar tidak tersentuh.
            var penjagaKeutuhanSelesai = await _integrityService.EnsureDraftStillOpenAsync(
                ClinicalDocumentKind.Assessment, entity.Id);

            if (!penjagaKeutuhanSelesai.IsAllowed)
            {
                return StatusCode(penjagaKeutuhanSelesai.StatusCode, ApiResponse<object>.Fail(
                    penjagaKeutuhanSelesai.StatusCode,
                    penjagaKeutuhanSelesai.ErrorMessage ?? "Assessment ini tidak dapat diselesaikan."
                ));
            }

            var isKajianMedis = IsKajianMedis(entity.AssessmentType);

            // BE-RWI-045 / VAL-DOK-10. Kajian medis yang belum lengkap tidak boleh difinalkan,
            // dan pesannya menyebut bagian mana yang masih kosong - bukan sekadar menolak.
            // Pengkajian keperawatan tidak tersentuh aturan ini; perilakunya tidak berubah.
            if (isKajianMedis)
            {
                // BE-RWI-068 / VAL-DOK-11. Sejak diagnosis terstruktur dapat lahir dari kajian
                // medis, daftar masalah punya dua bentuk sah: teks bebas WorkingDiagnosis, dan
                // baris berkode ICD pada TrxPatientDiagnosis. Kajian lolos bila salah satu
                // terisi; menuntut keduanya memaksa dokter mengetik hal yang sama dua kali.
                var adaDaftarMasalahTerstruktur =
                    await AdaDaftarMasalahTerstrukturAsync(entity);

                var bagianKosong = BagianKajianMedisYangKosong(
                    entity,
                    adaDaftarMasalahTerstruktur);

                if (bagianKosong.Count > 0)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Kajian medis belum dapat diselesaikan. Bagian berikut masih kosong: " +
                        $"{string.Join(", ", bagianKosong)}."
                    ));
                }
            }

            // BE-RWI-109 kriteria 3 / BE-RWI-111 kriteria 2. Dokumen keperawatan rawat inap V2
            // diselesaikan menurut versi instrumennya: versi sah di produksi, isian wajib versi itu,
            // isian berskor terjawab, dan keadaan nyeri pada Monitoring Nyeri. Daftar isian wajib
            // tetap BE-RWI-056 sudah dicabut; skor tidak dihitung ulang setelah dokumen selesai.
            if (!isKajianMedis && NursingAssessmentDocumentService.IsInpatientV2(entity.InpEpisodeId, entity.AssessmentType))
            {
                var penjagaSelesai = await _documentService.EnsureCanCompleteAsync(entity);

                if (!penjagaSelesai.IsSuccess)
                {
                    return StatusCode(penjagaSelesai.StatusCode, ApiResponse<object>.Fail(
                        penjagaSelesai.StatusCode,
                        penjagaSelesai.ErrorMessage ?? "Pengkajian belum dapat diselesaikan.",
                        penjagaSelesai.Errors));
                }
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.AssessmentStatus = PatientAssessmentStatus.Completed;
            entity.CompletedAt = now;
            entity.CompletedByUserId = actorUserId;
            entity.NurseNote = NormalizeNullableText(request.NurseNote) ?? entity.NurseNote;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            // Tidak mengubah QueueStatus/EncounterStatus di sini.
            // Finalisasi perjalanan pasien dilakukan oleh endpoint:
            // POST /nurse-station-queues/{id}/finish-screening

            // BE-RWI-045 / api-contract.md bagian 2, RWI-AC-157. Menyelesaikan kajian medis
            // sekaligus mendaftarkannya pada mesin keutuhan rekam medis, dalam SaveChanges yang
            // sama - supaya tidak pernah ada kajian selesai yang tidak punya baris keutuhan.
            //
            // Sengaja hanya untuk kajian medis. Pendaftaran pengkajian keperawatan adalah
            // pekerjaan sub-modul keperawatan; menyalakannya dari sini akan mengubah perilaku
            // jalur poliklinik dan IGD yang tidak diminta task ini.
            //
            // BE-RWI-038 menaikkannya dari sekadar terdaftar menjadi TERTANDA TANGAN, dengan
            // penulis kajian sebagai penanda tangannya. Bedanya menentukan: dokumen berstatus
            // draf ditolak mesin koreksi dengan arahan menyunting langsung, sedangkan dokumen
            // tertanda tangan justru satu-satunya yang menerima koreksi. Kajian yang selesai
            // tidak dapat disunting lagi, jadi ia wajib berada di keadaan yang menerima koreksi.
            //
            // BE-RWI-065 melebarkannya ke pengkajian KEPERAWATAN yang menempel pada perawatan
            // rawat inap. Batas itu disengaja: pengkajian poliklinik, medical check-up, dan IGD
            // tidak punya perawatan, sehingga perilakunya tidak bergeser satu langkah pun -
            // persis yang dijaga kriteria 5 task itu. Tanpa pendaftaran ini, BE-RWI-057 tidak
            // punya apa pun untuk dikoreksi: mesin koreksi menolak dokumen yang belum terdaftar.
            var isRegisteredToIntegrity = false;

            var wajibDidaftarkan = isKajianMedis || entity.InpEpisodeId.HasValue;

            if (wajibDidaftarkan && entity.EncounterId != Guid.Empty)
            {
                var authorUserId = entity.AssessmentByUserId ?? entity.CreateBy;

                if (authorUserId == Guid.Empty)
                    authorUserId = actorUserId;

                try
                {
                    await _integrityService.RegisterSignedAsync(
                        ClinicalDocumentKind.Assessment,
                        entity.Id,
                        entity.PatientId,
                        entity.EncounterId,
                        authorUserId,
                        deviceInfo: Request.Headers.UserAgent.ToString(),
                        ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                        nowUtc: now);
                }
                catch (InvalidOperationException pendaftaranGagal)
                {
                    // BE-RWI-038 kriteria 2, dilanjutkan BE-RWI-065 kriteria 2. Pendaftaran dan
                    // penyelesaian berada pada SaveChanges yang sama; keluar lebih awal berarti
                    // tidak satu pun perubahan di atas ikut tersimpan, sehingga pengkajian tetap
                    // belum selesai. Tidak boleh ada dokumen Completed yang tidak punya baris
                    // keutuhan - dokumen seperti itu tidak dapat dikoreksi selamanya.
                    var sebutan = isKajianMedis ? "Kajian medis" : "Pengkajian";

                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"{sebutan} tidak dapat diselesaikan karena pendaftaran pada rekam " +
                        $"medis gagal: {pendaftaranGagal.Message}"
                    ));
                }

                isRegisteredToIntegrity = true;
            }

            await _dbContext.SaveChangesAsync();

            var response = new PatientAssessmentCompleteResponse
            {
                Id = entity.Id,
                AssessmentNumber = entity.AssessmentNumber,
                EncounterId = entity.EncounterId,
                QueueId = entity.QueueId,
                InpEpisodeId = entity.InpEpisodeId,
                AssessmentType = entity.AssessmentType,
                AssessmentStatus = entity.AssessmentStatus,
                CompletedAt = entity.CompletedAt,
                CompletedByUserId = entity.CompletedByUserId,
                IsAlreadyCompleted = false,
                IsRegisteredToIntegrity = isRegisteredToIntegrity
            };

            return Ok(ApiResponse<PatientAssessmentCompleteResponse>.Ok(
                response,
                "Assessment pasien berhasil diselesaikan."
            ));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Patient Assessment", Description = "Membatalkan assessment pasien", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PatientAssessment", "Update")]
        public async Task<IActionResult> CancelAssessment(Guid id, [FromBody] CancelPatientAssessmentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientAssessment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Assessment pasien tidak ditemukan."
                ));
            }

            // BE-RWI-078 / GUARD-INP-08. Pembatalan adalah perubahan rekam medis, jadi ia tunduk
            // pada gerbang unit yang sama seperti penulisannya.
            var kewenanganPerawat = await EnsureNursingUnitAuthorityAsync(
                entity.AssessmentType, entity.InpEpisodeId);

            if (!kewenanganPerawat.IsValid)
            {
                return StatusCode(kewenanganPerawat.StatusCode, ApiResponse<object>.Fail(
                    kewenanganPerawat.StatusCode,
                    kewenanganPerawat.ErrorMessage ?? "Assessment ini tidak dapat dibatalkan.",
                    kewenanganPerawat.Errors
                ));
            }

            // BE-RWI-088 titik panggil 7 dari 9.
            var penjagaPenulis = await EnsureSoleAuthorAsync(entity);

            if (!penjagaPenulis.IsValid)
            {
                return StatusCode(penjagaPenulis.StatusCode, ApiResponse<object>.Fail(
                    penjagaPenulis.StatusCode,
                    penjagaPenulis.ErrorMessage ?? "Assessment ini tidak dapat dibatalkan.",
                    penjagaPenulis.Errors
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.AssessmentStatus = PatientAssessmentStatus.Cancelled;
            entity.CancelledAt = now;
            entity.CancelledByUserId = actorUserId;
            entity.CancelReason = request.CancelReason.Trim();
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            // BE-RWI-091 / RWI-AC-217. Registrasi keutuhannya menjadi Cancelled, barisnya tidak
            // dihapus, dan keduanya tersimpan pada SaveChanges yang sama.
            await _integrityService.MarkCancelledAsync(
                ClinicalDocumentKind.Assessment,
                entity.Id,
                actorUserId,
                entity.CancelReason,
                now);

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Assessment pasien berhasil dibatalkan."
            ));
        }

        // =====================================================================
        // BE-RWI-057 - koreksi pengkajian final lewat mesin addendum rekam medis
        // =====================================================================

        /// <summary>
        /// Menambahkan koreksi pada pengkajian yang sudah selesai.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-057</c>, <c>RWI-DEC-091</c>, <c>FR-KEP-008</c>. Perawat yang salah mengisi
        /// membetulkannya lewat koreksi beralasan, <b>bukan</b> dengan menimpa isinya. Isi asli
        /// tetap tersimpan sebagai bukti klinis, dan status pengkajian <b>tetap</b>
        /// <c>Completed</c> sesudah berapa kali pun dikoreksi.
        /// </para>
        /// <para>
        /// <b>Endpoint ini tidak menyimpan apa pun sendiri.</b> Ia meneruskan ke
        /// <c>ClinicalNoteAddendumService</c> milik <c>MedicalRecordManagement</c> dengan jenis
        /// dokumen <c>Assessment</c>. Membangun penyimpanan koreksi di dalam
        /// <c>ClinicalManagement</c> berarti membuat mesin koreksi tandingan, dan itu dilarang
        /// <c>RWI-DEC-087</c>. Karena itu migration task ini kosong dan nol tabel baru dibuat.
        /// </para>
        /// <para>
        /// <b>Contoh nyata.</b> Ns. Sari menyelesaikan pengkajian awal Tn. Budi pukul 09.00, lalu
        /// pukul 10.00 menyadari skor nyerinya tertulis 3 padahal seharusnya 7. Ia menambahkan
        /// koreksi berbunyi "skala nyeri seharusnya 7" beserta alasannya. Yang terjadi: baris
        /// pengkajian <b>tidak berubah sedikit pun</b>, koreksi tercatat sebagai addendum bernomor
        /// 1 atas namanya beserta waktunya, dan status pengkajian tetap <c>Completed</c>.
        /// </para>
        /// <para>
        /// Koreksi atas nama penulis lain - misalnya ketika penulisnya berhalangan - memakai
        /// endpoint pengganti milik <c>MedicalRecordManagement</c>, karena aturan pengganti beserta
        /// penetapan berhalangannya dimiliki modul itu.
        /// </para>
        /// </remarks>
        [HttpPost("{id:guid}/addendums")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalNoteAddendumResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Amend", "Amend Patient Assessment", Description = "Menambahkan koreksi pada pengkajian yang sudah selesai", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PatientAssessment", "Amend")]
        public async Task<IActionResult> CreateAddendum(
            Guid id,
            [FromBody] CreateAssessmentAddendumRequest request,
            CancellationToken cancellationToken = default)
        {
            var pengkajian = await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new { x.AssessmentType, x.InpEpisodeId })
                .FirstOrDefaultAsync(cancellationToken);

            if (pengkajian == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Assessment pasien tidak ditemukan."
                ));
            }

            // BE-RWI-078 / GUARD-INP-08. Koreksi adalah penulisan, dan penulisnya tetap wajib
            // bertugas di unit tempat perawatan berada.
            var kewenanganPerawat = await EnsureNursingUnitAuthorityAsync(
                pengkajian.AssessmentType, pengkajian.InpEpisodeId, cancellationToken);

            if (!kewenanganPerawat.IsValid)
            {
                return StatusCode(kewenanganPerawat.StatusCode, ApiResponse<object>.Fail(
                    kewenanganPerawat.StatusCode,
                    kewenanganPerawat.ErrorMessage ?? "Koreksi tidak dapat ditambahkan.",
                    kewenanganPerawat.Errors
                ));
            }

            var actorUserId = GetCurrentUserId();

            var (hasil, addendum) = await _addendumService.CreateAsync(
                ClinicalDocumentKind.Assessment,
                id,
                actorUserId,
                actorHasSubstituteAuthority: false,
                request.Content,
                request.Reason,
                deviceInfo: Request.Headers.UserAgent.ToString(),
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                nowUtc: DateTime.UtcNow,
                cancellationToken);

            if (!hasil.IsAllowed || addendum == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Koreksi tidak dapat ditambahkan."
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientAssessment.CreateAddendum",
                "Menambahkan koreksi pada pengkajian yang sudah selesai.",
                new
                {
                    EntityId = id,
                    addendum.IntegrityId,
                    addendum.Sequence,
                    Controller = "PatientAssessment",
                    Action = "Amend"
                });

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<ClinicalNoteAddendumResponse>.Ok(
                    await ToAddendumResponseAsync(addendum, cancellationToken),
                    "Koreksi berhasil ditambahkan."));
        }

        /// <summary>Daftar koreksi satu pengkajian, terurut nomor.</summary>
        /// <remarks>
        /// <c>BE-RWI-057</c>. Dibaca ruang kerja keperawatan supaya isi asli dan koreksinya
        /// tampil bersebelahan - pembaca berikutnya melihat keduanya, bukan hanya yang terakhir.
        /// </remarks>
        [HttpGet("{id:guid}/addendums")]
        [ProducesResponseType(typeof(ApiResponse<List<ClinicalNoteAddendumResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat koreksi satu pengkajian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> GetAddendums(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var adaPengkajian = await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (!adaPengkajian)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Assessment pasien tidak ditemukan."
                ));
            }

            var daftar = await _addendumService.ListByDocumentAsync(
                ClinicalDocumentKind.Assessment, id, cancellationToken);

            var nama = await AmbilNamaPenulisAsync(
                daftar.Select(x => x.AuthorUserId).Distinct().ToList(), cancellationToken);

            var response = daftar
                .Select(x => new ClinicalNoteAddendumResponse
                {
                    Id = x.Id,
                    IntegrityId = x.IntegrityId,
                    Sequence = x.Sequence,
                    AuthorUserId = x.AuthorUserId,
                    AuthorName = nama.GetValueOrDefault(x.AuthorUserId),
                    IsSubstituteAuthor = x.IsSubstituteAuthor,
                    DelegationId = x.DelegationId,
                    AddendumText = x.AddendumText,
                    CorrectionReason = x.CorrectionReason,
                    SignedAt = x.SignedAt
                })
                .ToList();

            return Ok(ApiResponse<List<ClinicalNoteAddendumResponse>>.Ok(
                response,
                "Daftar koreksi pengkajian berhasil diambil."));
        }

        // =====================================================================
        // BE-RWI-058 - lini masa dan keadaan tenggat
        // =====================================================================

        /// <summary>
        /// Lini masa pengkajian keperawatan satu perawatan: nyeri, risiko jatuh, dan skrining
        /// gizi dari waktu ke waktu.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-058</c>, <c>FR-KEP-007</c>, <c>AC-CAP012-02</c>. Yang dijawab endpoint ini
        /// bukan "berapa nilai nyeri pasien", melainkan "apakah nyerinya membaik atau memburuk".
        /// Karena itu isinya seluruh pengukuran terurut waktu; nilai lama tidak pernah ditimpa.
        /// </para>
        /// <para>
        /// Keadaan tenggat setiap baris dihitung dari tenggat yang <b>sudah tersimpan</b> pada
        /// pengkajiannya, sehingga mengubah kebijakan hari ini tidak mengubah penilaian
        /// pengkajian yang lalu - <c>AC-CAP012-04</c>.
        /// </para>
        /// </remarks>
        [HttpGet("episodes/{episodeId:guid}/timeline")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentTimelineResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat lini masa pengkajian satu perawatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> GetEpisodeTimeline(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _monitoringService.GetTimelineAsync(episodeId, cancellationToken);

            if (hasil == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Perawatan rawat inap tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<AssessmentTimelineResponse>.Ok(
                hasil,
                "Lini masa pengkajian berhasil diambil."));
        }

        /// <summary>Keadaan tenggat seluruh pengkajian keperawatan pada satu perawatan.</summary>
        /// <remarks>
        /// <c>BE-RWI-058</c>, <c>FR-KEP-010</c>, <c>VAL-KEP-17</c>. Master kebijakan yang kosong
        /// menghasilkan keadaan <c>NotMonitored</c> beserta kalimat "batas waktu pengkajian belum
        /// ditetapkan" - <b>bukan</b> "terlambat" dan bukan pula "tidak ada data".
        /// </remarks>
        [HttpGet("episodes/{episodeId:guid}/due-status")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentDueStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat keadaan tenggat pengkajian satu perawatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> GetEpisodeDueStatus(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _monitoringService.GetDueStatusAsync(episodeId, cancellationToken);

            if (hasil == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Perawatan rawat inap tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<AssessmentDueStatusResponse>.Ok(
                hasil,
                hasil.Explanation));
        }

        // =====================================================================
        // BE-RWI-112 - progres pengkajian lima bagian
        // =====================================================================

        /// <summary>Progres Pengkajian Pasien lima bagian beserta alert klinis kepala konteks.</summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-112</c>, <c>FR-KEP-050</c> s.d. <c>FR-KEP-052</c>, api-contract 0.5.0 bagian 7.1.
        /// Persen dihitung server (<c>CompletedCount × 20</c>); Pengawasan Harian dan Evaluasi Awal tampil
        /// tanpa dihitung. Temuan berisiko ada pada <c>Alerts</c>, tidak mengubah ✓/!/○.
        /// </para>
        /// <para>
        /// <b>Gagal bukan ○.</b> Galat membaca data tidak pernah dijawab lima bagian kosong; layar
        /// menampilkan "Gagal memuat progres pengkajian" (<c>AC-KEP-072</c>).
        /// </para>
        /// </remarks>
        [HttpGet("episodes/{episodeId:guid}/progress")]
        [ProducesResponseType(typeof(ApiResponse<NursingAssessmentProgressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat progres pengkajian lima bagian satu perawatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> GetEpisodeProgress(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _progressService.GetProgressAsync(episodeId, cancellationToken);

            if (hasil == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Perawatan rawat inap tidak ditemukan."));
            }

            return Ok(ApiResponse<NursingAssessmentProgressResponse>.Ok(
                hasil,
                $"Progres pengkajian {hasil.ProgressPercent}%."));
        }

        /// <summary>Versi instrumen yang berlaku bagi pasien satu perawatan — dipanggil formulir.</summary>
        /// <remarks>
        /// <c>INT-KEP-16</c>, api-contract 0.5.0 bagian 7.2 <c>GET /resolve</c>. Dipasang pada grup ini
        /// supaya pemanggilnya cukup memegang <c>PatientAssessment : Read</c> seperti yang diminta
        /// kontrak. Contoh: Budi 67 tahun, <c>instrumentKind=FallRiskScale</c> → instrumen dewasa;
        /// bila hanya ada konsep di produksi, <c>IsApproved = false</c> dan tombol Selesai nonaktif.
        /// </remarks>
        [HttpGet("instruments/resolve")]
        [ProducesResponseType(typeof(ApiResponse<ResolvedInstrumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat instrumen berversi yang berlaku bagi pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> ResolveInstrument(
            [FromQuery] ClinicalInstrumentKind instrumentKind,
            [FromQuery] Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _instrumentService.ResolveForEpisodeAsync(instrumentKind, episodeId, cancellationToken);
            return this.ToActionResult(hasil);
        }

        // =====================================================================
        // BE-RWI-064 - daftar pantau kepatuhan pengkajian awal
        // =====================================================================

        /// <summary>
        /// Daftar perawatan berjalan yang pengkajian awalnya belum ada atau sudah lewat tenggat.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-064</c>, <c>FR-KEP-024</c> s.d. <c>FR-KEP-026</c>, <c>RWI-RULE-023</c>. Ini
        /// daftar pantau ketiga yang selama ini tercatat sebagai gap sejak <c>BE-RWI-029</c>.
        /// </para>
        /// <para>
        /// <b>Daftar ini tidak pernah menahan pekerjaan siapa pun.</b> Keterlambatan pengkajian
        /// bukan gerbang: perawat tetap dapat mencatat tindakan pada perawatan yang muncul di
        /// sini, dan dokter tetap dapat menulis catatannya - <c>INV-KEP-03</c>,
        /// <c>VAL-KEP-18</c>. Daftar pantau yang memblokir pekerjaan klinis hanya akan mendorong
        /// orang mengakali sistem.
        /// </para>
        /// <para>
        /// <b>Tiga keadaan kosong yang berbeda artinya.</b> Tanpa baris terlambat, pesannya
        /// "seluruh pengkajian awal sudah tepat waktu". Tanpa kebijakan sama sekali, pesannya
        /// "batas waktu pengkajian belum ditetapkan". Keduanya bukan "tidak ada data" -
        /// <c>FR-KEP-025</c>.
        /// </para>
        /// <para>
        /// Isinya sengaja hanya nama pasien, lokasi, dan keterlambatan. Tidak ada satu pun isi
        /// klinis di sini.
        /// </para>
        /// </remarks>
        [HttpGet("monitoring/initial-assessment-compliance")]
        [ProducesResponseType(typeof(ApiResponse<ResponseInitialAssessmentCompliancePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Assessment", Description = "Melihat daftar pantau kepatuhan pengkajian awal", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientAssessment", "Read")]
        public async Task<IActionResult> GetInitialAssessmentCompliance(
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] bool onlyLate = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _monitoringService.GetInitialAssessmentComplianceAsync(
                serviceUnitId, onlyLate, pageNumber, pageSize, cancellationToken);

            var pesan = await _monitoringService.JelaskanDaftarPantauAsync(
                hasil.TotalData, cancellationToken);

            return Ok(ApiResponse<ResponseInitialAssessmentCompliancePagedResult>.Ok(hasil, pesan));
        }

        /// <summary>Menyusun balasan satu koreksi beserta nama penulisnya.</summary>
        private async Task<ClinicalNoteAddendumResponse> ToAddendumResponseAsync(
            MrcClinicalNoteAddendum addendum,
            CancellationToken cancellationToken)
        {
            var nama = await AmbilNamaPenulisAsync([addendum.AuthorUserId], cancellationToken);

            return new ClinicalNoteAddendumResponse
            {
                Id = addendum.Id,
                IntegrityId = addendum.IntegrityId,
                Sequence = addendum.Sequence,
                AuthorUserId = addendum.AuthorUserId,
                AuthorName = nama.GetValueOrDefault(addendum.AuthorUserId),
                IsSubstituteAuthor = addendum.IsSubstituteAuthor,
                DelegationId = addendum.DelegationId,
                AddendumText = addendum.AddendumText,
                CorrectionReason = addendum.CorrectionReason,
                SignedAt = addendum.SignedAt
            };
        }

        private async Task<Dictionary<Guid, string>> AmbilNamaPenulisAsync(
            List<Guid> userIds,
            CancellationToken cancellationToken)
        {
            if (userIds.Count == 0)
                return [];

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);
        }

        private async Task<CreateGuard> ValidateCreateRequestAsync(
            CreatePatientAssessmentRequest request)
        {
            // BE-RWI-106 kriteria 1 / RLN3-CAP-29. Angka enum di luar daftar sah ditolak paling
            // awal, sebelum satu pun penjaga lain membaca database.
            var enumTidakSah = TemukanEnumTidakSah(
                request.AssessmentType,
                request.OxygenSupportType,
                request.ConsciousnessStatus,
                request.AppetiteStatus,
                request.NutritionRiskStatus,
                request.FallRiskStatus,
                request.FunctionalStatus);

            if (enumTidakSah != null)
                return CreateGuard.Fail(enumTidakSah);

            // BE-RWI-044 / VAL-DOK-26 dan BE-RWI-045 / VAL-DOK-01, VAL-DOK-05. Penanda perawatan
            // dan kewenangan menulis kajian medis diperiksa pada kedua cabang: kunjungan yang
            // berantre pun dapat menaungi perawatan rawat inap, dan penanda yang salah di sana
            // menempelkan dokumen ke perawatan yang keliru dengan cara yang sama persis.
            var konteks = await ValidateInpatientMarkersAsync(request);

            if (!konteks.IsValid)
                return konteks;

            var kajianMedis = await ValidateMedicalAssessmentRuleAsync(request);

            if (!kajianMedis.IsValid)
                return kajianMedis;

            // BE-RWI-056 / VAL-KEP-11. Satu perawatan hanya boleh punya satu pengkajian awal
            // keperawatan yang hidup. Penjagaannya di tingkat service, bukan unique index -
            // lihat keterangan pada ValidateSingleInitialNursingAssessmentAsync.
            var pengkajianAwal = await ValidateSingleInitialNursingAssessmentAsync(request);

            if (!pengkajianAwal.IsValid)
                return pengkajianAwal;

            // BE-RWI-078 / GUARD-INP-07 dan GUARD-INP-08. Pengkajian keperawatan rawat inap hanya
            // ditulis perawat yang bertugas di unit tempat perawatannya berada.
            var kewenanganPerawat = await ValidateNursingUnitAuthorityAsync(request);

            if (!kewenanganPerawat.IsValid)
                return kewenanganPerawat;

            if (!request.QueueId.HasValue || request.QueueId.Value == Guid.Empty)
                return await ValidateCreateWithoutQueueAsync(request);

            var queue = await _dbContext.Set<TrxQueue>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.QueueId.Value &&
                    x.EncounterId == request.EncounterId &&
                    !x.IsDelete);

            if (queue == null)
                return CreateGuard.Fail("Antrean tidak ditemukan atau tidak sesuai dengan encounter.");

            if (!queue.IsScreeningRequired)
                return CreateGuard.Fail("Antrean ini tidak membutuhkan screening.");

            var isNurseScreeningStatus =
                queue.QueueStatus == QueueStatus.CalledByNurse ||
                queue.QueueStatus == QueueStatus.InNurseScreening ||
                queue.QueueStatus == QueueStatus.WaitingForNurse;

            var isDoctorScreeningStatus =
                queue.QueueStatus == QueueStatus.WaitingForDoctor ||
                queue.QueueStatus == QueueStatus.CalledByDoctor ||
                queue.QueueStatus == QueueStatus.InConsultation;

            if (!isNurseScreeningStatus && !isDoctorScreeningStatus)
            {
                return CreateGuard.Fail("Status antrean tidak valid untuk assessment.");
            }

            if (isDoctorScreeningStatus && !request.CompleteImmediately)
            {
                // BE-RWI-045. Penjagaan draf disaring jenis: kajian medis dan pengkajian
                // keperawatan hidup pada satu tabel tetapi punya mesin status sendiri-sendiri,
                // sehingga draf milik profesi lain tidak boleh menutup pembuatan milik profesi
                // ini - AC-CAP022-02. Bagi jalur rawat jalan tidak ada yang berubah: seluruh
                // baris lama maupun kirimannya bernilai Initial.
                var doctorDraftExists = await _dbContext.Set<TrxPatientAssessment>()
                    .AnyAsync(x =>
                        x.QueueId == request.QueueId &&
                        x.AssessmentType == request.AssessmentType &&
                        !x.IsDelete &&
                        x.IsActive &&
                        (x.AssessmentStatus == PatientAssessmentStatus.Draft ||
                         x.AssessmentStatus == PatientAssessmentStatus.InProgress));

                if (doctorDraftExists)
                    return CreateGuard.Fail("Draft assessment dokter untuk antrean ini sudah ada. Lanjutkan draft tersebut, bukan membuat baru.");
            }
            else if (!isDoctorScreeningStatus)
            {
                var editableAssessmentExists = await _dbContext.Set<TrxPatientAssessment>()
                    .AnyAsync(x =>
                        x.EncounterId == request.EncounterId &&
                        x.AssessmentType == request.AssessmentType &&
                        !x.IsDelete &&
                        x.IsActive &&
                        (x.AssessmentStatus == PatientAssessmentStatus.Draft ||
                         x.AssessmentStatus == PatientAssessmentStatus.InProgress));

                if (editableAssessmentExists)
                    return CreateGuard.Fail("Draft assessment untuk encounter ini sudah ada. Lanjutkan draft tersebut, bukan membuat baru.");
            }

            return CreateGuard.Ok();
        }

        /// <summary>
        /// Penjagaan pembuatan pengkajian <b>tanpa antrean</b>.
        /// </summary>
        /// <remarks>
        /// <c>BE-IGD-027</c>, requirement <c>FR-IGD-061</c>, keputusan <c>IGD-DEC-068</c>.
        ///
        /// <para>
        /// Jalur ini sengaja <b>tidak</b> dibuka untuk sembarang encounter. Melepas kewajiban
        /// antrean tanpa syarat berarti pengkajian rawat jalan dapat dibuat melewati screening
        /// — penjagaan yang justru menjadi alasan kolom antrean ada. Karena itu jalur tanpa
        /// antrean hanya terbuka bagi encounter yang punya kunjungan IGD.
        /// </para>
        ///
        /// <para>
        /// Syaratnya diperiksa lewat kunjungan IGD, <b>bukan</b> lewat
        /// <c>EncounterType.Emergency</c>. Sebabnya <c>IGD-DEC-109</c>: seluruh kunjungan IGD
        /// lama bertipe <c>Outpatient</c> dan migration penggantinya belum diterapkan, sehingga
        /// menyaring dengan jenis encounter akan menutup pengkajian bagi pasien IGD yang sudah
        /// terdaftar hari ini.
        /// </para>
        /// </remarks>
        private async Task<CreateGuard> ValidateCreateWithoutQueueAsync(
            CreatePatientAssessmentRequest request)
        {
            var encounterExists = await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.EncounterId && !x.IsDelete);

            if (!encounterExists)
                return CreateGuard.Fail("Encounter tidak ditemukan.");

            var pintuMasuk = await ValidateWithoutQueueGateAsync(request);

            if (!pintuMasuk.IsValid)
                return pintuMasuk;

            var editableAssessmentExists = await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.EncounterId == request.EncounterId &&
                    x.AssessmentType == request.AssessmentType &&
                    !x.IsDelete &&
                    x.IsActive &&
                    (x.AssessmentStatus == PatientAssessmentStatus.Draft ||
                     x.AssessmentStatus == PatientAssessmentStatus.InProgress));

            if (editableAssessmentExists)
                return CreateGuard.Fail("Draft assessment untuk encounter ini sudah ada. Lanjutkan draft tersebut, bukan membuat baru.");

            return CreateGuard.Ok();
        }

        /// <summary>
        /// Pintu masuk pembuatan pengkajian <b>tanpa antrean</b>: siapa yang boleh melewatinya.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-044</c>. Bentuknya sengaja sama persis dengan penjagaan pada catatan
        /// dokter. Kunjungan IGD diperiksa lebih dulu dan dilewatkan apa adanya, sehingga
        /// perilaku IGD - <c>BE-IGD-027</c>, <c>IGD-DEC-068</c>, <c>IGD-DEC-109</c> - tidak
        /// bergeser satu langkah pun. Baru sesudahnya konteks rawat inap dibentuk, dan sebab
        /// penolakannya diteruskan beserta kode aslinya.
        /// </para>
        /// <para>
        /// Kunjungan poliklinik dan medical check-up tetap tertutup: melepas kewajiban antrean
        /// bagi keduanya berarti pengkajian rawat jalan dapat dibuat melewati screening, dan
        /// penjagaan itulah yang justru menjadi alasan kolom antrean ada.
        /// </para>
        /// </remarks>
        private async Task<CreateGuard> ValidateWithoutQueueGateAsync(
            CreatePatientAssessmentRequest request)
        {
            var isEmergencyEncounter = await _dbContext.Set<EmgVisit>()
                .AsNoTracking()
                .AnyAsync(x => x.EncounterId == request.EncounterId && !x.IsDelete);

            if (isEmergencyEncounter)
                return CreateGuard.Ok();

            var context = await _inpatientClinicalContextService.ResolveAsync(
                request.EncounterId,
                expectedEpisodeId: request.InpEpisodeId,
                forNewDocument: true);

            if (context.IsResolved)
                return CreateGuard.Ok();

            if (context.Outcome == InpatientClinicalContextOutcome.NoInpatientEpisode)
            {
                // BE-RWI-054, VAL-KEP-01 dan VAL-KEP-04. Dua sebab yang terlihat sama tetapi
                // berbeda artinya bagi pengguna, jadi kalimat dan kodenya pun dibedakan:
                //
                //  - Kunjungan rawat inap yang belum punya perawatan berarti admisinya belum
                //    dibuat. Itu keadaan data yang belum siap, dijawab 422 supaya layar
                //    keperawatan dapat mengarahkan petugas menyelesaikan admisi lebih dulu.
                //  - Kunjungan poliklinik dan medical check-up memang tidak pernah punya
                //    perawatan. Bagi keduanya jawabannya tetap 400 beserta kalimat lama, dan
                //    perilakunya tidak berubah satu langkah pun - RWI-DEC-070.
                var jenisKunjungan = await _dbContext.Set<RegPatientEncounter>()
                    .AsNoTracking()
                    .Where(x => x.Id == request.EncounterId && !x.IsDelete)
                    .Select(x => x.EncounterType)
                    .FirstOrDefaultAsync();

                return jenisKunjungan == EncounterType.Inpatient
                    ? CreateGuard.Fail(
                        PenolakanTanpaPerawatanRawatInap,
                        StatusCodes.Status422UnprocessableEntity)
                    : CreateGuard.Fail(PenolakanTanpaAntreanBukanRawatInap);
            }

            return CreateGuard.Fail(
                context.ErrorMessage ?? "Konteks perawatan rawat inap tidak dapat dibentuk.",
                context.StatusCode);
        }

        /// <summary>
        /// Penjagaan kewenangan menulis perawat pada <b>pembuatan</b> pengkajian -
        /// <c>BE-RWI-078</c>, <c>GUARD-INP-08</c>, <c>AC-KEP-044</c> s.d. <c>AC-KEP-049</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Hanya menyentuh pengkajian keperawatan rawat inap.</b> Kajian medis punya
        /// penjaganya sendiri lewat <c>ValidateMedicalAssessmentRuleAsync</c>, dan kunjungan yang
        /// tidak menaungi perawatan rawat inap - poliklinik, medical check-up, dan IGD -
        /// dilewatkan apa adanya. Tanpa penyaringan itu, penjaga ini akan menutup screening
        /// rawat jalan yang tidak pernah punya unit rawat inap untuk dibandingkan.
        /// </para>
        /// <para>
        /// Perawatan dicari dari penanda yang dikirim bila ada, atau dari kunjungannya - urutan
        /// yang sama dengan <c>ValidateSingleInitialNursingAssessmentAsync</c>, supaya dua
        /// penjaga pada permintaan yang sama tidak pernah menilai perawatan yang berbeda.
        /// </para>
        /// </remarks>
        private async Task<CreateGuard> ValidateNursingUnitAuthorityAsync(
            CreatePatientAssessmentRequest request)
        {
            if (IsKajianMedis(request.AssessmentType))
                return CreateGuard.Ok();

            var episodeId = request.InpEpisodeId.HasValue && request.InpEpisodeId.Value != Guid.Empty
                ? request.InpEpisodeId
                : await _inpatientClinicalContextService.FindOpenEpisodeIdAsync(request.EncounterId);

            if (!episodeId.HasValue || episodeId.Value == Guid.Empty)
                return CreateGuard.Ok();

            return await EnsureNursingUnitAuthorityAsync(request.AssessmentType, episodeId);
        }

        /// <summary>
        /// Penjagaan kewenangan menulis perawat pada pengkajian yang <b>sudah ada</b> -
        /// <c>BE-RWI-078</c>, <c>GUARD-INP-07</c> dan <c>GUARD-INP-08</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Dua penolakannya dijawab <c>403</c> keduanya, sesuai <c>AC-KEP-045</c> dan
        /// <c>AC-KEP-046</c>: pengguna tanpa pemetaan pegawai tidak dapat dinilai unitnya, dan
        /// tidak dapat dinilai berarti tidak berwenang - bukan sekadar data yang belum lengkap.
        /// </para>
        /// <para>
        /// Kajian medis dan pengkajian tanpa perawatan rawat inap dilewatkan, dengan alasan yang
        /// sama seperti pada jalur pembuatan.
        /// </para>
        /// </remarks>
        private async Task<CreateGuard> EnsureNursingUnitAuthorityAsync(
            PatientAssessmentType assessmentType,
            Guid? inpEpisodeId,
            CancellationToken cancellationToken = default)
        {
            // BE-RWI-088 / RLN3-CAP-34, api-contract.md 0.6.0 bagian 12.3. Kajian medis memang
            // BUKAN urusan kewenangan unit perawat — ia dijaga penjaga penulis tunggal
            // EnsureSoleAuthorAsync yang sekarang dipasang pada seluruh jalur ubah, selesaikan,
            // dan batalkan. Yang dicabut di sini hanya anggapan bahwa "jenis medis lolos begitu
            // saja"; penjagaannya berpindah tempat, tidak hilang.
            if (IsKajianMedis(assessmentType))
                return CreateGuard.Ok();

            if (!inpEpisodeId.HasValue || inpEpisodeId.Value == Guid.Empty)
                return CreateGuard.Ok();

            var employeeId = await _nursingActorService.ResolveEmployeeIdAsync(
                User, GetCurrentUserId(), cancellationToken);

            if (employeeId == null)
            {
                return CreateGuard.Fail(
                    InpatientClinicalContextService.PenolakanPerawatTanpaPegawai,
                    StatusCodes.Status403Forbidden,
                    KodeTanpaPegawai);
            }

            var bertugas = await _inpatientClinicalContextService.IsNurseOnDutyAtEpisodeAsync(
                inpEpisodeId.Value, employeeId.Value, DateTime.UtcNow, cancellationToken);

            return bertugas
                ? CreateGuard.Ok()
                : CreateGuard.Fail(
                    InpatientClinicalContextService.PenolakanPerawatUnitLain,
                    StatusCodes.Status403Forbidden,
                    KodeUnitLain);
        }

        /// <summary>
        /// Penjaga penulis tunggal atas satu pengkajian yang sudah ada — <c>BE-RWI-088</c>,
        /// <c>INV-DOK-14</c>, <c>FR-DOK-075</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Penulisnya dibaca dari <c>AssessmentByUserId</c>, dan bila kosong dari kolom audit
        /// <c>CreateBy</c>. Urutan itu mengikuti pendaftaran keutuhan yang sudah ada pada jalur
        /// penyelesaian, sehingga penulis yang dijaga di sini sama persis dengan penulis yang
        /// nanti tercatat sebagai penanda tangan.
        /// </para>
        /// <para>
        /// <b>Poliklinik, medical check-up, dan IGD tidak berubah.</b> Kunjungan tanpa perawatan
        /// rawat inap dijawab lolos.
        /// </para>
        /// </remarks>
        private async Task<CreateGuard> EnsureSoleAuthorAsync(
            TrxPatientAssessment entity,
            CancellationToken cancellationToken = default)
        {
            var penulis = entity.AssessmentByUserId.HasValue &&
                          entity.AssessmentByUserId.Value != Guid.Empty
                ? entity.AssessmentByUserId
                : entity.CreateBy;

            var penjaga = await _inpatientClinicalContextService.ResolveForAuthorEditAsync(
                entity.EncounterId,
                documentAuthorUserId: penulis,
                actorUserId: GetCurrentUserId(),
                expectedEpisodeId: entity.InpEpisodeId,
                cancellationToken: cancellationToken);

            if (!InpatientClinicalContextService.PerluDitolak(penjaga))
                return CreateGuard.Ok();

            return CreateGuard.Fail(
                penjaga.ErrorMessage ?? InpatientClinicalContextService.PenolakanBukanPenulisKonsep,
                penjaga.StatusCode,
                penjaga.StatusCode == StatusCodes.Status403Forbidden ? KodeBukanPenulis : null);
        }

        /// <summary>
        /// Penjagaan penanda perawatan - <c>VAL-DOK-26</c>, berlaku pada kedua cabang.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-044</c>. Keadaan perawatan sengaja tidak diperiksa di sini: perawatan yang
        /// masih <c>Draft</c> atau sudah ditutup hanya menutup pintu tanpa antrean, sedangkan
        /// kunjungan yang membawa antrean tetap tunduk pada penjagaan antrean yang lama.
        /// </remarks>
        private async Task<CreateGuard> ValidateInpatientMarkersAsync(
            CreatePatientAssessmentRequest request)
        {
            if (!request.InpEpisodeId.HasValue || request.InpEpisodeId.Value == Guid.Empty)
                return CreateGuard.Ok();

            var context = await _inpatientClinicalContextService.ResolveAsync(
                request.EncounterId,
                expectedEpisodeId: request.InpEpisodeId,
                forNewDocument: true);

            if (context.Outcome == InpatientClinicalContextOutcome.EpisodeMismatch)
                return CreateGuard.Fail(context.ErrorMessage!, context.StatusCode);

            if (context.Outcome == InpatientClinicalContextOutcome.NoInpatientEpisode)
                return CreateGuard.Fail("Perawatan rawat inap tidak sesuai dengan kunjungannya.");

            return CreateGuard.Ok();
        }

        /// <summary>
        /// Penjagaan <b>satu pengkajian awal keperawatan</b> per perawatan - <c>VAL-KEP-11</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-056</c>, <c>FR-KEP-006</c>, PRD 16.2 aturan 3. Pengkajian awal dan pengkajian
        /// ulang adalah dua catatan yang berbeda, dan nilai pengkajian awal tidak boleh hilang
        /// ketika perawat mengisi pengkajian esok harinya. Penjaganya bukan pada penyimpanan,
        /// melainkan pada pembuatan: pengkajian awal <b>kedua</b> ditolak dan diarahkan ke
        /// pengkajian ulang.
        /// </para>
        /// <para>
        /// <b>Hanya berlaku bagi pengkajian rawat inap.</b> Poliklinik, medical check-up, dan IGD
        /// tidak punya perawatan, sehingga aturan ini tidak menyentuh satu pun jalur lama —
        /// seluruh baris lama bernilai <c>Initial</c> dan justru itulah alasan penyaringnya
        /// memakai keberadaan perawatan, bukan jenis pengkajian saja.
        /// </para>
        /// <para>
        /// <b>Pengkajian awal yang dibatalkan tidak menghalangi.</b> Perawat yang salah memilih
        /// pasien membatalkan pengkajiannya, lalu membuat yang benar; menghitung baris yang
        /// dibatalkan akan mengunci perawatan itu selamanya. Alasan yang sama membuat aturan ini
        /// <b>tidak</b> dijaga unique index — index parsial pada tabel ini sengaja
        /// <b>non-unique</b>, dan hanya mempercepat pencarian (<c>02-backend-architecture.md</c>
        /// bagian 4.1).
        /// </para>
        /// </remarks>
        private async Task<CreateGuard> ValidateSingleInitialNursingAssessmentAsync(
            CreatePatientAssessmentRequest request)
        {
            if (request.AssessmentType != PatientAssessmentType.Initial)
                return CreateGuard.Ok();

            var episodeId = request.InpEpisodeId.HasValue && request.InpEpisodeId.Value != Guid.Empty
                ? request.InpEpisodeId
                : await _inpatientClinicalContextService.FindOpenEpisodeIdAsync(request.EncounterId);

            if (!episodeId.HasValue || episodeId.Value == Guid.Empty)
                return CreateGuard.Ok();

            var sudahAda = await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.InpEpisodeId == episodeId.Value &&
                    x.AssessmentType == PatientAssessmentType.Initial &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.AssessmentStatus != PatientAssessmentStatus.Cancelled);

            return sudahAda
                ? CreateGuard.Fail(PenolakanPengkajianAwalKedua, StatusCodes.Status409Conflict)
                : CreateGuard.Ok();
        }

        /// <summary>
        /// Penjagaan khusus <b>kajian medis</b>: kewenangan menulis, konteks perawatan, dan
        /// batas satu kajian medis awal yang berlaku per perawatan.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-045</c>, <c>CAP-022</c>, <c>AC-CAP022-02</c>, <c>VAL-DOK-01</c>,
        /// <c>VAL-DOK-05</c>. Tabel ini dipakai bersama pengkajian keperawatan, sehingga mesin
        /// hak akses hanya melihat <b>satu</b> sumber daya untuk dua jenis dokumen -
        /// <c>permission-audit-matrix.md</c> bagian 3. Pembedaannya karena itu wajib berada di
        /// sini, di dalam aturan bisnis.
        /// </para>
        /// <para>
        /// <b>Kewenangan diturunkan dari data, bukan dari nama peran.</b> Yang diperiksa adalah
        /// apakah pengguna yang sedang masuk benar-benar terhubung ke satu baris dokter yang
        /// aktif. Tidak ada pemeriksaan nama peran, nama jabatan, maupun <c>UserType</c>; siapa
        /// yang boleh memanggil endpoint ini tetap ditentukan admin lewat layar Akses Role.
        /// </para>
        /// <para>
        /// Batas satu kajian medis awal dihitung <b>per perawatan</b>, bukan per kunjungan, dan
        /// menghitung kajian yang sudah selesai maupun yang masih dikerjakan. Kajian yang
        /// dibatalkan tidak dihitung - pembatalan memang jalan keluar dari kajian yang salah.
        /// </para>
        /// </remarks>
        private async Task<CreateGuard> ValidateMedicalAssessmentRuleAsync(
            CreatePatientAssessmentRequest request)
        {
            if (!IsKajianMedis(request.AssessmentType))
                return CreateGuard.Ok();

            var doctorId = await ResolveCurrentDoctorIdAsync();

            if (!doctorId.HasValue)
                return CreateGuard.Fail(PenolakanKajianMedisBukanDokter, StatusCodes.Status403Forbidden);

            // BE-RWI-076 / GUARD-INP-05 dan GUARD-INP-06. Sebelum task ini konteks dibentuk
            // TANPA menyebut dokter pelaku, sehingga penjaga kewenangan di dalamnya tidak
            // pernah diuji: dokter mana pun yang memegang butir hak akses dapat menulis kajian
            // medis untuk pasien rawat inap siapa pun. Kajian medis tidak membawa waktu klinis
            // pada payload-nya — ia selalu lahir pada waktu penulisannya — sehingga atUtc
            // dibiarkan kosong dan penilaiannya memakai saat ini.
            var context = await _inpatientClinicalContextService.ResolveForDoctorWriteAsync(
                User,
                GetCurrentUserId(),
                request.EncounterId,
                expectedEpisodeId: request.InpEpisodeId,
                forNewDocument: true);

            if (!context.IsResolved)
            {
                return CreateGuard.Fail(
                    context.Outcome == InpatientClinicalContextOutcome.NoInpatientEpisode
                        ? "Pasien ini tidak sedang dirawat inap."
                        : context.ErrorMessage ?? "Konteks perawatan rawat inap tidak dapat dibentuk.",
                    context.StatusCode);
            }

            if (request.AssessmentType != PatientAssessmentType.MedicalInitial)
                return CreateGuard.Ok();

            var sudahAda = await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.InpEpisodeId == context.Context!.EpisodeId &&
                    x.AssessmentType == PatientAssessmentType.MedicalInitial &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.AssessmentStatus != PatientAssessmentStatus.Cancelled);

            if (sudahAda)
            {
                return CreateGuard.Fail(
                    "Perawatan ini sudah memiliki kajian medis awal. Lanjutkan kajian yang " +
                    "sudah ada, atau buat kajian medis ulang.");
            }

            return CreateGuard.Ok();
        }

        /// <summary>
        /// Menemukan baris dokter yang melekat pada pengguna yang sedang masuk.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-045</c>, <c>VAL-DOK-05</c>. Urutannya mengikuti pola yang sudah dipakai
        /// <c>DoctorQueueController.ResolveAllowedDoctorIdAsync</c>: klaim identitas dokter
        /// lebih dulu, lalu penautan lewat profil tenaga kerja, lalu surel. Tiga-tiganya
        /// bersandar pada <b>data</b> - tidak satu pun membaca nama peran.
        /// </para>
        /// <para>
        /// Mengembalikan kosong bila pengguna tidak terhubung ke dokter mana pun. Itulah yang
        /// membuat perawat ditolak <c>403</c> saat mencoba menulis kajian medis, tanpa satu
        /// baris pun kode yang menyebut kata "perawat".
        /// </para>
        /// </remarks>
        private async Task<Guid?> ResolveCurrentDoctorIdAsync()
        {
            var doctorIdClaim = User.FindFirstValue("doctor_id") ?? User.FindFirstValue("DoctorId");

            if (Guid.TryParse(doctorIdClaim, out var doctorIdFromClaim) &&
                doctorIdFromClaim != Guid.Empty)
            {
                var adaDokter = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == doctorIdFromClaim && !x.IsDelete && x.IsActive);

                if (adaDokter)
                    return doctorIdFromClaim;
            }

            var workforceClaim = User.FindFirstValue("workforce_profile_id")
                                 ?? User.FindFirstValue("WorkforceProfileId");

            Guid? workforceProfileId = Guid.TryParse(workforceClaim, out var dariKlaim) && dariKlaim != Guid.Empty
                ? dariKlaim
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

        /// <summary>
        /// Bagian kajian medis yang masih kosong saat kajian hendak diselesaikan.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-045</c>, <c>VAL-DOK-10</c>. Aturan yang disetujui menyebut tiga bagian -
        /// keluhan utama, pemeriksaan, dan rencana - ditambah diagnosis pada <c>VAL-DOK-11</c>.
        /// </para>
        /// <para>
        /// <b>Kelima bagian kini benar-benar diperiksa.</b> Pemeriksaan fisik, rencana terapi,
        /// dan diagnosis kerja sebelumnya tidak punya kolom sama sekali, sehingga daftar ini
        /// hanya berisi dua baris pertama dan <c>VAL-DOK-11</c> tidak dapat ditegakkan.
        /// Ketiganya ditambahkan ke <c>TrxPatientAssessment</c> sebagai keputusan struktur
        /// Product/Domain 5 September 2026 - jalan A pada
        /// <c>02-backend-architecture.md</c> bagian 4.2, dengan <c>data/data-dictionary.md</c>
        /// bagian 3 diperbarui mengikuti.
        /// </para>
        /// <para>
        /// Urutan daftarnya mengikuti urutan dokter mengisi kajian: keluhan, riwayat,
        /// pemeriksaan, diagnosis, lalu rencana. Yang dikembalikan adalah nama bagian dalam
        /// bahasa layar, bukan nama kolom, karena kalimatnya dibaca dokter.
        /// </para>
        /// </remarks>
        /// <param name="entity">Kajian yang hendak diselesaikan.</param>
        /// <param name="adaDaftarMasalahTerstruktur">
        /// Benar bila perawatan yang menaungi kajian ini sudah memiliki setidaknya satu
        /// diagnosis terstruktur berkode ICD yang masih berlaku - <c>BE-RWI-068</c>.
        /// </param>
        private static List<string> BagianKajianMedisYangKosong(
            TrxPatientAssessment entity,
            bool adaDaftarMasalahTerstruktur)
        {
            var kosong = new List<string>();

            if (string.IsNullOrWhiteSpace(entity.ChiefComplaint))
                kosong.Add("keluhan utama");

            if (string.IsNullOrWhiteSpace(entity.CurrentIllnessHistory))
                kosong.Add("riwayat penyakit sekarang");

            if (string.IsNullOrWhiteSpace(entity.PhysicalExamination))
                kosong.Add("pemeriksaan fisik");

            // BE-RWI-068 / VAL-DOK-11, dipertajam pada kontrak 0.4.0 - bukan diperketat.
            // Bagian ini terbaca kosong hanya ketika kedua bentuk daftar masalah sama-sama
            // kosong. Kajian lama yang mengisi teks bebas tetap lolos persis seperti dulu.
            if (string.IsNullOrWhiteSpace(entity.WorkingDiagnosis) &&
                !adaDaftarMasalahTerstruktur)
                kosong.Add("diagnosis kerja");

            if (string.IsNullOrWhiteSpace(entity.TherapyPlan))
                kosong.Add("rencana terapi");

            return kosong;
        }

        /// <summary>
        /// Menjawab apakah perawatan yang menaungi kajian ini sudah memiliki daftar masalah
        /// terstruktur - <c>BE-RWI-068</c>, <c>CAP-022</c> aturan 5.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Yang dihitung adalah baris <c>TrxPatientDiagnosis</c> milik perawatan yang sama,
        /// yang belum dibatalkan dan belum dinyatakan teratasi. Masalah yang sudah teratasi
        /// memang bukan lagi daftar masalah yang berlaku, sehingga tidak boleh menutup
        /// pemeriksaan kelengkapan.
        /// </para>
        /// <para>
        /// Kajian yang tidak menaungi perawatan - poliklinik, medical check-up, IGD - selalu
        /// menjawab salah, sehingga perilakunya tidak bergeser satu langkah pun.
        /// </para>
        /// </remarks>
        private async Task<bool> AdaDaftarMasalahTerstrukturAsync(TrxPatientAssessment entity)
        {
            if (!entity.InpEpisodeId.HasValue || entity.InpEpisodeId.Value == Guid.Empty)
                return false;

            return await _dbContext.Set<TrxPatientDiagnosis>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.InpEpisodeId == entity.InpEpisodeId.Value &&
                    !x.IsDelete &&
                    x.DiagnosisStatus != PatientDiagnosisStatus.Cancelled &&
                    x.DiagnosisStatus != PatientDiagnosisStatus.Resolved);
        }

        private async Task<string> GenerateAssessmentNumberAsync(DateTime now)
        {
            var prefix = $"ASM-{now:yyyyMMdd}";
            var countToday = await _dbContext.Set<TrxPatientAssessment>()
                .CountAsync(x => x.AssessmentNumber.StartsWith(prefix));

            return $"{prefix}-{countToday + 1:D5}";
        }

        private static CalculatedAssessmentValue CalculateAssessmentValues(
            CreatePatientAssessmentRequest request,
            bool pengkajianRawatInap)
        {
            var bmi = CalculateBmi(request.Weight, request.Height);
            var map = CalculateMap(request.BloodPressureSystolic, request.BloodPressureDiastolic);
            var mapStatus = CalculateMapStatus(map);
            var ewsScore = CalculateEwsScore(
                request.RespiratoryRate,
                request.OxygenSaturation,
                request.Temperature,
                request.BloodPressureSystolic,
                request.PulseRate,
                request.ConsciousnessStatus);

            var ewsRiskLevel = CalculateEwsRiskLevel(ewsScore);
            var ewsMonitoringRecommendation = GetEwsMonitoringRecommendation(ewsRiskLevel, ewsScore);

            // BE-RWI-109 / FR-KEP-043, RWI-FACT-036. Jalur rawat inap tidak lagi menghitung risiko jatuh
            // dari dua centang dan batas di kode. Kategori dan skornya lahir dari instrumen berversi
            // lewat NursingAssessmentDocumentService; sebelum itu keadaannya "belum dikaji".
            var hasFallRisk = request.HasFallRisk || request.HasAtaxia || request.HasPosturalInstability;
            int? fallRiskScore = null;
            var fallRiskStatus = FallRiskStatus.Unknown;

            if (!pengkajianRawatInap)
            {
                fallRiskScore = CalculateFallRiskScore(hasFallRisk, request.HasAtaxia, request.HasPosturalInstability);
                fallRiskStatus = CalculateFallRiskStatus(hasFallRisk, fallRiskScore);
            }

            return new CalculatedAssessmentValue
            {
                BMI = bmi,
                MeanArterialPressure = map,
                MapStatus = mapStatus,
                EarlyWarningScore = ewsScore,
                EwsRiskLevel = ewsRiskLevel,
                EwsMonitoringRecommendation = ewsMonitoringRecommendation,
                FallRiskScore = fallRiskScore,
                FallRiskStatus = fallRiskStatus
            };
        }

        private static CalculatedAssessmentValue CalculateAssessmentValues(
            UpdatePatientAssessmentRequest request,
            bool pengkajianRawatInap)
        {
            var bmi = CalculateBmi(request.Weight, request.Height);
            var map = CalculateMap(request.BloodPressureSystolic, request.BloodPressureDiastolic);
            var mapStatus = CalculateMapStatus(map);
            var ewsScore = CalculateEwsScore(
                request.RespiratoryRate,
                request.OxygenSaturation,
                request.Temperature,
                request.BloodPressureSystolic,
                request.PulseRate,
                request.ConsciousnessStatus);

            var ewsRiskLevel = CalculateEwsRiskLevel(ewsScore);
            var ewsMonitoringRecommendation = GetEwsMonitoringRecommendation(ewsRiskLevel, ewsScore);

            // BE-RWI-109 / FR-KEP-043, RWI-FACT-036. Jalur rawat inap tidak lagi menghitung risiko jatuh
            // dari dua centang dan batas di kode. Kategori dan skornya lahir dari instrumen berversi
            // lewat NursingAssessmentDocumentService; sebelum itu keadaannya "belum dikaji".
            var hasFallRisk = request.HasFallRisk || request.HasAtaxia || request.HasPosturalInstability;
            int? fallRiskScore = null;
            var fallRiskStatus = FallRiskStatus.Unknown;

            if (!pengkajianRawatInap)
            {
                fallRiskScore = CalculateFallRiskScore(hasFallRisk, request.HasAtaxia, request.HasPosturalInstability);
                fallRiskStatus = CalculateFallRiskStatus(hasFallRisk, fallRiskScore);
            }

            return new CalculatedAssessmentValue
            {
                BMI = bmi,
                MeanArterialPressure = map,
                MapStatus = mapStatus,
                EarlyWarningScore = ewsScore,
                EwsRiskLevel = ewsRiskLevel,
                EwsMonitoringRecommendation = ewsMonitoringRecommendation,
                FallRiskScore = fallRiskScore,
                FallRiskStatus = fallRiskStatus
            };
        }

        private static decimal? CalculateBmi(decimal? weightKg, decimal? heightCm)
        {
            if (!weightKg.HasValue || !heightCm.HasValue || heightCm.Value <= 0)
                return null;

            var heightMeter = heightCm.Value / 100;
            var bmi = weightKg.Value / (heightMeter * heightMeter);

            return Math.Round(bmi, 2);
        }

        private static decimal? CalculateMap(int? systolic, int? diastolic)
        {
            if (!systolic.HasValue || !diastolic.HasValue)
                return null;

            var map = diastolic.Value + ((systolic.Value - diastolic.Value) / 3m);

            return Math.Round(map, 2);
        }

        private static MapStatus CalculateMapStatus(decimal? map)
        {
            if (!map.HasValue)
                return MapStatus.Unknown;

            if (map.Value < 60)
                return MapStatus.Hypotension;

            if (map.Value > 100)
                return MapStatus.Hypertension;

            return MapStatus.Normal;
        }

        private static int? CalculateEwsScore(
            int? respiratoryRate,
            decimal? oxygenSaturation,
            decimal? temperature,
            int? systolicBloodPressure,
            int? pulseRate,
            ConsciousnessStatus consciousnessStatus)
        {
            var hasAnyValue =
                respiratoryRate.HasValue ||
                oxygenSaturation.HasValue ||
                temperature.HasValue ||
                systolicBloodPressure.HasValue ||
                pulseRate.HasValue ||
                consciousnessStatus != ConsciousnessStatus.Unknown;

            if (!hasAnyValue)
                return null;

            var score = 0;

            if (respiratoryRate.HasValue)
            {
                if (respiratoryRate.Value <= 8) score += 3;
                else if (respiratoryRate.Value <= 11) score += 1;
                else if (respiratoryRate.Value <= 20) score += 0;
                else if (respiratoryRate.Value <= 24) score += 2;
                else score += 3;
            }

            if (oxygenSaturation.HasValue)
            {
                if (oxygenSaturation.Value <= 91) score += 3;
                else if (oxygenSaturation.Value <= 93) score += 2;
                else if (oxygenSaturation.Value <= 95) score += 1;
                else score += 0;
            }

            if (temperature.HasValue)
            {
                if (temperature.Value <= 35.0m) score += 3;
                else if (temperature.Value <= 36.0m) score += 1;
                else if (temperature.Value <= 38.0m) score += 0;
                else if (temperature.Value <= 39.0m) score += 1;
                else score += 2;
            }

            if (systolicBloodPressure.HasValue)
            {
                if (systolicBloodPressure.Value <= 90) score += 3;
                else if (systolicBloodPressure.Value <= 100) score += 2;
                else if (systolicBloodPressure.Value <= 110) score += 1;
                else if (systolicBloodPressure.Value <= 219) score += 0;
                else score += 3;
            }

            if (pulseRate.HasValue)
            {
                if (pulseRate.Value <= 40) score += 3;
                else if (pulseRate.Value <= 50) score += 1;
                else if (pulseRate.Value <= 90) score += 0;
                else if (pulseRate.Value <= 110) score += 1;
                else if (pulseRate.Value <= 130) score += 2;
                else score += 3;
            }

            if (consciousnessStatus != ConsciousnessStatus.Unknown &&
                consciousnessStatus != ConsciousnessStatus.ComposMentis)
            {
                score += 3;
            }

            return score;
        }

        private static EwsRiskLevel CalculateEwsRiskLevel(int? ewsScore)
        {
            if (!ewsScore.HasValue)
                return EwsRiskLevel.Unknown;

            if (ewsScore.Value >= 7)
                return EwsRiskLevel.Critical;

            if (ewsScore.Value >= 5)
                return EwsRiskLevel.High;

            if (ewsScore.Value >= 3)
                return EwsRiskLevel.Medium;

            return EwsRiskLevel.Low;
        }

        private static string? GetEwsMonitoringRecommendation(EwsRiskLevel riskLevel, int? ewsScore)
        {
            if (!ewsScore.HasValue)
                return null;

            return riskLevel switch
            {
                EwsRiskLevel.Low => "Monitoring rutin sesuai kondisi klinis pasien.",
                EwsRiskLevel.Medium => "Monitoring ulang tanda vital dan evaluasi klinis berkala.",
                EwsRiskLevel.High => "Monitoring lebih sering dan informasikan dokter penanggung jawab.",
                EwsRiskLevel.Critical => "Pemantauan terus menerus tanda-tanda vital, pertimbangkan eskalasi klinis segera.",
                _ => null
            };
        }

        private static int? CalculateFallRiskScore(bool hasFallRisk, bool hasAtaxia, bool hasPosturalInstability)
        {
            if (!hasFallRisk)
                return null;

            var score = 0;

            if (hasAtaxia)
                score += 1;

            if (hasPosturalInstability)
                score += 1;

            return score;
        }

        /// <summary>
        /// Kategori risiko jatuh jalur <b>non-rawat-inap</b> — poliklinik, medical check-up, IGD.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-109</c> kriteria 4. Perhitungan lama dipertahankan tanpa perubahan hasil sampai
        /// pemilik <c>rawat-jalan</c> memutuskan sendiri (<c>RWI-DEC-152</c>). Jalur rawat inap tidak lagi
        /// memanggil metode ini: kategorinya lahir dari pita instrumen berversi.
        /// </para>
        /// <para>
        /// Hasil jalur lama sama persis dengan sebelum task ini: tanpa penanda risiko → <c>NoRisk</c>;
        /// dengan penanda, skor 2 → <c>HighRisk</c>, skor 1 → <c>MediumRisk</c>, skor 0 → <c>LowRisk</c>.
        /// </para>
        /// </remarks>
        private static FallRiskStatus CalculateFallRiskStatus(
            bool hasFallRisk,
            int? fallRiskScore)
        {
            if (!hasFallRisk)
                return FallRiskStatus.NoRisk;

            if (!fallRiskScore.HasValue)
                return FallRiskStatus.Unknown;

            if (fallRiskScore.Value >= 2)
                return FallRiskStatus.HighRisk;

            if (fallRiskScore.Value == 1)
                return FallRiskStatus.MediumRisk;

            return FallRiskStatus.LowRisk;
        }

        private static void NormalizeAssessmentData(TrxPatientAssessment entity)
        {
            if (!entity.IsUsingOxygen)
            {
                entity.OxygenSupportType = OxygenSupportType.None;
                entity.OxygenFlowRate = null;
                entity.OxygenSupportNote = null;
            }

            if (!entity.HasPain)
            {
                entity.PainScale = null;
                entity.PainTrigger = null;
                entity.PainQuality = null;
                entity.PainLocation = null;
                entity.PainFrequency = null;
                entity.PainManagement = null;
                entity.PainNote = null;
            }

            if (!entity.HasHereditaryDisease)
            {
                entity.HereditaryDiseaseNote = null;
            }

            if (!entity.HasAllergy)
            {
                entity.AllergyType = null;
                entity.AllergyNote = null;
            }

            // BE-RWI-109. Dokumen V2 rawat inap tidak memakai dua centang lama; catatan pencegahan
            // Resiko Jatuh dan kategori dari instrumen tidak boleh ikut dirapikan menjadi kosong.
            if (!entity.HasFallRisk &&
                !NursingAssessmentDocumentService.IsInpatientV2(entity.InpEpisodeId, entity.AssessmentType))
            {
                entity.HasAtaxia = false;
                entity.HasPosturalInstability = false;
                entity.FallRiskScore = null;

                // BE-RWI-056. Perapian tidak boleh menghidupkan kembali penyamaan yang baru saja
                // dicabut: "belum diisi" dibiarkan apa adanya, sedangkan kategori apa pun yang
                // dinyatakan perawat dirapikan menjadi "tidak berisiko" karena tidak ada satu pun
                // penanda risiko yang menyertainya.
                if (entity.FallRiskStatus != FallRiskStatus.Unknown)
                    entity.FallRiskStatus = FallRiskStatus.NoRisk;

                entity.FallRiskNote = null;
            }
        }

        private IQueryable<TrxPatientAssessment> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPatientAssessment>()
                .Include(x => x.Encounter)
                .Include(x => x.Queue)
                    .ThenInclude(x => x.Doctor)
                .Include(x => x.Patient)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.AssessmentByUser)
                .Include(x => x.CompletedByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPatientAssessment> ApplySorting(
            IQueryable<TrxPatientAssessment> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "assessmentDateTime").ToLowerInvariant() switch
            {
                "assessmentnumber" => isDesc ? query.OrderByDescending(x => x.AssessmentNumber) : query.OrderBy(x => x.AssessmentNumber),
                "assessmentstatus" => isDesc ? query.OrderByDescending(x => x.AssessmentStatus) : query.OrderBy(x => x.AssessmentStatus),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc ? query.OrderByDescending(x => x.AssessmentDateTime) : query.OrderBy(x => x.AssessmentDateTime)
            };
        }

        private static PatientAssessmentResponse ToResponse(TrxPatientAssessment x)
        {
            return new PatientAssessmentResponse
            {
                Id = x.Id,
                AssessmentNumber = x.AssessmentNumber,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                QueueId = x.QueueId,
                QueueCode = x.Queue != null ? x.Queue.QueueCode : string.Empty,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Queue != null && x.Queue.Doctor != null ? x.Queue.Doctor.FullName : null,
                InpEpisodeId = x.InpEpisodeId,
                AssessmentType = x.AssessmentType,
                DueAt = x.DueAt,
                PolicyId = x.PolicyId,
                AssessmentDateTime = x.AssessmentDateTime,
                AssessmentStatus = x.AssessmentStatus,
                AssessmentByUserId = x.AssessmentByUserId,
                AssessmentByUserName = x.AssessmentByUser != null ? x.AssessmentByUser.DisplayName : null,
                ChiefComplaint = x.ChiefComplaint,

                BloodPressureSystolic = x.BloodPressureSystolic,
                BloodPressureDiastolic = x.BloodPressureDiastolic,
                PulseRate = x.PulseRate,
                IsPulseReadable = x.IsPulseReadable,
                RespiratoryRate = x.RespiratoryRate,
                Temperature = x.Temperature,
                OxygenSaturation = x.OxygenSaturation,
                IsUsingOxygen = x.IsUsingOxygen,
                OxygenSupportType = x.OxygenSupportType,
                OxygenFlowRate = x.OxygenFlowRate,
                ConsciousnessStatus = x.ConsciousnessStatus,
                Weight = x.Weight,
                Height = x.Height,
                BMI = x.BMI,
                MeanArterialPressure = x.MeanArterialPressure,
                MapStatus = x.MapStatus,
                EarlyWarningScore = x.EarlyWarningScore,
                EwsRiskLevel = x.EwsRiskLevel,

                HasPain = x.HasPain,
                PainScale = x.PainScale,
                HasHereditaryDisease = x.HasHereditaryDisease,
                HasAllergy = x.HasAllergy,
                AllergyType = x.AllergyType,

                HasBcgImmunization = x.HasBcgImmunization,
                HasHepatitisBImmunization = x.HasHepatitisBImmunization,
                HasPolioImmunization = x.HasPolioImmunization,
                HasDptImmunization = x.HasDptImmunization,
                HasMeaslesImmunization = x.HasMeaslesImmunization,

                AppetiteStatus = x.AppetiteStatus,
                HasNausea = x.HasNausea,
                HasVomiting = x.HasVomiting,

                HasFallRisk = x.HasFallRisk,
                FallRiskStatus = x.FallRiskStatus,
                NutritionRiskStatus = x.NutritionRiskStatus,
                FunctionalStatus = x.FunctionalStatus,

                NurseNote = x.NurseNote,

                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime,
                PainAssessmentState = x.PainAssessmentState,
                PainReassessmentDueAt = x.PainReassessmentDueAt,
                VitalSignId = x.VitalSignId
            };
        }

        private static PatientAssessmentDetailResponse ToDetailResponse(TrxPatientAssessment x)
        {
            return new PatientAssessmentDetailResponse
            {
                Id = x.Id,
                AssessmentNumber = x.AssessmentNumber,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                QueueId = x.QueueId,
                QueueCode = x.Queue != null ? x.Queue.QueueCode : string.Empty,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Queue != null && x.Queue.Doctor != null ? x.Queue.Doctor.FullName : null,
                InpEpisodeId = x.InpEpisodeId,
                AssessmentType = x.AssessmentType,
                DueAt = x.DueAt,
                PolicyId = x.PolicyId,
                AssessmentDateTime = x.AssessmentDateTime,
                AssessmentStatus = x.AssessmentStatus,
                AssessmentByUserId = x.AssessmentByUserId,
                AssessmentByUserName = x.AssessmentByUser != null ? x.AssessmentByUser.DisplayName : null,
                ChiefComplaint = x.ChiefComplaint,
                CurrentIllnessHistory = x.CurrentIllnessHistory,
                MedicationHistory = x.MedicationHistory,
                PhysicalExamination = x.PhysicalExamination,
                WorkingDiagnosis = x.WorkingDiagnosis,
                TherapyPlan = x.TherapyPlan,

                BloodPressureSystolic = x.BloodPressureSystolic,
                BloodPressureDiastolic = x.BloodPressureDiastolic,
                PulseRate = x.PulseRate,
                IsPulseReadable = x.IsPulseReadable,
                RespiratoryRate = x.RespiratoryRate,
                Temperature = x.Temperature,
                OxygenSaturation = x.OxygenSaturation,
                IsUsingOxygen = x.IsUsingOxygen,
                OxygenSupportType = x.OxygenSupportType,
                OxygenFlowRate = x.OxygenFlowRate,
                OxygenSupportNote = x.OxygenSupportNote,
                ConsciousnessStatus = x.ConsciousnessStatus,
                Weight = x.Weight,
                Height = x.Height,
                BMI = x.BMI,
                MeanArterialPressure = x.MeanArterialPressure,
                MapStatus = x.MapStatus,
                EarlyWarningScore = x.EarlyWarningScore,
                EwsRiskLevel = x.EwsRiskLevel,
                EwsMonitoringRecommendation = x.EwsMonitoringRecommendation,

                HasPain = x.HasPain,
                PainScale = x.PainScale,
                PainTrigger = x.PainTrigger,
                PainQuality = x.PainQuality,
                PainLocation = x.PainLocation,
                PainFrequency = x.PainFrequency,
                PainManagement = x.PainManagement,
                PainNote = x.PainNote,

                HasHereditaryDisease = x.HasHereditaryDisease,
                HereditaryDiseaseNote = x.HereditaryDiseaseNote,

                HasAllergy = x.HasAllergy,
                AllergyType = x.AllergyType,
                AllergyNote = x.AllergyNote,

                HasBcgImmunization = x.HasBcgImmunization,
                HasHepatitisBImmunization = x.HasHepatitisBImmunization,
                HasPolioImmunization = x.HasPolioImmunization,
                HasDptImmunization = x.HasDptImmunization,
                HasMeaslesImmunization = x.HasMeaslesImmunization,
                ImmunizationNote = x.ImmunizationNote,

                AppetiteStatus = x.AppetiteStatus,
                HasNausea = x.HasNausea,
                HasVomiting = x.HasVomiting,
                NutritionRiskStatus = x.NutritionRiskStatus,
                NutritionRiskScore = x.NutritionRiskScore,
                NutritionNote = x.NutritionNote,

                HasFallRisk = x.HasFallRisk,
                HasAtaxia = x.HasAtaxia,
                HasPosturalInstability = x.HasPosturalInstability,
                FallRiskStatus = x.FallRiskStatus,
                FallRiskScore = x.FallRiskScore,
                FallRiskNote = x.FallRiskNote,

                FunctionalStatus = x.FunctionalStatus,
                FunctionalNote = x.FunctionalNote,
                PsychosocialNote = x.PsychosocialNote,
                EducationNote = x.EducationNote,
                NurseNote = x.NurseNote,

                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                CompletedByUserId = x.CompletedByUserId,
                CompletedByUserName = x.CompletedByUser != null ? x.CompletedByUser.DisplayName : null,
                CancelledAt = x.CancelledAt,
                CancelledByUserId = x.CancelledByUserId,
                CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName : null,
                CancelReason = x.CancelReason,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime,
                PainAssessmentState = x.PainAssessmentState,
                PainReassessmentDueAt = x.PainReassessmentDueAt,
                VitalSignId = x.VitalSignId
            };
        }

        // =====================================================================
        // BE-RWI-106 — pembantu enum satu sumber dan penjaga baca detail
        // =====================================================================

        /// <summary>
        /// Toleransi perbandingan jejak waktu. PostgreSQL menyimpan mikrodetik, .NET menyimpan
        /// 100 nanodetik; satu milidetik cukup menutup selisih pembulatan tanpa meloloskan
        /// perubahan orang lain yang berselang detik.
        /// </summary>
        private static readonly TimeSpan ToleransiJejakWaktu = TimeSpan.FromMilliseconds(1);

        /// <summary>
        /// Menegakkan <c>BE-RWI-106</c> kriteria 4: penyuntingan pengkajian keperawatan rawat inap
        /// wajib berangkat dari pembacaan detail terbaru.
        /// </summary>
        /// <remarks>
        /// Wajib hanya untuk pengkajian keperawatan yang menempel pada perawatan rawat inap. Jalur
        /// poliklinik, medical check-up, IGD, dan kajian medis tidak berubah perilakunya: tanpa
        /// <c>ExpectedUpdateDate</c> tetap diterima, dan bila dikirim tetap diperiksa.
        /// </remarks>
        private static CreateGuard PeriksaBacaDetailSebelumSunting(
            TrxPatientAssessment entity,
            DateTime? expectedUpdateDate)
        {
            var wajib = entity.InpEpisodeId.HasValue &&
                        entity.InpEpisodeId.Value != Guid.Empty &&
                        !IsKajianMedis(entity.AssessmentType);

            if (!expectedUpdateDate.HasValue)
            {
                return wajib
                    ? CreateGuard.Fail(
                        "Muat ulang detail pengkajian sebelum menyunting. Perubahan tidak " +
                        "disimpan supaya isian yang tidak tampil di daftar tidak terhapus.",
                        StatusCodes.Status400BadRequest,
                        "DETAIL_NOT_LOADED")
                    : CreateGuard.Ok();
            }

            var tersimpan = entity.UpdateDateTime ?? entity.CreateDateTime;
            var dikirim = expectedUpdateDate.Value.Kind == DateTimeKind.Local
                ? expectedUpdateDate.Value.ToUniversalTime()
                : expectedUpdateDate.Value;

            var selisih = (tersimpan - dikirim).Duration();

            return selisih <= ToleransiJejakWaktu
                ? CreateGuard.Ok()
                : CreateGuard.Fail(
                    "Pengkajian sudah diubah pengguna lain. Muat ulang detail sebelum menyunting.",
                    StatusCodes.Status409Conflict,
                    "STALE_ASSESSMENT");
        }

        /// <summary>
        /// Mengembalikan kalimat penolakan untuk enum pertama yang nilainya di luar daftar sah,
        /// atau <c>null</c> bila seluruhnya sah — <c>BE-RWI-106</c> kriteria 1.
        /// </summary>
        private static string? TemukanEnumTidakSah(
            PatientAssessmentType? assessmentType,
            OxygenSupportType oxygenSupportType,
            ConsciousnessStatus consciousnessStatus,
            AppetiteStatus appetiteStatus,
            NutritionRiskStatus nutritionRiskStatus,
            FallRiskStatus fallRiskStatus,
            FunctionalStatus functionalStatus)
        {
            return (assessmentType.HasValue ? PeriksaEnum(assessmentType.Value, "jenis pengkajian") : null)
                ?? PeriksaEnum(oxygenSupportType, "jenis alat bantu oksigen")
                ?? PeriksaEnum(consciousnessStatus, "kesadaran")
                ?? PeriksaEnum(appetiteStatus, "nafsu makan")
                ?? PeriksaEnum(nutritionRiskStatus, "risiko gizi")
                ?? PeriksaEnum(fallRiskStatus, "kategori risiko jatuh")
                ?? PeriksaEnum(functionalStatus, "status fungsional");
        }

        /// <summary>
        /// Kalimat penolakan satu enum yang tidak dikenal, menyebut seluruh nilai sahnya.
        /// </summary>
        /// <remarks>
        /// Contoh: status fungsional <c>4</c> → "Nilai status fungsional (4) tidak dikenal. Nilai
        /// yang sah: 0 = Unknown, 1 = Independent, 2 = NeedPartialAssistance, 3 = FullyDependent."
        /// </remarks>
        private static string? PeriksaEnum<TEnum>(TEnum value, string label)
            where TEnum : struct, Enum
        {
            if (Enum.IsDefined(value))
                return null;

            var sah = string.Join(", ", Enum.GetValues<TEnum>()
                .Select(x => $"{Convert.ToInt32(x)} = {x}"));

            return $"Nilai {label} ({Convert.ToInt32(value)}) tidak dikenal. Nilai yang sah: {sah}.";
        }

        private static List<PatientAssessmentEnumOptionResponse> OpsiEnum<TEnum>(
            Func<TEnum, string> label)
            where TEnum : struct, Enum
        {
            return Enum.GetValues<TEnum>()
                .Select(x => new PatientAssessmentEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Code = x.ToString(),
                    Label = label(x)
                })
                .ToList();
        }

        /// <summary>Label jenis pengkajian sesuai nama menu ruang kerja keperawatan dan dokter.</summary>
        internal static string LabelJenisPengkajian(PatientAssessmentType type) => type switch
        {
            PatientAssessmentType.Initial => "Kajian Umum — awal",
            PatientAssessmentType.Reassessment => "Kajian Umum — ulang",
            PatientAssessmentType.DailyReassessment => "Pengkajian ulang harian (tidak dipakai jalur V2)",
            PatientAssessmentType.DischargePlanning => "Perencanaan Pulang",
            PatientAssessmentType.MedicalInitial => "Kajian medis awal",
            PatientAssessmentType.MedicalReassessment => "Kajian medis ulang",
            PatientAssessmentType.FallRisk => "Resiko Jatuh",
            PatientAssessmentType.PainMonitoring => "Monitoring Nyeri",
            PatientAssessmentType.EducationAssessment => "Assesment Edukasi",
            _ => type.ToString()
        };

        private static string LabelRisiko(NutritionRiskStatus status) => status switch
        {
            NutritionRiskStatus.Unknown => "Belum dikaji",
            NutritionRiskStatus.NoRisk => "Tidak berisiko",
            NutritionRiskStatus.LowRisk => "Risiko rendah",
            NutritionRiskStatus.MediumRisk => "Risiko sedang",
            NutritionRiskStatus.HighRisk => "Risiko tinggi",
            _ => status.ToString()
        };

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var id)
                ? id
                : Guid.Empty;
        }

        private class CalculatedAssessmentValue
        {
            public decimal? BMI { get; set; }
            public decimal? MeanArterialPressure { get; set; }
            public MapStatus MapStatus { get; set; }
            public int? EarlyWarningScore { get; set; }
            public EwsRiskLevel EwsRiskLevel { get; set; }
            public string? EwsMonitoringRecommendation { get; set; }
            public int? FallRiskScore { get; set; }
            public FallRiskStatus FallRiskStatus { get; set; }
        }
    }
}
