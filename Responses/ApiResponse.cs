namespace QuilvianSystemBackend.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }

        public int StatusCode { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public object? Errors { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public static ApiResponse<T> Ok(T? data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = message,
                Data = data,
                Errors = null,
                Timestamp = DateTime.Now
            };
        }

        public static ApiResponse<T> Fail(int statusCode, string message, object? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = default,
                Errors = errors,
                Timestamp = DateTime.Now
            };
        }
    }
}
