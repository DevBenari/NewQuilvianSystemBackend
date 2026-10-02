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
    /// Pemberian obat intradialisis dan penerusan pemakaiannya ke Farmasi (<c>BE-HMD-15</c>).
    /// </summary>
    /// <remarks>
    /// Catatan klinis dahulu, persediaan belakangan: pemberian obat selalu tersimpan dan dijawab
    /// <c>201</c>, sekalipun Farmasi sedang gagal dihubungi. Status penerusannya dibaca pada
    /// <c>PharmacySyncStatus</c> dan dapat diulang lewat endpoint pengulangan.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/hemodialysis-sessions")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Medication",
        AreaName = P.AreaName,
        ControllerName = P.Medication.Resource,
        Description = "Pemberian obat selama sesi hemodialisa",
        SortOrder = 11
    )]
    [Tags("Health Services / Hemodialysis Management / Hemodialysis Session")]
    public class HmdMedicationController : ControllerBase
    {
        private readonly HmdSessionService _service;
        private readonly LoggerService _logger;

        public HmdMedicationController(HmdSessionService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{id:guid}/medications")]
        [ProducesResponseType(typeof(ApiResponse<List<HmdMedicationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Medication", Description = "Melihat pemberian obat pada satu sesi HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Medication.Resource, P.Read)]
        public async Task<IActionResult> GetList(Guid id, CancellationToken cancellationToken = default) =>
            HmdHttp.Map(this, await _service.GetMedicationsAsync(id, cancellationToken), "Daftar pemberian obat berhasil diambil.");

        [HttpPost("{id:guid}/medications")]
        [ProducesResponseType(typeof(ApiResponse<HmdMedicationResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Medication.Administer, "Administer Hemodialysis Medication", Description = "Mencatat pemberian obat selama sesi HD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Medication.Resource, P.Medication.Administer)]
        public async Task<IActionResult> Create(Guid id, [FromBody] CreateHmdMedicationRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateMedicationAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Pemberian obat berhasil dicatat.", _logger, P.Medication.Resource, P.Medication.Administer,
                x => new { x.Id, x.SessionId, x.DrugId, x.PharmacySyncStatus });

        /// <summary>Mengulang penerusan pemberian obat ke Farmasi yang masih tertunda.</summary>
        [HttpPost("{id:guid}/medications/{medicationId:guid}/pharmacy-handoff/retry")]
        [ProducesResponseType(typeof(ApiResponse<HmdMedicationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Medication.Administer, "Administer Hemodialysis Medication", Description = "Mengulang penerusan pemberian obat sesi HD ke Farmasi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Medication.Resource, P.Medication.Administer)]
        public async Task<IActionResult> RetryPharmacyHandoff(
            Guid id,
            Guid medicationId,
            [FromBody] RetryHmdPharmacyHandoffRequest? request,
            CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.RetryPharmacyHandoffAsync(id, medicationId, request ?? new RetryHmdPharmacyHandoffRequest(), HmdHttp.ActorId(User), cancellationToken),
                "Penerusan pemberian obat ke Farmasi berhasil diproses ulang.", _logger, P.Medication.Resource, "RetryPharmacyHandoff",
                x => new { x.Id, x.SessionId, x.PharmacySyncStatus });
    }
}
