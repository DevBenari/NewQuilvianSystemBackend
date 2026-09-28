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
    /// Rujukan dan tinjauan hasil serologi — bagian paling sensitif modul ini (<c>BE-HMD-08</c>).
    /// </summary>
    /// <remarks>
    /// <b>Pembacaan ikut dicatat logger</b>, padahal <c>GET</c> biasanya tidak — pengecualian
    /// bernama pada <c>permission-audit-matrix.md</c> bagian 5. Status Hepatitis B, Hepatitis C,
    /// dan HIV adalah informasi kesehatan sensitif, sehingga siapa membacanya dan kapan harus
    /// tertelusur. Isi hasilnya sendiri tidak pernah masuk ke payload logger.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/hemodialysis-episodes")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Serology",
        AreaName = P.AreaName,
        ControllerName = P.Serology.Resource,
        Description = "Rujukan dan tinjauan hasil serologi pasien hemodialisa",
        SortOrder = 5
    )]
    [Tags("Health Services / Hemodialysis Management / Hemodialysis Episode")]
    public class HmdSerologyController : ControllerBase
    {
        private readonly HmdEpisodeService _service;
        private readonly LoggerService _logger;

        public HmdSerologyController(HmdEpisodeService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{id:guid}/serology-reviews")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdSerologyReviewResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Serology", Description = "Melihat status serologi pasien HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Serology.Resource, P.Read)]
        public async Task<IActionResult> GetList(
            Guid id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.GetSerologyReviewsAsync(id, pageNumber, pageSize, cancellationToken),
                "Riwayat tinjauan serologi berhasil diambil.", _logger, P.Serology.Resource, P.Read,
                x => new { EpisodeId = id, x.TotalData });

        [HttpPost("{id:guid}/serology-reviews")]
        [ProducesResponseType(typeof(ApiResponse<HmdSerologyReviewResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, "Create Hemodialysis Serology", Description = "Mencatat rujukan hasil serologi dan hasil tinjauannya", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Serology.Resource, P.Create)]
        public async Task<IActionResult> Create(Guid id, [FromBody] CreateHmdSerologyReviewRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateSerologyReviewAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Tinjauan serologi berhasil dicatat.", _logger, P.Serology.Resource, P.Create,
                x => new { x.Id, x.EpisodeId, x.TestType, x.ReviewStatus });
    }
}
