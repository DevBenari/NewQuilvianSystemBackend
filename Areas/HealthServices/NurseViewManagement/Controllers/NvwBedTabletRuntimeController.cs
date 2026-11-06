using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Controllers
{
    [ApiController]
    [Authorize(Policy = AuthorizationPolicies.NurseViewBedTabletAccess)]
    [Route("api/v1/health-services/nurse-view-management/bed-tablet-runtime")]
    [Tags("Health Services / Nurse View Management / Bed Tablet Runtime")]
    public class NvwBedTabletRuntimeController : ControllerBase
    {
        private readonly NvwBedTabletContextService _tabletContext;

        public NvwBedTabletRuntimeController(NvwBedTabletContextService tabletContext)
        {
            _tabletContext = tabletContext;
        }

        [HttpGet("current")]
        [Authorize(Policy = AuthorizationPolicies.NurseViewBedTabletAccess)]
        [ProducesResponseType(typeof(ApiResponse<NvwBedTabletRuntimeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken = default) =>
            NvwHttp.Map(
                this,
                await _tabletContext.GetCurrentAsync(
                    NvwHttp.ActorId(User),
                    User.FindFirstValue(NvwBedTabletContextService.SessionStampClaim),
                    cancellationToken),
                "Keadaan tablet bed berhasil diambil.");
    }
}
