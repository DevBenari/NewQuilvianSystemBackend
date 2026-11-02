using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Controllers
{
    /// <summary>
    /// Penolong bersama controller MCU: memetakan <see cref="McuResult{T}"/> menjadi kode HTTP
    /// kontrak dan mencatat logger aplikasi. Polanya sama dengan <c>HmdHttp</c>.
    /// </summary>
    /// <remarks>
    /// <b>Isi log ditulis di teks pesan, bukan di payload.</b> <c>LoggerService</c> tidak
    /// menyerialisasi objek data; ia hanya membacanya untuk menimpa ruas seperti <c>Id</c> dan
    /// <c>Name</c> — yang akan mengganti pelaku pada baris log. Karena itu pemanggil menyusun
    /// kalimat lengkap (nomor/kode, aksi) dan payload tidak dikirim. Kalimat itu tidak pernah
    /// memuat kolom bertanda Sensitif pada kamus data (<c>MCU-PERM-v1</c>).
    /// </remarks>
    internal static class McuHttp
    {
        public const string LogCategory = "HealthServices.MedicalCheckupManagement";

        /// <summary>Pengguna terautentikasi. Tidak pernah diterima dari isian permintaan.</summary>
        public static Guid ActorId(ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("user_id") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        public static IActionResult Map<T>(ControllerBase controller, McuResult<T> result, string successMessage)
        {
            object? errors = result.Code == null ? null : new { result.Code };

            return result.Kind switch
            {
                McuResultKind.Success => controller.Ok(ApiResponse<T>.Ok(result.Value, successMessage)),

                McuResultKind.NotFound => controller.NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, result.Message ?? "Data tidak ditemukan.", errors)),

                McuResultKind.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                    StatusCodes.Status403Forbidden, result.Message ?? "Anda tidak memiliki hak akses untuk tindakan ini.", errors)),

                McuResultKind.Conflict => controller.Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, result.Message ?? "Keadaan data tidak mengizinkan tindakan ini.", errors)),

                _ => controller.BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, result.Message ?? "Permintaan tidak valid.", errors))
            };
        }

        /// <summary>
        /// Memetakan hasil lalu, bila berhasil, mencatat satu baris logger aplikasi. Dipakai seluruh
        /// endpoint selain <c>GET</c>.
        /// </summary>
        /// <param name="logMessage">Kalimat log dari nilai hasil, contoh <c>Paket MCU MCU-BASIC dibuat.</c></param>
        public static async Task<IActionResult> RespondAsync<T>(
            ControllerBase controller,
            McuResult<T> result,
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
