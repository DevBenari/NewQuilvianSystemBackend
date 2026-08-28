using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Controllers
{
    /// <summary>
    /// Permukaan HTTP rekonsiliasi dan pemulihan Billing — <c>RJ-BIL-BE-007</c>.
    ///
    /// Kewenangan sengaja dipecah. Membaca case, memindai, menugaskan, dan menyelesaikan adalah
    /// empat kemampuan berbeda: petugas yang berhak melihat daftar masalah belum tentu berhak
    /// menyatakan sebuah masalah selesai. Menyatukan keempatnya menjadi satu permission akan
    /// membuat hak lihat diam-diam berubah menjadi hak menutup.
    ///
    /// Tidak ada satu pun endpoint di sini yang memindahkan uang.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/billing-management/reconciliation")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_BILLING_MANAGEMENT",
        moduleName: "Health Service Billing Management",
        displayName: "Billing Reconciliation",
        AreaName = "HealthServices",
        ControllerName = "BillingReconciliation",
        Description = "Rekonsiliasi, pemulihan, dan gerbang penutupan folio Billing",
        SortOrder = 2)]
    [Tags("Health Services / Billing Management / Billing Reconciliation")]
    public class BillingReconciliationController : ControllerBase
    {
        private const string LogCategory = "HealthServices.BillingManagement";

        private readonly BillingReconciliationService _reconciliationService;
        private readonly LoggerService _loggerService;

        public BillingReconciliationController(
            BillingReconciliationService reconciliationService,
            LoggerService loggerService)
        {
            _reconciliationService = reconciliationService;
            _loggerService = loggerService;
        }

        [HttpGet("cases")]
        [ProducesResponseType(typeof(ApiResponse<List<BillingReconciliationCaseResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Reconciliation Case", Description = "Melihat daftar reconciliation case", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BillingReconciliation", "Read")]
        public async Task<IActionResult> GetCases(
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? folioId,
            [FromQuery] BillingReconciliationCaseStatus? status,
            CancellationToken cancellationToken = default)
        {
            var cases = await _reconciliationService.GetCasesAsync(
                encounterId, folioId, status, cancellationToken);

            return Ok(ApiResponse<List<BillingReconciliationCaseResponse>>.Ok(
                cases,
                "Daftar reconciliation case berhasil diambil."));
        }

        [HttpGet("cases/{caseId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BillingReconciliationCaseResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Reconciliation Case", Description = "Melihat satu reconciliation case", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("BillingReconciliation", "Read")]
        public async Task<IActionResult> GetCaseById(
            Guid caseId,
            CancellationToken cancellationToken = default)
        {
            var reconciliationCase = await _reconciliationService.GetCaseByIdAsync(caseId, cancellationToken);

            if (reconciliationCase == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Reconciliation case tidak ditemukan.",
                    new { Code = "BIL_RECON_CASE_NOT_FOUND" }));
            }

            return Ok(ApiResponse<BillingReconciliationCaseResponse>.Ok(
                reconciliationCase,
                "Reconciliation case berhasil diambil."));
        }

        /// <summary>
        /// Menjalankan pemindaian rekonsiliasi. Aman diulang: masalah yang sama tidak melahirkan
        /// case kedua.
        /// </summary>
        [HttpPost("scan")]
        [ProducesResponseType(typeof(ApiResponse<ReconciliationScanResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [AccessAction("Scan", "Run Reconciliation Scan", Description = "Menjalankan pemindaian rekonsiliasi", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("BillingReconciliation", "Scan")]
        public async Task<IActionResult> Scan(
            [FromQuery] Guid? encounterId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return UnauthorizedActor();

            var response = await _reconciliationService.ScanAsync(
                encounterId, actorUserId, cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "BillingReconciliation.Scan",
                "Menjalankan pemindaian rekonsiliasi Billing.",
                new
                {
                    UserId = actorUserId,
                    EncounterId = encounterId,
                    response.EffectsExamined,
                    response.CasesOpened,
                    response.CasesReused,
                    response.SlaBreachesMarked
                });

            return Ok(ApiResponse<ReconciliationScanResponse>.Ok(
                response,
                "Pemindaian rekonsiliasi selesai."));
        }

        [HttpPost("cases/{caseId:guid}/assign")]
        [ProducesResponseType(typeof(ApiResponse<BillingReconciliationCaseResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Assign", "Assign Reconciliation Case", Description = "Menugaskan pemilik reconciliation case", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("BillingReconciliation", "Assign")]
        public async Task<IActionResult> Assign(
            Guid caseId,
            [FromBody] AssignReconciliationCaseRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return UnauthorizedActor();

            var result = await _reconciliationService.AssignAsync(
                caseId, request, actorUserId, cancellationToken);

            return MapResult(result, "Reconciliation case berhasil ditugaskan.");
        }

        /// <summary>
        /// Menyatakan sebuah case selesai. Kewenangannya dibedakan dari penugasan, karena
        /// menutup masalah adalah pernyataan yang jauh lebih berat daripada memilih siapa yang
        /// menanganinya.
        /// </summary>
        [HttpPost("cases/{caseId:guid}/resolve")]
        [ProducesResponseType(typeof(ApiResponse<BillingReconciliationCaseResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Resolve", "Resolve Reconciliation Case", Description = "Menyatakan reconciliation case selesai", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("BillingReconciliation", "Resolve")]
        public async Task<IActionResult> Resolve(
            Guid caseId,
            [FromBody] ResolveReconciliationCaseRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return UnauthorizedActor();

            var result = await _reconciliationService.ResolveAsync(
                caseId, request, actorUserId, cancellationToken);

            if (result.Kind == BillingServiceResultKind.Success)
            {
                await _loggerService.AuditAsync(
                    LogCategory,
                    "BillingReconciliation.Resolve",
                    "Menyatakan reconciliation case selesai.",
                    new
                    {
                        UserId = actorUserId,
                        CaseId = caseId,
                        request.ResolutionType
                    });
            }

            return MapResult(result, "Reconciliation case berhasil diselesaikan.");
        }

        [HttpGet("folios/{folioId:guid}/closure-readiness")]
        [ProducesResponseType(typeof(ApiResponse<FolioClosureReadinessResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Folio Closure Readiness", Description = "Memeriksa kesiapan penutupan folio", AccessType = AccessTypes.Read, SortOrder = 6)]
        [AccessPermission("BillingReconciliation", "Read")]
        public async Task<IActionResult> GetClosureReadiness(
            Guid folioId,
            CancellationToken cancellationToken = default)
        {
            var result = await _reconciliationService.EvaluateClosureReadinessAsync(folioId, cancellationToken);

            return MapResult(result, "Kesiapan penutupan folio berhasil dievaluasi.");
        }

        [HttpGet("recovery-report")]
        [ProducesResponseType(typeof(ApiResponse<BillingRecoveryReportResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Recovery Report", Description = "Melihat laporan pemulihan Billing", AccessType = AccessTypes.Read, SortOrder = 7)]
        [AccessPermission("BillingReconciliation", "Read")]
        public async Task<IActionResult> GetRecoveryReport(
            [FromQuery] Guid? encounterId,
            CancellationToken cancellationToken = default)
        {
            var report = await _reconciliationService.GetRecoveryReportAsync(encounterId, cancellationToken);

            return Ok(ApiResponse<BillingRecoveryReportResponse>.Ok(
                report,
                "Laporan pemulihan berhasil disusun."));
        }

        /// <summary>
        /// Pencarian status pemrosesan kanonik. Inilah yang dipakai modul klinis ketika jawaban
        /// atas pengirimannya hilang, menggantikan kebiasaan mengirim ulang secara buta.
        /// </summary>
        [HttpGet("processing-status")]
        [ProducesResponseType(typeof(ApiResponse<BillingProcessingStatusResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Processing Status", Description = "Mencari status pemrosesan kanonik berdasarkan identitas sumber", AccessType = AccessTypes.Read, SortOrder = 8)]
        [AccessPermission("BillingReconciliation", "Read")]
        public async Task<IActionResult> GetProcessingStatus(
            [FromQuery] string sourceContext,
            [FromQuery] Guid milestoneFactId,
            [FromQuery] int milestoneFactVersion,
            [FromQuery] string effectType,
            CancellationToken cancellationToken = default)
        {
            var response = await _reconciliationService.GetProcessingStatusAsync(
                sourceContext, milestoneFactId, milestoneFactVersion, effectType, cancellationToken);

            return Ok(ApiResponse<BillingProcessingStatusResponse>.Ok(
                response,
                "Status pemrosesan berhasil diambil."));
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private IActionResult MapResult<T>(BillingServiceResult<T> result, string successMessage)
        {
            if (result.Kind == BillingServiceResultKind.Success && result.Value != null)
            {
                return Ok(ApiResponse<T>.Ok(result.Value, successMessage));
            }

            return result.Kind switch
            {
                BillingServiceResultKind.NotFound => NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    result.ErrorMessage ?? "Data tidak ditemukan.",
                    new { Code = result.ErrorCode })),

                BillingServiceResultKind.Conflict => Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    result.ErrorMessage ?? "Terjadi konflik.",
                    new { Code = result.ErrorCode })),

                _ => BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    result.ErrorMessage ?? "Permintaan tidak valid.",
                    new { Code = result.ErrorCode }))
            };
        }

        private IActionResult UnauthorizedActor() =>
            Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Identitas actor tidak tersedia.",
                new { Code = "BIL_FORBIDDEN" }));

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
