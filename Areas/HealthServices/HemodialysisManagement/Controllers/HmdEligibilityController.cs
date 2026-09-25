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
    /// Penilaian kelayakan HD — sub-proses ter-scope episode (<c>BE-HMD-08</c>). Sistem tidak
    /// pernah menentukan kelayakan sendiri; setiap baris lahir dari keputusan dokter.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/hemodialysis-episodes")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Eligibility",
        AreaName = P.AreaName,
        ControllerName = P.Eligibility.Resource,
        Description = "Penilaian kelayakan hemodialisa oleh dokter",
        SortOrder = 3
    )]
    [Tags("Health Services / Hemodialysis Management / Hemodialysis Episode")]
    public class HmdEligibilityController : ControllerBase
    {
        private readonly HmdEpisodeService _service;
        private readonly LoggerService _logger;

        public HmdEligibilityController(HmdEpisodeService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{id:guid}/eligibility-assessments")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdEligibilityResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Eligibility", Description = "Melihat riwayat penilaian kelayakan HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Eligibility.Resource, P.Read)]
        public async Task<IActionResult> GetList(
            Guid id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default) =>
            HmdHttp.Map(this, await _service.GetEligibilityAsync(id, pageNumber, pageSize, cancellationToken),
                "Riwayat penilaian kelayakan berhasil diambil.");

        [HttpPost("{id:guid}/eligibility-assessments")]
        [ProducesResponseType(typeof(ApiResponse<HmdEligibilityResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Eligibility.Decide, "Decide Hemodialysis Eligibility", Description = "Dokter mencatat keputusan kelayakan HD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Eligibility.Resource, P.Eligibility.Decide)]
        public async Task<IActionResult> Create(Guid id, [FromBody] CreateHmdEligibilityRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateEligibilityAsync(id, request, HmdHttp.Actor(this), cancellationToken),
                "Keputusan kelayakan berhasil dicatat.", _logger, P.Eligibility.Resource, P.Eligibility.Decide,
                x => new { x.Id, x.EpisodeId, x.Outcome });
    }
}
