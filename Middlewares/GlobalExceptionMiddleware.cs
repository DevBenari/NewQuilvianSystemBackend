using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Features;
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
            catch (BpjsFeatureDisabledException ex)
            {
                // Operasi BPJS baru ditolak karena fitur BPJS nonaktif: penolakan aturan yang diharapkan (409), bukan galat sistem.
                // Jalur yang menangkapnya sendiri (controller) tetap berlaku; ini jaring pengaman agar tidak pernah menjadi 500.
                await WriteConflictAsync(context, ex.Message);
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

                await HandleExceptionAsync(context);
            }
        }

        private static async Task WriteConflictAsync(HttpContext context, string message)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(
                ApiResponse<object>.Fail(StatusCodes.Status409Conflict, message),
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(json);
        }

        private static async Task HandleExceptionAsync(HttpContext context)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Fail(
                StatusCodes.Status500InternalServerError,
                "Terjadi kesalahan pada server."
            );

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
