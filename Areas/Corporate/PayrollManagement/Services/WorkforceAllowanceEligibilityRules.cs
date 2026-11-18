using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Sumber kebenaran tunggal aturan kelayakan jenis Master Tunjangan (<see cref="MstAllowanceType"/>) untuk penetapan tunjangan
/// tetap khusus pegawai (<c>PrlWorkforceAllowance</c>).
///
/// Dipakai bersama oleh:
/// 1. <c>AllowanceTypeController.GetAllowanceTypeOptions</c> (filter SQL sebelum pagination + metadata kelayakan opsi), dan
/// 2. <c>PayrollWorkforceAllowanceService.LoadEligibleTypeAsync</c> (validasi sisi penyimpanan POST/PUT/Approve).
///
/// Menjaga agar pilihan di dropdown Frontend dan validasi penyimpanan Backend selalu 100% selaras tanpa duplikasi aturan atau N+1 query.
/// </summary>
public static class WorkforceAllowanceEligibilityRules
{
    public const string MasterFixed = "Fixed";
    public const string MasterPercentage = "Percentage";
    public const string MasterPolicyBased = "PolicyBased";
    public const string MasterManualInput = "ManualInput";

    // Nilai huruf kecil untuk penerjemahan langsung ke SQL `LOWER(...) IN (...)` di EF Core / PostgreSQL.
    private static readonly string[] AllowedMethodsLower = { "fixed", "percentage" };
    private static readonly string[] PolicyCategoriesLower = { "shift", "oncall", "hazard", "transport", "variable" };

    /// <summary>Kategori master yang dihitung dari aktivitas atau kebijakan, bukan dari penetapan tetap per pegawai.</summary>
    public static readonly HashSet<string> PolicyCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "Shift", "OnCall", "Hazard", "Transport", "Variable"
    };

    /// <summary>
    /// Menyaring <see cref="MstAllowanceType"/> di level query database SEBELUM pagination sehingga seluruh halaman opsi
    /// hanya memuat jenis tunjangan yang sah ditetapkan langsung kepada pegawai (aktif, belum dihapus, recurring, metode
    /// Fixed/Percentage, bukan kategori kebijakan/aktivitas, tidak terkait kebijakan shift/on-call/hazard/transportasi aktif,
    /// dan komponen payroll terkait aktif bila diisi).
    /// </summary>
    public static IQueryable<MstAllowanceType> ApplyEligibleForWorkforceAssignmentFilter(
        IQueryable<MstAllowanceType> query,
        ApplicationDbContext dbContext)
    {
        var allowedMethods = AllowedMethodsLower;
        var policyCategories = PolicyCategoriesLower;

        return query.Where(x =>
            !x.IsDelete
            && x.IsActive
            && x.IsRecurring
            && allowedMethods.Contains(x.CalculationMethod.ToLower())
            && !policyCategories.Contains(x.AllowanceCategory.ToLower())
            && !x.ShiftAllowancePolicies.Any(p => p.IsActive && !p.IsDelete)
            && !x.OnCallAllowancePolicies.Any(p => p.IsActive && !p.IsDelete)
            && !x.HazardAllowancePolicies.Any(p => p.IsActive && !p.IsDelete)
            && (x.PayrollComponentId == null
                || (x.PayrollComponent != null
                    && x.PayrollComponent.IsActive
                    && !x.PayrollComponent.IsDelete
                    && !dbContext.PrlTransportAllowancePolicies.Any(p =>
                        p.PayrollComponentId == x.PayrollComponentId && p.IsActive && !p.IsDelete))));
    }

    /// <summary>
    /// Mengevaluasi alasan ketidaklayakan satu jenis tunjangan untuk penetapan khusus pegawai.
    /// Mengembalikan <c>null</c> bila layak; bila tidak layak, mengembalikan pesan spesifik yang juga dipakai pada validasi POST/PUT.
    /// </summary>
    public static string? EvaluateIneligibilityReason(
        string name,
        string category,
        string calculationMethod,
        bool isActive,
        bool isRecurring,
        bool hasActivePolicy,
        bool hasInvalidPayrollComponent)
    {
        if (!isActive)
            return "Jenis tunjangan tidak ditemukan atau tidak aktif di Master Tunjangan.";

        if (string.Equals(calculationMethod, MasterManualInput, StringComparison.OrdinalIgnoreCase) || !isRecurring)
            return $"Tunjangan {name} bersifat input per periode. Masukkan lewat Penghasilan Variabel pada Payroll Run, bukan sebagai tunjangan tetap pegawai.";

        if (string.Equals(calculationMethod, MasterPolicyBased, StringComparison.OrdinalIgnoreCase)
            || PolicyCategories.Contains(category)
            || hasActivePolicy)
            return $"Tunjangan {name} dihitung dari kebijakan atau aktivitas kerja. Kelola lewat kebijakannya (mis. Tunjangan Transportasi), bukan sebagai tunjangan tetap pegawai.";

        if (!string.Equals(calculationMethod, MasterFixed, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(calculationMethod, MasterPercentage, StringComparison.OrdinalIgnoreCase))
            return $"Metode hitung tunjangan {name} ({calculationMethod}) belum didukung untuk penetapan per pegawai.";

        if (hasInvalidPayrollComponent)
            return $"Komponen payroll untuk tunjangan {name} tidak valid atau tidak aktif. Perbaiki di Master Tunjangan.";

        return null;
    }
}
