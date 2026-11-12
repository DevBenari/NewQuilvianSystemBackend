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
    [Route("api/corporate/human-resource/attendance/official-duties")]
    [Route("api/v1/corporate/human-resource/attendance-management/official-duties")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Official Duty Request Administration",
        AreaName = "Corporate",
        ControllerName = "OfficialDutyRequest",
        Description = "Surat perintah tugas dinas luar rumah sakit dan pengesahan status kehadiran OnDuty staf medis & penunjang",
        SortOrder = 27)]
    [Tags("HR Attendance - Official Duty")]
    public class OfficialDutyRequestController : ControllerBase
    {
        private readonly OfficialDutyRequestService _service;

        public OfficialDutyRequestController(OfficialDutyRequestService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        [AccessAction("Read", "Read Official Duty Requests", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OfficialDutyRequest", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] OfficialDutyQueryRequest query,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(query, cancellationToken);
            return Ok(ApiResponse<OfficialDutyPagedResponse>.Ok(result, "Data penugasan dinas luar berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Official Duty Request Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OfficialDutyRequest", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return ToActionResult(await _service.GetByIdAsync(id, cancellationToken));
        }

        [HttpPost]
        [AccessAction("Create", "Create Official Duty Request", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("OfficialDutyRequest", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateOfficialDutyRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.CreateAsync(request, userId, cancellationToken));
        }

        [HttpPut("{id:guid}/approve")]
        [HttpPost("{id:guid}/approve")]
        [AccessAction("Approve", "Approve Official Duty Request", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OfficialDutyRequest", "Approve")]
        public async Task<IActionResult> Approve(
            Guid id,
            [FromBody] ApproveOfficialDutyRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.ApproveAsync(id, request, userId, cancellationToken));
        }

        [HttpPut("{id:guid}/reject")]
        [HttpPost("{id:guid}/reject")]
        [AccessAction("Reject", "Reject Official Duty Request", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("OfficialDutyRequest", "Reject")]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] RejectOfficialDutyRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.RejectAsync(id, request, userId, cancellationToken));
        }

        private IActionResult ToActionResult<T>(OfficialDutyRequestServiceResult<T> result)
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
