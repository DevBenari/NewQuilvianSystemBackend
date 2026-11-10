using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/hrd/learning/plans/annual")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEARNING_DEVELOPMENT",
        moduleName: "Human Resource Learning and Development",
        displayName: "Rencana Pelatihan Tahunan (TNA)",
        AreaName = "Corporate",
        ControllerName = "AnnualTrainingPlan",
        Description = "Penyusunan, pengajuan, dan pengesahan rencana kerja pelatihan tahunan",
        SortOrder = 11)]
    [Tags("Corporate - HRD Training Enrollment & Budget Governance")]
    [Authorize]
    public class AnnualTrainingPlansController : ControllerBase
    {
        private readonly IAnnualTrainingPlanService _planService;
        private readonly ILogger<AnnualTrainingPlansController> _logger;

        public AnnualTrainingPlansController(
            IAnnualTrainingPlanService planService,
            ILogger<AnnualTrainingPlansController> logger)
        {
            _planService = planService;
            _logger = logger;
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Rencana Pelatihan Tahunan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AnnualTrainingPlan", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<AnnualTrainingPlanDto>>), 200)]
        public async Task<IActionResult> GetAnnualPlans(
            [FromQuery] int? planYear,
            [FromQuery] string? planStatus,
            [FromQuery] Guid? departmentId,
            CancellationToken cancellationToken)
        {
            try
            {
                var plans = await _planService.GetAnnualPlansAsync(planYear, planStatus, departmentId, cancellationToken);
                return Ok(ApiResponse<List<AnnualTrainingPlanDto>>.Ok(plans, "Daftar rencana kerja tahunan diklat berhasil dimuat."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memuat rencana program diklat tahunan.");
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal pada server saat mengambil rencana kerja diklat."));
            }
        }

        [HttpGet("{id}")]
        [AccessAction("Read", "Melihat Rencana Pelatihan Tahunan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AnnualTrainingPlan", "Read")]
        [ProducesResponseType(typeof(ApiResponse<AnnualTrainingPlanDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> GetAnnualPlanById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var plan = await _planService.GetAnnualPlanByIdAsync(id, cancellationToken);
                if (plan == null)
                {
                    return NotFound(ApiResponse<string>.Fail(404, $"Rencana kerja tahunan dengan ID {id} tidak ditemukan."));
                }

                return Ok(ApiResponse<AnnualTrainingPlanDto>.Ok(plan, "Detail rencana kerja tahunan diklat berhasil dimuat."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memuat rencana diklat {PlanId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan saat memuat detail rencana diklat."));
            }
        }

        [HttpPost]
        [AccessAction("Create", "Menyusun Rencana Pelatihan Tahunan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AnnualTrainingPlan", "Create")]
        [ProducesResponseType(typeof(ApiResponse<AnnualTrainingPlanDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> CreateAnnualPlan(
            [FromBody] CreateAnnualTrainingPlanRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Data formulir pengajuan rencana kerja tahunan tidak valid."));
            }

            try
            {
                var currentUser = ResolveActorName();
                var created = await _planService.CreateAnnualPlanAsync(request, currentUser, cancellationToken);

                return StatusCode(201, ApiResponse<AnnualTrainingPlanDto>.Ok(created, "Rencana program tahunan diklat (TNA) berhasil didaftarkan."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal membuat rencana program diklat baru.");
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat mendaftarkan rencana program diklat."));
            }
        }

        [HttpPut("{id}")]
        [AccessAction("Update", "Mengubah Rencana Pelatihan Tahunan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AnnualTrainingPlan", "Update")]
        [ProducesResponseType(typeof(ApiResponse<AnnualTrainingPlanDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 422)]
        public async Task<IActionResult> UpdateAnnualPlan(
            [FromRoute] Guid id,
            [FromBody] UpdateAnnualTrainingPlanRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Data pembaruan rencana diklat tidak valid."));
            }

            try
            {
                var currentUser = ResolveActorName();
                var updated = await _planService.UpdateAnnualPlanAsync(id, request, currentUser, cancellationToken);

                return Ok(ApiResponse<AnnualTrainingPlanDto>.Ok(updated, "Rencana program tahunan diklat berhasil diperbarui."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(422, ApiResponse<string>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memperbarui rencana diklat {PlanId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat memperbarui rencana diklat."));
            }
        }

        [HttpPost("{id}/submit")]
        [AccessAction("Submit", "Mengajukan Rencana Pelatihan Tahunan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("AnnualTrainingPlan", "Submit")]
        [ProducesResponseType(typeof(ApiResponse<AnnualTrainingPlanDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 422)]
        public async Task<IActionResult> SubmitAnnualPlan(
            [FromRoute] Guid id,
            [FromBody] SubmitTrainingPlanRequestDto? request,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUser = ResolveActorName();
                var submitted = await _planService.SubmitAnnualPlanAsync(id, request, currentUser, cancellationToken);

                return Ok(ApiResponse<AnnualTrainingPlanDto>.Ok(submitted, "Rencana program kerja tahunan diklat (TNA) berhasil diajukan ke Direksi."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(422, ApiResponse<string>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengajukan rencana diklat {PlanId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat mengajukan rencana program diklat ke Direksi."));
            }
        }

        [HttpPost("{id}/approve")]
        [AccessAction("Approve", "Mengesahkan Rencana Pelatihan Tahunan", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("AnnualTrainingPlan", "Approve")]
        [ProducesResponseType(typeof(ApiResponse<AnnualTrainingPlanDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 422)]
        public async Task<IActionResult> ApproveAnnualPlan(
            [FromRoute] Guid id,
            [FromBody] ApproveTrainingPlanRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Data formulir pengesahan rencana kerja tidak valid."));
            }

            try
            {
                var currentUser = ResolveActorName();
                var approved = await _planService.ApproveAnnualPlanAsync(id, request, currentUser, cancellationToken);

                return Ok(ApiResponse<AnnualTrainingPlanDto>.Ok(approved, "Rencana program tahunan diklat resmi DISAHKAN oleh Direksi dan pagu anggaran tahunan dibuka."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(422, ApiResponse<string>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengesahkan rencana diklat {PlanId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan saat mengesahkan rencana program diklat."));
            }
        }

        [HttpPost("{id}/reject")]
        [AccessAction("Reject", "Menolak Rencana Pelatihan Tahunan", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("AnnualTrainingPlan", "Reject")]
        [ProducesResponseType(typeof(ApiResponse<AnnualTrainingPlanDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 422)]
        public async Task<IActionResult> RejectAnnualPlan(
            [FromRoute] Guid id,
            [FromBody] RejectTrainingPlanRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Alasan penolakan / revisi wajib diisi."));
            }

            try
            {
                var currentUser = ResolveActorName();
                var rejected = await _planService.RejectAnnualPlanAsync(id, request, currentUser, cancellationToken);

                return Ok(ApiResponse<AnnualTrainingPlanDto>.Ok(rejected, "Rencana program tahunan diklat dikembalikan ke status Draft untuk revisi."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(422, ApiResponse<string>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengembalikan rencana diklat {PlanId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan saat mengembalikan rencana program diklat untuk revisi."));
            }
        }

        [HttpDelete("{id}")]
        [AccessAction("Delete", "Menghapus Rencana Pelatihan Tahunan", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("AnnualTrainingPlan", "Delete")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 422)]
        public async Task<IActionResult> DeleteAnnualPlan([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var currentUser = ResolveActorName();
                var success = await _planService.DeleteAnnualPlanAsync(id, currentUser, cancellationToken);

                if (!success)
                {
                    return NotFound(ApiResponse<string>.Fail(404, $"Rencana kerja tahunan dengan ID {id} tidak ditemukan."));
                }

                return Ok(ApiResponse<bool>.Ok(true, "Rencana kerja tahunan diklat berhasil dihapus."));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(422, ApiResponse<string>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menghapus rencana diklat {PlanId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat menghapus rencana program diklat."));
            }
        }

        /// <summary>
        /// Nama pengguna yang sedang login untuk jejak aktor. Sengaja tidak memakai aktor cadangan:
        /// permintaan tanpa identitas ditolak, bukan dicatat atas nama orang lain.
        /// </summary>
        private string ResolveActorName()
        {
            var actorName = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(actorName))
            {
                throw new UnauthorizedAccessException("Identitas user pada token tidak valid.");
            }

            return actorName;
        }
    }
}
