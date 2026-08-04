using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/attendance/schedule-resolver")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Schedule Resolver",
        AreaName = "Corporate",
        ControllerName = "AttendanceScheduleResolver",
        Description = "Resolve canonical daily attendance schedule from roster and work schedule assignment",
        SortOrder = 2)]
    [Tags("Corporate / Human Resource / Attendance Management / Attendance Schedule Resolver")]
    public class AttendanceScheduleResolverController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.AttendanceManagement";

        private readonly AttendanceScheduleResolverService _service;
        private readonly LoggerService _loggerService;

        public AttendanceScheduleResolverController(
            AttendanceScheduleResolverService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceScheduleResolverMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Schedule Resolver", Description = "Melihat metadata attendance schedule resolver", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceScheduleResolver", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = _service.GetMetadata();
            return Ok(ApiResponse<AttendanceScheduleResolverMetadataResponse>.Ok(
                result,
                "Metadata attendance schedule resolver berhasil diambil."));
        }

        [HttpGet("resolve")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceScheduleResolutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Resolve Attendance Schedule", Description = "Menyelesaikan jadwal attendance satu workforce pada satu tanggal", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceScheduleResolver", "Read")]
        public async Task<IActionResult> Resolve(
            [FromQuery] Guid workforceProfileId,
            [FromQuery] DateOnly workDate,
            CancellationToken cancellationToken)
        {
            var result = await _service.ResolveAsync(
                workforceProfileId,
                workDate,
                cancellationToken);

            if (!result.Success || result.Data == null)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "AttendanceScheduleResolver.Resolve",
                result.Message,
                new
                {
                    result.Data.WorkforceProfileId,
                    result.Data.WorkDate,
                    result.Data.IsResolved,
                    result.Data.ScheduleSource,
                    result.Data.PrimaryShiftAssignmentId,
                    result.Data.WorkScheduleAssignmentId,
                    result.Data.HasBlockingConflict
                });

            return Ok(ApiResponse<AttendanceScheduleResolutionResponse>.Ok(
                result.Data,
                result.Message));
        }

        [HttpGet("range")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceScheduleRangeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Resolve Attendance Schedule Range", Description = "Menyelesaikan rentang jadwal attendance workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceScheduleResolver", "Read")]
        public async Task<IActionResult> ResolveRange(
            [FromQuery] Guid workforceProfileId,
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate,
            CancellationToken cancellationToken)
        {
            var result = await _service.ResolveRangeAsync(
                workforceProfileId,
                startDate,
                endDate,
                cancellationToken);

            if (!result.Success || result.Data == null)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "AttendanceScheduleResolver.ResolveRange",
                result.Message,
                new
                {
                    result.Data.WorkforceProfileId,
                    result.Data.StartDate,
                    result.Data.EndDate,
                    result.Data.TotalDate,
                    result.Data.ResolvedDate,
                    result.Data.UnresolvedDate,
                    result.Data.BlockingConflictCount
                });

            return Ok(ApiResponse<AttendanceScheduleRangeResponse>.Ok(
                result.Data,
                result.Message));
        }
    }
}
