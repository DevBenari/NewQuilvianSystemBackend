using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/hrd/learning/budgets")]
    [Tags("Corporate - HRD Training Enrollment & Budget Governance")]
    [Authorize]
    public class TrainingBudgetsController : ControllerBase
    {
        private readonly ITrainingBudgetControlService _budgetService;
        private readonly ILogger<TrainingBudgetsController> _logger;

        public TrainingBudgetsController(
            ITrainingBudgetControlService budgetService,
            ILogger<TrainingBudgetsController> logger)
        {
            _budgetService = budgetService;
            _logger = logger;
        }

        [HttpGet("department/{departmentId}/status")]
        [ProducesResponseType(typeof(ApiResponse<TrainingBudgetSummaryDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> GetDepartmentBudgetStatus(
            [FromRoute] Guid departmentId,
            [FromQuery] int? fiscalYear,
            CancellationToken cancellationToken)
        {
            try
            {
                var budget = await _budgetService.GetDepartmentBudgetStatusAsync(departmentId, fiscalYear, cancellationToken);
                if (budget == null)
                {
                    return NotFound(ApiResponse<string>.Fail(404, $"Pagu anggaran diklat untuk Departemen ID {departmentId} pada tahun yang diminta belum tersedia."));
                }

                return Ok(ApiResponse<TrainingBudgetSummaryDto>.Ok(budget, "Status pagu alokasi, komitmen berjalan, dan sisa saldo anggaran diklat unit berhasil dimuat."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memuat status pagu anggaran departemen {DepartmentId}", departmentId);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat memeriksa pagu anggaran departemen."));
            }
        }

        [HttpGet("plan/{trainingPlanId}")]
        [ProducesResponseType(typeof(ApiResponse<List<TrainingBudgetSummaryDto>>), 200)]
        public async Task<IActionResult> GetPlanBudgets(
            [FromRoute] Guid trainingPlanId,
            CancellationToken cancellationToken)
        {
            try
            {
                var budgets = await _budgetService.GetPlanBudgetsAsync(trainingPlanId, cancellationToken);
                return Ok(ApiResponse<List<TrainingBudgetSummaryDto>>.Ok(budgets, "Daftar rincian pagu anggaran per unit pada rencana pelatihan berhasil dimuat."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memuat daftar anggaran rencana pelatihan {PlanId}", trainingPlanId);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat memuat data anggaran rencana pelatihan."));
            }
        }

        [HttpPost("allocate")]
        [ProducesResponseType(typeof(ApiResponse<TrainingBudgetSummaryDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> AllocateBudget(
            [FromBody] AllocateDepartmentBudgetRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Data formulir alokasi pagu anggaran tidak valid."));
            }

            try
            {
                var currentUser = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Diklat_Manager";
                var allocated = await _budgetService.AllocateBudgetAsync(request, currentUser, cancellationToken);

                return StatusCode(201, ApiResponse<TrainingBudgetSummaryDto>.Ok(allocated, "Pagu anggaran diklat berhasil dialokasikan."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengalokasikan pagu anggaran diklat.");
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat mengalokasikan pagu anggaran diklat."));
            }
        }

        [HttpPost("commit")]
        [ProducesResponseType(typeof(ApiResponse<DepartmentBudgetValidationResultDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<DepartmentBudgetValidationResultDto>), 422)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> CommitBudget(
            [FromBody] CommitTrainingBudgetRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Data formulir komitmen anggaran tidak valid."));
            }

            try
            {
                var currentUser = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Diklat_Admin";
                var result = await _budgetService.ValidateAndCommitBudgetAsync(request, currentUser, cancellationToken);

                if (!result.IsEligible)
                {
                    return StatusCode(422, ApiResponse<DepartmentBudgetValidationResultDto>.Fail(422, result.StatusMessage, result));
                }

                return Ok(ApiResponse<DepartmentBudgetValidationResultDto>.Ok(result, "Komitmen pagu anggaran diklat berhasil dibukukan secara otomatis (VR-TRN-15)."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memproses komitmen anggaran diklat.");
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat membukukan komitmen anggaran diklat."));
            }
        }

        [HttpPost("realize")]
        [ProducesResponseType(typeof(ApiResponse<TrainingBudgetSummaryDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> RealizeBudget(
            [FromBody] RealizeTrainingBudgetRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Data formulir realisasi anggaran tidak valid."));
            }

            try
            {
                var currentUser = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Finance_Staff";
                var realized = await _budgetService.RealizeBudgetAsync(request, currentUser, cancellationToken);

                return Ok(ApiResponse<TrainingBudgetSummaryDto>.Ok(realized, "Realisasi pencairan biaya pelatihan berhasil dibukukan dan komitmen disesuaikan."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memproses realisasi anggaran diklat.");
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat membukukan realisasi anggaran diklat."));
            }
        }
    }
}
