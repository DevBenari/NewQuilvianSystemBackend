using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/corporate/human-resource/attendance/early-leave-permissions")]
    [Route("api/v1/corporate/human-resource/attendance-management/early-leave-permissions")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Early Leave Permission Administration",
        AreaName = "Corporate",
        ControllerName = "EarlyLeavePermission",
        Description = "Manajemen permohonan izin pulang cepat non-deductible staf rumah sakit",
        SortOrder = 26)]
    [Tags("HR Attendance - Early Leave Permissions")]
    public class EarlyLeavePermissionController : ControllerBase
    {
        private readonly EarlyLeavePermissionService _service;

        public EarlyLeavePermissionController(EarlyLeavePermissionService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        [AccessAction("Read", "Read Early Leave Permissions", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EarlyLeavePermission", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] EarlyLeavePermissionQueryRequest query,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(query, cancellationToken);
            return Ok(ApiResponse<EarlyLeavePermissionPagedResponse>.Ok(result, "Data izin pulang cepat berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Early Leave Permission Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("EarlyLeavePermission", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return ToActionResult(await _service.GetByIdAsync(id, cancellationToken));
        }

        [HttpPost]
        [AccessAction("Create", "Create Early Leave Permission", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("EarlyLeavePermission", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateEarlyLeavePermissionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.CreateAsync(request, userId, cancellationToken));
        }

        [HttpPut("{id:guid}/approve")]
        [HttpPost("{id:guid}/approve")]
        [AccessAction("Approve", "Approve Early Leave Permission", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EarlyLeavePermission", "Approve")]
        public async Task<IActionResult> Approve(
            Guid id,
            [FromBody] ApproveEarlyLeavePermissionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.ApproveAsync(id, request, userId, cancellationToken));
        }

        [HttpPut("{id:guid}/reject")]
        [HttpPost("{id:guid}/reject")]
        [AccessAction("Reject", "Reject Early Leave Permission", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("EarlyLeavePermission", "Reject")]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] RejectEarlyLeavePermissionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.RejectAsync(id, request, userId, cancellationToken));
        }

        private IActionResult ToActionResult<T>(EarlyLeavePermissionServiceResult<T> result)
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
