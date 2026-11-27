namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Services;

/// <summary>Isian tidak memenuhi aturan <c>PBJ-VAL-*</c> (HTTP 422), mis. <c>PBJ-VAL-100</c> kategori nonaktif.</summary>
public sealed class ProcurementVendorValidationException(string? code, string message) : Exception(message)
{
    public string? Code { get; } = code;
}

/// <summary>Bentrok status atau data basi (HTTP 409), mis. <c>PBJ-VAL-090</c> profil diubah orang lain.</summary>
public sealed class ProcurementVendorConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>
/// Kewenangan yang melekat pada data tidak terpenuhi (HTTP 403), mis. <c>PBJ-VAL-103</c> pengunggah memverifikasi
/// dokumennya sendiri. Bukan pengganti izin Akses Role; izin tetap dijaga <c>[AccessPermission]</c>.
/// </summary>
public sealed class ProcurementVendorForbiddenException(string? code, string message) : Exception(message)
{
    public string? Code { get; } = code;
}
