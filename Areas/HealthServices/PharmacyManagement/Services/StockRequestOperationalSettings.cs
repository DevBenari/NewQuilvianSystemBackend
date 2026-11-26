using System.Globalization;
using Microsoft.Extensions.Configuration;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Tenggat verifikasi susulan CITO (<c>LGS-DEC-005</c>), dapat dikonfigurasi per lingkungan.
/// </summary>
/// <remarks>
/// <para>
/// Tidak ada nilai bawaan: lamanya tenggat adalah keputusan operasional rumah sakit, bukan
/// ketetapan kode. Selama <see cref="DueHoursConfigKey"/> belum diatur, keadaan tenggat dilaporkan
/// <see cref="StockRequestEmergencyReviewDueState.NotConfigured"/>.
/// </para>
/// <para>
/// Waktu acuan adalah waktu otorisasi CITO di riwayat (ditulis server, UTC). Perhitungan tidak
/// mengubah apa pun: tidak ada verifikasi otomatis dan tidak ada mutasi stok.
/// </para>
/// </remarks>
public sealed record StockRequestEmergencyReviewDeadline(
    bool IsConfigured,
    TimeSpan? DueAfter,
    TimeSpan? WarningBefore,
    string Message)
{
    public const string DueHoursConfigKey = "PharmacyManagement:StockRequest:EmergencyReviewDueHours";
    public const string WarningHoursConfigKey = "PharmacyManagement:StockRequest:EmergencyReviewWarningHours";

    /// <summary>
    /// Batas teknis, bukan kebijakan: 100 tahun. Nilai di atasnya tidak bermakna sebagai tenggat dan
    /// membuat hitungan waktu meluap — <c>TimeSpan.FromHours</c> melempar galat di atas ±2,56×10⁸ jam,
    /// dan pengurangan dari waktu sekarang keluar dari rentang <c>DateTime</c> jauh sebelum itu.
    /// Karena tenggat dibaca saat service dibuat, luapan itu akan menjatuhkan seluruh endpoint
    /// permintaan stok; nilai demikian diperlakukan sama dengan nilai tidak sah lainnya.
    /// </summary>
    public const double MaxConfigurableHours = 24d * 366 * 100;

    public static StockRequestEmergencyReviewDeadline FromConfiguration(IConfiguration? configuration) =>
        Resolve(configuration?[DueHoursConfigKey], configuration?[WarningHoursConfigKey]);

    /// <summary>Menafsirkan nilai jam. Fungsi murni.</summary>
    public static StockRequestEmergencyReviewDeadline Resolve(string? rawDueHours, string? rawWarningHours)
    {
        if (string.IsNullOrWhiteSpace(rawDueHours))
            return new(false, null, null, "Tenggat verifikasi susulan CITO belum dikonfigurasi.");

        if (!double.TryParse(rawDueHours.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture,
                out var dueHours) || double.IsNaN(dueHours) || double.IsInfinity(dueHours) || dueHours <= 0 ||
            dueHours > MaxConfigurableHours)
            return new(false, null, null,
                $"Tenggat verifikasi susulan CITO '{rawDueHours}' tidak sah; tenggat dianggap belum dikonfigurasi.");

        TimeSpan? warning = null;
        if (!string.IsNullOrWhiteSpace(rawWarningHours))
        {
            if (!double.TryParse(rawWarningHours.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var warningHours) || double.IsNaN(warningHours) || warningHours < 0 ||
                warningHours >= dueHours)
                return new(false, null, null,
                    $"Batas peringatan tenggat '{rawWarningHours}' harus antara 0 dan kurang dari tenggat " +
                    $"{dueHours} jam; tenggat dianggap belum dikonfigurasi.");

            if (warningHours > 0) warning = TimeSpan.FromHours(warningHours);
        }

        return new(true, TimeSpan.FromHours(dueHours), warning,
            $"Tenggat verifikasi susulan CITO {dueHours} jam sejak otorisasi" +
            (warning.HasValue ? $", peringatan {warning.Value.TotalHours} jam sebelum tenggat." : "."));
    }

    /// <summary>Saat jatuh tempo untuk otorisasi pada <paramref name="authorizedAtUtc"/>.</summary>
    public DateTime? DueAtUtc(DateTime? authorizedAtUtc) =>
        IsConfigured && authorizedAtUtc.HasValue ? authorizedAtUtc.Value + DueAfter!.Value : null;

    /// <summary>
    /// Keadaan tenggat untuk satu CITO yang masih menunggu verifikasi. Untuk keadaan lain
    /// pemanggil memakai <see cref="StockRequestEmergencyReviewDueState.NotApplicable"/>.
    /// </summary>
    public StockRequestEmergencyReviewDueState Classify(DateTime? authorizedAtUtc, DateTime nowUtc)
    {
        if (!IsConfigured) return StockRequestEmergencyReviewDueState.NotConfigured;
        if (!authorizedAtUtc.HasValue) return StockRequestEmergencyReviewDueState.NotApplicable;

        var dueAt = authorizedAtUtc.Value + DueAfter!.Value;
        if (nowUtc >= dueAt) return StockRequestEmergencyReviewDueState.Overdue;
        if (WarningBefore.HasValue && nowUtc >= dueAt - WarningBefore.Value)
            return StockRequestEmergencyReviewDueState.DueSoon;
        return StockRequestEmergencyReviewDueState.NotDue;
    }
}
