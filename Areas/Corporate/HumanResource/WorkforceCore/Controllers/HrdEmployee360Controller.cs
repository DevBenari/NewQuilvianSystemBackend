using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/employee-360")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Employee 360 Profile",
        AreaName = "Corporate",
        ControllerName = "HrdEmployee360",
        Description = "Unified Employee 360 View for HR and Direct Managers",
        SortOrder = 22)]
    [Tags("Corporate / Human Resource / Workforce Core / Employee 360")]
    public class HrdEmployee360Controller : ControllerBase
    {
        private readonly IHrdEmployee360Service _service;

        public HrdEmployee360Controller(IHrdEmployee360Service service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Employee360Response>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Employee 360",
            Description = "Melihat profil 360 derajat pegawai (mode HR atau Atasan Langsung)",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("HrdEmployee360", "Read")]
        public async Task<IActionResult> GetEmployee360(Guid workforceProfileId, CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var callerUserId))
            {
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Pengguna tidak terautentikasi."));
            }

            var result = await _service.GetEmployee360Async(workforceProfileId, User, callerUserId, ct);

            if (result.IsForbidden)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, result.Message ?? "Akses ditolak."));
            }

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message ?? "Profil pegawai tidak ditemukan."));
            }

            return Ok(ApiResponse<Employee360Response>.Ok(result.Data!, "Profil 360 derajat pegawai berhasil diambil."));
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            var value = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out userId) && userId != Guid.Empty;
        }
    }
}
