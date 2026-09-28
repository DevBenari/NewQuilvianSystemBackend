using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;

/// <summary>
/// Nama role ASP.NET Identity yang menentukan jenjang persetujuan Finance (FIN-DEC-052,
/// FIN-VAL-102, BE-FIN-032). Dipilih lewat AskUserQuestion 26 September 2026 karena tidak ada
/// mekanisme role/klaim apa pun sebelumnya yang membedakan penyetuju TIER_1 dari TIER_2 —
/// bahkan FinancePaymentService.ApproveAsync (BE-FIN-020, sudah selesai) belum punya pengecekan
/// ini, hanya menolak self-approval. Dua role ini disiapkan FinanceApprovalRoleSeeder.
/// </summary>
public static class FinanceApprovalRoles
{
    public const string Supervisor = "Supervisor Finance";
    public const string Manager = "Manajer Finance";
}

/// <summary>
/// Menentukan apakah seorang actor berwenang menyetujui sesuatu pada jenjang
/// (<see cref="ApprovalTiers"/>) tertentu. Manajer Finance MUST NOT dianggap lebih rendah dari
/// Supervisor Finance — role Manajer mencakup wewenang Supervisor (TIER_1 dan TIER_2 keduanya),
/// bukan dua jenjang yang saling eksklusif.
/// </summary>
public sealed class FinanceApprovalAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public FinanceApprovalAuthorizationService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> CanApproveAsync(Guid actorUserId, string approvalTier, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(actorUserId.ToString());
        if (user is null) return false;

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Contains(FinanceApprovalRoles.Manager)) return true;

        return approvalTier == ApprovalTiers.Tier1 && roles.Contains(FinanceApprovalRoles.Supervisor);
    }
}
