using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Controllers
{
    internal static class NvwHttp
    {
        public const string LogCategory = "HealthServices.NurseViewManagement";

        public static Guid ActorId(ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("user_id") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        public static IActionResult Map<T>(ControllerBase controller, NvwResult<T> result, string successMessage)
        {
            object? errors = result.Code == null ? null : new { Code = result.Code };

            return result.Kind switch
            {
                NvwResultKind.Success => controller.Ok(ApiResponse<T>.Ok(result.Value, successMessage)),

                NvwResultKind.Created => controller.StatusCode(StatusCodes.Status201Created, new ApiResponse<T>
                {
                    Success = true,
                    StatusCode = StatusCodes.Status201Created,
                    Message = successMessage,
                    Data = result.Value
                }),

                NvwResultKind.NotFound => controller.NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, result.Message ?? "Data tidak ditemukan.", errors)),

                NvwResultKind.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                    StatusCodes.Status403Forbidden, result.Message ?? "Anda tidak memiliki hak akses untuk tindakan ini.", errors)),

                NvwResultKind.Conflict => controller.Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, result.Message ?? "Terjadi konflik data.", errors)),

                NvwResultKind.BusinessRule => controller.UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, result.Message ?? "Syarat tindakan belum terpenuhi.", errors)),

                _ => controller.BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, result.Message ?? "Permintaan tidak valid.", errors))
            };
        }

        public static async Task<IActionResult> RespondAsync<T>(
            ControllerBase controller,
            NvwResult<T> result,
            string successMessage,
            LoggerService logger,
            string resource,
            string action,
            Func<T, object>? payload = null)
        {
            if (result.IsSuccess && result.Value != null)
            {
                await logger.InfoAsync(
                    LogCategory,
                    $"{resource}.{action}",
                    successMessage,
                    new
                    {
                        Controller = resource,
                        Action = action,
                        Actor = ActorId(controller.User),
                        Data = payload?.Invoke(result.Value)
                    });
            }

            return Map(controller, result, successMessage);
        }
    }
}
