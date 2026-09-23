using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers
{
    /// <summary>
    /// Gerbang pemulangan rawat inap: webhook sinyal clearance kasir, supervisor override darurat,
    /// dan konfirmasi kepergian fisik pasien beserta pelepasan tempat tidur.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/inpatient-management/episodes")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_INPATIENT",
        moduleName: "Health Service Inpatient",
        displayName: "Inpatient Discharge Clearance",
        AreaName = "HealthServices",
        ControllerName = "InpatientDischargeClearance",
        Description = "Gerbang pemulangan pasien, webhook clearance kasir, dan supervisor override",
        SortOrder = 19
    )]
    [Tags("Inpatient Discharge Clearance")]
    public class InpatientDischargeClearanceController : ControllerBase
    {
        private const string LogCategory = "HealthServices.InPatientManagement.DischargeClearance";

        private readonly IInpatientClearanceGateService _clearanceGateService;
        private readonly LoggerService _loggerService;

        public InpatientDischargeClearanceController(
            IInpatientClearanceGateService clearanceGateService,
            LoggerService loggerService)
        {
            _clearanceGateService = clearanceGateService;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Menerima sinyal clearance dari kasir (ClearanceApproved / ClearanceRevoked dengan Auto-Reblock).
        /// </summary>
        [HttpPost("{episodeId:guid}/discharge-clearance/webhook")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AllowAnonymous] // Internal system webhook integration
        public async Task<IActionResult> HandleClearanceWebhook(
            Guid episodeId,
            [FromBody] ClearanceSignalWebhookDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _clearanceGateService.HandleClearanceSignalAsync(
                episodeId,
                request,
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return BuildFailure(result.Status, result.Message);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientDischargeClearance.HandleClearanceWebhook",
                "Sinyal webhook clearance kasir berhasil diproses.",
                new
                {
                    EpisodeId = episodeId,
                    Action = request.Action,
                    Reason = request.Reason
                });

            return Ok(ApiResponse<object>.Ok(null, result.Message));
        }

        /// <summary>
        /// Otorisasi supervisor override darurat untuk pemulangan pasien saat clearance kasir belum disetujui.
        /// </summary>
        [HttpPost("{episodeId:guid}/supervisor-override")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("SupervisorOverride", "Supervisor Override Discharge Gate", Description = "Otorisasi supervisor override untuk pelepasan darurat medis", AccessType = AccessTypes.Update, SortOrder = 1)]
        [AccessPermission("InpatientDischargeClearance", "SupervisorOverride")]
        public async Task<IActionResult> SupervisorOverride(
            Guid episodeId,
            [FromBody] SupervisorOverrideRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var isSupervisor = User.IsSupervisor()
                || User.Claims.Any(c => (c.Type == "permission" || c.Type == ClaimTypes.Role) && c.Value == "InpatientSupervisor:Override")
                || User.IsInRole("SuperAdmin");

            var result = await _clearanceGateService.ExecuteSupervisorOverrideAsync(
                episodeId,
                request,
                User.GetUserId(),
                isSupervisor,
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return BuildFailure(result.Status, result.Message);
            }

            await _loggerService.WarningAsync(
                LogCategory,
                "InpatientDischargeClearance.SupervisorOverride",
                "Supervisor override darurat disahkan untuk pemulangan pasien.",
                new
                {
                    EpisodeId = episodeId,
                    UserId = User.GetUserId(),
                    Reason = request.Reason
                });

            return Ok(ApiResponse<object>.Ok(null, result.Message));
        }

        /// <summary>
        /// Mengonfirmasi pelepasan fisik pasien dari bangsal, menutup jam hunian tempat tidur, dan menerbitkan event BED_RELEASED.
        /// </summary>
        [HttpPost("{episodeId:guid}/confirm-physical-discharge")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("ConfirmPhysicalDischarge", "Confirm Physical Discharge", Description = "Konfirmasi pelepasan fisik pasien dari tempat tidur", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("InpatientDischargeClearance", "ConfirmPhysicalDischarge")]
        public async Task<IActionResult> ConfirmPhysicalDischarge(
            Guid episodeId,
            [FromBody] ConfirmPhysicalDischargeRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _clearanceGateService.ConfirmPhysicalDischargeAsync(
                episodeId,
                request,
                User.GetUserId(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return BuildFailure(result.Status, result.Message);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientDischargeClearance.ConfirmPhysicalDischarge",
                "Pasien berhasil dipulangkan secara fisik dan tempat tidur dilepaskan.",
                new
                {
                    EpisodeId = episodeId,
                    UserId = User.GetUserId(),
                    PhysicalDischargeDateTime = request.PhysicalDischargeDateTime
                });

            return Ok(ApiResponse<object>.Ok(null, result.Message));
        }

        private IActionResult BuildFailure(InpEpisodeOperationStatus status, string message)
        {
            return status switch
            {
                InpEpisodeOperationStatus.Invalid => BadRequest(
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, message)),

                InpEpisodeOperationStatus.Forbidden => StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, message)),

                InpEpisodeOperationStatus.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, message)),

                InpEpisodeOperationStatus.Conflict => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, message)),

                _ => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, message))
            };
        }
    }
}
