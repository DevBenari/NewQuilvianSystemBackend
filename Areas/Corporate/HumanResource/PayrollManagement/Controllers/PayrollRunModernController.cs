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
        private readonly IPayrollDeductionsIngestionService _deductionsService;
        private readonly IPayrollVarianceGuardService _varianceGuardService;
        private readonly IPayrollVarianceResolutionService _varianceResolutionService;
        private readonly IPayrollApprovalWorkflowService _approvalService;
        private readonly IPayrollFinanceHandoffService _financeHandoffService;
        private readonly IPayrollDisbursementReconciliationService _reconciliationService;
        private readonly IPayrollPayslipDistributionService _payslipService;

        public PayrollRunModernController(
            IPayrollPreconditionGateService preconditionGateService,
            IPayrollRunInitializationService runInitializationService,
            IPayrollEarningsCalculationService earningsCalculationService,
            IPayrollOvertimeAndShiftIngestionService overtimeAndShiftService,
            IPayrollBpjsCalculationService bpjsCalculationService,
            IPayrollTaxCalculationService taxCalculationService,
            IPayrollDeductionsIngestionService deductionsService,
            IPayrollVarianceGuardService varianceGuardService,
            IPayrollVarianceResolutionService varianceResolutionService,
            IPayrollApprovalWorkflowService approvalService,
            IPayrollFinanceHandoffService financeHandoffService,
            IPayrollDisbursementReconciliationService reconciliationService,
            IPayrollPayslipDistributionService payslipService)
        {
            _preconditionGateService = preconditionGateService ?? throw new ArgumentNullException(nameof(preconditionGateService));
            _runInitializationService = runInitializationService ?? throw new ArgumentNullException(nameof(runInitializationService));
            _earningsCalculationService = earningsCalculationService ?? throw new ArgumentNullException(nameof(earningsCalculationService));
            _overtimeAndShiftService = overtimeAndShiftService ?? throw new ArgumentNullException(nameof(overtimeAndShiftService));
            _bpjsCalculationService = bpjsCalculationService ?? throw new ArgumentNullException(nameof(bpjsCalculationService));
            _taxCalculationService = taxCalculationService ?? throw new ArgumentNullException(nameof(taxCalculationService));
            _deductionsService = deductionsService ?? throw new ArgumentNullException(nameof(deductionsService));
            _varianceGuardService = varianceGuardService ?? throw new ArgumentNullException(nameof(varianceGuardService));
            _varianceResolutionService = varianceResolutionService ?? throw new ArgumentNullException(nameof(varianceResolutionService));
            _approvalService = approvalService ?? throw new ArgumentNullException(nameof(approvalService));
            _financeHandoffService = financeHandoffService ?? throw new ArgumentNullException(nameof(financeHandoffService));
            _reconciliationService = reconciliationService ?? throw new ArgumentNullException(nameof(reconciliationService));
            _payslipService = payslipService ?? throw new ArgumentNullException(nameof(payslipService));
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

        // ==========================================
        // MVP-31: DEDUCTIONS INGESTION (BE-HRD-154)
        // ==========================================

        [HttpPost("{id:guid}/ingest-deductions")]
        [AccessAction("IngestDeductions", "Penyerapan Potongan Pinjaman & Restitusi Batch", Description = "Menyerap potongan cicilan pinjaman koperasi/internal dan restitusi seluruh pegawai dengan proteksi plafon 50%", AccessType = AccessTypes.Update, SortOrder = 13)]
        [AccessPermission("PayrollRunModern", "IngestDeductions")]
        [ProducesResponseType(typeof(ApiResponse<IngestDeductionsResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> IngestDeductions(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _deductionsService.IngestDeductionsAsync(id, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<IngestDeductionsResultDto>.Ok(result.Data!, result.Message));
        }

        [HttpPost("{id:guid}/employees/{employeeRunId:guid}/ingest-deductions")]
        [AccessAction("IngestEmployeeDeductions", "Penyerapan Potongan Pinjaman Pegawai", Description = "Menyerap potongan cicilan pinjaman untuk 1 pegawai spesifik", AccessType = AccessTypes.Update, SortOrder = 14)]
        [AccessPermission("PayrollRunModern", "IngestEmployeeDeductions")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeDeductionSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> IngestEmployeeDeductions(
            Guid id,
            Guid employeeRunId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _deductionsService.IngestEmployeeDeductionsAsync(id, employeeRunId, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<EmployeeDeductionSummaryDto>.Ok(result.Data!, result.Message));
        }

        // ==========================================
        // MVP-31: VARIANCE GUARD & AUDIT (BE-HRD-155, BE-HRD-156)
        // ==========================================

        [HttpPost("{id:guid}/evaluate-variances")]
        [AccessAction("EvaluateVariances", "Audit Payroll Variance Guard", Description = "Evaluasi lonjakan/penurunan deviasi gaji bersih >= 20% / Rp 2jt dibanding bulan lalu", AccessType = AccessTypes.Update, SortOrder = 15)]
        [AccessPermission("PayrollRunModern", "EvaluateVariances")]
        [ProducesResponseType(typeof(ApiResponse<EvaluateVarianceResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EvaluateVariances(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _varianceGuardService.EvaluateRunVariancesAsync(id, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<EvaluateVarianceResultDto>.Ok(result.Data!, result.Message));
        }

        [HttpGet("{id:guid}/variances")]
        [AccessAction("ViewVariances", "Melihat Daftar Alert Variansi Gaji", Description = "Melihat seluruh tiket peringatan anomali variansi gaji pegawai pada batch", AccessType = AccessTypes.Read, SortOrder = 16)]
        [AccessPermission("PayrollRunModern", "ViewVariances")]
        [ProducesResponseType(typeof(ApiResponse<List<PayrollVarianceAlertDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVariances(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _varianceGuardService.GetRunVarianceAlertsAsync(id, cancellationToken);
            return Ok(ApiResponse<List<PayrollVarianceAlertDto>>.Ok(result.Data ?? new(), result.Message));
        }

        [HttpPost("{id:guid}/variances/{alertId:guid}/resolve")]
        [AccessAction("ResolveVariance", "Resolusi Justifikasi Audit Variansi", Description = "Mengisi catatan alasan justifikasi audit anomali variansi pegawai (minimal 20 karakter)", AccessType = AccessTypes.Update, SortOrder = 17)]
        [AccessPermission("PayrollRunModern", "ResolveVariance")]
        [ProducesResponseType(typeof(ApiResponse<PayrollVarianceAlertDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ResolveVariance(
            Guid id,
            Guid alertId,
            [FromBody] ResolveVarianceAlertCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _varianceResolutionService.ResolveAlertAsync(alertId, command, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<PayrollVarianceAlertDto>.Ok(result.Data!, result.Message));
        }

        [HttpGet("{id:guid}/variances/check-submission")]
        [AccessAction("CheckSubmissionGate", "Pengecekan Gerbang Submission Batch", Description = "Memverifikasi apakah seluruh alert variansi telah terjustifikasi sebelum submit", AccessType = AccessTypes.Read, SortOrder = 18)]
        [AccessPermission("PayrollRunModern", "CheckSubmissionGate")]
        [ProducesResponseType(typeof(ApiResponse<BatchVarianceSubmissionCheckDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckSubmissionGate(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _varianceResolutionService.CheckBatchCanBeSubmittedAsync(id, cancellationToken);
            return Ok(ApiResponse<BatchVarianceSubmissionCheckDto>.Ok(result.Data!, result.Message));
        }

        // ==========================================
        // MVP-31: APPROVAL WORKFLOW 3-TIER (BE-HRD-157)
        // ==========================================

        [HttpPost("{id:guid}/submit")]
        [AccessAction("SubmitForReview", "Tier 1: Pengajuan Batch oleh Payroll Officer", Description = "Mengajukan draft batch penggajian ke HR Manager dengan gerbang lolos audit variansi (PAY-B3)", AccessType = AccessTypes.Update, SortOrder = 19)]
        [AccessPermission("PayrollRunModern", "SubmitForReview")]
        [ProducesResponseType(typeof(ApiResponse<PayrollApprovalResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> SubmitForReview(
            Guid id,
            [FromBody] SubmitPayrollRunCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _approvalService.SubmitRunForReviewAsync(id, command, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<PayrollApprovalResultDto>.Ok(result.Data!, result.Message));
        }

        [HttpPost("{id:guid}/review")]
        [AccessAction("ReviewByHrManager", "Tier 2: Review Batch oleh HR Manager", Description = "Review manajerial batch penggajian dengan penegakan keras anti-self-approval (PAY-B4)", AccessType = AccessTypes.Update, SortOrder = 20)]
        [AccessPermission("PayrollRunModern", "ReviewByHrManager")]
        [ProducesResponseType(typeof(ApiResponse<PayrollApprovalResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ReviewByHrManager(
            Guid id,
            [FromBody] ReviewPayrollRunCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _approvalService.ReviewRunAsync(id, command, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<PayrollApprovalResultDto>.Ok(result.Data!, result.Message));
        }

        [HttpPost("{id:guid}/approve-final")]
        [AccessAction("ApproveFinalByDirector", "Tier 3: Persetujuan Final Direksi", Description = "Persetujuan final Direktur SDM & Keuangan serta penguncian permanen batch immutable (PAY-B2)", AccessType = AccessTypes.Update, SortOrder = 21)]
        [AccessPermission("PayrollRunModern", "ApproveFinalByDirector")]
        [ProducesResponseType(typeof(ApiResponse<PayrollApprovalResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ApproveFinalByDirector(
            Guid id,
            [FromBody] ApprovePayrollRunCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _approvalService.ApproveRunFinalAsync(id, command, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<PayrollApprovalResultDto>.Ok(result.Data!, result.Message));
        }

        [HttpPost("{id:guid}/reject")]
        [AccessAction("RejectBatch", "Penolakan Batch Penggajian ke NeedCorrection", Description = "Menolak batch kembali ke NeedCorrection dengan kewajiban catatan alasan tertulis resmi minimal 15 karakter (PAY-B7)", AccessType = AccessTypes.Update, SortOrder = 22)]
        [AccessPermission("PayrollRunModern", "RejectBatch")]
        [ProducesResponseType(typeof(ApiResponse<PayrollApprovalResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RejectBatch(
            Guid id,
            [FromBody] RejectPayrollRunCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _approvalService.RejectRunAsync(id, command, actorUserId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<PayrollApprovalResultDto>.Ok(result.Data!, result.Message));
        }

        [HttpGet("{id:guid}/approvals")]
        [AccessAction("ViewApprovalHistory", "Melihat Riwayat Persetujuan Approval", Description = "Melihat rekam jejak audit trail seluruh tahapan persetujuan batch penggajian", AccessType = AccessTypes.Read, SortOrder = 23)]
        [AccessPermission("PayrollRunModern", "ViewApprovalHistory")]
        [ProducesResponseType(typeof(ApiResponse<List<PayrollApprovalHistoryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetApprovalHistory(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _approvalService.GetApprovalHistoryAsync(id, cancellationToken);
            return Ok(ApiResponse<List<PayrollApprovalHistoryDto>>.Ok(result.Data ?? new(), result.Message));
        }

        // ==========================================
        // MVP-32: FINANCE HANDOFF & CHECKSUIM SHA-256 (BE-HRD-158)
        // ==========================================

        [HttpPost("{id:guid}/dispatch-finance-handoff")]
        [Tags("HR Payroll - Finance Handoff")]
        [AccessAction("DispatchFinanceHandoff", "Serah Terima Berkas ke Finance", Description = "Membentuk paket serah terima final dengan verifikasi checksum SHA-256 dan mengunci batch immutable (INT-HR-53)", AccessType = AccessTypes.Update, SortOrder = 24)]
        [AccessPermission("PayrollRunModern", "DispatchFinanceHandoff")]
        [ProducesResponseType(typeof(ApiResponse<FinanceHandoffBatchResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DispatchFinanceHandoff(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _financeHandoffService.DispatchFinanceHandoffAsync(id, actorUserId, cancellationToken);
                return Ok(ApiResponse<FinanceHandoffBatchResultDto>.Ok(result, result.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpGet("{id:guid}/handoff-status")]
        [Tags("HR Payroll - Finance Handoff")]
        [AccessAction("ViewHandoffStatus", "Melihat Status Handoff Finance", Description = "Memantau status penerimaan berkas, konfirmasi bayar, atau penolakan oleh Finance", AccessType = AccessTypes.Read, SortOrder = 25)]
        [AccessPermission("PayrollRunModern", "ViewHandoffStatus")]
        [ProducesResponseType(typeof(ApiResponse<FinanceHandoffStatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHandoffStatus(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var status = await _financeHandoffService.GetHandoffStatusAsync(id, cancellationToken);
            if (status == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, $"Status handoff untuk batch ID {id} tidak ditemukan."));
            }
            return Ok(ApiResponse<FinanceHandoffStatusDto>.Ok(status, "Status serah terima berkas Finance berhasil diambil."));
        }

        [HttpGet("{id:guid}/handoff-payload")]
        [Tags("HR Payroll - Finance Handoff")]
        [AccessAction("ViewHandoffPayload", "Melihat Payload Berkas Handoff Finance", Description = "Mengambil snapshot payload baris transfer rekening bank berintegritas SHA-256", AccessType = AccessTypes.Read, SortOrder = 26)]
        [AccessPermission("PayrollRunModern", "ViewHandoffPayload")]
        [ProducesResponseType(typeof(ApiResponse<PayrollFinanceHandoffPayload>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHandoffPayload(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var payload = await _financeHandoffService.GetHandoffPayloadAsync(id, cancellationToken);
            if (payload == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, $"Payload handoff untuk batch ID {id} tidak ditemukan."));
            }
            return Ok(ApiResponse<PayrollFinanceHandoffPayload>.Ok(payload, "Payload berkas serah terima Finance berhasil diambil."));
        }

        // ==========================================
        // MVP-32: WEBHOOK RECONCILER & DISBURSEMENT (BE-HRD-159)
        // ==========================================

        [HttpPost("{id:guid}/finance-payment-confirmed")]
        [Tags("HR Payroll - Finance Handoff")]
        [AccessAction("ConfirmFinancePayment", "Webhook: Konfirmasi Pembayaran Finance", Description = "Menerima callback konfirmasi transfer bank sukses dari Finance (INT-HR-54) & rilis slip gaji", AccessType = AccessTypes.Update, SortOrder = 27)]
        [AccessPermission("PayrollRunModern", "ConfirmFinancePayment")]
        [ProducesResponseType(typeof(ApiResponse<FinanceConfirmationResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmFinancePayment(
            Guid id,
            [FromBody] FinancePaymentConfirmationCommand command,
            CancellationToken cancellationToken = default)
        {
            command.PayrollRunId = id;
            try
            {
                var result = await _reconciliationService.ConfirmPaymentAsync(command, cancellationToken);
                return Ok(ApiResponse<FinanceConfirmationResultDto>.Ok(result, result.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        [HttpPost("{id:guid}/finance-rejection")]
        [Tags("HR Payroll - Finance Handoff")]
        [AccessAction("RejectFinancePayment", "Webhook: Penolakan Pembayaran Finance", Description = "Menerima callback penolakan pencairan Finance dengan rollback status terkendali (PAY-B7, INT-HR-54)", AccessType = AccessTypes.Update, SortOrder = 28)]
        [AccessPermission("PayrollRunModern", "RejectFinancePayment")]
        [ProducesResponseType(typeof(ApiResponse<FinanceRejectionResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectFinancePayment(
            Guid id,
            [FromBody] FinanceRejectionCallbackCommand command,
            CancellationToken cancellationToken = default)
        {
            command.PayrollRunId = id;
            try
            {
                var result = await _reconciliationService.RejectPaymentAsync(command, cancellationToken);
                return Ok(ApiResponse<FinanceRejectionResultDto>.Ok(result, result.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        // ==========================================
        // MVP-32: SELF-SERVICE PAYSLIPS ESS (BE-HRD-159)
        // ==========================================

        [HttpGet("self-service/my-payslips")]
        [Tags("HR Payroll - Self-Service Payslips")]
        [AccessAction("GetMyPayslips", "Portal ESS: Melihat Daftar Slip Gaji Saya", Description = "Mengambil daftar riwayat slip gaji pegawai mandiri yang sudah dirilis secara resmi", AccessType = AccessTypes.Read, SortOrder = 29)]
        [AccessPermission("PayrollRunModern", "GetMyPayslips")]
        [ProducesResponseType(typeof(ApiResponse<List<SelfServicePayslipSummaryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyPayslips(
            [FromQuery] int? year,
            CancellationToken cancellationToken = default)
        {
            var employeeId = GetCurrentUserId();
            var list = await _payslipService.GetMyPayslipsAsync(employeeId, year, cancellationToken);
            return Ok(ApiResponse<List<SelfServicePayslipSummaryDto>>.Ok(list, "Daftar slip gaji mandiri berhasil dimuat."));
        }

        [HttpPost("self-service/verify-pin")]
        [Tags("HR Payroll - Self-Service Payslips")]
        [AccessAction("VerifyPayslipPin", "Portal ESS: Verifikasi PIN Sekunder 6-Digit", Description = "Verifikasi autentikasi PIN sekunder 6-digit dengan penalti penguncian 30 menit (VR-PAY-22, PAY-B6)", AccessType = AccessTypes.Update, SortOrder = 30)]
        [AccessPermission("PayrollRunModern", "VerifyPayslipPin")]
        [ProducesResponseType(typeof(ApiResponse<PayslipPinVerificationResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PayslipPinVerificationResultDto>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> VerifyPayslipPin(
            [FromBody] VerifyPayslipPinRequest request,
            CancellationToken cancellationToken = default)
        {
            var employeeId = GetCurrentUserId();
            var result = await _payslipService.VerifyPinAsync(employeeId, request.Pin, cancellationToken);
            if (!result.IsSuccess && result.LockoutMinutesRemaining > 0)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, ApiResponse<PayslipPinVerificationResultDto>.Fail(StatusCodes.Status429TooManyRequests, result.Message));
            }
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<PayslipPinVerificationResultDto>.Fail(StatusCodes.Status400BadRequest, result.Message));
            }
            return Ok(ApiResponse<PayslipPinVerificationResultDto>.Ok(result, result.Message));
        }

        [HttpGet("self-service/payslips/{payslipId:guid}/detail")]
        [Tags("HR Payroll - Self-Service Payslips")]
        [AccessAction("GetPayslipDetail", "Portal ESS: Rincian Lengkap Lembar Slip Gaji", Description = "Melihat rincian komponen penerimaan & potongan slip gaji setelah PIN sekunder terverifikasi", AccessType = AccessTypes.Read, SortOrder = 31)]
        [AccessPermission("PayrollRunModern", "GetPayslipDetail")]
        [ProducesResponseType(typeof(ApiResponse<SelfServicePayslipDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPayslipDetail(
            Guid payslipId,
            [FromHeader(Name = "X-Payroll-PIN")] string pinToken,
            CancellationToken cancellationToken = default)
        {
            var employeeId = GetCurrentUserId();
            try
            {
                var detail = await _payslipService.GetPayslipDetailAsync(payslipId, employeeId, pinToken, cancellationToken);
                return Ok(ApiResponse<SelfServicePayslipDetailDto>.Ok(detail, "Rincian slip gaji berhasil dimuat."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        [HttpGet("self-service/payslips/{payslipId:guid}/download-pdf")]
        [Tags("HR Payroll - Self-Service Payslips")]
        [AccessAction("DownloadPayslipPdf", "Portal ESS: Unduh Dokumen PDF Slip Gaji", Description = "Mengunduh berkas PDF slip gaji ber-watermark resmi RS berproteksi kata sandi (VR-PAY-24)", AccessType = AccessTypes.Read, SortOrder = 32)]
        [AccessPermission("PayrollRunModern", "DownloadPayslipPdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadPayslipPdf(
            Guid payslipId,
            [FromHeader(Name = "X-Payroll-PIN")] string pinToken,
            CancellationToken cancellationToken = default)
        {
            var employeeId = GetCurrentUserId();
            try
            {
                var (fileBytes, fileName, contentType) = await _payslipService.DownloadPayslipPdfAsync(payslipId, employeeId, pinToken, cancellationToken);
                return File(fileBytes, contentType, fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        private Guid GetCurrentUserId()
        {
            var rawId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(rawId, out var id) ? id : Guid.Empty;
        }
    }
}
