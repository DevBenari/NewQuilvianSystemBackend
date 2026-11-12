using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Controllers
{
    /// <summary>
    /// Penolong bersama controller Rehab Medik: memetakan <see cref="RhbResult{T}"/> menjadi kode
    /// HTTP kontrak dan mencatat logger aplikasi — pola <c>HmdHttp</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Pemetaan: <c>201</c> pembuatan, <c>400</c> isian salah, <c>403</c> tidak berwenang atas
    /// data ini, <c>404</c>, <c>409</c> bentrok status/versi/duplikasi, <c>422</c> aturan bisnis
    /// belum terpenuhi. Kode penolakan dikirim di <c>errors</c> sebagai <c>{ Code, Fields, Details }</c>.
    /// </para>
    /// <para>
    /// <b>Logger tidak pernah memuat kolom sensitif</b> (<c>permission-audit-matrix.md</c> bagian 6):
    /// yang dicatat hanya id, controller, aksi, aktor, dan payload ringkas tanpa isi klinis.
    /// </para>
    /// </remarks>
    internal static class RhbHttp
    {
        public const string LogCategory = "HealthServices.RehabilitationManagement";

        /// <summary>Pengguna terautentikasi. Tidak pernah diterima dari isian permintaan.</summary>
        public static Guid ActorId(ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("user_id") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        public static IActionResult Map<T>(ControllerBase controller, RhbResult<T> result, string successMessage)
        {
            object? errors = result.Code == null && result.Details == null && result.Fields.Count == 0
                ? null
                : new { Code = result.Code, Fields = result.Fields, Details = result.Details };

            return result.Kind switch
            {
                RhbResultKind.Success => controller.Ok(ApiResponse<T>.Ok(result.Value, successMessage)),

                RhbResultKind.Created => controller.StatusCode(StatusCodes.Status201Created, new ApiResponse<T>
                {
                    Success = true,
                    StatusCode = StatusCodes.Status201Created,
                    Message = successMessage,
                    Data = result.Value
                }),

                RhbResultKind.NotFound => controller.NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, result.Message ?? "Data tidak ditemukan.", errors)),

                RhbResultKind.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                    StatusCodes.Status403Forbidden, result.Message ?? "Anda tidak memiliki hak akses untuk tindakan ini.", errors)),

                RhbResultKind.Conflict => controller.Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, result.Message ?? "Terjadi konflik data.", errors)),

                RhbResultKind.BusinessRule => controller.UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, result.Message ?? "Syarat tindakan belum terpenuhi.", errors)),

                _ => controller.BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, result.Message ?? "Permintaan tidak valid.", errors))
            };
        }

        /// <summary>
        /// Memetakan hasil lalu, bila berhasil, mencatat satu baris logger aplikasi. Dipakai seluruh
        /// endpoint selain <c>GET</c>.
        /// </summary>
        public static async Task<IActionResult> RespondAsync<T>(
            ControllerBase controller,
            RhbResult<T> result,
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
