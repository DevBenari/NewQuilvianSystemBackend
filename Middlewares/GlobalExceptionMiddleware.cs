using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Net;
using System.Text.Json;

namespace QuilvianSystemBackend.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            LoggerService loggerService)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // Permintaan dibatalkan oleh klien (misalnya browser berganti halaman atau memutus koneksi).
                // Keadaan ini bukan kesalahan internal server sehingga tidak dicatat sebagai unhandled crash 500.
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 499; // Client Closed Request
                }
            }
            catch (Exception ex)
            {
                await loggerService.ErrorAsync(
                    "Global",
                    "UnhandledException",
                    "Terjadi error tidak terduga pada aplikasi.",
                    ex,
                    new
                    {
                        Path = context.Request.Path.ToString(),
                        Method = context.Request.Method
                    }
                );

                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Pesan untuk pengguna tanpa detail teknis (MSL-11, 18 Nov 2026). Penolakan basis data karena aturan data
        /// (unique, foreign key, check, not null — SQLSTATE kelas 23) dan bentrok penyimpanan bersamaan dijawab 409,
        /// karena penyebabnya isian atau keadaan data, bukan kerusakan server. Detail lengkap tetap hanya di log.
        /// </summary>
        internal static (int StatusCode, string Message) Describe(Exception ex)
        {
            var postgres = FindPostgresException(ex);
            if (ex is DbUpdateConcurrencyException || postgres?.SqlState is "40001" or "40P01")
            {
                return (StatusCodes.Status409Conflict,
                    "Data ini baru saja diubah oleh pengguna lain. Muat ulang halaman, periksa perubahan terbaru, lalu simpan kembali.");
            }

            if (postgres?.SqlState is { } state && state.StartsWith("23", StringComparison.Ordinal))
            {
                return state switch
                {
                    "23505" => (StatusCodes.Status409Conflict,
                        "Data tidak dapat disimpan karena data yang sama sudah tercatat. Periksa kembali isian Anda."),
                    "23503" => (StatusCodes.Status409Conflict,
                        "Data tidak dapat disimpan karena merujuk data lain yang tidak ada atau sudah dihapus. Muat ulang halaman lalu pilih kembali datanya."),
                    _ => (StatusCodes.Status409Conflict,
                        "Data tidak dapat disimpan karena tidak memenuhi aturan penyimpanan. Periksa kembali isian Anda; bila masalah berlanjut, hubungi admin sistem.")
                };
            }

            if (ex is DbUpdateException)
            {
                return (StatusCodes.Status500InternalServerError,
                    "Data belum dapat disimpan karena gangguan pada basis data. Silakan coba lagi; bila masalah berlanjut, hubungi admin sistem.");
            }

            return (StatusCodes.Status500InternalServerError, "Terjadi kesalahan pada server.");
        }

        private static PostgresException? FindPostgresException(Exception? ex)
        {
            while (ex != null)
            {
                if (ex is PostgresException postgres)
                {
                    return postgres;
                }

                ex = ex.InnerException;
            }

            return null;
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            var (statusCode, message) = Describe(ex);

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Fail(statusCode, message);

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
