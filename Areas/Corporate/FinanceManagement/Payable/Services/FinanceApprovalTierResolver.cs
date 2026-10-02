namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;

/// <summary>
/// Jenjang persetujuan berdasarkan nominal (FIN-DEC-022, FIN-DEC-050, FIN-DEC-052).
/// </summary>
public static class ApprovalTiers
{
    public const string Tier1 = "TIER_1";
    public const string Tier2 = "TIER_2";
}

/// <summary>
/// Resolver ambang jenjang persetujuan tunggal, dipakai bersama oleh pembayaran keluar
/// (<c>FinancePaymentService</c>), Purchase Order, dan Purchasing Invoice — satu-satunya tempat
/// ambang nominal ditulis, supaya tidak ada salinan kedua yang bisa menyimpang
/// (02-backend-architecture.md FIN-DES-039, dikoreksi 25 September 2026).
///
/// Ambang: FIN-DEC-052 (approved 25 September 2026) — di bawah Rp 50.000.000 disetujui Supervisor
/// Finance (TIER_1), Rp 50.000.000 ke atas (termasuk tepat Rp 50.000.000) disetujui Manajer
/// Finance (TIER_2). Nilai ini menggantikan placeholder provisional sebelumnya
/// (&lt;= 50.000.000 => TIER_1) yang salah satu nilai batasnya (tepat Rp 50.000.000) tidak cocok
/// dengan keputusan yang sudah diratifikasi.
/// </summary>
public static class FinanceApprovalTierResolver
{
    public static string Resolve(decimal totalAmount)
    {
        return totalAmount switch
        {
            < 50_000_000m => ApprovalTiers.Tier1,
            _ => ApprovalTiers.Tier2
        };
    }
}
