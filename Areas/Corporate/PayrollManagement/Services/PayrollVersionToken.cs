namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Token konkurensi optimistik turunan untuk payroll run. <c>IdentityModel</c> tidak memiliki kolom RowVersion, sehingga token
/// diturunkan dari waktu pembaruan terakhir (tick) tanpa perubahan skema. Klien mengirim kembali token yang ia lihat sebagai
/// <c>expectedRowVersion</c>; bila berbeda dengan token saat ini, aksi ditolak dengan 409.
/// </summary>
public static class PayrollVersionToken
{
    public static string From(DateTime createDateTime, DateTime? updateDateTime) =>
        (updateDateTime ?? createDateTime).Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Token kosong berarti klien tidak meminta pemeriksaan (kompatibel dengan klien lama).</summary>
    public static void EnsureMatches(string? expected, DateTime createDateTime, DateTime? updateDateTime, string subject)
    {
        if (string.IsNullOrWhiteSpace(expected)) return;
        if (!string.Equals(expected.Trim(), From(createDateTime, updateDateTime), StringComparison.Ordinal))
            throw new PayrollVersionConflictException($"{subject} sudah diperbarui pihak lain. Muat ulang lalu coba lagi.");
    }
}
