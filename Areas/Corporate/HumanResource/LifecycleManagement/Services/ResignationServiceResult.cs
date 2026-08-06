using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class ResignationServiceResult<T>
    {
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }

        public static ResignationServiceResult<T> Ok(
            T? data,
            string message,
            int statusCode = StatusCodes.Status200OK)
        {
            return new ResignationServiceResult<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static ResignationServiceResult<T> Fail(
            int statusCode,
            string message)
        {
            return new ResignationServiceResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message
            };
        }
    }
}
