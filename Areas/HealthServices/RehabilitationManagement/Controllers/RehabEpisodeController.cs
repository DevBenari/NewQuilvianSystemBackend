using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using P = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants.RehabilitationPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Controllers
{
    /// <summary>
    /// Worklist dan ruang kerja episode rehab — <c>BE-RHB-008</c>, <c>contracts/api-contract.md</c>
    /// v1 grup Rehab Episode. Seluruhnya <c>GET</c> baca saja di bawah <c>RehabEpisode : Read</c>.
    /// </summary>
    /// <remarks>
    /// Episode dibuka oleh <c>accept</c> permintaan dan ditutup oleh ringkasan akhir; controller ini
    /// tidak punya aksi tulis. Rincian menyertakan aksi yang sah menurut status episode dan butir hak
    /// akses pengguna. Endpoint baca tidak dicatat ke logger.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/rehabilitation-management/rehab-episodes")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Rehab Episode",
        AreaName = P.AreaName,
        ControllerName = P.Episode.Resource,
        Description = "Worklist dan ruang kerja episode Rehab Medik",
        SortOrder = 4
    )]
    [Tags("Health Services / Rehabilitation Management / Rehab Episode")]
    public class RehabEpisodeController : ControllerBase
    {
        private const string ReadName = "Read Rehab Episode";
        private const string ReadDescription = "Melihat episode rehab";

        private readonly RhbEpisodeService _service;

        public RehabEpisodeController(RhbEpisodeService service)
        {
            _service = service;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<RehabEpisodeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, ReadName, Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Episode.Resource, P.Read)]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<RehabEpisodeFilterMetadataResponse>.Ok(
                await _service.BuildFilterMetadataAsync(cancellationToken), "Metadata filter episode rehab berhasil diambil."));

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<RehabEpisodeSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, ReadName, Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Episode.Resource, P.Read)]
        public async Task<IActionResult> GetSummary([FromQuery] RehabEpisodeQuery query, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetSummaryAsync(query, cancellationToken), "Ringkasan episode rehab berhasil diambil.");

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RehabEpisodeListItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, ReadName, Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Episode.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] RehabEpisodeQuery query, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetListAsync(query, cancellationToken), "Daftar episode rehab berhasil diambil.");

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabEpisodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Episode.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var detail = await _service.GetDetailAsync(id, User, cancellationToken);
            return detail == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Episode rehab tidak ditemukan."))
                : Ok(ApiResponse<RehabEpisodeDetailResponse>.Ok(detail, "Rincian episode rehab berhasil diambil."));
        }

        [HttpGet("{id:guid}/timeline")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabTimelineItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Episode.Resource, P.Read)]
        public async Task<IActionResult> GetTimeline(Guid id, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetTimelineAsync(id, cancellationToken), "Timeline episode rehab berhasil diambil.");

        [HttpGet("{id:guid}/status-history")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabStatusHistoryItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Episode.Resource, P.Read)]
        public async Task<IActionResult> GetStatusHistory(Guid id, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetStatusHistoryAsync(id, cancellationToken), "Riwayat status episode rehab berhasil diambil.");
    }
}
