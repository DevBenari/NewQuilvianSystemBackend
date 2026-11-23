using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    /// <summary>
    /// Dashboard SDM baca-saja untuk Manajer SDM dan Admin HRD (Paket B, kontrak v30 bagian 36.1, <c>HRD-DEC-243</c>).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-dashboard")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Dashboard SDM",
        AreaName = "Corporate",
        ControllerName = "HrdWorkforceDashboard",
        Description = "Ringkasan tenaga kerja rumah sakit beserta daftar nama di balik setiap angka",
        SortOrder = 23)]
    [Tags("Corporate / Human Resource / Workforce Core / Workforce Dashboard")]
    public class HrdWorkforceDashboardController : ControllerBase
    {
        private readonly IHrdWorkforceDashboardService _service;

        public HrdWorkforceDashboardController(IHrdWorkforceDashboardService service)
        {
            _service = service;
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceDashboardSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction(
            "Read",
            "Melihat Dashboard SDM",
            Description = "Melihat lima panel Dashboard SDM beserta daftar nama di balik setiap angka",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("HrdWorkforceDashboard", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var result = await _service.GetSummaryAsync(cancellationToken);
            return Ok(ApiResponse<WorkforceDashboardSummaryResponse>.Ok(result, "Dashboard SDM berhasil diambil."));
        }

        [HttpGet("drilldown")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WorkforceDashboardDrilldownItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction(
            "Read",
            "Melihat Dashboard SDM",
            Description = "Melihat lima panel Dashboard SDM beserta daftar nama di balik setiap angka",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("HrdWorkforceDashboard", "Read")]
        public async Task<IActionResult> GetDrilldown(
            [FromQuery] WorkforceDashboardDrilldownQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetDrilldownAsync(query, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.Message ?? "Permintaan tidak valid."));
            }

            return Ok(ApiResponse<PagedResult<WorkforceDashboardDrilldownItem>>.Ok(result.Data!, "Daftar nama berhasil diambil."));
        }
    }
}
