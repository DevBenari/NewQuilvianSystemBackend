using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public class SchedulingRequestServiceResult<T>
    {
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }

        public static SchedulingRequestServiceResult<T> Ok(
            T? data,
            string message,
            int statusCode = StatusCodes.Status200OK)
        {
            return new SchedulingRequestServiceResult<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static SchedulingRequestServiceResult<T> Fail(
            int statusCode,
            string message)
        {
            return new SchedulingRequestServiceResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message
            };
        }
    }
}
