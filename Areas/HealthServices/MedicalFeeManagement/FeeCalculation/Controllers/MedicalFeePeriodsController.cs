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
    /// Controller pengelola siklus periode perhitungan jasa medis (MdfFeePeriod).
    /// Mengimplementasikan kontrak MDF-API-1.0 §4 dan matriks izin MDF-PERM-1.0 §2.
    /// Mengatur alur: Buka Periode -> Hitung -> Buka Kembali / Verifikasi -> Kembalikan / Setujui -> Tutup.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-fee-management/fee-periods")]
    [AccessController(
        moduleCode: "MEDICAL_FEE_MANAGEMENT",
        moduleName: "Medical Fee Management",
        displayName: "Medical Fee Period",
        AreaName = "HealthServices",
        ControllerName = "MedicalFeePeriods",
        Description = "Mengelola periode kalkulasi, verifikasi, approval, dan penutupan jasa medis",
        SortOrder = 3
    )]
    [Tags("Health Services / Medical Fee Management / Fee Periods")]
    public class MedicalFeePeriodsController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalFeeManagement.FeeCalculation.Period";

        private readonly MedicalFeePeriodService _periodService;
        private readonly MedicalFeeCalculationService _calculationService;
        private readonly LoggerService _loggerService;

        public MedicalFeePeriodsController(
            MedicalFeePeriodService periodService,
            MedicalFeeCalculationService calculationService,
            LoggerService loggerService)
        {
            _periodService = periodService;
            _calculationService = calculationService;
            _loggerService = loggerService;
        }

        // =====================================================================
        // 1. GET /filters/metadata
        // =====================================================================

        /// <summary>
        /// Mengambil konfigurasi metadata filter, sort, dan opsi status untuk periode kalkulasi.
        /// </summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<FeePeriodFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Period Filter Metadata", Description = "Melihat metadata filter periode kalkulasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeePeriods", AccessTypes.Read)]
        public IActionResult GetFilterMetadata()
        {
            var metadata = _periodService.GetFilterMetadata();
            return Ok(ApiResponse<FeePeriodFilterMetadataResponse>.Ok(metadata, "Metadata filter periode kalkulasi berhasil diambil."));
        }

        // =====================================================================
        // 2. GET /summary
        // =====================================================================

        /// <summary>
        /// Mengambil ringkasan statistik periode perhitungan (total, open, calculated, verified, approved, closed).
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<FeePeriodSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Period Summary", Description = "Melihat ringkasan statistik periode kalkulasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeePeriods", AccessTypes.Read)]
        public async Task<IActionResult> GetSummary(CancellationToken ct = default)
        {
            var summary = await _periodService.GetSummaryAsync(ct);
            return Ok(ApiResponse<FeePeriodSummaryResponse>.Ok(summary, "Ringkasan statistik periode perhitungan berhasil diambil."));
        }

        // =====================================================================
        // 3. GET / (List Paged Table)
        // =====================================================================

        /// <summary>
        /// Mengambil daftar periode perhitungan secara terpaginasi dengan filter status, tahun, dan pencarian kode.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<FeePeriodHeaderResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Period List", Description = "Melihat daftar periode perhitungan jasa medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeePeriods", AccessTypes.Read)]
        public async Task<IActionResult> GetPaged([FromQuery] FeePeriodPagedQuery query, CancellationToken ct = default)
        {
            var result = await _periodService.GetPagedAsync(query, ct);
            return Ok(ApiResponse<PagedResult<FeePeriodHeaderResponse>>.Ok(result, "Daftar periode perhitungan jasa medis berhasil diambil."));
        }

        // =====================================================================
        // 4. GET /{id} (Detail)
        // =====================================================================

        /// <summary>
        /// Mengambil detail satu periode perhitungan beserta rekap nominal kotor, koreksi, final, dan layanan belum terhitung.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FeePeriodDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Medical Fee Period Detail", Description = "Melihat detail satu periode perhitungan jasa medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeePeriods", AccessTypes.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        {
            try
            {
                var result = await _periodService.GetByIdAsync(id, ct);
                return Ok(ApiResponse<FeePeriodDetailResponse>.Ok(result, "Detail periode perhitungan berhasil diambil."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        // =====================================================================
        // 5. POST / (Create / Buka Periode Baru)
        // =====================================================================

        /// <summary>
        /// Membuka periode perhitungan jasa medis baru dengan status Open.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<FeePeriodHeaderResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Medical Fee Period", Description = "Membuka periode perhitungan jasa medis baru", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MedicalFeePeriods", AccessTypes.Create)]
        public async Task<IActionResult> Create([FromBody] CreateFeePeriodRequest request, CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _periodService.CreatePeriodAsync(request, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "MedicalFeePeriod.Create", $"Periode {result.PeriodCode} berhasil dibuka.", new { result.Id, result.PeriodCode, actorUserId });
                return StatusCode(StatusCodes.Status201Created, ApiResponse<FeePeriodHeaderResponse>.Ok(result, $"Periode {result.PeriodCode} berhasil dibuka."));
            }
            catch (InvalidOperationException ex)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeePeriod.Create", ex.Message, new { request, actorUserId });
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }

        // =====================================================================
        // 6. POST /{id}/calculation (Jalankan Perhitungan Jasa Medis)
        // =====================================================================

        /// <summary>
        /// Menjalankan kalkulasi jasa medis untuk satu periode tertentu.
        /// Mendukung perhitungan awal dari Open atau kalkulasi ulang dari Calculated (MDF-DES-015).
        /// Wajib membawa header Idempotency-Key untuk perlindungan eksekusi ganda.
        /// </summary>
        [HttpPost("{id:guid}/calculation")]
        [ProducesResponseType(typeof(ApiResponse<CalculationResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Calculate", "Calculate Medical Fee Period", Description = "Menjalankan kalkulasi jasa medis untuk satu periode", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalFeePeriods", AccessTypes.Update)]
        public async Task<IActionResult> Calculate(
            Guid id,
            [FromBody] CalculatePeriodRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _calculationService.CalculatePeriodAsync(id, request, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "MedicalFeePeriod.Calculate", $"Perhitungan periode {result.PeriodCode} selesai.", new { result.PeriodId, result.GrossTotal, result.PayeeCount, idempotencyKey, actorUserId });
                return Ok(ApiResponse<CalculationResultResponse>.Ok(result, $"Perhitungan periode {result.PeriodCode} selesai."));
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
                await _loggerService.WarningAsync(LogCategory, "MedicalFeePeriod.Calculate", ex.Message, new { id, actorUserId });
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }

        // =====================================================================
        // 7. POST /{id}/reopen (Buka Kembali dari Calculated ke Open)
        // =====================================================================

        /// <summary>
        /// Membuka kembali periode yang berstatus Calculated ke Open (MDF-DES-015, MDF-STATE-1.0 §1).
        /// Menghapus rincian dan daftar unresolved yang masih berstatus Open.
        /// </summary>
        [HttpPost("{id:guid}/reopen")]
        [ProducesResponseType(typeof(ApiResponse<FeePeriodDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Reopen", "Reopen Medical Fee Period", Description = "Membuka kembali periode kalkulasi yang berstatus Calculated ke Open", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalFeePeriods", AccessTypes.Update)]
        public async Task<IActionResult> Reopen(
            Guid id,
            [FromBody] ReopenFeePeriodRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _periodService.ReopenPeriodAsync(id, request, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "MedicalFeePeriod.Reopen", $"Periode {result.PeriodCode} berhasil dibuka kembali ke status Open.", new { id, actorUserId });
                return Ok(ApiResponse<FeePeriodDetailResponse>.Ok(result, $"Periode {result.PeriodCode} berhasil dibuka kembali ke status Open."));
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
                await _loggerService.WarningAsync(LogCategory, "MedicalFeePeriod.Reopen", ex.Message, new { id, actorUserId });
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }

        // =====================================================================
        // 8. POST /{id}/verification (Verifikasi Periode)
        // =====================================================================

        /// <summary>
        /// Memverifikasi periode kalkulasi: Calculated -> Verified (MDF-STATE-1.0 §1).
        /// </summary>
        [HttpPost("{id:guid}/verification")]
        [ProducesResponseType(typeof(ApiResponse<FeePeriodDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Verify", "Verify Medical Fee Period", Description = "Memverifikasi hasil perhitungan periode jasa medis", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("MedicalFeePeriods", AccessTypes.Update)]
        public async Task<IActionResult> Verify(
            Guid id,
            [FromBody] VerifyFeePeriodRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _periodService.VerifyPeriodAsync(id, request, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "MedicalFeePeriod.Verify", $"Periode {result.PeriodCode} berhasil diverifikasi.", new { id, actorUserId });
                return Ok(ApiResponse<FeePeriodDetailResponse>.Ok(result, $"Periode {result.PeriodCode} berhasil diverifikasi."));
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
                await _loggerService.WarningAsync(LogCategory, "MedicalFeePeriod.Verify", ex.Message, new { id, actorUserId });
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }

        // =====================================================================
        // 9. POST /{id}/return (Kembalikan dari Verified ke Calculated)
        // =====================================================================

        /// <summary>
        /// Mengembalikan status periode: Verified -> Calculated (MDF-STATE-1.0 §1).
        /// </summary>
        [HttpPost("{id:guid}/return")]
        [ProducesResponseType(typeof(ApiResponse<FeePeriodDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Return", "Return Medical Fee Period", Description = "Mengembalikan status verifikasi periode ke Calculated", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("MedicalFeePeriods", AccessTypes.Update)]
        public async Task<IActionResult> Return(
            Guid id,
            [FromBody] ReturnFeePeriodRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _periodService.ReturnPeriodAsync(id, request, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "MedicalFeePeriod.Return", $"Periode {result.PeriodCode} dikembalikan ke status Calculated. Alasan: {request.Reason}", new { id, request.Reason, actorUserId });
                return Ok(ApiResponse<FeePeriodDetailResponse>.Ok(result, $"Periode {result.PeriodCode} dikembalikan ke status Calculated."));
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
                await _loggerService.WarningAsync(LogCategory, "MedicalFeePeriod.Return", ex.Message, new { id, actorUserId });
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }

        // =====================================================================
        // 10. POST /{id}/approval (Setujui Periode - Maker-Checker)
        // =====================================================================

        /// <summary>
        /// Menyetujui periode kalkulasi: Verified -> Approved (MDF-DES-017, MDF-STATE-1.0 §1).
        /// Penyetuju wajib berbeda dari pemverifikasi (Maker-Checker).
        /// </summary>
        [HttpPost("{id:guid}/approval")]
        [ProducesResponseType(typeof(ApiResponse<FeePeriodDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Approve", "Approve Medical Fee Period", Description = "Menyetujui periode kalkulasi jasa medis", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("MedicalFeePeriods", AccessTypes.Update)]
        public async Task<IActionResult> Approve(
            Guid id,
            [FromBody] ApproveFeePeriodRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _periodService.ApprovePeriodAsync(id, request, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "MedicalFeePeriod.Approve", $"Periode {result.PeriodCode} berhasil disetujui.", new { id, actorUserId });
                return Ok(ApiResponse<FeePeriodDetailResponse>.Ok(result, $"Periode {result.PeriodCode} berhasil disetujui."));
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
                await _loggerService.WarningAsync(LogCategory, "MedicalFeePeriod.Approve", ex.Message, new { id, actorUserId });
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
        }

        // =====================================================================
        // 11. POST /{id}/closure (Tutup Periode Permanen)
        // =====================================================================

        /// <summary>
        /// Menutup periode kalkulasi secara permanen: Approved -> Closed (MDF-DES-016, MDF-STATE-1.0 §1).
        /// Ditolak dengan 422 bila masih ada layanan unresolved berstatus Open (MDF_PERIOD_HAS_UNRESOLVED).
        /// </summary>
        [HttpPost("{id:guid}/closure")]
        [ProducesResponseType(typeof(ApiResponse<FeePeriodDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Close", "Close Medical Fee Period", Description = "Menutup periode jasa medis secara permanen setelah seluruh unresolved diselesaikan", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("MedicalFeePeriods", AccessTypes.Update)]
        public async Task<IActionResult> Close(
            Guid id,
            [FromBody] CloseFeePeriodRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _periodService.ClosePeriodAsync(id, request, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "MedicalFeePeriod.Close", $"Periode {result.PeriodCode} berhasil ditutup permanen.", new { id, actorUserId });
                return Ok(ApiResponse<FeePeriodDetailResponse>.Ok(result, $"Periode {result.PeriodCode} berhasil ditutup secara permanen."));
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
                await _loggerService.WarningAsync(LogCategory, "MedicalFeePeriod.Close", ex.Message, new { id, actorUserId });
                var errorCode = ex.Message.Contains("MDF_PERIOD_HAS_UNRESOLVED") ? "MDF_PERIOD_HAS_UNRESOLVED" : null;
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

