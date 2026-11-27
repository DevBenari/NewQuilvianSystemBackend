using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Services.Security
{
    /// <summary>Hasil satu verifikasi captcha.</summary>
    public enum TurnstileVerificationOutcome
    {
        /// <summary>Token sah, permintaan boleh lanjut.</summary>
        Passed = 1,

        /// <summary>Token kosong atau ditolak Cloudflare (<c>KSK-VAL-PO-010</c>, <c>400</c>).</summary>
        Rejected = 2,

        /// <summary>Cloudflare tidak terjangkau atau konfigurasi tidak lengkap (<c>KSK-VAL-PO-011</c>, <c>503</c>).</summary>
        Unavailable = 3
    }

    /// <summary>
    /// Memeriksa token Cloudflare Turnstile ke <c>siteverify</c> (kiosk Amendment PO,
    /// <c>BE-KSK-PO-003</c>, <c>INT-PO-01</c>).
    ///
    /// <para>Ini panggilan HTTP keluar pertama di backend (<c>CAP-PO-04</c>). Batas waktunya 5
    /// detik dan tidak ada retry: pasien yang captcha-nya tidak dapat diperiksa diminta mencoba
    /// lagi, bukan dibiarkan menunggu. Bawaannya fail-closed (<c>GATE-PO-07</c>).</para>
    ///
    /// <para>Log hanya memuat hasil dan kode galat Cloudflare. Token, secret, dan alamat IP
    /// pengunjung tidak pernah dicatat.</para>
    /// </summary>
    public class TurnstileVerificationService
    {
        public const string HttpClientName = "Turnstile";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TurnstileOptions _options;
        private readonly ILogger<TurnstileVerificationService> _logger;

        public TurnstileVerificationService(
            IHttpClientFactory httpClientFactory,
            IOptions<TurnstileOptions> options,
            ILogger<TurnstileVerificationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<TurnstileVerificationOutcome> VerifyAsync(
            string? token,
            string? remoteIp,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return TurnstileVerificationOutcome.Passed;

            if (string.IsNullOrWhiteSpace(token) || token.Length > 2048)
                return TurnstileVerificationOutcome.Rejected;

            if (string.IsNullOrWhiteSpace(_options.SecretKey) || string.IsNullOrWhiteSpace(_options.VerifyUrl))
            {
                _logger.LogError("Turnstile aktif tetapi SecretKey atau VerifyUrl belum dikonfigurasi.");
                return ResolveUnavailable();
            }

            var form = new List<KeyValuePair<string, string>>
            {
                new("secret", _options.SecretKey),
                new("response", token.Trim())
            };

            if (!string.IsNullOrWhiteSpace(remoteIp))
                form.Add(new("remoteip", remoteIp.Trim()));

            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(_options.ResolveTimeout());

            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientName);
                using var content = new FormUrlEncodedContent(form);
                using var response = await client.PostAsync(_options.VerifyUrl, content, timeoutSource.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Verifikasi Turnstile gagal dihubungi: status HTTP {StatusCode}.",
                        (int)response.StatusCode);
                    return ResolveUnavailable();
                }

                await using var stream = await response.Content.ReadAsStreamAsync(timeoutSource.Token);
                var result = await JsonSerializer.DeserializeAsync<SiteVerifyResponse>(
                    stream,
                    cancellationToken: timeoutSource.Token);

                if (result?.Success == true)
                    return TurnstileVerificationOutcome.Passed;

                _logger.LogInformation(
                    "Token Turnstile ditolak. Kode galat: {ErrorCodes}.",
                    result?.ErrorCodes is { Length: > 0 } codes ? string.Join(",", codes) : "-");
                return TurnstileVerificationOutcome.Rejected;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "Verifikasi Turnstile melewati batas waktu {TimeoutSeconds} detik.",
                    _options.ResolveTimeout().TotalSeconds);
                return ResolveUnavailable();
            }
            catch (Exception exception) when (exception is HttpRequestException or JsonException)
            {
                _logger.LogWarning(
                    "Verifikasi Turnstile tidak dapat diselesaikan: {ErrorType}.",
                    exception.GetType().Name);
                return ResolveUnavailable();
            }
        }

        /// <summary>
        /// Dipanggil sekali saat aplikasi menyala. Captcha yang dimatikan atau tanpa secret hanya
        /// sah di lingkungan pengembangan, jadi keduanya wajib terlihat di log.
        /// </summary>
        public static void WarnIfMisconfigured(IServiceProvider services)
        {
            var options = services.GetRequiredService<IOptions<TurnstileOptions>>().Value;
            var logger = services.GetRequiredService<ILoggerFactory>()
                .CreateLogger<TurnstileVerificationService>();

            if (!options.Enabled)
            {
                logger.LogWarning(
                    "Turnstile dimatikan (Turnstile:Enabled = false): seluruh token captcha dianggap lolos. " +
                    "Hanya untuk lingkungan pengembangan.");
                return;
            }

            if (string.IsNullOrWhiteSpace(options.SecretKey))
            {
                logger.LogWarning(
                    "Turnstile aktif tetapi Turnstile:SecretKey kosong. Pendaftaran online akan ditolak " +
                    "sampai kunci diisi lewat user-secrets atau variabel lingkungan.");
            }

            if (options.FailOpen)
            {
                logger.LogWarning(
                    "Turnstile:FailOpen = true: bila Cloudflare tidak terjangkau, captcha dianggap lolos.");
            }
        }

        private TurnstileVerificationOutcome ResolveUnavailable() =>
            _options.FailOpen ? TurnstileVerificationOutcome.Passed : TurnstileVerificationOutcome.Unavailable;

        private sealed class SiteVerifyResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("error-codes")]
            public string[]? ErrorCodes { get; set; }
        }
    }
}
