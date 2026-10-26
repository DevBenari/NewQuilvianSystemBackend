using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Constants.MedicalCheckupPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Controllers
{
    /// <summary>
    /// Order MCU — <c>MCU-API-v1</c> grup Mcu Order. <c>BE-MCU-010</c> membangun bagian baca: Daftar
    /// Pasien MCU, ringkasan, detail, dan riwayat. Booking, jadwal ulang, dan batal menyusul di
    /// <c>BE-MCU-011</c>.
    /// </summary>
    /// <remarks>
    /// Action sengaja tanpa <c>&lt;summary&gt;</c> XML. <c>filters/metadata</c> dan <c>summary</c>
    /// dideklarasikan tepat sesudah konstruktor.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-checkup-management/orders")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Mcu Order",
        AreaName = P.AreaName,
        ControllerName = P.Order.Resource,
        Description = "Order MCU: Daftar Pasien MCU, detail, booking, jadwal ulang, dan batal",
        SortOrder = 4
    )]
    [Tags("Health Services / Medical Checkup Management / Mcu Order")]
    public class McuOrderController : ControllerBase
    {
        private readonly McuOrderQueryService _queryService;
        private readonly McuBookingService _bookingService;
        private readonly McuExaminationService _examinationService;
        private readonly LoggerService _logger;

        public McuOrderController(
            McuOrderQueryService queryService,
            McuBookingService bookingService,
            McuExaminationService examinationService,
            LoggerService logger)
        {
            _queryService = queryService;
            _bookingService = bookingService;
            _examinationService = examinationService;
            _logger = logger;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<McuOrderFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Mcu Order", Description = "Melihat konfigurasi penyaring Daftar Pasien MCU", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Order.Resource, P.Read)]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<McuOrderFilterMetadataResponse>.Ok(
                await _queryService.GetFilterMetadataAsync(cancellationToken), "Metadata filter Daftar Pasien MCU berhasil diambil."));

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<McuOrderSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Mcu Order", Description = "Melihat ringkasan jumlah order MCU per status untuk satu tanggal", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Order.Resource, P.Read)]
        public async Task<IActionResult> GetSummary([FromQuery] McuOrderSummaryQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<McuOrderSummaryResponse>.Ok(
                await _queryService.GetSummaryAsync(query, cancellationToken), "Ringkasan order MCU berhasil diambil."));

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<McuOrderListResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Mcu Order", Description = "Melihat Daftar Pasien MCU", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Order.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] McuOrderPagedQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<McuOrderListResponse>>.Ok(
                await _queryService.GetOrdersAsync(query, cancellationToken), "Daftar Pasien MCU berhasil diambil."));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<McuOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Mcu Order", Description = "Melihat rincian order MCU", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Order.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _queryService.GetOrderAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Order MCU tidak ditemukan."))
                : Ok(ApiResponse<McuOrderDetailResponse>.Ok(result, "Rincian order MCU berhasil diambil."));
        }

        [HttpGet("{id:guid}/history")]
        [ProducesResponseType(typeof(ApiResponse<List<McuOrderHistoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Mcu Order", Description = "Melihat riwayat status dan dokter MCU order", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Order.Resource, P.Read)]
        public async Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken = default) =>
            McuHttp.Map(this, await _queryService.GetHistoryAsync(id, cancellationToken), "Riwayat order MCU berhasil diambil.");

        [HttpPost("bookings")]
        [ProducesResponseType(typeof(ApiResponse<McuOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Create, "Create Mcu Order", Description = "Mencatat booking MCU dari telepon/WhatsApp", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Order.Resource, P.Create)]
        public async Task<IActionResult> CreateBooking([FromBody] CreateMcuBookingRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _bookingService.CreateBookingAsync(request, McuHttp.ActorId(User), cancellationToken),
                "Booking MCU berhasil disimpan.", _logger, P.Order.Resource, P.Create,
                x => $"Booking MCU {x.OrderNumber} dibuat untuk {McuScheduleService.FormatDate(x.ExaminationDate)} " +
                     $"({x.SourceLabel}, {(x.PatientId.HasValue ? "pasien terdaftar" : "identitas sementara")}).");

        [HttpPost("walk-ins")]
        [ProducesResponseType(typeof(ApiResponse<McuOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Create, "Create Mcu Order", Description = "Mendaftarkan peserta MCU walk-in dan langsung check-in", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Order.Resource, P.Create)]
        public async Task<IActionResult> CreateWalkIn([FromBody] CreateMcuWalkInRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _bookingService.CreateWalkInAsync(request, McuHttp.ActorId(User), cancellationToken),
                "Walk-in MCU berhasil didaftarkan dan check-in.", _logger, P.Order.Resource, P.Create,
                x => $"Walk-in MCU {x.OrderNumber} didaftarkan dan check-in: kunjungan {x.EncounterNumber}, " +
                     $"dokter MCU {x.McuDoctorName}.");

        [HttpPatch("{id:guid}/reschedule")]
        [ProducesResponseType(typeof(ApiResponse<McuOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Mcu Order", Description = "Menjadwal ulang order MCU Terjadwal atau Tidak Hadir", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Order.Resource, P.Update)]
        public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleMcuOrderRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _bookingService.RescheduleAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Order MCU berhasil dijadwal ulang.", _logger, P.Order.Resource, P.Update,
                x => $"Order MCU {x.OrderNumber} dijadwal ulang ke {McuScheduleService.FormatDate(x.ExaminationDate)}.");

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<McuOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Order.Cancel, "Cancel Mcu Order", Description = "Membatalkan order MCU beserta alasannya", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.Order.Resource, P.Order.Cancel)]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelMcuOrderRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _examinationService.CancelOrderAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Order MCU berhasil dibatalkan.", _logger, P.Order.Resource, P.Order.Cancel,
                // Alasan pembatalan tidak dicatat di log aplikasi.
                x => x.EncounterNumber == null
                    ? $"Order MCU {x.OrderNumber} dibatalkan sebelum datang; kuota {McuScheduleService.FormatDate(x.ExaminationDate)} dilepas."
                    : $"Order MCU {x.OrderNumber} dibatalkan penuh sesudah check-in; kunjungan {x.EncounterNumber} dibatalkan, baris paket dikembalikan utuh.");
    }
}
