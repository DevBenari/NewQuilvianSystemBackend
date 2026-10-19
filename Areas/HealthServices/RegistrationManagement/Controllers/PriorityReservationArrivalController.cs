using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    /// <summary>
    /// Halaman publik link konfirmasi kedatangan Reservasi Prioritas (rawat-jalan Amendment RP,
    /// <c>BE-RJP-012</c>). Satu-satunya endpoint bisnis tanpa login: pemegang link di pesan WA
    /// menekan "Saya sudah tiba" (<c>RJ-PRI-DEC-012</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sengaja tanpa <c>[AccessController]</c> agar tidak muncul di layar Akses Role. Dijaga kode acak
    /// 256 bit dan batas laju per IP (<c>PriorityReservationArrivalPublic</c>).
    /// </para>
    /// <para>
    /// <b>Kode ada di path, jadi controller ini tidak menulis log.</b> Setiap log yang ditulis selama
    /// permintaan ikut membawa path permintaan; jejaknya cukup riwayat reservasi (<c>ARRIVED</c> sumber
    /// <c>PatientLink</c>). Log permintaan HTTP disamarkan lewat <see cref="RedactRequestPath"/>.
    /// </para>
    /// </remarks>
    [ApiController]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicy)]
    [Route(RoutePrefix)]
    [Tags("Health Services / Registration Management / Priority Reservation Arrival")]
    public class PriorityReservationArrivalController : ControllerBase
    {
        public const string RateLimitPolicy = "PriorityReservationArrivalPublic";
        public const string RoutePrefix = "api/v1/health-services/registration-management/priority-reservation-arrivals";

        public const string InvalidLinkMessage = "Link tidak berlaku. Silakan hubungi petugas pendaftaran.";
        public const string RateLimitedMessage = "Terlalu banyak percobaan. Coba lagi sebentar lagi.";

        private static readonly PathString RoutePrefixPath = new("/" + RoutePrefix);

        private readonly OutpatientPriorityReservationService _reservationService;
        private readonly QueueRealtimeService _queueRealtimeService;

        public PriorityReservationArrivalController(
            OutpatientPriorityReservationService reservationService,
            QueueRealtimeService queueRealtimeService)
        {
            _reservationService = reservationService;
            _queueRealtimeService = queueRealtimeService;
        }

        /// <summary>Keadaan link: belum bisa dikonfirmasi, siap dikonfirmasi, atau sudah dikonfirmasi.</summary>
        /// <remarks>Kode salah, sudah diganti, atau reservasinya tidak berlaku dijawab 404 yang sama.</remarks>
        [HttpGet("{code}")]
        [ProducesResponseType(typeof(ApiResponse<PriorityReservationArrivalPublicResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GetState(string code, CancellationToken cancellationToken = default)
        {
            MarkNoStore();
            var state = await _reservationService.GetArrivalStateAsync(code, cancellationToken);

            return state == null
                ? InvalidLink()
                : Ok(ApiResponse<PriorityReservationArrivalPublicResponse>.Ok(state, state.Message));
        }

        /// <summary>Pasien menekan "Saya sudah tiba".</summary>
        /// <remarks>
        /// Hanya mengubah status pada hari kunjungan. Sebelum hari itu jawabannya <c>NOT_YET_OPEN</c>.
        /// Konfirmasi berikutnya menjawab <c>CONFIRMED</c> dengan jam pertama.
        /// </remarks>
        [HttpPost("{code}/confirm")]
        [ProducesResponseType(typeof(ApiResponse<PriorityReservationArrivalPublicResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Confirm(string code, CancellationToken cancellationToken = default)
        {
            MarkNoStore();
            var result = await _reservationService.ConfirmArrivalByCodeAsync(code, cancellationToken);

            if (result == null)
                return InvalidLink();

            if (result.Changed is RegPriorityReservation changed)
            {
                await _queueRealtimeService.NotifyPriorityReservationChangedAsync(
                    changed,
                    RegPriorityReservationHistory.EventArrived,
                    changed.ArrivedAt ?? DateTime.UtcNow);
            }

            return Ok(ApiResponse<PriorityReservationArrivalPublicResponse>.Ok(result.Response, result.Response.Message));
        }

        /// <summary>
        /// Menyamarkan kode pada path permintaan sesudah pipeline selesai, sebelum log permintaan HTTP
        /// ditulis (<c>INV-RP-05</c>). Contoh: <c>/…/priority-reservation-arrivals/Abc…/confirm</c>
        /// menjadi <c>/…/priority-reservation-arrivals/{code}/confirm</c>.
        /// </summary>
        public static void RedactRequestPath(HttpContext httpContext)
        {
            if (!httpContext.Request.Path.StartsWithSegments(RoutePrefixPath, StringComparison.OrdinalIgnoreCase, out var remaining) ||
                !remaining.HasValue)
            {
                return;
            }

            var segments = remaining.Value!.Trim('/').Split('/');
            var suffix = segments.Length > 1 ? "/" + string.Join('/', segments.Skip(1)) : string.Empty;

            httpContext.Request.Path = RoutePrefixPath.Add(new PathString("/{code}" + suffix));
        }

        private IActionResult InvalidLink() =>
            NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, InvalidLinkMessage));

        private void MarkNoStore() => Response.Headers.CacheControl = "no-store";
    }
}
