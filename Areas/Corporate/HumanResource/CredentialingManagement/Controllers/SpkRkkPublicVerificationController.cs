using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Controllers
{
    /// <summary>
    /// Verifikasi publik SPK-RKK lewat kode QR (register masalah SDM MSL-16, 19 November 2026).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Contoh: perawat IGD rumah sakit mitra memindai QR di SPK dr. Rina untuk memastikan dokter itu memang berwenang
    /// melakukan tindakan. Pemindai tidak punya akun Quilvian, sehingga endpoint ini sengaja <c>[AllowAnonymous]</c>
    /// dan tidak memakai <c>[AccessPermission]</c>.
    /// </para>
    /// <para>
    /// Pengamannya: (1) hash SHA-256 64 karakter dari penandatanganan Direktur Utama tidak bisa ditebak; (2) batas laju
    /// 30 permintaan per menit per IP; (3) jawaban hanya berisi data yang memang tercetak di dokumen (nama, nomor
    /// SPK/RKK, jenis kewenangan, masa berlaku, nama Direktur), tanpa rincian kewenangan klinis, NIK, atau kontak;
    /// (4) hash tidak dicatat di log.
    /// </para>
    /// </remarks>
    [ApiController]
    [Route("api/v1/public/clinical-privileges")]
    [Tags("HR Credentialing - Public SPK-RKK Verification")]
    [AllowAnonymous]
    public class SpkRkkPublicVerificationController : ControllerBase
    {
        public const string RateLimitPolicy = "hr-spk-rkk-public-verify";

        private readonly ICredentialingWorklistQueryService _worklist;

        public SpkRkkPublicVerificationController(ICredentialingWorklistQueryService worklist)
        {
            _worklist = worklist;
        }

        /// <summary>
        /// Memeriksa keabsahan SPK-RKK berdasarkan hash pada kode QR. 200 berisi status (Valid, Expired, Suspended,
        /// Revoked, NotSigned, NotYetValid); 404 bila hash tidak dikenal.
        /// </summary>
        [HttpGet("verify/{hash}")]
        [EnableRateLimiting(RateLimitPolicy)]
        [ProducesResponseType(typeof(ApiResponse<SpkRkkPublicVerificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Verify([FromRoute] string hash, CancellationToken cancellationToken = default)
        {
            var result = await _worklist.VerifyPublicAsync(hash, cancellationToken);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen tidak dikenal. Pastikan kode QR dipindai dari SPK-RKK asli terbitan rumah sakit."));
            }

            return Ok(ApiResponse<SpkRkkPublicVerificationDto>.Ok(result, result.StatusLabel));
        }
    }
}
