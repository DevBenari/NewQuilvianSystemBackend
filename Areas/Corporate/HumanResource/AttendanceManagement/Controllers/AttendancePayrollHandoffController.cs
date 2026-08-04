using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/attendance/payroll-handoff")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Payroll Handoff",
        AreaName = "Corporate",
        ControllerName = "AttendancePayrollHandoff",
        Description = "Corporate human resource attendance handoff to payroll snapshot",
        SortOrder = 7)]
    [Tags("Corporate / Human Resource / Attendance Management / Attendance Payroll Handoff")]
    public class AttendancePayrollHandoffController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.AttendanceManagement";

        private readonly AttendancePayrollHandoffService _service;
        private readonly LoggerService _loggerService;

        public AttendancePayrollHandoffController(
            AttendancePayrollHandoffService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<AttendancePayrollHandoffFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Payroll Handoff", Description = "Melihat metadata filter attendance payroll handoff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendancePayrollHandoff", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<AttendancePayrollHandoffFilterMetadataResponse>.Ok(
                _service.GetMetadata(),
                "Metadata attendance payroll handoff berhasil diambil."));
        }

        [HttpGet("payroll-runs/options")]
        [ProducesResponseType(typeof(ApiResponse<List<AttendancePayrollRunOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Payroll Handoff", Description = "Melihat pilihan payroll run untuk attendance handoff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendancePayrollHandoff", "Read")]
        public async Task<IActionResult> GetPayrollRunOptions(
            [FromQuery] string? search,
            [FromQuery] int take = 100,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPayrollRunOptionsAsync(search, take, cancellationToken);
            return Ok(ApiResponse<List<AttendancePayrollRunOptionResponse>>.Ok(
                result,
                "Pilihan payroll run berhasil diambil."));
        }

        [HttpGet("payroll-runs/{payrollRunId:guid}/summary")]
        [ProducesResponseType(typeof(ApiResponse<AttendancePayrollHandoffSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Read", "Read Attendance Payroll Handoff", Description = "Melihat ringkasan kesiapan attendance payroll handoff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendancePayrollHandoff", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid payrollRunId,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetSummaryAsync(payrollRunId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("payroll-runs/{payrollRunId:guid}/preview")]
        [ProducesResponseType(typeof(ApiResponse<AttendancePayrollHandoffPreviewPagedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Read", "Read Attendance Payroll Handoff", Description = "Melihat preview dan validasi attendance sebelum handoff ke payroll", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendancePayrollHandoff", "Read")]
        public async Task<IActionResult> GetPreview(
            Guid payrollRunId,
            [FromQuery] AttendancePayrollHandoffQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPreviewAsync(payrollRunId, request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("payroll-runs/{payrollRunId:guid}/reconciliation")]
        [ProducesResponseType(typeof(ApiResponse<AttendancePayrollHandoffReconciliationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Read", "Read Attendance Payroll Handoff Reconciliation", Description = "Melihat rekonsiliasi attendance dengan payroll attendance input", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendancePayrollHandoff", "Read")]
        public async Task<IActionResult> GetReconciliation(
            Guid payrollRunId,
            [FromQuery] AttendancePayrollHandoffReconciliationQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetReconciliationAsync(payrollRunId, request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("payroll-runs/{payrollRunId:guid}/execute")]
        [ProducesResponseType(typeof(ApiResponse<AttendancePayrollHandoffExecutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Execute", "Execute Attendance Payroll Handoff", Description = "Membuat snapshot attendance untuk payroll run", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("AttendancePayrollHandoff", "Execute")]
        public async Task<IActionResult> Execute(
            Guid payrollRunId,
            [FromBody] ExecuteAttendancePayrollHandoffRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.ExecuteAsync(
                payrollRunId,
                request ?? new ExecuteAttendancePayrollHandoffRequest(),
                actorUserId,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "AttendancePayrollHandoff.Execute",
                    "Menjalankan attendance payroll handoff.",
                    new
                    {
                        result.Data.PayrollRunId,
                        result.Data.RunNumber,
                        result.Data.TotalTarget,
                        result.Data.CreatedCount,
                        result.Data.UpdatedCount,
                        result.Data.IdempotentCount,
                        result.Data.FailedCount
                    });
            }

            return ToActionResult(result);
        }

        [HttpPost("payroll-runs/{payrollRunId:guid}/repair")]
        [ProducesResponseType(typeof(ApiResponse<AttendancePayrollHandoffExecutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Repair", "Repair Attendance Payroll Handoff", Description = "Memperbaiki snapshot attendance payroll yang hilang atau berubah", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendancePayrollHandoff", "Repair")]
        public async Task<IActionResult> Repair(
            Guid payrollRunId,
            [FromBody] RepairAttendancePayrollHandoffRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.RepairAsync(
                payrollRunId,
                request ?? new RepairAttendancePayrollHandoffRequest(),
                actorUserId,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "AttendancePayrollHandoff.Repair",
                    "Memperbaiki attendance payroll handoff.",
                    new
                    {
                        result.Data.PayrollRunId,
                        result.Data.TotalTarget,
                        result.Data.CreatedCount,
                        result.Data.UpdatedCount,
                        result.Data.FailedCount
                    });
            }

            return ToActionResult(result);
        }

        [HttpPost("payroll-runs/{payrollRunId:guid}/rollback")]
        [ProducesResponseType(typeof(ApiResponse<AttendancePayrollHandoffRollbackResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Rollback", "Rollback Attendance Payroll Handoff", Description = "Membatalkan attendance payroll input sebelum payroll run final", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("AttendancePayrollHandoff", "Rollback")]
        public async Task<IActionResult> Rollback(
            Guid payrollRunId,
            [FromBody] RollbackAttendancePayrollHandoffRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.RollbackAsync(
                payrollRunId,
                request,
                actorUserId,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "AttendancePayrollHandoff.Rollback",
                    "Melakukan rollback attendance payroll handoff.",
                    result.Data);
            }

            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(AttendancePayrollHandoffServiceResult<T> result)
        {
            if (result.Success)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<T>.Ok(result.Data!, result.Message));
            }

            return StatusCode(
                result.StatusCode,
                ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
