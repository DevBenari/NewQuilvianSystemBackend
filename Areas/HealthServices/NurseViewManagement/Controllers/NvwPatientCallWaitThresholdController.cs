using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Constants.NurseViewPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/nurse-view-management/patient-call-wait-thresholds")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Nurse View Patient Call Wait Threshold",
        AreaName = P.AreaName,
        ControllerName = P.PatientCallWaitThreshold.Resource,
        Description = "Ambang lama menunggu panggilan pasien yang disahkan Komite Keperawatan",
        SortOrder = 5
    )]
    [Tags("Health Services / Nurse View Management / Patient Call Wait Threshold")]
    public class NvwPatientCallWaitThresholdController : ControllerBase
    {
        private readonly NvwPatientCallWaitThresholdService _service;
        private readonly LoggerService _logger;

        public NvwPatientCallWaitThresholdController(NvwPatientCallWaitThresholdService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<NvwPatientCallWaitThresholdResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Nurse View Patient Call Wait Threshold", Description = "Melihat ambang lama menunggu panggilan pasien yang berlaku", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.PatientCallWaitThreshold.Resource, P.Read)]
        public async Task<IActionResult> GetActive(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<NvwPatientCallWaitThresholdResponse>.Ok(
                await _service.GetActiveAsync(cancellationToken), "Ambang lama menunggu berhasil diambil."));

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<NvwPatientCallWaitThresholdResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Update, "Update Nurse View Patient Call Wait Threshold", Description = "Mengganti atau mencabut ambang lama menunggu beserta rujukan pengesahannya", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission(P.PatientCallWaitThreshold.Resource, P.Update)]
        public async Task<IActionResult> Replace([FromBody] NvwPatientCallWaitThresholdReplaceRequest request, CancellationToken cancellationToken = default) =>
            await NvwHttp.RespondAsync(this,
                await _service.ReplaceAsync(request, NvwHttp.ActorId(User), cancellationToken),
                request.WarningAfterMinutes == null && request.CriticalAfterMinutes == null
                    ? "Ambang lama menunggu dicabut. Panggilan tampil tanpa warna."
                    : "Ambang lama menunggu berhasil disimpan.",
                _logger, P.PatientCallWaitThreshold.Resource, P.Update,
                logMessage: _ => NvwPatientCallWaitThresholdService.BuildAuditMessage(request));
    }
}
