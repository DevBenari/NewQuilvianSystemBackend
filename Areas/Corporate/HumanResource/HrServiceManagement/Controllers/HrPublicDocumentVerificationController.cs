using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Controllers
{
    [ApiController]
    [Tags("HR Services - Public Verification & Rate Limiting")]
    [AllowAnonymous]
    public class HrPublicDocumentVerificationController : ControllerBase
    {
        private readonly IEmployeeDocumentIssuanceService _issuanceService;
        private readonly ILogger<HrPublicDocumentVerificationController> _logger;

        public HrPublicDocumentVerificationController(
            IEmployeeDocumentIssuanceService issuanceService,
            ILogger<HrPublicDocumentVerificationController> logger)
        {
            _issuanceService = issuanceService;
            _logger = logger;
        }

        private string GetClientIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        /// <summary>
        /// Portal Verifikasi Publik Naskah Dinas Resmi Rumah Sakit Quilvian (HRS-B5, BE-HRD-204)
        /// Dipindai langsung dari QR Code dokumen fisik/digital tanpa login.
        /// Nominal finansial di-masking secara default demi privasi nakes.
        /// </summary>
        [HttpGet("verify/document/{hash}")]
        [HttpGet("public-verify/hr-document")]
        [HttpGet("api/v1/public/hr-documents/verify/{hash}")]
        public async Task<IActionResult> VerifyDocument(
            [FromRoute] string? hash = null,
            [FromQuery] string? queryHash = null,
            CancellationToken cancellationToken = default)
        {
            var targetHash = !string.IsNullOrWhiteSpace(hash) ? hash : queryHash;
            if (string.IsNullOrWhiteSpace(targetHash))
            {
                return BadRequest(new { message = "Parameter hash verifikasi tidak valid atau tidak disertakan." });
            }

            var clientIp = GetClientIp();
            var userAgent = Request.Headers.UserAgent.ToString();

            _logger.LogInformation("[PUBLIC-VERIFY-ACCESS] Akses verifikasi dokumen hash {Hash} dari IP {Ip}, UA: {UserAgent}",
                targetHash, clientIp, userAgent);

            var verification = await _issuanceService.VerifyPublicDocumentAsync(targetHash, cancellationToken);
            return Ok(verification);
        }

        /// <summary>
        /// Membuka Kunci Rincian Finansial dengan 4-Digit PIN Resmi (VR-HRS-09, VR-HRS-12, BE-HRD-204)
        /// Dilindungi pembatas laju anti brute-force: gagal 3 kali berturut-turut memicu penguncian akses selama 30 menit.
        /// </summary>
        [HttpPost("verify/document/{hash}/unlock-pin")]
        [HttpPost("api/v1/public/hr-documents/unlock-pin")]
        public async Task<IActionResult> UnlockPin(
            [FromRoute] string? hash,
            [FromBody] UnlockFinancialDetailsRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(hash))
            {
                request.VerificationHash = hash;
            }

            var clientIp = GetClientIp();
            try
            {
                var result = await _issuanceService.UnlockFinancialDetailsAsync(request, clientIp, cancellationToken);
                _logger.LogInformation("[PIN-UNLOCK-SUCCESS] Akses data finansial dokumen {DocNum} berhasil dibuka dari IP {Ip}",
                    result.OfficialDocumentNumber, clientIp);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("[PIN-UNLOCK-FAILED] Percobaan pembukaan PIN untuk hash {Hash} dari IP {Ip} gagal: {Msg}",
                    request.VerificationHash, clientIp, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
