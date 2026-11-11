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
    [Route("api/v1/corporate/human-resource/overtime/realizations")]
    [Route("api/overtime/realizations")]
    [Tags("HR Overtime - Realizations & Cito Claims")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Realization & Cito Claims",
        AreaName = "Corporate",
        ControllerName = "OvertimeRealizationEss",
        Description = "Pengajuan realisasi lembur terencana SPKL dan klaim darurat medis cito pasca-dinas H+1",
        SortOrder = 2)]
    public class OvertimeRealizationEssController : ControllerBase
    {
        private readonly IOvertimeRealizationService _realizationService;

        public OvertimeRealizationEssController(IOvertimeRealizationService realizationService)
        {
            _realizationService = realizationService ?? throw new ArgumentNullException(nameof(realizationService));
        }

        [HttpPost]
        [AccessAction("Create", "Mengajukan Klaim Realisasi Lembur", Description = "Mengajukan klaim realisasi lembur terencana atau darurat medis cito", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("OvertimeRealizationEss", "Create")]
        [ProducesResponseType(typeof(ApiResponse<OvertimeRealizationResultDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitRealization(
            [FromBody] SubmitOvertimeRealizationCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _realizationService.SubmitRealizationAsync(command, actorUserId, cancellationToken);

            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<OvertimeRealizationResultDto>.Ok(result.Data!, result.Message)
                    : ApiResponse<OvertimeRealizationResultDto>.Fail(result.StatusCode, result.Message));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("View", "Melihat Detail Realisasi Lembur", Description = "Melihat detail realisasi lembur, data klinis, dan status persetujuan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OvertimeRealizationEss", "View")]
        [ProducesResponseType(typeof(ApiResponse<OvertimeRealizationDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var detail = await _realizationService.GetRealizationByIdAsync(id, cancellationToken);
            if (detail == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data realisasi lembur tidak ditemukan."));
            }

            return Ok(ApiResponse<OvertimeRealizationDetailDto>.Ok(detail, "Detail realisasi lembur berhasil diambil."));
        }

        [HttpGet("my")]
        [AccessAction("View", "Melihat Riwayat Realisasi Lembur Sendiri", Description = "Melihat riwayat klaim realisasi lembur yang pernah diajukan pegawai", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("OvertimeRealizationEss", "View")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OvertimeRealizationSummaryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyRealizations(
            [FromQuery] Guid workforceProfileId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _realizationService.GetMyRealizationsAsync(workforceProfileId, page, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<OvertimeRealizationSummaryDto>>.Ok(result, "Riwayat klaim lembur berhasil diambil."));
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Update", "Membatalkan Klaim Realisasi Lembur", Description = "Membatalkan klaim lembur yang belum diproses HR", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OvertimeRealizationEss", "Update")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelRealization(
            Guid id,
            [FromBody] CancelOvertimeRealizationCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _realizationService.CancelRealizationAsync(id, command, actorUserId, cancellationToken);

            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<bool>.Ok(result.Data, result.Message)
                    : ApiResponse<bool>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
        }
    }
}
