using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/leave/request-workflow")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Request Workflow",
        AreaName = "Corporate",
        ControllerName = "LeaveRequestWorkflow",
        Description = "Workflow lifecycle synchronization and balance retry for leave requests",
        SortOrder = 7)]
    [Tags("Corporate / Human Resource / Leave Management / Leave Request Workflow")]
    public class LeaveRequestWorkflowController : ControllerBase
    {
        private readonly LeaveRequestWorkflowIntegrationService _service;

        public LeaveRequestWorkflowController(
            LeaveRequestWorkflowIntegrationService service)
        {
            _service = service;
        }

        [HttpGet("filters/metadata")]
        [AccessAction(
            "Read",
            "Read Leave Request Workflow Metadata",
            Description = "Melihat metadata integrasi workflow leave request",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("LeaveRequestWorkflow", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(
                ApiResponse<LeaveRequestWorkflowMetadataResponse>.Ok(
                    _service.GetMetadata(),
                    "Metadata workflow leave request berhasil diambil."));
        }

        [HttpGet("{leaveRequestId:guid}")]
        [AccessAction(
            "Read",
            "Read Leave Request Workflow Status",
            Description = "Melihat status sinkronisasi workflow dan balance",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("LeaveRequestWorkflow", "Read")]
        public async Task<IActionResult> GetStatus(
            Guid leaveRequestId,
            CancellationToken cancellationToken)
        {
            return ToActionResult(
                await _service.GetStatusAsync(
                    leaveRequestId,
                    cancellationToken));
        }

        [HttpPost("{leaveRequestId:guid}/synchronize")]
        [AccessAction(
            "Synchronize",
            "Synchronize Leave Request Workflow",
            Description = "Menyinkronkan status workflow ke leave request dan balance",
            AccessType = AccessTypes.Update,
            SortOrder = 2)]
        [AccessPermission("LeaveRequestWorkflow", "Synchronize")]
        public async Task<IActionResult> Synchronize(
            Guid leaveRequestId,
            [FromBody] LeaveRequestWorkflowSynchronizeRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(
                await _service.SynchronizeAsync(
                    leaveRequestId,
                    GetCurrentUserId(),
                    request.AllowBalanceApply,
                    cancellationToken));
        }

        [HttpPost("{leaveRequestId:guid}/retry-balance")]
        [AccessAction(
            "RetryBalance",
            "Retry Leave Request Balance",
            Description = "Mencoba ulang reservation atau deduction leave balance",
            AccessType = AccessTypes.Update,
            SortOrder = 3)]
        [AccessPermission("LeaveRequestWorkflow", "RetryBalance")]
        public async Task<IActionResult> RetryBalance(
            Guid leaveRequestId,
            CancellationToken cancellationToken)
        {
            return ToActionResult(
                await _service.RetryBalanceAsync(
                    leaveRequestId,
                    GetCurrentUserId(),
                    cancellationToken));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("user_id");

            return Guid.TryParse(value, out var id)
                ? id
                : Guid.Empty;
        }

        private IActionResult ToActionResult<T>(LeaveRequestServiceResult<T> result)
        {
            var response = result.Success
                ? ApiResponse<T>.Ok(result.Data, result.Message)
                : ApiResponse<T>.Fail(result.StatusCode, result.Message);

            return StatusCode(result.StatusCode, response);
        }
    }
}
