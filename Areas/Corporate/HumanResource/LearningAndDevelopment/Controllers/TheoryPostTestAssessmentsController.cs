using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/hrd/training/sessions/{sessionId:guid}/assessments/theory-post-test")]
    [Tags("Corporate - HRD Clinical Competency & OSCE Assessment")]
    [Authorize]
    public class TheoryPostTestAssessmentsController : ControllerBase
    {
        private readonly ITheoryPostTestAssessmentService _postTestService;

        public TheoryPostTestAssessmentsController(ITheoryPostTestAssessmentService postTestService)
        {
            _postTestService = postTestService;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitPostTest([FromRoute] Guid sessionId, [FromBody] SubmitTheoryPostTestRequestDto request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
                var result = await _postTestService.SubmitPostTestAsync(sessionId, request, currentUserId);
                return Ok(ApiResponse<TheoryPostTestResultDto>.Ok(result, "Nilai ujian post-test teori berhasil disimpan dan dievaluasi."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        [HttpGet("participants/{workforceProfileId:guid}")]
        public async Task<IActionResult> GetParticipantSummary([FromRoute] Guid sessionId, [FromRoute] Guid workforceProfileId)
        {
            try
            {
                var result = await _postTestService.GetParticipantAssessmentSummaryAsync(sessionId, workforceProfileId);
                return Ok(ApiResponse<ParticipantAssessmentSummaryDto>.Ok(result, "Ringkasan nilai post-test peserta berhasil diambil."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSessionPostTests([FromRoute] Guid sessionId)
        {
            try
            {
                var results = await _postTestService.GetSessionPostTestsAsync(sessionId);
                return Ok(ApiResponse<List<TheoryPostTestResultDto>>.Ok(results, "Daftar hasil post-test teori sesi berhasil diambil."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }
    }
}
