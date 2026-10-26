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
    /// Data referensi MCU — <c>MCU-API-v1</c> grup Mcu Reference Data (<c>BE-MCU-006</c>):
    /// jenis Pemeriksaan MCU beserta isiannya, dan kategori kelayakan.
    /// </summary>
    /// <remarks>
    /// Dua sumber daya, satu resource <c>McuReferenceData</c>, sehingga satu controller
    /// (<c>AccessMenuSeeder</c> mendaftarkan action di bawah controller pemiliknya). Action sengaja
    /// tanpa <c>&lt;summary&gt;</c> XML. Kontrak grup ini tidak memuat <c>filters/metadata</c>.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-checkup-management")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Mcu Reference Data",
        AreaName = P.AreaName,
        ControllerName = P.ReferenceData.Resource,
        Description = "Jenis Pemeriksaan MCU beserta isian formulirnya, dan kategori kelayakan",
        SortOrder = 2
    )]
    [Tags("Health Services / Medical Checkup Management / Mcu Reference Data")]
    public class McuReferenceDataController : ControllerBase
    {
        private readonly McuReferenceDataService _service;
        private readonly LoggerService _logger;

        public McuReferenceDataController(McuReferenceDataService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("examination-types")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<McuExaminationTypeResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Mcu Reference Data", Description = "Melihat daftar jenis Pemeriksaan MCU", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.ReferenceData.Resource, P.Read)]
        public async Task<IActionResult> GetExaminationTypes([FromQuery] McuExaminationTypePagedQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<McuExaminationTypeResponse>>.Ok(
                await _service.GetExaminationTypesAsync(query, cancellationToken), "Daftar jenis pemeriksaan MCU berhasil diambil."));

        [HttpGet("examination-types/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<McuExaminationTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Mcu Reference Data", Description = "Melihat rincian jenis Pemeriksaan MCU beserta isiannya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.ReferenceData.Resource, P.Read)]
        public async Task<IActionResult> GetExaminationType(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetExaminationTypeAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Jenis pemeriksaan MCU tidak ditemukan."))
                : Ok(ApiResponse<McuExaminationTypeDetailResponse>.Ok(result, "Rincian jenis pemeriksaan MCU berhasil diambil."));
        }

        [HttpPost("examination-types")]
        [ProducesResponseType(typeof(ApiResponse<McuExaminationTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Create, "Create Mcu Reference Data", Description = "Membuat jenis Pemeriksaan MCU beserta isiannya", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.ReferenceData.Resource, P.Create)]
        public async Task<IActionResult> CreateExaminationType([FromBody] CreateMcuExaminationTypeRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.CreateExaminationTypeAsync(request, McuHttp.ActorId(User), cancellationToken),
                "Jenis pemeriksaan MCU berhasil dibuat.", _logger, P.ReferenceData.Resource, P.Create,
                x => $"Jenis pemeriksaan MCU {x.ExaminationTypeCode} dibuat dengan {x.FieldCount} isian.");

        [HttpPut("examination-types/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<McuExaminationTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Mcu Reference Data", Description = "Mengubah jenis Pemeriksaan MCU beserta isiannya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.ReferenceData.Resource, P.Update)]
        public async Task<IActionResult> UpdateExaminationType(Guid id, [FromBody] UpdateMcuExaminationTypeRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.UpdateExaminationTypeAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Jenis pemeriksaan MCU berhasil diperbarui.", _logger, P.ReferenceData.Resource, P.Update,
                x => $"Jenis pemeriksaan MCU {x.ExaminationTypeCode} diperbarui; {x.FieldCount} isian; " +
                     $"{(x.IsActive ? "aktif" : "nonaktif")}.");

        [HttpGet("fitness-categories")]
        [ProducesResponseType(typeof(ApiResponse<List<McuFitnessCategoryResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Mcu Reference Data", Description = "Melihat daftar kategori kelayakan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.ReferenceData.Resource, P.Read)]
        public async Task<IActionResult> GetFitnessCategories([FromQuery] McuFitnessCategoryQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<List<McuFitnessCategoryResponse>>.Ok(
                await _service.GetFitnessCategoriesAsync(query, cancellationToken), "Daftar kategori kelayakan berhasil diambil."));

        [HttpPost("fitness-categories")]
        [ProducesResponseType(typeof(ApiResponse<McuFitnessCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Create, "Create Mcu Reference Data", Description = "Membuat kategori kelayakan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.ReferenceData.Resource, P.Create)]
        public async Task<IActionResult> CreateFitnessCategory([FromBody] CreateMcuFitnessCategoryRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.CreateFitnessCategoryAsync(request, McuHttp.ActorId(User), cancellationToken),
                "Kategori kelayakan berhasil dibuat.", _logger, P.ReferenceData.Resource, P.Create,
                x => $"Kategori kelayakan {x.FitnessCategoryCode} dibuat.");

        [HttpPut("fitness-categories/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<McuFitnessCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Update, "Update Mcu Reference Data", Description = "Mengubah atau menonaktifkan kategori kelayakan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.ReferenceData.Resource, P.Update)]
        public async Task<IActionResult> UpdateFitnessCategory(Guid id, [FromBody] UpdateMcuFitnessCategoryRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.UpdateFitnessCategoryAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Kategori kelayakan berhasil diperbarui.", _logger, P.ReferenceData.Resource, P.Update,
                x => $"Kategori kelayakan {x.FitnessCategoryCode} diperbarui; {(x.IsActive ? "aktif" : "nonaktif")}.");
    }
}
