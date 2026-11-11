using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/overtime/plans")]
    [Route("api/overtime/plans")]
    [Tags("HR Overtime - Plans & ESS Requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Plans ESS",
        AreaName = "Corporate",
        ControllerName = "OvertimePlanEss",
        Description = "Pengajuan dan pengelolaan Surat Perintah Kerja Lembur (SPKL) terencana pra-dinas (H-1)",
        SortOrder = 2)]
    public class OvertimePlanEssController : ControllerBase
    {
        private readonly IOvertimePlanService _planService;

        public OvertimePlanEssController(IOvertimePlanService planService)
        {
            _planService = planService ?? throw new ArgumentNullException(nameof(planService));
        }

        [HttpGet("my")]
        [AccessAction("ViewSelf", "Melihat Daftar SPKL Pribadi", Description = "Melihat riwayat pengajuan SPKL terencana staf aktif", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePlan", "ViewSelf")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OvertimePlanSummaryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyPlans(
            [FromQuery] string? status = null,
            [FromQuery] int? year = null,
            [FromQuery] int? month = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _planService.GetMyPlansAsync(actorUserId, status, year, month, page, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<OvertimePlanSummaryDto>>.Ok(result, "Daftar SPKL terencana berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("View", "Melihat Detail SPKL", Description = "Melihat rincian SPKL dan daftar nakes pelaksana", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OvertimePlan", "View")]
        [ProducesResponseType(typeof(ApiResponse<OvertimePlanDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var detail = await _planService.GetPlanByIdAsync(id, cancellationToken);
            if (detail == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rencana lembur (SPKL) tidak ditemukan atau sudah dihapus."));
            }
            return Ok(ApiResponse<OvertimePlanDetailDto>.Ok(detail, "Detail SPKL berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Mengajukan SPKL Pra-Dinas", Description = "Mengajukan SPKL pra-dinas terencana H-1 individu atau kelompok tim", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("OvertimePlan", "Create")]
        [ProducesResponseType(typeof(ApiResponse<OvertimePlanResultDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreatePlan(
            [FromBody] CreateOvertimePlanCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _planService.CreatePlanAsync(command, actorUserId, cancellationToken);
            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<OvertimePlanResultDto>.Ok(result.Data!, result.Message)
                    : ApiResponse<OvertimePlanResultDto>.Fail(result.StatusCode, result.Message));
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("CancelSelf", "Membatalkan Rencana SPKL", Description = "Membatalkan rencana SPKL sebelum jam dinas lembur terlaksana", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OvertimePlan", "CancelSelf")]
        [ProducesResponseType(typeof(ApiResponse<OvertimePlanResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CancelPlan(
            Guid id,
            [FromBody] CancelOvertimePlanCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _planService.CancelPlanAsync(id, command, actorUserId, cancellationToken);
            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<OvertimePlanResultDto>.Ok(result.Data!, result.Message)
                    : ApiResponse<OvertimePlanResultDto>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
        }
    }
}
