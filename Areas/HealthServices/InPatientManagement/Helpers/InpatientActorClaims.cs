using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers
{
    /// <summary>
    /// Membaca identitas pelaku dari klaim pengguna yang sedang masuk, untuk seluruh controller
    /// Rawat Inap.
    /// </summary>
    /// <remarks>
    /// <b>Kenapa dikumpulkan di satu tempat.</b> Empat penjaga kewenangan per pasien —
    /// <c>GUARD-INP-01</c> sampai <c>GUARD-INP-04</c> — semuanya bergantung pada jawaban dua
    /// pertanyaan: siapa penggunanya, dan apakah ia seorang dokter. Bila setiap controller
    /// membaca klaimnya sendiri-sendiri, satu controller yang membaca nama klaim yang salah
    /// akan mengembalikan "bukan dokter" untuk seorang DPJP, dan penjaganya menolak orang yang
    /// justru berwenang. Kesalahan seperti itu tidak menghasilkan galat apa pun; ia hanya
    /// terlihat sebagai 403 yang membingungkan.
    ///
    /// <para>
    /// Nama klaim di sini mengikuti yang benar-benar diterbitkan
    /// <c>AuthController</c>: <c>user_id</c>, <c>doctor_id</c>, dan <c>employee_id</c>.
    /// </para>
    /// </remarks>
    public static class InpatientActorClaims
    {
        /// <summary>
        /// Daftar nama peran yang diperlakukan sebagai supervisor atau kepala ruangan.
        /// </summary>
        /// <remarks>
        /// <b>Ini asumsi yang masih perlu dikonfirmasi.</b> Nama peran di repository ini adalah
        /// data yang disiapkan admin, bukan daftar tetap di dalam kode, dan tidak satu pun
        /// kontrak modul Rawat Inap menyebutkan nama peran sesungguhnya. Bila nama peran di
        /// rumah sakit berbeda, penjaga yang memakainya akan menolak supervisor yang sah.
        /// Tercatat sebagai risiko terbuka pada laporan `BE-RWI-008` bagian 5.3.
        /// </remarks>
        public static readonly string[] SupervisorOrWardHeadRoles =
        {
            "SuperAdmin",
            "Supervisor",
            "KepalaRuangan",
            "Kepala Ruangan"
        };

        /// <summary>Nama peran yang diperlakukan sebagai supervisor saja, tanpa kepala ruangan.</summary>
        public static readonly string[] SupervisorRoles =
        {
            "SuperAdmin",
            "Supervisor"
        };

        /// <summary>
        /// Identitas pengguna yang sedang masuk. Mengembalikan <c>Guid.Empty</c> bila klaimnya
        /// tidak terbaca.
        /// </summary>
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value =
                user.FindFirstValue("user_id") ??
                user.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        /// <summary>
        /// Identitas dokter milik pengguna yang sedang masuk, atau <c>null</c> bila
        /// penggunanya bukan dokter.
        /// </summary>
        /// <remarks>
        /// Klaim <c>doctor_id</c> selalu diterbitkan, tetapi berisi string kosong untuk
        /// pengguna yang bukan dokter. Karena itu nilai yang tidak dapat diurai dan
        /// <c>Guid.Empty</c> sama-sama diperlakukan sebagai bukan dokter.
        /// </remarks>
        public static Guid? GetDoctorId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("doctor_id");

            if (Guid.TryParse(value, out var id) && id != Guid.Empty)
            {
                return id;
            }

            return null;
        }

        /// <summary>Benar bila pengguna berperan supervisor atau kepala ruangan.</summary>
        public static bool IsSupervisorOrWardHead(this ClaimsPrincipal user)
        {
            return SupervisorOrWardHeadRoles.Any(user.IsInRole);
        }

        /// <summary>Benar bila pengguna berperan supervisor.</summary>
        public static bool IsSupervisor(this ClaimsPrincipal user)
        {
            return SupervisorRoles.Any(user.IsInRole);
        }
    }
}
