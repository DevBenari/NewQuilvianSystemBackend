using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    /// <summary>
    /// Check-in Pasien Online di kiosk (kiosk Amendment PO, <c>BE-KSK-PO-006</c>,
    /// <c>KSK-CONTRACT-v2</c>): verifikasi kode atau Lupa Kode, lalu token check-in 15 menit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Sengaja tanpa <c>[AccessAction]</c> dan <c>[AccessPermission]</c></b> (<c>KSK-DSN-006</c>),
    /// sama dengan <c>KioskPatientLookupController</c>: pemanggilnya akun perangkat kiosk yang tidak
    /// punya Departemen × Posisi. Otorisasinya policy <see cref="AuthorizationPolicies.KioskRead"/>.
    /// </para>
    /// <para>
    /// <b>POST walaupun hanya mencari</b>, supaya kode, NIK/No. RM, dan tanggal lahir berada di body
    /// dan tidak tercatat di URL maupun log proxy. Batas laju per akun kiosk
    /// (<see cref="RateLimitPolicy"/>, <c>KSK-VAL-PO-044</c>) menahan penebakan kode di kiosk.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/online-registrations/kiosk")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Online Registration Check-In",
        AreaName = "HealthServices",
        ControllerName = "OnlineRegistrationCheckIn",
        Description = "Check-in pendaftaran pasien online dari Kiosk memakai kode atau NIK/No. RM dan tanggal lahir",
        SortOrder = 11
    )]
    [Tags("Health Services / Registration Management / Online Registration Check-In")]
    public class OnlineRegistrationCheckInController : ControllerBase
    {
        /// <summary>
        /// Policy batas laju per akun kiosk, didaftarkan di <c>Program.cs</c>. Batasnya dari
        /// <c>HealthServices:Registration:OnlineRegistration:CheckInPermitPerMinute</c>, bawaan 10.
        /// </summary>
        public const string RateLimitPolicy = "OnlineRegistrationCheckIn";

        public const string RateLimitedMessage = "Terlalu banyak percobaan. Silakan coba lagi sebentar.";

        private readonly OnlineRegistrationCheckInService _checkInService;

        public OnlineRegistrationCheckInController(OnlineRegistrationCheckInService checkInService)
        {
            _checkInService = checkInService;
        }

        /// <summary>Memeriksa kode atau identitas + tanggal lahir, lalu menerbitkan token check-in.</summary>
        /// <remarks>
        /// <c>404</c> dengan pesan seragam bila kode/identitas atau tanggal lahir tidak cocok, atau
        /// pendaftaran sudah tidak berlaku. <c>409</c> bila tanggal kunjungan belum hari ini; pesannya
        /// menyebut tanggal kunjungan. Paket tidak memuat kode pendaftaran.
        /// </remarks>
        [HttpPost("check-in/verify")]
        [Authorize(Policy = AuthorizationPolicies.KioskRead)]
        [EnableRateLimiting(RateLimitPolicy)]
        [ProducesResponseType(typeof(ApiResponse<OnlineRegistrationCheckInPackage>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Verify(
            [FromBody] OnlineRegistrationCheckInVerifyRequest? request,
            CancellationToken cancellationToken = default)
        {
            Response.Headers.CacheControl = "no-store";

            var result = await _checkInService.VerifyAsync(request, cancellationToken);

            return result.Outcome switch
            {
                OnlineRegistrationPublicOutcome.Ok => Ok(ApiResponse<OnlineRegistrationCheckInPackage>.Ok(result.Data!, result.Message)),
                OnlineRegistrationPublicOutcome.NotFound => NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),
                OnlineRegistrationPublicOutcome.Conflict => Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message)),
                _ => BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.Message))
            };
        }
    }
}
