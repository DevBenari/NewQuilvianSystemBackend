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
    /// Mengurutkan <see cref="MstAllowanceType"/> di level query database SEBELUM pagination sehingga jenis tunjangan
    /// yang layak ditetapkan langsung kepada pegawai selalu muncul lebih dulu di halaman awal, diikuti jenis lain
    /// berdasarkan nama dan kode agar data Master Tunjangan yang belum layak tetap dapat ditemukan beserta alasannya.
    /// </summary>
    public static IOrderedQueryable<MstAllowanceType> OrderByWorkforceEligibilityFirst(
        IQueryable<MstAllowanceType> query,
        ApplicationDbContext dbContext)
    {
        var allowedMethods = AllowedMethodsLower;
        var policyCategories = PolicyCategoriesLower;

        return query
            .OrderByDescending(x =>
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
                            p.PayrollComponentId == x.PayrollComponentId && p.IsActive && !p.IsDelete))))
            .ThenBy(x => x.AllowanceTypeName)
            .ThenBy(x => x.AllowanceTypeCode);
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
        var safeName = string.IsNullOrWhiteSpace(name) ? "ini" : name.Trim();
        var safeCategory = (category ?? string.Empty).Trim();
        var safeMethod = (calculationMethod ?? string.Empty).Trim();

        if (!isActive)
            return "Jenis tunjangan tidak ditemukan atau tidak aktif di Master Tunjangan.";

        if (string.Equals(safeMethod, MasterManualInput, StringComparison.OrdinalIgnoreCase))
            return $"Tunjangan {safeName} bersifat input per periode (metode Input Manual). Masukkan lewat Penghasilan Variabel pada Payroll Run, bukan sebagai tunjangan tetap pegawai.";

        if (!isRecurring)
            return $"Tunjangan {safeName} bersifat input per periode (opsi Berulang nonaktif di Master Tunjangan). Aktifkan Berulang di Master Tunjangan atau masukkan lewat Penghasilan Variabel pada Payroll Run.";

        if (string.Equals(safeMethod, MasterPolicyBased, StringComparison.OrdinalIgnoreCase))
            return $"Tunjangan {safeName} dihitung dari kebijakan (metode Berdasarkan Kebijakan). Kelola lewat kebijakannya, bukan sebagai tunjangan tetap pegawai.";

        if (PolicyCategories.Contains(safeCategory))
            return $"Tunjangan {safeName} dihitung dari kebijakan atau aktivitas kerja (kategori {BuildPolicyCategoryLabel(safeCategory)}). Kelola lewat kebijakannya, bukan sebagai tunjangan tetap pegawai.";

        if (hasActivePolicy)
            return $"Tunjangan {safeName} dihitung dari kebijakan tunjangan aktif yang sudah terhubung (Shift/On Call/Risiko/Transportasi). Kelola lewat kebijakannya agar tidak terhitung ganda.";

        if (!string.Equals(safeMethod, MasterFixed, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(safeMethod, MasterPercentage, StringComparison.OrdinalIgnoreCase))
            return $"Metode hitung tunjangan {safeName} ({safeMethod}) belum didukung untuk penetapan per pegawai.";

        if (hasInvalidPayrollComponent)
            return $"Komponen payroll untuk tunjangan {safeName} tidak valid atau tidak aktif. Perbaiki di Master Tunjangan.";

        return null;
    }

    private static string BuildPolicyCategoryLabel(string category)
    {
        return category.ToLowerInvariant() switch
        {
            "shift" => "Shift",
            "oncall" => "On Call",
            "hazard" => "Risiko",
            "transport" => "Transportasi",
            "variable" => "Variabel",
            _ => category
        };
    }
}
