using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/overtime-management/requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Workflow",
        AreaName = "Corporate",
        ControllerName = "OvertimeWorkflow",
        Description = "Generic workflow integration and lifecycle recovery for overtime requests",
        SortOrder = 2)]
    [Tags("Corporate / Human Resource / Overtime Management / Overtime Workflow")]
    public class OvertimeWorkflowController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.OvertimeManagement";
        private readonly OvertimeRequestWorkflowIntegrationService _integrationService;
        private readonly LoggerService _loggerService;

        public OvertimeWorkflowController(
            OvertimeRequestWorkflowIntegrationService integrationService,
            LoggerService loggerService)
        {
            _integrationService = integrationService;
            _loggerService = loggerService;
        }

        [HttpPost("{id:guid}/workflow/start")]
        [AccessAction("Submit", "Submit Overtime Workflow", Description = "Membuat atau submit ulang Generic Workflow untuk overtime request", AccessType = AccessTypes.Update, SortOrder = 1)]
        [AccessPermission("OvertimeWorkflow", "Submit")]
        public async Task<IActionResult> StartOrResubmit(
            Guid id,
            [FromBody] StartOvertimeWorkflowRequest? request,
            CancellationToken cancellationToken = default)
        {
            var result = await _integrationService.StartOrResubmitAsync(
                id,
                request,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimeWorkflow.StartOrResubmit",
                    "Membuat atau submit ulang Generic Workflow Overtime Request.",
                    result.Data);
            }

            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/workflow/synchronize")]
        [AccessAction("Synchronize", "Synchronize Overtime Workflow", Description = "Menyinkronkan ulang status workflow ke overtime request", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("OvertimeWorkflow", "Synchronize")]
        public async Task<IActionResult> Synchronize(
            Guid id,
            [FromBody] SynchronizeOvertimeWorkflowRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _integrationService.SynchronizeAsync(
                id,
                actorUserId,
                request?.AllowAutoApply ?? true,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimeWorkflow.Synchronize",
                    "Menyinkronkan Generic Workflow ke Overtime Request.",
                    result.Data);
            }

            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(OvertimeWorkflowServiceResult<T> result) =>
            result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
