using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/hrd/training/plataran-sehat-skp")]
    [Tags("Corporate - HRD Plataran Sehat SKP & OPPE Feed")]
    [Authorize]
    public class PlataranSehatSkpsController : ControllerBase
    {
        private readonly IPlataranSehatSkpService _skpService;

        public PlataranSehatSkpsController(IPlataranSehatSkpService skpService)
        {
            _skpService = skpService;
        }

        [HttpPost]
        public async Task<IActionResult> RecordActivity([FromBody] RecordSkpActivityRequestDto request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
                var result = await _skpService.RecordSkpActivityAsync(request, currentUserId);
                return Ok(ApiResponse<PlataranSehatSkpRecordDto>.Ok(result, "Pencatatan perolehan SKP nakes berhasil disimpan."));
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

        [HttpGet("portfolio/{workforceProfileId:guid}")]
        public async Task<IActionResult> GetStaffPortfolio([FromRoute] Guid workforceProfileId)
        {
            try
            {
                var result = await _skpService.GetStaffSkpPortfolioAsync(workforceProfileId);
                return Ok(ApiResponse<SkpPortfolioSummaryDto>.Ok(result, "Portofolio target 50 SKP nakes berhasil diambil."));
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

        [HttpPost("sync-oppe/{workforceProfileId:guid}")]
        public async Task<IActionResult> SyncToOppe([FromRoute] Guid workforceProfileId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
                var result = await _skpService.SyncStaffSkpToOppeAsync(workforceProfileId, currentUserId);
                return Ok(ApiResponse<SyncOppeResponseDto>.Ok(result, "Sinkronisasi data SKP ke evaluasi OPPE Sub-15 berhasil dilakukan."));
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

        [HttpPost("verify/{skpRecordId:guid}")]
        public async Task<IActionResult> VerifyPlataranSehat([FromRoute] Guid skpRecordId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
                var result = await _skpService.VerifyPlataranSehatAsync(skpRecordId, currentUserId);
                return Ok(ApiResponse<PlataranSehatSkpRecordDto>.Ok(result, "Verifikasi sertifikat Plataran Sehat Kemenkes SatuSehat berhasil."));
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
    }
}
