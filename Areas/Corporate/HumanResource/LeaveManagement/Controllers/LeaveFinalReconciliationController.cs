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
    [Route("api/v1/corporate/human-resource/leave/final-reconciliation")]
    [AccessController("HUMAN_RESOURCE_LEAVE", "Human Resource Leave", "Leave Final Reconciliation",
        AreaName = "Corporate", ControllerName = "LeaveFinalReconciliation",
        Description = "Final reconciliation across workflow, execution, attendance, balance, cancellation, recall, and payroll readiness", SortOrder = 11)]
    [Tags("Corporate / Human Resource / Leave Management / Final Reconciliation")]
    public class LeaveFinalReconciliationController : ControllerBase
    {
        private readonly LeaveFinalReconciliationService _service;
        public LeaveFinalReconciliationController(LeaveFinalReconciliationService service) => _service = service;

        [HttpGet("{leaveRequestId:guid}")]
        [AccessAction("Read", "Read Leave Final Reconciliation", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveFinalReconciliation", "Read")]
        public async Task<IActionResult> Get(Guid leaveRequestId, CancellationToken token)
            => ToActionResult(await _service.GetAsync(leaveRequestId, token));

        [HttpPost("{leaveRequestId:guid}/repair")]
        [AccessAction("Repair", "Repair Leave Final Reconciliation", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("LeaveFinalReconciliation", "Repair")]
        public async Task<IActionResult> Repair(Guid leaveRequestId, [FromBody] RepairLeaveFinalReconciliationRequest request, CancellationToken token)
            => ToActionResult(await _service.RepairAsync(leaveRequestId, request, GetUserId(), token));

        private Guid GetUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id"), out var id) ? id : Guid.Empty;
        private IActionResult ToActionResult<T>(LeaveRequestServiceResult<T> result)
            => StatusCode(result.StatusCode, result.Success ? ApiResponse<T>.Ok(result.Data, result.Message) : ApiResponse<T>.Fail(result.StatusCode, result.Message));
    }
}
