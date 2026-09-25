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
    /// <summary>Akses vaskular pasien HD — sub-proses ter-scope episode (<c>BE-HMD-08</c>).</summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/hemodialysis-episodes")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Vascular Access",
        AreaName = P.AreaName,
        ControllerName = P.VascularAccess.Resource,
        Description = "Akses vaskular pasien hemodialisa",
        SortOrder = 4
    )]
    [Tags("Health Services / Hemodialysis Management / Hemodialysis Episode")]
    public class HmdVascularAccessController : ControllerBase
    {
        private readonly HmdEpisodeService _service;
        private readonly LoggerService _logger;

        public HmdVascularAccessController(HmdEpisodeService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{id:guid}/vascular-accesses")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdVascularAccessResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Vascular Access", Description = "Melihat daftar akses vaskular pasien HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.VascularAccess.Resource, P.Read)]
        public async Task<IActionResult> GetList(
            Guid id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default) =>
            HmdHttp.Map(this, await _service.GetVascularAccessesAsync(id, pageNumber, pageSize, cancellationToken),
                "Daftar akses vaskular berhasil diambil.");

        [HttpPost("{id:guid}/vascular-accesses")]
        [ProducesResponseType(typeof(ApiResponse<HmdVascularAccessResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, "Create Hemodialysis Vascular Access", Description = "Mencatat akses vaskular baru", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.VascularAccess.Resource, P.Create)]
        public async Task<IActionResult> Create(Guid id, [FromBody] CreateHmdVascularAccessRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateVascularAccessAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Akses vaskular berhasil dicatat.", _logger, P.VascularAccess.Resource, P.Create,
                x => new { x.Id, x.EpisodeId, x.AccessType, x.AccessStatus, x.IsPrimary });

        [HttpPatch("vascular-accesses/{accessId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<HmdVascularAccessResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, "Update Hemodialysis Vascular Access", Description = "Mengubah kondisi akses vaskular", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.VascularAccess.Resource, P.Update)]
        public async Task<IActionResult> ChangeStatus(Guid accessId, [FromBody] ChangeHmdVascularAccessStatusRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.ChangeVascularAccessStatusAsync(accessId, request, HmdHttp.ActorId(User), cancellationToken),
                "Kondisi akses vaskular berhasil diperbarui.", _logger, P.VascularAccess.Resource, P.Update,
                x => new { x.Id, x.EpisodeId, x.AccessStatus, x.IsPrimary });
    }
}
