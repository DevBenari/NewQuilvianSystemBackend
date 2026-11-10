using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants.RehabilitationPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Controllers
{
    /// <summary>
    /// Asesmen awal profesi (fisioterapi, okupasi, wicara, …) — <c>BE-RHB-009</c>,
    /// <c>contracts/api-contract.md</c> v1 grup Rehab Discipline Assessment.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bentuk sama dengan asesmen medis. Pembuatan wajib memuat <c>disciplineId</c> yang sudah
    /// diresepkan asesmen medis final (422 <c>RHB-VAL-032</c>), dan penulis harus berprofesi sesuai
    /// layanan itu (403 <c>RHB-VAL-031</c>, RHB-INV-08). Profesi diperiksa dari data akun, bukan dari
    /// nama peran.
    /// </para>
    /// <para>Logger hanya memuat id, status, dan versi; isi klinis tidak pernah dicatat.</para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/rehabilitation-management/rehab-discipline-assessments")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Rehab Discipline Assessment",
        AreaName = P.AreaName,
        ControllerName = P.DisciplineAssessment.Resource,
        Description = "Asesmen awal profesi terapi untuk layanan yang sudah diresepkan",
        SortOrder = 6
    )]
    [Tags("Health Services / Rehabilitation Management / Rehab Discipline Assessment")]
    public class RehabDisciplineAssessmentController : ControllerBase
    {
        private const RehabAssessmentType Type = RehabAssessmentType.DisciplineInitial;
        private const string Resource = P.DisciplineAssessment.Resource;
        private const string ReadName = "Read Rehab Discipline Assessment";
        private const string CreateName = "Create Rehab Discipline Assessment";
        private const string UpdateName = "Update Rehab Discipline Assessment";
        private const string SignName = "Sign Rehab Discipline Assessment";
        private const string Label = "Asesmen profesi";

        private readonly RhbAssessmentService _service;
        private readonly LoggerService _logger;

        public RehabDisciplineAssessmentController(RhbAssessmentService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("by-episode/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabAssessmentListItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = "Melihat asesmen profesi rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetByEpisode(Guid episodeId, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.ListByEpisodeAsync(episodeId, Type, cancellationToken), $"Daftar {Label.ToLowerInvariant()} berhasil diambil.");

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = "Melihat asesmen profesi rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetDetailAsync(id, Type, RhbHttp.ActorId(User), cancellationToken), $"Rincian {Label.ToLowerInvariant()} berhasil diambil.");

        [HttpPost("by-episode/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabAssessmentDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, CreateName, Description = "Membuat draft asesmen profesi rehab", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(Resource, P.Create)]
        public async Task<IActionResult> Create(Guid episodeId, [FromBody] SaveRehabAssessmentRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.CreateAsync(episodeId, Type, request, RhbHttp.ActorId(User), cancellationToken),
                $"Draft {Label.ToLowerInvariant()} berhasil dibuat.", _logger, Resource, P.Create, Trace);

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = "Mengubah dan membatalkan draft asesmen profesi rehab", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SaveRehabAssessmentRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.UpdateAsync(id, Type, request, RhbHttp.ActorId(User), cancellationToken),
                $"{Label} berhasil diubah.", _logger, Resource, P.Update, Trace);

        [HttpPost("{id:guid}/sign")]
        [ProducesResponseType(typeof(ApiResponse<RehabAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Sign, SignName, Description = "Menandatangani asesmen profesi rehab", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(Resource, P.Sign)]
        public async Task<IActionResult> Sign(Guid id, [FromBody] RehabSignRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.SignAsync(id, Type, request, RhbHttp.ActorId(User),
                    Request.Headers.UserAgent.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken),
                $"{Label} berhasil ditandatangani.", _logger, Resource, P.Sign, Trace);

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<RehabAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = "Mengubah dan membatalkan draft asesmen profesi rehab", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] RehabReasonRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.CancelAsync(id, Type, request, RhbHttp.ActorId(User), cancellationToken),
                $"Draft {Label.ToLowerInvariant()} dibatalkan.", _logger, Resource, "Cancel", Trace);

        /// <summary>Isi logger: id, status, versi — tanpa isi klinis.</summary>
        private static object Trace(RehabAssessmentDetailResponse x) =>
            new { x.Id, x.EpisodeId, Status = x.RecordStatus.ToString(), x.Version };
    }
}
