using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants.RehabilitationPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Controllers
{
    /// <summary>
    /// Tujuan terapi jangka pendek dan panjang — <c>BE-RHB-011</c>, <c>contracts/api-contract.md</c> v1 grup
    /// Rehab Goal, <c>state-transition-matrix.md</c> §8.
    /// </summary>
    /// <remarks>
    /// Arketipe <b>sub-proses ter-scope induk</b> (episode). Perubahan status lewat <c>POST /{id}/revise</c> dan
    /// <c>POST /{id}/discontinue</c>; tidak ada <c>DELETE</c>. Logger tanpa isi tujuan.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/rehabilitation-management/rehab-goals")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Rehab Goal",
        AreaName = P.AreaName,
        ControllerName = P.Goal.Resource,
        Description = "Tujuan terapi rehab jangka pendek dan panjang beserta revisinya",
        SortOrder = 7
    )]
    [Tags("Health Services / Rehabilitation Management / Rehab Goal")]
    public class RehabGoalController : ControllerBase
    {
        private const string Resource = P.Goal.Resource;
        private const string ReadName = "Read Rehab Goal";
        private const string CreateName = "Create Rehab Goal";
        private const string UpdateName = "Update Rehab Goal";

        private readonly RhbGoalService _service;
        private readonly LoggerService _logger;

        public RehabGoalController(RhbGoalService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("by-episode/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabGoalItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = "Melihat tujuan terapi rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetByEpisode(Guid episodeId, [FromQuery] RehabGoalQuery query, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.ListByEpisodeAsync(episodeId, query, RhbHttp.ActorId(User), cancellationToken), "Daftar tujuan terapi berhasil diambil.");

        [HttpPost("by-episode/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabGoalItem>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, CreateName, Description = "Membuat tujuan terapi rehab", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(Resource, P.Create)]
        public async Task<IActionResult> Create(Guid episodeId, [FromBody] SaveRehabGoalRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.CreateAsync(episodeId, request, RhbHttp.ActorId(User), cancellationToken),
                "Tujuan terapi berhasil dibuat.", _logger, Resource, P.Create, Trace);

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabGoalItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = "Mengubah, merevisi, dan menghentikan tujuan terapi rehab", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SaveRehabGoalRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.UpdateAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Tujuan terapi berhasil diubah.", _logger, Resource, P.Update, Trace);

        [HttpPost("{id:guid}/revise")]
        [ProducesResponseType(typeof(ApiResponse<RehabGoalItem>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = "Mengubah, merevisi, dan menghentikan tujuan terapi rehab", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> Revise(Guid id, [FromBody] ReviseRehabGoalRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.ReviseAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Versi baru tujuan terapi berhasil dibuat.", _logger, Resource, "Revise", Trace);

        [HttpPost("{id:guid}/discontinue")]
        [ProducesResponseType(typeof(ApiResponse<RehabGoalItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = "Mengubah, merevisi, dan menghentikan tujuan terapi rehab", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> Discontinue(Guid id, [FromBody] RehabReasonRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.DiscontinueAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Tujuan terapi dihentikan.", _logger, Resource, "Discontinue", Trace);

        private static object Trace(RehabGoalItem x) => new { x.Id, x.EpisodeId, Status = x.GoalStatus.ToString(), x.SupersedesGoalId, x.Version };
    }
}
