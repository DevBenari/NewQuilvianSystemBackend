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
    [Route("api/v1/corporate/hrd/training/bonding-contracts")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEARNING_DEVELOPMENT",
        moduleName: "Human Resource Learning and Development",
        displayName: "Ikatan Dinas Pelatihan",
        AreaName = "Corporate",
        ControllerName = "TrainingBonding",
        Description = "Kontrak ikatan dinas pascapelatihan dan penalti pelanggaran",
        SortOrder = 19)]
    [Tags("Corporate - HRD Training Enrollment & Budget Governance")]
    [Authorize]
    public class TrainingBondingsController : ControllerBase
    {
        private readonly ITrainingBondingService _bondingService;

        public TrainingBondingsController(ITrainingBondingService bondingService)
        {
            _bondingService = bondingService;
        }

        [HttpPost]
        [AccessAction("Manage", "Mengelola Ikatan Dinas", AccessType = AccessTypes.Update, SortOrder = 1)]
        [AccessPermission("TrainingBonding", "Manage")]
        public async Task<IActionResult> GenerateContract([FromBody] GenerateBondContractRequestDto request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
                var result = await _bondingService.GenerateBondContractAsync(request, currentUserId);
                return Ok(ApiResponse<TrainingBondContractDto>.Ok(result, "Akta perjanjian ikatan dinas retensi pelatihan berhasil diterbitkan."));
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

        [HttpPost("{contractId:guid}/process-breach-penalty")]
        [AccessAction("Manage", "Mengelola Ikatan Dinas", AccessType = AccessTypes.Update, SortOrder = 1)]
        [AccessPermission("TrainingBonding", "Manage")]
        public async Task<IActionResult> ProcessBreachPenalty([FromRoute] Guid contractId, [FromBody] ProcessBreachPenaltyRequestDto request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
                var result = await _bondingService.CalculateAndApplyPenaltyAsync(contractId, request, currentUserId);
                return Ok(ApiResponse<TrainingBondContractDto>.Ok(result, "Kompensasi denda wanprestasi resign dini berhasil dihitung dan invoice diterbitkan."));
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

        [HttpGet("{contractId:guid}")]
        [AccessAction("Read", "Melihat Ikatan Dinas", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("TrainingBonding", "Read")]
        public async Task<IActionResult> GetById([FromRoute] Guid contractId)
        {
            try
            {
                var result = await _bondingService.GetContractByIdAsync(contractId);
                if (result == null)
                {
                    return NotFound(ApiResponse<string>.Fail(StatusCodes.Status404NotFound, $"Kontrak ikatan dinas ID '{contractId}' tidak ditemukan."));
                }
                return Ok(ApiResponse<TrainingBondContractDto>.Ok(result, "Data akta ikatan dinas berhasil diambil."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        [HttpGet("staff/{workforceProfileId:guid}")]
        [AccessAction("Read", "Melihat Ikatan Dinas", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("TrainingBonding", "Read")]
        public async Task<IActionResult> GetByStaff([FromRoute] Guid workforceProfileId)
        {
            try
            {
                var results = await _bondingService.GetContractsByStaffAsync(workforceProfileId);
                return Ok(ApiResponse<List<TrainingBondContractDto>>.Ok(results, "Daftar ikatan dinas staf berhasil diambil."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        [HttpGet("active")]
        [AccessAction("Read", "Melihat Ikatan Dinas", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("TrainingBonding", "Read")]
        public async Task<IActionResult> GetActiveContracts()
        {
            try
            {
                var results = await _bondingService.GetActiveContractsAsync();
                return Ok(ApiResponse<List<TrainingBondContractDto>>.Ok(results, "Daftar ikatan dinas aktif rumah sakit berhasil diambil."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }
    }
}
