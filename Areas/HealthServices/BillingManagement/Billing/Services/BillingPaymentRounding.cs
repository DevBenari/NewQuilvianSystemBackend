namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Kebijakan pembulatan pembayaran pasien (keputusan pemilik modul Billing, 25 November 2026): nominal yang harus dibayar dibulatkan
/// KE ATAS ke rupiah penuh, untuk SEMUA metode pembayaran (tunai dan non-tunai). Tidak ada sen pada uang tunai maupun transfer.
/// Contoh: sisa Rp24.461.828,74 dibayar Rp24.461.829; selisih Rp0,26 dicatat sebagai penyesuaian pembulatan (BilAdjustment) yang dapat
/// diaudit, bukan sebagai kembalian, deposit, atau pendapatan lain. Satu sumber untuk seluruh titik pembulatan agar tidak ada yang
/// masih memakai pembulatan terdekat (0,5 naik).
/// </summary>
internal static class BillingPaymentRounding
{
    /// <summary>Sisa dibulatkan ke atas ke rupiah penuh. Nilai sudah bulat tetap; nilai non-positif menjadi 0.</summary>
    public static decimal RoundUp(decimal amount) =>
        amount <= 0m ? 0m : Math.Ceiling(decimal.Round(amount, 2, MidpointRounding.AwayFromZero));
}
