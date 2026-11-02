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
    [Route("api/v1/health-services/nurse-view-management/vital-sign-intervals")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Nurse View Vital Sign Interval",
        AreaName = P.AreaName,
        ControllerName = P.VitalSignInterval.Resource,
        Description = "Interval ukur ulang TTV per tingkat EWS yang disahkan Komite Keperawatan",
        SortOrder = 2
    )]
    [Tags("Health Services / Nurse View Management / Vital Sign Interval")]
    public class NvwVitalSignIntervalController : ControllerBase
    {
        private readonly NvwVitalSignIntervalService _service;
        private readonly LoggerService _logger;

        public NvwVitalSignIntervalController(NvwVitalSignIntervalService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<NvwVitalSignIntervalSetResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Nurse View Vital Sign Interval", Description = "Melihat interval ukur ulang TTV yang berlaku", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.VitalSignInterval.Resource, P.Read)]
        public async Task<IActionResult> GetActive(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<NvwVitalSignIntervalSetResponse>.Ok(
                await _service.GetActiveAsync(cancellationToken), "Interval ukur ulang TTV berhasil diambil."));

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<NvwVitalSignIntervalSetResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Update, "Update Nurse View Vital Sign Interval", Description = "Mengganti seluruh interval ukur ulang TTV beserta rujukan pengesahannya", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission(P.VitalSignInterval.Resource, P.Update)]
        public async Task<IActionResult> Replace([FromBody] NvwVitalSignIntervalReplaceRequest request, CancellationToken cancellationToken = default) =>
            await NvwHttp.RespondAsync(this,
                await _service.ReplaceAsync(request, NvwHttp.ActorId(User), cancellationToken),
                request.Items is { Count: 0 }
                    ? "Seluruh interval dicabut. Tanda lewat jadwal tidak tampil di TV."
                    : "Interval ukur ulang TTV berhasil disimpan.",
                _logger, P.VitalSignInterval.Resource, P.Update,
                x => new
                {
                    x.IsConfigured,
                    Items = x.Items.Select(item => new { item.EwsRiskLevel, item.IntervalMinutes }).ToList(),
                    request.RatificationReference,
                    request.RatifiedOn
                });
    }
}
