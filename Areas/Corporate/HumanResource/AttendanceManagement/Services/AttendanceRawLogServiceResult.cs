using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendanceRawLogServiceResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static AttendanceRawLogServiceResult<T> Ok(
            T data,
            string message,
            int statusCode = StatusCodes.Status200OK)
        {
            return new AttendanceRawLogServiceResult<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static AttendanceRawLogServiceResult<T> Fail(
            int statusCode,
            string message)
        {
            return new AttendanceRawLogServiceResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = default
            };
        }
    }
}
