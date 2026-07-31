namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class AttendanceScheduleResolverServiceResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static AttendanceScheduleResolverServiceResult<T> Ok(
            T data,
            string message = "Berhasil.") =>
            new()
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = message,
                Data = data
            };

        public static AttendanceScheduleResolverServiceResult<T> Fail(
            int statusCode,
            string message) =>
            new()
            {
                Success = false,
                StatusCode = statusCode,
                Message = message
            };
    }
}
