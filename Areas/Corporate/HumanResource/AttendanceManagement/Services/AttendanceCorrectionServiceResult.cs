using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendanceCorrectionServiceResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static AttendanceCorrectionServiceResult<T> Ok(
            T data,
            string message = "Berhasil.",
            int statusCode = StatusCodes.Status200OK)
        {
            return new AttendanceCorrectionServiceResult<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static AttendanceCorrectionServiceResult<T> Fail(
            int statusCode,
            string message)
        {
            return new AttendanceCorrectionServiceResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = default
            };
        }
    }
}
