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
    [Route("api/v1/corporate/hrd/learning/enrollments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEARNING_DEVELOPMENT",
        moduleName: "Human Resource Learning and Development",
        displayName: "Pendaftaran Pelatihan",
        AreaName = "Corporate",
        ControllerName = "TrainingEnrollment",
        Description = "Pengajuan pendaftaran diklat oleh pegawai dan persetujuannya",
        SortOrder = 13)]
    [Tags("Corporate - HRD Training Enrollment & Budget Governance")]
    [Authorize]
    public class TrainingEnrollmentsController : ControllerBase
    {
        private readonly ITrainingEnrollmentService _enrollmentService;
        private readonly ILogger<TrainingEnrollmentsController> _logger;

        public TrainingEnrollmentsController(
            ITrainingEnrollmentService enrollmentService,
            ILogger<TrainingEnrollmentsController> logger)
        {
            _enrollmentService = enrollmentService;
            _logger = logger;
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Pendaftaran Pelatihan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TrainingEnrollment", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<TrainingEnrollmentDto>>), 200)]
        public async Task<IActionResult> GetEnrollments(
            [FromQuery] Guid? sessionId,
            [FromQuery] Guid? workforceProfileId,
            [FromQuery] string? status,
            CancellationToken cancellationToken)
        {
            try
            {
                var enrollments = await _enrollmentService.GetEnrollmentsAsync(sessionId, workforceProfileId, status, cancellationToken);
                return Ok(ApiResponse<List<TrainingEnrollmentDto>>.Ok(enrollments, "Daftar permohonan pendaftaran pelatihan berhasil dimuat."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memuat daftar permohonan pendaftaran diklat.");
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat memuat data pendaftaran diklat."));
            }
        }

        [HttpGet("{id}")]
        [AccessAction("Read", "Melihat Pendaftaran Pelatihan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TrainingEnrollment", "Read")]
        [ProducesResponseType(typeof(ApiResponse<TrainingEnrollmentDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> GetEnrollmentById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id, cancellationToken);
                if (enrollment == null)
                {
                    return NotFound(ApiResponse<string>.Fail(404, $"Pendaftaran diklat dengan ID {id} tidak ditemukan."));
                }

                return Ok(ApiResponse<TrainingEnrollmentDto>.Ok(enrollment, "Detail pendaftaran diklat berhasil dimuat."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memuat detail pendaftaran diklat {EnrollmentId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan saat memuat detail pendaftaran diklat."));
            }
        }

        [HttpPost("ess-submit")]
        [AccessAction("Submit", "Mengajukan Pendaftaran Pelatihan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("TrainingEnrollment", "Submit")]
        [ProducesResponseType(typeof(ApiResponse<TrainingEnrollmentDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 409)]
        public async Task<IActionResult> SubmitEssEnrollment(
            [FromBody] SubmitEnrollmentRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Data formulir pendaftaran diklat ESS tidak valid."));
            }

            try
            {
                var currentUser = ResolveActorName();
                var created = await _enrollmentService.SubmitEssEnrollmentAsync(request, currentUser, cancellationToken);

                return StatusCode(201, ApiResponse<TrainingEnrollmentDto>.Ok(created, "Permohonan pendaftaran pelatihan via ESS berhasil diajukan dan diteruskan ke atasan langsung."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(409, ApiResponse<string>.Fail(409, ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengajukan pendaftaran diklat via ESS.");
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat mengajukan pendaftaran diklat via ESS."));
            }
        }

        [HttpPost("{id}/approval-workflow")]
        [AccessAction("Approve", "Menyetujui Pendaftaran Pelatihan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("TrainingEnrollment", "Approve")]
        [ProducesResponseType(typeof(ApiResponse<TrainingEnrollmentDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 422)]
        public async Task<IActionResult> ProcessApprovalWorkflow(
            [FromRoute] Guid id,
            [FromBody] ProcessEnrollmentApprovalDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Data formulir keputusan approval pendaftaran tidak valid."));
            }

            try
            {
                var currentUser = ResolveActorName();
                var updated = await _enrollmentService.ProcessApprovalWorkflowAsync(id, request, currentUser, cancellationToken);

                return Ok(ApiResponse<TrainingEnrollmentDto>.Ok(updated, $"Persetujuan pendaftaran tahap {request.Stage} berhasil diproses dengan status {updated.RequestStatus}."));
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
                _logger.LogError(ex, "Gagal memproses approval pendaftaran diklat {EnrollmentId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat memproses persetujuan pendaftaran diklat."));
            }
        }

        [HttpDelete("{id}")]
        [AccessAction("Delete", "Menghapus Pendaftaran Pelatihan", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("TrainingEnrollment", "Delete")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 422)]
        public async Task<IActionResult> DeleteEnrollment([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var currentUser = ResolveActorName();
                var success = await _enrollmentService.DeleteEnrollmentAsync(id, currentUser, cancellationToken);

                if (!success)
                {
                    return NotFound(ApiResponse<string>.Fail(404, $"Pendaftaran diklat dengan ID {id} tidak ditemukan."));
                }

                return Ok(ApiResponse<bool>.Ok(true, "Permohonan pendaftaran diklat berhasil dihapus."));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(422, ApiResponse<string>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menghapus pendaftaran diklat {EnrollmentId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat menghapus permohonan pendaftaran diklat."));
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
