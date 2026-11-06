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
    [Route("api/v1/health-services/nurse-view-management/bed-tablets")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Nurse View Bed Tablet",
        AreaName = P.AreaName,
        ControllerName = P.BedTablet.Resource,
        Description = "Tablet di samping bed untuk panggilan pasien ke perawat",
        SortOrder = 3
    )]
    [Tags("Health Services / Nurse View Management / Bed Tablet")]
    public class NvwBedTabletController : ControllerBase
    {
        private readonly NvwBedTabletService _service;
        private readonly LoggerService _logger;

        public NvwBedTabletController(NvwBedTabletService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<NvwBedTabletResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Nurse View Bed Tablet", Description = "Melihat daftar tablet bed Nurse View", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.BedTablet.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] NvwBedTabletPagedQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<NvwBedTabletResponse>>.Ok(
                await _service.GetListAsync(query, cancellationToken), "Daftar tablet bed berhasil diambil."));

        [HttpGet("floors")]
        [ProducesResponseType(typeof(ApiResponse<List<NvwFloorOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Nurse View Bed Tablet", Description = "Melihat pilihan lantai untuk tablet bed", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.BedTablet.Resource, P.Read)]
        public async Task<IActionResult> GetFloors(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<List<NvwFloorOptionResponse>>.Ok(
                await _service.GetFloorsAsync(cancellationToken), "Daftar lantai berhasil diambil."));

        [HttpGet("bed-options")]
        [ProducesResponseType(typeof(ApiResponse<List<NvwBedOptionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Read, "Read Nurse View Bed Tablet", Description = "Melihat pilihan bed satu lantai beserta tanda sudah punya tablet", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.BedTablet.Resource, P.Read)]
        public async Task<IActionResult> GetBedOptions([FromQuery] string? floorCode, CancellationToken cancellationToken = default) =>
            NvwHttp.Map(this,
                await _service.GetBedOptionsAsync(floorCode, cancellationToken),
                "Daftar bed berhasil diambil.");

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NvwBedTabletDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Nurse View Bed Tablet", Description = "Melihat rincian tablet bed Nurse View", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.BedTablet.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetDetailAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, NvwMessages.TabletNotFound))
                : Ok(ApiResponse<NvwBedTabletDetailResponse>.Ok(result, "Rincian tablet bed berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NvwBedTabletDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, "Create Nurse View Bed Tablet", Description = "Mendaftarkan tablet untuk satu bed", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.BedTablet.Resource, P.Create)]
        public async Task<IActionResult> Create([FromBody] NvwBedTabletCreateRequest request, CancellationToken cancellationToken = default) =>
            await NvwHttp.RespondAsync(this,
                await _service.CreateAsync(request, NvwHttp.ActorId(User), cancellationToken),
                "Tablet bed berhasil didaftarkan.", _logger, P.BedTablet.Resource, P.Create,
                logMessage: x => $"Tablet bed {x.DeviceCode} (Id {x.Id}) didaftarkan untuk bed {x.BedCode}. Aksi: {P.Create}. Status: {x.DeviceStatusLabel}.");

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NvwBedTabletDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, "Update Nurse View Bed Tablet", Description = "Mengubah nama, bed, lokasi, atau keterangan tablet", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.BedTablet.Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] NvwBedTabletUpdateRequest request, CancellationToken cancellationToken = default) =>
            await NvwHttp.RespondAsync(this,
                await _service.UpdateAsync(id, request, NvwHttp.ActorId(User), cancellationToken),
                "Tablet bed berhasil diperbarui.", _logger, P.BedTablet.Resource, P.Update,
                logMessage: x => x.PreviousBedCode == null
                    ? $"Tablet bed {x.DeviceCode} (Id {x.Id}) diperbarui; bed {x.BedCode}. Aksi: {P.Update}. Status: {x.DeviceStatusLabel}."
                    : $"Tablet bed {x.DeviceCode} (Id {x.Id}) dipindah dari bed {x.PreviousBedCode} ke bed {x.BedCode}. Aksi: {P.Update}. Status: {x.DeviceStatusLabel}.");

        [HttpPost("{id:guid}/login")]
        [ProducesResponseType(typeof(ApiResponse<NvwBedTabletLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Nurse View Bed Tablet", Description = "Membuat login tablet; kata sandi tampil sekali", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.BedTablet.Resource, P.Update)]
        public async Task<IActionResult> CreateLogin(Guid id, [FromBody] NvwBedTabletLoginRequest request, CancellationToken cancellationToken = default) =>
            await NvwHttp.RespondAsync(this,
                await _service.CreateLoginAsync(id, request, NvwHttp.ActorId(User), cancellationToken),
                "Login tablet berhasil dibuat. Catat kata sandi karena hanya ditampilkan sekali.", _logger,
                P.BedTablet.Resource, NvwBedTabletService.ActionCreateLogin,
                logMessage: x => $"Login tablet bed {x.DeviceCode} (Id {x.TabletId}) dibuat untuk bed {x.BedCode}; akun {x.LoginUserName}. Aksi: {NvwBedTabletService.ActionCreateLogin}. Status: {x.DeviceStatusLabel}.");

        [HttpPost("{id:guid}/login/reset")]
        [ProducesResponseType(typeof(ApiResponse<NvwBedTabletLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Nurse View Bed Tablet", Description = "Mengatur ulang login tablet dan mengaktifkan kembali tablet nonaktif", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.BedTablet.Resource, P.Update)]
        public async Task<IActionResult> ResetLogin(Guid id, [FromBody] NvwBedTabletLoginRequest request, CancellationToken cancellationToken = default) =>
            await NvwHttp.RespondAsync(this,
                await _service.ResetLoginAsync(id, request, NvwHttp.ActorId(User), cancellationToken),
                "Login tablet berhasil diatur ulang. Catat kata sandi karena hanya ditampilkan sekali.", _logger,
                P.BedTablet.Resource, NvwBedTabletService.ActionResetLogin,
                logMessage: x => $"Login tablet bed {x.DeviceCode} (Id {x.TabletId}) diatur ulang untuk bed {x.BedCode}; akun {x.LoginUserName}. Aksi: {NvwBedTabletService.ActionResetLogin}. Status: {x.DeviceStatusLabel}.");

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<NvwBedTabletDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Nurse View Bed Tablet", Description = "Menonaktifkan tablet; login dikunci seketika", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.BedTablet.Resource, P.Update)]
        public async Task<IActionResult> Deactivate(Guid id, [FromBody] NvwBedTabletDeactivateRequest request, CancellationToken cancellationToken = default) =>
            await NvwHttp.RespondAsync(this,
                await _service.DeactivateAsync(id, request, NvwHttp.ActorId(User), cancellationToken),
                "Tablet bed berhasil dinonaktifkan.", _logger, P.BedTablet.Resource,
                NvwBedTabletService.ActionDeactivate,
                logMessage: x => $"Tablet bed {x.DeviceCode} (Id {x.Id}) untuk bed {x.BedCode} dinonaktifkan. Aksi: {NvwBedTabletService.ActionDeactivate}. Status: {x.DeviceStatusLabel}.");
    }
}
