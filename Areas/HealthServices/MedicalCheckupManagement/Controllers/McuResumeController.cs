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
    /// Resume MCU — <c>MCU-API-v1</c> grup Mcu Resume. <c>BE-MCU-030</c> memasang draf, approve, kembalikan, dan
    /// verifikasi; <c>BE-MCU-031</c> buka ulang berversi.
    /// </summary>
    /// <remarks>Action sengaja tanpa <c>&lt;summary&gt;</c> XML. Isi resume sensitif — log hanya nomor order, versi, dan status.</remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-checkup-management/orders/{id:guid}/resumes")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Mcu Resume",
        AreaName = P.AreaName,
        ControllerName = P.Resume.Resource,
        Description = "Resume MCU: draf dan approve dokter, verifikasi petugas",
        SortOrder = 6
    )]
    [Tags("Health Services / Medical Checkup Management / Mcu Resume")]
    public class McuResumeController : ControllerBase
    {
        private readonly McuResumeService _service;
        private readonly LoggerService _logger;

        public McuResumeController(McuResumeService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<McuResumeResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Mcu Resume", Description = "Melihat seluruh versi resume MCU", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Resume.Resource, P.Read)]
        public async Task<IActionResult> GetAll(Guid id, CancellationToken cancellationToken = default) =>
            McuHttp.Map(this, await _service.GetAllAsync(id, cancellationToken), "Resume MCU berhasil diambil.");

        [HttpPut("current")]
        [ProducesResponseType(typeof(ApiResponse<McuResumeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Mcu Resume", Description = "Dokter MCU yang ditetapkan menyimpan draf resume", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission(P.Resume.Resource, P.Update)]
        public async Task<IActionResult> SaveCurrent(Guid id, [FromBody] SaveMcuResumeRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.SaveCurrentAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Draf resume disimpan.", _logger, P.Resume.Resource, P.Update,
                x => $"Draf resume order MCU {x.OrderNumber} versi {x.VersionNumber} disimpan.");

        [HttpPost("current/approve")]
        [ProducesResponseType(typeof(ApiResponse<McuResumeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Resume.Approve, "Approve Mcu Resume", Description = "Dokter MCU yang ditetapkan menyetujui resume sesudah semua komponen tuntas", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Resume.Resource, P.Resume.Approve)]
        public async Task<IActionResult> Approve(Guid id, [FromBody] McuResumeActionRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.ApproveAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Resume disetujui.", _logger, P.Resume.Resource, P.Resume.Approve,
                x => $"Resume order MCU {x.OrderNumber} versi {x.VersionNumber} disetujui {x.ApprovedByDoctorName}.");

        [HttpPost("current/return")]
        [ProducesResponseType(typeof(ApiResponse<McuResumeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Resume.Verify, "Verify Mcu Resume", Description = "Petugas MCU memverifikasi resume atau mengembalikannya ke dokter", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.Resume.Resource, P.Resume.Verify)]
        public async Task<IActionResult> Return(Guid id, [FromBody] ReturnMcuResumeRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.ReturnAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Resume dikembalikan ke dokter.", _logger, P.Resume.Resource, P.Resume.Verify,
                // Alasan pengembalian dapat menyebut isi klinis — tidak dicatat.
                x => $"Resume order MCU {x.OrderNumber} versi {x.VersionNumber} dikembalikan ke dokter.");

        [HttpPost("current/verify")]
        [ProducesResponseType(typeof(ApiResponse<McuResumeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Resume.Verify, "Verify Mcu Resume", Description = "Petugas MCU memverifikasi resume atau mengembalikannya ke dokter", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.Resume.Resource, P.Resume.Verify)]
        public async Task<IActionResult> Verify(Guid id, [FromBody] McuResumeActionRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.VerifyAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Resume diverifikasi.", _logger, P.Resume.Resource, P.Resume.Verify,
                x => $"Resume order MCU {x.OrderNumber} versi {x.VersionNumber} diverifikasi {x.VerifiedByName}.");

        [HttpPost("reopen")]
        [ProducesResponseType(typeof(ApiResponse<McuResumeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Resume.Reopen, "Reopen Mcu Resume", Description = "Dokter MCU yang ditetapkan membuka ulang resume menjadi versi baru", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.Resume.Resource, P.Resume.Reopen)]
        public async Task<IActionResult> Reopen(Guid id, [FromBody] ReopenMcuResumeRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.ReopenAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Resume dibuka ulang.", _logger, P.Resume.Resource, P.Resume.Reopen,
                // Alasan koreksi dapat menyebut isi klinis — tidak dicatat.
                x => $"Resume order MCU {x.OrderNumber} dibuka ulang menjadi versi {x.VersionNumber} oleh {x.AuthorDoctorName}.");
    }
}
