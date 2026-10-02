using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers
{
    /// <summary>
    /// Pengawasan status penagihan kasir pada bangsal rawat inap.
    /// Memisahkan secara ketat pandangan operasional non-finansial dari rincian nominal uang (VAL-INT-006, RWI-DEC-160).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/inpatient-management/episodes")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_INPATIENT",
        moduleName: "Health Service Inpatient",
        displayName: "Inpatient Billing Operational",
        AreaName = "HealthServices",
        ControllerName = "InpatientBillingOperational",
        Description = "Pemantauan status penagihan kasir dan rincian biaya rawat inap",
        SortOrder = 18
    )]
    [Tags("Inpatient Billing Operational")]
    public class InpatientBillingOperationalController : ControllerBase
    {
        private const string LogCategory = "HealthServices.InPatientManagement.BillingOperational";

        private readonly IInpatientBillingQueryService _queryService;
        private readonly LoggerService _loggerService;

        public InpatientBillingOperationalController(
            IInpatientBillingQueryService queryService,
            LoggerService loggerService)
        {
            _queryService = queryService;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Mengambil ringkasan status operasional kasir untuk perawat bangsal (steril dari nominal rupiah).
        /// </summary>
        [HttpGet("{episodeId:guid}/billing-status")]
        [ProducesResponseType(typeof(ApiResponse<InpatientBillingStatusResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Inpatient Billing Status", Description = "Melihat ringkasan status kasir tanpa rupiah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientBillingOperational", "Read")]
        public async Task<IActionResult> GetBillingStatus(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var result = await _queryService.GetOperationalBillingStatusAsync(episodeId, cancellationToken);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Episode rawat inap tidak ditemukan."));
            }

            return Ok(ApiResponse<InpatientBillingStatusResponseDto>.Ok(
                result,
                "Status operasional kasir berhasil diambil."));
        }

        /// <summary>
        /// Mengambil rincian akumulasi biaya finansial lengkap beserta nominal rupiah (khusus staf berizin InpatientBilling:View).
        /// </summary>
        [HttpGet("{episodeId:guid}/billing-details")]
        [ProducesResponseType(typeof(ApiResponse<InpatientBillingDetailsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("ViewBillingDetails", "View Inpatient Billing Details", Description = "Melihat rincian finansial dan nominal rupiah tagihan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("InpatientBillingOperational", "ViewBillingDetails")]
        public async Task<IActionResult> GetBillingDetails(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            // VAL-INT-006: Pengguna mencoba mengakses endpoint rincian nominal rupiah tanpa permission InpatientBilling:View
            var hasBillingView = User.Claims.Any(c => (c.Type == "permission" || c.Type == ClaimTypes.Role) && c.Value == "InpatientBilling:View")
                || User.IsInRole("SuperAdmin")
                || User.IsInRole("Billing")
                || User.IsInRole("Kasir");

            if (!hasBillingView)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "InpatientBillingOperational.GetBillingDetails.AccessDenied",
                    "Akses rincian nominal rupiah ditolak karena tidak memiliki izin InpatientBilling:View.",
                    new { EpisodeId = episodeId, UserId = User.FindFirstValue("user_id") });

                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status403Forbidden,
                        "Akses ditolak: Anda tidak memiliki hak akses untuk melihat rincian finansial dan nominal rupiah tagihan rawat inap."));
            }

            var result = await _queryService.GetFinancialDetailsAsync(episodeId, cancellationToken);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Episode rawat inap tidak ditemukan."));
            }

            return Ok(ApiResponse<InpatientBillingDetailsResponseDto>.Ok(
                result,
                "Rincian finansial kasir berhasil diambil."));
        }
    }
}
