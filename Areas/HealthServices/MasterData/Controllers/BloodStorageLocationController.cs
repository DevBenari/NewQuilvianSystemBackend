using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using BloodStorageLocationPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.BloodStorageLocationResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    /// <summary>
    /// Layar pengelola lokasi penyimpanan darah milik Bank Darah — kulkas darah dan tempat
    /// simpan lain yang benar-benar ada di BDRS.
    /// </summary>
    /// <remarks>
    /// <b>Layar ini menentukan apakah modul Bank Darah berjalan atau berhenti.</b> Selama tidak
    /// ada satu pun lokasi aktif, tidak ada kantong yang dapat disimpan, dialokasikan, maupun
    /// diberikan (<c>INV-BD-025</c>). Ringkasan di halaman index menandai keadaan itu secara
    /// tegas, supaya tidak ditemukan saat ada pasien menunggu.
    ///
    /// <b>Contoh alur nyata.</b> Kulkas Besar rusak Selasa siang. Petugas menonaktifkannya dari
    /// sini. Dua belas kantong di dalamnya tetap tercatat di sana dengan status yang sama
    /// persis — sistem tidak memindahkan apa pun sendiri. Yang berubah: kulkas itu hilang dari
    /// pilihan penyimpanan, dan keduabelas kantong tertahan alokasinya sampai petugas
    /// memindahkannya ke kulkas yang aktif.
    ///
    /// <b>Bukan cold storage farmasi.</b> <c>MstDrugStorageLocation</c> punya controller
    /// sendiri dan tidak disentuh modul ini (<c>DEC-BD-035</c>).
    ///
    /// <b>Di luar scope MVP dan memang tidak ada di sini:</b> pemantauan suhu, kapasitas, rak
    /// atau laci, dan hierarki gudang (<c>AC-BD-064</c>).
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/blood-storage-locations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Blood Storage Location",
        AreaName = "HealthServices",
        ControllerName = "BloodStorageLocation",
        Description = "Mengelola master lokasi penyimpanan darah Bank Darah",
        SortOrder = 44
    )]
    [Tags("Health Services / Master Data / Blood Storage Location")]
    public class BloodStorageLocationController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.BloodBank";

        private readonly BloodStorageLocationService _storageLocationService;
        private readonly LoggerService _loggerService;

        public BloodStorageLocationController(
            BloodStorageLocationService storageLocationService,
            LoggerService loggerService)
        {
            _storageLocationService = storageLocationService;
            _loggerService = loggerService;
        }

        /// <summary>Konfigurasi penyaring, pengurutan, dan isian form untuk halaman master.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<BloodStorageLocationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Storage Location", Description = "Melihat konfigurasi penyaring lokasi penyimpanan darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodStorageLocation", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<BloodStorageLocationFilterMetadataResponse>.Ok(
                BloodStorageLocationService.BuildFilterMetadata(),
                "Konfigurasi penyaring lokasi penyimpanan darah berhasil diambil."));
        }

        /// <summary>Ringkasan jumlah lokasi, termasuk penanda modul berhenti.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<BloodStorageLocationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Storage Location", Description = "Melihat ringkasan lokasi penyimpanan darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodStorageLocation", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var summary = await _storageLocationService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<BloodStorageLocationSummaryResponse>.Ok(
                summary,
                summary.IsBloodBankHaltedByEmptyActiveLocation
                    ? "Ringkasan berhasil diambil. Peringatan: tidak ada satu pun lokasi penyimpanan yang aktif, sehingga tidak ada kantong darah yang dapat disimpan, dialokasikan, maupun diberikan."
                    : "Ringkasan lokasi penyimpanan darah berhasil diambil."));
        }

        /// <summary>Daftar lokasi penyimpanan darah, dengan pencarian dan halaman.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<BloodStorageLocationPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Storage Location", Description = "Melihat daftar lokasi penyimpanan darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodStorageLocation", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _storageLocationService.GetPagedAsync(
                search,
                isActive,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<BloodStorageLocationPagedResult>.Ok(
                result,
                "Daftar lokasi penyimpanan darah berhasil diambil."));
        }

        /// <summary>Pilihan lokasi penyimpanan yang <b>aktif saja</b>, untuk kotak isian.</summary>
        /// <remarks>
        /// Penyaringan lokasi aktif dilakukan di backend dan tidak dapat dimatikan pemanggil.
        /// Itu disengaja: layar tidak boleh dapat menawarkan lokasi nonaktif walaupun penulis
        /// layarnya lupa menyaring (<c>INV-BD-027</c>).
        /// </remarks>
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<BloodStorageLocationOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Storage Location", Description = "Melihat pilihan lokasi penyimpanan darah yang aktif", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodStorageLocation", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            CancellationToken cancellationToken = default)
        {
            var result = await _storageLocationService.GetOptionsAsync(search, cancellationToken);

            return Ok(ApiResponse<List<BloodStorageLocationOptionResponse>>.Ok(
                result,
                result.Count == 0
                    ? "Belum ada lokasi penyimpanan darah yang aktif. Selama daftar ini kosong, kantong darah tidak dapat disimpan, dialokasikan, maupun diberikan."
                    : "Pilihan lokasi penyimpanan darah berhasil diambil."));
        }

        /// <summary>Detail satu lokasi penyimpanan darah.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BloodStorageLocationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Storage Location", Description = "Melihat detail lokasi penyimpanan darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodStorageLocation", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _storageLocationService.GetByIdAsync(id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Lokasi penyimpanan darah tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<BloodStorageLocationResponse>.Ok(
                BloodStorageLocationService.ToResponse(entity),
                "Detail lokasi penyimpanan darah berhasil diambil."));
        }

        /// <summary>Menambah lokasi penyimpanan darah baru.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BloodStorageLocationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Blood Storage Location", Description = "Menambah lokasi penyimpanan darah", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BloodStorageLocation", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateBloodStorageLocationRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _storageLocationService.CreateAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != BloodStorageLocationStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodStorageLocation.Create",
                "Menambah lokasi penyimpanan darah.",
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.StorageLocationCode,
                    result.Entity.IsActive,
                    Controller = "BloodStorageLocation",
                    Action = "Create"
                });

            return Ok(ApiResponse<BloodStorageLocationResponse>.Ok(
                BloodStorageLocationService.ToResponse(result.Entity!),
                result.Message));
        }

        /// <summary>Mengubah kode, nama, keterangan, dan status lokasi penyimpanan darah.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BloodStorageLocationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Blood Storage Location", Description = "Mengubah lokasi penyimpanan darah", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodStorageLocation", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateBloodStorageLocationRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _storageLocationService.UpdateAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != BloodStorageLocationStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodStorageLocation.Update",
                "Mengubah lokasi penyimpanan darah.",
                new
                {
                    EntityId = id,
                    result.Entity!.StorageLocationCode,
                    result.Entity.IsActive,
                    Controller = "BloodStorageLocation",
                    Action = "Update"
                });

            return Ok(ApiResponse<BloodStorageLocationResponse>.Ok(
                BloodStorageLocationService.ToResponse(result.Entity!),
                result.Message));
        }

        /// <summary>Mengaktifkan atau menonaktifkan lokasi penyimpanan darah.</summary>
        /// <remarks>
        /// <b>Penonaktifan tidak pernah ditolak</b> walaupun masih ada kantong di dalamnya
        /// (<c>VAL-BD-068</c>). Menonaktifkan lokasi justru dilakukan ketika kulkasnya rusak;
        /// menolaknya akan memaksa petugas memindahkan kantong ke lokasi yang sedang rusak.
        ///
        /// Yang terjadi hanya ini: lokasi hilang dari pilihan penyimpanan, dan kantong yang
        /// masih tercatat di sana tertahan alokasinya. <b>Kantong tidak berpindah dan tidak
        /// berubah status</b> (<c>DEC-BD-037</c>).
        /// </remarks>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<BloodStorageLocationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Blood Storage Location Status", Description = "Mengubah status aktif lokasi penyimpanan darah", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodStorageLocation", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateBloodStorageLocationStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _storageLocationService.UpdateStatusAsync(
                id,
                request.IsActive,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != BloodStorageLocationStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodStorageLocation.UpdateStatus",
                "Mengubah status lokasi penyimpanan darah.",
                new
                {
                    EntityId = id,
                    request.IsActive,
                    Controller = "BloodStorageLocation",
                    Action = "UpdateStatus"
                });

            return Ok(ApiResponse<BloodStorageLocationResponse>.Ok(
                BloodStorageLocationService.ToResponse(result.Entity!),
                result.Message));
        }

        /// <summary>Menandai lokasi penyimpanan terhapus. Tidak pernah menghapus baris fisik.</summary>
        /// <remarks>
        /// Untuk keadaan sehari-hari, <b>menonaktifkan lebih tepat daripada menghapus</b>:
        /// penonaktifan menutup gerbang tanpa memutus makna riwayat penempatan lama yang
        /// menyebut lokasi itu.
        /// </remarks>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Blood Storage Location", Description = "Menghapus lokasi penyimpanan darah", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("BloodStorageLocation", "Delete")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _storageLocationService.DeleteAsync(
                id,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != BloodStorageLocationStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodStorageLocation.Delete",
                "Menghapus lokasi penyimpanan darah.",
                new
                {
                    EntityId = id,
                    result.Entity!.StorageLocationCode,
                    Controller = "BloodStorageLocation",
                    Action = "Delete"
                });

            return Ok(ApiResponse<bool>.Ok(true, result.Message));
        }

        private IActionResult MapFailure(BloodStorageLocationResult result)
            => result.Status switch
            {
                BloodStorageLocationStatus.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),
                BloodStorageLocationStatus.DuplicateIdentity => Conflict(
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
