using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using AdmissionTicketPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs.AdmissionTicketListItemResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    /// <summary>
    /// Daftar Antrean Admisi untuk petugas loket (rawat-jalan Cetak Antrean Admisi v1.1 bagian
    /// 8.2). Halaman tersendiri, terpisah dari Daftar Pasien Rawat Jalan dan Skrining Pasien.
    /// </summary>
    /// <remarks>
    /// Worklist transaksi: perpindahan status lewat <c>POST /{id}/&lt;aksi&gt;</c> dan
    /// <c>POST /call-next</c>; pembatalan lewat <c>cancel</c>. Tidak ada <c>PUT</c>,
    /// <c>PATCH /status</c>, maupun <c>DELETE</c>. Registrasi kunjungan memakai endpoint
    /// <c>patient-encounters/admin</c> yang sudah ada dengan <c>admissionTicketId</c>.
    /// Controller tidak menyentuh <c>ApplicationDbContext</c>.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/admission-queues")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Admission Queue",
        AreaName = "HealthServices",
        ControllerName = "AdmissionQueue",
        Description = "Daftar dan pemanggilan antrean admisi rawat jalan dari tiket kiosk",
        SortOrder = 10
    )]
    [Tags("Health Services / Registration Management / Admission Queue")]
    public class AdmissionQueueController : ControllerBase
    {
        private const string LogCategory = "HealthServices.RegistrationManagement.AdmissionQueue";
        private const string ControllerName = "AdmissionQueue";
        private const string NotFoundMessage = "Tiket antrean admisi tidak ditemukan.";

        private readonly AdmissionTicketService _ticketService;
        private readonly LoggerService _loggerService;

        public AdmissionQueueController(
            AdmissionTicketService ticketService,
            LoggerService loggerService)
        {
            _ticketService = ticketService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionQueueFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Admission Queue", Description = "Melihat daftar antrean admisi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AdmissionQueue", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<AdmissionQueueFilterMetadataResponse>.Ok(
                _ticketService.GetFilterMetadata(),
                "Metadata filter antrean admisi berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionQueueSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Admission Queue", Description = "Melihat ringkasan antrean admisi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AdmissionQueue", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateTime? serviceDate,
            [FromQuery] DateTime? serviceDateFrom,
            [FromQuery] DateTime? serviceDateTo,
            [FromQuery] string? queueGroupCode,
            CancellationToken cancellationToken = default)
        {
            return Ok(ApiResponse<AdmissionQueueSummaryResponse>.Ok(
                await _ticketService.GetSummaryAsync(serviceDate, serviceDateFrom, serviceDateTo, queueGroupCode, cancellationToken),
                "Ringkasan antrean admisi berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<AdmissionTicketPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Admission Queue", Description = "Melihat daftar antrean admisi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AdmissionQueue", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] AdmissionQueueListQuery query,
            CancellationToken cancellationToken = default)
        {
            return Ok(ApiResponse<AdmissionTicketPagedResult>.Ok(
                await _ticketService.GetPagedAsync(query, cancellationToken),
                "Daftar antrean admisi berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionTicketDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Admission Queue", Description = "Melihat detail tiket antrean admisi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AdmissionQueue", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var detail = await _ticketService.GetDetailAsync(id, cancellationToken);

            if (detail == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, NotFoundMessage));

            return Ok(ApiResponse<AdmissionTicketDetailResponse>.Ok(detail, "Detail tiket antrean admisi berhasil diambil."));
        }

        /// <summary>Panggil Berikutnya: klaim atomik tiket menunggu tertua untuk loket ini.</summary>
        [HttpPost("call-next")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionQueueCallNextResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Call", "Call Admission Queue", Description = "Memanggil dan memanggil ulang nomor antrean admisi", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("AdmissionQueue", "Call")]
        public async Task<IActionResult> CallNext(
            [FromBody] AdmissionQueueCallNextRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _ticketService.CallNextAsync(request, GetCurrentUserId(), cancellationToken);

            if (!result.IsSuccess)
                return MapFailure(result);

            if (result.Ticket != null)
                await LogCommandAsync("AdmissionQueue.CallNext", "Memanggil antrean admisi berikutnya.", result, "Call");

            return Ok(ApiResponse<AdmissionQueueCallNextResponse>.Ok(
                new AdmissionQueueCallNextResponse
                {
                    Ticket = result.Ticket == null ? null : _ticketService.MapListItem(result.Ticket),
                    Voice = result.Voice
                },
                result.Message));
        }

        /// <summary>Panggil satu tiket Menunggu yang dipilih dari daftar ke loket petugas.</summary>
        [HttpPost("{id:guid}/call")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionTicketActionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Call", "Call Admission Queue", Description = "Memanggil dan memanggil ulang nomor antrean admisi", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("AdmissionQueue", "Call")]
        public async Task<IActionResult> Call(
            Guid id,
            [FromBody] AdmissionTicketCounterRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _ticketService.CallAsync(id, request, GetCurrentUserId(), cancellationToken);
            return await RespondAsync(result, "AdmissionQueue.Call", "Memanggil antrean admisi.", "Call");
        }

        [HttpPost("{id:guid}/recall")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionTicketActionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Call", "Call Admission Queue", Description = "Memanggil dan memanggil ulang nomor antrean admisi", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("AdmissionQueue", "Call")]
        public async Task<IActionResult> Recall(
            Guid id,
            [FromBody] AdmissionTicketCounterRequest? request,
            CancellationToken cancellationToken = default)
        {
            var result = await _ticketService.RecallAsync(id, request ?? new AdmissionTicketCounterRequest(), GetCurrentUserId(), cancellationToken);
            return await RespondAsync(result, "AdmissionQueue.Recall", "Memanggil ulang antrean admisi.", "Call");
        }

        [HttpPost("{id:guid}/start-service")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionTicketActionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Serve Admission Queue", Description = "Memulai, melewati, dan menyelesaikan pelayanan antrean admisi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AdmissionQueue", "Update")]
        public async Task<IActionResult> StartService(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _ticketService.StartServiceAsync(id, GetCurrentUserId(), cancellationToken);
            return await RespondAsync(result, "AdmissionQueue.StartService", "Memulai pelayanan antrean admisi.", "Update");
        }

        [HttpPost("{id:guid}/skip")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionTicketActionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Serve Admission Queue", Description = "Memulai, melewati, dan menyelesaikan pelayanan antrean admisi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AdmissionQueue", "Update")]
        public async Task<IActionResult> Skip(
            Guid id,
            [FromBody] AdmissionTicketReasonRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _ticketService.SkipAsync(id, request, GetCurrentUserId(), cancellationToken);
            return await RespondAsync(result, "AdmissionQueue.Skip", "Melewati antrean admisi.", "Update");
        }

        [HttpPost("{id:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionTicketActionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Serve Admission Queue", Description = "Memulai, melewati, dan menyelesaikan pelayanan antrean admisi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AdmissionQueue", "Update")]
        public async Task<IActionResult> Complete(
            Guid id,
            [FromBody] AdmissionTicketCompleteRequest? request,
            CancellationToken cancellationToken = default)
        {
            var result = await _ticketService.CompleteAsync(id, request ?? new AdmissionTicketCompleteRequest(), GetCurrentUserId(), cancellationToken);
            return await RespondAsync(result, "AdmissionQueue.Complete", "Menyelesaikan pelayanan antrean admisi.", "Update");
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionTicketActionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Cancel", "Cancel Admission Queue", Description = "Membatalkan tiket antrean admisi dengan alasan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("AdmissionQueue", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] AdmissionTicketReasonRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _ticketService.CancelAsync(id, request, GetCurrentUserId(), cancellationToken);
            return await RespondAsync(result, "AdmissionQueue.Cancel", "Membatalkan tiket antrean admisi.", "Cancel");
        }

        private async Task<IActionResult> RespondAsync(
            AdmissionTicketCommandResult result,
            string logAction,
            string logMessage,
            string accessAction)
        {
            if (!result.IsSuccess)
                return MapFailure(result);

            await LogCommandAsync(logAction, logMessage, result, accessAction);

            return Ok(ApiResponse<AdmissionTicketActionResponse>.Ok(
                new AdmissionTicketActionResponse
                {
                    Ticket = _ticketService.MapListItem(result.Ticket!),
                    Voice = result.Voice
                },
                string.IsNullOrWhiteSpace(result.Message) ? "Tiket antrean admisi berhasil diperbarui." : result.Message));
        }

        /// <summary>Log berisi identitas tiket, aksi, dan status — tanpa alasan maupun identitas pasien.</summary>
        private async Task LogCommandAsync(string action, string message, AdmissionTicketCommandResult result, string accessAction)
        {
            try
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    action,
                    message,
                    new
                    {
                        EntityId = result.Ticket!.Id,
                        result.Ticket.DisplayNumber,
                        Status = result.Ticket.Status.ToString(),
                        result.Ticket.CounterCode,
                        Controller = ControllerName,
                        Action = accessAction
                    });
            }
            catch
            {
                // Perubahan sudah tersimpan; kegagalan log tidak mengubah response.
            }
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
