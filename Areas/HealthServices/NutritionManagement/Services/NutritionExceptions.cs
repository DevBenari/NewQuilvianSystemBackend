namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;

/// <summary>Benturan data atau transisi status yang tidak sah; dipetakan ke `409`.</summary>
public sealed class NutritionConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>Pengguna tidak berwenang atas tindakan ini; dipetakan ke `403`.</summary>
public sealed class NutritionForbiddenException(string message) : Exception(message);

/// <summary>Prasyarat aturan gizi belum terpenuhi; dipetakan ke `422`.</summary>
public sealed class NutritionUnprocessableException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
