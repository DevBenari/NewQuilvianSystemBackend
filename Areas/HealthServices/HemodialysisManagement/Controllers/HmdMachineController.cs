using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Constants;
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
    /// Master mesin cuci darah beserta riwayat status laik pakainya (<c>BE-HMD-04</c>).
    /// </summary>
    /// <remarks>
    /// <c>PATCH /{id}/status</c> pada controller ini mengikuti kontrak <c>HMD-CONTRACT-v1</c>: ia
    /// memindahkan <b>status laik pakai</b> (<c>Ready</c>, <c>Blocked</c>, <c>Maintenance</c>,
    /// <c>NotEligible</c>) beserta alasannya, bukan sekadar sakelar aktif. Sakelar aktif ikut
    /// pada <c>PUT /{id}</c>, dan penonaktifan permanen memakai <c>DELETE /{id}</c>.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/master-data/hemodialysis-machines")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Machine",
        AreaName = P.AreaName,
        ControllerName = P.Machine.Resource,
        Description = "Master mesin hemodialisa beserta status laik pakainya",
        SortOrder = 15
    )]
    [Tags("Health Services / Hemodialysis Management / Master Data / Hemodialysis Machine")]
    public class HmdMachineController : ControllerBase
    {
        private readonly HmdResourceService _service;
        private readonly LoggerService _logger;

        public HmdMachineController(HmdResourceService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<HmdMachineFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Machine", Description = "Melihat konfigurasi penyaring mesin HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Machine.Resource, P.Read)]
        public IActionResult GetFilterMetadata() =>
            Ok(ApiResponse<HmdMachineFilterMetadataResponse>.Ok(
                HmdResourceService.BuildMachineFilterMetadata(), "Metadata filter mesin HD berhasil diambil."));

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<HmdMachineSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Machine", Description = "Melihat ringkasan mesin HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Machine.Resource, P.Read)]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<HmdMachineSummaryResponse>.Ok(
                await _service.GetMachineSummaryAsync(cancellationToken), "Ringkasan mesin HD berhasil diambil."));

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdMachineResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Machine", Description = "Melihat daftar mesin HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Machine.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] HmdMachinePagedQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<HmdMachineResponse>>.Ok(
                await _service.GetMachinesAsync(query, cancellationToken), "Daftar mesin HD berhasil diambil."));

        /// <summary>
        /// Pilihan mesin untuk penjadwalan. Bawaannya hanya mesin aktif, boleh dijadwalkan, dan
        /// berstatus siap.
        /// </summary>
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdMachineOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Hemodialysis Machine", Description = "Melihat pilihan mesin HD yang dapat dijadwalkan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Machine.Resource, P.Read)]
        public async Task<IActionResult> GetOptions([FromQuery] HmdMachineOptionsQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<HmdMachineOptionResponse>>.Ok(
                await _service.GetMachineOptionsAsync(query, cancellationToken), "Pilihan mesin HD berhasil diambil."));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdMachineResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Machine", Description = "Melihat rincian mesin HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Machine.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetMachineAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Mesin tidak ditemukan atau sudah dihapus."))
                : Ok(ApiResponse<HmdMachineResponse>.Ok(result, "Rincian mesin HD berhasil diambil."));
        }

        [HttpGet("{id:guid}/status-history")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HmdMachineStatusHistoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Machine", Description = "Melihat riwayat perubahan status mesin HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Machine.Resource, P.Read)]
        public async Task<IActionResult> GetStatusHistory(
            Guid id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default) =>
            HmdHttp.Map(this, await _service.GetMachineStatusHistoryAsync(id, pageNumber, pageSize, cancellationToken),
                "Riwayat status mesin HD berhasil diambil.");

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<HmdMachineResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Create, "Create Hemodialysis Machine", Description = "Mendaftarkan mesin HD baru", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Machine.Resource, P.Create)]
        public async Task<IActionResult> Create([FromBody] CreateHmdMachineRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.CreateMachineAsync(request, HmdHttp.ActorId(User), cancellationToken),
                "Mesin HD berhasil didaftarkan.", _logger, P.Machine.Resource, P.Create,
                x => new { x.Id, x.MachineCode });

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdMachineResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Hemodialysis Machine", Description = "Memperbarui data mesin HD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Machine.Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHmdMachineRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.UpdateMachineAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Data mesin HD berhasil diperbarui.", _logger, P.Machine.Resource, P.Update,
                x => new { x.Id, x.MachineCode, x.IsActive });

        /// <summary>
        /// Mengubah status laik pakai mesin beserta alasannya. Setiap perubahan menulis satu baris
        /// riwayat status.
        /// </summary>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<HmdMachineResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Machine.ChangeStatus, "Change Hemodialysis Machine Status", Description = "Mengubah status laik pakai mesin HD beserta alasannya", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.Machine.Resource, P.Machine.ChangeStatus)]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeHmdMachineStatusRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.ChangeMachineStatusAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Status mesin HD berhasil diubah.", _logger, P.Machine.Resource, P.Machine.ChangeStatus,
                x => new { x.Id, x.MachineCode, x.MachineStatus });

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Delete, "Delete Hemodialysis Machine", Description = "Menonaktifkan mesin HD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission(P.Machine.Resource, P.Delete)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.DeleteMachineAsync(id, HmdHttp.ActorId(User), cancellationToken),
                "Mesin HD berhasil dinonaktifkan.", _logger, P.Machine.Resource, P.Delete,
                _ => new { Id = id });
    }
}
