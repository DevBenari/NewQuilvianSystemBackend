using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public class WorkflowServiceResult<T>
    {
        public bool Success { get; private set; }

        public int StatusCode { get; private set; }

        public string Message { get; private set; } = string.Empty;

        public T? Data { get; private set; }

        public static WorkflowServiceResult<T> Ok(
            T data,
            string message,
            int statusCode = StatusCodes.Status200OK)
        {
            return new WorkflowServiceResult<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static WorkflowServiceResult<T> Fail(
            int statusCode,
            string message)
        {
            return new WorkflowServiceResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = default
            };
        }
    }
}
