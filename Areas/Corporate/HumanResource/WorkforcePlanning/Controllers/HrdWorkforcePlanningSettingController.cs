using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Controllers
{
    /// <summary>
    /// Controller pengaturan kebijakan perencanaan tenaga kerja per rumah sakit.
    /// Memenuhi kontrak API v7 Bagian 13 grup Setting dan izin WorkforcePlanningSetting : Read / Update (BE-HRD-031).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-planning/settings")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_PLANNING",
        moduleName: "Human Resource Workforce Planning",
        displayName: "Workforce Planning Setting",
        AreaName = "Corporate",
        ControllerName = "WorkforcePlanningSetting",
        Description = "Pengaturan kebijakan perencanaan tenaga kerja rumah sakit",
        SortOrder = 30)]
    [Tags("Corporate / Human Resource / Workforce Planning / Setting")]
    public sealed class HrdWorkforcePlanningSettingController : ControllerBase
    {
        private readonly IHrdWorkforcePlanningSettingService _service;

        public HrdWorkforcePlanningSettingController(IHrdWorkforcePlanningSettingService service)
        {
            _service = service;
        }

        /// <summary>
        /// Mengambil pengaturan satu rumah sakit (BE-HRD-031 kriteria 1).
        /// </summary>
        /// <param name="hospitalSiteId">ID unik rumah sakit (MstHospitalSite)</param>
        /// <param name="cancellationToken">Token pembatalan request</param>
        [HttpGet]
        [AccessAction("Read", "Read Workforce Planning Setting", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforcePlanningSetting", "Read")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePlanningSettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromQuery] Guid hospitalSiteId, CancellationToken cancellationToken)
        {
            var result = await _service.GetByHospitalSiteIdAsync(hospitalSiteId, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Mengubah pengaturan satu rumah sakit (BE-HRD-031 kriteria 2-6).
        /// </summary>
        /// <param name="request">Payload pembaruan pengaturan</param>
        /// <param name="cancellationToken">Token pembatalan request</param>
        [HttpPut]
        [AccessAction("Update", "Update Workforce Planning Setting", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("WorkforcePlanningSetting", "Update")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePlanningSettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update([FromBody] UpdateWorkforcePlanningSettingRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(request, GetCurrentUserId(), cancellationToken);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(WorkforcePlanningResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
