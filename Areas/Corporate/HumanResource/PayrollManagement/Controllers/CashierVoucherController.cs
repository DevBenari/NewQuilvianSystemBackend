using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Controllers
{
    [ApiController]
    [Route("api/hr-deductions/vouchers")]
    [Authorize]
    public class CashierVoucherController : ControllerBase
    {
        private readonly ICashierVoucherDisbursementService _voucherService;
        private readonly ILogger<CashierVoucherController> _logger;

        public CashierVoucherController(
            ICashierVoucherDisbursementService voucherService,
            ILogger<CashierVoucherController> logger)
        {
            _voucherService = voucherService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var uid) ? uid : Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        /// <summary>
        /// Penerbitan voucher kasir darurat medis cito nakes 24/7 ber-token SHA-256 kedaluwarsa 48 jam (BE-HRD-172, HRD-DEC-133).
        /// </summary>
        [HttpPost]
        [Tags("HR Deductions - Cashier Vouchers & Schedules")]
        public async Task<IActionResult> IssueVoucher(
            [FromBody] IssueCashierVoucherRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _voucherService.IssueVoucherAsync(request, GetCurrentUserId(), cancellationToken);
            return CreatedAtAction(nameof(GetVoucherById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Mengambil daftar voucher kasir darurat dengan filter status dan rentang tanggal.
        /// </summary>
        [HttpGet]
        [Tags("HR Deductions - Cashier Vouchers & Schedules")]
        public async Task<IActionResult> GetVouchers(
            [FromQuery] CashierVoucherFilterDto filter,
            CancellationToken cancellationToken)
        {
            var result = await _voucherService.GetVouchersAsync(filter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Mengambil detail voucher kasir berdasarkan ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        [Tags("HR Deductions - Cashier Vouchers & Schedules")]
        public async Task<IActionResult> GetVoucherById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _voucherService.GetVoucherByIdAsync(id, cancellationToken);
            if (result == null) return NotFound(new { message = $"Voucher kasir dengan ID {id} tidak ditemukan." });
            return Ok(result);
        }

        /// <summary>
        /// Verifikasi token kriptografi SHA-256 dan keabsahan voucher di loket Kasir RS 24/7 sebelum pencairan tunai (BE-HRD-173, INT-HR-59).
        /// </summary>
        [HttpPost("verify")]
        [Tags("HR Deductions - Cashier Vouchers & Schedules")]
        public async Task<IActionResult> VerifyVoucher(
            [FromBody] VerifyCashierVoucherRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _voucherService.VerifyVoucherAsync(request, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Kliring dan pencairan tunai voucher di Loket Kasir RS 24/7 dengan penguncian mutlak DisbursedViaCashier (BE-HRD-173, INT-HR-59, DED-B6, VR-DED-06).
        /// </summary>
        [HttpPost("disburse")]
        [Tags("HR Deductions - Cashier Vouchers & Schedules")]
        public async Task<IActionResult> ClaimVoucherAtCashier(
            [FromBody] ClaimCashierVoucherRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _voucherService.ClaimVoucherAtCashierAsync(request, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }
    }
}
