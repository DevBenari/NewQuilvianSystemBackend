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
    [Route("api/v1/corporate/human-resource/scheduling-management/shift-replacements")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_SCHEDULING_MANAGEMENT",
        moduleName: "Human Resource Scheduling Management",
        displayName: "Shift Replacement Administration",
        AreaName = "Corporate",
        ControllerName = "ShiftReplacement",
        Description = "Pencatatan dan persetujuan penggantian shift mendadak staf rumah sakit",
        SortOrder = 23)]
    [Tags("Corporate / Human Resource / Scheduling Management / Shift Replacements")]
    public class ShiftReplacementController : ControllerBase
    {
        private readonly ShiftReplacementService _service;

        public ShiftReplacementController(ShiftReplacementService service)
        {
            _service = service;
        }

        [HttpGet]
        [AccessAction("Read", "Read Shift Replacements", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ShiftReplacement", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] Guid? shiftAssignmentId = null,
            [FromQuery] string? status = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(
                pageNumber,
                pageSize,
                shiftAssignmentId,
                status,
                cancellationToken);

            return Ok(ApiResponse<PagedResult<ShiftReplacementResponse>>.Ok(
                result,
                "Daftar penggantian shift berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Shift Replacement", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ShiftReplacement", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateShiftReplacementRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.CreateReplacementAsync(request, userId, cancellationToken));
        }

        [HttpPost("{id:guid}/approve")]
        [AccessAction("Update", "Approve Shift Replacement", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ShiftReplacement", "Update")]
        public async Task<IActionResult> Approve(
            Guid id,
            [FromBody] string? notes,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.ApproveReplacementAsync(id, userId, notes, cancellationToken));
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
