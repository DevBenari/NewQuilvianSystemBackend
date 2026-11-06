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
    /// Pesanan Optik — <c>OPT-API-v1</c> grup Opt Order: daftar, detail, status bayar, buat dan ubah draf, simpan &amp; tagihkan,
    /// batal sebelum pengerjaan (<c>BE-OPT-032</c>), kirim ke pengerjaan dan tandai siap (<c>BE-OPT-034</c>), penyaring Kepala Optik
    /// dan perkiraan harga draf (<c>BE-OPT-054</c>, <c>OPT-API-v1.1</c>). Serah dan batal
    /// sesudah pengerjaan menyusul.
    /// </summary>
    /// <remarks>Kalimat log hanya memuat nomor pesanan dan status. Action sengaja tanpa <c>&lt;summary&gt;</c> XML.</remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/optical-management/orders")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Opt Order",
        AreaName = P.AreaName,
        ControllerName = P.Order.Resource,
        Description = "Pesanan kacamata: draf, tagihan per barang, uang muka, pengerjaan, serah, batal",
        SortOrder = 5
    )]
    [Tags("Health Services / Optical Management / Opt Order")]
    public class OptOrderController : ControllerBase
    {
        private readonly OptOrderService _service;
        private readonly LoggerService _logger;

        public OptOrderController(OptOrderService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OptOrderListItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Opt Order", Description = "Melihat pesanan Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Order.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] OptOrderListQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<OptOrderListItemResponse>>.Ok(
                await _service.GetListAsync(query, cancellationToken), "Daftar pesanan berhasil diambil."));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OptOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Opt Order", Description = "Melihat pesanan Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Order.Resource, P.Read)]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken = default) =>
            OptHttp.Map(this, await _service.GetDetailAsync(id, cancellationToken), "Detail pesanan berhasil diambil.");

        [HttpGet("{id:guid}/payment-status")]
        [ProducesResponseType(typeof(ApiResponse<OptOrderPaymentStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.Read, "Read Opt Order", Description = "Melihat pesanan Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Order.Resource, P.Read)]
        public async Task<IActionResult> GetPaymentStatus(Guid id, CancellationToken cancellationToken = default) =>
            OptHttp.Map(this, await _service.GetPaymentStatusAsync(id, cancellationToken), "Status pembayaran pesanan berhasil diambil.");

        [HttpGet("{id:guid}/price-preview")]
        [ProducesResponseType(typeof(ApiResponse<OptOrderPricePreviewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.Read, "Read Opt Order", Description = "Melihat pesanan Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Order.Resource, P.Read)]
        public async Task<IActionResult> GetPricePreview(Guid id, CancellationToken cancellationToken = default) =>
            OptHttp.Map(this, await _service.GetPricePreviewAsync(id, cancellationToken), "Perkiraan harga pesanan berhasil diambil.");

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OptOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Create, "Create Opt Order", Description = "Membuat draf pesanan Optik", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Order.Resource, P.Create)]
        public async Task<IActionResult> Create(
            [FromBody] OptOrderCreateRequest request,
            [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey,
            CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.CreateAsync(request, OptHttp.ActorId(User), idempotencyKey, cancellationToken),
                "Draf pesanan tersimpan.", _logger, P.Order.Resource, P.Create,
                x => x.IsReplay
                    ? $"Pembuatan pesanan {x.OrderNumber} diulang dengan kunci yang sama; tidak ada pesanan baru."
                    : $"{x.OrderNumber} dibuat sebagai draf berisi {x.ItemCount} barang.");

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OptOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Opt Order", Description = "Mengubah barang pada draf pesanan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Order.Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] OptOrderUpdateRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.UpdateAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                "Draf pesanan diperbarui.", _logger, P.Order.Resource, P.Update,
                x => $"{x.OrderNumber} diubah menjadi {x.ItemCount} barang.");

        [HttpPost("{id:guid}/bill")]
        [ProducesResponseType(typeof(ApiResponse<OptOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.Order.Bill, "Bill Opt Order", Description = "Menyimpan dan menagihkan pesanan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.Order.Resource, P.Order.Bill)]
        public async Task<IActionResult> Bill(Guid id, [FromBody] OptOrderBillRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.BillAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                "Pesanan ditagihkan.", _logger, P.Order.Resource, P.Order.Bill,
                x => $"{x.OrderNumber} ditagihkan ({x.ItemCount} barang), status {x.StatusLabel}.");

        [HttpPost("{id:guid}/send-to-work")]
        [ProducesResponseType(typeof(ApiResponse<OptOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.Order.SendToWork, "Send Opt Order To Work", Description = "Mengirim pesanan ke pengerjaan: cek uang muka, kunci resep, potong stok", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission(P.Order.Resource, P.Order.SendToWork)]
        public async Task<IActionResult> SendToWork(Guid id, [FromBody] OptOrderSendToWorkRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.SendToWorkAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                "Pesanan dikirim ke pengerjaan.", _logger, P.Order.Resource, P.Order.SendToWork,
                x => $"{x.OrderNumber} dikirim ke {x.WorkshopTypeLabel}, perkiraan jadi {x.EstimatedReadyDate:yyyy-MM-dd}.");

        [HttpPost("{id:guid}/mark-ready")]
        [ProducesResponseType(typeof(ApiResponse<OptOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Order.MarkReady, "Mark Opt Order Ready", Description = "Menandai pesanan siap diambil", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission(P.Order.Resource, P.Order.MarkReady)]
        public async Task<IActionResult> MarkReady(Guid id, [FromBody] OptOrderMarkReadyRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.MarkReadyAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                "Pesanan siap diambil.", _logger, P.Order.Resource, P.Order.MarkReady,
                x => $"{x.OrderNumber} siap diambil.");

        [HttpPost("{id:guid}/hand-over")]
        [ProducesResponseType(typeof(ApiResponse<OptOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.Order.HandOver, "Hand Over Opt Order", Description = "Menyerahkan pesanan: lunas, identitas dicocokkan, garansi dibekukan", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission(P.Order.Resource, P.Order.HandOver)]
        public async Task<IActionResult> HandOver(Guid id, [FromBody] OptOrderHandOverRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.HandOverAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                "Pesanan diserahkan.", _logger, P.Order.Resource, P.Order.HandOver,
                x => $"{x.OrderNumber} diserahkan" + (x.HandoverReceiverName is null ? " kepada pasien" : " kepada wakil") + (x.InvoiceFinalized == true ? "; tagihan kunjungan difinalisasi." : "."));

        [HttpPost("{id:guid}/cancellation-requests")]
        [ProducesResponseType(typeof(ApiResponse<OptOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Order.RequestCancel, "Request Cancel Opt Order", Description = "Mengajukan batal pesanan sesudah pengerjaan, dengan tanda lensa sudah dipotong", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission(P.Order.Resource, P.Order.RequestCancel)]
        public async Task<IActionResult> RequestCancellation(Guid id, [FromBody] OptOrderCancellationCreateRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.RequestCancellationAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                "Pengajuan batal tersimpan dan menunggu keputusan Kepala Optik.", _logger, P.Order.Resource, P.Order.RequestCancel,
                x => $"{x.OrderNumber} diajukan batal sesudah pengerjaan.");

        [HttpPost("{id:guid}/cancellation-requests/{requestId:guid}/decision")]
        [ProducesResponseType(typeof(ApiResponse<OptOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Order.DecideCancel, "Decide Cancel Opt Order", Description = "Menyetujui atau menolak pengajuan batal sesudah pengerjaan (Kepala Optik)", AccessType = AccessTypes.Update, SortOrder = 10)]
        [AccessPermission(P.Order.Resource, P.Order.DecideCancel)]
        public async Task<IActionResult> DecideCancellation(Guid id, Guid requestId, [FromBody] OptOrderCancelDecisionRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.DecideCancellationAsync(id, requestId, request, OptHttp.ActorId(User), cancellationToken),
                request.Approve ? "Pembatalan disetujui." : "Pembatalan ditolak.", _logger, P.Order.Resource, P.Order.DecideCancel,
                x => $"{x.OrderNumber}: pengajuan batal {(request.Approve ? "disetujui" : "ditolak")}, status {x.StatusLabel}.");

        [HttpPost("{id:guid}/close-not-collected")]
        [ProducesResponseType(typeof(ApiResponse<OptOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Order.CloseNotCollected, "Close Opt Order Not Collected", Description = "Menutup pesanan yang tidak diambil sesudah batas hari (Kepala Optik)", AccessType = AccessTypes.Update, SortOrder = 11)]
        [AccessPermission(P.Order.Resource, P.Order.CloseNotCollected)]
        public async Task<IActionResult> CloseNotCollected(Guid id, [FromBody] OptOrderNotCollectedRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.CloseNotCollectedAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                "Pesanan ditutup sebagai tidak diambil.", _logger, P.Order.Resource, P.Order.CloseNotCollected,
                x => $"{x.OrderNumber} ditutup sebagai tidak diambil.");

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<OptOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Order.Cancel, "Cancel Opt Order", Description = "Membatalkan pesanan sebelum pengerjaan", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.Order.Resource, P.Order.Cancel)]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] OptOrderCancelRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.CancelAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                "Pesanan dibatalkan.", _logger, P.Order.Resource, P.Order.Cancel,
                x => $"{x.OrderNumber} dibatalkan sebelum pengerjaan.");
    }
}
