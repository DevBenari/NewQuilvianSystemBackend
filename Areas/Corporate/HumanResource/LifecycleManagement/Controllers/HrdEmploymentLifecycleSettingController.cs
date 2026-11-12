using System;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/corporate/human-resource/lifecycle-settings")]
    [Produces(MediaTypeNames.Application.Json)]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LIFECYCLE_MANAGEMENT",
        moduleName: "Human Resource Lifecycle Management",
        displayName: "Pengaturan Lifecycle Pegawai",
        AreaName = "Corporate",
        ControllerName = "WorkforceLifecycleSetting",
        Description = "Parameter lifecycle pegawai: masa percobaan, kontrak, dan onboarding",
        SortOrder = 37)]
    [Tags("Corporate / Human Resource / Lifecycle Management / Settings")]
    public class HrdEmploymentLifecycleSettingController : ControllerBase
    {
        private readonly IHrdEmploymentLifecycleSettingService _settingService;

        public HrdEmploymentLifecycleSettingController(IHrdEmploymentLifecycleSettingService settingService)
        {
            _settingService = settingService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<HrdEmploymentLifecycleSettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Lifecycle Settings", Description = "Melihat pengaturan siklus kepegawaian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceLifecycleSetting", "Read")]
        public async Task<IActionResult> GetSettings(
            [FromQuery] Guid hospitalSiteId,
            CancellationToken cancellationToken)
        {
            if (hospitalSiteId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "hospitalSiteId wajib disertakan."));
            }

            var result = await _settingService.GetByHospitalSiteIdAsync(hospitalSiteId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<HrdEmploymentLifecycleSettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Lifecycle Settings", Description = "Memperbarui pengaturan siklus kepegawaian", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("WorkforceLifecycleSetting", "Update")]
        public async Task<IActionResult> UpdateSettings(
            [FromQuery] Guid hospitalSiteId,
            [FromBody] UpdateEmploymentLifecycleSettingRequest request,
            CancellationToken cancellationToken)
        {
            if (hospitalSiteId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "hospitalSiteId wajib disertakan."));
            }

            var userId = GetUserId(User);
            var result = await _settingService.UpdateAsync(hospitalSiteId, request, userId, cancellationToken);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(RecruitmentResult<T> result)
        {
            if (result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message));
            }

            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message, result.Errors));
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }
    }
}
