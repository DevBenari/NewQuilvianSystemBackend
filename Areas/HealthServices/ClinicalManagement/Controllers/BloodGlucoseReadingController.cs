using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    /// <summary>
    /// GDS bangsal — <c>BE-RWI-119</c>, api-contract <c>keperawatan</c> 0.5.0 bagian 7.7.
    /// </summary>
    /// <remarks>
    /// <b>Satu-satunya jalur tulis GDS bangsal.</b> Satuan wajib dipilih tanpa bawaan. Hasil laboratorium tidak pernah ditulis
    /// ke sini (<c>RWI-DEC-148</c>). Hak akses <c>BloodGlucose : Read</c>/<c>: Create</c>/<c>: Update</c>.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/blood-glucose-readings")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Blood Glucose Reading",
        AreaName = "HealthServices",
        ControllerName = "BloodGlucose",
        Description = "Gula darah sewaktu bangsal yang diukur perawat",
        SortOrder = 23)]
    [Tags("Health Services / Clinical Management / Blood Glucose Reading")]
    public class BloodGlucoseReadingController : ControllerBase
    {
        private readonly DailyMonitoringService _service;

        public BloodGlucoseReadingController(DailyMonitoringService service)
        {
            _service = service;
        }

        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<BloodGlucoseReadingResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Glucose", Description = "Melihat GDS bangsal pasien rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodGlucose", "Read")]
        public async Task<IActionResult> GetByEpisode(
            Guid episodeId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.GetGlucoseReadingsAsync(episodeId, from, to, cancellationToken));

        /// <summary>Mencatat GDS dari Pengawasan Harian. <c>400</c> satuan kosong atau nilai tidak mungkin untuk satuannya.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BloodGlucoseReadingResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Blood Glucose", Description = "Mencatat GDS bangsal", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BloodGlucose", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateBloodGlucoseReadingRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.RecordGlucoseAsync(request, this.IdempotencyKey(request.IdempotencyKey), User, this.CurrentUserId(), cancellationToken));

        /// <summary>Koreksi GDS; pelaksanaan sliding scale yang memakainya ditandai, tidak dihitung ulang.</summary>
        [HttpPut("{id:guid}/correct")]
        [ProducesResponseType(typeof(ApiResponse<BloodGlucoseReadingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Blood Glucose", Description = "Mengoreksi atau membatalkan GDS bangsal beralasan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodGlucose", "Update")]
        public async Task<IActionResult> Correct(
            Guid id,
            [FromBody] CorrectBloodGlucoseReadingRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.CorrectGlucoseAsync(id, request, User, this.CurrentUserId(), cancellationToken));

        /// <summary>Membatalkan GDS yang <b>belum</b> dipakai pelaksanaan sliding scale.</summary>
        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<BloodGlucoseReadingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Blood Glucose", Description = "Mengoreksi atau membatalkan GDS bangsal beralasan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodGlucose", "Update")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelClinicalMeasurementRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.CancelGlucoseAsync(id, request, User, this.CurrentUserId(), cancellationToken));

        [HttpGet("{id:guid}/revisions")]
        [ProducesResponseType(typeof(ApiResponse<List<BloodGlucoseReadingRevisionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Glucose", Description = "Melihat GDS bangsal pasien rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodGlucose", "Read")]
        public async Task<IActionResult> GetRevisions(Guid id, CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.GetGlucoseRevisionsAsync(id, cancellationToken));
    }
}
