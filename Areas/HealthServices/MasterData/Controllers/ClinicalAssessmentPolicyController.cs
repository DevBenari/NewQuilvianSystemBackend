using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ClinicalAssessmentPolicyPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.ClinicalAssessmentPolicyResponse>;

using ClinicalAssessmentPolicyOptionPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.ClinicalAssessmentPolicyOptionResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    /// <summary>
    /// Layar pengelola batas waktu penyelesaian pengkajian. Clinical governance mengatur
    /// angkanya sendiri dari sini, tanpa meminta perubahan kode.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>BE-RWI-055</c>, <c>FR-KEP-010</c>, <c>FR-KEP-011</c>. <c>RWI-RULE-021</c> — nilai batas
    /// waktu klinis — sampai hari ini belum final karena pemilik klinisnya belum ditunjuk. Itu
    /// justru alasan layar ini ada: PRD 16.2 aturan 11 melarang angka SLA klinis ditanam di
    /// kode, dan mewajibkannya menjadi konfigurasi.
    /// </para>
    /// <para>
    /// <b>Selama master ini kosong, tidak ada satu pun pengkajian yang dinyatakan terlambat.</b>
    /// Pencatatan berjalan penuh; kolom tenggat pada pengkajian sekadar tidak terisi, dan daftar
    /// pantau kepatuhan berbunyi "batas waktu pengkajian belum ditetapkan" — bukan "terlambat"
    /// dan bukan "tidak ada data" (<c>VAL-KEP-17</c>).
    /// </para>
    /// <para>
    /// <b>Contoh nyata kenapa kebijakannya berversi.</b> Batas pengkajian awal semula 24 jam.
    /// Pengkajian Tn. Budi selesai pada jam ke-20 dan dinilai tepat waktu. Bila kelak batasnya
    /// diperketat menjadi 8 jam, pengkajian Tn. Budi <b>tetap</b> terbaca tepat waktu — ia
    /// menyimpan penunjuk ke kebijakan yang berlaku saat ia dibuat (<c>AC-CAP012-04</c>).
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/clinical-assessment-policies")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Clinical Assessment Policy",
        AreaName = "HealthServices",
        ControllerName = "ClinicalAssessmentPolicy",
        Description = "Mengelola batas waktu penyelesaian pengkajian",
        SortOrder = 43
    )]
    [Tags("Health Services / Master Data / Clinical Assessment Policy")]
    public class ClinicalAssessmentPolicyController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.Clinical";

        private readonly ClinicalAssessmentPolicyService _policyService;
        private readonly LoggerService _loggerService;

        public ClinicalAssessmentPolicyController(
            ClinicalAssessmentPolicyService policyService,
            LoggerService loggerService)
        {
            _policyService = policyService;
            _loggerService = loggerService;
        }

        /// <summary>Konfigurasi halaman: penyaring, pengurutan, dan metadata form.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalAssessmentPolicyFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Assessment Policy", Description = "Melihat konfigurasi penyaring kebijakan batas waktu pengkajian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalAssessmentPolicy", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<ClinicalAssessmentPolicyFilterMetadataResponse>.Ok(
                ClinicalAssessmentPolicyService.BuildFilterMetadata(),
                "Metadata filter kebijakan batas waktu pengkajian berhasil diambil."));
        }

        /// <summary>Rekap jumlah kebijakan untuk kartu statistik halaman index.</summary>
        /// <remarks>
        /// <c>IsPolicyMasterEmpty</c> sengaja ikut dikirim supaya layar dapat membedakan
        /// "belum ada kebijakan" dari "kebijakan ada tetapi tidak ada yang berlaku hari ini".
        /// </remarks>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalAssessmentPolicySummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Assessment Policy", Description = "Melihat rekap kebijakan batas waktu pengkajian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalAssessmentPolicy", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var hasil = await _policyService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<ClinicalAssessmentPolicySummaryResponse>.Ok(
                hasil,
                hasil.IsPolicyMasterEmpty
                    ? "Batas waktu pengkajian belum ditetapkan. Selama ini kosong, tidak ada pengkajian yang dinyatakan terlambat."
                    : "Rekap kebijakan batas waktu pengkajian berhasil diambil."));
        }

        /// <summary>Daftar kebijakan dengan pencarian, penyaringan, pengurutan, dan halaman.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ClinicalAssessmentPolicyPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Clinical Assessment Policy", Description = "Melihat daftar kebijakan batas waktu pengkajian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalAssessmentPolicy", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] PatientAssessmentType? assessmentType,
            [FromQuery] ServiceUnitType? serviceUnitType,
            [FromQuery] bool? onlyCurrentlyEffective,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal akhir tidak boleh mendahului tanggal awal."));
            }

            var hasil = await _policyService.GetPagedAsync(
                search,
                isActive,
                assessmentType,
                serviceUnitType,
                onlyCurrentlyEffective,
                startDate,
                endDate,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<ClinicalAssessmentPolicyPagedResult>.Ok(
                hasil,
                "Daftar kebijakan batas waktu pengkajian berhasil diambil."));
        }

        /// <summary>Pilihan kebijakan untuk kotak isian pada layar lain.</summary>
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalAssessmentPolicyOptionPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Assessment Policy", Description = "Melihat pilihan kebijakan batas waktu pengkajian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalAssessmentPolicy", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            [FromQuery] PatientAssessmentType? assessmentType,
            [FromQuery] ServiceUnitType? serviceUnitType,
            [FromQuery] bool onlyActive = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _policyService.GetOptionsAsync(
                search, assessmentType, serviceUnitType, onlyActive, pageNumber, pageSize, cancellationToken);

            return Ok(ApiResponse<ClinicalAssessmentPolicyOptionPagedResult>.Ok(
                hasil,
                hasil.TotalData == 0
                    ? "Belum ada kebijakan batas waktu pengkajian yang aktif."
                    : "Pilihan kebijakan batas waktu pengkajian berhasil diambil."));
        }

        /// <summary>Detail satu kebijakan.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalAssessmentPolicyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Clinical Assessment Policy", Description = "Melihat detail kebijakan batas waktu pengkajian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalAssessmentPolicy", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _policyService.GetByIdAsync(id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Data tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<ClinicalAssessmentPolicyResponse>.Ok(
                ClinicalAssessmentPolicyService.ToResponse(entity, DateTime.UtcNow),
                "Detail kebijakan batas waktu pengkajian berhasil diambil."));
        }

        /// <summary>Menambah kebijakan baru.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ClinicalAssessmentPolicyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Clinical Assessment Policy", Description = "Menambah kebijakan batas waktu pengkajian", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ClinicalAssessmentPolicy", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateClinicalAssessmentPolicyRequest request,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _policyService.CreateAsync(request, GetCurrentUserId(), cancellationToken);

            if (hasil.Status != ClinicalAssessmentPolicyStatus.Success)
                return MapFailure(hasil);

            await _loggerService.InfoAsync(
                LogCategory,
                "ClinicalAssessmentPolicy.Create",
                "Menambah kebijakan batas waktu pengkajian.",
                new
                {
                    EntityId = hasil.Entity!.Id,
                    hasil.Entity.PolicyCode,
                    hasil.Entity.AssessmentType,
                    hasil.Entity.DueWithinMinutes,
                    Controller = "ClinicalAssessmentPolicy",
                    Action = "Create"
                });

            return Ok(ApiResponse<ClinicalAssessmentPolicyResponse>.Ok(
                ClinicalAssessmentPolicyService.ToResponse(hasil.Entity!, DateTime.UtcNow),
                hasil.Message));
        }

        /// <summary>Mengubah seluruh field bisnis satu kebijakan.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalAssessmentPolicyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Clinical Assessment Policy", Description = "Mengubah kebijakan batas waktu pengkajian", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ClinicalAssessmentPolicy", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateClinicalAssessmentPolicyRequest request,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _policyService.UpdateAsync(id, request, GetCurrentUserId(), cancellationToken);

            if (hasil.Status != ClinicalAssessmentPolicyStatus.Success)
                return MapFailure(hasil);

            await _loggerService.InfoAsync(
                LogCategory,
                "ClinicalAssessmentPolicy.Update",
                "Mengubah kebijakan batas waktu pengkajian.",
                new
                {
                    EntityId = id,
                    hasil.Entity!.PolicyCode,
                    hasil.Entity.AssessmentType,
                    hasil.Entity.DueWithinMinutes,
                    Controller = "ClinicalAssessmentPolicy",
                    Action = "Update"
                });

            return Ok(ApiResponse<ClinicalAssessmentPolicyResponse>.Ok(
                ClinicalAssessmentPolicyService.ToResponse(hasil.Entity!, DateTime.UtcNow),
                hasil.Message));
        }

        /// <summary>Mengaktifkan atau menonaktifkan kebijakan.</summary>
        /// <remarks>
        /// Menonaktifkan kebijakan <b>tidak</b> mengubah penilaian pengkajian yang sudah
        /// memakainya. Yang berubah hanya ke depan: ia tidak lagi dipilih saat pengkajian baru
        /// dibuat.
        /// </remarks>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalAssessmentPolicyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Clinical Assessment Policy Status", Description = "Mengubah status aktif kebijakan batas waktu pengkajian", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ClinicalAssessmentPolicy", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateClinicalAssessmentPolicyStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _policyService.UpdateStatusAsync(
                id, request.IsActive, GetCurrentUserId(), cancellationToken);

            if (hasil.Status != ClinicalAssessmentPolicyStatus.Success)
                return MapFailure(hasil);

            await _loggerService.InfoAsync(
                LogCategory,
                "ClinicalAssessmentPolicy.UpdateStatus",
                "Mengubah status kebijakan batas waktu pengkajian.",
                new
                {
                    EntityId = id,
                    request.IsActive,
                    Controller = "ClinicalAssessmentPolicy",
                    Action = "UpdateStatus"
                });

            return Ok(ApiResponse<ClinicalAssessmentPolicyResponse>.Ok(
                ClinicalAssessmentPolicyService.ToResponse(hasil.Entity!, DateTime.UtcNow),
                hasil.Message));
        }

        /// <summary>Menandai kebijakan terhapus. Selalu soft delete.</summary>
        /// <remarks>
        /// Ditolak <c>400</c> bila kebijakan masih dipakai pengkajian mana pun — pengkajian
        /// menyimpan penunjuk kebijakannya sebagai bukti menurut aturan mana ia dinilai.
        /// </remarks>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Clinical Assessment Policy", Description = "Menghapus kebijakan batas waktu pengkajian", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("ClinicalAssessmentPolicy", "Delete")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _policyService.SoftDeleteAsync(id, GetCurrentUserId(), cancellationToken);

            if (hasil.Status != ClinicalAssessmentPolicyStatus.Success)
                return MapFailure(hasil);

            await _loggerService.InfoAsync(
                LogCategory,
                "ClinicalAssessmentPolicy.Delete",
                "Menghapus kebijakan batas waktu pengkajian.",
                new
                {
                    EntityId = id,
                    Controller = "ClinicalAssessmentPolicy",
                    Action = "Delete"
                });

            return Ok(ApiResponse<object>.Ok(null, hasil.Message));
        }

        private IActionResult MapFailure(ClinicalAssessmentPolicyResult hasil)
            => hasil.Status switch
            {
                ClinicalAssessmentPolicyStatus.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, hasil.Message)),
                ClinicalAssessmentPolicyStatus.DuplicateCode => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, hasil.Message)),
                _ => BadRequest(
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, hasil.Message))
            };

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
