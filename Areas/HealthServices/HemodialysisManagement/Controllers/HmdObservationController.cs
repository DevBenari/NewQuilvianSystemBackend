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
    /// Pemantauan berkala selama sesi berlangsung (<c>BE-HMD-14</c>). Setiap pengamatan adalah baris
    /// baru; riwayat disajikan urut waktu menaik dan tidak pernah saling menimpa.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/hemodialysis-sessions")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Observation",
        AreaName = P.AreaName,
        ControllerName = P.Observation.Resource,
        Description = "Pemantauan berkala sesi hemodialisa",
        SortOrder = 10
    )]
    [Tags("Health Services / Hemodialysis Management / Hemodialysis Session")]
    public class HmdObservationController : ControllerBase
    {
        private readonly HmdSessionService _service;
        private readonly LoggerService _logger;

        public HmdObservationController(HmdSessionService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{id:guid}/observations")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdObservationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Observation", Description = "Melihat seluruh riwayat pemantauan pada satu sesi HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Observation.Resource, P.Read)]
        public async Task<IActionResult> GetList(Guid id, [FromQuery] HmdObservationQuery query, CancellationToken cancellationToken = default) =>
            HmdHttp.Map(this, await _service.GetObservationsAsync(id, query, cancellationToken), "Riwayat pemantauan berhasil diambil.");

        [HttpPost("{id:guid}/observations")]
        [ProducesResponseType(typeof(ApiResponse<HmdObservationResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Create, "Create Hemodialysis Observation", Description = "Mencatat satu baris pemantauan sesi HD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Observation.Resource, P.Create)]
        public async Task<IActionResult> Create(Guid id, [FromBody] CreateHmdObservationRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateObservationAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Pemantauan berhasil dicatat.", _logger, P.Observation.Resource, P.Create,
                x => new { x.Id, x.SessionId, x.SequenceNumber, x.ObservedAt });
    }
}
