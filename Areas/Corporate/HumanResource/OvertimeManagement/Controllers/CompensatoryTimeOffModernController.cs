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
    [Route("api/v1/corporate/human-resource/overtime/compensatory-time-off")]
    [Route("api/overtime/compensatory-time-off")]
    [Tags("HR Overtime - Compensatory Time Off (CTO)")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Compensatory Time Off (CTO)",
        AreaName = "Corporate",
        ControllerName = "CompensatoryTimeOffModern",
        Description = "Penerbitan hak libur pengganti lembur CTO (8 jam = 1 hari), masa berlaku 90 hari, dan sinkronisasi saldo cuti Sub-08",
        SortOrder = 5)]
    public class CompensatoryTimeOffModernController : ControllerBase
    {
        private readonly ICompensatoryTimeOffService _ctoService;
        private readonly ICompensatoryTimeOffExpirationService _expirationService;

        public CompensatoryTimeOffModernController(
            ICompensatoryTimeOffService ctoService,
            ICompensatoryTimeOffExpirationService expirationService)
        {
            _ctoService = ctoService ?? throw new ArgumentNullException(nameof(ctoService));
            _expirationService = expirationService ?? throw new ArgumentNullException(nameof(expirationService));
        }

        [HttpPost("generate/{realizationId:guid}")]
        [AccessAction("Create", "Menerbitkan Tiket CTO", Description = "Menerbitkan tiket hak cuti pengganti lembur 90 hari kalender dan posting mutasi kredit cuti", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("CompensatoryTimeOffModern", "Create")]
        [ProducesResponseType(typeof(ApiResponse<GenerateCompensatoryTimeOffResultDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> GenerateCto(
            Guid realizationId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _ctoService.GenerateCompensatoryTimeOffAsync(realizationId, actorUserId, cancellationToken);

            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<GenerateCompensatoryTimeOffResultDto>.Ok(result.Data!, result.Message)
                    : ApiResponse<GenerateCompensatoryTimeOffResultDto>.Fail(result.StatusCode, result.Message));
        }

        [HttpGet("staff-tickets/{workforceProfileId:guid}")]
        [AccessAction("View", "Melihat Tiket CTO Pegawai", Description = "Melihat seluruh riwayat tiket hak libur pengganti lembur milik pegawai tertentu", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("CompensatoryTimeOffModern", "View")]
        [ProducesResponseType(typeof(ApiResponse<List<CompensatoryTimeOffTicketDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStaffTickets(
            Guid workforceProfileId,
            CancellationToken cancellationToken = default)
        {
            var result = await _ctoService.GetStaffTicketsAsync(workforceProfileId, cancellationToken);
            return Ok(ApiResponse<List<CompensatoryTimeOffTicketDto>>.Ok(result, "Daftar tiket CTO pegawai berhasil diambil."));
        }

        [HttpPost("run-expiry")]
        [AccessAction("Execute", "Menjalankan Penghangusan CTO Kedaluwarsa", Description = "Mengeksekusi penghangusan tiket CTO yang telah melewati batas 90 hari kalender", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("CompensatoryTimeOffModern", "Execute")]
        [ProducesResponseType(typeof(ApiResponse<CompensatoryExpirationRunResultDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RunExpiry(CancellationToken cancellationToken = default)
        {
            var result = await _expirationService.ExecuteExpirationRunAsync(GetCurrentUserId(), cancellationToken);
            return Ok(ApiResponse<CompensatoryExpirationRunResultDto>.Ok(result, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
        }
    }
}
