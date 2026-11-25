using System.Globalization;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Batas aktivasi persetujuan atasan per lingkungan (<c>LGS-DEC-006</c>).
/// </summary>
/// <remarks>
/// <para>
/// Permintaan yang diajukan <b>sebelum</b> batas mengikuti aturan peralihan: masih boleh
/// diserahkan langsung, dan tetap boleh diputus atasan. Permintaan yang diajukan sesudahnya
/// wajib disetujui. Waktu pengajuan yang dibandingkan adalah <c>PhmStockRequest.SubmittedAt</c>
/// yang ditulis server saat pengajuan, bukan waktu dari browser.
/// </para>
/// <para>
/// Prinsipnya: keadaan apa pun selain batas yang sah dan sudah lewat berarti <b>semua permintaan
/// wajib disetujui</b>. Nilai kosong, nilai yang tidak dapat dibaca, dan nilai di masa depan
/// tidak pernah membuka jalan pintas. Batas di masa depan ditolak karena keputusan menetapkan
/// batas dikunci pada saat aktivasi; menjadwalkannya ke depan berarti permintaan yang diajukan
/// sebelum waktu itu kelak berubah menjadi permintaan lama secara surut.
/// </para>
/// <para>
/// Keterbatasan yang dicatat: penguncian batas setelah aktivasi bergantung pada kendali
/// perubahan konfigurasi. Sistem mencatat setiap perubahan nilai yang terlihat pada log, tetapi
/// tidak dapat menolaknya tanpa tabel aktivasi tersendiri (butuh migration).
/// </para>
/// </remarks>
public sealed record StockRequestApprovalCutover(
    StockRequestApprovalCutoverState State,
    DateTime? EffectiveFromUtc,
    string? RawValue,
    string Message)
{
    /// <summary>Selisih jam antarserver yang masih ditoleransi untuk batas "saat ini".</summary>
    public static readonly TimeSpan ClockTolerance = TimeSpan.FromMinutes(5);

    /// <summary>Hanya keadaan <see cref="StockRequestApprovalCutoverState.Active"/> yang memberi aturan peralihan.</summary>
    public bool AllowsLegacyTransition => State == StockRequestApprovalCutoverState.Active;

    /// <summary>
    /// Menafsirkan nilai konfigurasi. Fungsi murni: tidak membaca jam sendiri, tidak menulis log.
    /// </summary>
    /// <param name="rawValue">Nilai mentah konfigurasi, ISO 8601. Tanpa zona waktu dianggap UTC.</param>
    /// <param name="utcNow">Waktu server saat ini (UTC).</param>
    public static StockRequestApprovalCutover Resolve(string? rawValue, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return new(StockRequestApprovalCutoverState.NotConfigured, null, null,
                "Batas aktivasi persetujuan belum diatur; seluruh permintaan wajib disetujui atasan unit.");

        var trimmed = rawValue.Trim();

        if (!DateTimeOffset.TryParse(trimmed, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out var parsed))
            return new(StockRequestApprovalCutoverState.Invalid, null, trimmed,
                $"Batas aktivasi persetujuan '{trimmed}' bukan tanggal-waktu yang sah; aturan " +
                "peralihan diabaikan dan seluruh permintaan wajib disetujui atasan unit.");

        var cutoverUtc = parsed.UtcDateTime;

        if (cutoverUtc > utcNow + ClockTolerance)
            return new(StockRequestApprovalCutoverState.FutureRejected, null, trimmed,
                $"Batas aktivasi persetujuan {cutoverUtc:O} berada di masa depan. Batas dikunci pada " +
                "saat aktivasi dan tidak boleh dijadwalkan; aturan peralihan diabaikan dan seluruh " +
                "permintaan wajib disetujui atasan unit.");

        return new(StockRequestApprovalCutoverState.Active, cutoverUtc, trimmed,
            $"Batas aktivasi persetujuan berlaku sejak {cutoverUtc:O}; permintaan yang diajukan " +
            "sebelumnya mengikuti aturan peralihan.");
    }

    /// <summary>
    /// Benar bila permintaan Submitted diajukan sebelum batas yang aktif. Permintaan tanpa waktu
    /// pengajuan tidak pernah dianggap permintaan lama, supaya data yang tidak lengkap tidak
    /// menjadi jalan pintas.
    /// </summary>
    public bool IsBeforeCutover(DateTime? submittedAtUtc) =>
        AllowsLegacyTransition &&
        submittedAtUtc.HasValue &&
        submittedAtUtc.Value < EffectiveFromUtc!.Value;
}

/// <summary>Keadaan batas aktivasi persetujuan.</summary>
public enum StockRequestApprovalCutoverState
{
    /// <summary>Tidak diatur; semua permintaan wajib disetujui.</summary>
    NotConfigured = 0,

    /// <summary>Sah dan sudah lewat; permintaan sebelum batas mengikuti aturan peralihan.</summary>
    Active = 1,

    /// <summary>Tidak dapat dibaca; diabaikan, semua permintaan wajib disetujui.</summary>
    Invalid = 2,

    /// <summary>Di masa depan; ditolak, semua permintaan wajib disetujui.</summary>
    FutureRejected = 3
}
