namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;

/// <summary>
/// Status pembayaran GAJI satu penerima pada satu payroll run, diturunkan dari instruksi pembayarannya (PAY-BE-11). Bukan kolom
/// tersimpan. Dipakai untuk menandai potongan piutang yang sudah dikonfirmasi ke Finance (saat run disahkan) tetapi gaji
/// penerimanya belum dibayar atau gagal. Aturan penurunannya tunggal di <c>PayrollSalaryPaymentStatusResolver</c>.
/// </summary>
public static class PayrollSalaryPaymentStatuses
{
    /// <summary>Belum ada instruksi pembayaran (run belum dikirim ke Finance) dan masih ada gaji yang harus dibayar.</summary>
    public const string NotSent = "BELUM_DIKIRIM";

    /// <summary>Instruksi sudah dikirim (Scheduled/Processing) tetapi hasil bank belum dikonfirmasi.</summary>
    public const string Pending = "MENUNGGU_BAYAR";

    /// <summary>Gaji sudah dibayar (instruksi Paid atau PartiallyReversed).</summary>
    public const string Paid = "LUNAS";

    /// <summary>Instruksi gagal dan belum ada pengganti yang hidup.</summary>
    public const string Failed = "GAGAL";

    /// <summary>Pembayaran sudah dibalik seluruhnya.</summary>
    public const string Reversed = "DIBALIK";

    /// <summary>Tidak ada gaji yang harus dibayar (nominal pembayaran nol), jadi tidak ada instruksi.</summary>
    public const string NothingToPay = "TIDAK_ADA_PEMBAYARAN";

    /// <summary>Perlu perhatian karena gaji belum dibayar: belum dikirim atau menunggu hasil bank.</summary>
    public static bool IsPendingAttention(string? status) => status is NotSent or Pending;

    /// <summary>Perlu perhatian karena pembayaran gagal atau dibalik setelah potongan sudah dikonfirmasi.</summary>
    public static bool IsFailedAttention(string? status) => status is Failed or Reversed;

    /// <summary>Tidak ada yang menunggu: sudah lunas atau memang tidak ada gaji yang harus dibayar.</summary>
    public static bool IsSettled(string? status) => status is Paid or NothingToPay;
}
