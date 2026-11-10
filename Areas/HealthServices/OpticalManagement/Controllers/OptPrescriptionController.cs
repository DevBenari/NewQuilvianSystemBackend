using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Constants.OpticalPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Controllers
{
    /// <summary>
    /// Resep Optik — <c>OPT-API-v1</c> grup Opt Prescription (<c>BE-OPT-012</c>): daftar, detail, pilihan untuk pesanan,
    /// resep internal, salin resep luar, dan revisi berversi.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>GET range-override</c> membawa action <c>OverrideRange</c>. Hak itu dicek service di dalam create/revise
    /// (<c>OPT-PERM-v1</c>), tetapi action hanya terdaftar di layar Akses Role bila menempel pada endpoint; endpoint
    /// ini sekaligus memberi tahu layar apakah isian alasan konfirmasi perlu ditampilkan.
    /// </para>
    /// <para>
    /// Kalimat log hanya memuat nomor resep dan nomor versi, tidak pernah nilai ukur (Sensitif). Action sengaja tanpa
    /// <c>&lt;summary&gt;</c> XML.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/optical-management/prescriptions")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Opt Prescription",
        AreaName = P.AreaName,
        ControllerName = P.Prescription.Resource,
        Description = "Resep kacamata dan lensa kontak: internal, salinan resep luar, revisi berversi",
        SortOrder = 4
    )]
    [Tags("Health Services / Optical Management / Opt Prescription")]
    public class OptPrescriptionController : ControllerBase
    {
        private readonly OptPrescriptionService _service;
        private readonly LoggerService _logger;

        public OptPrescriptionController(OptPrescriptionService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("range-override")]
        [ProducesResponseType(typeof(ApiResponse<OptRangeOverrideEligibilityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.Prescription.OverrideRange, "Override Opt Prescription Range", Description = "Mengonfirmasi ukuran resep di luar rentang (khusus Sp.M, dengan alasan)", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.Prescription.Resource, P.Prescription.OverrideRange)]
        public async Task<IActionResult> GetRangeOverrideEligibility(CancellationToken cancellationToken = default) =>
            OptHttp.Map(this, await _service.GetRangeOverrideEligibilityAsync(OptHttp.ActorId(User), cancellationToken),
                "Hak konfirmasi ukuran di luar rentang berhasil diperiksa.");

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OptPrescriptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Opt Prescription", Description = "Melihat resep Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Prescription.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] OptPrescriptionListQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<OptPrescriptionResponse>>.Ok(
                await _service.GetListAsync(query, cancellationToken), "Daftar resep berhasil diambil."));

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<OptPrescriptionOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Opt Prescription", Description = "Melihat resep Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Prescription.Resource, P.Read)]
        public async Task<IActionResult> GetOptions([FromQuery] Guid patientId, [FromQuery] OptPrescriptionType? prescriptionType, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<List<OptPrescriptionOptionResponse>>.Ok(
                await _service.GetOptionsAsync(patientId, prescriptionType, cancellationToken), "Resep berlaku berhasil diambil."));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OptPrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Opt Prescription", Description = "Melihat resep Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Prescription.Resource, P.Read)]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken = default) =>
            OptHttp.Map(this, await _service.GetDetailAsync(id, cancellationToken), "Detail resep berhasil diambil.");

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OptPrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.Create, "Create Opt Prescription", Description = "Membuat resep internal atau menyalin resep luar", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Prescription.Resource, P.Create)]
        public async Task<IActionResult> Create(
            [FromBody] OptPrescriptionCreateRequest request,
            [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey,
            CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.CreateInternalAsync(request, OptHttp.ActorId(User), idempotencyKey, cancellationToken),
                "Resep tersimpan.", _logger, P.Prescription.Resource, P.Create,
                x => x.IsReplay
                    ? ReplayLog(x)
                    : $"{x.PrescriptionNumber} versi 1 dibuat oleh {x.Current?.AuthorName}{Override(x)}.");

        [HttpPost("external")]
        [ProducesResponseType(typeof(ApiResponse<OptPrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.Create, "Create Opt Prescription", Description = "Membuat resep internal atau menyalin resep luar", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Prescription.Resource, P.Create)]
        public async Task<IActionResult> CopyExternal(
            [FromBody] OptPrescriptionCopyExternalRequest request,
            [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey,
            CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.CopyExternalAsync(request, OptHttp.ActorId(User), idempotencyKey, cancellationToken),
                "Resep luar tersalin.", _logger, P.Prescription.Resource, P.Create,
                x => x.IsReplay
                    ? ReplayLog(x)
                    : $"{x.PrescriptionNumber} versi 1 disalin dari resep luar oleh {x.Current?.AuthorName}"
                        + (x.Current?.IsTransposed == true ? " (ditransposisi ke silinder minus)" : string.Empty) + $"{Override(x)}.");

        private static string ReplayLog(OptPrescriptionResponse x) =>
            $"Pembuatan resep {x.PrescriptionNumber} diulang dengan kunci yang sama; tidak ada resep baru.";

        [HttpPost("{id:guid}/revisions")]
        [ProducesResponseType(typeof(ApiResponse<OptPrescriptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.Prescription.Revise, "Revise Opt Prescription", Description = "Merevisi resep yang belum terkunci", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Prescription.Resource, P.Prescription.Revise)]
        public async Task<IActionResult> Revise(Guid id, [FromBody] OptPrescriptionReviseRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.ReviseAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                "Revisi resep tersimpan.", _logger, P.Prescription.Resource, P.Prescription.Revise,
                x => $"{x.PrescriptionNumber} versi {x.CurrentVersionNo} oleh {x.Current?.AuthorName}{Override(x)}.");

        private static string Override(OptPrescriptionResponse x) =>
            x.Current?.RangeOverrideReason is null ? string.Empty : " dengan konfirmasi ukuran di luar rentang";
    }
}
