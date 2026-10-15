namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

/// <summary>
/// Keadaan satu baris penahanan stok milik resep (<c>PHA-DEC-066</c>).
/// </summary>
/// <remarks>
/// Keempat keadaan ini saling eksklusif dan hanya bergerak satu arah: <c>Reserved</c> menuju
/// salah satu dari tiga keadaan akhir. Tidak ada jalan kembali ke <c>Reserved</c> — penahanan
/// baru selalu menjadi baris baru, supaya histori tidak hilang.
/// </remarks>
public enum PrescriptionReservationState
{
    /// <summary>Stok sedang ditahan untuk resep ini.</summary>
    Reserved = 1,

    /// <summary>Obat sudah diserahkan; penahanan berubah menjadi pengurangan stok fisik.</summary>
    Consumed = 2,

    /// <summary>Dilepas sebelum diserahkan: resep dibatalkan, atau pembayaran tidak dilanjutkan.</summary>
    Released = 3,

    /// <summary>
    /// Dilepas otomatis karena melewati batas waktu sebelum pembayaran valid. Dibedakan dari
    /// <see cref="Released"/> karena laporan perlu memisahkan pembatalan yang disengaja dari
    /// pasien yang tidak kembali menebus.
    /// </summary>
    Expired = 4
}

/// <summary>
/// Alasan pelepasan penahanan, dipakai sebagai nilai <c>ReleaseReasonCode</c>.
/// </summary>
/// <remarks>
/// Kode, bukan kalimat bebas, supaya jumlah penahanan yang lepas per sebab dapat dihitung tanpa
/// menafsirkan teks.
/// </remarks>
public static class PrescriptionReservationReleaseReasons
{
    /// <summary>Resep dibatalkan (<c>PHA-DEC-066</c> butir 5).</summary>
    public const string PrescriptionCancelled = "PHA_RSV_PRESCRIPTION_CANCELLED";

    /// <summary>Kasir menandai pembayaran tidak dilanjutkan (<c>PHA-DEC-023</c>).</summary>
    public const string PaymentNotContinued = "PHA_RSV_PAYMENT_NOT_CONTINUED";

    /// <summary>Pembalikan tagihan oleh Billing.</summary>
    public const string PaymentReversed = "PHA_RSV_PAYMENT_REVERSED";

    /// <summary>Batas waktu sebelum pembayaran habis (<c>PHA-DEC-066</c> butir 6).</summary>
    public const string ExpiredBeforePayment = "PHA_RSV_EXPIRED_BEFORE_PAYMENT";

    /// <summary>Episode rawat inap atau IGD ditutup tanpa penyerahan (butir 8).</summary>
    public const string EpisodeClosed = "PHA_RSV_EPISODE_CLOSED";
}

/// <summary>
/// Kode galat bisnis penahanan stok.
/// </summary>
/// <remarks>
/// <c>PHA-DEC-066</c> menuntut keadaan bisnis normal menghasilkan status yang eksplisit, bukan
/// <c>HTTP 500</c> generik. Kode-kode ini dipakai pada pesan penolakan yang dibaca pengguna.
/// </remarks>
public static class PrescriptionReservationErrorCodes
{
    public const string InsufficientStock = "PHA_RSV_INSUFFICIENT_STOCK";
    public const string ReservationExpired = "PHA_RSV_EXPIRED";
    public const string ReservationUnavailable = "PHA_RSV_UNAVAILABLE";
    public const string ReservationMismatch = "PHA_RSV_MISMATCH";
    public const string DepotUnresolved = "PHA_RSV_DEPOT_UNRESOLVED";
    public const string ConcurrentStockChange = "PHA_RSV_CONCURRENT_CHANGE";
}
