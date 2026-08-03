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
    [Route("api/v1/corporate/human-resource/leave/executions")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Execution",
        AreaName = "Corporate",
        ControllerName = "LeaveExecution",
        Description = "Leave execution monitoring, attendance integration, balance lifecycle, and cancellation application",
        SortOrder = 9)]
    [Tags("Corporate / Human Resource / Leave Management / Leave Execution")]
    public class LeaveExecutionController : ControllerBase
    {
        private readonly LeaveExecutionQueryService _queryService;
        private readonly LeaveExecutionProcessorService _processorService;

        public LeaveExecutionController(
            LeaveExecutionQueryService queryService,
            LeaveExecutionProcessorService processorService)
        {
            _queryService = queryService;
            _processorService = processorService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction(
            "Read",
            "Read Leave Execution Metadata",
            Description = "Melihat metadata filter leave execution",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("LeaveExecution", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(ApiResponse<LeaveExecutionFilterMetadataResponse>.Ok(
                _queryService.GetMetadata(),
                "Metadata leave execution berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction(
            "Read",
            "Read Leave Execution Summary",
            Description = "Melihat ringkasan leave execution dan issue",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("LeaveExecution", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] LeaveExecutionQueryRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _queryService.GetSummaryAsync(request, cancellationToken));
        }

        [HttpGet]
        [AccessAction(
            "Read",
            "Read Leave Execution",
            Description = "Melihat daftar monitoring leave execution",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("LeaveExecution", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] LeaveExecutionQueryRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _queryService.GetPagedAsync(request, cancellationToken));
        }

        [HttpGet("{leaveRequestId:guid}")]
        [AccessAction(
            "Read",
            "Read Leave Execution Detail",
            Description = "Melihat detail leave execution dan attendance integration",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("LeaveExecution", "Read")]
        public async Task<IActionResult> GetById(
            Guid leaveRequestId,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _queryService.GetByLeaveRequestIdAsync(
                leaveRequestId,
                cancellationToken));
        }

        [HttpGet("{leaveRequestId:guid}/reconciliation")]
        [AccessAction(
            "Read",
            "Reconcile Leave Execution",
            Description = "Melihat reconciliation leave request, attendance, dan balance ledger",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("LeaveExecution", "Read")]
        public async Task<IActionResult> Reconcile(
            Guid leaveRequestId,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _queryService.ReconcileAsync(
                leaveRequestId,
                cancellationToken));
        }

        [HttpPost("process-due")]
        [AccessAction(
            "Execute",
            "Process Due Leave Execution",
            Description = "Memproses leave execution yang sudah jatuh tempo",
            AccessType = AccessTypes.Update,
            SortOrder = 2)]
        [AccessPermission("LeaveExecution", "Execute")]
        public async Task<IActionResult> ProcessDue(
            [FromBody] ProcessDueLeaveRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _processorService.ProcessDueAsync(
                request,
                GetCurrentUserId(),
                cancellationToken));
        }

        [HttpPost("{leaveRequestId:guid}/execute")]
        [AccessAction(
            "Execute",
            "Execute Leave Request",
            Description = "Menjalankan leave execution untuk satu pengajuan",
            AccessType = AccessTypes.Update,
            SortOrder = 2)]
        [AccessPermission("LeaveExecution", "Execute")]
        public async Task<IActionResult> Execute(
            Guid leaveRequestId,
            [FromBody] ExecuteLeaveRequestRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _processorService.ExecuteAsync(
                leaveRequestId,
                request,
                GetCurrentUserId(),
                cancellationToken));
        }

        [HttpPost("{leaveRequestId:guid}/retry")]
        [AccessAction(
            "Retry",
            "Retry Leave Execution",
            Description = "Mencoba ulang attendance atau balance execution yang gagal",
            AccessType = AccessTypes.Update,
            SortOrder = 3)]
        [AccessPermission("LeaveExecution", "Retry")]
        public async Task<IActionResult> Retry(
            Guid leaveRequestId,
            [FromBody] ExecuteLeaveRequestRequest request,
            CancellationToken cancellationToken)
        {
            request.ForceRetry = true;
            return ToActionResult(await _processorService.ExecuteAsync(
                leaveRequestId,
                request,
                GetCurrentUserId(),
                cancellationToken));
        }

        [HttpPost("{leaveRequestId:guid}/reverse")]
        [AccessAction(
            "Reverse",
            "Reverse Leave Execution",
            Description = "Membalik attendance integration dan restore leave balance",
            AccessType = AccessTypes.Update,
            SortOrder = 4)]
        [AccessPermission("LeaveExecution", "Reverse")]
        public async Task<IActionResult> Reverse(
            Guid leaveRequestId,
            [FromBody] ReverseLeaveExecutionRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _processorService.ReverseAsync(
                leaveRequestId,
                request,
                GetCurrentUserId(),
                cancellationToken));
        }

        [HttpPost("cancellations/{cancellationRequestId:guid}/apply")]
        [AccessAction(
            "ApplyCancellation",
            "Apply Approved Leave Cancellation",
            Description = "Menerapkan cancellation request yang sudah disetujui",
            AccessType = AccessTypes.Update,
            SortOrder = 5)]
        [AccessPermission("LeaveExecution", "ApplyCancellation")]
        public async Task<IActionResult> ApplyCancellation(
            Guid cancellationRequestId,
            [FromBody] ApplyLeaveCancellationRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _processorService.ApplyApprovedCancellationAsync(
                cancellationRequestId,
                request,
                GetCurrentUserId(),
                cancellationToken));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
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
