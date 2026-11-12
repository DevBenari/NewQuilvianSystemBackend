namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Services;

/// <summary>Isian tidak memenuhi aturan <c>PBJ-VAL-*</c> (HTTP 422).</summary>
public sealed class ProcurementRequestValidationException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>Bentrok status atau data basi (HTTP 409), mis. PBJ-VAL-090/093.</summary>
public sealed class ProcurementRequestConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>Kewenangan yang melekat pada data tidak terpenuhi (HTTP 403), mis. bukan pembuat PR.</summary>
public sealed class ProcurementRequestForbiddenException(string message) : Exception(message);
