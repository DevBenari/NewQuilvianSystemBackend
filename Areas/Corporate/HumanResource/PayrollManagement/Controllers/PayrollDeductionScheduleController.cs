using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services;
using System.Security.Claims;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Controllers
{
    [ApiController]
    [Route("api/hr-deductions")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PAYROLL_MANAGEMENT",
        moduleName: "Human Resource Payroll Management",
        displayName: "Jadwal Pemotongan Payroll",
        AreaName = "Corporate",
        ControllerName = "PayrollDeductionSchedule",
        Description = "Sinkronisasi piutang keuangan, jadwal angsuran, dan eksekusi pemotongan gaji",
        SortOrder = 42)]
    public class PayrollDeductionScheduleController : ControllerBase
    {
        private readonly IFinanceDeductionSyncService _syncService;
        private readonly IFinanceInstallmentScheduleService _financeScheduleService;
        private readonly IPayrollDeductionEngineService _engineService;
        private readonly ILogger<PayrollDeductionScheduleController> _logger;

        public PayrollDeductionScheduleController(
            IFinanceDeductionSyncService syncService,
            IFinanceInstallmentScheduleService financeScheduleService,
            IPayrollDeductionEngineService engineService,
            ILogger<PayrollDeductionScheduleController> logger)
        {
            _syncService = syncService;
            _financeScheduleService = financeScheduleService;
            _engineService = engineService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(claim, out var uid) && uid != Guid.Empty)
            {
                return uid;
            }

            // Tidak memakai aktor cadangan: jejak audit tanpa identitas asli lebih berbahaya
            // daripada permintaan yang ditolak.
            throw new UnauthorizedAccessException("Identitas user pada token tidak valid.");
        }

        /// <summary>
        /// Sinkronisasi jadwal piutang/pinjaman aktif dari Modul Keuangan Finance H-3 cut-off tgl 17 (INT-HR-57).
        /// </summary>
        [HttpPost("schedules/sync-finance")]
        [AccessAction("Manage", "Menyinkronkan Piutang ke Jadwal Potongan", AccessType = AccessTypes.Update, SortOrder = 1)]
        [AccessPermission("PayrollDeductionSchedule", "Manage")]
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
        [AccessAction("Read", "Melihat Jadwal Pemotongan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("PayrollDeductionSchedule", "Read")]
        [Tags("HR Deductions - Cashier Vouchers & Schedules")]
        public async Task<IActionResult> GetSchedules([FromQuery] DeductionScheduleFilterDto filter, CancellationToken cancellationToken)
        {
            var result = await _syncService.GetSchedulesAsync(filter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Jadwal angsuran potongan pegawai yang dibaca LANGSUNG dari Finance AR (perjanjian dan angsuran per periode), tanpa
        /// sinkronisasi dan tanpa mengubah data apa pun. Disertai jadwal HRD lama yang bukan salinan perjanjian Finance (PAY-BE-FIX-029).
        /// </summary>
        [HttpGet("schedules/finance")]
        [AccessAction("Read", "Melihat Jadwal Pemotongan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("PayrollDeductionSchedule", "Read")]
        [Tags("HR Deductions - Cashier Vouchers & Schedules")]
        public async Task<IActionResult> GetFinanceSchedules([FromQuery] FinanceInstallmentScheduleFilterDto filter, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _financeScheduleService.GetAsync(filter, cancellationToken);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengambil jadwal cicilan pemotongan aktif untuk seorang pegawai urut prioritas 1-4.
        /// </summary>
        [HttpGet("schedules/employee/{employeeId:guid}")]
        [AccessAction("Read", "Melihat Jadwal Pemotongan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("PayrollDeductionSchedule", "Read")]
        [Tags("HR Deductions - Cashier Vouchers & Schedules")]
        public async Task<IActionResult> GetActiveSchedulesForEmployee(Guid employeeId, CancellationToken cancellationToken)
        {
            var result = await _syncService.GetActiveSchedulesForEmployeeAsync(employeeId, cancellationToken);
            return Ok(result);
        }

        // PAY-BE-FIX-029: endpoint publik GET engine/simulate dipensiunkan. Perhitungan batas potongan tidak dihapus: tetap dipakai
        // ExecutePayrollDeductionsAsync lewat metode internal engine.

        /// <summary>
        /// Eksekusi pemotongan gaji pada payroll run berjalan dengan penegakan hard-cap 50% dan pencatatan carry-over (DED-B1, DED-B3).
        /// </summary>
        [HttpPost("engine/execute")]
        [AccessAction("Execute", "Mengeksekusi Pemotongan Gaji", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PayrollDeductionSchedule", "Execute")]
        [Tags("HR Deductions - Engine")]
        public async Task<IActionResult> ExecuteDeductions([FromBody] ProcessPayrollDeductionsRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _engineService.ExecutePayrollDeductionsAsync(request.PayrollRunId, request.EmployeeId, request.GrossPay, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }
    }
}
