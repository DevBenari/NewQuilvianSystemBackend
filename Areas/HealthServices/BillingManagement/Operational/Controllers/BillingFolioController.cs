using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/billing-management/folios")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_BILLING_MANAGEMENT",
        moduleName: "Health Service Billing Management",
        displayName: "Billing Folio",
        AreaName = "HealthServices",
        ControllerName = "BillingFolio",
        Description = "Mengelola folio dan pengenalan milestone Billing Rawat Jalan",
        SortOrder = 1)]
    [Tags("Health Services / Billing Management / Billing Folio")]
    public class BillingFolioController : ControllerBase
    {
        private const string LogCategory = "HealthServices.BillingManagement";

        private readonly BillingFolioService _billingFolioService;
        private readonly LoggerService _loggerService;

        public BillingFolioController(
            BillingFolioService billingFolioService,
            LoggerService loggerService)
        {
            _billingFolioService = billingFolioService;
            _loggerService = loggerService;
        }

        [HttpGet("by-encounter/{encounterId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BillingFolioDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Billing Folio", Description = "Melihat folio Billing berdasarkan encounter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BillingFolio", "Read")]
        public async Task<IActionResult> GetByEncounter(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            var response = await _billingFolioService.GetByEncounterAsync(encounterId, cancellationToken);
            if (response == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Folio Billing tidak ditemukan.",
                    new { Code = "BIL_FOLIO_NOT_FOUND" }));
            }

            return Ok(ApiResponse<BillingFolioDetailResponse>.Ok(
                response,
                "Folio Billing berhasil diambil."));
        }

        [HttpGet("{folioId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BillingFolioDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Billing Folio", Description = "Melihat folio Billing", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BillingFolio", "Read")]
        public async Task<IActionResult> GetById(
            Guid folioId,
            CancellationToken cancellationToken = default)
        {
            var response = await _billingFolioService.GetByIdAsync(folioId, cancellationToken);
            if (response == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Folio Billing tidak ditemukan.",
                    new { Code = "BIL_FOLIO_NOT_FOUND" }));
            }

            return Ok(ApiResponse<BillingFolioDetailResponse>.Ok(
                response,
                "Folio Billing berhasil diambil."));
        }

        [HttpPost("internal/milestones/recognize")]
        [ProducesResponseType(typeof(ApiResponse<RecognizeBillingMilestoneResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("RecognizeInternal", "Recognize Internal Billing Milestone", Description = "Memproses milestone internal yang telah diotorisasi", AccessType = AccessTypes.Create, SortOrder = 2, IsSystemOnly = true)]
        [AccessPermission("BillingMilestone", "RecognizeInternal")]
        public async Task<IActionResult> RecognizeInternalMilestone(
            [FromBody] RecognizeBillingMilestoneRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas actor tidak tersedia.",
                    new { Code = "BIL_FORBIDDEN" }));
            }

            var result = await _billingFolioService.RecognizeMilestoneAsync(
                request,
                actorUserId,
                cancellationToken: cancellationToken);

            if (result.Kind != BillingServiceResultKind.Success || result.Value == null)
            {
                if (string.Equals(result.ErrorCode, "BIL_VERSION_CONFLICT", StringComparison.Ordinal))
                {
                    await _loggerService.AuditAsync(
                        LogCategory,
                        "BillingMilestone.VersionConflict",
                        "Menolak milestone Billing dengan revisi stale atau konflik revision identity.",
                        new
                        {
                            UserId = actorUserId,
                            request.SourceContext,
                            request.MilestoneFactId,
                            IncomingVersion = request.MilestoneFactVersion,
                            result.AppliedVersion,
                            request.EffectType,
                            request.EncounterId,
                            request.CorrelationId,
                            IdempotencyKeyReference = HashReference(request.IdempotencyKey),
                            request.OccurredAt,
                            DetectedAt = DateTime.UtcNow,
                            Outcome = result.ErrorCode
                        });
                }

                return ToFailureResult(result);
            }

            await _loggerService.AuditAsync(
                LogCategory,
                "BillingMilestone.RecognizeInternal",
                "Memproses milestone internal Billing.",
                new
                {
                    UserId = actorUserId,
                    result.Value.ProcessingEffectId,
                    result.Value.FolioId,
                    result.Value.ChargeLineId,
                    result.Value.IsReplay,
                    result.Value.Outcome,
                    result.Value.CalculationStatus,
                    request.SourceContext,
                    request.MilestoneFactId,
                    request.MilestoneFactVersion,
                    request.EffectType,
                    request.EncounterId,
                    request.CorrelationId,
                    request.CausationId,
                    IdempotencyKeyReference = HashReference(request.IdempotencyKey),
                    request.OccurredAt
                });

            return Ok(ApiResponse<RecognizeBillingMilestoneResponse>.Ok(
                result.Value,
                result.Value.IsReplay
                    ? "Milestone telah diproses sebelumnya; hasil canonical dikembalikan."
                    : "Milestone Billing berhasil diproses."));
        }

        private IActionResult ToFailureResult(
            BillingServiceResult<RecognizeBillingMilestoneResponse> result)
        {
            var errors = new { Code = result.ErrorCode };
            return result.Kind switch
            {
                BillingServiceResultKind.NotFound => NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    result.ErrorMessage ?? "Sumber Billing tidak ditemukan.",
                    errors)),
                BillingServiceResultKind.Conflict => Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    result.ErrorMessage ?? "Terjadi konflik pemrosesan Billing.",
                    errors)),
                _ => BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    result.ErrorMessage ?? "Milestone Billing tidak valid.",
                    errors))
            };
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string HashReference(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
