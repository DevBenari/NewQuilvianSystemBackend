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
        Description = "Sinkronisasi piutang keuangan, simulasi, dan eksekusi pemotongan gaji",
        SortOrder = 42)]
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

        /// <summary>
        /// Simulasi evaluasi batas hard-cap 50% Gross Pay dan alokasi prioritas 1-4 untuk pegawai (DED-B1, DED-B2, PP 36/2021).
        /// </summary>
        [HttpGet("engine/simulate")]
        [AccessAction("Read", "Melihat Jadwal Pemotongan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("PayrollDeductionSchedule", "Read")]
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
