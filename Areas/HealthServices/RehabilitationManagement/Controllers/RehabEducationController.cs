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
    /// Edukasi rencana terapi dan persetujuan pasien/keluarga — <c>BE-RHB-010</c>,
    /// <c>contracts/api-contract.md</c> v1 grup Rehab Education, FR-RHB-020.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Arketipe <b>sub-proses ter-scope induk</b> (episode, opsional program). Empat endpoint sesuai
    /// kontrak: daftar, catat draft, ubah draft, tanda tangan. Tidak ada <c>DELETE</c> maupun
    /// <c>cancel</c>; koreksi sesudah final lewat addendum Rekam Medis (jenis dokumen 52).
    /// </para>
    /// <para>Logger hanya memuat id, status, dan versi; isi edukasi, penerima, dan catatan tidak dicatat.</para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/rehabilitation-management/rehab-education-records")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Rehab Education",
        AreaName = P.AreaName,
        ControllerName = P.Education.Resource,
        Description = "Edukasi rencana terapi rehab dan status persetujuan pasien/keluarga",
        SortOrder = 9
    )]
    [Tags("Health Services / Rehabilitation Management / Rehab Education")]
    public class RehabEducationController : ControllerBase
    {
        private const string Resource = P.Education.Resource;
        private const string ReadName = "Read Rehab Education";
        private const string CreateName = "Create Rehab Education";
        private const string UpdateName = "Update Rehab Education";
        private const string SignName = "Sign Rehab Education";

        private readonly RhbEducationService _service;
        private readonly LoggerService _logger;

        public RehabEducationController(RhbEducationService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("by-episode/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabEducationItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = "Melihat edukasi dan persetujuan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetByEpisode(Guid episodeId, [FromQuery] RehabEducationQuery query, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.ListByEpisodeAsync(episodeId, query, RhbHttp.ActorId(User), cancellationToken),
                "Daftar edukasi rehab berhasil diambil.");

        [HttpPost("by-episode/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabEducationItem>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, CreateName, Description = "Mencatat edukasi dan persetujuan rehab", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(Resource, P.Create)]
        public async Task<IActionResult> Create(Guid episodeId, [FromBody] SaveRehabEducationRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.CreateAsync(episodeId, request, RhbHttp.ActorId(User), cancellationToken),
                "Catatan edukasi rehab berhasil dibuat.", _logger, Resource, P.Create, Trace);

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabEducationItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = "Mengubah draft edukasi dan persetujuan rehab", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SaveRehabEducationRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.UpdateAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Catatan edukasi rehab berhasil diubah.", _logger, Resource, P.Update, Trace);

        [HttpPost("{id:guid}/sign")]
        [ProducesResponseType(typeof(ApiResponse<RehabEducationItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Sign, SignName, Description = "Menandatangani edukasi dan persetujuan rehab", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(Resource, P.Sign)]
        public async Task<IActionResult> Sign(Guid id, [FromBody] RehabSignRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.SignAsync(id, request, RhbHttp.ActorId(User),
                    Request.Headers.UserAgent.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken),
                "Catatan edukasi rehab berhasil ditandatangani.", _logger, Resource, P.Sign, Trace);

        /// <summary>Isi logger: id, status, persetujuan, versi — tanpa isi edukasi.</summary>
        private static object Trace(RehabEducationItem x) =>
            new { x.Id, x.EpisodeId, x.ProgramId, Status = x.RecordStatus.ToString(), Consent = x.ConsentStatus.ToString(), x.Version };
    }
}
