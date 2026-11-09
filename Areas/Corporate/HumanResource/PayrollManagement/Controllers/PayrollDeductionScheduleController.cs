using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Controllers
{
    [ApiController]
    [Route("api/hr-deductions")]
    [Authorize]
    public class PayrollDeductionScheduleController : ControllerBase
    {
        private readonly IFinanceDeductionSyncService _syncService;
        private readonly IPayrollDeductionEngineService _engineService;
        private readonly ILogger<PayrollDeductionScheduleController> _logger;

        public PayrollDeductionScheduleController(
            IFinanceDeductionSyncService syncService,
            IPayrollDeductionEngineService engineService,
            ILogger<PayrollDeductionScheduleController> logger)
        {
            _syncService = syncService;
            _engineService = engineService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var uid) ? uid : Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        /// <summary>
        /// Sinkronisasi jadwal piutang/pinjaman aktif dari Modul Keuangan Finance H-3 cut-off tgl 17 (INT-HR-57).
        /// </summary>
        [HttpPost("schedules/sync-finance")]
        [Tags("HR Deductions - Cashier Vouchers & Schedules")]
        public async Task<IActionResult> SyncFinanceReceivables(
            [FromQuery] int periodYear,
            [FromQuery] int periodMonth,
            [FromBody] List<FinanceReceivableSyncItemDto>? mockPayload,
            CancellationToken cancellationToken)
        {
            var result = await _syncService.SyncFinanceReceivablesAsync(periodYear, periodMonth, mockPayload, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Mengambil daftar jadwal cicilan pemotongan dengan filter kategori dan prioritas.
        /// </summary>
        [HttpGet("schedules")]
        [Tags("HR Deductions - Cashier Vouchers & Schedules")]
        public async Task<IActionResult> GetSchedules([FromQuery] DeductionScheduleFilterDto filter, CancellationToken cancellationToken)
        {
            var result = await _syncService.GetSchedulesAsync(filter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Mengambil jadwal cicilan pemotongan aktif untuk seorang pegawai urut prioritas 1-4.
        /// </summary>
        [HttpGet("schedules/employee/{employeeId:guid}")]
        [Tags("HR Deductions - Cashier Vouchers & Schedules")]
        public async Task<IActionResult> GetActiveSchedulesForEmployee(Guid employeeId, CancellationToken cancellationToken)
        {
            var result = await _syncService.GetActiveSchedulesForEmployeeAsync(employeeId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Simulasi evaluasi batas hard-cap 50% Gross Pay dan alokasi prioritas 1-4 untuk pegawai (DED-B1, DED-B2, PP 36/2021).
        /// </summary>
        [HttpGet("engine/simulate")]
        [Tags("HR Deductions - Engine")]
        public async Task<IActionResult> SimulateDeductions([FromQuery] Guid employeeId, [FromQuery] decimal grossPay, CancellationToken cancellationToken)
        {
            var result = await _engineService.SimulateEmployeeDeductionsAsync(employeeId, grossPay, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Eksekusi pemotongan gaji pada payroll run berjalan dengan penegakan hard-cap 50% dan pencatatan carry-over (DED-B1, DED-B3).
        /// </summary>
        [HttpPost("engine/execute")]
        [Tags("HR Deductions - Engine")]
        public async Task<IActionResult> ExecuteDeductions([FromBody] ProcessPayrollDeductionsRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _engineService.ExecutePayrollDeductionsAsync(request.PayrollRunId, request.EmployeeId, request.GrossPay, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }
    }
}
