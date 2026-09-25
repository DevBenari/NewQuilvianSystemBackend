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
    /// Program HD pasien — aggregate ber-lifecycle <c>Draft → Active ⇄ Suspended → Closed</c>
    /// (<c>BE-HMD-08</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>PATCH /{id}/status</c> dipertahankan persis seperti kontrak <c>HMD-CONTRACT-v1</c>
    /// (<c>HemodialysisEpisode : ChangeStatus</c>), walaupun <c>transaction-endpoint-standard.md</c>
    /// menganjurkan <c>POST /{id}/&lt;aksi&gt;</c>. Selisih itu dicatat pada laporan task sebagai
    /// drift kontrak, bukan diubah sepihak.
    /// </para>
    /// <para>
    /// Kelayakan, akses vaskular, serologi, dan isolasi berada pada base URL yang sama tetapi
    /// dilayani controller masing-masing, karena setiap Resource hak akses wajib punya controller
    /// sendiri supaya terdaftar pada layar Akses Role.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/hemodialysis-episodes")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Episode",
        AreaName = P.AreaName,
        ControllerName = P.Episode.Resource,
        Description = "Program hemodialisa pasien",
        SortOrder = 2
    )]
    [Tags("Health Services / Hemodialysis Management / Hemodialysis Episode")]
    public class HmdEpisodeController : ControllerBase
    {
        private readonly HmdEpisodeService _service;
        private readonly LoggerService _logger;

        public HmdEpisodeController(HmdEpisodeService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<HmdEpisodeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Episode", Description = "Melihat konfigurasi penyaring episode HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Episode.Resource, P.Read)]
        public IActionResult GetFilterMetadata() =>
            Ok(ApiResponse<HmdEpisodeFilterMetadataResponse>.Ok(
                HmdEpisodeService.BuildFilterMetadata(), "Metadata filter episode HD berhasil diambil."));

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<HmdEpisodeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Episode", Description = "Melihat ringkasan episode HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Episode.Resource, P.Read)]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<HmdEpisodeSummaryResponse>.Ok(
                await _service.GetSummaryAsync(cancellationToken), "Ringkasan episode HD berhasil diambil."));

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdEpisodeListResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Episode", Description = "Melihat daftar pasien HD beserta status episodenya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Episode.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] HmdEpisodePagedQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<HmdEpisodeListResponse>>.Ok(
                await _service.GetListAsync(query, cancellationToken), "Daftar episode HD berhasil diambil."));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdEpisodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Episode", Description = "Melihat ringkasan satu episode HD beserta konteks pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Episode.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetDetailAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Episode hemodialisa tidak ditemukan atau sudah dihapus."))
                : Ok(ApiResponse<HmdEpisodeDetailResponse>.Ok(result, "Rincian episode HD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<HmdEpisodeDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Create, "Create Hemodialysis Episode", Description = "Membuka episode HD baru untuk pasien", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Episode.Resource, P.Create)]
        public async Task<IActionResult> Create([FromBody] CreateHmdEpisodeRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateAsync(request, HmdHttp.ActorId(User), cancellationToken),
                "Episode hemodialisa berhasil dibuat.", _logger, P.Episode.Resource, P.Create,
                x => new { x.Id, x.EpisodeNumber, x.PatientId, x.EpisodeStatus });

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdEpisodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, "Update Hemodialysis Episode", Description = "Memperbarui data administratif episode HD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Episode.Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHmdEpisodeRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.UpdateAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Episode hemodialisa berhasil diperbarui.", _logger, P.Episode.Resource, P.Update,
                x => new { x.Id, x.EpisodeNumber });

        /// <summary>
        /// Mengaktifkan, menangguhkan, atau menutup episode. Episode aktif kedua untuk pasien yang
        /// sama ditolak <c>409</c>; penutupan dengan sesi yang belum disahkan ditolak <c>422</c>.
        /// </summary>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<HmdEpisodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Episode.ChangeStatus, "Change Hemodialysis Episode Status", Description = "Mengaktifkan, menangguhkan, atau menutup episode HD", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.Episode.Resource, P.Episode.ChangeStatus)]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeHmdEpisodeStatusRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.ChangeStatusAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Status episode hemodialisa berhasil diubah.", _logger, P.Episode.Resource, P.Episode.ChangeStatus,
                x => new { x.Id, x.EpisodeNumber, x.EpisodeStatus, x.ClosureReason });
    }
}
