using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using BloodGroupExamPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs.BloodGroupExamListDto>;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Controllers
{
    /// <summary>
    /// Layar pemeriksaan golongan darah Bank Darah — sampel diambil, hasil dicatat, hasil
    /// divalidasi, dan perbedaan hasil diselesaikan validator klinis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Inilah sumber sah golongan darah pasien</b> (<c>DEC-BD-015</c>). Bukan
    /// <c>MstPatient.BloodType</c>, yang merupakan data induk administratif hasil wawancara
    /// pendaftaran dan tidak pernah menjadi sumber klinis (<c>INV-BD-014</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Dua butir hak akses, bukan satu</b> (<c>DEC-BD-039</c>). <c>Validate</c> menjaga
    /// validasi hasil rutin dan boleh dipegang petugas BDRS yang ditunjuk;
    /// <c>ResolveConflict</c> menjaga penyelesaian perbedaan dan hanya dipegang validator
    /// klinis. Satu butir yang menjaga keduanya akan membuat siapa pun yang boleh memvalidasi
    /// hasil rutin otomatis boleh menutup konflik — dan penutupan konflik adalah keputusan
    /// klinis terberat pada alur ini. Pemisahannya ditegakkan dua butir yang benar-benar
    /// berbeda di layar Akses Role, bukan oleh pemeriksaan nama peran di dalam kode.
    /// </para>
    ///
    /// <para>
    /// <b>Contoh alur nyata.</b> Pasien punya hasil sah O Positif dari bulan lalu. Hari ini
    /// sampel baru diperiksa dan hasilnya A Positif, lalu divalidasi. Sistem <b>tidak memilih</b>
    /// mana yang benar: kedua hasil ditahan, pasien tercatat tidak punya golongan darah sah, dan
    /// setiap gerbang klinis yang menuntutnya tertutup. Yang membukanya kembali hanya
    /// pemeriksaan ulang yang tervalidasi, lalu validator klinis menyatakan hasil mana yang
    /// berlaku (<c>DEC-BD-031</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Penyelesaian konflik ada di layar ini</b>, bukan sebagai daftar kerja keempat
    /// (<c>DEC-BD-033</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Bentuk transaksi, bukan master data.</b> Karena itu sengaja tidak ada
    /// <c>GET /options</c>, tidak ada <c>PATCH /{id}/status</c> generik, dan tidak ada
    /// <c>DELETE /{id}</c>: pemeriksaan yang sudah terjadi tidak dihapus, dan statusnya
    /// berpindah karena kejadian bernama.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/blood-bank-management/blood-group-exams")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_BLOOD_BANK_MANAGEMENT",
        moduleName: "Health Service Blood Bank Management",
        displayName: "Blood Group Exam",
        AreaName = "HealthServices",
        ControllerName = "BloodGroupExam",
        Description = "Mencatat, memvalidasi, dan menyelesaikan perbedaan hasil golongan darah",
        SortOrder = 1
    )]
    [Tags("Health Services / Blood Bank Management / Blood Group Exam")]
    public class BbkBloodGroupExamController : ControllerBase
    {
        private const string LogCategory = "HealthServices.BloodBankManagement.BloodGroupExam";

        private readonly BbkBloodGroupExamService _bloodGroupExamService;
        private readonly LoggerService _loggerService;

        public BbkBloodGroupExamController(
            BbkBloodGroupExamService bloodGroupExamService,
            LoggerService loggerService)
        {
            _bloodGroupExamService = bloodGroupExamService;
            _loggerService = loggerService;
        }

        /// <summary>Konfigurasi penyaring dan pengurutan untuk halaman pemeriksaan.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<BloodGroupExamFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Group Exam", Description = "Melihat konfigurasi penyaring pemeriksaan golongan darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodGroupExam", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<BloodGroupExamFilterMetadataResponse>.Ok(
                BbkBloodGroupExamService.BuildFilterMetadata(),
                "Konfigurasi penyaring pemeriksaan golongan darah berhasil diambil."));
        }

        /// <summary>Ringkasan jumlah pemeriksaan per status, termasuk perbedaan yang tertahan.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<BloodGroupExamSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Group Exam", Description = "Melihat ringkasan pemeriksaan golongan darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodGroupExam", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var summary = await _bloodGroupExamService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<BloodGroupExamSummaryResponse>.Ok(
                summary,
                summary.PatientWithHeldConflict > 0
                    ? $"Ringkasan berhasil diambil. Perhatian: {summary.PatientWithHeldConflict} pasien sedang tidak punya golongan darah sah karena perbedaan hasil yang belum diselesaikan."
                    : "Ringkasan pemeriksaan golongan darah berhasil diambil."));
        }

        /// <summary>Daftar pemeriksaan golongan darah, dengan penyaring dan halaman.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<BloodGroupExamPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Group Exam", Description = "Melihat daftar pemeriksaan golongan darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodGroupExam", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] BbkBloodGroupExamStatus? examStatus,
            [FromQuery] bool? isConflictHeld,
            [FromQuery] bool? isValidResult,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodGroupExamService.GetPagedAsync(
                search,
                patientId,
                examStatus,
                isConflictHeld,
                isValidResult,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<BloodGroupExamPagedResult>.Ok(
                result,
                "Daftar pemeriksaan golongan darah berhasil diambil."));
        }

        /// <summary>Detail satu pemeriksaan, termasuk sampel dan status perbedaan hasil.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BloodGroupExamDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Group Exam", Description = "Melihat detail pemeriksaan golongan darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodGroupExam", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var detail = await _bloodGroupExamService.GetDetailAsync(id, cancellationToken);

            if (detail == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pemeriksaan golongan darah tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<BloodGroupExamDetailDto>.Ok(
                detail,
                "Detail pemeriksaan golongan darah berhasil diambil."));
        }

        /// <summary>
        /// Golongan darah sah pasien, atau penanda bahwa hasilnya sedang bertentangan
        /// (<c>BD-DOM-21</c>).
        /// </summary>
        /// <remarks>
        /// <b>Inilah pintu yang dipakai seluruh gerbang klinis Bank Darah.</b> Ketika pasien
        /// sedang menahan perbedaan, jawabannya kosong dan
        /// <c>isUsableForClinicalDecision</c> bernilai salah — dua lapis yang menyatakan hal
        /// sama, supaya pemanggil yang lalai memeriksa salah satunya tetap tidak mendapat nilai
        /// yang seolah sah (<c>VAL-BD-034</c>).
        ///
        /// Endpoint ini <b>tidak pernah</b> membaca <c>MstPatient.BloodType</c>.
        /// </remarks>
        [HttpGet("patient/{patientId:guid}/valid")]
        [ProducesResponseType(typeof(ApiResponse<ValidBloodGroupDto>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Group Exam", Description = "Melihat golongan darah sah pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodGroupExam", "Read")]
        public async Task<IActionResult> GetValidBloodGroup(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodGroupExamService.GetValidBloodGroupAsync(patientId, cancellationToken);

            return Ok(ApiResponse<ValidBloodGroupDto>.Ok(result, result.Message));
        }

        /// <summary>Mencatat pengambilan sampel dan membuka pemeriksaan baru.</summary>
        /// <remarks>
        /// Petugas pengambil diturunkan dari pengguna terautentikasi, bukan dari isian
        /// permintaan — tidak ada sampel yang dapat mengaku diambil orang lain.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BloodGroupExamDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Blood Group Exam", Description = "Mencatat pengambilan sampel golongan darah", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BloodGroupExam", "Create")]
        public async Task<IActionResult> RecordSample(
            [FromBody] RecordSampleRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodGroupExamService.RecordSampleAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != BloodGroupExamOutcome.Success)
                return MapFailure(result.Outcome, result.Message);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodGroupExam.RecordSample",
                "Mencatat pengambilan sampel golongan darah.",
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.PatientId,
                    Controller = "BloodGroupExam",
                    Action = "RecordSample"
                });

            return Ok(ApiResponse<BloodGroupExamDetailDto>.Ok(
                BbkBloodGroupExamService.ToDetail(result.Entity!),
                result.Message));
        }

        /// <summary>Mencatat hasil ABO dan Rhesus pada pemeriksaan yang sampelnya sudah diambil.</summary>
        /// <remarks>
        /// <c>VAL-BD-030</c> ditegakkan dengan tidak pernah menerima pemeriksa maupun waktu
        /// pemeriksaan dari pemanggil: keduanya diturunkan dari pengguna terautentikasi dan jam
        /// server, sehingga tidak ada jalan menyimpan hasil tanpa keduanya.
        /// </remarks>
        [HttpPost("{id:guid}/result")]
        [ProducesResponseType(typeof(ApiResponse<BloodGroupExamDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Blood Group Exam", Description = "Mencatat hasil ABO dan Rhesus", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodGroupExam", "Update")]
        public async Task<IActionResult> RecordResult(
            Guid id,
            [FromBody] RecordResultRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodGroupExamService.RecordResultAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != BloodGroupExamOutcome.Success)
                return MapFailure(result.Outcome, result.Message);

            // AboRhesusResult adalah data klinis dan MUST NOT masuk log
            // (contracts/permission-audit-matrix.md).
            await _loggerService.InfoAsync(
                LogCategory,
                "BloodGroupExam.RecordResult",
                "Mencatat hasil golongan darah.",
                new
                {
                    EntityId = id,
                    result.Entity!.PatientId,
                    Controller = "BloodGroupExam",
                    Action = "RecordResult"
                });

            return Ok(ApiResponse<BloodGroupExamDetailDto>.Ok(
                BbkBloodGroupExamService.ToDetail(result.Entity!),
                result.Message));
        }

        /// <summary>Memvalidasi hasil <b>rutin</b>, lalu menilai perbedaan dengan hasil sah sebelumnya.</summary>
        /// <remarks>
        /// <b>Butir hak akses ini berbeda dari penyelesaian konflik</b> (<c>DEC-BD-039</c>).
        /// Petugas BDRS berwenang validasi dapat memanggil endpoint ini, tetapi
        /// <b>tidak</b> dapat memanggil <c>POST /conflict-resolution</c> — dan itulah tepatnya
        /// yang dibuktikan <c>AC-BD-077</c> dan <c>AC-BD-078</c>.
        ///
        /// Bila hasilnya berbeda dari hasil sah sebelumnya, endpoint ini <b>tetap berhasil</b>:
        /// yang terjadi adalah kedua hasil ditahan dan pasien kehilangan golongan darah sah
        /// (<c>BD-XINV-04</c>). Validasi tidak gagal — konfliknya yang lahir.
        /// </remarks>
        [HttpPost("{id:guid}/validate")]
        [ProducesResponseType(typeof(ApiResponse<BloodGroupExamDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Validate", "Validate Blood Group Exam Result", Description = "Memvalidasi hasil golongan darah rutin", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("BloodGroupExam", "Validate")]
        public async Task<IActionResult> Validate(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodGroupExamService.ValidateAsync(
                id,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != BloodGroupExamOutcome.Success)
                return MapFailure(result.Outcome, result.Message);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodGroupExam.Validate",
                "Memvalidasi hasil golongan darah.",
                new
                {
                    EntityId = id,
                    result.Entity!.PatientId,
                    result.Entity.IsValidResult,
                    result.Entity.IsConflictHeld,
                    Controller = "BloodGroupExam",
                    Action = "Validate"
                });

            return Ok(ApiResponse<BloodGroupExamDetailDto>.Ok(
                BbkBloodGroupExamService.ToDetail(result.Entity!),
                result.Message));
        }

        /// <summary>
        /// Menyelesaikan perbedaan hasil golongan darah dengan menunjuk pemeriksaan ulang
        /// tervalidasi (<c>DEC-BD-031</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Butir hak akses terberat di alur ini.</b> Hanya validator klinis yang ditunjuk —
        /// Dokter BDRS atau penanggung jawab klinis — yang boleh memegang
        /// <c>BloodGroupExam : ResolveConflict</c> (<c>DEC-BD-039</c>, <c>VAL-BD-069</c>).
        /// </para>
        /// <para>
        /// <b>Wewenang tidak menggantikan prasyarat.</b> Validator klinis sekalipun tidak dapat
        /// menutup konflik tanpa menunjuk pemeriksaan ulang yang tervalidasi
        /// (<c>VAL-BD-051</c>, <c>AC-BD-080</c>). Sistem juga tidak pernah menghitung mayoritas
        /// atau memilih hasil sendiri (<c>VAL-BD-054</c>) — permintaannya bahkan tidak punya
        /// field untuk memintanya.
        /// </para>
        /// <para>
        /// Hasil ulang yang bernilai <b>ketiga</b>, berbeda dari kedua hasil bentrok, tetap boleh
        /// menjadi sah bila validator menyatakannya (<c>AC-BD-053</c>).
        /// </para>
        /// </remarks>
        [HttpPost("conflict-resolution")]
        [ProducesResponseType(typeof(ApiResponse<ValidBloodGroupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("ResolveConflict", "Resolve Blood Group Conflict", Description = "Menyelesaikan perbedaan hasil golongan darah", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("BloodGroupExam", "ResolveConflict")]
        public async Task<IActionResult> ResolveConflict(
            [FromBody] ResolveConflictRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodGroupExamService.ResolveConflictAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != BloodGroupExamOutcome.Success)
                return MapFailure(result.Outcome, result.Message);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodGroupExam.ResolveConflict",
                "Menyelesaikan perbedaan hasil golongan darah.",
                new
                {
                    request.PatientId,
                    request.ResolvingExamId,
                    request.ReasonCode,
                    Controller = "BloodGroupExam",
                    Action = "ResolveConflict"
                });

            return Ok(ApiResponse<ValidBloodGroupDto>.Ok(
                result.ValidBloodGroup!,
                result.Message));
        }

        /// <remarks>
        /// <c>NotAllowedByState</c> dipetakan ke <c>422</c>, bukan <c>403</c>: pelakunya
        /// berwenang — kalau tidak, <c>AccessPermissionFilter</c> sudah menahannya lebih dulu —
        /// yang tidak memenuhi syarat adalah keadaan datanya.
        /// </remarks>
        private IActionResult MapFailure(BloodGroupExamOutcome outcome, string message)
            => outcome switch
            {
                BloodGroupExamOutcome.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, message)),
                BloodGroupExamOutcome.DuplicateIdentity => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, message)),
                BloodGroupExamOutcome.NotAllowedByState => UnprocessableEntity(
                    ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, message)),
                _ => BadRequest(
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, message))
            };

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
