namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Services;

/// <summary>
/// Keadaan berlaku dokumen legalitas yang <c>VERIFIED</c> (PBJ-VD-STATE bagian 2; PBJ-ASM-002), dihitung saat dibaca.
/// </summary>
/// <remarks>
/// Contoh dengan batas peringatan 30 hari dan hari ini 17-11-2026: berlaku sampai 16-11-2026 → <c>EXPIRED</c>;
/// sampai 07-12-2026 (20 hari lagi) → <c>EXPIRING</c>; sampai 30-06-2027 → <c>VALID</c>; jenis tanpa tanggal berlaku →
/// <c>VALID</c>. Dokumen terverifikasi yang sudah digantikan dokumen terverifikasi lebih baru dengan jenis yang sama →
/// <c>REPLACED</c>, supaya izin lama yang kedaluwarsa tidak dihitung lagi.
/// </remarks>
public static class ProcurementVendorDocumentValidity
{
    public const string Valid = "VALID";
    public const string Expiring = "EXPIRING";
    public const string Expired = "EXPIRED";
    public const string Replaced = "REPLACED";

    public const string ConfigurationKey = "Procurement:Vendor:ExpiryWarningDays";
    public const int DefaultWarningDays = 30;

    public static readonly IReadOnlyList<string> All = [Valid, Expiring, Expired, Replaced];

    public static int WarningDays(IConfiguration configuration) =>
        Math.Max(0, configuration.GetValue<int?>(ConfigurationKey) ?? DefaultWarningDays);

    public static string Resolve(DateOnly? expiryDate, DateOnly today, int warningDays)
    {
        if (!expiryDate.HasValue) return Valid;
        if (expiryDate.Value < today) return Expired;
        return expiryDate.Value <= today.AddDays(warningDays) ? Expiring : Valid;
    }

    public static string Label(string? value) => value switch
    {
        Valid => "Berlaku",
        Expiring => "Segera kedaluwarsa",
        Expired => "Kedaluwarsa",
        Replaced => "Digantikan",
        _ => string.Empty
    };
}
