using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services
{
    /// <summary>
    /// Hasil satu operasi service Accounting beserta kode status yang pantas dikembalikan
    /// controller. Mengikuti pola <c>LeaveAccrualServiceResult</c> dan
    /// <c>AttendancePeriodSchedulerServiceResult</c> di <c>Areas/Corporate/</c>.
    /// </summary>
    /// <remarks>
    /// Dipakai bersama seluruh service Accounting, bukan hanya daftar akun, supaya pemetaan
    /// kode status tidak ditulis ulang berbeda-beda di tiap controller.
    /// </remarks>
    public class AccountingServiceResult<T>
    {
        public bool Success { get; set; }

        public int StatusCode { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public static AccountingServiceResult<T> Ok(
            T? data,
            string message,
            int statusCode = StatusCodes.Status200OK)
        {
            return new AccountingServiceResult<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static AccountingServiceResult<T> Fail(int statusCode, string message)
        {
            return new AccountingServiceResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message
            };
        }
    }
}
