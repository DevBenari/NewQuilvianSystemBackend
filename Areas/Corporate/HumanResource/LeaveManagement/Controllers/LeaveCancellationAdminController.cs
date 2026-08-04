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
    [Route("api/v1/corporate/human-resource/leave/cancellations")]
    [AccessController("HUMAN_RESOURCE_LEAVE", "Human Resource Leave", "Leave Cancellation",
        AreaName = "Corporate", ControllerName = "LeaveCancellation",
        Description = "Monitoring, synchronization, and apply approved leave cancellation", SortOrder = 8)]
    [Tags("Corporate / Human Resource / Leave Management / Leave Cancellation")]
    public class LeaveCancellationAdminController : ControllerBase
    {
        private readonly LeaveCancellationService _service;
        public LeaveCancellationAdminController(LeaveCancellationService service) => _service = service;

        [HttpGet]
        [AccessAction("Read", "Read Leave Cancellation", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveCancellation", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] LeaveLifecycleQueryRequest request, CancellationToken token)
            => ToActionResult(await _service.GetPagedAsync(request, token));

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Leave Cancellation Detail", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveCancellation", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken token)
            => ToActionResult(await _service.GetByIdAsync(id, null, token));

        [HttpPost("{id:guid}/synchronize")]
        [AccessAction("Synchronize", "Synchronize Leave Cancellation", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("LeaveCancellation", "Synchronize")]
        public async Task<IActionResult> Synchronize(Guid id, CancellationToken token)
            => ToActionResult(await _service.SynchronizeAsync(id, GetUserId(), true, token));

        [HttpPost("{id:guid}/apply")]
        [AccessAction("Apply", "Apply Leave Cancellation", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LeaveCancellation", "Apply")]
        public async Task<IActionResult> Apply(Guid id, CancellationToken token)
            => ToActionResult(await _service.SynchronizeAsync(id, GetUserId(), true, token));

        private Guid GetUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id"), out var id) ? id : Guid.Empty;
        private IActionResult ToActionResult<T>(LeaveRequestServiceResult<T> result)
            => StatusCode(result.StatusCode, result.Success ? ApiResponse<T>.Ok(result.Data, result.Message) : ApiResponse<T>.Fail(result.StatusCode, result.Message));
    }
}
