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
    /// Sesi terapi — <c>BE-RHB-012</c>, <c>contracts/api-contract.md</c> v1 grup Rehab Session,
    /// <c>state-transition-matrix.md</c> §5. Endpoint handoff billing (<c>/{id}/billing-handoff</c>) milik
    /// <c>RehabSessionBillingController</c> di <c>BE-RHB-013</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Arketipe <b>aggregate ber-lifecycle + worklist</b>: dibuat lewat <c>POST /by-program/{programId}/start</c>
    /// atau <c>/not-done</c>; perpindahan status lewat <c>complete</c>, <c>stop-early</c>, <c>sign</c>,
    /// <c>cancel</c>. Tidak ada <c>DELETE</c>, <c>options</c>, maupun <c>PATCH status</c>.
    /// </para>
    /// <para>Logger hanya memuat id, nomor, status, dan versi; kondisi, respons, dan deviasi tidak dicatat.</para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/rehabilitation-management/rehab-sessions")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Rehab Session",
        AreaName = P.AreaName,
        ControllerName = P.Session.Resource,
        Description = "Sesi terapi rehab: cek keselamatan, pelaksanaan, henti dini, tanda tangan",
        SortOrder = 10
    )]
    [Tags("Health Services / Rehabilitation Management / Rehab Session")]
    public class RehabSessionController : ControllerBase
    {
        private const string Resource = P.Session.Resource;
        private const string ReadName = "Read Rehab Session";
        private const string CreateName = "Create Rehab Session";
        private const string UpdateName = "Update Rehab Session";
        private const string SignName = "Sign Rehab Session";
        private const string ReadDescription = "Melihat sesi terapi rehab";
        private const string UpdateDescription = "Mengisi, menyelesaikan, menghentikan dini, dan membatalkan sesi terapi rehab";

        private readonly RhbSessionService _service;
        private readonly LoggerService _logger;

        public RehabSessionController(RhbSessionService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        // =====================================================================
        // Baca
        // =====================================================================

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<RehabSessionFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, ReadName, Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<RehabSessionFilterMetadataResponse>.Ok(await _service.BuildFilterMetadataAsync(cancellationToken), "Metadata filter sesi rehab berhasil diambil."));

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<RehabSessionSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, ReadName, Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetSummary([FromQuery] RehabSessionQuery query, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetSummaryAsync(query, RhbHttp.ActorId(User), cancellationToken), "Ringkasan sesi rehab berhasil diambil.");

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RehabSessionListItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, ReadName, Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] RehabSessionQuery query, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetListAsync(query, RhbHttp.ActorId(User), cancellationToken), "Daftar sesi rehab berhasil diambil.");

        [HttpGet("by-episode/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabSessionListItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetByEpisode(Guid episodeId, [FromQuery] RehabSessionByEpisodeQuery query, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.ListByEpisodeAsync(episodeId, query, cancellationToken), "Sesi episode berhasil diambil.");

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabSessionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetDetailAsync(id, RhbHttp.ActorId(User), cancellationToken), "Rincian sesi rehab berhasil diambil.");

        // =====================================================================
        // Buat
        // =====================================================================

        [HttpPost("by-program/{programId:guid}/start")]
        [ProducesResponseType(typeof(ApiResponse<RehabSessionDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, CreateName, Description = "Memulai sesi atau mencatat sesi tidak terlaksana", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(Resource, P.Create)]
        public async Task<IActionResult> Start(Guid programId, [FromBody] StartRehabSessionRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.StartAsync(programId, request, RhbHttp.ActorId(User), cancellationToken),
                "Sesi terapi dimulai.", _logger, Resource, "Start", Trace);

        [HttpPost("by-program/{programId:guid}/not-done")]
        [ProducesResponseType(typeof(ApiResponse<RehabSessionDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, CreateName, Description = "Memulai sesi atau mencatat sesi tidak terlaksana", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(Resource, P.Create)]
        public async Task<IActionResult> RecordNotDone(Guid programId, [FromBody] RecordNotDoneRehabSessionRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.RecordNotDoneAsync(programId, request, RhbHttp.ActorId(User), cancellationToken),
                "Sesi tidak terlaksana dicatat.", _logger, Resource, "NotDone", Trace);

        // =====================================================================
        // Selama berlangsung dan sesudahnya
        // =====================================================================

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabSessionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = UpdateDescription, AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRehabSessionRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.UpdateAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Sesi terapi berhasil diubah.", _logger, Resource, P.Update, Trace);

        [HttpPost("{id:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<RehabSessionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = UpdateDescription, AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> Complete(Guid id, [FromBody] RehabVersionRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.CompleteAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Sesi terapi selesai.", _logger, Resource, "Complete", Trace);

        [HttpPost("{id:guid}/stop-early")]
        [ProducesResponseType(typeof(ApiResponse<RehabSessionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = UpdateDescription, AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> StopEarly(Guid id, [FromBody] StopEarlyRehabSessionRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.StopEarlyAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Sesi terapi dihentikan dini.", _logger, Resource, "StopEarly", Trace);

        [HttpPost("{id:guid}/sign")]
        [ProducesResponseType(typeof(ApiResponse<RehabSessionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Sign, SignName, Description = "Menandatangani sesi terapi rehab", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(Resource, P.Sign)]
        public async Task<IActionResult> Sign(Guid id, [FromBody] RehabSignRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.SignAsync(id, request, RhbHttp.ActorId(User),
                    Request.Headers.UserAgent.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken),
                "Sesi terapi berhasil ditandatangani.", _logger, Resource, P.Sign, Trace);

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<RehabSessionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = UpdateDescription, AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] RehabReasonRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.CancelAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Sesi terapi dibatalkan.", _logger, Resource, "Cancel", Trace);

        private static object Trace(RehabSessionDetailResponse x) =>
            new { x.Id, x.SessionNumber, Status = x.SessionStatus.ToString(), Record = x.RecordStatus.ToString(), Billing = x.BillingHandoffStatus.ToString(), x.Version };
    }
}
