using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace QuilvianSystemBackend.Shared.HumanResource.Helpers
{
    /// <summary>
    /// Memisahkan galat bisnis yang aman ditampilkan ke pengguna dari galat teknis (EF Core, Npgsql, runtime).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Latar belakang (register-masalah-hr.md MSL-11, 18 November 2026).</b> Banyak controller modul SDM
    /// menangkap <c>catch (Exception ex)</c> lalu mengirim <c>ex.Message</c> ke klien. Saat basis data menolak
    /// penyimpanan, pengguna melihat kalimat teknis berbahasa Inggris seperti
    /// "An error occurred while saving the entity changes. See the inner exception for details."
    /// </para>
    /// <para>
    /// <b>Cara pakai.</b> Pasang sebagai filter: <c>catch (Exception ex) when (HrSafeError.IsDomain(ex))</c>.
    /// Galat bisnis tetap dijawab controller dengan pesannya sendiri; galat lain tidak tertangkap dan diteruskan ke
    /// <c>GlobalExceptionMiddleware</c>, yang mencatat detailnya ke log server dan mengirim pesan Bahasa Indonesia.
    /// </para>
    /// <para>
    /// Galat dianggap bisnis bila jenisnya galat validasi/aturan (<see cref="InvalidOperationException"/>,
    /// <see cref="ArgumentException"/> selain <see cref="ArgumentNullException"/>, <see cref="KeyNotFoundException"/>,
    /// <see cref="UnauthorizedAccessException"/>, <see cref="ValidationException"/>) <b>dan</b> dilempar oleh kode
    /// aplikasi ini sendiri, atau bila jenis exception-nya didefinisikan di aplikasi ini.
    /// <c>InvalidOperationException</c> dari EF Core (misalnya "Sequence contains no elements") dilempar pustaka,
    /// sehingga tidak lolos.
    /// </para>
    /// </remarks>
    public static class HrSafeError
    {
        private static readonly System.Reflection.Assembly AppAssembly = typeof(HrSafeError).Assembly;

        /// <summary>True bila pesan galat aman dan bermakna untuk ditampilkan kepada pengguna.</summary>
        public static bool IsDomain(Exception ex)
        {
            if (ex is DbUpdateException or ArgumentNullException)
            {
                return false;
            }

            // Exception khusus yang didefinisikan aplikasi (misalnya PayrollRunConflictException) selalu galat bisnis.
            if (ex.GetType().Assembly == AppAssembly)
            {
                return true;
            }

            var isRuleViolation = ex is InvalidOperationException
                or ArgumentException
                or KeyNotFoundException
                or UnauthorizedAccessException
                or ValidationException;

            return isRuleViolation && ex.TargetSite?.DeclaringType?.Assembly == AppAssembly;
        }
    }
}
