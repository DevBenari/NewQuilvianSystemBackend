using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeavePayrollIntegrationServiceResult<T>
    {
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }

        public static LeavePayrollIntegrationServiceResult<T> Ok(
            T data,
            string message,
            int statusCode = StatusCodes.Status200OK)
        {
            return new LeavePayrollIntegrationServiceResult<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static LeavePayrollIntegrationServiceResult<T> Fail(
            int statusCode,
            string message)
        {
            return new LeavePayrollIntegrationServiceResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message
            };
        }
    }
}
