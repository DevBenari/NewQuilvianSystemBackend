namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

public sealed class OperatingRoomConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

public sealed class OperatingRoomForbiddenException(string message) : Exception(message);

/// <summary>
/// Aturan klinis atau prasyarat modul belum terpenuhi; dipetakan ke `422` sesuai `opr-api-v1`.
/// </summary>
public sealed class OperatingRoomUnprocessableException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
