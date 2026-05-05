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
