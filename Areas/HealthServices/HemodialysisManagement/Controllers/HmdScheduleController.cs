using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Constants.HemodialysisPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Controllers
{
    /// <summary>
    /// Penjadwalan sesi, penugasan petugas, dan daftar kerja unit (<c>BE-HMD-10</c>, <c>BE-HMD-11</c>).
    /// </summary>
    /// <remarks>
    /// Berbagi base URL <c>hemodialysis-sessions</c> dengan controller pelaksanaan sesi, tetapi
    /// memegang Resource hak akses sendiri, <c>HemodialysisSchedule</c>, sesuai
    /// <c>contracts/api-contract.md</c> grup Hemodialysis Schedule.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/hemodialysis-sessions")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Schedule",
        AreaName = P.AreaName,
        ControllerName = P.Schedule.Resource,
        Description = "Penjadwalan sesi dan penugasan petugas hemodialisa",
        SortOrder = 8
    )]
    [Tags("Health Services / Hemodialysis Management / Hemodialysis Schedule")]
    public class HmdScheduleController : ControllerBase
    {
        private readonly HmdScheduleService _service;
        private readonly LoggerService _logger;

        public HmdScheduleController(HmdScheduleService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("worklist/filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<HmdWorklistFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Schedule", Description = "Melihat konfigurasi penyaring daftar kerja unit HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Schedule.Resource, P.Read)]
        public IActionResult GetWorklistFilterMetadata() =>
            Ok(ApiResponse<HmdWorklistFilterMetadataResponse>.Ok(
                HmdScheduleService.BuildWorklistFilterMetadata(), "Metadata filter daftar kerja berhasil diambil."));

        [HttpGet("worklist/summary")]
        [ProducesResponseType(typeof(ApiResponse<HmdWorklistSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Schedule", Description = "Melihat ringkasan daftar kerja unit HD pada satu tanggal", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Schedule.Resource, P.Read)]
        public async Task<IActionResult> GetWorklistSummary(
            [FromQuery] DateOnly? date,
            [FromQuery] Guid? serviceUnitId,
            CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<HmdWorklistSummaryResponse>.Ok(
                await _service.GetWorklistSummaryAsync(date, serviceUnitId, cancellationToken), "Ringkasan daftar kerja berhasil diambil."));

        /// <summary>
        /// Daftar kerja unit pada satu tanggal dan shift. Penanda isolasi hanya berupa
        /// <c>true</c>/<c>false</c>; diagnosis dan status serologi tidak pernah ikut.
        /// </summary>
        [HttpGet("worklist")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdWorklistItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Schedule", Description = "Melihat daftar kerja unit HD pada satu tanggal dan shift", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Schedule.Resource, P.Read)]
        public async Task<IActionResult> GetWorklist([FromQuery] HmdWorklistQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<HmdWorklistItemResponse>>.Ok(
                await _service.GetWorklistAsync(query, cancellationToken), "Daftar kerja unit HD berhasil diambil."));

        /// <summary>
        /// Menjadwalkan sesi baru. Tabrakan pasien, mesin, atau station ditolak <c>409</c>; mesin
        /// yang tidak siap atau tidak memenuhi kebutuhan isolasi ditolak <c>422</c>.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, "Create Hemodialysis Schedule", Description = "Menjadwalkan sesi HD baru", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Schedule.Resource, P.Create)]
        public async Task<IActionResult> Create([FromBody] CreateHmdSessionRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateAsync(request, HmdHttp.ActorId(User), cancellationToken),
                "Sesi hemodialisa berhasil dijadwalkan.", _logger, P.Schedule.Resource, P.Create,
                x => new { x.Id, x.SessionNumber, x.EpisodeId, x.MachineId, x.StationId, x.ScheduledStartAt, x.ScheduledEndAt });

        [HttpPatch("{id:guid}/schedule")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Update, "Update Hemodialysis Schedule", Description = "Mengubah tanggal, shift, mesin, station, atau dokter sesi HD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Schedule.Resource, P.Update)]
        public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleHmdSessionRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.RescheduleAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Jadwal sesi hemodialisa berhasil diubah.", _logger, P.Schedule.Resource, "Reschedule",
                x => new { x.Id, x.SessionNumber, x.MachineId, x.StationId, x.ScheduledStartAt, x.ScheduledEndAt, x.SessionStatus });

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Schedule.Cancel, "Cancel Hemodialysis Schedule", Description = "Membatalkan sesi HD sebelum dimulai", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.Schedule.Resource, P.Schedule.Cancel)]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelHmdSessionRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CancelAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Sesi hemodialisa berhasil dibatalkan.", _logger, P.Schedule.Resource, P.Schedule.Cancel,
                x => new { x.Id, x.SessionNumber, x.SessionStatus });

        [HttpGet("{id:guid}/staff-assignments")]
        [ProducesResponseType(typeof(ApiResponse<List<HmdStaffAssignmentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Schedule", Description = "Melihat petugas sesi HD beserta status kewenangannya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Schedule.Resource, P.Read)]
        public async Task<IActionResult> GetStaffAssignments(Guid id, CancellationToken cancellationToken = default) =>
            HmdHttp.Map(this, await _service.GetStaffAssignmentsAsync(id, cancellationToken), "Petugas sesi berhasil diambil.");

        /// <summary>
        /// Menetapkan dokter penanggung jawab dan petugas sesi. Status kewenangan tersimpan
        /// <c>NotVerifiable</c> selama pembacaan HR belum tersedia (<c>HMD-DEP-002</c>).
        /// </summary>
        [HttpPut("{id:guid}/staff-assignments")]
        [ProducesResponseType(typeof(ApiResponse<List<HmdStaffAssignmentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Update, "Update Hemodialysis Schedule", Description = "Menetapkan dokter dan petugas pada sesi HD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Schedule.Resource, P.Update)]
        public async Task<IActionResult> AssignStaff(Guid id, [FromBody] AssignHmdStaffRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.AssignStaffAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Petugas sesi berhasil ditetapkan.", _logger, P.Schedule.Resource, "AssignStaff",
                x => new { SessionId = id, StaffCount = x.Count, request.ResponsibleDoctorId });
    }
}
