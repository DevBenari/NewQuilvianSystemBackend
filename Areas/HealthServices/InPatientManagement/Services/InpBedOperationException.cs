namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public sealed class InpBedOperationException(int statusCode, string code, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Code { get; } = code;
}

