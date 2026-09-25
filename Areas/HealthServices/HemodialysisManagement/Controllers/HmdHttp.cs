using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Controllers
{
    /// <summary>
    /// Penolong bersama controller Hemodialisa: memetakan <see cref="HmdResult{T}"/> menjadi kode
    /// HTTP kontrak dan mencatat logger aplikasi.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Pemetaannya mengikuti <c>contracts/api-contract.md</c>: <c>201</c> untuk pembuatan,
    /// <c>400</c> isian salah, <c>403</c> pelaku tidak berwenang atas data ini, <c>404</c>,
    /// <c>409</c> bentrok status atau data berubah di tangan orang lain, <c>422</c> syarat belum
    /// terpenuhi, dan <c>423</c> catatan sudah disahkan.
    /// </para>
    /// <para>
    /// <b>Logger tidak pernah memuat kolom sensitif</b> (<c>permission-audit-matrix.md</c> bagian 7):
    /// yang dicatat hanya id baris, controller, aksi, dan status. Alasan klinis, catatan serologi,
    /// dan isi dokumen tinggal di baris datanya sendiri.
    /// </para>
    /// </remarks>
    internal static class HmdHttp
    {
        public const string LogCategory = "HealthServices.HemodialysisManagement";

        /// <summary>Pengguna terautentikasi. Tidak pernah diterima dari isian permintaan.</summary>
        public static Guid ActorId(ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("user_id") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        public static HmdActor Actor(ControllerBase controller) =>
            new(ActorId(controller.User), controller.User);

        public static IActionResult Map<T>(ControllerBase controller, HmdResult<T> result, string successMessage)
        {
            object? errors = result.Code == null && result.Details == null
                ? null
                : new { Code = result.Code, Details = result.Details };

            return result.Kind switch
            {
                HmdResultKind.Success => controller.Ok(ApiResponse<T>.Ok(result.Value, successMessage)),

                HmdResultKind.Created => controller.StatusCode(StatusCodes.Status201Created, new ApiResponse<T>
                {
                    Success = true,
                    StatusCode = StatusCodes.Status201Created,
                    Message = successMessage,
                    Data = result.Value
                }),

                HmdResultKind.NotFound => controller.NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, result.Message ?? "Data tidak ditemukan.", errors)),

                HmdResultKind.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                    StatusCodes.Status403Forbidden, result.Message ?? "Anda tidak memiliki hak akses untuk tindakan ini.", errors)),

                HmdResultKind.Conflict => controller.Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, result.Message ?? "Terjadi konflik data.", errors)),

                HmdResultKind.BusinessRule => controller.UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, result.Message ?? "Syarat tindakan belum terpenuhi.", errors)),

                HmdResultKind.Locked => controller.StatusCode(StatusCodes.Status423Locked, ApiResponse<object>.Fail(
                    StatusCodes.Status423Locked, result.Message ?? "Catatan sudah terkunci.", errors)),

                _ => controller.BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, result.Message ?? "Permintaan tidak valid.", errors))
            };
        }

        /// <summary>
        /// Memetakan hasil lalu, bila berhasil, mencatat satu baris logger aplikasi. Dipakai seluruh
        /// endpoint selain <c>GET</c>, ditambah pengecualian bernama pada
        /// <c>permission-audit-matrix.md</c> bagian 5.
        /// </summary>
        public static async Task<IActionResult> RespondAsync<T>(
            ControllerBase controller,
            HmdResult<T> result,
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
