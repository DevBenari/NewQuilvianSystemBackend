using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Constants.OpticalPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Controllers
{
    /// <summary>
    /// Pemeriksaan refraksi — <c>OPT-API-v1</c> grup Opt Refraction Exam (<c>BE-OPT-011</c>): daftar, detail, mulai, isi
    /// hasil ukur, simpan final, revisi, dan batal.
    /// </summary>
    /// <remarks>
    /// Selain hak akses, service menegakkan profesi RO/Sp.M serta SIP dan STR berlaku (<c>OPT-VAL-001</c>..<c>003</c>).
    /// Kalimat log hanya memuat nomor pemeriksaan dan nomor versi, tidak pernah nilai ukur (Sensitif). Action sengaja
    /// tanpa <c>&lt;summary&gt;</c> XML.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/optical-management/refraction-exams")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Opt Refraction Exam",
        AreaName = P.AreaName,
        ControllerName = P.RefractionExam.Resource,
        Description = "Pemeriksaan refraksi oleh RO/Sp.M: hasil ukur berversi",
        SortOrder = 3
    )]
    [Tags("Health Services / Optical Management / Opt Refraction Exam")]
    public class OptRefractionExamController : ControllerBase
    {
        private readonly OptRefractionExamService _service;
        private readonly LoggerService _logger;

        public OptRefractionExamController(OptRefractionExamService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OptRefractionExamResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Opt Refraction Exam", Description = "Melihat pemeriksaan refraksi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.RefractionExam.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] OptRefractionExamListQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<OptRefractionExamResponse>>.Ok(
                await _service.GetListAsync(query, cancellationToken), "Daftar pemeriksaan refraksi berhasil diambil."));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OptRefractionExamResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Opt Refraction Exam", Description = "Melihat pemeriksaan refraksi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.RefractionExam.Resource, P.Read)]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken = default) =>
            OptHttp.Map(this, await _service.GetDetailAsync(id, cancellationToken), "Detail pemeriksaan refraksi berhasil diambil.");

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OptRefractionExamResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.Create, "Create Opt Refraction Exam", Description = "Memulai pemeriksaan refraksi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.RefractionExam.Resource, P.Create)]
        public async Task<IActionResult> Start([FromBody] OptRefractionExamStartRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.StartAsync(request, OptHttp.ActorId(User), cancellationToken),
                "Pemeriksaan refraksi dimulai.", _logger, P.RefractionExam.Resource, P.Create,
                x => $"Pemeriksaan {x.ExamNumber} dimulai oleh {x.ExaminerName}.");

        [HttpPut("{id:guid}/measurements")]
        [ProducesResponseType(typeof(ApiResponse<OptRefractionExamResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.Update, "Update Opt Refraction Exam", Description = "Mengisi hasil ukur selama draf", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.RefractionExam.Resource, P.Update)]
        public async Task<IActionResult> UpdateMeasurements(Guid id, [FromBody] OptRefractionExamMeasurementRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.UpdateMeasurementsAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                "Hasil ukur tersimpan.", _logger, P.RefractionExam.Resource, P.Update,
                x => $"Hasil ukur draf pemeriksaan {x.ExamNumber} diperbarui.");

        [HttpPost("{id:guid}/finalize")]
        [ProducesResponseType(typeof(ApiResponse<OptRefractionExamResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.RefractionExam.Finalize, "Finalize Opt Refraction Exam", Description = "Menyimpan final pemeriksaan refraksi", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.RefractionExam.Resource, P.RefractionExam.Finalize)]
        public async Task<IActionResult> Finalize(Guid id, [FromBody] OptRefractionExamFinalizeRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.FinalizeAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                "Pemeriksaan refraksi disimpan final.", _logger, P.RefractionExam.Resource, P.RefractionExam.Finalize,
                x => $"Pemeriksaan {x.ExamNumber} difinalkan oleh {x.Current?.RevisedByName}.");

        [HttpPost("{id:guid}/revisions")]
        [ProducesResponseType(typeof(ApiResponse<OptRefractionExamResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.RefractionExam.Revise, "Revise Opt Refraction Exam", Description = "Merevisi hasil ukur sesudah final (tanpa tagih ulang)", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.RefractionExam.Resource, P.RefractionExam.Revise)]
        public async Task<IActionResult> Revise(Guid id, [FromBody] OptRefractionExamReviseRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.ReviseAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                "Revisi hasil ukur tersimpan.", _logger, P.RefractionExam.Resource, P.RefractionExam.Revise,
                x => $"Pemeriksaan {x.ExamNumber} direvisi ke versi {x.CurrentRevisionNo} oleh {x.Current?.RevisedByName}.");

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<OptRefractionExamResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.RefractionExam.Cancel, "Cancel Opt Refraction Exam", Description = "Membatalkan pemeriksaan draf", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission(P.RefractionExam.Resource, P.RefractionExam.Cancel)]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] OptRefractionExamCancelRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.CancelAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                "Pemeriksaan refraksi dibatalkan.", _logger, P.RefractionExam.Resource, P.RefractionExam.Cancel,
                x => $"Pemeriksaan {x.ExamNumber} dibatalkan.");
    }
}
