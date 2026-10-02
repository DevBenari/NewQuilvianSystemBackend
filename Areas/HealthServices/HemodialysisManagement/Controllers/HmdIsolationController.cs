using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Constants.HemodialysisPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Controllers
{
    /// <summary>
    /// Keputusan kebutuhan isolasi pasien HD oleh tim PPI atau dokter dialisis (<c>BE-HMD-08</c>).
    /// Keputusan yang berlaku dibaca gerbang penjadwalan (<c>HMD-VAL-035</c>).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/hemodialysis-episodes")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Isolation",
        AreaName = P.AreaName,
        ControllerName = P.Isolation.Resource,
        Description = "Keputusan kebutuhan isolasi pasien hemodialisa",
        SortOrder = 6
    )]
    [Tags("Health Services / Hemodialysis Management / Hemodialysis Episode")]
    public class HmdIsolationController : ControllerBase
    {
        private readonly HmdEpisodeService _service;
        private readonly LoggerService _logger;

        public HmdIsolationController(HmdEpisodeService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{id:guid}/isolation-decisions")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdIsolationDecisionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Isolation", Description = "Melihat riwayat keputusan isolasi pasien HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Isolation.Resource, P.Read)]
        public async Task<IActionResult> GetList(
            Guid id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default) =>
            HmdHttp.Map(this, await _service.GetIsolationDecisionsAsync(id, pageNumber, pageSize, cancellationToken),
                "Riwayat keputusan isolasi berhasil diambil.");

        [HttpPost("{id:guid}/isolation-decisions")]
        [ProducesResponseType(typeof(ApiResponse<HmdIsolationDecisionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Isolation.Decide, "Decide Hemodialysis Isolation", Description = "Menetapkan kebutuhan isolasi pasien HD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Isolation.Resource, P.Isolation.Decide)]
        public async Task<IActionResult> Create(Guid id, [FromBody] CreateHmdIsolationDecisionRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateIsolationDecisionAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Keputusan isolasi berhasil ditetapkan.", _logger, P.Isolation.Resource, P.Isolation.Decide,
                x => new { x.Id, x.EpisodeId, x.IsActive });
    }
}
