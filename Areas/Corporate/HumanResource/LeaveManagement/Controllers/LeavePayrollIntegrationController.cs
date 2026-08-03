using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/leave/payroll-integration")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Payroll Integration",
        AreaName = "Corporate",
        ControllerName = "LeavePayrollIntegration",
        Description = "Leave days, unpaid leave, allowance quantity, encashment, payroll handoff, reconciliation, and rollback",
        SortOrder = 10)]
    [Tags("Corporate / Human Resource / Leave Management / Leave Payroll Integration")]
    public class LeavePayrollIntegrationController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.LeaveManagement";

        private readonly LeavePayrollIntegrationService _service;
        private readonly LoggerService _loggerService;

        public LeavePayrollIntegrationController(
            LeavePayrollIntegrationService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LeavePayrollIntegrationMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Payroll Integration", Description = "Melihat metadata leave payroll integration", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeavePayrollIntegration", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(ApiResponse<LeavePayrollIntegrationMetadataResponse>.Ok(
                _service.GetMetadata(),
                "Metadata leave payroll integration berhasil diambil."));
        }

        [HttpGet("payroll-runs/options")]
        [ProducesResponseType(typeof(ApiResponse<List<LeavePayrollRunOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Payroll Integration", Description = "Melihat pilihan payroll run", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeavePayrollIntegration", "Read")]
        public async Task<IActionResult> GetPayrollRunOptions(
            [FromQuery] string? search,
            [FromQuery] int take = 100,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPayrollRunOptionsAsync(search, take, cancellationToken);
            return Ok(ApiResponse<List<LeavePayrollRunOptionResponse>>.Ok(
                result,
                "Pilihan payroll run berhasil diambil."));
        }

        [HttpGet("payroll-components/options")]
        [ProducesResponseType(typeof(ApiResponse<List<LeavePayrollComponentOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Payroll Integration", Description = "Melihat pilihan payroll component untuk mapping leave", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeavePayrollIntegration", "Read")]
        public async Task<IActionResult> GetPayrollComponentOptions(
            [FromQuery] string? search,
            [FromQuery] int take = 100,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPayrollComponentOptionsAsync(search, take, cancellationToken);
            return Ok(ApiResponse<List<LeavePayrollComponentOptionResponse>>.Ok(
                result,
                "Pilihan payroll component berhasil diambil."));
        }

        [HttpGet("payroll-runs/{payrollRunId:guid}/summary")]
        [ProducesResponseType(typeof(ApiResponse<LeavePayrollIntegrationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Payroll Integration", Description = "Melihat ringkasan leave payroll integration", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeavePayrollIntegration", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid payrollRunId,
            CancellationToken cancellationToken = default)
        {
            return ToActionResult(await _service.GetSummaryAsync(payrollRunId, cancellationToken));
        }

        [HttpGet("payroll-runs/{payrollRunId:guid}/preview")]
        [ProducesResponseType(typeof(ApiResponse<LeavePayrollIntegrationPreviewPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Payroll Integration", Description = "Melihat preview leave sebelum handoff ke payroll", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeavePayrollIntegration", "Read")]
        public async Task<IActionResult> GetPreview(
            Guid payrollRunId,
            [FromQuery] LeavePayrollIntegrationQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            return ToActionResult(await _service.GetPreviewAsync(payrollRunId, request, cancellationToken));
        }

        [HttpGet("payroll-runs/{payrollRunId:guid}/reconciliation")]
        [ProducesResponseType(typeof(ApiResponse<LeavePayrollReconciliationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Reconcile", "Reconcile Leave Payroll Integration", Description = "Membandingkan leave execution dengan payroll input", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LeavePayrollIntegration", "Reconcile")]
        public async Task<IActionResult> GetReconciliation(
            Guid payrollRunId,
            [FromQuery] LeavePayrollReconciliationQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            return ToActionResult(await _service.GetReconciliationAsync(payrollRunId, request, cancellationToken));
        }

        [HttpPost("payroll-runs/{payrollRunId:guid}/execute")]
        [ProducesResponseType(typeof(ApiResponse<LeavePayrollIntegrationExecutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Execute", "Execute Leave Payroll Integration", Description = "Mengirim paid leave, unpaid leave, allowance quantity, dan encashment quantity ke payroll", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LeavePayrollIntegration", "Execute")]
        public async Task<IActionResult> Execute(
            Guid payrollRunId,
            [FromBody] ExecuteLeavePayrollIntegrationRequest? request,
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
                request ?? new ExecuteLeavePayrollIntegrationRequest(),
                actorUserId,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "LeavePayrollIntegration.Execute",
                    "Menjalankan leave payroll integration.",
                    new
                    {
                        result.Data.PayrollRunId,
                        result.Data.RunNumber,
                        result.Data.TotalTarget,
                        result.Data.AttendanceInputUpdatedCount,
                        result.Data.VariableInputCreatedCount,
                        result.Data.VariableInputUpdatedCount,
                        result.Data.FailedCount,
                        result.Data.PaidLeaveDays,
                        result.Data.UnpaidLeaveDays,
                        result.Data.EncashmentPayoutDays
                    });
            }

            return ToActionResult(result);
        }

        [HttpPost("payroll-runs/{payrollRunId:guid}/rollback")]
        [ProducesResponseType(typeof(ApiResponse<LeavePayrollIntegrationRollbackResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Rollback", "Rollback Leave Payroll Integration", Description = "Membatalkan leave payroll input sebelum payroll final", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeavePayrollIntegration", "Rollback")]
        public async Task<IActionResult> Rollback(
            Guid payrollRunId,
            [FromBody] RollbackLeavePayrollIntegrationRequest request,
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
                    "LeavePayrollIntegration.Rollback",
                    "Rollback leave payroll integration.",
                    result.Data);
            }

            return ToActionResult(result);
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private IActionResult ToActionResult<T>(LeavePayrollIntegrationServiceResult<T> result)
        {
            var response = result.Success
                ? ApiResponse<T>.Ok(result.Data, result.Message)
                : ApiResponse<T>.Fail(result.StatusCode, result.Message);
            return StatusCode(result.StatusCode, response);
        }
    }
}
