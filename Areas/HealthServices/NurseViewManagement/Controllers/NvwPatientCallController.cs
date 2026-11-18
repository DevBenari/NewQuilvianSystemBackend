using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;
using P = QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Constants.NurseViewPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/nurse-view-management/patient-calls")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Nurse View Patient Call",
        AreaName = P.AreaName,
        ControllerName = P.PatientCall.Resource,
        Description = "Konsol panggilan pasien dari tablet bed: daftar per lantai, klaim, ambil alih, lepas, dan tutup",
        SortOrder = 4
    )]
    [Tags("Health Services / Nurse View Management / Patient Call")]
    public class NvwPatientCallController : ControllerBase
    {
        private readonly NvwPatientCallService _calls;
        private readonly NvwPatientCallQueryService _queries;
        private readonly NvwFloorBoardQueryService _boards;
        private readonly NvwBedTabletService _tablets;
        private readonly AccessPermissionService _access;
        private readonly LoggerService _logger;

        public NvwPatientCallController(
            NvwPatientCallService calls,
            NvwPatientCallQueryService queries,
            NvwFloorBoardQueryService boards,
            NvwBedTabletService tablets,
            AccessPermissionService access,
            LoggerService logger)
        {
            _calls = calls;
            _queries = queries;
            _boards = boards;
            _tablets = tablets;
            _access = access;
            _logger = logger;
        }

        [HttpGet("floors")]
        [ProducesResponseType(typeof(ApiResponse<List<NvwFloorOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Nurse View Patient Call", Description = "Melihat pilihan lantai Konsol Panggilan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.PatientCall.Resource, P.Read)]
        public async Task<IActionResult> GetFloors(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<List<NvwFloorOptionResponse>>.Ok(
                await _tablets.GetFloorsAsync(cancellationToken), "Daftar lantai berhasil diambil."));

        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<NvwConsoleCallListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Read, "Read Nurse View Patient Call", Description = "Melihat panggilan aktif satu lantai di Konsol Panggilan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.PatientCall.Resource, P.Read)]
        public async Task<IActionResult> GetActive([FromQuery] string? floorCode, CancellationToken cancellationToken = default) =>
            NvwHttp.Map(
                this,
                await _queries.GetActiveForConsoleAsync(
                    floorCode,
                    NvwHttp.ActorId(User),
                    await _access.HasAccessAsync(User, P.PatientCall.Resource, P.Update),
                    cancellationToken),
                "Daftar panggilan aktif berhasil diambil.");

        [HttpGet("floor-board")]
        [ProducesResponseType(typeof(ApiResponse<NvwNurseMonitorResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Read, "Read Nurse View Patient Call", Description = "Melihat peta kamar dan bed satu lantai di Monitor perawat", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.PatientCall.Resource, P.Read)]
        public async Task<IActionResult> GetFloorBoard([FromQuery] string? floorCode, CancellationToken cancellationToken = default) =>
            NvwHttp.Map(
                this,
                await _boards.GetNurseMonitorAsync(floorCode, cancellationToken),
                "Peta lantai berhasil diambil.");

        [HttpPost("{id:guid}/claim")]
        [ProducesResponseType(typeof(ApiResponse<NvwConsoleCallResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Nurse View Patient Call", Description = "Mengklaim, mengambil alih, melepas, dan menutup panggilan pasien", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission(P.PatientCall.Resource, P.Update)]
        public async Task<IActionResult> Claim(Guid id, [FromBody] NvwConsoleCallVersionRequest request, CancellationToken cancellationToken = default) =>
            await RespondAsync(
                await _calls.ClaimAsync(id, request, NvwHttp.ActorId(User), cancellationToken),
                "Panggilan berhasil diklaim.",
                NvwPatientCallQueryService.ActionClaim);

        [HttpPost("{id:guid}/take-over")]
        [ProducesResponseType(typeof(ApiResponse<NvwConsoleCallResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Nurse View Patient Call", Description = "Mengklaim, mengambil alih, melepas, dan menutup panggilan pasien", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission(P.PatientCall.Resource, P.Update)]
        public async Task<IActionResult> TakeOver(Guid id, [FromBody] NvwConsoleCallVersionRequest request, CancellationToken cancellationToken = default) =>
            await RespondAsync(
                await _calls.TakeOverAsync(id, request, NvwHttp.ActorId(User), cancellationToken),
                "Panggilan berhasil diambil alih.",
                NvwPatientCallQueryService.ActionTakeOver);

        [HttpPost("{id:guid}/release")]
        [ProducesResponseType(typeof(ApiResponse<NvwConsoleCallResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Nurse View Patient Call", Description = "Mengklaim, mengambil alih, melepas, dan menutup panggilan pasien", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission(P.PatientCall.Resource, P.Update)]
        public async Task<IActionResult> Release(Guid id, [FromBody] NvwConsoleCallVersionRequest request, CancellationToken cancellationToken = default) =>
            await RespondAsync(
                await _calls.ReleaseAsync(id, request, NvwHttp.ActorId(User), cancellationToken),
                "Klaim panggilan berhasil dilepas.",
                NvwPatientCallQueryService.ActionRelease);

        [HttpPost("{id:guid}/close")]
        [ProducesResponseType(typeof(ApiResponse<NvwConsoleCallResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Nurse View Patient Call", Description = "Mengklaim, mengambil alih, melepas, dan menutup panggilan pasien", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission(P.PatientCall.Resource, P.Update)]
        public async Task<IActionResult> Close(Guid id, [FromBody] NvwConsoleCloseRequest request, CancellationToken cancellationToken = default) =>
            await RespondAsync(
                await _calls.CloseFromConsoleAsync(id, request, NvwHttp.ActorId(User), cancellationToken),
                "Panggilan berhasil ditutup.",
                NvwPatientCallQueryService.ActionClose);

        private Task<IActionResult> RespondAsync(
            NvwResult<NvwConsoleCallResponse> result,
            string successMessage,
            string action) =>
            NvwHttp.RespondAsync(
                this,
                result,
                successMessage,
                _logger,
                P.PatientCall.Resource,
                action,
                logMessage: x =>
                    $"Panggilan pasien {x.CallId} bed {x.BedCode} dari Konsol Panggilan. Aksi: {action}. Status: {x.StatusLabel}. Prioritas: {x.PriorityLabel}. Pengklaim: {x.ClaimedByName ?? "-"}.");
    }
}
