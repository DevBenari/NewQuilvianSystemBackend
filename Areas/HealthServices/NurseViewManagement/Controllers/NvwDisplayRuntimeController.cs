using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;
using P = QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Constants.NurseViewPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/nurse-view-management/display-runtime")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Nurse View Display Runtime",
        AreaName = P.AreaName,
        ControllerName = P.DisplayRuntime.Resource,
        Description = "Runtime papan lantai Nurse View untuk akun perangkat",
        SortOrder = 3
    )]
    [Tags("Health Services / Nurse View Management / Display Runtime")]
    public class NvwDisplayRuntimeController : ControllerBase
    {
        private readonly NvwDeviceContextService _deviceContext;
        private readonly NvwFloorBoardQueryService _floorBoard;
        private readonly NvwPatientCallQueryService _calls;

        public NvwDisplayRuntimeController(
            NvwDeviceContextService deviceContext,
            NvwFloorBoardQueryService floorBoard,
            NvwPatientCallQueryService calls)
        {
            _deviceContext = deviceContext;
            _floorBoard = floorBoard;
            _calls = calls;
        }

        [HttpGet("current")]
        [Authorize(Policy = AuthorizationPolicies.NurseViewDisplayRead)]
        [ProducesResponseType(typeof(ApiResponse<NvwDisplayRuntimeCurrentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction(P.Read, "Read Nurse View Display Runtime", Description = "Melihat identitas perangkat Nurse View yang login", AccessType = AccessTypes.Read, SortOrder = 1)]
        public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken = default) =>
            NvwHttp.Map(
                this,
                await _deviceContext.GetCurrentAsync(
                    NvwHttp.ActorId(User),
                    User.FindFirstValue(NvwDeviceContextService.SessionStampClaim),
                    cancellationToken),
                "Informasi perangkat Nurse View berhasil diambil.");

        [HttpGet("board")]
        [Authorize(Policy = AuthorizationPolicies.NurseViewDisplayRead)]
        [ProducesResponseType(typeof(ApiResponse<NvwFloorBoardResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction(P.Read, "Read Nurse View Floor Board", Description = "Melihat papan lantai milik perangkat Nurse View yang login", AccessType = AccessTypes.Read, SortOrder = 2)]
        public async Task<IActionResult> GetBoard(CancellationToken cancellationToken = default)
        {
            var resolved = await _deviceContext.ResolveActiveDeviceAsync(
                NvwHttp.ActorId(User),
                User.FindFirstValue(NvwDeviceContextService.SessionStampClaim),
                cancellationToken);

            if (!resolved.IsSuccess || resolved.Value == null)
                return NvwHttp.Map(
                    this,
                    NvwResult<NvwFloorBoardResponse>.Forbidden(
                        resolved.Code ?? NvwErrorCodes.NotDisplayAccount,
                        resolved.Message ?? NvwMessages.NotDisplayAccount),
                    string.Empty);

            return Ok(ApiResponse<NvwFloorBoardResponse>.Ok(
                await _floorBoard.GetBoardAsync(resolved.Value.FloorCode, cancellationToken),
                "Papan lantai berhasil diambil."));
        }

        [HttpGet("active-calls")]
        [Authorize(Policy = AuthorizationPolicies.NurseViewDisplayRead)]
        [ProducesResponseType(typeof(ApiResponse<NvwDisplayCallStripResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction(P.Read, "Read Nurse View Active Call Strip", Description = "Melihat strip panggilan aktif lantai milik perangkat Nurse View yang login, tanpa identitas pasien", AccessType = AccessTypes.Read, SortOrder = 3)]
        public async Task<IActionResult> GetActiveCalls(CancellationToken cancellationToken = default)
        {
            var resolved = await _deviceContext.ResolveActiveDeviceAsync(
                NvwHttp.ActorId(User),
                User.FindFirstValue(NvwDeviceContextService.SessionStampClaim),
                cancellationToken);

            if (!resolved.IsSuccess || resolved.Value == null)
                return NvwHttp.Map(
                    this,
                    NvwResult<NvwDisplayCallStripResponse>.Forbidden(
                        resolved.Code ?? NvwErrorCodes.NotDisplayAccount,
                        resolved.Message ?? NvwMessages.NotDisplayAccount),
                    string.Empty);

            return Ok(ApiResponse<NvwDisplayCallStripResponse>.Ok(
                await _calls.GetActiveForDisplayAsync(resolved.Value.FloorCode, cancellationToken),
                "Panggilan aktif lantai berhasil diambil."));
        }
    }
}
