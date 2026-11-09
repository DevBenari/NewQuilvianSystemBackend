using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/hrd/learning/sessions")]
    [Tags("Corporate - HRD Clinical Competency & OSCE Assessment")]
    [Authorize]
    public class TrainingSessionsController : ControllerBase
    {
        private readonly ITrainingSessionService _sessionService;
        private readonly ILogger<TrainingSessionsController> _logger;

        public TrainingSessionsController(
            ITrainingSessionService sessionService,
            ILogger<TrainingSessionsController> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<TrainingSessionDto>>), 200)]
        public async Task<IActionResult> GetSessions(
            [FromQuery] Guid? trainingPlanId,
            [FromQuery] string? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            CancellationToken cancellationToken)
        {
            try
            {
                var sessions = await _sessionService.GetSessionsAsync(trainingPlanId, status, fromDate, toDate, cancellationToken);
                return Ok(ApiResponse<List<TrainingSessionDto>>.Ok(sessions, "Daftar jadwal sesi pelatihan berhasil dimuat."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memuat jadwal sesi pelatihan.");
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat memuat jadwal sesi pelatihan."));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<TrainingSessionDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> GetSessionById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var session = await _sessionService.GetSessionByIdAsync(id, cancellationToken);
                if (session == null)
                {
                    return NotFound(ApiResponse<string>.Fail(404, $"Sesi pelatihan dengan ID {id} tidak ditemukan."));
                }

                return Ok(ApiResponse<TrainingSessionDto>.Ok(session, "Detail jadwal sesi pelatihan berhasil dimuat."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memuat sesi pelatihan {SessionId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan saat memuat detail sesi pelatihan."));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TrainingSessionDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 409)]
        public async Task<IActionResult> CreateSession(
            [FromBody] CreateTrainingSessionRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Data formulir pembuatan sesi pelatihan tidak valid."));
            }

            try
            {
                var currentUser = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Diklat_Admin";
                var created = await _sessionService.CreateSessionAsync(request, currentUser, cancellationToken);

                return StatusCode(201, ApiResponse<TrainingSessionDto>.Ok(created, "Jadwal sesi kelas pelatihan berhasil dibuat dan pendaftaran dibuka."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(409, ApiResponse<string>.Fail(409, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal membuat jadwal sesi pelatihan.");
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat membuat jadwal sesi pelatihan."));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<TrainingSessionDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 409)]
        public async Task<IActionResult> UpdateSession(
            [FromRoute] Guid id,
            [FromBody] UpdateTrainingSessionRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Data formulir pembaruan sesi pelatihan tidak valid."));
            }

            try
            {
                var currentUser = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Diklat_Admin";
                var updated = await _sessionService.UpdateSessionAsync(id, request, currentUser, cancellationToken);

                return Ok(ApiResponse<TrainingSessionDto>.Ok(updated, "Jadwal sesi kelas pelatihan berhasil diperbarui."));
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
                return StatusCode(409, ApiResponse<string>.Fail(409, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memperbarui sesi pelatihan {SessionId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat memperbarui jadwal sesi pelatihan."));
            }
        }

        [HttpPost("{id}/open-registration")]
        [ProducesResponseType(typeof(ApiResponse<TrainingSessionDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> OpenRegistration([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var currentUser = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Diklat_Admin";
                var session = await _sessionService.OpenRegistrationAsync(id, currentUser, cancellationToken);

                return Ok(ApiResponse<TrainingSessionDto>.Ok(session, "Masa pendaftaran sesi pelatihan resmi DIBUKA."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal membuka pendaftaran sesi {SessionId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat membuka pendaftaran sesi pelatihan."));
            }
        }

        [HttpPost("{id}/close-registration")]
        [ProducesResponseType(typeof(ApiResponse<TrainingSessionDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> CloseRegistration([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var currentUser = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Diklat_Admin";
                var session = await _sessionService.CloseRegistrationAsync(id, currentUser, cancellationToken);

                return Ok(ApiResponse<TrainingSessionDto>.Ok(session, "Masa pendaftaran sesi pelatihan resmi DITUTUP."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menutup pendaftaran sesi {SessionId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat menutup pendaftaran sesi pelatihan."));
            }
        }

        [HttpPost("{id}/complete")]
        [ProducesResponseType(typeof(ApiResponse<TrainingSessionDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> CompleteSession([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var currentUser = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Diklat_Admin";
                var session = await _sessionService.CompleteSessionAsync(id, currentUser, cancellationToken);

                return Ok(ApiResponse<TrainingSessionDto>.Ok(session, "Sesi pelatihan telah diselesaikan."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menyelesaikan sesi {SessionId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat menandai sesi pelatihan selesai."));
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 422)]
        public async Task<IActionResult> DeleteSession([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var currentUser = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Diklat_Admin";
                var success = await _sessionService.DeleteSessionAsync(id, currentUser, cancellationToken);

                if (!success)
                {
                    return NotFound(ApiResponse<string>.Fail(404, $"Sesi pelatihan dengan ID {id} tidak ditemukan."));
                }

                return Ok(ApiResponse<bool>.Ok(true, "Sesi pelatihan berhasil dihapus."));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(422, ApiResponse<string>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menghapus sesi pelatihan {SessionId}", id);
                return StatusCode(500, ApiResponse<string>.Fail(500, "Terjadi kesalahan internal saat menghapus sesi pelatihan."));
            }
        }
    }
}
