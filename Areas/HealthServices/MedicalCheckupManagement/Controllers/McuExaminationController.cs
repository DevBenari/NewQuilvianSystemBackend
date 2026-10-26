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
    /// Pemeriksaan MCU — <c>MCU-API-v1</c> grup Mcu Examination. <c>BE-MCU-021</c> memasang
    /// <i>Mulai pemeriksaan</i>, <c>BE-MCU-022</c> komponen (daftar, sinkron, tambah, jadwal ulang),
    /// <c>BE-MCU-023</c> hasil Pemeriksaan MCU, <c>BE-MCU-024</c> batal komponen + refund.
    /// </summary>
    /// <remarks>Action sengaja tanpa <c>&lt;summary&gt;</c> XML.</remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-checkup-management/orders/{id:guid}")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Mcu Examination",
        AreaName = P.AreaName,
        ControllerName = P.Examination.Resource,
        Description = "Pemeriksaan MCU: mulai pemeriksaan, komponen, dan hasil Pemeriksaan MCU",
        SortOrder = 5
    )]
    [Tags("Health Services / Medical Checkup Management / Mcu Examination")]
    public class McuExaminationController : ControllerBase
    {
        private readonly McuExaminationService _service;
        private readonly LoggerService _logger;

        public McuExaminationController(McuExaminationService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("start-examination")]
        [ProducesResponseType(typeof(ApiResponse<McuOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Examination.Start, "Start Mcu Examination", Description = "Mulai pemeriksaan: cek lunas/terjamin lalu buat pesanan Lab/Rad paket", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission(P.Examination.Resource, P.Examination.Start)]
        public async Task<IActionResult> Start(Guid id, [FromBody] StartMcuExaminationRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.StartAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Pemeriksaan MCU dimulai.", _logger, P.Examination.Resource, P.Examination.Start,
                x => $"Mulai pemeriksaan MCU {x.OrderNumber}: dasar {x.ClearanceBasisLabel}, " +
                     $"pesanan Lab {x.Components.Where(c => c.LabOrderNumber != null).Select(c => c.LabOrderNumber).Distinct().Count()}, " +
                     $"pesanan Rad {x.Components.Count(c => c.RadOrderNumber != null)}, dokter MCU {x.McuDoctorName}.");

        [HttpGet("components")]
        [ProducesResponseType(typeof(ApiResponse<List<McuOrderComponentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Mcu Examination", Description = "Melihat komponen order MCU beserta status dan pesanan Lab/Rad", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Examination.Resource, P.Read)]
        public async Task<IActionResult> GetComponents(Guid id, CancellationToken cancellationToken = default) =>
            McuHttp.Map(this, await _service.GetComponentsAsync(id, cancellationToken), "Komponen order MCU berhasil diambil.");

        [HttpPost("components/refresh")]
        [ProducesResponseType(typeof(ApiResponse<List<McuOrderComponentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Read, "Read Mcu Examination", Description = "Memuat ulang status komponen dari Laboratorium dan Radiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Examination.Resource, P.Read)]
        public async Task<IActionResult> RefreshComponents(Guid id, CancellationToken cancellationToken = default) =>
            McuHttp.Map(this, await _service.RefreshComponentsAsync(id, McuHttp.ActorId(User), cancellationToken), "Status komponen order MCU dimuat ulang.");

        [HttpPost("components")]
        [ProducesResponseType(typeof(ApiResponse<McuOrderComponentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Examination.AddComponent, "Add Mcu Component", Description = "Dokter MCU yang ditetapkan menambah pemeriksaan di luar paket", AccessType = AccessTypes.Create, SortOrder = 4)]
        [AccessPermission(P.Examination.Resource, P.Examination.AddComponent)]
        public async Task<IActionResult> AddComponent(Guid id, [FromBody] AddMcuComponentRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.AddComponentAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Pemeriksaan di luar paket ditambahkan.", _logger, P.Examination.Resource, P.Examination.AddComponent,
                x => $"Tambah pemeriksaan di luar paket pada order MCU {id}: {x.ComponentName} ({x.ComponentTypeLabel}), " +
                     $"pesanan {x.LabOrderNumber ?? x.RadOrderNumber ?? "-"}.");

        [HttpPatch("components/{componentId:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<McuOrderComponentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Examination.CancelComponent, "Cancel Mcu Component", Description = "Dokter MCU yang ditetapkan membatalkan komponen dan mengajukan refund nilai pengembaliannya", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.Examination.Resource, P.Examination.CancelComponent)]
        public async Task<IActionResult> CancelComponent(Guid id, Guid componentId, [FromBody] CancelMcuComponentRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.CancelComponentAsync(id, componentId, request, McuHttp.ActorId(User), cancellationToken),
                "Komponen dibatalkan.", _logger, P.Examination.Resource, P.Examination.CancelComponent,
                // Alasan pembatalan (mis. kehamilan) tidak dicatat di log aplikasi.
                x => $"Batal komponen {x.ComponentName} ({x.ComponentTypeLabel}) order MCU {id}, " +
                     $"pesanan {x.LabOrderNumber ?? x.RadOrderNumber ?? "-"}.");

        [HttpPatch("components/{componentId:guid}/reschedule")]
        [ProducesResponseType(typeof(ApiResponse<McuOrderComponentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Mcu Examination", Description = "Memindah tanggal komponen susulan tanpa memakai kuota", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Examination.Resource, P.Update)]
        public async Task<IActionResult> RescheduleComponent(Guid id, Guid componentId, [FromBody] RescheduleMcuComponentRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.RescheduleComponentAsync(id, componentId, request, McuHttp.ActorId(User), cancellationToken),
                "Komponen dijadwal ulang.", _logger, P.Examination.Resource, P.Update,
                x => $"Jadwal ulang komponen {x.ComponentName} order MCU {id} ke {McuScheduleService.FormatDate(x.ScheduledDate)}.");

        [HttpGet("components/{componentId:guid}/examination-result")]
        [ProducesResponseType(typeof(ApiResponse<McuExaminationResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Mcu Examination", Description = "Melihat isian dan hasil Pemeriksaan MCU", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Examination.Resource, P.Read)]
        public async Task<IActionResult> GetExaminationResult(Guid id, Guid componentId, CancellationToken cancellationToken = default) =>
            McuHttp.Map(this, await _service.GetResultAsync(id, componentId, cancellationToken), "Hasil Pemeriksaan MCU berhasil diambil.");

        [HttpPut("components/{componentId:guid}/examination-result")]
        [ProducesResponseType(typeof(ApiResponse<McuExaminationResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Mcu Examination", Description = "Mengisi atau mengubah hasil Pemeriksaan MCU", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Examination.Resource, P.Update)]
        public async Task<IActionResult> SaveExaminationResult(Guid id, Guid componentId, [FromBody] SaveMcuExaminationResultRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.SaveResultAsync(id, componentId, request, McuHttp.ActorId(User), cancellationToken),
                "Hasil Pemeriksaan MCU disimpan.", _logger, P.Examination.Resource, P.Update,
                // Nilai dan kesimpulan sensitif — log hanya menyebut order, pemeriksaan, dan statusnya.
                x => $"Hasil {x.ComponentName} order MCU {x.OrderNumber} disimpan, status {x.ComponentStatusLabel}.");
    }
}
