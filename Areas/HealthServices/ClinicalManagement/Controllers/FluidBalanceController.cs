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
    /// Entri cairan masuk dan keluar serta balance — <c>BE-RWI-119</c>, <c>BE-RWI-120</c>, <c>BE-RWI-122</c>, api-contract
    /// <c>keperawatan</c> 0.5.0 bagian 7.6.
    /// </summary>
    /// <remarks>
    /// <b>Arketipe: sub-proses ter-scope episode</b> dengan entri terukur <c>Active</c>/<c>Cancelled</c> beserta revisi.
    /// Tidak ada <c>DELETE</c>. Hak akses <c>FluidBalance : Read</c>/<c>: Create</c>/<c>: Update</c>; service menjaga episode
    /// berjalan dan penempatan unit.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/fluid-balance-entries")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Fluid Balance",
        AreaName = "HealthServices",
        ControllerName = "FluidBalance",
        Description = "Cairan masuk dan keluar pasien rawat inap beserta balance per shift dan 24 jam",
        SortOrder = 22)]
    [Tags("Health Services / Clinical Management / Fluid Balance")]
    public class FluidBalanceController : ControllerBase
    {
        private readonly DailyMonitoringService _service;

        public FluidBalanceController(DailyMonitoringService service)
        {
            _service = service;
        }

        /// <summary>Entri satu rentang waktu (bawaan 24 jam terakhir, paling panjang 31 hari).</summary>
        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FluidBalanceEntryResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Fluid Balance", Description = "Melihat entri cairan dan balance pasien rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("FluidBalance", "Read")]
        public async Task<IActionResult> GetByEpisode(
            Guid episodeId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] bool includeCancelled = false,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.GetFluidEntriesAsync(episodeId, from, to, includeCancelled, cancellationToken));

        /// <summary>
        /// Balance per shift dan 24 jam satu hari. Unit tanpa konfigurasi shift hanya mendapat baris 24 jam
        /// (<c>ShiftConfigurationMissing = true</c>).
        /// </summary>
        [HttpGet("episodes/{episodeId:guid}/totals")]
        [ProducesResponseType(typeof(ApiResponse<FluidTotalsResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Fluid Balance", Description = "Melihat entri cairan dan balance pasien rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("FluidBalance", "Read")]
        public async Task<IActionResult> GetTotals(
            Guid episodeId,
            [FromQuery] DateOnly? date,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.GetFluidTotalsAsync(episodeId, date, cancellationToken));

        /// <summary>Mencatat entri cairan; sumber Obat wajib menunjuk dosis MAR yang sudah diberikan.</summary>
        /// <remarks>
        /// <c>400</c> volume, sumber, atau waktu tidak sah; <c>403</c> tidak ditempatkan di unit; <c>409</c> dosis belum
        /// diberikan atau sudah punya entri intake; <c>422</c> perawatan ditutup atau dosis milik pasien lain.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<FluidBalanceEntryResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Fluid Balance", Description = "Mencatat entri cairan masuk dan keluar", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("FluidBalance", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateFluidBalanceEntryRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.RecordFluidAsync(request, this.IdempotencyKey(request.IdempotencyKey), User, this.CurrentUserId(), cancellationToken));

        [HttpPut("{id:guid}/correct")]
        [ProducesResponseType(typeof(ApiResponse<FluidBalanceEntryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Fluid Balance", Description = "Mengoreksi atau membatalkan entri cairan beralasan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("FluidBalance", "Update")]
        public async Task<IActionResult> Correct(
            Guid id,
            [FromBody] CorrectFluidBalanceEntryRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.CorrectFluidAsync(id, request, User, this.CurrentUserId(), cancellationToken));

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<FluidBalanceEntryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Fluid Balance", Description = "Mengoreksi atau membatalkan entri cairan beralasan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("FluidBalance", "Update")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelClinicalMeasurementRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.CancelFluidAsync(id, request, User, this.CurrentUserId(), cancellationToken));

        [HttpGet("{id:guid}/revisions")]
        [ProducesResponseType(typeof(ApiResponse<List<FluidBalanceEntryRevisionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Fluid Balance", Description = "Melihat entri cairan dan balance pasien rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("FluidBalance", "Read")]
        public async Task<IActionResult> GetRevisions(Guid id, CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.GetFluidRevisionsAsync(id, cancellationToken));
    }
}
