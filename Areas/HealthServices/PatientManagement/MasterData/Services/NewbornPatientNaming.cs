namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;

/// <summary>
/// Satu-satunya aturan pembersih nama ibu untuk pasien bayi baru lahir. Dipakai pendaftaran bayi
/// (nama otomatis "By. Ny. [ibu]") dan gelang bayi rawat inap, supaya keduanya tidak menyimpang
/// lagi (ISSUE-EPS-040, gelang "BY. NY. NY.").
/// </summary>
/// <remarks>
/// Contoh: "Ny. Siti Aminah", "NYONYA Siti Aminah", dan "Siti Aminah" sama-sama menjadi
/// "Siti Aminah".
/// </remarks>
public static class NewbornPatientNaming
{
    private static readonly string[] MotherSalutations = ["Ny. ", "Ny ", "Nyonya ", "Ibu "];

    public static string NormalizeMotherName(string value)
    {
        var normalized = value.Trim();
        foreach (var prefix in MotherSalutations)
        {
            if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return normalized[prefix.Length..].Trim();
        }
        return normalized;
    }
}
