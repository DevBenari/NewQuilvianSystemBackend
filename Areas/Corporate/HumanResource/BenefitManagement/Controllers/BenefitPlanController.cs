using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Controllers
{
    [ApiController]
    [Route("api/hr-benefits")]
    [Authorize]
    public class BenefitPlanController : ControllerBase
    {
        private readonly IBenefitPlanService _benefitPlanService;
        private readonly ILogger<BenefitPlanController> _logger;

        public BenefitPlanController(
            IBenefitPlanService benefitPlanService,
            ILogger<BenefitPlanController> logger)
        {
            _benefitPlanService = benefitPlanService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var uid) ? uid : Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        /// <summary>
        /// Mengambil daftar master paket manfaat rumah sakit (3-lapisan) dengan filter kategori dan pencarian (BE-HRD-177, FR-BEN-01).
        /// </summary>
        [HttpGet("plans")]
        [Tags("HR Benefits - Plans & Eligibility")]
        public async Task<IActionResult> GetBenefitPlans(
            [FromQuery] BenefitPlanFilterDto filter,
            CancellationToken cancellationToken)
        {
            var result = await _benefitPlanService.GetBenefitPlansAsync(filter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Mengambil detail master paket manfaat berdasarkan ID (BE-HRD-177).
        /// </summary>
        [HttpGet("plans/{id:guid}")]
        [Tags("HR Benefits - Plans & Eligibility")]
        public async Task<IActionResult> GetBenefitPlanById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _benefitPlanService.GetBenefitPlanByIdAsync(id, cancellationToken);
            if (result == null) return NotFound(new { message = $"Paket manfaat dengan ID {id} tidak ditemukan." });
            return Ok(result);
        }

        /// <summary>
        /// Membuat master paket manfaat baru (BE-HRD-177, HRD-DEC-134).
        /// </summary>
        [HttpPost("plans")]
        [Tags("HR Benefits - Plans & Eligibility")]
        public async Task<IActionResult> CreateBenefitPlan(
            [FromBody] CreateBenefitPlanRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _benefitPlanService.CreateBenefitPlanAsync(request, GetCurrentUserId(), cancellationToken);
            return CreatedAtAction(nameof(GetBenefitPlanById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Memperbarui konfigurasi master paket manfaat (BE-HRD-177).
        /// </summary>
        [HttpPut("plans/{id:guid}")]
        [Tags("HR Benefits - Plans & Eligibility")]
        public async Task<IActionResult> UpdateBenefitPlan(
            Guid id,
            [FromBody] UpdateBenefitPlanRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _benefitPlanService.UpdateBenefitPlanAsync(id, request, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Inisialisasi/penyemaian otomatis 4 master paket manfaat default rumah sakit dan matriks eligibilitas grade (BE-HRD-177).
        /// </summary>
        [HttpPost("plans/seed-defaults")]
        [Tags("HR Benefits - Plans & Eligibility")]
        public async Task<IActionResult> SeedDefaultHospitalPlans(CancellationToken cancellationToken)
        {
            var count = await _benefitPlanService.SeedDefaultHospitalBenefitPlansAsync(GetCurrentUserId(), cancellationToken);
            return Ok(new { message = $"Berhasil memproses penyemaian default paket manfaat rumah sakit.", count });
        }

        /// <summary>
        /// Mengambil daftar aturan kelayakan grade untuk suatu paket manfaat (BE-HRD-178, FR-BEN-03).
        /// </summary>
        [HttpGet("plans/{planId:guid}/grade-eligibilities")]
        [Tags("HR Benefits - Plans & Eligibility")]
        public async Task<IActionResult> GetGradeEligibilitiesByPlan(
            Guid planId,
            CancellationToken cancellationToken)
        {
            var result = await _benefitPlanService.GetGradeEligibilitiesAsync(planId, null, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Menambahkan aturan kelayakan grade kepegawaian staf pada paket manfaat (BE-HRD-178, FR-BEN-03, FR-BEN-04).
        /// </summary>
        [HttpPost("plans/{planId:guid}/grade-eligibilities")]
        [Tags("HR Benefits - Plans & Eligibility")]
        public async Task<IActionResult> AddGradeEligibility(
            Guid planId,
            [FromBody] CreateBenefitGradeEligibilityRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            request.BenefitPlanId = planId;
            var result = await _benefitPlanService.AddGradeEligibilityAsync(request, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Memperbarui aturan kelas kamar atau limit spesifik grade (BE-HRD-178).
        /// </summary>
        [HttpPut("grade-eligibilities/{id:guid}")]
        [Tags("HR Benefits - Plans & Eligibility")]
        public async Task<IActionResult> UpdateGradeEligibility(
            Guid id,
            [FromBody] UpdateBenefitGradeEligibilityRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _benefitPlanService.UpdateGradeEligibilityAsync(id, request, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Menghapus/menonaktifkan aturan kelayakan grade (BE-HRD-178).
        /// </summary>
        [HttpDelete("grade-eligibilities/{id:guid}")]
        [Tags("HR Benefits - Plans & Eligibility")]
        public async Task<IActionResult> DeleteGradeEligibility(
            Guid id,
            CancellationToken cancellationToken)
        {
            var success = await _benefitPlanService.DeleteGradeEligibilityAsync(id, GetCurrentUserId(), cancellationToken);
            return Ok(new { success, message = "Aturan kelayakan grade berhasil dinonaktifkan." });
        }

        /// <summary>
        /// Evaluasi eligibilitas terpadu paket manfaat staf berdasarkan grade dan masa kerja probation (BE-HRD-180, AT-HRD-BEN-01, AT-HRD-BEN-02).
        /// </summary>
        [HttpPost("plans/evaluate-eligibility")]
        [Tags("HR Benefits - Plans & Eligibility")]
        public async Task<IActionResult> EvaluateEmployeePlanEligibility(
            [FromBody] EvaluateEmployeeBenefitEligibilityRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _benefitPlanService.EvaluateEmployeePlanEligibilityAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}
