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
    /// Permintaan HD masuk dari bangsal, poliklinik, atau IGD — aggregate ber-lifecycle
    /// (<c>BE-HMD-07</c>).
    /// </summary>
    /// <remarks>
    /// Bentuk transaksi: tanpa <c>GET /options</c>, tanpa <c>PATCH /{id}/status</c> generik, tanpa
    /// <c>DELETE</c>. Setiap perpindahan status punya perintah <c>POST /{id}/&lt;aksi&gt;</c>, dan
    /// pembatalan menyimpan alasannya.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/hemodialysis-orders")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Order",
        AreaName = P.AreaName,
        ControllerName = P.Order.Resource,
        Description = "Permintaan hemodialisa masuk dari unit peminta",
        SortOrder = 1
    )]
    [Tags("Health Services / Hemodialysis Management / Hemodialysis Order")]
    public class HmdOrderController : ControllerBase
    {
        private readonly HmdOrderService _service;
        private readonly LoggerService _logger;

        public HmdOrderController(HmdOrderService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<HmdOrderFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Order", Description = "Melihat konfigurasi penyaring permintaan HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Order.Resource, P.Read)]
        public IActionResult GetFilterMetadata() =>
            Ok(ApiResponse<HmdOrderFilterMetadataResponse>.Ok(
                HmdOrderService.BuildFilterMetadata(), "Metadata filter permintaan HD berhasil diambil."));

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<HmdOrderSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Order", Description = "Melihat ringkasan permintaan HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Order.Resource, P.Read)]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<HmdOrderSummaryResponse>.Ok(
                await _service.GetSummaryAsync(cancellationToken), "Ringkasan permintaan HD berhasil diambil."));

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdOrderResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Order", Description = "Melihat daftar permintaan HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Order.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] HmdOrderPagedQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<HmdOrderResponse>>.Ok(
                await _service.GetListAsync(query, cancellationToken), "Daftar permintaan HD berhasil diambil."));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Order", Description = "Melihat rincian permintaan HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Order.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetDetailAsync(id, HmdHttp.ActorId(User), cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Permintaan hemodialisa tidak ditemukan atau sudah dihapus."))
                : Ok(ApiResponse<HmdOrderDetailResponse>.Ok(result, "Rincian permintaan HD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<HmdOrderDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Create, "Create Hemodialysis Order", Description = "Membuat permintaan HD dari unit peminta", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Order.Resource, P.Create)]
        public async Task<IActionResult> Create([FromBody] CreateHmdOrderRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateAsync(request, HmdHttp.Actor(this), cancellationToken),
                "Permintaan hemodialisa berhasil dikirim.", _logger, P.Order.Resource, P.Create,
                x => new { x.Id, x.OrderNumber, x.PatientId, x.EncounterId, x.Priority });

        [HttpPost("{id:guid}/accept")]
        [ProducesResponseType(typeof(ApiResponse<HmdOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Order.Accept, "Accept Hemodialysis Order", Description = "Koordinator menerima permintaan HD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Order.Resource, P.Order.Accept)]
        public async Task<IActionResult> Accept(Guid id, [FromBody] AcceptHmdOrderRequest? request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.AcceptAsync(id, request ?? new AcceptHmdOrderRequest(), HmdHttp.Actor(this), cancellationToken),
                "Permintaan hemodialisa berhasil diterima.", _logger, P.Order.Resource, P.Order.Accept,
                x => new { x.Id, x.OrderNumber, x.OrderStatus });

        [HttpPost("{id:guid}/hold")]
        [ProducesResponseType(typeof(ApiResponse<HmdOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Order.Hold, "Hold Hemodialysis Order", Description = "Menahan permintaan HD karena alasan operasional", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.Order.Resource, P.Order.Hold)]
        public async Task<IActionResult> Hold(Guid id, [FromBody] HoldHmdOrderRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.HoldAsync(id, request, HmdHttp.Actor(this), cancellationToken),
                "Permintaan hemodialisa berhasil ditahan.", _logger, P.Order.Resource, P.Order.Hold,
                x => new { x.Id, x.OrderNumber, x.OrderStatus });

        [HttpPost("{id:guid}/release-hold")]
        [ProducesResponseType(typeof(ApiResponse<HmdOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Order.Hold, "Hold Hemodialysis Order", Description = "Melepas penahanan permintaan HD", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.Order.Resource, P.Order.Hold)]
        public async Task<IActionResult> ReleaseHold(Guid id, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.ReleaseHoldAsync(id, HmdHttp.Actor(this), cancellationToken),
                "Penahanan permintaan hemodialisa berhasil dilepas.", _logger, P.Order.Resource, "ReleaseHold",
                x => new { x.Id, x.OrderNumber, x.OrderStatus });

        /// <summary>
        /// Dokter menolak permintaan dengan alasan klinis. Penolakan bersifat final; pemanggil yang
        /// tidak tertaut ke data dokter aktif ditolak <c>403</c>.
        /// </summary>
        [HttpPost("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<HmdOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Order.Reject, "Reject Hemodialysis Order", Description = "Dokter menolak permintaan HD dengan alasan klinis", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.Order.Resource, P.Order.Reject)]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectHmdOrderRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.RejectAsync(id, request, HmdHttp.Actor(this), cancellationToken),
                "Permintaan hemodialisa berhasil ditolak.", _logger, P.Order.Resource, P.Order.Reject,
                x => new { x.Id, x.OrderNumber, x.OrderStatus });

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<HmdOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Order.Cancel, "Cancel Hemodialysis Order", Description = "Pembuat membatalkan permintaan HD miliknya sendiri", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission(P.Order.Resource, P.Order.Cancel)]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelHmdOrderRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CancelAsync(id, request, HmdHttp.Actor(this), cancellationToken),
                "Permintaan hemodialisa berhasil dibatalkan.", _logger, P.Order.Resource, P.Order.Cancel,
                x => new { x.Id, x.OrderNumber, x.OrderStatus });
    }
}
