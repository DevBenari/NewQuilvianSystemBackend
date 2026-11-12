namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;

/// <summary>
/// Sumber konfirmasi hasil pembayaran pada <c>POST .../payments/acknowledge</c> (PAY-BE-11). Seluruh item dalam satu kiriman berasal
/// dari sumber yang sama. Sumber tidak disimpan sebagai kolom (tanpa perubahan skema): ia dicatat pada audit, pada catatan persetujuan
/// run <c>PaymentAck</c>, dan untuk item Paid hasil konfirmasi manual pada <c>PrlPayrollPayment.Notes</c>.
/// </summary>
public static class PayrollConfirmationSources
{
    /// <summary>Hasil bank yang dikirim Finance lewat ACK. Bawaan bila sumber tidak diisi.</summary>
    public const string FinanceAck = "FINANCE_ACK";

    /// <summary>Konfirmasi manual oleh petugas Payroll yang berwenang, selama belum ada koneksi bank.</summary>
    public const string Manual = "MANUAL";

    /// <summary>
    /// Kosong atau null menjadi <see cref="FinanceAck"/>. Dua nilai yang dikenal (tanpa peka huruf besar-kecil, spasi tepi dipangkas)
    /// dikembalikan dalam bentuk bakunya. Nilai lain tidak valid dan mengembalikan null.
    /// </summary>
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return FinanceAck;

        var trimmed = value.Trim();
        if (string.Equals(trimmed, FinanceAck, StringComparison.OrdinalIgnoreCase)) return FinanceAck;
        if (string.Equals(trimmed, Manual, StringComparison.OrdinalIgnoreCase)) return Manual;
        return null;
    }
}
