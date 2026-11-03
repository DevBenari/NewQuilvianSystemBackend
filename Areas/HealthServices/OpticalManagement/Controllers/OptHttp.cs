using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Controllers
{
    /// <summary>
    /// Penolong bersama controller Optik: memetakan <see cref="OptResult{T}"/> menjadi kode HTTP kontrak
    /// (<c>OPT-API-v1</c>) dan mencatat logger aplikasi. Polanya sama dengan <c>McuHttp</c>.
    /// </summary>
    /// <remarks>
    /// <b>Isi log ditulis di teks pesan, bukan di payload.</b> <c>LoggerService</c> tidak menyerialisasi objek
    /// data, dan ruas <c>Id</c>/<c>Name</c> pada payload menimpa pelaku. Kalimat log tidak pernah memuat kolom
    /// bertanda Sensitif pada kamus data (<c>OPT-PERM-v1</c> bagian 3).
    /// </remarks>
    internal static class OptHttp
    {
        public const string LogCategory = "HealthServices.OpticalManagement";

        /// <summary>Pengguna terautentikasi. Tidak pernah diterima dari isian permintaan.</summary>
        public static Guid ActorId(ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("user_id") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        public static IActionResult Map<T>(ControllerBase controller, OptResult<T> result, string successMessage)
        {
            object? errors = result.Code == null ? null : new { result.Code };

            return result.Kind switch
            {
                OptResultKind.Success => controller.Ok(ApiResponse<T>.Ok(result.Value, successMessage)),

                OptResultKind.NotFound => controller.NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, result.Message ?? "Data tidak ditemukan.", errors)),

                OptResultKind.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                    StatusCodes.Status403Forbidden, result.Message ?? "Anda tidak memiliki hak akses untuk tindakan ini.", errors)),

                OptResultKind.Conflict => controller.Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, result.Message ?? "Keadaan data tidak mengizinkan tindakan ini.", errors)),

                OptResultKind.Unavailable => controller.StatusCode(StatusCodes.Status503ServiceUnavailable, ApiResponse<object>.Fail(
                    StatusCodes.Status503ServiceUnavailable,
                    result.Message ?? "Modul pendukung sedang tidak dapat dihubungi. Coba lagi beberapa saat.", errors)),

                _ => controller.BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, result.Message ?? "Permintaan tidak valid.", errors))
            };
        }

        /// <summary>Memetakan hasil lalu, bila berhasil, mencatat satu baris logger aplikasi. Dipakai seluruh endpoint selain <c>GET</c>.</summary>
        public static async Task<IActionResult> RespondAsync<T>(
            ControllerBase controller,
            OptResult<T> result,
            string successMessage,
            LoggerService logger,
            string resource,
            string action,
            Func<T, string> logMessage)
        {
            if (result.IsSuccess && result.Value != null)
            {
                await logger.InfoAsync(LogCategory, $"{resource}.{action}", logMessage(result.Value));
            }

            return Map(controller, result, successMessage);
        }
    }
}
