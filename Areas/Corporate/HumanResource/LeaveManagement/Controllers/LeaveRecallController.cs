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
    [Route("api/v1/corporate/human-resource/leave/recalls")]
    [AccessController("HUMAN_RESOURCE_LEAVE", "Human Resource Leave", "Leave Recall",
        AreaName = "Corporate", ControllerName = "LeaveRecall",
        Description = "Recall employee dari leave dan return-to-work lifecycle", SortOrder = 9)]
    [Tags("Corporate / Human Resource / Leave Management / Leave Recall")]
    public class LeaveRecallController : ControllerBase
    {
        private readonly LeaveRecallService _service;
        public LeaveRecallController(LeaveRecallService service) => _service = service;

        [HttpGet]
        [AccessAction("Read", "Read Leave Recall", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveRecall", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] LeaveLifecycleQueryRequest request, CancellationToken token)
            => ToActionResult(await _service.GetPagedAsync(request, token));

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Leave Recall Detail", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveRecall", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken token)
            => ToActionResult(await _service.GetByIdAsync(id, token));

        [HttpPost]
        [AccessAction("Create", "Create Leave Recall", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LeaveRecall", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateLeaveRecallRequest request, CancellationToken token)
            => ToActionResult(await _service.CreateAsync(GetUserId(), request, token));

        [HttpPost("{id:guid}/prepare-workflow")]
        [AccessAction("Update", "Prepare Leave Recall Workflow", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LeaveRecall", "Update")]
        public async Task<IActionResult> PrepareWorkflow(Guid id, [FromBody] PrepareLeaveLifecycleWorkflowRequest request, CancellationToken token)
            => ToActionResult(await _service.PrepareWorkflowAsync(id, GetUserId(), request, token));

        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Submit Leave Recall", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeaveRecall", "Submit")]
        public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitLeaveLifecycleWorkflowRequest request, CancellationToken token)
            => ToActionResult(await _service.SubmitAsync(id, GetUserId(), request, token));

        [HttpPost("{id:guid}/synchronize")]
        [AccessAction("Synchronize", "Synchronize Leave Recall", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LeaveRecall", "Synchronize")]
        public async Task<IActionResult> Synchronize(Guid id, CancellationToken token)
            => ToActionResult(await _service.SynchronizeAsync(id, GetUserId(), true, token));

        [HttpPost("{id:guid}/apply")]
        [AccessAction("Apply", "Apply Leave Recall", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("LeaveRecall", "Apply")]
        public async Task<IActionResult> Apply(Guid id, CancellationToken token)
            => ToActionResult(await _service.SynchronizeAsync(id, GetUserId(), true, token));

        private Guid GetUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id"), out var id) ? id : Guid.Empty;
        private IActionResult ToActionResult<T>(LeaveRequestServiceResult<T> result)
            => StatusCode(result.StatusCode, result.Success ? ApiResponse<T>.Ok(result.Data, result.Message) : ApiResponse<T>.Fail(result.StatusCode, result.Message));
    }
}
