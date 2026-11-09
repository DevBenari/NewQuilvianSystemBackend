using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Controllers
{
    [ApiController]
    [Route("api/hr-deductions/exit-clearance")]
    [Authorize]
    public class ExitClearanceReceivableController : ControllerBase
    {
        private readonly IExitClearanceReceivableService _exitClearanceService;
        private readonly IFinanceDeductionSyncService _financeSyncService;
        private readonly ILogger<ExitClearanceReceivableController> _logger;

        public ExitClearanceReceivableController(
            IExitClearanceReceivableService exitClearanceService,
            IFinanceDeductionSyncService financeSyncService,
            ILogger<ExitClearanceReceivableController> logger)
        {
            _exitClearanceService = exitClearanceService;
            _financeSyncService = financeSyncService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var uid) ? uid : Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        /// <summary>
        /// Simulasi dan kalkulasi pelunasan utang pinjaman pada pesangon penutupan Exit Clearance Sub-20 dengan batas 100% (BE-HRD-174, DED-B7, VR-DED-07).
        /// </summary>
        [HttpPost("calculate")]
        [Tags("HR Deductions - Exit Clearance & Webhooks")]
        public async Task<IActionResult> CalculateExitClearanceDeduction(
            [FromBody] ExitClearanceReceivableCalculationRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _exitClearanceService.CalculateExitClearanceDeductionAsync(request, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Konfirmasi eksekusi pemotongan utang pesangon akhir dan penutupan jadwal cicilan (ClearedAtSeparation / BlockedPendingSettlement) (BE-HRD-174, INT-HR-60, DED-W5).
        /// </summary>
        [HttpPost("confirm-settlement")]
        [Tags("HR Deductions - Exit Clearance & Webhooks")]
        public async Task<IActionResult> ConfirmExitClearanceSettlement(
            [FromBody] ConfirmExitClearanceSettlementRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _exitClearanceService.ConfirmExitClearanceSettlementAsync(request, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Transmisi webhook callback realisasi pemotongan bulanan tanggal 25 ke Departemen Keuangan (BE-HRD-174, INT-HR-58, HRD-DEC-129).
        /// </summary>
        [HttpPost("finance-realization-callback")]
        [Tags("HR Deductions - Exit Clearance & Webhooks")]
        public async Task<IActionResult> SendFinanceRealizationCallback(
            [FromQuery] int periodYear,
            [FromQuery] int periodMonth,
            [FromQuery] Guid payrollRunId,
            CancellationToken cancellationToken)
        {
            var result = await _financeSyncService.SendDeductionRealizationCallbackToFinanceAsync(
                periodYear, periodMonth, payrollRunId, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }
    }
}
