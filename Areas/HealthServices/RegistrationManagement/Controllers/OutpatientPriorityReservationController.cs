using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using PriorityReservationPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs.PriorityReservationListItemResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    /// <summary>
    /// Reservasi Pasien Prioritas Rawat Jalan (rawat-jalan Amendment RP, kontrak
    /// <c>RJ-PRI-RESV-001@1.0.0</c>). Admisi membuat reservasi untuk member Aktif yang menelepon
    /// atau mengirim WA, tanpa memilih poli dan dokter. Poli dan dokter dilengkapi saat pasien tiba.
    /// </summary>
    /// <remarks>
    /// Transaksi ber-lifecycle: perpindahan status lewat <c>POST /{id}/&lt;aksi&gt;</c>, pembatalan
    /// lewat <c>cancel</c>. Tidak ada <c>PUT</c>, <c>PATCH /status</c>, maupun <c>DELETE</c>. Controller
    /// tidak menyentuh <c>ApplicationDbContext</c>; seluruh aturan ada di service.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/outpatient-priority-reservations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Outpatient Priority Reservation",
        AreaName = "HealthServices",
        ControllerName = "OutpatientPriorityReservation",
        Description = "Reservasi pasien prioritas rawat jalan sebelum poli dan dokter dipilih",
        SortOrder = 9
    )]
    [Tags("Health Services / Registration Management / Outpatient Priority Reservation")]
    public class OutpatientPriorityReservationController : ControllerBase
    {
        private const string LogCategory = "HealthServices.RegistrationManagement.OutpatientPriorityReservation";
        private const string ControllerName = "OutpatientPriorityReservation";
        private const string NotFoundMessage = "Reservasi prioritas tidak ditemukan.";

        private readonly OutpatientPriorityReservationService _reservationService;
        private readonly QueueRealtimeService _queueRealtimeService;
        private readonly LoggerService _loggerService;

        public OutpatientPriorityReservationController(
            OutpatientPriorityReservationService reservationService,
            QueueRealtimeService queueRealtimeService,
            LoggerService loggerService)
        {
            _reservationService = reservationService;
            _queueRealtimeService = queueRealtimeService;
            _loggerService = loggerService;
        }

        /// <summary>Daftar reservasi pada rentang tanggal kunjungan (bawaan hari ini).</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PriorityReservationPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Outpatient Priority Reservation", Description = "Melihat reservasi pasien prioritas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OutpatientPriorityReservation", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] PriorityReservationListQuery query,
            CancellationToken cancellationToken = default)
        {
            var (result, error) = await _reservationService.GetPagedAsync(query, cancellationToken);

            if (error != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, error));

            return Ok(ApiResponse<PriorityReservationPagedResult>.Ok(
                result,
                "Daftar reservasi prioritas berhasil diambil."));
        }

        /// <summary>
        /// Jumlah reservasi per status pada rentang tanggal kunjungan (bawaan hari ini).
        /// <c>visitDate</c> tunggal dari kontrak <c>1.0.0</c> tetap diterima.
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PriorityReservationSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Outpatient Priority Reservation", Description = "Melihat reservasi pasien prioritas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OutpatientPriorityReservation", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateTime? visitDate,
            [FromQuery] DateTime? visitDateFrom,
            [FromQuery] DateTime? visitDateTo,
            CancellationToken cancellationToken = default)
        {
            var (result, error) = await _reservationService.GetSummaryAsync(
                visitDateFrom ?? visitDate,
                visitDateTo ?? visitDateFrom ?? visitDate,
                cancellationToken);

            if (error != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, error));

            return Ok(ApiResponse<PriorityReservationSummaryResponse>.Ok(
                result,
                "Ringkasan reservasi prioritas berhasil diambil."));
        }

        /// <summary>Cek apakah pasien boleh dibuatkan reservasi prioritas pada tanggal itu.</summary>
        [HttpGet("eligibility")]
        [ProducesResponseType(typeof(ApiResponse<PriorityReservationEligibilityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Outpatient Priority Reservation", Description = "Melihat reservasi pasien prioritas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OutpatientPriorityReservation", "Read")]
        public async Task<IActionResult> GetEligibility(
            [FromQuery] Guid patientId,
            [FromQuery] DateTime? visitDate,
            CancellationToken cancellationToken = default)
        {
            var result = await _reservationService.CheckEligibilityAsync(patientId, visitDate, cancellationToken);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    OutpatientPriorityReservationService.PatientNotFoundMessage));
            }

            return Ok(ApiResponse<PriorityReservationEligibilityResponse>.Ok(result, result.Message));
        }

        /// <summary>Detail reservasi beserta riwayatnya.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PriorityReservationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Outpatient Priority Reservation", Description = "Melihat reservasi pasien prioritas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OutpatientPriorityReservation", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var detail = await _reservationService.GetDetailAsync(id, cancellationToken);

            if (detail == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, NotFoundMessage));

            return Ok(ApiResponse<PriorityReservationDetailResponse>.Ok(
                detail,
                "Detail reservasi prioritas berhasil diambil."));
        }

        /// <summary>Membuat reservasi prioritas untuk member Aktif hari ini.</summary>
        /// <remarks>
        /// Respons memuat link konfirmasi kedatangan <b>sekali saja</b>, teks WA siap kirim tanpa poli
        /// dan dokter, serta No. HP pasien dari master untuk tombol <c>wa.me</c>. Link tidak dapat
        /// diambil ulang; bila hilang, gunakan kirim ulang.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PriorityReservationIssuedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Outpatient Priority Reservation", Description = "Membuat reservasi pasien prioritas dan mengirim ulang link", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("OutpatientPriorityReservation", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreatePriorityReservationRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _reservationService.CreateAsync(request, GetCurrentUserId(), cancellationToken);

            if (!result.IsSuccess)
                return MapFailure(result);

            await LogCommandAsync("OutpatientPriorityReservation.Create", "Membuat reservasi prioritas.", result, "Create");
            await NotifyBoardAsync(result);

            return Ok(ApiResponse<PriorityReservationIssuedResponse>.Ok(
                await _reservationService.BuildIssuedResponseAsync(result, cancellationToken),
                result.Message));
        }

        /// <summary>Kirim ulang link: kode baru terbit dan kode lama tidak berlaku lagi.</summary>
        /// <remarks>Hanya untuk reservasi Menunggu Kedatangan. Link baru hanya muncul di respons ini.</remarks>
        [HttpPost("{id:guid}/arrival-code")]
        [ProducesResponseType(typeof(ApiResponse<PriorityReservationIssuedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Outpatient Priority Reservation", Description = "Membuat reservasi pasien prioritas dan mengirim ulang link", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("OutpatientPriorityReservation", "Create")]
        public async Task<IActionResult> ReissueArrivalCode(
            Guid id,
            [FromBody] PriorityReservationRowVersionRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _reservationService.ReissueArrivalCodeAsync(id, request, GetCurrentUserId(), cancellationToken);

            if (!result.IsSuccess)
                return MapFailure(result);

            await LogCommandAsync("OutpatientPriorityReservation.ReissueArrivalCode", "Menerbitkan ulang link konfirmasi kedatangan.", result, "Create");
            await NotifyBoardAsync(result);

            return Ok(ApiResponse<PriorityReservationIssuedResponse>.Ok(
                await _reservationService.BuildIssuedResponseAsync(result, cancellationToken),
                result.Message));
        }

        /// <summary>Petugas menandai pasien sudah tiba. Hanya pada hari kunjungan.</summary>
        [HttpPost("{id:guid}/arrival")]
        [ProducesResponseType(typeof(ApiResponse<PriorityReservationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Complete", "Complete Outpatient Priority Reservation", Description = "Menandai tiba, mengembalikan ke Belum Tiba, dan melengkapi reservasi menjadi kunjungan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OutpatientPriorityReservation", "Complete")]
        public async Task<IActionResult> MarkArrival(
            Guid id,
            [FromBody] PriorityReservationRowVersionRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _reservationService.MarkArrivalAsync(id, request, GetCurrentUserId(), cancellationToken);

            return await RespondDetailAsync(result, "OutpatientPriorityReservation.MarkArrival", "Menandai pasien tiba.", "Complete", cancellationToken);
        }

        /// <summary>Mengembalikan reservasi ke Menunggu Kedatangan dengan alasan.</summary>
        /// <remarks>Link yang sama dapat dipakai lagi oleh pasien.</remarks>
        [HttpPost("{id:guid}/arrival-reversal")]
        [ProducesResponseType(typeof(ApiResponse<PriorityReservationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Complete", "Complete Outpatient Priority Reservation", Description = "Menandai tiba, mengembalikan ke Belum Tiba, dan melengkapi reservasi menjadi kunjungan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OutpatientPriorityReservation", "Complete")]
        public async Task<IActionResult> RevertArrival(
            Guid id,
            [FromBody] PriorityReservationReasonRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _reservationService.RevertArrivalAsync(id, request, GetCurrentUserId(), cancellationToken);

            return await RespondDetailAsync(result, "OutpatientPriorityReservation.RevertArrival", "Mengembalikan reservasi ke Menunggu Kedatangan.", "Complete", cancellationToken);
        }

        /// <summary>Menyimpan checklist skrining awal: batuk, demam, dan risiko jatuh.</summary>
        [HttpPut("{id:guid}/screening")]
        [ProducesResponseType(typeof(ApiResponse<PriorityReservationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Screen", "Screen Outpatient Priority Reservation", Description = "Menyimpan checklist skrining awal reservasi pasien prioritas", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OutpatientPriorityReservation", "Screen")]
        public async Task<IActionResult> SaveScreening(
            Guid id,
            [FromBody] PriorityReservationScreeningRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _reservationService.SaveScreeningAsync(id, request, GetCurrentUserId(), cancellationToken);

            return await RespondDetailAsync(result, "OutpatientPriorityReservation.SaveScreening", "Menyimpan skrining awal reservasi.", "Screen", cancellationToken);
        }

        /// <summary>Membatalkan reservasi terbuka dengan alasan.</summary>
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<PriorityReservationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Cancel", "Cancel Outpatient Priority Reservation", Description = "Membatalkan reservasi pasien prioritas", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("OutpatientPriorityReservation", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] PriorityReservationReasonRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _reservationService.CancelAsync(id, request, GetCurrentUserId(), cancellationToken);

            return await RespondDetailAsync(result, "OutpatientPriorityReservation.Cancel", "Membatalkan reservasi prioritas.", "Cancel", cancellationToken);
        }

        private async Task<IActionResult> RespondDetailAsync(
            PriorityReservationCommandResult result,
            string logAction,
            string logMessage,
            string accessAction,
            CancellationToken cancellationToken)
        {
            if (!result.IsSuccess)
                return MapFailure(result);

            await LogCommandAsync(logAction, logMessage, result, accessAction);
            await NotifyBoardAsync(result);

            return Ok(ApiResponse<PriorityReservationDetailResponse>.Ok(
                await _reservationService.GetDetailAsync(result.Reservation!.Id, cancellationToken),
                result.Message));
        }

        /// <summary>Kabar ke layar admisi (INT-RP-07), dikirim sesudah perubahan tersimpan.</summary>
        private Task NotifyBoardAsync(PriorityReservationCommandResult result)
            => _queueRealtimeService.NotifyPriorityReservationChangedAsync(
                result.Reservation!,
                result.EventCode,
                result.Reservation!.UpdateDateTime ?? result.Reservation.CreateDateTime);

        /// <summary>
        /// Log hanya berisi identitas, aksi, dan status — tanpa kode link, hash, No. HP, checklist,
        /// maupun alasan (permission-audit-matrix RP-4).
        /// </summary>
        private Task LogCommandAsync(string action, string message, PriorityReservationCommandResult result, string accessAction)
            => _loggerService.InfoAsync(
                LogCategory,
                action,
                message,
                new
                {
                    EntityId = result.Reservation!.Id,
                    result.Reservation.ReservationNumber,
                    Status = result.Reservation.Status.ToString(),
                    Controller = ControllerName,
                    Action = accessAction
                });

        private IActionResult MapFailure(PriorityReservationCommandResult result)
            => result.Outcome switch
            {
                PriorityReservationOutcome.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),

                PriorityReservationOutcome.Conflict => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message)),

                PriorityReservationOutcome.Forbidden => StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, result.Message)),

                _ => BadRequest(
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.Message))
            };

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
