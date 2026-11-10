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
    [Route("api/v1/corporate/hrd/training/sessions/{sessionId:guid}/attendances")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEARNING_DEVELOPMENT",
        moduleName: "Human Resource Learning and Development",
        displayName: "Presensi Pelatihan",
        AreaName = "Corporate",
        ControllerName = "TrainingAttendance",
        Description = "Check-in/check-out peserta sesi pelatihan dan kelayakan kelulusan kehadiran",
        SortOrder = 14)]
    [Tags("Corporate - HRD Clinical Competency & OSCE Assessment")]
    [Authorize]
    public class TrainingAttendancesController : ControllerBase
    {
        private readonly ITrainingAttendanceService _attendanceService;

        public TrainingAttendancesController(ITrainingAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpPost("check-in")]
        [AccessAction("Record", "Mencatat Presensi Pelatihan", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("TrainingAttendance", "Record")]
        public async Task<IActionResult> RecordCheckIn([FromRoute] Guid sessionId, [FromBody] RecordAttendanceCheckInRequestDto request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
                var result = await _attendanceService.RecordCheckInAsync(sessionId, request, currentUserId);
                return Ok(ApiResponse<TrainingAttendanceDto>.Ok(result, "Check-in presensi peserta diklat berhasil dicatat."));
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

        [HttpPost("check-out")]
        [AccessAction("Record", "Mencatat Presensi Pelatihan", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("TrainingAttendance", "Record")]
        public async Task<IActionResult> RecordCheckOut([FromRoute] Guid sessionId, [FromBody] RecordAttendanceCheckOutRequestDto request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
                var result = await _attendanceService.RecordCheckOutAsync(sessionId, request, currentUserId);
                return Ok(ApiResponse<TrainingAttendanceDto>.Ok(result, "Check-out presensi peserta diklat berhasil dicatat."));
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

        [HttpGet]
        [AccessAction("Read", "Melihat Presensi Pelatihan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("TrainingAttendance", "Read")]
        public async Task<IActionResult> GetAttendances([FromRoute] Guid sessionId)
        {
            try
            {
                var results = await _attendanceService.GetSessionAttendancesAsync(sessionId);
                return Ok(ApiResponse<List<TrainingAttendanceDto>>.Ok(results, "Data presensi sesi diklat berhasil diambil."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        [HttpGet("eligibility/{workforceProfileId:guid}")]
        [AccessAction("Read", "Melihat Presensi Pelatihan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("TrainingAttendance", "Read")]
        public async Task<IActionResult> EvaluateStaffEligibility([FromRoute] Guid sessionId, [FromRoute] Guid workforceProfileId)
        {
            try
            {
                var result = await _attendanceService.EvaluateEligibilityAsync(sessionId, workforceProfileId);
                return Ok(ApiResponse<ParticipantAttendanceEligibilityDto>.Ok(result, "Evaluasi kelayakan ambang presensi berhasil dianalisis."));
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

        [HttpGet("eligibilities")]
        [AccessAction("Read", "Melihat Presensi Pelatihan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("TrainingAttendance", "Read")]
        public async Task<IActionResult> EvaluateAllEligibilities([FromRoute] Guid sessionId)
        {
            try
            {
                var results = await _attendanceService.EvaluateSessionAllEligibilitiesAsync(sessionId);
                return Ok(ApiResponse<List<ParticipantAttendanceEligibilityDto>>.Ok(results, "Evaluasi seluruh kelayakan peserta sesi diklat berhasil diproses."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }
    }
}
