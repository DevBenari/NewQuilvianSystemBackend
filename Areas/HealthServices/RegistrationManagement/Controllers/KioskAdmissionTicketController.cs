using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    /// <summary>
    /// Menu Cetak Antrean pada kiosk reguler (rawat-jalan Cetak Antrean Admisi v1.1 bagian 8.1).
    /// Tiket admisi anonim: tidak meminta NIK, No. RM, penjamin, poli, maupun dokter, dan tidak
    /// pernah memuat nomor antrean poliklinik internal.
    /// </summary>
    /// <remarks>
    /// <b>Sengaja tanpa <c>[AccessAction]</c> dan <c>[AccessPermission]</c></b>, mengikuti
    /// <see cref="KioskPatientLookupController"/> (<c>KSK-DSN-006</c>): pemanggilnya akun
    /// perangkat Kiosk yang tidak punya Departemen × Posisi. Policy
    /// <see cref="AuthorizationPolicies.KioskRead"/> adalah otorisasi alternatif yang disetujui.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/kiosk-admission-tickets")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Kiosk Admission Ticket",
        AreaName = "HealthServices",
        ControllerName = "KioskAdmissionTicket",
        Description = "Cetak tiket antrean admisi rawat jalan dari kiosk",
        SortOrder = 11
    )]
    [Tags("Health Services / Registration Management / Kiosk Admission Ticket")]
    public class KioskAdmissionTicketController : ControllerBase
    {
        private readonly AdmissionTicketService _ticketService;

        public KioskAdmissionTicketController(AdmissionTicketService ticketService)
        {
            _ticketService = ticketService;
        }

        /// <summary>Kelompok antrean aktif; kiosk langsung mencetak bila hanya satu kelompok.</summary>
        [HttpGet("config")]
        [Authorize(Policy = AuthorizationPolicies.KioskRead)]
        [ProducesResponseType(typeof(ApiResponse<KioskAdmissionQueueConfigResponse>), StatusCodes.Status200OK)]
        public IActionResult GetConfig()
        {
            return Ok(ApiResponse<KioskAdmissionQueueConfigResponse>.Ok(
                _ticketService.GetKioskConfig(),
                "Konfigurasi cetak antrean admisi berhasil diambil."));
        }

        /// <summary>
        /// Menerbitkan tiket admisi. Ketukan ganda atau request ulang dengan
        /// <c>idempotencyKey</c> yang sama memulangkan tiket yang sama.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.KioskRead)]
        [ProducesResponseType(typeof(ApiResponse<KioskAdmissionTicketResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Issue(
            [FromBody] KioskAdmissionTicketIssueRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _ticketService.IssueAsync(request, GetCurrentUserId(), cancellationToken);

            if (!result.IsSuccess)
                return MapFailure(result);

            return Ok(ApiResponse<KioskAdmissionTicketResponse>.Ok(
                _ticketService.MapKioskTicket(result.Ticket!, result.IsExisting),
                result.IsExisting ? "Tiket antrean admisi sudah diterbitkan sebelumnya." : "Tiket antrean admisi berhasil diterbitkan."));
        }

        /// <summary>Cetak ulang tiket yang sama setelah printer gagal; nomor tidak berubah.</summary>
        [HttpPost("{id:guid}/reprint")]
        [Authorize(Policy = AuthorizationPolicies.KioskRead)]
        [ProducesResponseType(typeof(ApiResponse<KioskAdmissionTicketResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Reprint(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _ticketService.ReprintAsync(id, GetCurrentUserId(), cancellationToken);

            if (!result.IsSuccess)
                return MapFailure(result);

            return Ok(ApiResponse<KioskAdmissionTicketResponse>.Ok(
                _ticketService.MapKioskTicket(result.Ticket!, isExisting: true),
                "Tiket antrean admisi siap dicetak ulang."));
        }

        /// <summary>Hasil cetak dari kiosk; status cetak terpisah dari status pelayanan.</summary>
        [HttpPost("{id:guid}/print-result")]
        [Authorize(Policy = AuthorizationPolicies.KioskRead)]
        [ProducesResponseType(typeof(ApiResponse<KioskAdmissionTicketResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReportPrintResult(
            Guid id,
            [FromBody] KioskAdmissionTicketPrintResultRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _ticketService.ReportPrintResultAsync(id, GetCurrentUserId(), request, cancellationToken);

            if (!result.IsSuccess)
                return MapFailure(result);

            return Ok(ApiResponse<KioskAdmissionTicketResponse>.Ok(
                _ticketService.MapKioskTicket(result.Ticket!, isExisting: true),
                "Hasil cetak tiket antrean admisi tersimpan."));
        }

        private IActionResult MapFailure(AdmissionTicketCommandResult result)
            => result.Outcome switch
            {
                AdmissionTicketOutcome.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),

                AdmissionTicketOutcome.Conflict => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message)),

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
