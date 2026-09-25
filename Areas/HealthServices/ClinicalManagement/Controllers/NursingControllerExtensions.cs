using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    /// <summary>
    /// Pembantu balasan controller keperawatan rawat inap revision 7 — <c>BE-RWI-107</c> s.d.
    /// <c>BE-RWI-123</c>. Dipakai controller <c>ClinicalManagement</c> dan <c>PharmacyManagement</c>.
    /// </summary>
    /// <remarks>
    /// Satu tempat yang menerjemahkan <see cref="NursingResult{T}"/> menjadi <c>ApiResponse</c>, supaya
    /// kode HTTP, kalimat, dan kode alasan <c>errors.code</c> terbentuk sama pada seluruh grup baru.
    /// </remarks>
    public static class NursingControllerExtensions
    {
        public static IActionResult ToActionResult<T>(this ControllerBase controller, NursingResult<T> result)
        {
            if (!result.IsSuccess)
            {
                return controller.StatusCode(result.StatusCode, ApiResponse<object>.Fail(
                    result.StatusCode,
                    result.ErrorMessage ?? "Permintaan tidak dapat diproses.",
                    result.Errors));
            }

            var body = ApiResponse<T>.Ok(result.Value, result.Message);
            body.StatusCode = result.StatusCode;

            return controller.StatusCode(result.StatusCode, body);
        }

        public static Guid CurrentUserId(this ControllerBase controller)
        {
            var userId = controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }

        /// <summary>
        /// Kunci permintaan dari header <c>Idempotency-Key</c>, atau dari badan permintaan bila header kosong.
        /// Kosong dibiarkan kosong — kunci acak buatan server tidak menjaga kiriman ulang apa pun.
        /// </summary>
        public static string? IdempotencyKey(this ControllerBase controller, string? fromBody = null)
        {
            var dariHeader = controller.Request.Headers["Idempotency-Key"].ToString();

            return NursingEpisodeWriteGuard.NormalizeIdempotencyKey(
                string.IsNullOrWhiteSpace(dariHeader) ? fromBody : dariHeader);
        }
    }
}
