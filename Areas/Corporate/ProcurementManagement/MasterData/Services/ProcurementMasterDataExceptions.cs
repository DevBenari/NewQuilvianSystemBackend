using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Services;

/// <summary>
/// Isian tidak memenuhi aturan (HTTP 422). <see cref="Code"/> diisi bila aturannya punya kode di matriks validasi,
/// misalnya <c>PBJ-VAL-113</c> untuk bobot kriteria di luar 0–100 (BE-PBJ-VD-002); klien membacanya dari <c>errors.code</c>.
/// </summary>
public sealed class ProcurementMasterValidationException(string message, string? code = null) : Exception(message)
{
    public string? Code { get; } = code;
}

/// <summary>
/// Bentuk isian tidak sah menurut kontrak (HTTP 400), dipakai bila kontrak menetapkan 400, bukan 422. Contoh: dokumen wajib
/// paket mengisi metode <b>dan</b> jenis pengadaan sekaligus, atau tidak satu pun (PBJ-PS-API 1.0; BE-PBJ-PS-002).
/// </summary>
public sealed class ProcurementMasterBadRequestException(string message) : Exception(message);

/// <summary>Bentrok dengan data lain, misalnya kode sudah dipakai (HTTP 409).</summary>
public sealed class ProcurementMasterConflictException(string message) : Exception(message);

/// <summary>
/// Data masih dipakai data lain sehingga tidak dapat dihapus (HTTP 400, standar master data). Respons membawa
/// <c>errors.code</c> <c>PBJ-VAL-071</c> agar klien dapat mengenali sebabnya (PBJ-DEC-041).
/// </summary>
public sealed class ProcurementMasterInUseException(string message) : Exception(message)
{
    public string Code { get; } = "PBJ-VAL-071";
}

internal static class ProcurementMasterDataSupport
{
    public static string Required(string? value, string label, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ProcurementMasterValidationException($"{label} wajib diisi.");
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ProcurementMasterValidationException($"{label} maksimal {maxLength} karakter.");
        return trimmed;
    }

    public static string? Optional(string? value, string label, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ProcurementMasterValidationException($"{label} maksimal {maxLength} karakter.");
        return trimmed;
    }

    public static bool IsUniqueViolation(DbUpdateException exception) =>
        (exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation;

    public static int TotalPage(int total, int pageSize) =>
        pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
}
