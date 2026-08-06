using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimePlanningServiceResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static OvertimePlanningServiceResult<T> Ok(
            T data,
            string message,
            int statusCode = StatusCodes.Status200OK) => new()
        {
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Data = data
        };

        public static OvertimePlanningServiceResult<T> Fail(
            int statusCode,
            string message) => new()
        {
            Success = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}
