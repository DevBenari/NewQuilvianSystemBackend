using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/leave/return-to-work")]
    [AccessController("HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE", "Human Resource Employee Self Service", "My Return To Work",
        AreaName = "SelfServices", ControllerName = "MyReturnToWork",
        Description = "Employee acknowledgement of recall and actual return-to-work date", SortOrder = 10)]
    [Tags("Self Services / Human Resource / Return To Work")]
    public class LeaveReturnToWorkController : ControllerBase
    {
        private readonly LeaveRecallService _service;
        public LeaveReturnToWorkController(LeaveRecallService service) => _service = service;

        [HttpGet]
        [AccessAction("Read", "Read My Return To Work", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyReturnToWork", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] LeaveLifecycleQueryRequest request, CancellationToken token)
            => ToActionResult(await _service.GetMyReturnToWorkAsync(GetUserId(), request, token));

        [HttpPost("{recallId:guid}/acknowledge")]
        [AccessAction("Acknowledge", "Acknowledge Return To Work", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("MyReturnToWork", "Acknowledge")]
        public async Task<IActionResult> Acknowledge(Guid recallId, [FromBody] AcknowledgeReturnToWorkRequest request, CancellationToken token)
            => ToActionResult(await _service.AcknowledgeReturnToWorkAsync(recallId, GetUserId(), request, token));

        private Guid GetUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id"), out var id) ? id : Guid.Empty;
        private IActionResult ToActionResult<T>(LeaveRequestServiceResult<T> result)
            => StatusCode(result.StatusCode, result.Success ? ApiResponse<T>.Ok(result.Data, result.Message) : ApiResponse<T>.Fail(result.StatusCode, result.Message));
    }
}
