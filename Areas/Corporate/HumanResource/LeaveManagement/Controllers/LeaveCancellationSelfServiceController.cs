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
    [Route("api/v1/corporate/human-resource/employee-self-service/leave/cancellations")]
    [AccessController("HUMAN_RESOURCE_LEAVE", "Human Resource Leave", "Leave Cancellation Self Service",
        AreaName = "Corporate", ControllerName = "LeaveCancellationSelfService",
        Description = "Employee self-service cancellation setelah leave disetujui", SortOrder = 7)]
    [Tags("Corporate / Human Resource / Employee Self Service / Leave Cancellation")]
    public class LeaveCancellationSelfServiceController : ControllerBase
    {
        private readonly LeaveCancellationService _service;
        public LeaveCancellationSelfServiceController(LeaveCancellationService service) => _service = service;

        [HttpGet]
        [AccessAction("Read", "Read My Leave Cancellation", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveCancellationSelfService", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] LeaveLifecycleQueryRequest request, CancellationToken token)
            => ToActionResult(await _service.GetMyPagedAsync(GetUserId(), request, token));

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read My Leave Cancellation Detail", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveCancellationSelfService", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken token)
        {
            var actor = await HttpContext.RequestServices.GetRequiredService<LeaveRequestCalculationService>()
                .GetActorContextAsync(GetUserId(), token);
            return actor.Success && actor.Data != null
                ? ToActionResult(await _service.GetByIdAsync(id, actor.Data.WorkforceProfileId, token))
                : StatusCode(actor.StatusCode, ApiResponse<object>.Fail(actor.StatusCode, actor.Message));
        }

        [HttpPost]
        [AccessAction("Create", "Create Leave Cancellation", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LeaveCancellationSelfService", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateLeaveCancellationRequest request, CancellationToken token)
            => ToActionResult(await _service.CreateAsync(GetUserId(), request, token));

        [HttpPost("{id:guid}/prepare-workflow")]
        [AccessAction("Update", "Prepare Leave Cancellation Workflow", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LeaveCancellationSelfService", "Update")]
        public async Task<IActionResult> PrepareWorkflow(Guid id, [FromBody] PrepareLeaveLifecycleWorkflowRequest request, CancellationToken token)
            => ToActionResult(await _service.PrepareWorkflowAsync(id, GetUserId(), request, token));

        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Submit Leave Cancellation", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeaveCancellationSelfService", "Submit")]
        public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitLeaveLifecycleWorkflowRequest request, CancellationToken token)
            => ToActionResult(await _service.SubmitAsync(id, GetUserId(), request, token));

        private Guid GetUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id"), out var id) ? id : Guid.Empty;
        private IActionResult ToActionResult<T>(LeaveRequestServiceResult<T> result)
            => StatusCode(result.StatusCode, result.Success ? ApiResponse<T>.Ok(result.Data, result.Message) : ApiResponse<T>.Fail(result.StatusCode, result.Message));
    }
}
