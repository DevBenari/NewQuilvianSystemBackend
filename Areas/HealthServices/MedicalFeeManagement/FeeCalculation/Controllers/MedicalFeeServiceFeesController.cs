using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    /// Controller pengelola hasil perhitungan jasa medis per penerima (MdfServiceFee) dan rincian tindakannya (MdfServiceFeeDetail).
    /// Mengimplementasikan kontrak MDF-API-1.0 §5 dan matriks izin MDF-PERM-1.0 §2.
    /// Arketipe: Transaksi / Detail Akuntansi & Rincian Transaksi (transaction-endpoint-standard.md §2.2).
    /// 
    /// CATATAN PENTING ARSITEKTUR & KEAMANAN:
    /// 1. Endpoint POST /service-fees manual dan PUT /service-fees/{id} sengaja DITOLAK (api-contract.md §9).
    ///    Hasil jasa hanya lahir secara deterministik dari perhitungan periode (MDF-DES-010).
    /// 2. Setiap rincian tindakan pada GET /{id}/details memuat sharingRuleId dan agreementNumber
    ///    untuk memenuhi keterlacakan penuh "Dari mana angkanya" (MDF-DES-010, MDF-DES-012).
    /// 3. Verifikasi dapat dilakukan per individu (POST /{id}/verification) atau massal per periode (POST /batch-verification).
    ///    Pengembalian (POST /{id}/return) mengembalikan status ke Calculated dan menurunkan periode jika perlu (MDF-VAL-1.0 §4.7).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-fee-management/service-fees")]
    [AccessController(
        moduleCode: "MEDICAL_FEE_MANAGEMENT",
        moduleName: "Medical Fee Management",
        displayName: "Medical Fee Service Fee",
        AreaName = "HealthServices",
        ControllerName = "MedicalFeeServiceFees",
        Description = "Mengelola hasil perhitungan jasa medis per penerima beserta rincian dan verifikasinya",
        SortOrder = 5
    )]
    [Tags("Health Services / Medical Fee Management / Service Fees")]
    public class MedicalFeeServiceFeesController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalFeeManagement.FeeCalculation.ServiceFee";

        private readonly MedicalFeePeriodService _periodService;
        private readonly MedicalFeeAdjustmentService _adjustmentService;
        private readonly MedicalFeeAccessGuard _accessGuard;
        private readonly LoggerService _loggerService;

        public MedicalFeeServiceFeesController(
            MedicalFeePeriodService periodService,
            MedicalFeeAdjustmentService adjustmentService,
            MedicalFeeAccessGuard accessGuard,
            LoggerService loggerService)
        {
            _periodService = periodService;
            _adjustmentService = adjustmentService;
            _accessGuard = accessGuard;
            _loggerService = loggerService;
        }

        // =====================================================================
        // 1. GET /filters/metadata
        // =====================================================================

        /// <summary>
        /// Mengambil konfigurasi metadata filter, opsi pengurutan, dan status hasil jasa medis.
        /// </summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ServiceFeeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("View", "View Medical Fee Service Fee Filters", Description = "Melihat metadata filter hasil jasa medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeServiceFees", "View")]
        public IActionResult GetFilterMetadata()
        {
            var metadata = _periodService.GetServiceFeeFilterMetadata();
            return Ok(ApiResponse<ServiceFeeFilterMetadataResponse>.Ok(metadata, "Metadata filter hasil jasa medis berhasil diambil."));
        }

        // =====================================================================
        // 2. GET / (List Terpaginasi)
        // =====================================================================

        /// <summary>
        /// Mengambil daftar hasil perhitungan jasa medis (MdfServiceFee) secara terpaginasi.
        /// Mendukung filter berdasarkan periodId, payeeType, payeeReferenceId, status, dan pencarian nomor hasil jasa.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ServiceFeeHeaderResponse>>), StatusCodes.Status200OK)]
        [AccessAction("View", "View Medical Fee Service Fee List", Description = "Melihat daftar hasil perhitungan jasa medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeServiceFees", "View")]
        public async Task<IActionResult> GetPaged([FromQuery] ServiceFeePagedQuery query, CancellationToken ct = default)
        {
            var actorContext = await _accessGuard.ResolveActorContextAsync(User, ct);
            var result = await _periodService.GetServiceFeesPagedAsync(query, actorContext, ct);
            return Ok(ApiResponse<PagedResult<ServiceFeeHeaderResponse>>.Ok(result, "Daftar hasil perhitungan jasa medis berhasil diambil."));
        }

        // =====================================================================
        // 3. GET /{id} (Detail Tunggal)
        // =====================================================================

        /// <summary>
        /// Mengambil rincian header satu hasil perhitungan jasa medis berdasarkan ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ServiceFeeHeaderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("View", "View Medical Fee Service Fee Detail", Description = "Melihat detail header satu hasil perhitungan jasa medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeServiceFees", "View")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        {
            try
            {
                var actorContext = await _accessGuard.ResolveActorContextAsync(User, ct);
                var result = await _periodService.GetServiceFeeByIdAsync(id, actorContext, ct);
                return Ok(ApiResponse<ServiceFeeHeaderResponse>.Ok(result, "Detail hasil perhitungan jasa medis berhasil diambil."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        // =====================================================================
        // 4. GET /{id}/details (Rincian Tindakan / Breakdown — "Dari Mana Angkanya")
        // =====================================================================

        /// <summary>
        /// Mengambil rincian per layanan/tindakan (MdfServiceFeeDetail) untuk satu hasil jasa medis.
        /// Menyediakan data audit akuntansi "Dari mana angkanya" (MDF-API-1.0 §5, MDF-DES-010).
        /// </summary>
        [HttpGet("{id:guid}/details")]
        [ProducesResponseType(typeof(ApiResponse<List<ServiceFeeDetailBreakdownResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("View", "View Medical Fee Service Fee Breakdown", Description = "Melihat rincian tindakan medis pembentuk nilai jasa", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeServiceFees", "View")]
        public async Task<IActionResult> GetDetails(Guid id, CancellationToken ct = default)
        {
            try
            {
                var actorContext = await _accessGuard.ResolveActorContextAsync(User, ct);
                var result = await _periodService.GetServiceFeeDetailsAsync(id, actorContext, ct);
                return Ok(ApiResponse<List<ServiceFeeDetailBreakdownResponse>>.Ok(result, "Rincian tindakan pembentuk jasa medis berhasil diambil."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        // =====================================================================
        // 5. POST /{id}/verification (Verifikasi Satu Hasil Jasa)
        // =====================================================================

        /// <summary>
        /// Memverifikasi satu hasil jasa medis: Calculated -> Verified (MDF-STATE-1.0 §2, MDF-VAL-1.0 §4.7).
        /// </summary>
        [HttpPost("{id:guid}/verification")]
        [ProducesResponseType(typeof(ApiResponse<ServiceFeeHeaderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Verify", "Verify Medical Fee Service Fee", Description = "Memverifikasi hasil perhitungan jasa satu penerima", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("MedicalFeeServiceFees", "Verify")]
        public async Task<IActionResult> Verify(
            Guid id,
            [FromBody] VerifyServiceFeeRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _periodService.VerifyServiceFeeAsync(id, request, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "MedicalFeeServiceFee.Verify", $"Hasil jasa {result.FeeNumber} berhasil diverifikasi.", new { id, actorUserId });
                return Ok(ApiResponse<ServiceFeeHeaderResponse>.Ok(result, $"Hasil jasa {result.FeeNumber} berhasil diverifikasi."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeServiceFee.Verify", ex.Message, new { id, actorUserId });
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }

        // =====================================================================
        // 6. POST /{id}/return (Kembalikan ke Calculated)
        // =====================================================================

        /// <summary>
        /// Mengembalikan status hasil jasa: Verified -> Calculated (MDF-STATE-1.0 §2).
        /// Wajib menyertakan alasan pengembalian. Jika periode induk berstatus Verified, status periode otomatis ikut kembali ke Calculated.
        /// </summary>
        [HttpPost("{id:guid}/return")]
        [ProducesResponseType(typeof(ApiResponse<ServiceFeeHeaderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Verify", "Return Medical Fee Service Fee", Description = "Mengembalikan status verifikasi hasil jasa ke Calculated", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("MedicalFeeServiceFees", "Verify")]
        public async Task<IActionResult> Return(
            Guid id,
            [FromBody] ReturnServiceFeeRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _periodService.ReturnServiceFeeAsync(id, request, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "MedicalFeeServiceFee.Return", $"Hasil jasa {result.FeeNumber} dikembalikan ke status Calculated. Alasan: {request.Reason}", new { id, request.Reason, actorUserId });
                return Ok(ApiResponse<ServiceFeeHeaderResponse>.Ok(result, $"Hasil jasa {result.FeeNumber} dikembalikan ke status Calculated."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeServiceFee.Return", ex.Message, new { id, actorUserId });
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }

        // =====================================================================
        // 7. POST /batch-verification (Verifikasi Massal Seluruh Jasa pada Periode)
        // =====================================================================

        /// <summary>
        /// Melakukan verifikasi massal terhadap seluruh hasil jasa medis yang masih berstatus 'Calculated' pada satu periode.
        /// </summary>
        [HttpPost("batch-verification")]
        [ProducesResponseType(typeof(ApiResponse<BatchVerifyResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Verify", "Batch Verify Medical Fee Service Fees", Description = "Memverifikasi seluruh hasil jasa pada satu periode kalkulasi secara massal", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalFeeServiceFees", "Verify")]
        public async Task<IActionResult> BatchVerify(
            [FromBody] BatchVerifyServiceFeesRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _periodService.BatchVerifyServiceFeesAsync(request.PeriodId, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "MedicalFeeServiceFee.BatchVerify", $"Batch verifikasi periode {request.PeriodId}: {result.VerifiedCount} hasil jasa diverifikasi.", new { request.PeriodId, actorUserId, result.VerifiedCount });
                return Ok(ApiResponse<BatchVerifyResultResponse>.Ok(result, $"Batch verifikasi berhasil: {result.VerifiedCount} hasil jasa diverifikasi."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeServiceFee.BatchVerify", ex.Message, new { request.PeriodId, actorUserId });
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }

        // =====================================================================
        // 8. GET /{id}/adjustments (Riwayat Koreksi Hasil Jasa)
        // =====================================================================

        /// <summary>
        /// Mengambil daftar riwayat penyesuaian koreksi untuk satu hasil jasa medis tertentu (MDF-API-1.0 §5).
        /// </summary>
        [HttpGet("{id:guid}/adjustments")]
        [ProducesResponseType(typeof(ApiResponse<List<ServiceFeeAdjustmentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("View", "View Medical Fee Service Fee Adjustments", Description = "Melihat riwayat penyesuaian hasil jasa medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeServiceFees", "View")]
        public async Task<IActionResult> GetAdjustments(Guid id, CancellationToken ct = default)
        {
            try
            {
                var actorContext = await _accessGuard.ResolveActorContextAsync(User, ct);
                var result = await _adjustmentService.GetAdjustmentsByServiceFeeIdAsync(id, actorContext, ct);
                return Ok(ApiResponse<List<ServiceFeeAdjustmentResponse>>.Ok(result, "Riwayat koreksi hasil jasa medis berhasil diambil."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        // =====================================================================
        // 9. GET /{id}/adjustments/{adjId} (Detail Satu Koreksi)
        // =====================================================================

        /// <summary>
        /// Mengambil detail satu penyesuaian koreksi jasa medis berdasarkan ID koreksi.
        /// </summary>
        [HttpGet("{id:guid}/adjustments/{adjId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ServiceFeeAdjustmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("View", "View Medical Fee Service Fee Adjustment Detail", Description = "Melihat detail satu penyesuaian hasil jasa medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeServiceFees", "View")]
        public async Task<IActionResult> GetAdjustmentById(Guid id, Guid adjId, CancellationToken ct = default)
        {
            try
            {
                var actorContext = await _accessGuard.ResolveActorContextAsync(User, ct);
                var result = await _adjustmentService.GetAdjustmentByIdAsync(id, adjId, actorContext, ct);
                return Ok(ApiResponse<ServiceFeeAdjustmentResponse>.Ok(result, "Detail koreksi hasil jasa medis berhasil diambil."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        // =====================================================================
        // 10. POST /{id}/adjustments (Ajukan Koreksi Penyesuaian)
        // =====================================================================

        /// <summary>
        /// Mengajukan penyesuaian koreksi nilai jasa medis (status: Pending) — MDF-API-1.0 §5, MDF-VAL-1.0 §6.
        /// Wajib menyertakan header Idempotency-Key untuk perlindungan eksekusi ganda (MDF-VAL-1.0 §6.12, T-70).
        /// </summary>
        [HttpPost("{id:guid}/adjustments")]
        [ProducesResponseType(typeof(ApiResponse<ServiceFeeAdjustmentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("RequestAdjustment", "Request Medical Fee Service Fee Adjustment", Description = "Mengajukan penyesuaian nominal hasil jasa medis", AccessType = AccessTypes.Create, SortOrder = 4)]
        [AccessPermission("MedicalFeeServiceFees", "RequestAdjustment")]
        public async Task<IActionResult> RequestAdjustment(
            Guid id,
            [FromBody] CreateAdjustmentRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _adjustmentService.RequestAdjustmentAsync(id, request, idempotencyKey, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "MedicalFeeServiceFee.RequestAdjustment", $"Koreksi {result.AdjustmentNumber} berhasil diajukan untuk jasa {id}.", new { id, result.AdjustmentNumber, actorUserId, request.Direction, request.Amount });
                return StatusCode(StatusCodes.Status201Created, ApiResponse<ServiceFeeAdjustmentResponse>.Ok(result, $"Koreksi jasa {result.AdjustmentNumber} berhasil diajukan."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeServiceFee.RequestAdjustment", ex.Message, new { id, actorUserId });
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }

        // =====================================================================
        // 11. POST /{id}/adjustments/{adjId}/approval (Setujui Koreksi - Maker-Checker)
        // =====================================================================

        /// <summary>
        /// Menyetujui pengajuan koreksi penyesuaian hasil jasa medis: Pending -> Approved (MDF-DES-017, MDF-VAL-1.0 §6.8).
        /// Menerapkan kontrol Maker-Checker: Penyetuju wajib berbeda dari pengaju koreksi (T-60, T-61).
        /// </summary>
        [HttpPost("{id:guid}/adjustments/{adjId:guid}/approval")]
        [ProducesResponseType(typeof(ApiResponse<ServiceFeeAdjustmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("ApproveAdjustment", "Approve Medical Fee Service Fee Adjustment", Description = "Menyetujui pengajuan penyesuaian hasil jasa medis (Maker-Checker)", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("MedicalFeeServiceFees", "ApproveAdjustment")]
        public async Task<IActionResult> ApproveAdjustment(
            Guid id,
            Guid adjId,
            [FromBody] ApproveAdjustmentRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _adjustmentService.ApproveAdjustmentAsync(id, adjId, request, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "MedicalFeeServiceFee.ApproveAdjustment", $"Koreksi {result.AdjustmentNumber} disetujui. Nilai akhir jasa {result.FeeNumber} kini Rp{result.CurrentFinalAmount:N2}.", new { id, adjId, actorUserId, result.CurrentFinalAmount });
                return Ok(ApiResponse<ServiceFeeAdjustmentResponse>.Ok(result, $"Koreksi jasa {result.AdjustmentNumber} berhasil disetujui."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeServiceFee.ApproveAdjustment", ex.Message, new { id, adjId, actorUserId });
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }

        // =====================================================================
        // 12. POST /{id}/adjustments/{adjId}/rejection (Tolak Koreksi)
        // =====================================================================

        /// <summary>
        /// Menolak pengajuan koreksi penyesuaian hasil jasa medis: Pending -> Rejected (MDF-VAL-1.0 §6.10, T-69).
        /// Wajib menyertakan alasan penolakan minimal 10 karakter.
        /// </summary>
        [HttpPost("{id:guid}/adjustments/{adjId:guid}/rejection")]
        [ProducesResponseType(typeof(ApiResponse<ServiceFeeAdjustmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("ApproveAdjustment", "Reject Medical Fee Service Fee Adjustment", Description = "Menolak pengajuan penyesuaian hasil jasa medis dengan alasan", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("MedicalFeeServiceFees", "ApproveAdjustment")]
        public async Task<IActionResult> RejectAdjustment(
            Guid id,
            Guid adjId,
            [FromBody] RejectAdjustmentRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _adjustmentService.RejectAdjustmentAsync(id, adjId, request, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "MedicalFeeServiceFee.RejectAdjustment", $"Koreksi {result.AdjustmentNumber} ditolak. Alasan: {request.RejectionReason}", new { id, adjId, actorUserId, request.RejectionReason });
                return Ok(ApiResponse<ServiceFeeAdjustmentResponse>.Ok(result, $"Koreksi jasa {result.AdjustmentNumber} berhasil ditolak."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeServiceFee.RejectAdjustment", ex.Message, new { id, adjId, actorUserId });
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
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

