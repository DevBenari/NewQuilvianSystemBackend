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
    /// Reasesmen kemajuan dan keputusannya — <c>BE-RHB-014</c>, <c>contracts/api-contract.md</c> v1 grup
    /// Rehab Progress Review, <c>state-transition-matrix.md</c> §7.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Arketipe <b>sub-proses ter-scope induk</b> (episode). Efek keputusan (program dihentikan/selesai/perlu
    /// telaah, status tujuan) terjadi saat <c>POST /{id}/sign</c>. Tidak ada <c>DELETE</c>; koreksi sesudah
    /// final lewat addendum Rekam Medis (jenis dokumen 50).
    /// </para>
    /// <para>Logger hanya memuat id, keputusan, status, dan versi; ringkasan dan alasan klinis tidak dicatat.</para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/rehabilitation-management/rehab-progress-reviews")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Rehab Progress Review",
        AreaName = P.AreaName,
        ControllerName = P.ProgressReview.Resource,
        Description = "Reasesmen kemajuan rehab: pembanding baseline, penilaian tujuan, dan keputusan program",
        SortOrder = 12
    )]
    [Tags("Health Services / Rehabilitation Management / Rehab Progress Review")]
    public class RehabProgressReviewController : ControllerBase
    {
        private const string Resource = P.ProgressReview.Resource;
        private const string ReadName = "Read Rehab Progress Review";
        private const string CreateName = "Create Rehab Progress Review";
        private const string UpdateName = "Update Rehab Progress Review";
        private const string SignName = "Sign Rehab Progress Review";

        private readonly RhbProgressReviewService _service;
        private readonly LoggerService _logger;

        public RehabProgressReviewController(RhbProgressReviewService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("by-episode/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabProgressReviewItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = "Melihat reasesmen rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetByEpisode(Guid episodeId, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.ListByEpisodeAsync(episodeId, cancellationToken), "Daftar reasesmen berhasil diambil.");

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabProgressReviewDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = "Melihat reasesmen rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetDetailAsync(id, RhbHttp.ActorId(User), cancellationToken), "Rincian reasesmen berhasil diambil.");

        [HttpPost("by-episode/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabProgressReviewDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, CreateName, Description = "Membuat draft reasesmen rehab", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(Resource, P.Create)]
        public async Task<IActionResult> Create(Guid episodeId, [FromBody] SaveRehabProgressReviewRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.CreateAsync(episodeId, request, RhbHttp.ActorId(User), cancellationToken),
                "Draft reasesmen berhasil dibuat.", _logger, Resource, P.Create, Trace);

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabProgressReviewDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = "Mengubah draft reasesmen rehab", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SaveRehabProgressReviewRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.UpdateAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Draft reasesmen berhasil diubah.", _logger, Resource, P.Update, Trace);

        [HttpPost("{id:guid}/sign")]
        [ProducesResponseType(typeof(ApiResponse<RehabProgressReviewDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Sign, SignName, Description = "Menandatangani reasesmen rehab beserta efek keputusannya", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(Resource, P.Sign)]
        public async Task<IActionResult> Sign(Guid id, [FromBody] RehabSignRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.SignAsync(id, request, RhbHttp.ActorId(User),
                    Request.Headers.UserAgent.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken),
                "Reasesmen berhasil ditandatangani.", _logger, Resource, P.Sign, Trace);

        /// <summary>Delta kontrak BE-RHB-014: batalkan draft reasesmen (butir <c>Update</c>, penulis, alasan wajib).</summary>
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<RehabProgressReviewDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = "Mengubah draft reasesmen rehab", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] RehabReasonRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.CancelAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Draft reasesmen dibatalkan.", _logger, Resource, "Cancel", Trace);

        private static object Trace(RehabProgressReviewDetailResponse x) =>
            new { x.Id, x.EpisodeId, x.ProgramId, Decision = x.Decision?.ToString(), Status = x.RecordStatus.ToString(), Program = x.ProgramStatus?.ToString(), x.Version };
    }
}
