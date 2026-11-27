namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Services;

// =====================================================================================
// Pengecualian sub-modul Pemilihan Penyedia (BE-PBJ-PS-005; PBJ-PS-API 1.0 tabel kode respons). Controller Sourcing
// memetakannya ke kode HTTP: 400 isian salah, 409 status/data basi/bentrok, 422 aturan bisnis. Kode errors.code
// mengikuti PBJ-PS-VAL 1.0.
// =====================================================================================

/// <summary>Aturan bisnis menolak (422). Contoh: baris PR berjenis Jasa dimasukkan ke paket Barang → <c>PBJ-VAL-122</c>.</summary>
public sealed class ProcurementSourcingValidationException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>
/// Status tidak mengizinkan, data basi, atau bentrok keunikan (409). Contoh: baris PR yang sudah masuk PKT-000012
/// dimasukkan ke paket kedua → <c>PBJ-VAL-121</c>.
/// </summary>
public sealed class ProcurementSourcingConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>Isian tidak lengkap atau rujukannya tidak ada (400). Contoh: <c>packagePlanId</c> yang tidak dikenal.</summary>
public sealed class ProcurementSourcingBadRequestException(string message) : Exception(message);

/// <summary>
/// Kewenangan yang melekat pada data menolak (403). Contoh: pengguna tanpa <c>ProcurementPackage : ReadPrice</c> mengubah HPS
/// yang tidak boleh ia lihat (BE-PBJ-PS-007).
/// </summary>
public sealed class ProcurementSourcingForbiddenException(string message) : Exception(message);
