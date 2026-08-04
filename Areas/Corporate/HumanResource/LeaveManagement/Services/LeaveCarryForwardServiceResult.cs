using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveCarryForwardServiceResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static LeaveCarryForwardServiceResult<T> Ok(
            T? data,
            string message,
            int statusCode = StatusCodes.Status200OK)
        {
            return new LeaveCarryForwardServiceResult<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static LeaveCarryForwardServiceResult<T> Fail(
            int statusCode,
            string message)
        {
            return new LeaveCarryForwardServiceResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message
            };
        }
    }
}
