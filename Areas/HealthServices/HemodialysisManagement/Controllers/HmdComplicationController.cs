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
    /// Komplikasi intradialisis beserta penanganannya (<c>BE-HMD-14</c>). Sistem tidak pernah
    /// menyimpulkan komplikasi dari nilai tanda vital; setiap baris dicatat petugas secara sadar.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/hemodialysis-sessions")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Complication",
        AreaName = P.AreaName,
        ControllerName = P.Complication.Resource,
        Description = "Komplikasi selama sesi hemodialisa",
        SortOrder = 12
    )]
    [Tags("Health Services / Hemodialysis Management / Hemodialysis Session")]
    public class HmdComplicationController : ControllerBase
    {
        private readonly HmdSessionService _service;
        private readonly LoggerService _logger;

        public HmdComplicationController(HmdSessionService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{id:guid}/complications")]
        [ProducesResponseType(typeof(ApiResponse<List<HmdComplicationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Complication", Description = "Melihat komplikasi pada satu sesi HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Complication.Resource, P.Read)]
        public async Task<IActionResult> GetList(Guid id, CancellationToken cancellationToken = default) =>
            HmdHttp.Map(this, await _service.GetComplicationsAsync(id, cancellationToken), "Daftar komplikasi berhasil diambil.");

        [HttpPost("{id:guid}/complications")]
        [ProducesResponseType(typeof(ApiResponse<HmdComplicationResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Create, "Create Hemodialysis Complication", Description = "Mencatat komplikasi beserta penanganannya", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Complication.Resource, P.Create)]
        public async Task<IActionResult> Create(Guid id, [FromBody] CreateHmdComplicationRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateComplicationAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Komplikasi berhasil dicatat.", _logger, P.Complication.Resource, P.Create,
                x => new { x.Id, x.SessionId, x.ComplicationType, x.Outcome });
    }
}
