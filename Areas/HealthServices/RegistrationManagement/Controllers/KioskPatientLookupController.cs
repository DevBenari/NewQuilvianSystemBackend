using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    /// <summary>
    /// Cek Nomor Rekam Medis dari Kiosk (<c>BE-KSK-001</c>, <c>KSK-CONTRACT-v1</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>POST walaupun hanya membaca</b>, supaya No. KTP/HP berada di body dan tidak pernah
    /// tercatat di URL, riwayat browser, maupun log proxy (PRIV-3, <c>KSK-DSN-001</c>).
    /// </para>
    /// <para>
    /// <b>Sengaja tanpa <c>[AccessAction]</c> dan <c>[AccessPermission]</c></b>
    /// (<c>KSK-DSN-006</c>). Pemanggilnya akun perangkat Kiosk yang tidak punya Departemen ×
    /// Posisi, sehingga matriks Akses Role tidak punya baris untuk diperiksa. Policy
    /// <see cref="AuthorizationPolicies.KioskRead"/> terdaftar sebagai otorisasi alternatif yang
    /// disetujui. Memasang <c>[AccessAction]</c> tanpa <c>[AccessPermission]</c> akan menambah
    /// himpunan fallback kompatibilitas yang dijaga <c>tools/authorization-verifier</c>.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/kiosk-patient-lookups")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Kiosk Patient Lookup",
        AreaName = "HealthServices",
        ControllerName = "KioskPatientLookup",
        Description = "Cek nomor rekam medis pasien dari Kiosk memakai No. KTP atau No. HP",
        SortOrder = 1
    )]
    [Tags("Health Services / Registration Management / Kiosk Patient Lookup")]
    public class KioskPatientLookupController : ControllerBase
    {
        /// <summary>
        /// Nama policy rate limit yang didaftarkan di <c>Program.cs</c> (<c>BE-KSK-002</c>).
        /// Batasnya dari konfigurasi <c>KioskPatientLookup:PermitPerMinute</c>, bawaan 10.
        /// </summary>
        public const string RateLimitPolicy = "KioskPatientLookup";

        private readonly KioskPatientLookupService _service;

        public KioskPatientLookupController(KioskPatientLookupService service)
        {
            _service = service;
        }

        /// <summary>
        /// Mencari tepat satu pasien dari No. KTP, No. HP, nomor kartu asuransi, atau nomor member.
        /// </summary>
        /// <remarks>
        /// Ditemukan, tidak ditemukan, cocok ganda, dan hubungi petugas seluruhnya dijawab
        /// <c>200</c> dengan field <c>result</c> (<c>KSK-DSN-003</c>). <c>400</c> hanya untuk
        /// isian yang tidak sah.
        /// </remarks>
        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.KioskRead)]
        [EnableRateLimiting(RateLimitPolicy)]
        [ProducesResponseType(typeof(ApiResponse<KioskPatientLookupResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Lookup(
            [FromBody] KioskPatientLookupRequest request,
            CancellationToken cancellationToken = default)
        {
            var deviceUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _service.LookupAsync(request, deviceUserId, cancellationToken);

            if (!result.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.ValidationError!));
            }

            var data = result.Data!;
            var message = data.Result switch
            {
                KioskPatientLookupResult.Found => "Pasien ditemukan.",
                KioskPatientLookupResult.NotFound => "Pasien belum terdaftar.",
                _ => "Data perlu diverifikasi."
            };

            return Ok(ApiResponse<KioskPatientLookupResponse>.Ok(data, message));
        }
    }
}
