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
    [Route("api/v1/corporate/human-resource/overtime/periods")]
    [Route("api/overtime/periods")]
    [Tags("HR Overtime - Period & Retrospective Reconciliation")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Period & Reconciliation",
        AreaName = "Corporate",
        ControllerName = "OvertimePeriodModern",
        Description = "Manajemen periode cut-off bulanan lembur tgl 20, penguncian data, rekonsiliasi retrospektif klaim susulan",
        SortOrder = 6)]
    public class OvertimePeriodModernController : ControllerBase
    {
        private readonly IOvertimePeriodManagementService _periodService;

        public OvertimePeriodModernController(IOvertimePeriodManagementService periodService)
        {
            _periodService = periodService ?? throw new ArgumentNullException(nameof(periodService));
        }

        [HttpGet("current")]
        [AccessAction("View", "Melihat Periode Lembur Aktif", Description = "Mengambil data periode lembur berjalan dan tanggal cut-off tgl 20", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePeriodModern", "View")]
        [ProducesResponseType(typeof(ApiResponse<OvertimePeriodDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentPeriod(CancellationToken cancellationToken = default)
        {
            var period = await _periodService.GetOrCreateCurrentPeriodAsync(cancellationToken);
            return Ok(ApiResponse<OvertimePeriodDto>.Ok(period, "Periode lembur aktif berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("View", "Melihat Daftar Seluruh Periode Lembur", Description = "Melihat riwayat seluruh periode lembur bulanan rumah sakit", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OvertimePeriodModern", "View")]
        [ProducesResponseType(typeof(ApiResponse<List<OvertimePeriodDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPeriods(CancellationToken cancellationToken = default)
        {
            var list = await _periodService.GetAllPeriodsAsync(cancellationToken);
            return Ok(ApiResponse<List<OvertimePeriodDto>>.Ok(list, "Daftar periode lembur berhasil diambil."));
        }

        [HttpPost("{id:guid}/close")]
        [AccessAction("Close", "Menutup dan Mengunci Periode Lembur", Description = "Menutup siklus cut-off bulanan, mengunci realisasi lembur, dan memproses rekonsiliasi retrospektif", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OvertimePeriodModern", "Close")]
        [ProducesResponseType(typeof(ApiResponse<OvertimePeriodOperationResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ClosePeriod(
            Guid id,
            [FromBody] CloseOvertimePeriodCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _periodService.ClosePeriodAsync(id, command, actorUserId, cancellationToken);

            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<OvertimePeriodOperationResultDto>.Ok(result.Data!, result.Message)
                    : ApiResponse<OvertimePeriodOperationResultDto>.Fail(result.StatusCode, result.Message));
        }

        [HttpPost("{id:guid}/reopen")]
        [AccessAction("Reopen", "Membuka Kembali Periode Lembur", Description = "Membuka kembali periode terkunci untuk penyesuaian koreksi administratif penting", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OvertimePeriodModern", "Reopen")]
        [ProducesResponseType(typeof(ApiResponse<OvertimePeriodOperationResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ReopenPeriod(
            Guid id,
            [FromBody] ReopenOvertimePeriodCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _periodService.ReopenPeriodAsync(id, command, actorUserId, cancellationToken);

            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<OvertimePeriodOperationResultDto>.Ok(result.Data!, result.Message)
                    : ApiResponse<OvertimePeriodOperationResultDto>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
        }
    }
}
