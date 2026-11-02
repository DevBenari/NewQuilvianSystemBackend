using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Controllers
{
    /// <summary>
    /// Controller pengelola daftar layanan medis yang belum dapat dihitung jasanya (MdfUnresolvedService).
    /// Mengimplementasikan kontrak MDF-API-1.0 §6 dan matriks izin MDF-PERM-1.0 §2.
    /// Arketipe: Transaksi / Worklist dan Antrean (transaction-endpoint-standard.md §2.2).
    /// 
    /// CATATAN PENTING ARSITEKTUR & KEAMANAN:
    /// 1. Endpoint DELETE sengaja DITOLAK sesuai MF-DEC-018 dan api-contract.md §9. Menghapus baris berarti menyembunyikan masalah yang seharusnya diaudit.
    /// 2. Pengesampingan (Waive) menuntut izin tersendiri [AccessPermission("MedicalFeeUnresolvedServices", "Waive")] (MDF-PERM-1.0 §3.1).
    /// 3. Periode kalkulasi tidak dapat ditutup (Closed) bila masih ada baris berstatus Open (MDF-DES-016).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-fee-management/unresolved-services")]
    [AccessController(
        moduleCode: "MEDICAL_FEE_MANAGEMENT",
        moduleName: "Medical Fee Management",
        displayName: "Medical Fee Unresolved Service",
        AreaName = "HealthServices",
        ControllerName = "MedicalFeeUnresolvedServices",
        Description = "Mengelola daftar, rekapitulasi, dan pengesampingan layanan medis yang belum dapat dihitung jasanya",
        SortOrder = 4
    )]
    [Tags("Health Services / Medical Fee Management / Unresolved Services")]
    public class MedicalFeeUnresolvedServicesController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalFeeManagement.FeeCalculation.Unresolved";

        private readonly MedicalFeeCalculationService _calculationService;
        private readonly LoggerService _loggerService;

        public MedicalFeeUnresolvedServicesController(
            MedicalFeeCalculationService calculationService,
            LoggerService loggerService)
        {
            _calculationService = calculationService;
            _loggerService = loggerService;
        }

        // =====================================================================
        // 1. GET /filters/metadata (Konfigurasi Filter dan Opsi Pengurutan)
        // =====================================================================

        /// <summary>
        /// Mengambil konfigurasi metadata filter, sort, dan opsi status/alasan untuk tabel layanan unresolved.
        /// </summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<UnresolvedFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Unresolved Service Metadata", Description = "Melihat konfigurasi metadata filter layanan belum terhitung", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeUnresolvedServices", AccessTypes.Read)]
        public IActionResult GetFilterMetadata()
        {
            var metadata = _calculationService.GetUnresolvedFilterMetadata();
            return Ok(ApiResponse<UnresolvedFilterMetadataResponse>.Ok(metadata, "Metadata filter layanan belum terhitung berhasil diambil."));
        }

        // =====================================================================
        // 2. GET /summary (Rekapitulasi Layanan Belum Terhitung)
        // =====================================================================

        /// <summary>
        /// Mengambil ringkasan rekapitulasi layanan belum terhitung per alasan dan modul sumber (MDF-API-1.0 §6).
        /// Membantu petugas mengetahui secara langsung apa yang perlu dilengkapi tanpa membuka layar lain.
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<UnresolvedSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Unresolved Service Summary", Description = "Melihat ringkasan rekapitulasi layanan belum terhitung per alasan dan modul sumber", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeUnresolvedServices", AccessTypes.Read)]
        public async Task<IActionResult> GetSummary([FromQuery] Guid? periodId, CancellationToken ct = default)
        {
            var summary = await _calculationService.GetUnresolvedSummaryAsync(periodId, ct);
            return Ok(ApiResponse<UnresolvedSummaryResponse>.Ok(summary, "Ringkasan rekapitulasi layanan belum terhitung berhasil diambil."));
        }

        // =====================================================================
        // 3. GET / (Daftar Layanan Belum Terhitung Terpaginasi)
        // =====================================================================

        /// <summary>
        /// Mengambil daftar layanan belum terhitung secara terpaginasi dengan filter periodId, reason, status, sourceDomain, dan pencarian teks.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<UnresolvedServiceItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Unresolved Service List", Description = "Melihat daftar layanan medis belum terhitung secara terpaginasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeUnresolvedServices", AccessTypes.Read)]
        public async Task<IActionResult> GetPaged([FromQuery] UnresolvedServicePagedQuery query, CancellationToken ct = default)
        {
            var result = await _calculationService.GetUnresolvedPagedAsync(query, ct);
            return Ok(ApiResponse<PagedResult<UnresolvedServiceItemResponse>>.Ok(result, "Daftar layanan medis belum terhitung berhasil diambil."));
        }

        // =====================================================================
        // 4. GET /{id} (Detail Satu Layanan Belum Terhitung)
        // =====================================================================

        /// <summary>
        /// Mengambil rincian satu baris layanan medis belum terhitung beserta alasan dan penjelasannya (MDF-API-1.0 §6).
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<UnresolvedServiceItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Medical Fee Unresolved Service Detail", Description = "Melihat rincian satu layanan medis belum terhitung beserta alasan dan penjelasannya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeUnresolvedServices", AccessTypes.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        {
            try
            {
                var result = await _calculationService.GetUnresolvedByIdAsync(id, ct);
                return Ok(ApiResponse<UnresolvedServiceItemResponse>.Ok(result, "Detail layanan medis belum terhitung berhasil diambil."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        // =====================================================================
        // 5. POST /{id}/waiver (Pengesampingan Layanan Belum Terhitung)
        // =====================================================================

        /// <summary>
        /// Mengesampingkan layanan belum terhitung dengan catatan wajib (MDF-DES-014, MDF-VAL-1.0 §7.1, T-46..T-48).
        /// Menuntut izin khusus 'Waive' (MDF-PERM-1.0 §3.1). Status berubah menjadi Waived dan tidak lagi menahan penutupan periode.
        /// </summary>
        [HttpPost("{id:guid}/waiver")]
        [ProducesResponseType(typeof(ApiResponse<UnresolvedServiceItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Waive", "Waive Medical Fee Unresolved Service", Description = "Mengesampingkan layanan medis belum terhitung dengan catatan resmi (MDF-DES-014, MDF-VAL-1.0 §7.1)", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("MedicalFeeUnresolvedServices", "Waive")]
        public async Task<IActionResult> Waive(
            Guid id,
            [FromBody] WaiveUnresolvedServiceRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _calculationService.WaiveUnresolvedAsync(id, request, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "MedicalFeeUnresolvedService.Waive", $"Layanan belum terhitung {id} berhasil dikesampingkan dengan catatan.", new { id, actorUserId, request.ResolutionNote });
                return Ok(ApiResponse<UnresolvedServiceItemResponse>.Ok(result, "Layanan belum terhitung berhasil dikesampingkan."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeUnresolvedService.Waive", ex.Message, new { id, actorUserId });
                var errorCode = ex.Message.Contains("MDF_WAIVER_NOTE_REQUIRED") ? "MDF_WAIVER_NOTE_REQUIRED" :
                                ex.Message.Contains("MDF_UNRESOLVED_STATE_INVALID") ? "MDF_UNRESOLVED_STATE_INVALID" : null;
                object? errorDetails = errorCode != null ? new { errorCode } : null;
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message, errorDetails));
            }
        }

        // =====================================================================
        // Internal Helpers
        // =====================================================================

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
