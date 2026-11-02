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
    [Route("api/v1/health-services/nurse-view-management/display-devices")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Nurse View Display Device",
        AreaName = P.AreaName,
        ControllerName = P.DisplayDevice.Resource,
        Description = "Perangkat TV papan pemantauan perawat per lantai",
        SortOrder = 1
    )]
    [Tags("Health Services / Nurse View Management / Display Device")]
    public class NvwDisplayDeviceController : ControllerBase
    {
        private readonly NvwDisplayDeviceService _service;
        private readonly LoggerService _logger;

        public NvwDisplayDeviceController(NvwDisplayDeviceService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<NvwDisplayDeviceResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Nurse View Display Device", Description = "Melihat daftar perangkat display Nurse View", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.DisplayDevice.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] NvwDisplayDevicePagedQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<NvwDisplayDeviceResponse>>.Ok(
                await _service.GetListAsync(query, cancellationToken), "Daftar perangkat Nurse View berhasil diambil."));

        [HttpGet("floors")]
        [ProducesResponseType(typeof(ApiResponse<List<NvwFloorOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Nurse View Display Device", Description = "Melihat pilihan lantai yang terdaftar pada data kamar", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.DisplayDevice.Resource, P.Read)]
        public async Task<IActionResult> GetFloors(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<List<NvwFloorOptionResponse>>.Ok(
                await _service.GetFloorsAsync(cancellationToken), "Daftar lantai berhasil diambil."));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NvwDisplayDeviceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Nurse View Display Device", Description = "Melihat rincian perangkat display Nurse View", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.DisplayDevice.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetDetailAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, NvwMessages.DeviceNotFound))
                : Ok(ApiResponse<NvwDisplayDeviceDetailResponse>.Ok(result, "Rincian perangkat Nurse View berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NvwDisplayDeviceDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, "Create Nurse View Display Device", Description = "Mendaftarkan perangkat display Nurse View", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.DisplayDevice.Resource, P.Create)]
        public async Task<IActionResult> Create([FromBody] NvwDisplayDeviceCreateRequest request, CancellationToken cancellationToken = default) =>
            await NvwHttp.RespondAsync(this,
                await _service.CreateAsync(request, NvwHttp.ActorId(User), cancellationToken),
                "Perangkat Nurse View berhasil didaftarkan.", _logger, P.DisplayDevice.Resource, P.Create,
                x => new { x.Id, x.DeviceCode, x.FloorCode });

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NvwDisplayDeviceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, "Update Nurse View Display Device", Description = "Mengubah nama, lantai, lokasi, atau keterangan perangkat", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.DisplayDevice.Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] NvwDisplayDeviceUpdateRequest request, CancellationToken cancellationToken = default) =>
            await NvwHttp.RespondAsync(this,
                await _service.UpdateAsync(id, request, NvwHttp.ActorId(User), cancellationToken),
                "Perangkat Nurse View berhasil diperbarui.", _logger, P.DisplayDevice.Resource, P.Update,
                x => new { x.Id, x.DeviceCode, x.FloorCode, x.Version });

        [HttpPost("{id:guid}/login")]
        [ProducesResponseType(typeof(ApiResponse<NvwDisplayDeviceLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Nurse View Display Device", Description = "Membuat login perangkat; kata sandi tampil sekali", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.DisplayDevice.Resource, P.Update)]
        public async Task<IActionResult> CreateLogin(Guid id, [FromBody] NvwDisplayDeviceLoginRequest request, CancellationToken cancellationToken = default) =>
            await NvwHttp.RespondAsync(this,
                await _service.CreateLoginAsync(id, request, NvwHttp.ActorId(User), cancellationToken),
                "Login perangkat berhasil dibuat. Catat kata sandi karena hanya ditampilkan sekali.", _logger,
                P.DisplayDevice.Resource, NvwDisplayDeviceService.ActionCreateLogin,
                x => new { x.DeviceId, x.DeviceCode, x.LoginUserName });

        [HttpPost("{id:guid}/login/reset")]
        [ProducesResponseType(typeof(ApiResponse<NvwDisplayDeviceLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Nurse View Display Device", Description = "Mengatur ulang login perangkat dan mengaktifkan kembali perangkat nonaktif", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.DisplayDevice.Resource, P.Update)]
        public async Task<IActionResult> ResetLogin(Guid id, [FromBody] NvwDisplayDeviceLoginRequest request, CancellationToken cancellationToken = default) =>
            await NvwHttp.RespondAsync(this,
                await _service.ResetLoginAsync(id, request, NvwHttp.ActorId(User), cancellationToken),
                "Login perangkat berhasil diatur ulang. Catat kata sandi karena hanya ditampilkan sekali.", _logger,
                P.DisplayDevice.Resource, NvwDisplayDeviceService.ActionResetLogin,
                x => new { x.DeviceId, x.DeviceCode, x.LoginUserName });

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<NvwDisplayDeviceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Nurse View Display Device", Description = "Menonaktifkan perangkat; login dikunci seketika", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.DisplayDevice.Resource, P.Update)]
        public async Task<IActionResult> Deactivate(Guid id, [FromBody] NvwDisplayDeviceDeactivateRequest request, CancellationToken cancellationToken = default) =>
            await NvwHttp.RespondAsync(this,
                await _service.DeactivateAsync(id, request, NvwHttp.ActorId(User), cancellationToken),
                "Perangkat Nurse View berhasil dinonaktifkan.", _logger, P.DisplayDevice.Resource,
                NvwDisplayDeviceService.ActionDeactivate,
                x => new { x.Id, x.DeviceCode, x.DeviceStatus });
    }
}
