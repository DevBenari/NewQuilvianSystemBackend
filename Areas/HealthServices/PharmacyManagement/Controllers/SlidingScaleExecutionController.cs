using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers
{
    /// <summary>
    /// Pelaksanaan sliding scale insulin oleh perawat — <c>BE-RWI-123</c>, api-contract <c>keperawatan</c> 0.5.0 bagian 7.12.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Arketipe: sub-proses ter-scope order</b> — pratinjau tanpa simpan, lalu satu transaksi yang menulis GDS, dosis MAR,
    /// dan pelaksanaan. Tidak ada ubah maupun hapus; pelaksanaan dibatalkan hanya bersama koreksi dosis MAR menjadi
    /// <c>Cancelled</c>.
    /// </para>
    /// <para>
    /// <b>Hak akses.</b> <c>SlidingScaleExecution : Read</c>/<c>: Create</c>; menyimpan juga memeriksa
    /// <c>MedicationAdministration : Create</c> di service. <c>Idempotency-Key</c> wajib.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/pharmacy-management/sliding-scale-executions")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY",
        moduleName: "Health Service Pharmacy",
        displayName: "Sliding Scale Execution",
        AreaName = "HealthServices",
        ControllerName = "SlidingScaleExecution",
        Description = "Pelaksanaan protokol sliding scale insulin oleh perawat",
        SortOrder = 16)]
    [Tags("Health Services / Pharmacy Management / Sliding Scale Execution")]
    public class SlidingScaleExecutionController : ControllerBase
    {
        private readonly SlidingScaleExecutionService _service;

        public SlidingScaleExecutionController(SlidingScaleExecutionService service)
        {
            _service = service;
        }

        /// <summary>Rentang dan dosis hitung <b>tanpa menyimpan</b> — perawat melihat sebelum memberi.</summary>
        /// <remarks>
        /// <c>400</c> satuan GDS kosong; <c>409</c> order tidak aktif atau satuan berbeda dari protokol; <c>422</c> GDS bukan
        /// GDS bangsal atau nilai tidak masuk tepat satu rentang.
        /// </remarks>
        [HttpPost("preview")]
        [ProducesResponseType(typeof(ApiResponse<SlidingScalePreviewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Read", "Read Sliding Scale Execution", Description = "Melihat pratinjau dan riwayat pelaksanaan sliding scale", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SlidingScaleExecution", "Read")]
        public async Task<IActionResult> Preview(
            [FromBody] PreviewSlidingScaleExecutionRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.PreviewAsync(request, cancellationToken));

        /// <summary>Mencatat GDS, dosis MAR, dan pelaksanaan dalam satu transaksi idempoten.</summary>
        /// <remarks>
        /// <c>400</c> satuan kosong, dosis berbeda tanpa alasan, kunci pengiriman kosong; <c>403</c> tidak ditempatkan di unit
        /// atau tanpa hak mencatat pemberian obat; <c>409</c> order dihentikan, versi order basi, GDS sudah dipakai, satuan
        /// berbeda; <c>422</c> GDS bukan GDS bangsal atau perawatan ditutup. Galat pada langkah mana pun → tidak ada yang tercatat.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<SlidingScaleExecutionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Sliding Scale Execution", Description = "Mencatat pelaksanaan sliding scale insulin", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("SlidingScaleExecution", "Create")]
        public async Task<IActionResult> Execute(
            [FromBody] CreateSlidingScaleExecutionRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.ExecuteAsync(request, this.IdempotencyKey(request.IdempotencyKey), User, this.CurrentUserId(), cancellationToken));

        /// <summary>Riwayat pelaksanaan satu episode (bawaan 7 hari terakhir).</summary>
        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<SlidingScaleExecutionListItem>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Sliding Scale Execution", Description = "Melihat pratinjau dan riwayat pelaksanaan sliding scale", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SlidingScaleExecution", "Read")]
        public async Task<IActionResult> GetByEpisode(
            Guid episodeId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.GetByEpisodeAsync(episodeId, from, to, cancellationToken));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SlidingScaleExecutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Sliding Scale Execution", Description = "Melihat pratinjau dan riwayat pelaksanaan sliding scale", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SlidingScaleExecution", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.GetAsync(id, this.CurrentUserId(), cancellationToken));
    }
}
