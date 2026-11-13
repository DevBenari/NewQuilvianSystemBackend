using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Services;

/// <summary>Isian tidak memenuhi aturan (HTTP 422).</summary>
public sealed class ProcurementMasterValidationException(string message) : Exception(message);

/// <summary>Bentrok dengan data lain, misalnya kode sudah dipakai (HTTP 409).</summary>
public sealed class ProcurementMasterConflictException(string message) : Exception(message);

/// <summary>Data masih dipakai data lain sehingga tidak dapat dihapus (HTTP 400, standar master data).</summary>
public sealed class ProcurementMasterInUseException(string message) : Exception(message);

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
