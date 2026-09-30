using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/daily-nursing-actions")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Daily Nursing Actions",
        AreaName = "HealthServices",
        ControllerName = "DailyNursingAction",
        Description = "Health service master data daily nursing actions checklist",
        SortOrder = 13
    )]
    [Tags("Health Services / Master Data / Daily Nursing Actions")]
    public class DailyNursingActionController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.DailyNursingAction";

        private readonly DailyNursingActionService _service;
        private readonly LoggerService _loggerService;

        public DailyNursingActionController(
            DailyNursingActionService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var uid) ? uid : Guid.Empty;
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DailyNursingActionSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Daily Nursing Action", Description = "Melihat ringkasan master tindakan harian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DailyNursingAction", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken ct)
        {
            var summary = await _service.GetSummaryAsync(ct);
            return Ok(ApiResponse<DailyNursingActionSummaryResponse>.Ok(summary, "Ringkasan master tindakan harian berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DailyNursingActionListItemDto>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Daily Nursing Action", Description = "Melihat daftar master tindakan harian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DailyNursingAction", "Read")]
        public async Task<IActionResult> GetPagedList(
            [FromQuery] string? search,
            [FromQuery] string? category,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int perPage = 25,
            CancellationToken ct = default)
        {
            var result = await _service.GetPagedListAsync(search, category, isActive, page, perPage, ct);
            return Ok(ApiResponse<PagedResult<DailyNursingActionListItemDto>>.Ok(result, "Daftar master tindakan harian berhasil diambil."));
        }

        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<List<DailyNursingActionActiveItemDto>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Daily Nursing Action", Description = "Melihat daftar master tindakan harian aktif", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DailyNursingAction", "Read")]
        public async Task<IActionResult> GetActiveList(
            [FromQuery] string? category,
            CancellationToken ct = default)
        {
            var result = await _service.GetActiveListAsync(category, ct);
            return Ok(ApiResponse<List<DailyNursingActionActiveItemDto>>.Ok(result, "Daftar master tindakan harian aktif berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DailyNursingActionDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Daily Nursing Action", Description = "Melihat detail master tindakan harian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DailyNursingAction", "Read")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Master tindakan harian tidak ditemukan."));
            }

            return Ok(ApiResponse<DailyNursingActionDetailDto>.Ok(result, "Detail master tindakan harian berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DailyNursingActionDetailDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Daily Nursing Action", Description = "Menambahkan master tindakan harian", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DailyNursingAction", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateDailyNursingActionRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Validasi formulir gagal."));
            }

            var actorUserId = GetCurrentUserId();
            var (success, error, result) = await _service.CreateAsync(request, actorUserId, ct);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, error ?? "Gagal menambahkan tindakan harian."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "CreateDailyNursingAction",
                $"Menambahkan master tindakan harian: {result?.ActionCode} - {result?.ActionName}",
                result);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<DailyNursingActionDetailDto>.Ok(result!, "Master tindakan harian berhasil ditambahkan."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DailyNursingActionDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Daily Nursing Action", Description = "Memperbarui master tindakan harian", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DailyNursingAction", "Update")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateDailyNursingActionRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Validasi formulir gagal."));
            }

            var actorUserId = GetCurrentUserId();
            var (success, error, result) = await _service.UpdateAsync(id, request, actorUserId, ct);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, error ?? "Gagal memperbarui tindakan harian."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "UpdateDailyNursingAction",
                $"Memperbarui master tindakan harian ID: {id} - {result?.ActionName}",
                result);

            return Ok(ApiResponse<DailyNursingActionDetailDto>.Ok(result!, "Master tindakan harian berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/toggle-active")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Daily Nursing Action", Description = "Mengubah status aktif master tindakan harian", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DailyNursingAction", "Update")]
        public async Task<IActionResult> ToggleActive([FromRoute] Guid id, CancellationToken ct)
        {
            var actorUserId = GetCurrentUserId();
            var (success, error) = await _service.ToggleActiveAsync(id, actorUserId, ct);

            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, error ?? "Master tindakan harian tidak ditemukan."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "ToggleActiveDailyNursingAction",
                $"Mengubah status aktif master tindakan harian ID: {id}");

            return Ok(ApiResponse<bool>.Ok(true, "Status aktif master tindakan harian berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Daily Nursing Action", Description = "Menghapus master tindakan harian", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DailyNursingAction", "Delete")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
        {
            var actorUserId = GetCurrentUserId();
            var (success, error) = await _service.DeleteAsync(id, actorUserId, ct);

            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, error ?? "Master tindakan harian tidak ditemukan."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "DeleteDailyNursingAction",
                $"Menghapus master tindakan harian ID: {id}");

            return Ok(ApiResponse<bool>.Ok(true, "Master tindakan harian berhasil dihapus."));
        }
    }
}
