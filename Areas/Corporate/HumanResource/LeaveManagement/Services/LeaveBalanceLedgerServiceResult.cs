namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveBalanceLedgerServiceResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static LeaveBalanceLedgerServiceResult<T> Ok(T data, string message = "Sukses") =>
            new() { Success = true, StatusCode = 200, Message = message, Data = data };

        public static LeaveBalanceLedgerServiceResult<T> Fail(int statusCode, string message) =>
            new() { Success = false, StatusCode = statusCode, Message = message, Data = default };
    }
}
