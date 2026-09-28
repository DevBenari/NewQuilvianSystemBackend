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
    /// Resep HD — aggregate ber-lifecycle <c>Draft → Active → Superseded</c>, atau <c>Cancelled</c>
    /// (<c>BE-HMD-09</c>). Resep aktif tidak dapat disunting; perubahan instruksi membuat resep baru.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/hemodialysis-prescriptions")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Prescription",
        AreaName = P.AreaName,
        ControllerName = P.Prescription.Resource,
        Description = "Resep hemodialisa dokter",
        SortOrder = 7
    )]
    [Tags("Health Services / Hemodialysis Management / Hemodialysis Prescription")]
    public class HmdPrescriptionController : ControllerBase
    {
        private readonly HmdPrescriptionService _service;
        private readonly LoggerService _logger;

        public HmdPrescriptionController(HmdPrescriptionService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdPrescriptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Prescription", Description = "Melihat riwayat resep HD pada satu episode", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Prescription.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] HmdPrescriptionPagedQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<HmdPrescriptionResponse>>.Ok(
                await _service.GetListAsync(query, cancellationToken), "Riwayat resep HD berhasil diambil."));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdPrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Prescription", Description = "Melihat rincian resep HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Prescription.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Resep hemodialisa tidak ditemukan atau sudah dihapus."))
                : Ok(ApiResponse<HmdPrescriptionResponse>.Ok(result, "Rincian resep HD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<HmdPrescriptionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, "Create Hemodialysis Prescription", Description = "Membuat draf resep HD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Prescription.Resource, P.Create)]
        public async Task<IActionResult> Create([FromBody] CreateHmdPrescriptionRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateAsync(request, HmdHttp.Actor(this), cancellationToken),
                "Draf resep HD berhasil dibuat.", _logger, P.Prescription.Resource, P.Create,
                x => new { x.Id, x.EpisodeId, x.PrescriptionStatus });

        /// <summary>Menyunting resep yang masih draf. Resep aktif ditolak <c>423 Locked</c>.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdPrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Update, "Update Hemodialysis Prescription", Description = "Menyunting resep HD yang masih berstatus draf", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Prescription.Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHmdPrescriptionRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.UpdateAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Draf resep HD berhasil diperbarui.", _logger, P.Prescription.Resource, P.Update,
                x => new { x.Id, x.EpisodeId, x.PrescriptionStatus });

        [HttpPost("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<HmdPrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Prescription.Activate, "Activate Hemodialysis Prescription", Description = "Mengaktifkan resep HD; resep aktif sebelumnya otomatis digantikan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.Prescription.Resource, P.Prescription.Activate)]
        public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.ActivateAsync(id, HmdHttp.ActorId(User), cancellationToken),
                "Resep HD berhasil diaktifkan.", _logger, P.Prescription.Resource, P.Prescription.Activate,
                x => new { x.Id, x.EpisodeId, x.PrescriptionStatus, x.ActivatedAt });

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<HmdPrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Prescription.Cancel, "Cancel Hemodialysis Prescription", Description = "Membatalkan resep HD draf atau aktif", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.Prescription.Resource, P.Prescription.Cancel)]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelHmdPrescriptionRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CancelAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Resep HD berhasil dibatalkan.", _logger, P.Prescription.Resource, P.Prescription.Cancel,
                x => new { x.Id, x.EpisodeId, x.PrescriptionStatus });
    }
}
