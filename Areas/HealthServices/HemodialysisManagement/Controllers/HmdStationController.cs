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
    /// <summary>Master station cuci darah — kursi atau tempat tidur (<c>BE-HMD-04</c>).</summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/master-data/hemodialysis-stations")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Station",
        AreaName = P.AreaName,
        ControllerName = P.Station.Resource,
        Description = "Master station hemodialisa beserta statusnya",
        SortOrder = 16
    )]
    [Tags("Health Services / Hemodialysis Management / Master Data / Hemodialysis Station")]
    public class HmdStationController : ControllerBase
    {
        private readonly HmdResourceService _service;
        private readonly LoggerService _logger;

        public HmdStationController(HmdResourceService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<HmdStationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Station", Description = "Melihat konfigurasi penyaring station HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Station.Resource, P.Read)]
        public IActionResult GetFilterMetadata() =>
            Ok(ApiResponse<HmdStationFilterMetadataResponse>.Ok(
                HmdResourceService.BuildStationFilterMetadata(), "Metadata filter station HD berhasil diambil."));

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<HmdStationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Station", Description = "Melihat ringkasan station HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Station.Resource, P.Read)]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<HmdStationSummaryResponse>.Ok(
                await _service.GetStationSummaryAsync(cancellationToken), "Ringkasan station HD berhasil diambil."));

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdStationResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Station", Description = "Melihat daftar station HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Station.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] HmdStationPagedQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<HmdStationResponse>>.Ok(
                await _service.GetStationsAsync(query, cancellationToken), "Daftar station HD berhasil diambil."));

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdStationOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Station", Description = "Melihat pilihan station HD yang tersedia", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Station.Resource, P.Read)]
        public async Task<IActionResult> GetOptions([FromQuery] HmdStationOptionsQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<HmdStationOptionResponse>>.Ok(
                await _service.GetStationOptionsAsync(query, cancellationToken), "Pilihan station HD berhasil diambil."));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdStationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Station", Description = "Melihat rincian station HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Station.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetStationAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Station tidak ditemukan atau sudah dihapus."))
                : Ok(ApiResponse<HmdStationResponse>.Ok(result, "Rincian station HD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<HmdStationResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Create, "Create Hemodialysis Station", Description = "Mendaftarkan station HD baru", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Station.Resource, P.Create)]
        public async Task<IActionResult> Create([FromBody] CreateHmdStationRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateStationAsync(request, HmdHttp.ActorId(User), cancellationToken),
                "Station HD berhasil didaftarkan.", _logger, P.Station.Resource, P.Create,
                x => new { x.Id, x.StationCode });

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdStationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Hemodialysis Station", Description = "Memperbarui data station HD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Station.Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHmdStationRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.UpdateStationAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Data station HD berhasil diperbarui.", _logger, P.Station.Resource, P.Update,
                x => new { x.Id, x.StationCode, x.IsActive });

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<HmdStationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Station.ChangeStatus, "Change Hemodialysis Station Status", Description = "Mengubah status station HD beserta alasannya", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.Station.Resource, P.Station.ChangeStatus)]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeHmdStationStatusRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.ChangeStationStatusAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Status station HD berhasil diubah.", _logger, P.Station.Resource, P.Station.ChangeStatus,
                x => new { x.Id, x.StationCode, x.StationStatus });

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Delete, "Delete Hemodialysis Station", Description = "Menonaktifkan station HD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission(P.Station.Resource, P.Delete)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.DeleteStationAsync(id, HmdHttp.ActorId(User), cancellationToken),
                "Station HD berhasil dinonaktifkan.", _logger, P.Station.Resource, P.Delete,
                _ => new { Id = id });
    }
}
