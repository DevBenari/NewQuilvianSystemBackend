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
    /// Jadwal harian MCU — <c>MCU-API-v1</c> grup Mcu Daily Schedule (<c>BE-MCU-007</c>): hari buka,
    /// kuota, dokter MCU jaga, dan pelampauan kuota oleh kepala unit.
    /// </summary>
    /// <remarks>
    /// Action sengaja tanpa <c>&lt;summary&gt;</c> XML. Pelampauan kuota memegang action tersendiri
    /// <c>ApproveQuotaOverride</c> (<c>MCU-DEC-029</c>), sehingga petugas yang hanya boleh mengatur
    /// jadwal menerima <c>403</c>.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-checkup-management/daily-schedules")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Mcu Daily Schedule",
        AreaName = P.AreaName,
        ControllerName = P.Schedule.Resource,
        Description = "Jadwal harian MCU: hari buka, kuota, dokter jaga, dan pelampauan kuota",
        SortOrder = 3
    )]
    [Tags("Health Services / Medical Checkup Management / Mcu Daily Schedule")]
    public class McuScheduleController : ControllerBase
    {
        private readonly McuScheduleService _service;
        private readonly LoggerService _logger;

        public McuScheduleController(McuScheduleService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<McuDailyScheduleResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, "Read Mcu Daily Schedule", Description = "Melihat jadwal, sisa kuota, dan dokter MCU jaga", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Schedule.Resource, P.Read)]
        public async Task<IActionResult> GetSchedules([FromQuery] McuDailyScheduleQuery query, CancellationToken cancellationToken = default) =>
            McuHttp.Map(this, await _service.GetSchedulesAsync(query, cancellationToken), "Jadwal MCU berhasil diambil.");

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<List<McuDailyScheduleResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Mcu Daily Schedule", Description = "Mengatur hari buka, kuota, dan dokter MCU jaga", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission(P.Schedule.Resource, P.Update)]
        public async Task<IActionResult> UpsertRange([FromBody] UpsertMcuDailyScheduleRangeRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.UpsertRangeAsync(request, McuHttp.ActorId(User), cancellationToken),
                "Jadwal MCU berhasil disimpan.", _logger, P.Schedule.Resource, P.Update,
                x => $"Jadwal MCU {McuScheduleService.FormatDate(request.From)} s.d. {McuScheduleService.FormatDate(request.To)} " +
                     $"diatur: {(request.IsOpen ? "buka" : "tutup")}, kuota {request.Quota}, {x.Count} hari.");

        [HttpPost("{scheduleDate}/quota-overrides")]
        [ProducesResponseType(typeof(ApiResponse<McuDailyScheduleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Schedule.ApproveQuotaOverride, "Approve Mcu Quota Override", Description = "Menyetujui pelampauan kuota harian MCU beserta alasannya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Schedule.Resource, P.Schedule.ApproveQuotaOverride)]
        public async Task<IActionResult> ApproveQuotaOverride(DateOnly scheduleDate, [FromBody] CreateMcuQuotaOverrideRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.ApproveQuotaOverrideAsync(scheduleDate, request, McuHttp.ActorId(User), cancellationToken),
                "Pelampauan kuota MCU berhasil disetujui.", _logger, P.Schedule.Resource, P.Schedule.ApproveQuotaOverride,
                x => $"Pelampauan kuota MCU {McuScheduleService.FormatDate(scheduleDate)} sebanyak {request.AdditionalQuota} disetujui; " +
                     $"kapasitas kini {x.Capacity}.");
    }
}
