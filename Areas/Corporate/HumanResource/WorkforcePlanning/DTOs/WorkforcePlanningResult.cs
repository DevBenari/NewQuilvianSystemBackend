namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    /// <summary>
    /// Wrapper hasil operasi layanan Workforce Planning untuk standardisasi status dan respons.
    /// </summary>
    /// <typeparam name="T">Tipe data kembalian</typeparam>
    public sealed class WorkforcePlanningResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static WorkforcePlanningResult<T> Ok(T data, string message = "Success", int statusCode = 200) =>
            new() { Success = true, StatusCode = statusCode, Data = data, Message = message };

        public static WorkforcePlanningResult<T> Fail(int statusCode, string message) =>
            new() { Success = false, StatusCode = statusCode, Message = message };

        public static WorkforcePlanningResult<T> Fail(string message, int statusCode = 400) =>
            new() { Success = false, StatusCode = statusCode, Message = message };
    }
}
