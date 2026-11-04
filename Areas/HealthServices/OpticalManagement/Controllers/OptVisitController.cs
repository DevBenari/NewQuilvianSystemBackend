using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Constants.OpticalPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Controllers
{
    /// <summary>
    /// Kunjungan Optik — <c>OPT-API-v1</c> grup Opt Visit (<c>BE-OPT-010</c>): daftar, ringkasan, dan pendaftaran
    /// kunjungan Optik lewat Registrasi (<c>INT-OPT-01</c>).
    /// </summary>
    /// <remarks>
    /// Pendaftaran juga mensyaratkan izin Registrasi <c>PatientEncounter : Create</c>, karena kunjungannya dibuat
    /// <c>EncounterIntakeService</c>. Action sengaja tanpa <c>&lt;summary&gt;</c> XML.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/optical-management/visits")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Opt Visit",
        AreaName = P.AreaName,
        ControllerName = P.Visit.Resource,
        Description = "Kunjungan Optik: daftar, ringkasan, dan pendaftaran lewat Registrasi",
        SortOrder = 2
    )]
    [Tags("Health Services / Optical Management / Opt Visit")]
    public class OptVisitController : ControllerBase
    {
        private readonly OptVisitService _service;
        private readonly LoggerService _logger;

        public OptVisitController(OptVisitService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OptVisitListItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Opt Visit", Description = "Melihat daftar kunjungan Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Visit.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] OptVisitListQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<OptVisitListItemResponse>>.Ok(
                await _service.GetListAsync(query, cancellationToken), "Daftar kunjungan Optik berhasil diambil."));

        [HttpGet("{encounterId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OptVisitDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Opt Visit", Description = "Melihat ringkasan kunjungan Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Visit.Resource, P.Read)]
        public async Task<IActionResult> GetDetail(Guid encounterId, CancellationToken cancellationToken = default) =>
            OptHttp.Map(this, await _service.GetDetailAsync(encounterId, cancellationToken), "Ringkasan kunjungan Optik berhasil diambil.");

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OptVisitListItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.Create, "Create Opt Visit", Description = "Mendaftarkan kunjungan Optik", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Visit.Resource, P.Create)]
        public async Task<IActionResult> Register(
            [FromBody] OptVisitRegisterRequest request,
            [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey,
            CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.RegisterAsync(request, idempotencyKey?.ToString(), cancellationToken),
                "Kunjungan Optik berhasil didaftarkan.", _logger, P.Visit.Resource, P.Create,
                x => x.IsReplay
                    ? $"Pendaftaran kunjungan Optik {x.EncounterNumber} diulang dengan kunci yang sama; tidak ada kunjungan baru."
                    : $"Kunjungan Optik {x.EncounterNumber} didaftarkan.");
    }
}
