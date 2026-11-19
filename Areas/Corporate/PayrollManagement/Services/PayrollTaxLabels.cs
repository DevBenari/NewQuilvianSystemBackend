using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Label pajak pada slip dan komponen payroll yang diturunkan dari hasil hitung sebenarnya (jenis PPh, skema, cara hitung, dan versi
/// kebijakan yang dipakai), bukan label tertanam. Run tanpa snapshot pajak memakai label umum tanpa klaim cara hitung.
/// </summary>
public static class PayrollTaxLabels
{
    public const string Generic = "Pajak Penghasilan";

    private static string? DescribeMethod(string? method) => method switch
    {
        "TER" => "TER",
        "ANNUAL_PASAL17" => "perhitungan tahunan Pasal 17",
        "NON_EMPLOYEE" => "penerima bukan pegawai",
        "PPH26_FLAT" => "tarif PPh Pasal 26",
        _ => null
    };

    private static string? ReadMethod(IReadOnlyDictionary<string, object?>? detail)
    {
        if (detail is null || !detail.TryGetValue("method", out var value) || value is null) return null;
        return value is JsonElement element
            ? element.ValueKind == JsonValueKind.String ? element.GetString() : null
            : value.ToString();
    }

    /// <summary>Nama komponen/baris slip pajak, mis. "Pajak Penghasilan (PPh Pasal 21, TER, kebijakan PMK168-2024.1)".</summary>
    public static string Build(
        string? article, string? mode, string source, string? policyVersion, IReadOnlyDictionary<string, object?>? detail)
    {
        var articleLabel = article == TaxArticles.Pph26 ? "PPh Pasal 26" : "PPh Pasal 21";

        string? basis;
        if (source == PayrollTaxInsuranceSources.LegacyProfileAmount)
        {
            basis = "tetapan lama profil payroll";
        }
        else if (mode == TaxCalculationModes.AutoRegulatory)
        {
            var methodLabel = DescribeMethod(ReadMethod(detail));
            basis = string.IsNullOrWhiteSpace(policyVersion)
                ? methodLabel
                : methodLabel is null ? $"kebijakan {policyVersion}" : $"{methodLabel}, kebijakan {policyVersion}";
        }
        else if (mode == TaxCalculationModes.ApprovedManualAmount)
        {
            basis = "nominal manual disetujui";
        }
        else
        {
            basis = null;
        }

        return basis is null ? $"{Generic} ({articleLabel})" : $"{Generic} ({articleLabel}, {basis})";
    }

    /// <summary>Label dari snapshot pajak penerima; umum bila snapshot tidak ada (tanpa klaim cara hitung atau peraturan).</summary>
    public static string BuildFromSnapshot(PayrollTaxSnapshot? snapshot) =>
        snapshot is null
            ? Generic
            : Build(snapshot.Article, snapshot.CalculationMode, snapshot.Source, snapshot.PolicyVersion, snapshot.Detail);
}
