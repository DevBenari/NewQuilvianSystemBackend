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
    [Route("api/v1/corporate/hrd/training/plataran-sehat-skp")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEARNING_DEVELOPMENT",
        moduleName: "Human Resource Learning and Development",
        displayName: "SKP Plataran Sehat",
        AreaName = "Corporate",
        ControllerName = "PlataranSehatSkp",
        Description = "Pencatatan, verifikasi, dan sinkronisasi SKP Kemenkes ke OPPE",
        SortOrder = 20)]
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
        [AccessAction("Create", "Mencatat Kegiatan SKP", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("PlataranSehatSkp", "Create")]
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
        [AccessAction("Read", "Melihat Portofolio SKP", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("PlataranSehatSkp", "Read")]
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
        [AccessAction("Sync", "Menyinkronkan SKP ke OPPE", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PlataranSehatSkp", "Sync")]
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
        [AccessAction("Verify", "Memverifikasi SKP Plataran Sehat", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PlataranSehatSkp", "Verify")]
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
