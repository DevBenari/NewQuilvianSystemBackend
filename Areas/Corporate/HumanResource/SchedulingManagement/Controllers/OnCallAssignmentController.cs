using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/scheduling-management/on-call-assignments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_SCHEDULING_MANAGEMENT",
        moduleName: "Human Resource Scheduling Management",
        displayName: "On-Call Assignment Administration",
        AreaName = "Corporate",
        ControllerName = "OnCallAssignment",
        Description = "Penjadwalan staf siaga on-call 24/7 dan aktivasi pemanggilan tugas darurat",
        SortOrder = 24)]
    [Tags("Corporate / Human Resource / Scheduling Management / On-Call Assignments")]
    public class OnCallAssignmentController : ControllerBase
    {
        private readonly OnCallAssignmentService _service;

        public OnCallAssignmentController(OnCallAssignmentService service)
        {
            _service = service;
        }

        [HttpGet]
        [AccessAction("Read", "Read On-Call Assignments", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnCallAssignment", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] Guid? rosterPeriodId = null,
            [FromQuery] Guid? workforceProfileId = null,
            [FromQuery] string? status = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(
                pageNumber,
                pageSize,
                rosterPeriodId,
                workforceProfileId,
                status,
                cancellationToken);

            return Ok(ApiResponse<PagedResult<OnCallAssignmentResponse>>.Ok(
                result,
                "Daftar penugasan siaga (on-call) berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create On-Call Assignment", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("OnCallAssignment", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateOnCallAssignmentRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.CreateAsync(request, userId, cancellationToken));
        }

        [HttpPost("{id:guid}/activate")]
        [AccessAction("Update", "Activate On-Call Assignment", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OnCallAssignment", "Update")]
        public async Task<IActionResult> Activate(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.ActivateAsync(id, userId, cancellationToken));
        }

        private IActionResult ToActionResult<T>(SchedulingRequestServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private bool TryGetUserId(out Guid userId)
        {
            var value = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out userId) && userId != Guid.Empty;
        }

        private IActionResult UnauthorizedResponse()
        {
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Identitas user login tidak valid."));
        }
    }
}
