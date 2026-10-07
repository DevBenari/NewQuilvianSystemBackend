using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers
{
    /// <summary>
    /// Pemesanan ruang bedah dari bangsal, tab Bedah Operasi dan Bedah Obgyn (<c>BE-RWI-175</c>,
    /// kontrak <c>0.10.0</c> API 11.2).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Hak akses <c>OperatingRoomCase : Create</c> sesuai kontrak.</b> Endpoint ini sengaja tidak
    /// memasang <c>[AccessAction]</c>: kemampuan <c>OperatingRoomCase : Create</c> sudah didaftarkan
    /// <c>OperatingRoomCaseController</c> pada modul Kamar Operasi, dan mendaftarkannya sekali lagi
    /// dari modul Rawat Inap membuat resource yang sama tercatat pada dua modul — gerbang integritas
    /// permission menolak startup karenanya. Pola ini adalah endpoint alias yang didukung
    /// <c>PermissionRegistryDescriptor</c>: kunci yang ditegakkan identik dengan kunci yang dicentang
    /// admin pada layar Akses Role.
    /// </para>
    /// <para>
    /// Pembuatan kasus tetap milik Kamar Operasi; controller ini hanya memanggil
    /// <see cref="InpSurgeryBookingAdapter"/> yang memvalidasi episode dan order lebih dulu.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/inpatient-management/episodes")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_INPATIENT",
        moduleName: "Health Service Inpatient",
        displayName: "Inpatient Surgery Booking",
        AreaName = "HealthServices",
        ControllerName = "InpatientSurgeryBooking",
        Description = "Pemesanan ruang bedah dari bangsal yang merujuk satu order tindakan operasi",
        SortOrder = 17
    )]
    [Tags("Health Services / Inpatient Management / Inpatient Surgery Booking")]
    public class InpatientSurgeryBookingController : ControllerBase
    {
        private const string LogCategory = "HealthServices.InPatientManagement.SurgeryBooking";

        private readonly InpSurgeryBookingAdapter _adapter;
        private readonly LoggerService _loggerService;

        public InpatientSurgeryBookingController(InpSurgeryBookingAdapter adapter, LoggerService loggerService)
        {
            _adapter = adapter;
            _loggerService = loggerService;
        }

        /// <summary>Memesan ruang bedah; membuat kasus OK berstatus Diminta.</summary>
        /// <remarks>
        /// Order tidak aktif atau bukan milik kunjungan episode → 422 <c>INP-SRG-001</c>; episode bukan
        /// Admitted → 422 <c>INP-SRG-002</c>; kunci idempotensi sama dengan isi berbeda → 409; isi sama
        /// → kasus yang sama dikembalikan tanpa membuat kasus kedua.
        /// </remarks>
        [HttpPost("{episodeId:guid}/surgery-bookings")]
        [ProducesResponseType(typeof(ApiResponse<OprCaseDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessPermission("OperatingRoomCase", "Create")]
        public async Task<IActionResult> Book(
            Guid episodeId,
            [FromBody] SurgeryBookingRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            var result = await _adapter.BookAsync(episodeId, request, idempotencyKey, cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                var statusCode = result.Status switch
                {
                    InpEpisodeOperationStatus.Invalid => StatusCodes.Status400BadRequest,
                    InpEpisodeOperationStatus.Forbidden => StatusCodes.Status403Forbidden,
                    InpEpisodeOperationStatus.NotFound => StatusCodes.Status404NotFound,
                    InpEpisodeOperationStatus.Conflict => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status422UnprocessableEntity
                };

                return StatusCode(statusCode, ApiResponse<object>.Fail(
                    statusCode,
                    result.Message,
                    result.Code != null ? new { result.Code } : null));
            }

            // Indikasi dan catatan adalah isian klinis; tidak ikut ke log.
            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientSurgeryBooking.Book",
                "Memesan ruang bedah dari bangsal.",
                new
                {
                    EpisodeId = episodeId,
                    CaseId = result.Case!.Id,
                    result.Case.CaseNumber,
                    BookingTab = request.BookingTab,
                    ActorUserId = User.GetUserId(),
                    Controller = "InpatientSurgeryBooking",
                    Action = "Book",
                    StatusCode = StatusCodes.Status201Created
                });

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<OprCaseDetailResponse>.Ok(result.Case, result.Message));
        }
    }
}
