using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using SurgicalPreparationItemPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.SurgicalPreparationItemResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    /// <summary>
    /// Layar pengelola butir checklist persiapan bedah (<c>BE-RWI-173</c>, kontrak <c>0.10.0</c>
    /// API 11.4). Butir aktif menjadi isi checklist Catatan Pra-Operasi bangsal yang dikirim perawat
    /// bangsal dan dikonfirmasi perawat Kamar Operasi.
    /// </summary>
    /// <remarks>
    /// <b>Contoh.</b> Admin menambah butir "Gelang identitas terpasang" di kelompok Verifikasi
    /// pasien, bertanda wajib. Sejak itu setiap versi pra-operasi baru memuat butir tersebut, dan
    /// perawat bangsal tidak dapat mengirim sebelum mencentangnya (<c>OPR-WPO-003</c>). Versi yang
    /// sudah dibuat sebelum butir ditambahkan tidak berubah.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/surgical-preparation-items")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Surgical Preparation Item",
        AreaName = "HealthServices",
        ControllerName = "SurgicalPreparationItem",
        Description = "Mengelola butir checklist persiapan bedah untuk Catatan Pra-Operasi bangsal",
        SortOrder = 46
    )]
    [Tags("Health Services / Master Data / Surgical Preparation Item")]
    public class SurgicalPreparationItemController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.SurgicalPreparation";

        private readonly SurgicalPreparationItemService _service;
        private readonly LoggerService _loggerService;

        public SurgicalPreparationItemController(
            SurgicalPreparationItemService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        /// <summary>Konfigurasi penyaring, pengurutan, pilihan kelompok, dan isian form.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<SurgicalPreparationItemFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Surgical Preparation Item", Description = "Melihat konfigurasi penyaring butir persiapan bedah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SurgicalPreparationItem", "Read")]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken = default)
        {
            return Ok(ApiResponse<SurgicalPreparationItemFilterMetadataResponse>.Ok(
                await _service.BuildFilterMetadataAsync(cancellationToken),
                "Konfigurasi penyaring butir persiapan bedah berhasil diambil."));
        }

        /// <summary>Ringkasan jumlah butir untuk kartu statistik.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<SurgicalPreparationItemSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Surgical Preparation Item", Description = "Melihat ringkasan butir persiapan bedah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SurgicalPreparationItem", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var summary = await _service.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<SurgicalPreparationItemSummaryResponse>.Ok(
                summary,
                summary.ActiveSurgicalPreparationItem == 0
                    ? "Ringkasan berhasil diambil. Belum ada butir aktif, sehingga Catatan Pra-Operasi belum dapat dipakai."
                    : "Ringkasan butir persiapan bedah berhasil diambil."));
        }

        /// <summary>Daftar butir, dengan saringan kelompok dan aktif (kontrak API 11.4).</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<SurgicalPreparationItemPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Surgical Preparation Item", Description = "Melihat daftar butir persiapan bedah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SurgicalPreparationItem", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? groupName,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isMandatory,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(
                search,
                groupName,
                isActive,
                isMandatory,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<SurgicalPreparationItemPagedResult>.Ok(
                result,
                "Daftar butir persiapan bedah berhasil diambil."));
        }

        /// <summary>Pilihan butir aktif, diurutkan per kelompok lalu urutan tampil.</summary>
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<SurgicalPreparationItemOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Surgical Preparation Item", Description = "Melihat pilihan butir persiapan bedah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SurgicalPreparationItem", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            [FromQuery] string? groupName,
            [FromQuery] bool onlyActive = true,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetOptionsAsync(search, groupName, onlyActive, cancellationToken);

            return Ok(ApiResponse<List<SurgicalPreparationItemOptionResponse>>.Ok(
                result,
                result.Count == 0
                    ? "Belum ada butir persiapan bedah yang aktif."
                    : "Pilihan butir persiapan bedah berhasil diambil."));
        }

        /// <summary>Detail satu butir, termasuk <c>RowVersion</c> untuk mengubah.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SurgicalPreparationItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Surgical Preparation Item", Description = "Melihat detail butir persiapan bedah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SurgicalPreparationItem", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _service.GetByIdAsync(id, cancellationToken);

            if (item == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Butir persiapan bedah tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<SurgicalPreparationItemResponse>.Ok(
                item,
                "Detail butir persiapan bedah berhasil diambil."));
        }

        /// <summary>Menambah butir. Kode yang sudah dipakai → <c>409</c> <c>MST-SPI-001</c>.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<SurgicalPreparationItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Surgical Preparation Item", Description = "Menambah butir persiapan bedah", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("SurgicalPreparationItem", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateSurgicalPreparationItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.CreateAsync(request, GetCurrentUserId(), cancellationToken);

            if (result.Status != SurgicalPreparationItemStatus.Success)
                return MapFailure(result);

            await LogAsync("Create", "Menambah butir persiapan bedah.", result.Item!);

            return Ok(ApiResponse<SurgicalPreparationItemResponse>.Ok(result.Item, result.Message));
        }

        /// <summary>Mengubah butir dengan <c>RowVersion</c>.</summary>
        /// <remarks>
        /// <c>RowVersion</c> yang sudah berganti → <c>409</c>. Nama dan sifat wajib pada versi
        /// pra-operasi yang sudah ada tidak berubah; perubahan berlaku pada versi berikutnya.
        /// </remarks>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SurgicalPreparationItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Surgical Preparation Item", Description = "Mengubah butir persiapan bedah", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("SurgicalPreparationItem", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateSurgicalPreparationItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.UpdateAsync(id, request, GetCurrentUserId(), cancellationToken);

            if (result.Status != SurgicalPreparationItemStatus.Success)
                return MapFailure(result);

            await LogAsync("Update", "Mengubah butir persiapan bedah.", result.Item!);

            return Ok(ApiResponse<SurgicalPreparationItemResponse>.Ok(result.Item, result.Message));
        }

        /// <summary>Mengaktifkan atau menonaktifkan butir.</summary>
        /// <remarks>
        /// Butir nonaktif tidak muncul di versi pra-operasi baru; versi lama tetap utuh.
        /// </remarks>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<SurgicalPreparationItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Surgical Preparation Item Status", Description = "Mengubah status aktif butir persiapan bedah", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("SurgicalPreparationItem", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateSurgicalPreparationItemStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.UpdateStatusAsync(
                id,
                request.IsActive,
                request.RowVersion,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != SurgicalPreparationItemStatus.Success)
                return MapFailure(result);

            await LogAsync("UpdateStatus", "Mengubah status butir persiapan bedah.", result.Item!);

            return Ok(ApiResponse<SurgicalPreparationItemResponse>.Ok(result.Item, result.Message));
        }

        /// <summary>Menandai butir terhapus. Ditolak bila sudah dipakai Catatan Pra-Operasi.</summary>
        /// <remarks>
        /// Endpoint baseline master data di luar lima endpoint kontrak API 11.4 — dicatat sebagai
        /// delta kontrak pada laporan <c>BE-RWI-173</c>.
        /// </remarks>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Surgical Preparation Item", Description = "Menghapus butir persiapan bedah", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("SurgicalPreparationItem", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.DeleteAsync(id, GetCurrentUserId(), cancellationToken);

            if (result.Status != SurgicalPreparationItemStatus.Success)
                return MapFailure(result);

            await LogAsync("Delete", "Menghapus butir persiapan bedah.", result.Item!);

            return Ok(ApiResponse<bool>.Ok(true, result.Message));
        }

        private IActionResult MapFailure(SurgicalPreparationItemResult result)
            => result.Status switch
            {
                SurgicalPreparationItemStatus.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),
                SurgicalPreparationItemStatus.DuplicateCode => Conflict(
                    ApiResponse<object>.Fail(
                        StatusCodes.Status409Conflict,
                        result.Message,
                        new { Code = SurgicalPreparationItemService.DuplicateCodeErrorCode })),
                SurgicalPreparationItemStatus.VersionConflict => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message)),
                _ => BadRequest(
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.Message))
            };

        private Task LogAsync(string action, string message, SurgicalPreparationItemResponse item)
            => _loggerService.InfoAsync(
                LogCategory,
                $"SurgicalPreparationItem.{action}",
                message,
                new
                {
                    EntityId = item.Id,
                    item.Code,
                    item.GroupName,
                    item.IsMandatory,
                    item.IsActive,
                    Controller = "SurgicalPreparationItem",
                    Action = action
                });

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
