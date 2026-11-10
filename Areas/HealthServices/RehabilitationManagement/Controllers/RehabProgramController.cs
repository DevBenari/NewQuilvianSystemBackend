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
    /// Program terapi berversi per layanan — <c>BE-RHB-011</c>, <c>contracts/api-contract.md</c> v1 grup
    /// Rehab Program, <c>state-transition-matrix.md</c> §4.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Arketipe <b>aggregate ber-lifecycle ter-scope episode</b>: <c>revise</c> (butir <c>Create</c>),
    /// <c>activate</c> (butir <c>Activate</c> = tanda tangan dokumen RM jenis 48), <c>stop</c> (butir
    /// <c>Stop</c>), <c>cancel</c> draft (butir <c>Update</c>). Tidak ada <c>DELETE</c>.
    /// </para>
    /// <para>Logger tanpa isi klinis: id, layanan, versi program, status.</para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/rehabilitation-management/rehab-programs")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Rehab Program",
        AreaName = P.AreaName,
        ControllerName = P.TherapyProgram.Resource,
        Description = "Program terapi rehab berversi: susun, revisi, aktifkan, hentikan",
        SortOrder = 8
    )]
    [Tags("Health Services / Rehabilitation Management / Rehab Program")]
    public class RehabProgramController : ControllerBase
    {
        private const string Resource = P.TherapyProgram.Resource;
        private const string ReadName = "Read Rehab Program";
        private const string CreateName = "Create Rehab Program";
        private const string UpdateName = "Update Rehab Program";
        private const string ActivateName = "Activate Rehab Program";
        private const string StopName = "Stop Rehab Program";

        private readonly RhbProgramService _service;
        private readonly LoggerService _logger;

        public RehabProgramController(RhbProgramService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("by-episode/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabProgramListItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = "Melihat program terapi rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetByEpisode(Guid episodeId, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.ListByEpisodeAsync(episodeId, cancellationToken), "Daftar program terapi berhasil diambil.");

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabProgramDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = "Melihat program terapi rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetDetailAsync(id, RhbHttp.ActorId(User), cancellationToken), "Rincian program terapi berhasil diambil.");

        [HttpPost("by-episode/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabProgramDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, CreateName, Description = "Menyusun dan merevisi program terapi rehab", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(Resource, P.Create)]
        public async Task<IActionResult> Create(Guid episodeId, [FromBody] SaveRehabProgramRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.CreateAsync(episodeId, request, RhbHttp.ActorId(User), cancellationToken),
                "Draft program terapi berhasil dibuat.", _logger, Resource, P.Create, Trace);

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabProgramDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = "Mengubah dan membatalkan draft program terapi rehab", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SaveRehabProgramRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.UpdateAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Draft program terapi berhasil diubah.", _logger, Resource, P.Update, Trace);

        [HttpPost("{id:guid}/revise")]
        [ProducesResponseType(typeof(ApiResponse<RehabProgramDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, CreateName, Description = "Menyusun dan merevisi program terapi rehab", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(Resource, P.Create)]
        public async Task<IActionResult> Revise(Guid id, [FromBody] ReviseRehabProgramRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.ReviseAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Versi baru program terapi berhasil dibuat.", _logger, Resource, "Revise", Trace);

        [HttpPost("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<RehabProgramDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.TherapyProgram.Activate, ActivateName, Description = "Menandatangani dan mengaktifkan program terapi rehab", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(Resource, P.TherapyProgram.Activate)]
        public async Task<IActionResult> Activate(Guid id, [FromBody] RehabSignRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.ActivateAsync(id, request, RhbHttp.ActorId(User),
                    Request.Headers.UserAgent.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken),
                "Program terapi aktif.", _logger, Resource, P.TherapyProgram.Activate, Trace);

        [HttpPost("{id:guid}/stop")]
        [ProducesResponseType(typeof(ApiResponse<RehabProgramDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.TherapyProgram.Stop, StopName, Description = "Menghentikan program terapi rehab yang aktif", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(Resource, P.TherapyProgram.Stop)]
        public async Task<IActionResult> Stop(Guid id, [FromBody] RehabReasonRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.StopAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Program terapi dihentikan.", _logger, Resource, P.TherapyProgram.Stop, Trace);

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<RehabProgramDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = "Mengubah dan membatalkan draft program terapi rehab", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] RehabReasonRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.CancelAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Draft program terapi dibatalkan.", _logger, Resource, "Cancel", Trace);

        private static object Trace(RehabProgramDetailResponse x) =>
            new { x.Id, x.EpisodeId, x.DisciplineId, x.ProgramVersion, Status = x.ProgramStatus.ToString(), x.Version };
    }
}
