using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/hrd/training/sessions/{sessionId:guid}/assessments/clinical-osce")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEARNING_DEVELOPMENT",
        moduleName: "Human Resource Learning and Development",
        displayName: "Ujian Praktik Klinis (OSCE)",
        AreaName = "Corporate",
        ControllerName = "ClinicalOsceAssessment",
        Description = "Penilaian OSCE peserta sesi pelatihan klinis",
        SortOrder = 16)]
    [Tags("Corporate - HRD Clinical Competency & OSCE Assessment")]
    [Authorize]
    public class ClinicalOsceAssessmentsController : ControllerBase
    {
        private readonly IClinicalOsceAssessmentService _osceService;

        public ClinicalOsceAssessmentsController(IClinicalOsceAssessmentService osceService)
        {
            _osceService = osceService;
        }

        [HttpPost]
        [AccessAction("Grade", "Menilai Ujian OSCE", AccessType = AccessTypes.Update, SortOrder = 1)]
        [AccessPermission("ClinicalOsceAssessment", "Grade")]
        public async Task<IActionResult> SubmitClinicalOsce([FromRoute] Guid sessionId, [FromBody] SubmitClinicalOsceRequestDto request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
                var result = await _osceService.SubmitClinicalOsceAsync(sessionId, request, currentUserId);
                return Ok(ApiResponse<ClinicalOsceResultDto>.Ok(result, "Asesmen simulasi OSCE klinis berhasil diverifikasi dan disimpan."));
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
        [AccessAction("Read", "Melihat Nilai OSCE", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("ClinicalOsceAssessment", "Read")]
        public async Task<IActionResult> GetParticipantOsce([FromRoute] Guid sessionId, [FromRoute] Guid workforceProfileId)
        {
            try
            {
                var result = await _osceService.GetParticipantOsceAsync(sessionId, workforceProfileId);
                if (result == null)
                {
                    return NotFound(ApiResponse<string>.Fail(StatusCodes.Status404NotFound, "Hasil asesmen OSCE untuk peserta ini tidak ditemukan."));
                }
                return Ok(ApiResponse<ClinicalOsceResultDto>.Ok(result, "Hasil asesmen OSCE klinis peserta berhasil diambil."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Nilai OSCE", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("ClinicalOsceAssessment", "Read")]
        public async Task<IActionResult> GetSessionOsces([FromRoute] Guid sessionId)
        {
            try
            {
                var results = await _osceService.GetSessionOscesAsync(sessionId);
                return Ok(ApiResponse<List<ClinicalOsceResultDto>>.Ok(results, "Daftar hasil asesmen OSCE sesi berhasil diambil."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }
    }
}
