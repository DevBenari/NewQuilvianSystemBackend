using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using InpatientClearanceItemPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.InpatientClearanceItemResponse>;
using InpatientClearanceItemOptionPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.InpatientClearanceItemOptionResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    /// <summary>
    /// Layar daftar butir administrasi yang menahan penutupan episode Rawat Inap. Admin
    /// menambah, mengubah, menonaktifkan, dan menghapus butirnya dari sini.
    /// </summary>
    /// <remarks>
    /// Butir wajib menahan penutupan episode selama belum ditandai petugas. Butir tidak wajib
    /// tetap dapat ditandai, tetapi tidak menahan apa pun.
    ///
    /// <b>Contoh.</b> Ny. Sari sudah dinyatakan boleh pulang. Butir <c>ADM-DOC</c> berkas
    /// administrasi sudah ditandai, tetapi butir <c>RETURN-ITEM</c> pengembalian barang belum.
    /// Keduanya wajib, sehingga penutupan episodenya ditolak sampai <c>RETURN-ITEM</c>
    /// ditandai. Butir <c>DISCHARGE-MED</c> obat pulang tidak wajib, jadi belum ditandai pun
    /// tidak menahan penutupan.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/inpatient-clearance-items")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Inpatient Clearance Item",
        AreaName = "HealthServices",
        ControllerName = "InpatientClearanceItem",
        Description = "Mengelola butir administrasi yang menahan penutupan episode Rawat Inap",
        SortOrder = 41
    )]
    [Tags("Health Services / Master Data / Inpatient Clearance Item")]
    public class InpatientClearanceItemController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.Inpatient";

        private readonly InpatientClearanceItemService _clearanceItemService;
        private readonly LoggerService _loggerService;

        public InpatientClearanceItemController(
            InpatientClearanceItemService clearanceItemService,
            LoggerService loggerService)
        {
            _clearanceItemService = clearanceItemService;
            _loggerService = loggerService;
        }

        /// <summary>Metadata filter, pengurutan, pagination, dan field editor.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<InpatientClearanceItemFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Clearance Item", Description = "Melihat metadata filter butir administrasi Rawat Inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientClearanceItem", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = _clearanceItemService.GetFilterMetadata();

            return Ok(ApiResponse<InpatientClearanceItemFilterMetadataResponse>.Ok(
                result,
                "Metadata filter butir administrasi Rawat Inap berhasil diambil."));
        }

        /// <summary>Ringkasan jumlah butir administrasi aktif, nonaktif, wajib, dan opsional.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<InpatientClearanceItemSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Clearance Item", Description = "Melihat ringkasan butir administrasi Rawat Inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientClearanceItem", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var result = await _clearanceItemService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<InpatientClearanceItemSummaryResponse>.Ok(
                result,
                "Ringkasan butir administrasi Rawat Inap berhasil diambil."));
        }

        /// <summary>Daftar butir administrasi, dengan pencarian, penyaringan, dan halaman.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<InpatientClearanceItemPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Clearance Item", Description = "Melihat daftar butir administrasi Rawat Inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientClearanceItem", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isMandatory,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            InpatientClearanceItemPagedResult result;

            try
            {
                result = await _clearanceItemService.GetPagedAsync(
                    startDate,
                    endDate,
                    customPeriod,
                    search,
                    isMandatory,
                    isActive,
                    sortBy,
                    sortDirection,
                    pageNumber,
                    pageSize,
                    cancellationToken);
            }
            catch (ArgumentException error)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    error.Message));
            }

            return Ok(ApiResponse<InpatientClearanceItemPagedResult>.Ok(
                result,
                "Daftar butir administrasi Rawat Inap berhasil diambil."));
        }

        /// <summary>Daftar ringan butir administrasi untuk dropdown atau lookup.</summary>
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<InpatientClearanceItemOptionPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Clearance Item", Description = "Melihat pilihan butir administrasi Rawat Inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientClearanceItem", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool? isMandatory = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _clearanceItemService.GetOptionsAsync(
                onlyActive,
                isMandatory,
                search,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<InpatientClearanceItemOptionPagedResult>.Ok(
                result,
                "Pilihan butir administrasi Rawat Inap berhasil diambil."));
        }

        /// <summary>Detail satu butir administrasi.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InpatientClearanceItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Inpatient Clearance Item", Description = "Melihat detail butir administrasi Rawat Inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientClearanceItem", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _clearanceItemService.GetByIdAsync(id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Butir administrasi tidak ditemukan."));
            }

            return Ok(ApiResponse<InpatientClearanceItemResponse>.Ok(
                InpatientClearanceItemService.ToResponse(entity),
                "Detail butir administrasi berhasil diambil."));
        }

        /// <summary>Menambah butir administrasi baru.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<InpatientClearanceItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Inpatient Clearance Item", Description = "Menambah butir administrasi Rawat Inap", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("InpatientClearanceItem", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateInpatientClearanceItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _clearanceItemService.CreateAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != InpatientClearanceItemStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientClearanceItem.Create",
                "Menambah butir administrasi Rawat Inap.",
                new { EntityId = result.Entity!.Id, Controller = "InpatientClearanceItem", Action = "Create" }
            );

            return Ok(ApiResponse<InpatientClearanceItemResponse>.Ok(
                InpatientClearanceItemService.ToResponse(result.Entity!),
                result.Message));
        }

        /// <summary>Mengubah butir administrasi.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InpatientClearanceItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Inpatient Clearance Item", Description = "Mengubah butir administrasi Rawat Inap", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientClearanceItem", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateInpatientClearanceItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _clearanceItemService.UpdateAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != InpatientClearanceItemStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientClearanceItem.Update",
                "Mengubah butir administrasi Rawat Inap.",
                new { EntityId = id, Controller = "InpatientClearanceItem", Action = "Update" }
            );

            return Ok(ApiResponse<InpatientClearanceItemResponse>.Ok(
                InpatientClearanceItemService.ToResponse(result.Entity!),
                result.Message));
        }

        /// <summary>Mengaktifkan atau menonaktifkan butir administrasi.</summary>
        /// <remarks>
        /// Menonaktifkan butir tidak menghapus penandaan yang sudah ada pada episode lama.
        /// </remarks>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<InpatientClearanceItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Inpatient Clearance Item Status", Description = "Mengubah status aktif butir administrasi Rawat Inap", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientClearanceItem", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateInpatientClearanceItemStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _clearanceItemService.UpdateStatusAsync(
                id,
                request.IsActive,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != InpatientClearanceItemStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientClearanceItem.UpdateStatus",
                "Mengubah status butir administrasi Rawat Inap.",
                new { EntityId = id, request.IsActive, Controller = "InpatientClearanceItem", Action = "UpdateStatus" }
            );

            return Ok(ApiResponse<InpatientClearanceItemResponse>.Ok(
                InpatientClearanceItemService.ToResponse(result.Entity!),
                result.Message));
        }

        /// <summary>Menandai butir administrasi terhapus.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InpatientClearanceItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Inpatient Clearance Item", Description = "Menghapus butir administrasi Rawat Inap", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("InpatientClearanceItem", "Delete")]
        public async Task<IActionResult> Delete(
            Guid id,
            [FromBody] DeleteInpatientClearanceItemRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _clearanceItemService.DeleteAsync(
                id,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != InpatientClearanceItemStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientClearanceItem.Delete",
                "Menghapus butir administrasi Rawat Inap.",
                new
                {
                    EntityId = id,
                    DeleteReason = request?.DeleteReason,
                    Controller = "InpatientClearanceItem",
                    Action = "Delete"
                }
            );

            return Ok(ApiResponse<InpatientClearanceItemResponse>.Ok(
                InpatientClearanceItemService.ToResponse(result.Entity!),
                result.Message));
        }

        private IActionResult MapFailure(InpatientClearanceItemResult result)
            => result.Status switch
            {
                InpatientClearanceItemStatus.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),
                InpatientClearanceItemStatus.DuplicateCode => Conflict(
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
