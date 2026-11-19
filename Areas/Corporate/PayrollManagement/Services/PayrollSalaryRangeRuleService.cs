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

    /// <summary>
    /// Menghitung rentang gaji efektif gabungan antara Grade Gaji dan Struktur Gaji.
    /// EffectiveMinimum = MAX(Grade.MinimumSalary, Structure.MinimumBaseSalary)
    /// EffectiveMaximum = MIN(Grade.MaximumSalary, Structure.MaximumBaseSalary)
    /// </summary>
    public static EffectiveSalaryRange CalculateEffectiveRange(
        decimal gradeMin,
        decimal gradeMax,
        decimal? structMin,
        decimal? structMax)
    {
        var lower = gradeMin;
        if (structMin.HasValue && structMin.Value > 0m)
        {
            lower = Math.Max(lower, structMin.Value);
        }

        decimal upper = 0m;
        var hasGradeMax = gradeMax > 0m;
        var hasStructMax = structMax.HasValue && structMax.Value > 0m;

        if (hasGradeMax && hasStructMax)
        {
            upper = Math.Min(gradeMax, structMax.Value);
        }
        else if (hasGradeMax)
        {
            upper = gradeMax;
        }
        else if (hasStructMax)
        {
            upper = structMax.Value;
        }

        var hasRange = lower > 0m || upper > 0m;
        string rangeText;

        if (lower > 0m && upper > 0m)
        {
            rangeText = lower == upper
                ? $"{Rupiah(lower)} (Pasti)"
                : $"{Rupiah(lower)} – {Rupiah(upper)}";
        }
        else if (lower > 0m && upper <= 0m)
        {
            rangeText = $"{Rupiah(lower)} – Belum Dikonfigurasi";
        }
        else if (lower <= 0m && upper > 0m)
        {
            rangeText = $"Hingga {Rupiah(upper)}";
        }
        else
        {
            rangeText = "Belum Dikonfigurasi";
        }

        return new EffectiveSalaryRange(
            GradeMinimumSalary: gradeMin,
            GradeMaximumSalary: gradeMax,
            StructureMinimumSalary: structMin,
            StructureMaximumSalary: structMax,
            EffectiveMinimumSalary: lower,
            EffectiveMaximumSalary: upper,
            HasRange: hasRange,
            SalaryRangeText: rangeText);
    }

    /// <summary>
    /// Validasi penetapan gaji karyawan/dokter: memeriksa keaktifan struktur & grade, kesesuaian relasi
    /// struktur terhadap grade, kesesuaian mata uang, dan memastikan nominal gaji pokok berada dalam rentang efektif grade dan struktur.
    /// </summary>
    public Task<PayrollSalaryRangeCheck> ValidateSalaryAssignmentAsync(
        Guid salaryStructureId,
        Guid salaryGradeId,
        decimal baseSalary,
        CancellationToken cancellationToken) =>
        ValidateSalaryAssignmentAsync(salaryStructureId, salaryGradeId, baseSalary, null, cancellationToken);

    public async Task<PayrollSalaryRangeCheck> ValidateSalaryAssignmentAsync(
        Guid salaryStructureId,
        Guid salaryGradeId,
        decimal baseSalary,
        string? currencyCode,
        CancellationToken cancellationToken)
    {
        if (baseSalary <= 0m)
        {
            return PayrollSalaryRangeCheck.Invalid(400, "Gaji pokok harus lebih besar dari 0.");
        }

        var structure = await _dbContext.MstSalaryStructures
            .AsNoTracking()
            .Where(x => x.Id == salaryStructureId && !x.IsDelete)
            .Select(x => new
            {
                x.SalaryStructureCode,
                x.SalaryStructureName,
                x.SalaryGradeId,
                x.CurrencyCode,
                x.MinimumBaseSalary,
                x.MaximumBaseSalary,
                x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (structure == null || !structure.IsActive)
        {
            return PayrollSalaryRangeCheck.Invalid(400, "Struktur Gaji tidak ditemukan atau tidak aktif.");
        }

        if (structure.SalaryGradeId != salaryGradeId)
        {
            return PayrollSalaryRangeCheck.Invalid(400, "Struktur Gaji yang dipilih tidak sesuai dengan Grade Gaji yang dipilih.");
        }

        var grade = await _dbContext.MstSalaryGrades
            .AsNoTracking()
            .Where(x => x.Id == salaryGradeId && !x.IsDelete)
            .Select(x => new
            {
                x.SalaryGradeCode,
                x.SalaryGradeName,
                x.CurrencyCode,
                x.MinimumSalary,
                x.MaximumSalary,
                x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (grade == null || !grade.IsActive)
        {
            return PayrollSalaryRangeCheck.Invalid(400, "Grade Gaji tidak ditemukan atau tidak aktif.");
        }

        if (!string.IsNullOrWhiteSpace(structure.CurrencyCode) &&
            !string.IsNullOrWhiteSpace(grade.CurrencyCode) &&
            !string.Equals(structure.CurrencyCode, grade.CurrencyCode, StringComparison.OrdinalIgnoreCase))
        {
            return PayrollSalaryRangeCheck.Invalid(400,
                $"Mata uang Struktur Gaji ({structure.CurrencyCode}) tidak sesuai dengan Grade Gaji ({grade.CurrencyCode}).");
        }

        if (!string.IsNullOrWhiteSpace(currencyCode))
        {
            if (!string.IsNullOrWhiteSpace(structure.CurrencyCode) &&
                !string.Equals(structure.CurrencyCode, currencyCode.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return PayrollSalaryRangeCheck.Invalid(400,
                    $"Mata uang ({currencyCode.Trim().ToUpperInvariant()}) tidak sesuai dengan Struktur Gaji ({structure.CurrencyCode}).");
            }

            if (!string.IsNullOrWhiteSpace(grade.CurrencyCode) &&
                !string.Equals(grade.CurrencyCode, currencyCode.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return PayrollSalaryRangeCheck.Invalid(400,
                    $"Mata uang ({currencyCode.Trim().ToUpperInvariant()}) tidak sesuai dengan Grade Gaji ({grade.CurrencyCode}).");
            }
        }

        var range = CalculateEffectiveRange(grade.MinimumSalary, grade.MaximumSalary, structure.MinimumBaseSalary, structure.MaximumBaseSalary);

        if (range.EffectiveMaximumSalary > 0m && range.EffectiveMaximumSalary < range.EffectiveMinimumSalary)
        {
            return PayrollSalaryRangeCheck.Invalid(400,
                $"Konfigurasi rentang gaji tidak valid: batas atas ({Rupiah(range.EffectiveMaximumSalary)}) lebih kecil dari batas bawah ({Rupiah(range.EffectiveMinimumSalary)}).");
        }

        if (range.EffectiveMinimumSalary > 0m && baseSalary < range.EffectiveMinimumSalary)
        {
            return PayrollSalaryRangeCheck.Invalid(400,
                $"Gaji pokok kurang dari batas minimum grade yang dipilih (tidak boleh lebih rendah dari batas minimum grade) ({Rupiah(range.EffectiveMinimumSalary)}). Gaji pokok harus berada dalam rentang {range.SalaryRangeText}.");
        }

        if (range.EffectiveMaximumSalary > 0m && baseSalary > range.EffectiveMaximumSalary)
        {
            return PayrollSalaryRangeCheck.Invalid(400,
                $"Gaji pokok melebihi batas maksimum grade ({Rupiah(range.EffectiveMaximumSalary)}). Gaji pokok harus berada dalam rentang {range.SalaryRangeText}.");
        }

        return PayrollSalaryRangeCheck.Valid;
    }

    public static string FormatRupiah(decimal amount) => Rupiah(amount);

    private static bool HasRange(decimal maximumSalary) => maximumSalary > 0m;

    private static string Rupiah(decimal amount) => "Rp " + amount.ToString("N0", Indonesian);
}

/// <summary>Rentang gaji efektif hasil kombinasi master Grade dan Struktur.</summary>
public sealed record EffectiveSalaryRange(
    decimal GradeMinimumSalary,
    decimal GradeMaximumSalary,
    decimal? StructureMinimumSalary,
    decimal? StructureMaximumSalary,
    decimal EffectiveMinimumSalary,
    decimal EffectiveMaximumSalary,
    bool HasRange,
    string SalaryRangeText);

/// <summary>Hasil pemeriksaan batas gaji. <see cref="StatusCode"/> 400 untuk SR-01..SR-03, 409 untuk SR-04.</summary>
public sealed record PayrollSalaryRangeCheck(bool IsValid, int StatusCode, string? Message)
{
    public static readonly PayrollSalaryRangeCheck Valid = new(true, 200, null);

    public static PayrollSalaryRangeCheck Invalid(int statusCode, string message) => new(false, statusCode, message);
}
