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
    [Route("api/v1/corporate/hrd/training/certificates")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEARNING_DEVELOPMENT",
        moduleName: "Human Resource Learning and Development",
        displayName: "Sertifikat Pelatihan",
        AreaName = "Corporate",
        ControllerName = "TrainingCertificate",
        Description = "Penerbitan dan verifikasi sertifikat pelatihan staf",
        SortOrder = 17)]
    [Tags("Corporate - HRD Clinical Competency & OSCE Assessment")]
    [Authorize]
    public class TrainingCertificatesController : ControllerBase
    {
        private readonly ITrainingCertificationService _certificationService;

        public TrainingCertificatesController(ITrainingCertificationService certificationService)
        {
            _certificationService = certificationService;
        }

        [HttpPost("issue")]
        [AccessAction("Issue", "Menerbitkan Sertifikat Pelatihan", AccessType = AccessTypes.Update, SortOrder = 1)]
        [AccessPermission("TrainingCertificate", "Issue")]
        public async Task<IActionResult> IssueCertificate([FromBody] IssueCertificateRequestDto request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
                var result = await _certificationService.IssueCertificateAsync(request, currentUserId);
                return Ok(ApiResponse<TrainingCertificateDto>.Ok(result, "e-Sertifikat pelatihan berhasil diterbitkan dan disinkronkan ke e-Dossier portofolio tenaga kerja."));
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

        [HttpGet("verify/{certificateNumber}")]
        [AccessAction("Read", "Melihat Sertifikat Pelatihan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("TrainingCertificate", "Read")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCertificate([FromRoute] string certificateNumber)
        {
            try
            {
                var result = await _certificationService.GetCertificateByNumberAsync(certificateNumber);
                if (result == null)
                {
                    return NotFound(ApiResponse<string>.Fail(StatusCodes.Status404NotFound, $"Sertifikat dengan nomor '{certificateNumber}' tidak ditemukan atau tidak valid."));
                }
                return Ok(ApiResponse<TrainingCertificateDto>.Ok(result, "Verifikasi keabsahan sertifikat pelatihan valid."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        [HttpGet("participants/{participantId:guid}")]
        [AccessAction("Read", "Melihat Sertifikat Pelatihan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("TrainingCertificate", "Read")]
        public async Task<IActionResult> GetByParticipant([FromRoute] Guid participantId)
        {
            try
            {
                var results = await _certificationService.GetCertificatesByParticipantAsync(participantId);
                return Ok(ApiResponse<List<TrainingCertificateDto>>.Ok(results, "Daftar sertifikat peserta berhasil diambil."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        [HttpGet("staff/{workforceProfileId:guid}")]
        [AccessAction("Read", "Melihat Sertifikat Pelatihan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("TrainingCertificate", "Read")]
        public async Task<IActionResult> GetByStaff([FromRoute] Guid workforceProfileId)
        {
            try
            {
                var results = await _certificationService.GetCertificatesByStaffAsync(workforceProfileId);
                return Ok(ApiResponse<List<TrainingCertificateDto>>.Ok(results, "Daftar sertifikat portofolio staf berhasil diambil."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }
    }
}
