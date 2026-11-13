using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// PAY-BE-15 (blueprint payroll-gaji PGJ-C1, PRL-DEC-12): batas gaji Struktur Gaji wajib berada di dalam rentang Grade Gaji,
/// dan rentang grade tidak boleh dipersempit sampai struktur aktif keluar batas. Validation matrix SR-01..SR-04.
/// Hanya membaca; dipanggil controller master struktur dan grade sebelum menyimpan, sehingga controller tidak menambah
/// akses ApplicationDbContext baru (QBE-SVC-001).
///
/// Grade yang batas atasnya 0 dianggap belum punya rentang (nilai bawaan kolom), sehingga tidak membatasi apa pun.
/// </summary>
public sealed class PayrollSalaryRangeRuleService
{
    private const int MaxListedStructureCodes = 10;
    private static readonly CultureInfo Indonesian = CultureInfo.GetCultureInfo("id-ID");

    private readonly ApplicationDbContext _dbContext;

    public PayrollSalaryRangeRuleService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// SR-01..SR-03: batas bawah, batas atas, dan gaji bawaan struktur terhadap rentang grade.
    /// Keberadaan dan status aktif grade sudah diperiksa validasi controller sebelum method ini dipanggil.
    /// </summary>
    public async Task<PayrollSalaryRangeCheck> ValidateStructureAsync(
        Guid salaryGradeId,
        decimal defaultBaseSalary,
        decimal? minimumBaseSalary,
        decimal? maximumBaseSalary,
        CancellationToken cancellationToken)
    {
        var grade = await _dbContext.MstSalaryGrades
            .AsNoTracking()
            .Where(x => x.Id == salaryGradeId && !x.IsDelete)
            .Select(x => new { x.SalaryGradeCode, x.MinimumSalary, x.MaximumSalary })
            .FirstOrDefaultAsync(cancellationToken);

        if (grade == null || !HasRange(grade.MaximumSalary))
            return PayrollSalaryRangeCheck.Valid;

        if (minimumBaseSalary.HasValue && minimumBaseSalary.Value < grade.MinimumSalary)
        {
            return PayrollSalaryRangeCheck.Invalid(400,
                $"Batas bawah gaji struktur ({Rupiah(minimumBaseSalary.Value)}) lebih rendah dari batas bawah grade " +
                $"{grade.SalaryGradeCode} ({Rupiah(grade.MinimumSalary)}).");
        }

        if (maximumBaseSalary.HasValue && maximumBaseSalary.Value > grade.MaximumSalary)
        {
            return PayrollSalaryRangeCheck.Invalid(400,
                $"Batas atas gaji struktur ({Rupiah(maximumBaseSalary.Value)}) melebihi batas atas grade " +
                $"{grade.SalaryGradeCode} ({Rupiah(grade.MaximumSalary)}).");
        }

        var lower = Math.Max(grade.MinimumSalary, minimumBaseSalary ?? grade.MinimumSalary);
        var upper = Math.Min(grade.MaximumSalary, maximumBaseSalary ?? grade.MaximumSalary);

        if (defaultBaseSalary < lower || defaultBaseSalary > upper)
        {
            return PayrollSalaryRangeCheck.Invalid(400,
                $"Gaji pokok bawaan {Rupiah(defaultBaseSalary)} berada di luar rentang {Rupiah(lower)} – {Rupiah(upper)}.");
        }

        return PayrollSalaryRangeCheck.Valid;
    }

    /// <summary>
    /// SR-04: rentang baru grade tidak boleh membuat struktur aktif (belum dihapus) keluar batas.
    /// </summary>
    public async Task<PayrollSalaryRangeCheck> ValidateGradeChangeAsync(
        Guid salaryGradeId,
        decimal newMinimumSalary,
        decimal newMaximumSalary,
        CancellationToken cancellationToken)
    {
        if (!HasRange(newMaximumSalary))
            return PayrollSalaryRangeCheck.Valid;

        var outOfRangeCodes = await _dbContext.MstSalaryStructures
            .AsNoTracking()
            .Where(x => x.SalaryGradeId == salaryGradeId && x.IsActive && !x.IsDelete)
            .Where(x =>
                (x.MinimumBaseSalary.HasValue && x.MinimumBaseSalary.Value < newMinimumSalary) ||
                (x.MaximumBaseSalary.HasValue && x.MaximumBaseSalary.Value > newMaximumSalary) ||
                x.DefaultBaseSalary < newMinimumSalary ||
                x.DefaultBaseSalary > newMaximumSalary)
            .OrderBy(x => x.SalaryStructureCode)
            .Select(x => x.SalaryStructureCode)
            .ToListAsync(cancellationToken);

        if (outOfRangeCodes.Count == 0)
            return PayrollSalaryRangeCheck.Valid;

        var listed = string.Join(", ", outOfRangeCodes.Take(MaxListedStructureCodes));
        if (outOfRangeCodes.Count > MaxListedStructureCodes)
            listed += $", dan {outOfRangeCodes.Count - MaxListedStructureCodes} lainnya";

        return PayrollSalaryRangeCheck.Invalid(409,
            $"Rentang baru membuat {outOfRangeCodes.Count} struktur aktif keluar batas: {listed}. Sesuaikan struktur dulu.");
    }

    private static bool HasRange(decimal maximumSalary) => maximumSalary > 0m;

    private static string Rupiah(decimal amount) => "Rp " + amount.ToString("N0", Indonesian);
}

/// <summary>Hasil pemeriksaan batas gaji. <see cref="StatusCode"/> 400 untuk SR-01..SR-03, 409 untuk SR-04.</summary>
public sealed record PayrollSalaryRangeCheck(bool IsValid, int StatusCode, string? Message)
{
    public static readonly PayrollSalaryRangeCheck Valid = new(true, 200, null);

    public static PayrollSalaryRangeCheck Invalid(int statusCode, string message) => new(false, statusCode, message);
}
