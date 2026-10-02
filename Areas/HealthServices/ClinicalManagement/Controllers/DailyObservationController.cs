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
    /// Observasi harian dan ringkasan Pengawasan Harian — <c>BE-RWI-119</c>, <c>BE-RWI-120</c>, <c>BE-RWI-122</c>,
    /// api-contract <c>keperawatan</c> 0.5.0 bagian 7.5 dan 7.8.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Ringkasan tinggal di controller ini, bukan di controller sendiri.</b> Kontrak 7.5 menetapkan ringkasan dengan butir
    /// <c>DailyObservation : Read</c>, sedangkan argumen pertama <c>[AccessPermission]</c> wajib sama dengan
    /// <c>ControllerName</c>. Supaya URL <c>daily-monitoring/…</c>, judul grup "Daily Monitoring", dan butir hak akses sama
    /// persis dengan kontrak, action ringkasan memakai rute absolut dan judul grup pada tingkat method.
    /// </para>
    /// <para>Hak akses <c>DailyObservation : Read</c>/<c>: Create</c>/<c>: Update</c>.</para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/daily-observations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Daily Observation",
        AreaName = "HealthServices",
        ControllerName = "DailyObservation",
        Description = "Observasi harian dan ringkasan Pengawasan Harian pasien rawat inap",
        SortOrder = 24)]
    [Tags("Health Services / Clinical Management / Daily Observation")]
    public class DailyObservationController : ControllerBase
    {
        private readonly DailyMonitoringService _service;

        public DailyObservationController(DailyMonitoringService service)
        {
            _service = service;
        }

        /// <summary>
        /// Satu hari Pengawasan Harian: tanda vital, nyeri terakhir dari Monitoring Nyeri, GDS, cairan per shift dan 24 jam,
        /// observasi, dan pengingat dosis diberikan tanpa entri intake.
        /// </summary>
        /// <remarks><c>date</c> = hari jam dinding rumah sakit; hari mengikuti jam mulai shift pertama unit, atau 00.00 tanpa shift.</remarks>
        [HttpGet("~/api/v1/health-services/clinical-management/daily-monitoring/episodes/{episodeId:guid}/summary")]
        [Tags("Health Services / Clinical Management / Daily Monitoring")]
        [ProducesResponseType(typeof(ApiResponse<DailyMonitoringSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Daily Observation", Description = "Melihat observasi harian dan ringkasan Pengawasan Harian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DailyObservation", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid episodeId,
            [FromQuery] DateOnly? date,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.GetSummaryAsync(episodeId, date, cancellationToken));

        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<DailyObservationResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Daily Observation", Description = "Melihat observasi harian dan ringkasan Pengawasan Harian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DailyObservation", "Read")]
        public async Task<IActionResult> GetByEpisode(
            Guid episodeId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.GetObservationsAsync(episodeId, from, to, cancellationToken));

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DailyObservationResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Daily Observation", Description = "Mencatat diet, mobilisasi, lingkar perut, dan agitasi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DailyObservation", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateDailyObservationRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.RecordObservationAsync(request, this.IdempotencyKey(request.IdempotencyKey), User, this.CurrentUserId(), cancellationToken));

        [HttpPut("{id:guid}/correct")]
        [ProducesResponseType(typeof(ApiResponse<DailyObservationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Daily Observation", Description = "Mengoreksi atau membatalkan observasi harian beralasan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DailyObservation", "Update")]
        public async Task<IActionResult> Correct(
            Guid id,
            [FromBody] CorrectDailyObservationRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.CorrectObservationAsync(id, request, User, this.CurrentUserId(), cancellationToken));

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<DailyObservationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Daily Observation", Description = "Mengoreksi atau membatalkan observasi harian beralasan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DailyObservation", "Update")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelClinicalMeasurementRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.CancelObservationAsync(id, request, User, this.CurrentUserId(), cancellationToken));
    }
}
