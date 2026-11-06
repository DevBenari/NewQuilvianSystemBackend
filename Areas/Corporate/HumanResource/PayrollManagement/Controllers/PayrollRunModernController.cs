using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/payroll/runs")]
    [Route("api/payroll/runs")]
    [Tags("HR Payroll - Runs & Batches")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PAYROLL",
        moduleName: "Human Resource Payroll",
        displayName: "Payroll Runs & Batches",
        AreaName = "Corporate",
        ControllerName = "PayrollRunModern",
        Description = "Inisiasi batch siklus penggajian bulanan, validasi precondition gate, dan engine kalkulasi penghasilan tetap",
        SortOrder = 2)]
    public class PayrollRunModernController : ControllerBase
    {
        private readonly IPayrollPreconditionGateService _preconditionGateService;
        private readonly IPayrollRunInitializationService _runInitializationService;
        private readonly IPayrollEarningsCalculationService _earningsCalculationService;
        private readonly IPayrollOvertimeAndShiftIngestionService _overtimeAndShiftService;
        private readonly IPayrollBpjsCalculationService _bpjsCalculationService;
        private readonly IPayrollTaxCalculationService _taxCalculationService;

        public PayrollRunModernController(
            IPayrollPreconditionGateService preconditionGateService,
            IPayrollRunInitializationService runInitializationService,
            IPayrollEarningsCalculationService earningsCalculationService,
            IPayrollOvertimeAndShiftIngestionService overtimeAndShiftService,
            IPayrollBpjsCalculationService bpjsCalculationService,
            IPayrollTaxCalculationService taxCalculationService)
        {
            _preconditionGateService = preconditionGateService ?? throw new ArgumentNullException(nameof(preconditionGateService));
            _runInitializationService = runInitializationService ?? throw new ArgumentNullException(nameof(runInitializationService));
            _earningsCalculationService = earningsCalculationService ?? throw new ArgumentNullException(nameof(earningsCalculationService));
            _overtimeAndShiftService = overtimeAndShiftService ?? throw new ArgumentNullException(nameof(overtimeAndShiftService));
            _bpjsCalculationService = bpjsCalculationService ?? throw new ArgumentNullException(nameof(bpjsCalculationService));
            _taxCalculationService = taxCalculationService ?? throw new ArgumentNullException(nameof(taxCalculationService));
        }

        [HttpGet("preconditions")]
        [AccessAction("ViewPreconditions", "Pengecekan Precondition Gate Payroll", Description = "Memverifikasi penguncian modul Presensi, Cuti, dan Lembur sebelum pembuatan batch", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollRunModern", "ViewPreconditions")]
        [ProducesResponseType(typeof(ApiResponse<PreconditionGateCheckResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckPreconditions(
            [FromQuery] int year,
            [FromQuery] int month,
            CancellationToken cancellationToken = default)
        {
            if (year < 2020 || year > 2100 || month < 1 || month > 12)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Parameter tahun atau bulan tidak valid."));
            }

            var result = await _preconditionGateService.CheckPreconditionsAsync(year, month, cancellationToken);
            return Ok(ApiResponse<PreconditionGateCheckResponse>.Ok(result, "Pengecekan precondition gate berhasil."));
        }

        [HttpPost]
        [AccessAction("Create", "Inisiasi Batch Penggajian Baru", Description = "Membuat draft batch penggajian bulanan baru dan mempopulasi pegawai aktif", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PayrollRunModern", "Create")]
        [ProducesResponseType(typeof(ApiResponse<PayrollRunOperationResultDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> InitializeRun(
            [FromBody] CreatePayrollRunCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _runInitializationService.InitializePayrollRunAsync(command, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return StatusCode(result.StatusCode, ApiResponse<PayrollRunOperationResultDto>.Ok(result.Data!, result.Message));
        }

        [HttpGet]
        [AccessAction("View", "Melihat Daftar Batch Penggajian", Description = "Melihat seluruh riwayat dan daftar batch penggajian rumah sakit", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("PayrollRunModern", "View")]
        [ProducesResponseType(typeof(ApiResponse<List<PayrollRunSummaryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRuns(CancellationToken cancellationToken = default)
        {
            var runs = await _runInitializationService.GetAllPayrollRunsAsync(cancellationToken);
            return Ok(ApiResponse<List<PayrollRunSummaryDto>>.Ok(runs, "Daftar batch penggajian berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("ViewDetail", "Melihat Detail Batch Penggajian", Description = "Melihat informasi lengkap batch penggajian beserta daftar pegawai", AccessType = AccessTypes.Read, SortOrder = 4)]
        [AccessPermission("PayrollRunModern", "ViewDetail")]
        [ProducesResponseType(typeof(ApiResponse<PayrollRunDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRunDetail(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var detail = await _runInitializationService.GetPayrollRunDetailAsync(id, cancellationToken);
            if (detail == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, $"Batch penggajian dengan ID {id} tidak ditemukan."));
            }

            return Ok(ApiResponse<PayrollRunDetailDto>.Ok(detail, "Detail batch penggajian berhasil diambil."));
        }

        [HttpPost("{id:guid}/calculate-earnings")]
        [AccessAction("CalculateEarnings", "Kalkulasi Penghasilan Tetap Batch", Description = "Menghitung gaji pokok, tunjangan struktural, fungsional, dan risiko medis seluruh pegawai", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PayrollRunModern", "CalculateEarnings")]
        [ProducesResponseType(typeof(ApiResponse<PayrollEarningsCalculationResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CalculateFixedEarnings(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _earningsCalculationService.CalculateFixedEarningsAsync(id, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<PayrollEarningsCalculationResultDto>.Ok(result.Data!, result.Message));
        }

        [HttpPost("{id:guid}/employees/{employeeRunId:guid}/calculate-earnings")]
        [AccessAction("CalculateEmployeeEarnings", "Kalkulasi Penghasilan Tetap Pegawai", Description = "Menghitung gaji pokok, tunjangan struktural, fungsional, dan risiko medis satu pegawai", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PayrollRunModern", "CalculateEmployeeEarnings")]
        [ProducesResponseType(typeof(ApiResponse<PayrollEmployeeEarningsResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CalculateEmployeeFixedEarnings(
            Guid id,
            Guid employeeRunId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _earningsCalculationService.CalculateEmployeeFixedEarningsAsync(id, employeeRunId, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<PayrollEmployeeEarningsResultDto>.Ok(result.Data!, result.Message));
        }

        [HttpPost("{id:guid}/ingest-overtime-and-shift")]
        [AccessAction("IngestOvertimeAndShift", "Penyerapan Lembur & Premi Shift Malam Batch", Description = "Menyerap data lembur Sub-09 dan premi shift malam Sub-07 seluruh pegawai batch", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("PayrollRunModern", "IngestOvertimeAndShift")]
        [ProducesResponseType(typeof(ApiResponse<IngestOvertimeAndShiftResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> IngestOvertimeAndShift(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _overtimeAndShiftService.IngestOvertimeAndShiftAsync(id, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<IngestOvertimeAndShiftResultDto>.Ok(result.Data!, result.Message));
        }

        [HttpPost("{id:guid}/employees/{employeeRunId:guid}/ingest-overtime-and-shift")]
        [AccessAction("IngestEmployeeOvertimeAndShift", "Penyerapan Lembur & Premi Shift Malam Pegawai", Description = "Menyerap data lembur Sub-09 dan premi shift malam Sub-07 satu pegawai", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("PayrollRunModern", "IngestEmployeeOvertimeAndShift")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeVariableEarningSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> IngestEmployeeOvertimeAndShift(
            Guid id,
            Guid employeeRunId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _overtimeAndShiftService.IngestEmployeeOvertimeAndShiftAsync(id, employeeRunId, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<EmployeeVariableEarningSummaryDto>.Ok(result.Data!, result.Message));
        }

        [HttpPost("{id:guid}/calculate-bpjs")]
        [AccessAction("CalculateBpjs", "Kalkulasi Iuran BPJS Batch", Description = "Menghitung iuran BPJS Kesehatan dan 4 program BPJS Ketenagakerjaan berplafon seluruh pegawai", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission("PayrollRunModern", "CalculateBpjs")]
        [ProducesResponseType(typeof(ApiResponse<CalculateBpjsResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CalculateBpjs(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _bpjsCalculationService.CalculateBpjsContributionsAsync(id, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<CalculateBpjsResultDto>.Ok(result.Data!, result.Message));
        }

        [HttpPost("{id:guid}/employees/{employeeRunId:guid}/calculate-bpjs")]
        [AccessAction("CalculateEmployeeBpjs", "Kalkulasi Iuran BPJS Pegawai", Description = "Menghitung iuran BPJS Kesehatan dan 4 program BPJS Ketenagakerjaan satu pegawai", AccessType = AccessTypes.Update, SortOrder = 10)]
        [AccessPermission("PayrollRunModern", "CalculateEmployeeBpjs")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBpjsCalculationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CalculateEmployeeBpjs(
            Guid id,
            Guid employeeRunId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _bpjsCalculationService.CalculateEmployeeBpjsAsync(id, employeeRunId, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<EmployeeBpjsCalculationDto>.Ok(result.Data!, result.Message));
        }

        [HttpPost("{id:guid}/calculate-pph21")]
        [AccessAction("CalculatePph21", "Kalkulasi Pemotongan PPh 21 TER Batch", Description = "Menghitung pemotongan PPh 21 bulanan Tarif Efektif Rata-Rata (TER A, B, C) PMK 168/2023 seluruh pegawai", AccessType = AccessTypes.Update, SortOrder = 11)]
        [AccessPermission("PayrollRunModern", "CalculatePph21")]
        [ProducesResponseType(typeof(ApiResponse<CalculatePph21ResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CalculatePph21(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _taxCalculationService.CalculatePph21TerAsync(id, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<CalculatePph21ResultDto>.Ok(result.Data!, result.Message));
        }

        [HttpPost("{id:guid}/employees/{employeeRunId:guid}/calculate-pph21")]
        [AccessAction("CalculateEmployeePph21", "Kalkulasi Pemotongan PPh 21 Pegawai", Description = "Menghitung pemotongan PPh 21 bulanan Tarif Efektif Rata-Rata (TER) satu pegawai", AccessType = AccessTypes.Update, SortOrder = 12)]
        [AccessPermission("PayrollRunModern", "CalculateEmployeePph21")]
        [ProducesResponseType(typeof(ApiResponse<EmployeePph21CalculationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CalculateEmployeePph21(
            Guid id,
            Guid employeeRunId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _taxCalculationService.CalculateEmployeePph21TerAsync(id, employeeRunId, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<EmployeePph21CalculationDto>.Ok(result.Data!, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var rawId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(rawId, out var id) ? id : Guid.Empty;
        }
    }
}
