using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeRealizationServiceResult<T>
    {
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }

        public static OvertimeRealizationServiceResult<T> Ok(
            T data,
            string message,
            int statusCode = StatusCodes.Status200OK) => new()
        {
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Data = data
        };

        public static OvertimeRealizationServiceResult<T> Fail(
            int statusCode,
            string message) => new()
        {
            Success = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}
