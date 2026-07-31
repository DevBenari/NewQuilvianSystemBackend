using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendanceProcessingServiceResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static AttendanceProcessingServiceResult<T> Ok(
            T data,
            string message,
            int statusCode = StatusCodes.Status200OK)
        {
            return new AttendanceProcessingServiceResult<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static AttendanceProcessingServiceResult<T> Fail(
            int statusCode,
            string message)
        {
            return new AttendanceProcessingServiceResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = default
            };
        }
    }
}
