using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using BloodComponentPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.BloodComponentResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    /// <summary>
    /// Layar pengelola katalog komponen darah. Petugas Bank Darah menambah, mengubah, dan
    /// mengaktifkan atau menonaktifkan komponen dari sini, tanpa meminta perubahan kode.
    /// </summary>
    /// <remarks>
    /// Katalog ini menentukan dua hal di luar layarnya sendiri. Pertama, pendeteksian order
    /// darah ganda membandingkan pasien, kunjungan, dan komponen sekaligus, sehingga komponen
    /// yang tidak terdaftar membuat order tidak dapat dibuat. Kedua, gerbang pemberian
    /// menghitung masa berlaku bukti kecocokan dari kolom milik komponennya.
    ///
    /// <b>Contoh.</b> BDRS mendaftarkan PRC tanpa mengisi masa berlaku bukti kecocokan.
    /// Order PRC tetap dapat dibuat dan kantongnya tetap dapat disimpan serta dialokasikan,
    /// tetapi begitu petugas menekan Berikan, permintaannya ditolak dengan keterangan bahwa
    /// masa berlaku komponen ini belum ditetapkan. Halaman index menampilkan jumlah komponen
    /// yang masih dalam keadaan itu, supaya konfigurasinya tidak terlupakan sampai ada pasien
    /// yang menunggu.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/blood-components")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Blood Component",
        AreaName = "HealthServices",
        ControllerName = "BloodComponent",
        Description = "Mengelola katalog komponen darah Bank Darah",
        SortOrder = 43
    )]
    [Tags("Health Services / Master Data / Blood Component")]
    public class BloodComponentController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.BloodBank";

        private readonly BloodComponentService _bloodComponentService;
        private readonly LoggerService _loggerService;

        public BloodComponentController(
            BloodComponentService bloodComponentService,
            LoggerService loggerService)
        {
            _bloodComponentService = bloodComponentService;
            _loggerService = loggerService;
        }

        /// <summary>Konfigurasi penyaring, pengurutan, dan isian form untuk halaman katalog.</summary>
        /// <remarks>
        /// Dipanggil paling awal saat layar dibuka. Seluruh penyaring yang diumumkan di sini
        /// benar-benar didukung <c>GET /</c>, termasuk <c>isValidityConfigured</c> yang dipakai
        /// admin untuk menemukan komponen yang konfigurasinya masih tertinggal.
        /// </remarks>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<BloodComponentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Component", Description = "Melihat konfigurasi penyaring katalog komponen darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodComponent", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<BloodComponentFilterMetadataResponse>.Ok(
                BloodComponentService.BuildFilterMetadata(),
                "Konfigurasi penyaring komponen darah berhasil diambil."));
        }

        /// <summary>Ringkasan jumlah komponen darah untuk kartu statistik.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<BloodComponentSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Component", Description = "Melihat ringkasan katalog komponen darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodComponent", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var summary = await _bloodComponentService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<BloodComponentSummaryResponse>.Ok(
                summary,
                summary.ValidityNotConfiguredBloodComponent > 0
                    ? $"Ringkasan komponen darah berhasil diambil. {summary.ValidityNotConfiguredBloodComponent} komponen aktif belum punya masa berlaku bukti kecocokan, sehingga pemberiannya masih tertahan."
                    : "Ringkasan komponen darah berhasil diambil."));
        }

        /// <summary>Daftar komponen darah, dengan pencarian, penyaringan, dan halaman.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<BloodComponentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Component", Description = "Melihat daftar komponen darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodComponent", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isValidityConfigured,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodComponentService.GetPagedAsync(
                search,
                isActive,
                isValidityConfigured,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<BloodComponentPagedResult>.Ok(
                result,
                "Daftar komponen darah berhasil diambil."));
        }

        /// <summary>Pilihan komponen yang aktif, untuk kotak isian pada layar lain.</summary>
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<BloodComponentOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Component", Description = "Melihat pilihan komponen darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodComponent", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            [FromQuery] bool onlyActive = true,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodComponentService.GetOptionsAsync(
                search,
                onlyActive,
                cancellationToken);

            return Ok(ApiResponse<List<BloodComponentOptionResponse>>.Ok(
                result,
                result.Count == 0
                    ? "Belum ada komponen darah yang aktif. Selama katalog kosong, order darah tidak dapat dibuat."
                    : "Pilihan komponen darah berhasil diambil."));
        }

        /// <summary>Detail satu komponen darah.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BloodComponentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Component", Description = "Melihat detail komponen darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodComponent", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _bloodComponentService.GetByIdAsync(id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Komponen darah tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<BloodComponentResponse>.Ok(
                BloodComponentService.ToResponse(entity),
                "Detail komponen darah berhasil diambil."));
        }

        /// <summary>Menambah komponen darah baru.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BloodComponentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Blood Component", Description = "Menambah komponen darah", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BloodComponent", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateBloodComponentRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodComponentService.CreateAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != BloodComponentStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodComponent.Create",
                "Menambah komponen darah.",
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.ComponentCode,
                    result.Entity.CompatibilityEvidenceValidityHours,
                    Controller = "BloodComponent",
                    Action = "Create"
                });

            return Ok(ApiResponse<BloodComponentResponse>.Ok(
                BloodComponentService.ToResponse(result.Entity!),
                result.Message));
        }

        /// <summary>Mengubah komponen darah.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BloodComponentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Blood Component", Description = "Mengubah komponen darah", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodComponent", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateBloodComponentRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodComponentService.UpdateAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != BloodComponentStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodComponent.Update",
                "Mengubah komponen darah.",
                new
                {
                    EntityId = id,
                    result.Entity!.ComponentCode,
                    result.Entity.CompatibilityEvidenceValidityHours,
                    Controller = "BloodComponent",
                    Action = "Update"
                });

            return Ok(ApiResponse<BloodComponentResponse>.Ok(
                BloodComponentService.ToResponse(result.Entity!),
                result.Message));
        }

        /// <summary>Mengaktifkan atau menonaktifkan komponen darah.</summary>
        /// <remarks>
        /// Menonaktifkan komponen TIDAK menyentuh order darah maupun kantong yang sudah
        /// menyebutnya. Yang berubah hanya ke depan: ia tidak lagi muncul sebagai pilihan.
        /// </remarks>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<BloodComponentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Blood Component Status", Description = "Mengubah status aktif komponen darah", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodComponent", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateBloodComponentStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodComponentService.UpdateStatusAsync(
                id,
                request.IsActive,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != BloodComponentStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodComponent.UpdateStatus",
                "Mengubah status komponen darah.",
                new
                {
                    EntityId = id,
                    request.IsActive,
                    Controller = "BloodComponent",
                    Action = "UpdateStatus"
                });

            return Ok(ApiResponse<BloodComponentResponse>.Ok(
                BloodComponentService.ToResponse(result.Entity!),
                result.Message));
        }

        /// <summary>Menandai komponen darah terhapus. Tidak pernah menghapus baris fisik.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Blood Component", Description = "Menghapus komponen darah", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("BloodComponent", "Delete")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodComponentService.DeleteAsync(
                id,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != BloodComponentStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodComponent.Delete",
                "Menghapus komponen darah.",
                new
                {
                    EntityId = id,
                    result.Entity!.ComponentCode,
                    Controller = "BloodComponent",
                    Action = "Delete"
                });

            return Ok(ApiResponse<bool>.Ok(true, result.Message));
        }

        private IActionResult MapFailure(BloodComponentResult result)
            => result.Status switch
            {
                BloodComponentStatus.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),
                BloodComponentStatus.DuplicateCode => Conflict(
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
