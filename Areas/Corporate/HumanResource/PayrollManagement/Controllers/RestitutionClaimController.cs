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
        displayName: "Klaim Restitusi Pegawai",
        AreaName = "Corporate",
        ControllerName = "RestitutionClaim",
        Description = "Klaim restitusi salah potong, persetujuan berjenjang, koreksi gaji, dan injeksi ke payroll",
        SortOrder = 40)]
    public class RestitutionClaimController : ControllerBase
    {
        private readonly IRestitutionClaimService _claimService;
        private readonly ILogger<RestitutionClaimController> _logger;

        public RestitutionClaimController(
            IRestitutionClaimService claimService,
            ILogger<RestitutionClaimController> logger)
        {
            _claimService = claimService;
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
        /// Mengambil sisa kuota plafon tahunan pegawai untuk kategori tertentu (VR-DED-10, DED-W2).
        /// </summary>
        [HttpGet("claims/quota")]
        [AccessAction("ViewOwn", "Melihat Kuota Restitusi Sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RestitutionClaim", "ViewOwn")]
        [Tags("HR Deductions - Restitution Claims")]
        public async Task<IActionResult> GetRemainingQuota(
            [FromQuery] Guid employeeId,
            [FromQuery] string benefitCategory,
            CancellationToken cancellationToken)
        {
            var result = await _claimService.GetEmployeeRemainingQuotaAsync(employeeId, benefitCategory, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Pengajuan klaim restitusi mandiri oleh pegawai di portal ESS (filter kuitansi < 60 hari & upload nota).
        /// </summary>
        [HttpPost("claims")]
        [AccessAction("Submit", "Mengajukan Klaim Restitusi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RestitutionClaim", "Submit")]
        [Tags("HR Deductions - Restitution Claims")]
        public async Task<IActionResult> SubmitClaim(
            [FromBody] SubmitRestitutionClaimRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _claimService.SubmitClaimAsync(request, GetCurrentUserId(), cancellationToken);
            return CreatedAtAction(nameof(GetClaimById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Mengambil daftar klaim restitusi pegawai dengan filter kategori, status, dan cito.
        /// </summary>
        [HttpGet("claims")]
        [AccessAction("Read", "Melihat Klaim Restitusi", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("RestitutionClaim", "Read")]
        [Tags("HR Deductions - Restitution Claims")]
        public async Task<IActionResult> GetClaims(
            [FromQuery] RestitutionClaimFilterDto filter,
            CancellationToken cancellationToken)
        {
            var result = await _claimService.GetClaimsAsync(filter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Mengambil detail satu berkas klaim restitusi berdasarkan ID.
        /// </summary>
        [HttpGet("claims/{id:guid}")]
        [AccessAction("Read", "Melihat Klaim Restitusi", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("RestitutionClaim", "Read")]
        [Tags("HR Deductions - Restitution Claims")]
        public async Task<IActionResult> GetClaimById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _claimService.GetClaimByIdAsync(id, cancellationToken);
            if (result == null) return NotFound(new { message = "Berkas klaim restitusi tidak ditemukan." });
            return Ok(result);
        }

        /// <summary>
        /// Persetujuan tingkat pertama oleh atasan langsung / Supervisor (DED-B8 Anti-Self-Approval).
        /// </summary>
        [HttpPost("claims/{id:guid}/approve-supervisor")]
        [AccessAction("ApproveSupervisor", "Menyetujui Restitusi (Atasan)", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("RestitutionClaim", "ApproveSupervisor")]
        [Tags("HR Deductions - Restitution Claims")]
        public async Task<IActionResult> ApproveBySupervisor(
            Guid id,
            [FromBody] SupervisorApprovalRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _claimService.ApproveBySupervisorAsync(id, request, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Review verifikasi keabsahan nota dan dokumen medis oleh HR C&B Reviewer (Tier-2).
        /// </summary>
        [HttpPost("claims/{id:guid}/review-hr")]
        [AccessAction("ReviewHr", "Mereview Restitusi (HR)", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("RestitutionClaim", "ReviewHr")]
        [Tags("HR Deductions - Restitution Claims")]
        public async Task<IActionResult> ReviewByHr(
            Guid id,
            [FromBody] HrReviewRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _claimService.ReviewByHrAsync(id, request, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Pengesahan final klaim oleh HR Manager (Tier-3) dan pembentukan komponen restitusi payroll.
        /// </summary>
        [HttpPost("claims/{id:guid}/approve-final")]
        [AccessAction("ApproveFinal", "Menyetujui Final Restitusi", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("RestitutionClaim", "ApproveFinal")]
        [Tags("HR Deductions - Restitution Claims")]
        public async Task<IActionResult> FinalApprove(
            Guid id,
            [FromBody] FinalApprovalRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _claimService.FinalApproveAsync(id, request, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Penolakan klaim restitusi dengan kewajiban alasan penolakan audit minimal 15 karakter (VR-DED-22).
        /// </summary>
        [HttpPost("claims/{id:guid}/reject")]
        [AccessAction("Reject", "Menolak Klaim Restitusi", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("RestitutionClaim", "Reject")]
        [Tags("HR Deductions - Restitution Claims")]
        public async Task<IActionResult> RejectClaim(
            Guid id,
            [FromBody] RejectClaimRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _claimService.RejectClaimAsync(id, request, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Menerbitkan kompensasi salah potong masa lalu tanpa revisi slip lama berstatus non-pajak (PAY-B6, DED-B4, DED-B5).
        /// </summary>
        [HttpPost("restitutions/salary-correction")]
        [AccessAction("Correct", "Mengoreksi Gaji", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("RestitutionClaim", "Correct")]
        [Tags("HR Deductions - Restitutions")]
        public async Task<IActionResult> CreateSalaryCorrection(
            [FromBody] CreateSalaryCorrectionRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _claimService.CreateSalaryCorrectionAsync(request, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Investigasi keluhan salah potong gaji oleh HR Payroll Officer dan penerbitan restitusi koreksi bebas PPh 21 (BE-HRD-171, DED-B5, HRD-ASM-33).
        /// </summary>
        [HttpPost("restitutions/grievance-investigation")]
        [AccessAction("Correct", "Mengoreksi Gaji", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("RestitutionClaim", "Correct")]
        [Tags("HR Deductions - Restitutions")]
        public async Task<IActionResult> InvestigateAndResolveSalaryGrievance(
            [FromBody] SalaryGrievanceInvestigationRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _claimService.InvestigateAndResolveSalaryGrievanceAsync(request, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Mengambil daftar komponen restitusi dan koreksi penggajian.
        /// </summary>
        [HttpGet("restitutions")]
        [AccessAction("Read", "Melihat Klaim Restitusi", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("RestitutionClaim", "Read")]
        [Tags("HR Deductions - Restitutions")]
        public async Task<IActionResult> GetPayrollRestitutions(
            [FromQuery] Guid? employeeId,
            [FromQuery] Guid? targetPayrollRunId,
            [FromQuery] string? status,
            CancellationToken cancellationToken)
        {
            var result = await _claimService.GetPayrollRestitutionsAsync(employeeId, targetPayrollRunId, status, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Injeksi otomatis seluruh komponen restitusi approved ke payroll run berjalan tgl 25 sebagai komponen non-pajak.
        /// </summary>
        [HttpPost("restitutions/inject-to-payroll/{payrollRunId:guid}")]
        [AccessAction("InjectToPayroll", "Menginjeksi Restitusi ke Payroll", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission("RestitutionClaim", "InjectToPayroll")]
        [Tags("HR Deductions - Restitutions")]
        public async Task<IActionResult> InjectApprovedRestitutions(
            Guid payrollRunId,
            CancellationToken cancellationToken)
        {
            var result = await _claimService.InjectApprovedRestitutionsToPayrollAsync(payrollRunId, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }
    }
}
