using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-core/initial-batches")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Initial Placement Batch Administration",
        AreaName = "Corporate",
        ControllerName = "HrdInitialPlacementBatch",
        Description = "Batch Initial Placement for Baseline Employees",
        SortOrder = 23)]
    [Tags("Corporate / Human Resource / Workforce Core / Initial Placement Batch")]
    public class HrdInitialPlacementBatchController : ControllerBase
    {
        private readonly IHrdInitialPlacementBatchService _batchService;

        public HrdInitialPlacementBatchController(IHrdInitialPlacementBatchService batchService)
        {
            _batchService = batchService;
        }

        [HttpGet("template")]
        [ProducesResponseType(typeof(ApiResponse<List<InitialBatchRowResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Initial Placement Batch",
            Description = "Melihat template pra-isi 55 pegawai baseline yang belum memiliki penempatan jabatan/atasan",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("HrdInitialPlacementBatch", "Read")]
        public async Task<IActionResult> GetTemplate(CancellationToken ct)
        {
            var result = await _batchService.GetTemplateRowsAsync(ct);
            return Ok(ApiResponse<List<InitialBatchRowResponse>>.Ok(result, "Template baris batch berhasil diambil."));
        }

        [HttpPost("validate")]
        [ProducesResponseType(typeof(ApiResponse<BatchValidationResultResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Initial Placement Batch",
            Description = "Memvalidasi kelengkapan dan integritas baris batch penempatan awal",
            AccessType = AccessTypes.Read,
            SortOrder = 2)]
        [AccessPermission("HrdInitialPlacementBatch", "Read")]
        public async Task<IActionResult> ValidateRows([FromBody] List<CreateInitialBatchRowDto> rows, CancellationToken ct)
        {
            var result = await _batchService.ValidateRowsAsync(rows, ct);
            return Ok(ApiResponse<BatchValidationResultResponse>.Ok(result, "Hasil validasi batch berhasil diperoleh."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<InitialBatchDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Initial Placement Batch",
            Description = "Membuat draft batch penempatan awal",
            AccessType = AccessTypes.Create,
            SortOrder = 3)]
        [AccessPermission("HrdInitialPlacementBatch", "Create")]
        public async Task<IActionResult> CreateBatch([FromBody] CreateInitialBatchRequest request, CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Pengguna tidak terautentikasi."));
            }

            try
            {
                var result = await _batchService.CreateBatchDraftAsync(request, userId, ct);
                return StatusCode(StatusCodes.Status201Created,
                    ApiResponse<InitialBatchDetailResponse>.Ok(result, "Draft batch penempatan awal berhasil dibuat."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<InitialBatchListResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Initial Placement Batch",
            Description = "Melihat riwayat batch penempatan awal",
            AccessType = AccessTypes.Read,
            SortOrder = 4)]
        [AccessPermission("HrdInitialPlacementBatch", "Read")]
        public async Task<IActionResult> GetBatches([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            var result = await _batchService.GetBatchesAsync(page, pageSize, ct);
            return Ok(ApiResponse<PagedResult<InitialBatchListResponse>>.Ok(result, "Daftar batch berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InitialBatchDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Initial Placement Batch",
            Description = "Melihat detail baris batch penempatan awal",
            AccessType = AccessTypes.Read,
            SortOrder = 5)]
        [AccessPermission("HrdInitialPlacementBatch", "Read")]
        public async Task<IActionResult> GetBatchById(Guid id, CancellationToken ct)
        {
            var result = await _batchService.GetBatchByIdAsync(id, ct);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Batch penempatan awal tidak ditemukan."));
            }

            return Ok(ApiResponse<InitialBatchDetailResponse>.Ok(result, "Detail batch berhasil diambil."));
        }

        [HttpPost("{id:guid}/execute")]
        [ProducesResponseType(typeof(ApiResponse<InitialBatchDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Approve",
            "Approve Initial Placement Batch",
            Description = "Mengeksekusi penempatan posisi, atasan, dan gaji secara massal dalam transaksi atomik",
            AccessType = AccessTypes.Update,
            SortOrder = 6)]
        [AccessPermission("HrdInitialPlacementBatch", "Update")]
        public async Task<IActionResult> ExecuteBatch(Guid id, CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Pengguna tidak terautentikasi."));
            }

            try
            {
                var result = await _batchService.ExecuteBatchAsync(id, userId, ct);
                return Ok(ApiResponse<InitialBatchDetailResponse>.Ok(result, "Batch penempatan awal berhasil dieksekusi."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            var value = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out userId) && userId != Guid.Empty;
        }
    }
}
