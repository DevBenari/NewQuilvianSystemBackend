using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/leave/adjustments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Adjustment",
        AreaName = "Corporate",
        ControllerName = "LeaveAdjustment",
        Description = "Opening balance, manual adjustment, workflow, posting, and reversal leave balance",
        SortOrder = 3)]
    [Tags("Corporate / Human Resource / Leave Management / Leave Adjustment")]
    public class LeaveAdjustmentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.LeaveManagement";
        private readonly LeaveAdjustmentService _service;
        private readonly LoggerService _loggerService;

        public LeaveAdjustmentController(
            LeaveAdjustmentService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAdjustmentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Adjustment", Description = "Melihat metadata filter leave adjustment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAdjustment", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<LeaveAdjustmentFilterMetadataResponse>.Ok(
                _service.GetFilterMetadata(),
                "Metadata filter leave adjustment berhasil diambil."));
        }

        [HttpGet("reasons/options")]
        [ProducesResponseType(typeof(ApiResponse<List<LeaveAdjustmentReasonOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Adjustment", Description = "Melihat pilihan adjustment reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAdjustment", "Read")]
        public async Task<IActionResult> GetReasonOptions(
            [FromQuery] Guid? leaveTypeId,
            [FromQuery] string? adjustmentType,
            [FromQuery] string? direction,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            var data = await _service.GetReasonOptionsAsync(
                leaveTypeId,
                adjustmentType,
                direction,
                onlyActive,
                search,
                cancellationToken);
            return Ok(ApiResponse<List<LeaveAdjustmentReasonOptionResponse>>.Ok(
                data,
                "Pilihan adjustment reason berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAdjustmentSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Adjustment", Description = "Melihat ringkasan leave adjustment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAdjustment", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] LeaveAdjustmentQueryRequest request,
            CancellationToken cancellationToken)
        {
            var data = await _service.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<LeaveAdjustmentSummaryResponse>.Ok(
                data,
                "Ringkasan leave adjustment berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<LeaveAdjustmentPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Adjustment", Description = "Melihat daftar leave adjustment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAdjustment", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] LeaveAdjustmentQueryRequest request,
            CancellationToken cancellationToken)
        {
            var data = await _service.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<LeaveAdjustmentPagedResponse>.Ok(
                data,
                "Data leave adjustment berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAdjustmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Leave Adjustment", Description = "Melihat detail leave adjustment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAdjustment", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetByIdAsync(id, true, cancellationToken));
        }

        [HttpGet("{id:guid}/workflow")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Adjustment Workflow", Description = "Melihat workflow leave adjustment", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAdjustment", "Read")]
        public async Task<IActionResult> GetWorkflow(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetWorkflowAsync(id, cancellationToken));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LeaveAdjustmentActionResponse>), StatusCodes.Status201Created)]
        [AccessAction("Create", "Create Leave Adjustment", Description = "Membuat draft opening balance atau manual adjustment", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LeaveAdjustment", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateLeaveAdjustmentRequest request,
            CancellationToken cancellationToken)
        {
            var actor = GetCurrentUserId();
            var result = await _service.CreateAsync(request, actor, cancellationToken);
            if (result.Success)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "LeaveAdjustment.Create",
                    "Membuat draft leave adjustment.",
                    new { result.Data?.Adjustment.Id, result.Data?.Adjustment.AdjustmentNumber });
            }
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAdjustmentActionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Leave Adjustment", Description = "Mengubah draft leave adjustment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LeaveAdjustment", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateLeaveAdjustmentRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.UpdateAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken));
        }

        [HttpPost("{id:guid}/prepare-workflow")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAdjustmentActionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Prepare Leave Adjustment Workflow", Description = "Menyiapkan workflow dan attachment context leave adjustment", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeaveAdjustment", "Update")]
        public async Task<IActionResult> PrepareWorkflow(
            Guid id,
            [FromBody] PrepareLeaveAdjustmentWorkflowRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.PrepareWorkflowAsync(id, request, cancellationToken));
        }

        [HttpPost("{id:guid}/submit")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAdjustmentActionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Submit", "Submit Leave Adjustment", Description = "Submit adjustment ke workflow atau direct approval", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LeaveAdjustment", "Submit")]
        public async Task<IActionResult> Submit(
            Guid id,
            [FromBody] SubmitLeaveAdjustmentRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.SubmitAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken));
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAdjustmentActionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Cancel", "Cancel Leave Adjustment", Description = "Membatalkan atau menarik leave adjustment", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("LeaveAdjustment", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelLeaveAdjustmentRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.CancelAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken));
        }

        [HttpPost("{id:guid}/workflow/synchronize")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAdjustmentActionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Synchronize Leave Adjustment Workflow", Description = "Sinkronisasi status workflow dan retry auto-post", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("LeaveAdjustment", "Update")]
        public async Task<IActionResult> Synchronize(
            Guid id,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.SynchronizeAsync(
                id,
                GetCurrentUserId(),
                cancellationToken));
        }

        [HttpPost("{id:guid}/post")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAdjustmentActionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Post", "Post Leave Adjustment", Description = "Retry posting adjustment Approved ke balance ledger", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("LeaveAdjustment", "Post")]
        public async Task<IActionResult> Post(
            Guid id,
            [FromBody] PostLeaveAdjustmentRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.RetryPostAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken));
        }

        [HttpPost("{id:guid}/reverse")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAdjustmentActionResponse>), StatusCodes.Status201Created)]
        [AccessAction("Reverse", "Reverse Leave Adjustment", Description = "Membuat dan memposting reversal adjustment", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission("LeaveAdjustment", "Reverse")]
        public async Task<IActionResult> Reverse(
            Guid id,
            [FromBody] ReverseLeaveAdjustmentRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.ReverseAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Leave Adjustment", Description = "Soft delete draft leave adjustment", AccessType = AccessTypes.Delete, SortOrder = 10)]
        [AccessPermission("LeaveAdjustment", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.DeleteAsync(
                id,
                GetCurrentUserId(),
                cancellationToken));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private IActionResult ToActionResult<T>(LeaveAdjustmentServiceResult<T> result)
        {
            var response = result.Success
                ? ApiResponse<T>.Ok(result.Data, result.Message)
                : ApiResponse<T>.Fail(result.StatusCode, result.Message);
            return StatusCode(result.StatusCode, response);
        }
    }
}
