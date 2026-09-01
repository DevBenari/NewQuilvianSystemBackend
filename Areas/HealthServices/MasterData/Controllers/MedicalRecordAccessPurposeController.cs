using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using MedicalRecordAccessPurposePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.MedicalRecordAccessPurposeResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    /// <summary>
    /// Layar pengelola keperluan akses rekam medis. Unit rekam medis menambah, mengubah, dan
    /// mengaktifkan atau menonaktifkan keperluannya dari sini, tanpa meminta perubahan kode.
    /// </summary>
    /// <remarks>
    /// Layar ini bukan pelengkap. Selama master keperluan kosong, <b>pembukaan berkas rekam
    /// medis pasien di luar rawatan pengguna selalu ditolak</b> — penilaian akses menuntut
    /// keperluan yang sah, dan tidak ada satu pun yang dapat dipilih.
    ///
    /// <b>Contoh.</b> Petugas koding perlu membaca berkas Tn. Budi untuk melengkapi diagnosis,
    /// sedangkan Tn. Budi sudah pulang. Tanpa satu pun baris di master ini, permintaannya
    /// dijawab <c>400</c> dan pekerjaannya berhenti. Setelah unit rekam medis menambahkan
    /// keperluan <c>KODING</c>, permintaan yang sama berhasil dan jejaknya tercatat dengan
    /// alasan yang dapat ditinjau.
    ///
    /// Tidak ada endpoint penghapusan, dan itu bukan kelalaian: keperluan yang tidak berlaku
    /// lagi dinonaktifkan lewat <c>PATCH /{id}/status</c>. Menghapusnya akan memutus makna
    /// jejak akses lama yang menyebutnya.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/medical-record-access-purposes")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Medical Record Access Purpose",
        AreaName = "HealthServices",
        ControllerName = "MedicalRecordAccessPurpose",
        Description = "Mengelola daftar keperluan akses rekam medis",
        SortOrder = 42
    )]
    [Tags("Health Services / Master Data / Medical Record Access Purpose")]
    public class MedicalRecordAccessPurposeController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.MedicalRecord";

        private readonly MedicalRecordAccessPurposeService _accessPurposeService;
        private readonly LoggerService _loggerService;

        public MedicalRecordAccessPurposeController(
            MedicalRecordAccessPurposeService accessPurposeService,
            LoggerService loggerService)
        {
            _accessPurposeService = accessPurposeService;
            _loggerService = loggerService;
        }

        /// <summary>Daftar keperluan akses, dengan pencarian, penyaringan, dan halaman.</summary>
        /// <remarks>
        /// Penyaring halaman menerima <c>pageNumber</c> maupun <c>page</c>. Kontrak modul rekam
        /// medis menuliskan <c>page</c>, sedangkan 28 controller master data lain di folder ini
        /// mengikat <c>pageNumber</c>. Keduanya diterima supaya tidak ada pemanggil yang diam-diam
        /// selalu mendapat halaman pertama; <c>pageNumber</c> menang bila keduanya dikirim.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordAccessPurposePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Record Access Purpose", Description = "Melihat daftar keperluan akses rekam medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordAccessPurpose", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageNumber = 0,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _accessPurposeService.GetPagedAsync(
                search,
                isActive,
                pageNumber > 0 ? pageNumber : page,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<MedicalRecordAccessPurposePagedResult>.Ok(
                result,
                "Daftar keperluan akses berhasil diambil."));
        }

        /// <summary>Pilihan keperluan yang aktif, untuk kotak isian.</summary>
        /// <remarks>
        /// Bentuk balasannya sama persis dengan <c>accessPurposes</c> pada
        /// <c>GET /medical-records/filters/metadata</c>, sehingga layar yang berpindah dari satu
        /// sumber ke sumber lain tidak perlu menulis ulang pembacanya.
        ///
        /// Kotak keperluan pada layar rekam medis tetap memakai <c>/filters/metadata</c>, karena
        /// satu panggilan di sana membawa daftar keperluan, pilihan penyaring, dan penanda master
        /// kosong sekaligus. Endpoint ini untuk layar master.
        /// </remarks>
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<MedicalRecordAccessPurposeOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Record Access Purpose", Description = "Melihat pilihan keperluan akses rekam medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordAccessPurpose", "Read")]
        public async Task<IActionResult> GetOptions(CancellationToken cancellationToken = default)
        {
            var result = await _accessPurposeService.GetOptionsAsync(cancellationToken);

            return Ok(ApiResponse<List<MedicalRecordAccessPurposeOptionResponse>>.Ok(
                result,
                result.Count == 0
                    ? "Belum ada keperluan akses yang aktif. Selama daftar ini kosong, berkas rekam medis pasien di luar rawatan tidak dapat dibuka."
                    : "Pilihan keperluan akses berhasil diambil."));
        }

        /// <summary>Detail satu keperluan akses.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordAccessPurposeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Medical Record Access Purpose", Description = "Melihat detail keperluan akses rekam medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordAccessPurpose", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _accessPurposeService.GetByIdAsync(id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Keperluan akses tidak ditemukan."));
            }

            return Ok(ApiResponse<MedicalRecordAccessPurposeResponse>.Ok(
                MedicalRecordAccessPurposeService.ToResponse(entity),
                "Detail keperluan akses berhasil diambil."));
        }

        /// <summary>Menambah keperluan akses baru.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordAccessPurposeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Medical Record Access Purpose", Description = "Menambah keperluan akses rekam medis", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MedicalRecordAccessPurpose", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateMedicalRecordAccessPurposeRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _accessPurposeService.CreateAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != MedicalRecordAccessPurposeStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "MedicalRecordAccessPurpose.Create",
                "Menambah keperluan akses rekam medis.",
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.PurposeCode,
                    result.Entity.RequiresReview,
                    Controller = "MedicalRecordAccessPurpose",
                    Action = "Create"
                });

            return Ok(ApiResponse<MedicalRecordAccessPurposeResponse>.Ok(
                MedicalRecordAccessPurposeService.ToResponse(result.Entity!),
                result.Message));
        }

        /// <summary>Mengubah keperluan akses.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordAccessPurposeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Medical Record Access Purpose", Description = "Mengubah keperluan akses rekam medis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalRecordAccessPurpose", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateMedicalRecordAccessPurposeRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _accessPurposeService.UpdateAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != MedicalRecordAccessPurposeStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "MedicalRecordAccessPurpose.Update",
                "Mengubah keperluan akses rekam medis.",
                new
                {
                    EntityId = id,
                    result.Entity!.PurposeCode,
                    result.Entity.RequiresReview,
                    Controller = "MedicalRecordAccessPurpose",
                    Action = "Update"
                });

            return Ok(ApiResponse<MedicalRecordAccessPurposeResponse>.Ok(
                MedicalRecordAccessPurposeService.ToResponse(result.Entity!),
                result.Message));
        }

        /// <summary>Mengaktifkan atau menonaktifkan keperluan akses.</summary>
        /// <remarks>
        /// Menonaktifkan keperluan TIDAK menyentuh jejak akses yang sudah memakainya. Yang
        /// berubah hanya ke depan: ia tidak lagi muncul sebagai pilihan, dan pemakaiannya
        /// ditolak penilaian akses.
        /// </remarks>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordAccessPurposeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Medical Record Access Purpose Status", Description = "Mengubah status aktif keperluan akses rekam medis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalRecordAccessPurpose", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateMedicalRecordAccessPurposeStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _accessPurposeService.UpdateStatusAsync(
                id,
                request.IsActive,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != MedicalRecordAccessPurposeStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "MedicalRecordAccessPurpose.UpdateStatus",
                "Mengubah status keperluan akses rekam medis.",
                new
                {
                    EntityId = id,
                    request.IsActive,
                    Controller = "MedicalRecordAccessPurpose",
                    Action = "UpdateStatus"
                });

            return Ok(ApiResponse<MedicalRecordAccessPurposeResponse>.Ok(
                MedicalRecordAccessPurposeService.ToResponse(result.Entity!),
                result.Message));
        }

        private IActionResult MapFailure(MedicalRecordAccessPurposeResult result)
            => result.Status switch
            {
                MedicalRecordAccessPurposeStatus.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),
                MedicalRecordAccessPurposeStatus.DuplicateCode => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message)),
                _ => BadRequest(
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.Message))
            };

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
