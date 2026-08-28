using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Tests.Infrastructure
{
    /// <summary>
    /// Alat bantu memanggil controller langsung dari uji.
    ///
    /// Uji ini memanggil controller apa adanya, bukan lewat HTTP. Alasannya: perbaikan yang
    /// sedang dibuktikan memang berada di dalam controller, sehingga menguji lapisan di
    /// bawahnya saja tidak akan membuktikan apa pun.
    ///
    /// Yang TIDAK diuji dengan cara ini: pemeriksaan hak akses, penyaringan permintaan, dan
    /// bentuk balasan HTTP. Ketiganya berada di lapisan yang dilewati, dan perlu uji tersendiri
    /// lewat HTTP bila kelak diperlukan.
    /// </summary>
    public static class ControllerTestHarness
    {
        /// <summary>
        /// Membuat <see cref="LoggerService"/> yang aman dipakai uji: menulis ke logger kosong
        /// dan tidak memerlukan konteks HTTP sungguhan.
        /// </summary>
        public static LoggerService BuatLoggerService(Guid? userId = null)
        {
            var accessor = new HttpContextAccessor
            {
                HttpContext = BuatHttpContext(userId ?? Guid.NewGuid())
            };

            return new LoggerService(NullLogger<LoggerService>.Instance, accessor);
        }

        /// <summary>
        /// Membuat konteks HTTP tiruan yang membawa identitas satu pengguna.
        /// </summary>
        public static DefaultHttpContext BuatHttpContext(Guid userId)
        {
            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("sub", userId.ToString())
            ], authenticationType: "UjiOtomatis");

            return new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            };
        }

        /// <summary>
        /// Memasang identitas pengguna pada controller yang sedang diuji.
        /// </summary>
        public static TController DenganPengguna<TController>(this TController controller, Guid userId)
            where TController : ControllerBase
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = BuatHttpContext(userId)
            };

            return controller;
        }

        /// <summary>
        /// Mengambil kode status dari balasan controller, apa pun bentuknya.
        /// </summary>
        public static int KodeStatus(IActionResult hasil) => hasil switch
        {
            ObjectResult objek => objek.StatusCode ?? StatusCodes.Status200OK,
            StatusCodeResult kode => kode.StatusCode,
            _ => StatusCodes.Status200OK
        };

        /// <summary>
        /// Mengambil pesan dari balasan controller yang memakai pembungkus
        /// <c>ApiResponse&lt;T&gt;</c>.
        /// </summary>
        public static string? Pesan(IActionResult hasil)
        {
            if (hasil is not ObjectResult objek || objek.Value == null)
                return null;

            var properti = objek.Value.GetType().GetProperty(nameof(ApiResponse<object>.Message));
            return properti?.GetValue(objek.Value) as string;
        }
    }
}
