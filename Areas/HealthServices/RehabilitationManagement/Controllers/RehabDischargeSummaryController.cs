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
    /// Ringkasan akhir dan penutupan episode — <c>BE-RHB-015</c>, <c>contracts/api-contract.md</c> v1 grup
    /// Rehab Discharge Summary, V-DIS-01/02.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Satu dokumen per episode, sehingga path tetap <c>/by-episode/{episodeId}</c>: <c>GET</c> membaca (beserta
    /// daftar catatan yang belum final), <c>PUT</c> menyimpan draft, <c>POST /sign</c> menandatangani dan menutup
    /// episode. Tidak ada <c>DELETE</c>; koreksi sesudah final lewat addendum Rekam Medis (jenis dokumen 51).
    /// </para>
    /// <para>Logger hanya memuat id, status, dan alasan penutupan (kode), tanpa isi klinis.</para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/rehabilitation-management/rehab-discharge-summaries")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Rehab Discharge Summary",
        AreaName = P.AreaName,
        ControllerName = P.DischargeSummary.Resource,
        Description = "Ringkasan akhir rehab dan penutupan episode",
        SortOrder = 13
    )]
    [Tags("Health Services / Rehabilitation Management / Rehab Discharge Summary")]
    public class RehabDischargeSummaryController : ControllerBase
    {
        private const string Resource = P.DischargeSummary.Resource;
        private const string ReadName = "Read Rehab Discharge Summary";
        private const string UpdateName = "Update Rehab Discharge Summary";
        private const string SignName = "Sign Rehab Discharge Summary";

        private readonly RhbDischargeService _service;
        private readonly LoggerService _logger;

        public RehabDischargeSummaryController(RhbDischargeService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("by-episode/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabDischargeSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = "Melihat ringkasan akhir rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetByEpisode(Guid episodeId, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetAsync(episodeId, RhbHttp.ActorId(User), cancellationToken), "Ringkasan akhir berhasil diambil.");

        [HttpPut("by-episode/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabDischargeSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<RehabDischargeSummaryResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = "Menyusun draft ringkasan akhir rehab", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission(Resource, P.Update)]
        public async Task<IActionResult> Save(Guid episodeId, [FromBody] SaveRehabDischargeSummaryRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.SaveAsync(episodeId, request, RhbHttp.ActorId(User), cancellationToken),
                "Draft ringkasan akhir berhasil disimpan.", _logger, Resource, P.Update, Trace);

        [HttpPost("by-episode/{episodeId:guid}/sign")]
        [ProducesResponseType(typeof(ApiResponse<RehabDischargeSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Sign, SignName, Description = "Menandatangani ringkasan akhir dan menutup episode rehab", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(Resource, P.Sign)]
        public async Task<IActionResult> Sign(Guid episodeId, [FromBody] RehabSignRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.SignAsync(episodeId, request, RhbHttp.ActorId(User),
                    Request.Headers.UserAgent.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken),
                "Ringkasan akhir ditandatangani; episode ditutup.", _logger, Resource, P.Sign, Trace);

        private static object Trace(RehabDischargeSummaryResponse x) =>
            new { x.Id, x.EpisodeId, Episode = x.EpisodeStatus.ToString(), Status = x.RecordStatus?.ToString(), Closure = x.ClosureReason?.ToString(), x.Version };
    }
}
