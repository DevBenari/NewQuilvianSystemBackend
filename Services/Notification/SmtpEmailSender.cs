using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Services.Notification
{
    /// <summary>Hasil satu pengiriman email.</summary>
    public enum EmailSendOutcome
    {
        Sent = 1,

        /// <summary>Server menolak, tidak terjangkau, melewati batas waktu, atau alamat tidak sah.</summary>
        Failed = 2,

        /// <summary><c>Email:Smtp:Enabled = false</c> atau konfigurasi tidak lengkap; tidak ada yang dikirim.</summary>
        Disabled = 3
    }

    /// <summary>Satu email teks biasa.</summary>
    public sealed record EmailMessage(string ToAddress, string Subject, string PlainTextBody);

    /// <summary>
    /// Mengirim satu email teks lewat akun SMTP rumah sakit memakai <c>System.Net.Mail</c>
    /// bawaan .NET, tanpa paket baru (kiosk Amendment PO, <c>BE-KSK-PO-003</c>, <c>INT-PO-03</c>).
    ///
    /// <para>Satu percobaan dengan batas waktu 10 detik, tanpa antrean kirim ulang
    /// (<c>GATE-PO-10</c>). Kegagalan dikembalikan sebagai hasil, tidak dilempar, karena pemanggil
    /// (pendaftaran online) tidak boleh batal hanya karena email gagal (<c>KSK-DEC-042</c>).</para>
    ///
    /// <para>Log tidak pernah memuat alamat tujuan, isi email (yang berisi kode pendaftaran),
    /// nama pengguna, maupun sandi SMTP.</para>
    /// </summary>
    public class SmtpEmailSender
    {
        private readonly SmtpEmailOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<SmtpEmailOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<EmailSendOutcome> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            if (!_options.Enabled)
                return EmailSendOutcome.Disabled;

            if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.FromAddress))
            {
                _logger.LogError("Email:Smtp aktif tetapi Host atau FromAddress belum dikonfigurasi.");
                return EmailSendOutcome.Disabled;
            }

            var timeout = _options.ResolveTimeout();
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(timeout);

            try
            {
                using var mail = new MailMessage
                {
                    From = new MailAddress(_options.FromAddress.Trim(), _options.FromName?.Trim(), Encoding.UTF8),
                    Subject = message.Subject,
                    SubjectEncoding = Encoding.UTF8,
                    Body = message.PlainTextBody,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = false
                };
                mail.To.Add(new MailAddress(message.ToAddress.Trim()));

                using var client = new SmtpClient(_options.Host.Trim(), _options.Port)
                {
                    EnableSsl = _options.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = (int)timeout.TotalMilliseconds,
                    UseDefaultCredentials = false
                };

                if (!string.IsNullOrWhiteSpace(_options.UserName))
                    client.Credentials = new NetworkCredential(_options.UserName, _options.Password);

                await client.SendMailAsync(mail, timeoutSource.Token);
                return EmailSendOutcome.Sent;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "Pengiriman email melewati batas waktu {TimeoutSeconds} detik.",
                    timeout.TotalSeconds);
                return EmailSendOutcome.Failed;
            }
            catch (SmtpException exception)
            {
                _logger.LogWarning(
                    "Pengiriman email ditolak atau gagal: {ErrorType}, status SMTP {StatusCode}.",
                    exception.GetType().Name,
                    exception.StatusCode);
                return EmailSendOutcome.Failed;
            }
            catch (Exception exception) when (exception is FormatException or InvalidOperationException or ArgumentException)
            {
                _logger.LogWarning("Pengiriman email tidak dapat diproses: {ErrorType}.", exception.GetType().Name);
                return EmailSendOutcome.Failed;
            }
        }
    }
}
