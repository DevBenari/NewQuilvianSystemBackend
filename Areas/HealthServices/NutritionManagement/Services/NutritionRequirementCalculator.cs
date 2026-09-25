using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;

/// <summary>
/// Masukan yang tersedia bagi rumus kebutuhan nutrisi.
/// </summary>
/// <remarks>
/// Seluruhnya boleh kosong. Rumus yang menuntut masukan yang tidak tersedia wajib menolak
/// menghitung lewat <see cref="INutritionRequirementFormula.CanCalculate"/>, bukan menebak
/// nilai penggantinya.
/// </remarks>
public sealed record NutritionCalculationInput(
    decimal? WeightKg,
    decimal? HeightCm,
    int? AgeYears,
    string? Gender,
    decimal? ActivityFactor,
    decimal? StressFactor);

/// <summary>Hasil satu rumus untuk satu parameter nutrisi.</summary>
public sealed record NutritionCalculationOutput(string ParameterCode, decimal Value);

/// <summary>
/// Satu rumus kebutuhan nutrisi yang terdaftar di kode.
/// </summary>
/// <remarks>
/// <para>
/// Implementasi dihubungkan ke baris <see cref="GziNutritionFormula"/> lewat
/// <see cref="ImplementationKey"/>. Rumus tidak pernah dipilih sendiri oleh kode: yang memilih
/// adalah baris master yang didaftarkan admin, beserta sumber resminya.
/// </para>
/// <para>
/// <b>Pada V1 tidak ada satu pun implementasi.</b> Rumus dan faktornya belum diserahkan pemilik
/// proses (`GIZ-OQ-007`), dan rumus tidak diambil dari internet maupun dikarang. Satu rumus yang
/// keliru berdampak pada seluruh pasien sekaligus, dan kekeliruannya sulit terlihat karena
/// hasilnya tetap tampak masuk akal.
/// </para>
/// </remarks>
public interface INutritionRequirementFormula
{
    /// <summary>Kunci yang dicocokkan dengan <c>GziNutritionFormula.ImplementationKey</c>.</summary>
    string ImplementationKey { get; }

    /// <summary>Menolak secara terang-terangan bila masukannya tidak lengkap.</summary>
    bool CanCalculate(NutritionCalculationInput input);

    IReadOnlyCollection<NutritionCalculationOutput> Calculate(NutritionCalculationInput input);
}

/// <summary>
/// Mencari implementasi rumus berdasarkan kunci pada baris master.
/// </summary>
/// <remarks>
/// Bila tidak ada implementasi yang cocok, pencarian mengembalikan <c>null</c> dan pemanggilnya
/// wajib membiarkan nilai kalkulasi <b>kosong</b>. Mengisinya dengan nol atau dengan nilai
/// bawaan akan tampak sebagai angka hasil hitungan padahal bukan.
/// </remarks>
public sealed class NutritionRequirementCalculator
{
    private readonly IReadOnlyDictionary<string, INutritionRequirementFormula> _formulas;

    public NutritionRequirementCalculator(IEnumerable<INutritionRequirementFormula> formulas)
    {
        _formulas = formulas
            .GroupBy(x => x.ImplementationKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Benar bila ada sedikitnya satu rumus terdaftar di kode.</summary>
    public bool HasAnyFormula => _formulas.Count > 0;

    public INutritionRequirementFormula? Resolve(string? implementationKey)
        => string.IsNullOrWhiteSpace(implementationKey)
            ? null
            : _formulas.TryGetValue(implementationKey, out var formula) ? formula : null;

    /// <summary>
    /// Menghitung nilai awal untuk rumus yang dipilih. Daftar kosong berarti tidak ada
    /// kalkulasi yang dapat dipertanggungjawabkan, dan nilai kalkulasi dibiarkan kosong.
    /// </summary>
    public IReadOnlyCollection<NutritionCalculationOutput> Calculate(
        GziNutritionFormula? formula,
        NutritionCalculationInput input)
    {
        if (formula is null || !formula.IsActive) return Array.Empty<NutritionCalculationOutput>();

        var implementation = Resolve(formula.ImplementationKey);
        if (implementation is null || !implementation.CanCalculate(input))
            return Array.Empty<NutritionCalculationOutput>();

        return implementation.Calculate(input);
    }
}
