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
    /// Kesiapan unit HD per tanggal dan shift — aggregate ber-lifecycle <c>Draft → Ready/NotReady</c>
    /// (<c>BE-HMD-06</c>).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/hemodialysis-unit-readiness")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Unit Readiness",
        AreaName = P.AreaName,
        ControllerName = P.UnitReadiness.Resource,
        Description = "Pemeriksaan kesiapan unit hemodialisa per shift",
        SortOrder = 14
    )]
    [Tags("Health Services / Hemodialysis Management / Hemodialysis Unit Readiness")]
    public class HmdUnitReadinessController : ControllerBase
    {
        private readonly HmdUnitReadinessService _service;
        private readonly LoggerService _logger;

        public HmdUnitReadinessController(HmdUnitReadinessService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdUnitReadinessResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Unit Readiness", Description = "Melihat status kesiapan unit HD per tanggal dan shift", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.UnitReadiness.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] HmdReadinessQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<HmdUnitReadinessResponse>>.Ok(
                await _service.GetListAsync(query, cancellationToken), "Daftar kesiapan unit HD berhasil diambil."));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdUnitReadinessDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Unit Readiness", Description = "Melihat rincian pemeriksaan kesiapan unit HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.UnitReadiness.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetDetailAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Lembar kesiapan unit tidak ditemukan."))
                : Ok(ApiResponse<HmdUnitReadinessDetailResponse>.Ok(result, "Rincian kesiapan unit HD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<HmdUnitReadinessResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Create, "Create Hemodialysis Unit Readiness", Description = "Membentuk lembar pemeriksaan kesiapan unit HD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.UnitReadiness.Resource, P.Create)]
        public async Task<IActionResult> Create([FromBody] CreateHmdUnitReadinessRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateAsync(request, HmdHttp.ActorId(User), cancellationToken),
                "Lembar kesiapan unit HD berhasil dibentuk.", _logger, P.UnitReadiness.Resource, P.Create,
                x => new { x.Id, x.ServiceUnitId, x.ReadinessDate, x.Shift });

        [HttpPut("{id:guid}/items")]
        [ProducesResponseType(typeof(ApiResponse<HmdUnitReadinessDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, "Update Hemodialysis Unit Readiness", Description = "Menyimpan hasil pemeriksaan butir kesiapan unit HD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.UnitReadiness.Resource, P.Update)]
        public async Task<IActionResult> SaveItems(Guid id, [FromBody] SaveHmdReadinessItemsRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.SaveItemsAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Hasil pemeriksaan kesiapan berhasil disimpan.", _logger, P.UnitReadiness.Resource, P.Update,
                x => new { x.Id, x.ReadinessStatus });

        /// <summary>Menyatakan unit siap melayani. Ditolak <c>422</c> bila ada butir wajib belum terpenuhi atau hasil air kedaluwarsa.</summary>
        [HttpPost("{id:guid}/declare-ready")]
        [ProducesResponseType(typeof(ApiResponse<HmdUnitReadinessResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.UnitReadiness.DeclareReady, "Declare Hemodialysis Unit Ready", Description = "Menyatakan unit HD siap melayani", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.UnitReadiness.Resource, P.UnitReadiness.DeclareReady)]
        public async Task<IActionResult> DeclareReady(Guid id, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.DeclareReadyAsync(id, HmdHttp.ActorId(User), cancellationToken),
                "Unit HD dinyatakan siap.", _logger, P.UnitReadiness.Resource, P.UnitReadiness.DeclareReady,
                x => new { x.Id, x.ServiceUnitId, x.ReadinessDate, x.Shift, x.ReadinessStatus });

        [HttpPost("{id:guid}/declare-not-ready")]
        [ProducesResponseType(typeof(ApiResponse<HmdUnitReadinessResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.UnitReadiness.DeclareNotReady, "Declare Hemodialysis Unit Not Ready", Description = "Menyatakan unit HD tidak siap beserta alasannya", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.UnitReadiness.Resource, P.UnitReadiness.DeclareNotReady)]
        public async Task<IActionResult> DeclareNotReady(Guid id, [FromBody] DeclareNotReadyRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.DeclareNotReadyAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Unit HD dinyatakan tidak siap.", _logger, P.UnitReadiness.Resource, P.UnitReadiness.DeclareNotReady,
                x => new { x.Id, x.ServiceUnitId, x.ReadinessDate, x.Shift, x.ReadinessStatus });
    }
}
