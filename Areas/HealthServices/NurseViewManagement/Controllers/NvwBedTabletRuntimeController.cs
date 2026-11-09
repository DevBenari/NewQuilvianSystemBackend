using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Controllers
{
    [ApiController]
    [Authorize(Policy = AuthorizationPolicies.NurseViewBedTabletAccess)]
    [Route("api/v1/health-services/nurse-view-management/bed-tablet-runtime")]
    [Tags("Health Services / Nurse View Management / Bed Tablet Runtime")]
    public class NvwBedTabletRuntimeController : ControllerBase
    {
        private const string IdempotencyHeader = "Idempotency-Key";
        private const string LogResource = "NurseViewPatientCall";

        private readonly NvwBedTabletContextService _tabletContext;
        private readonly NvwPatientCallService _calls;
        private readonly LoggerService _logger;

        public NvwBedTabletRuntimeController(
            NvwBedTabletContextService tabletContext,
            NvwPatientCallService calls,
            LoggerService logger)
        {
            _tabletContext = tabletContext;
            _calls = calls;
            _logger = logger;
        }

        [HttpGet("current")]
        [Authorize(Policy = AuthorizationPolicies.NurseViewBedTabletAccess)]
        [ProducesResponseType(typeof(ApiResponse<NvwBedTabletRuntimeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken = default) =>
            NvwHttp.Map(
                this,
                await _tabletContext.GetCurrentAsync(
                    NvwHttp.ActorId(User),
                    User.FindFirstValue(NvwBedTabletContextService.SessionStampClaim),
                    cancellationToken),
                "Keadaan tablet bed berhasil diambil.");

        [HttpPost("calls")]
        [Authorize(Policy = AuthorizationPolicies.NurseViewBedTabletAccess)]
        [ProducesResponseType(typeof(ApiResponse<NvwTabletCallResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Press(
            [FromHeader(Name = IdempotencyHeader)] string? idempotencyKey,
            [FromBody] NvwTabletCallRequest request,
            CancellationToken cancellationToken = default) =>
            await RespondAsync(
                await _calls.PressAsync(NvwHttp.ActorId(User), SessionStamp(), idempotencyKey, request, cancellationToken),
                "Panggilan perawat berhasil dikirim.",
                "Press");

        [HttpPost("calls/current/cancel")]
        [Authorize(Policy = AuthorizationPolicies.NurseViewBedTabletAccess)]
        [ProducesResponseType(typeof(ApiResponse<NvwTabletCallResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Cancel(
            [FromHeader(Name = IdempotencyHeader)] string? idempotencyKey,
            [FromBody] NvwTabletCallVersionRequest request,
            CancellationToken cancellationToken = default) =>
            await RespondAsync(
                await _calls.CancelByPatientAsync(NvwHttp.ActorId(User), SessionStamp(), idempotencyKey, request, cancellationToken),
                "Panggilan berhasil dibatalkan.",
                "CancelByPatient");

        [HttpPost("calls/current/nurse-arrived")]
        [Authorize(Policy = AuthorizationPolicies.NurseViewBedTabletAccess)]
        [ProducesResponseType(typeof(ApiResponse<NvwTabletCallResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> NurseArrived(
            [FromHeader(Name = IdempotencyHeader)] string? idempotencyKey,
            [FromBody] NvwTabletCallVersionRequest request,
            CancellationToken cancellationToken = default) =>
            await RespondAsync(
                await _calls.ConfirmNurseArrivedAsync(NvwHttp.ActorId(User), SessionStamp(), idempotencyKey, request, cancellationToken),
                "Panggilan selesai. Perawat hadir di bed.",
                "NurseArrived");

        private string? SessionStamp() => User.FindFirstValue(NvwBedTabletContextService.SessionStampClaim);

        private async Task<IActionResult> RespondAsync(
            NvwResult<NvwTabletCallOutcome> result,
            string successMessage,
            string action)
        {
            if (!result.IsSuccess || result.Value == null)
                return NvwHttp.Map(this, result, successMessage);

            var outcome = result.Value;
            var call = outcome.Call;
            var message = outcome.IsReplay
                ? $"Permintaan ulang panggilan pasien {call.CallId} bed {outcome.BedCode} dari tablet {outcome.DeviceCode} memakai Idempotency-Key yang sudah tercatat; tanpa perubahan. Aksi: {action}. Status: {call.StatusLabel}."
                : $"Panggilan pasien {call.CallId} bed {outcome.BedCode} dari tablet {outcome.DeviceCode}: {outcome.EventType}. Aksi: {action}. Status: {call.StatusLabel}. Prioritas: {call.PriorityLabel}. Tekan: {call.PressCount}x.";

            await _logger.InfoAsync(
                NvwHttp.LogCategory,
                $"{LogResource}.{action}",
                message,
                new
                {
                    Controller = LogResource,
                    Action = action,
                    Actor = NvwHttp.ActorId(User)
                });

            return NvwHttp.Map(this, NvwResult<NvwTabletCallResponse>.Ok(call), successMessage);
        }
    }
}
