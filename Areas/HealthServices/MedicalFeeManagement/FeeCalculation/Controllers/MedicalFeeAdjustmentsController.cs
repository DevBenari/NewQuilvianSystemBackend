using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Controllers
{
    /// <summary>
    /// Controller pengelola daftar, filter metadata, dan audit langsung penyesuaian koreksi jasa medis (MdfServiceFeeAdjustment).
    /// Mengimplementasikan kontrak MDF-API-1.0 §5 dan matriks izin MDF-PERM-1.0 §2.
    /// Arketipe: Transaksi / Detail Akuntansi & Log Penyesuaian (transaction-endpoint-standard.md §2.2).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-fee-management/adjustments")]
    [AccessController(
        moduleCode: "MEDICAL_FEE_MANAGEMENT",
        moduleName: "Medical Fee Management",
        displayName: "Medical Fee Adjustment",
        AreaName = "HealthServices",
        ControllerName = "MedicalFeeAdjustments",
        Description = "Mengelola penelusuran, metadata, dan daftar penyesuaian koreksi jasa medis lintas hasil jasa",
        SortOrder = 6
    )]
    [Tags("Health Services / Medical Fee Management / Adjustments")]
    public class MedicalFeeAdjustmentsController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalFeeManagement.FeeCalculation.Adjustment";

        private readonly MedicalFeeAdjustmentService _adjustmentService;
        private readonly LoggerService _loggerService;

        public MedicalFeeAdjustmentsController(
            MedicalFeeAdjustmentService adjustmentService,
            LoggerService loggerService)
        {
            _adjustmentService = adjustmentService;
            _loggerService = loggerService;
        }

        // =====================================================================
        // 1. GET /filters/metadata
        // =====================================================================

        /// <summary>
        /// Mengambil konfigurasi metadata filter, opsi status, dan arah penyesuaian (Addition / Deduction).
        /// </summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<AdjustmentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("View", "View Medical Fee Adjustment Filters", Description = "Melihat metadata filter penyesuaian koreksi jasa medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeAdjustments", "View")]
        public IActionResult GetFilterMetadata()
        {
            var metadata = _adjustmentService.GetFilterMetadata();
            return Ok(ApiResponse<AdjustmentFilterMetadataResponse>.Ok(metadata, "Metadata filter penyesuaian koreksi berhasil diambil."));
        }

        // =====================================================================
        // 2. GET / (List Terpaginasi Lintas Hasil Jasa)
        // =====================================================================

        /// <summary>
        /// Mengambil daftar seluruh penyesuaian koreksi jasa medis secara terpaginasi.
        /// Mendukung filter berdasarkan serviceFeeId, status (Pending, Approved, Rejected), direction (Addition, Deduction), dan pencarian.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ServiceFeeAdjustmentResponse>>), StatusCodes.Status200OK)]
        [AccessAction("View", "View Medical Fee Adjustment List", Description = "Melihat daftar penyesuaian koreksi jasa medis terpaginasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeAdjustments", "View")]
        public async Task<IActionResult> GetPaged([FromQuery] AdjustmentPagedQuery query, CancellationToken ct = default)
        {
            var result = await _adjustmentService.GetAdjustmentsPagedAsync(query, ct);
            return Ok(ApiResponse<PagedResult<ServiceFeeAdjustmentResponse>>.Ok(result, "Daftar penyesuaian koreksi jasa medis berhasil diambil."));
        }

        // =====================================================================
        // 3. GET /{id} (Detail Tunggal Penyesuaian)
        // =====================================================================

        /// <summary>
        /// Mengambil rincian satu penyesuaian koreksi jasa medis langsung berdasarkan ID penyesuaian.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ServiceFeeAdjustmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("View", "View Medical Fee Adjustment Detail", Description = "Melihat detail satu penyesuaian koreksi jasa medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeAdjustments", "View")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        {
            try
            {
                var result = await _adjustmentService.GetAdjustmentByIdDirectAsync(id, ct);
                return Ok(ApiResponse<ServiceFeeAdjustmentResponse>.Ok(result, "Detail penyesuaian koreksi jasa medis berhasil diambil."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }
    }
}

