namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Services;

/// <summary>
/// Isian tidak memenuhi aturan <c>PBJ-VAL-*</c> (HTTP 422). <see cref="LineNo"/> menunjuk baris yang
/// bermasalah agar layar dapat menandainya, mis. PBJ-VAL-042 pada baris monitor.
/// </summary>
public sealed class ProcurementPlanningValidationException(string code, string message, int? lineNo = null) : Exception(message)
{
    public string Code { get; } = code;
    public int? LineNo { get; } = lineNo;
}

/// <summary>
/// Bentrok status, data basi, atau dokumen ganda (HTTP 409), mis. PBJ-VAL-040/041/060/090/091/093.
/// <see cref="ExistingId"/> menunjuk dokumen yang menghalangi, agar layar dapat menautkannya.
/// </summary>
public sealed class ProcurementPlanningConflictException(string code, string message, Guid? existingId = null) : Exception(message)
{
    public string Code { get; } = code;
    public Guid? ExistingId { get; } = existingId;
}

/// <summary>Kewenangan yang melekat pada data tidak terpenuhi (HTTP 403).</summary>
public sealed class ProcurementPlanningForbiddenException(string message) : Exception(message);
