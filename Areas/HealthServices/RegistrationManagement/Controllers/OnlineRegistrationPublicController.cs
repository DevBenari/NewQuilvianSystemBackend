using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    /// <summary>
    /// Halaman publik Pendaftaran Pasien Online, tanpa login (kiosk Amendment PO, <c>KSK-CONTRACT-v2</c>):
    /// opsi dan pencocokan pasien lama (<c>BE-KSK-PO-004</c>), serta pengiriman pendaftaran
    /// (<c>BE-KSK-PO-005</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sengaja tanpa <c>[AccessController]</c> agar tidak muncul di layar Akses Role, sama dengan
    /// <c>PriorityReservationArrivalController</c>. Dijaga batas laju per IP
    /// (<see cref="OptionsRateLimitPolicy"/>, <see cref="MatchRateLimitPolicy"/>,
    /// <see cref="SubmitRateLimitPolicy"/>) dan captcha Turnstile pada pencocokan pasien dan pengiriman
    /// (<c>KSK-DEC-038</c>, <c>044</c>).
    /// </para>
    /// <para>
    /// Data pasien hanya dikirim lewat body <c>POST</c>, tidak pernah lewat path atau query, sehingga
    /// path permintaan aman masuk log. Seluruh respons <c>Cache-Control: no-store</c>.
    /// </para>
    /// </remarks>
    [ApiController]
    [AllowAnonymous]
    [Route(RoutePrefix)]
    [Tags("Health Services / Registration Management / Online Registration Public")]
    public class OnlineRegistrationPublicController : ControllerBase
    {
        public const string RoutePrefix = "api/v1/health-services/registration-management/online-registrations/public";
        public const string OptionsRateLimitPolicy = "OnlineRegistrationPublicOptions";
        public const string MatchRateLimitPolicy = "OnlineRegistrationPublicMatch";
        public const string SubmitRateLimitPolicy = "OnlineRegistrationPublicSubmit";
        public const string RateLimitedMessage = "Terlalu banyak percobaan. Silakan coba lagi nanti.";

        /// <summary>
        /// Batas badan permintaan kirim: tiga berkas 5 MB, tanda tangan, dan JSON isian, dengan sisa
        /// untuk pembungkus multipart. Berkas yang lebih besar dari 5 MB tetap ditolak per berkas
        /// dengan pesan <c>KSK-VAL-PO-029</c> selama totalnya masih di bawah batas ini.
        /// </summary>
        private const long SubmitRequestLimitBytes = 20L * 1024 * 1024;

        private readonly OnlineRegistrationPublicService _publicService;
        private readonly JsonSerializerOptions _jsonOptions;

        public OnlineRegistrationPublicController(
            OnlineRegistrationPublicService publicService,
            IOptions<JsonOptions> jsonOptions)
        {
            _publicService = publicService;
            _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
        }

        /// <summary>Poli yang punya jadwal online pada tanggal kunjungan (H+1 s.d. H+30).</summary>
        [HttpGet("options/clinics")]
        [EnableRateLimiting(OptionsRateLimitPolicy)]
        [ProducesResponseType(typeof(ApiResponse<List<OnlineRegistrationClinicOption>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GetClinicOptions(
            [FromQuery] DateTime? visitDate,
            CancellationToken cancellationToken = default)
        {
            MarkNoStore();
            return Respond(await _publicService.GetClinicOptionsAsync(visitDate, cancellationToken));
        }

        /// <summary>Jadwal dokter poli terpilih beserta sisa kuota appointment.</summary>
        [HttpGet("options/doctor-schedules")]
        [EnableRateLimiting(OptionsRateLimitPolicy)]
        [ProducesResponseType(typeof(ApiResponse<List<OnlineRegistrationScheduleOption>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GetDoctorScheduleOptions(
            [FromQuery] Guid? clinicId,
            [FromQuery] DateTime? visitDate,
            CancellationToken cancellationToken = default)
        {
            MarkNoStore();
            return Respond(await _publicService.GetScheduleOptionsAsync(clinicId, visitDate, cancellationToken));
        }

        /// <summary>Penyedia asuransi untuk kartu asuransi baru.</summary>
        [HttpGet("options/insurance-providers")]
        [EnableRateLimiting(OptionsRateLimitPolicy)]
        [ProducesResponseType(typeof(ApiResponse<List<OnlineRegistrationInsuranceProviderOption>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GetInsuranceProviderOptions(CancellationToken cancellationToken = default)
        {
            MarkNoStore();
            return Respond(await _publicService.GetInsuranceProviderOptionsAsync(cancellationToken));
        }

        /// <summary>
        /// Cari faskes perujuk mitra yang perjanjiannya berlaku pada <paramref name="visitDate"/> (minimal 3 huruf,
        /// paling banyak 20 baris; <c>KSK-DEC-057</c>). Tanpa <paramref name="visitDate"/> yang sah → <c>400</c>.
        /// </summary>
        [HttpGet("options/referral-institutions")]
        [EnableRateLimiting(OptionsRateLimitPolicy)]
        [ProducesResponseType(typeof(ApiResponse<List<OnlineRegistrationReferralInstitutionOption>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GetReferralInstitutionOptions(
            [FromQuery] string? search,
            [FromQuery] DateTime? visitDate,
            CancellationToken cancellationToken = default)
        {
            MarkNoStore();
            return Respond(await _publicService.GetReferralInstitutionOptionsAsync(search, visitDate, cancellationToken));
        }

        /// <summary>Mencocokkan pasien lama dengan NIK/No. RM dan tanggal lahir (<c>KSK-DEC-031</c>).</summary>
        /// <remarks>
        /// Tidak cocok, pasien non-aktif, pasien gabungan, atau tanggal lahir salah semuanya dijawab
        /// <c>404</c> dengan pesan yang sama. Captcha ditolak <c>400</c>; Cloudflare tidak terjangkau <c>503</c>.
        /// </remarks>
        [HttpPost("patient-match")]
        [EnableRateLimiting(MatchRateLimitPolicy)]
        [ProducesResponseType(typeof(ApiResponse<OnlineRegistrationPatientMatchResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> MatchPatient(
            [FromBody] OnlineRegistrationPatientMatchRequest? request,
            CancellationToken cancellationToken = default)
        {
            MarkNoStore();
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            return Respond(await _publicService.MatchPatientAsync(request, remoteIp, cancellationToken));
        }

        /// <summary>Mengirim pendaftaran online (<c>FR-KSK-PO-012</c>).</summary>
        /// <remarks>
        /// <c>multipart/form-data</c>: <c>payload</c> berisi JSON <see cref="OnlineRegistrationSubmitRequest"/>,
        /// <c>referralDocuments</c> berisi 0–3 berkas PDF/JPG/PNG (wajib 1–3 untuk Pasien Rujukan).
        /// Kode check-in hanya muncul di respons ini. <c>404</c> bila data pasien lama tidak cocok;
        /// <c>409</c> bila jadwal tidak tersedia, kuota penuh, atau sudah ada pendaftaran/kunjungan pada
        /// tanggal itu.
        /// </remarks>
        [HttpPost]
        [EnableRateLimiting(SubmitRateLimitPolicy)]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(SubmitRequestLimitBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = SubmitRequestLimitBytes)]
        [ProducesResponseType(typeof(ApiResponse<OnlineRegistrationSubmitResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Submit(
            [FromForm] string? payload,
            [FromForm] List<IFormFile>? referralDocuments,
            CancellationToken cancellationToken = default)
        {
            MarkNoStore();

            OnlineRegistrationSubmitRequest? request = null;

            if (!string.IsNullOrWhiteSpace(payload))
            {
                try
                {
                    request = JsonSerializer.Deserialize<OnlineRegistrationSubmitRequest>(payload, _jsonOptions);
                }
                catch (JsonException)
                {
                    request = null;
                }
            }

            var files = (referralDocuments ?? new List<IFormFile>())
                .Select(f => new OnlineRegistrationUploadFile(f.FileName, f.Length, f.OpenReadStream))
                .ToList();

            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            return Respond(await _publicService.SubmitAsync(request, files, remoteIp, cancellationToken));
        }

        private IActionResult Respond<T>(OnlineRegistrationPublicResult<T> result) => result.Outcome switch
        {
            OnlineRegistrationPublicOutcome.Ok => Ok(ApiResponse<T>.Ok(result.Data!, result.Message)),
            OnlineRegistrationPublicOutcome.NotFound => NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),
            OnlineRegistrationPublicOutcome.Conflict => Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message)),
            OnlineRegistrationPublicOutcome.Unavailable => StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                ApiResponse<object>.Fail(StatusCodes.Status503ServiceUnavailable, result.Message)),
            _ => BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.Message))
        };

        private void MarkNoStore() => Response.Headers.CacheControl = "no-store";
    }
}
