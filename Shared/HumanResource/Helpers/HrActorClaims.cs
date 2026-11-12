using System.Security.Claims;

namespace QuilvianSystemBackend.Shared.HumanResource.Helpers
{
    /// <summary>
    /// Membaca identitas pelaku dari klaim token untuk controller modul SDM (Human Resource).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Latar belakang (remediasi 10 November 2026, kelompok G <c>backend-unfinished-work.md</c>).</b>
    /// Sepuluh controller SDM membaca klaim <c>"EmployeeId"</c> dan <c>"WorkforceProfileId"</c>
    /// yang tidak pernah diterbitkan <c>AuthController</c>. Token yang sebenarnya memuat
    /// <c>user_id</c>, <c>workforce_profile_id</c>, <c>employee_id</c>, dan <c>doctor_id</c>
    /// (huruf kecil, pemisah garis bawah). Akibatnya identitas pegawai selalu jatuh ke ID akun,
    /// sehingga data "milik saya" tidak cocok dengan data pegawai dan pemeriksaan kepemilikan tidak
    /// pernah bekerja.
    /// </para>
    /// <para>
    /// <b>Keputusan identitas.</b> Modul SDM menyimpan dua jenis kunci pegawai, sehingga helper ini
    /// menyediakan dua resolusi:
    /// <list type="bullet">
    /// <item><see cref="ResolveWorkforceProfileId"/> untuk tabel berkolom <c>WorkforceProfileId</c>
    /// (perjalanan dinas, benefit, tiket ESS, keluhan): <c>workforce_profile_id</c>, lalu <c>employee_id</c>,
    /// lalu ID akun sebagai cadangan terakhir supaya data lama yang terlanjur menyimpan ID akun tetap terbaca.</item>
    /// <item><see cref="ResolveEmployeeId"/> untuk tabel berkolom <c>EmployeeId</c> yang merujuk
    /// <c>MstEmployee</c> (workflow V2, rekam kesehatan kerja): <c>employee_id</c>, lalu ID akun.</item>
    /// </list>
    /// Nilai kosong dan <c>Guid.Empty</c> diperlakukan sebagai "tidak ada", sama seperti
    /// <c>InpatientActorClaims</c> pada modul Rawat Inap.
    /// </para>
    /// </remarks>
    public static class HrActorClaims
    {
        public const string UserIdClaim = "user_id";
        public const string WorkforceProfileIdClaim = "workforce_profile_id";
        public const string EmployeeIdClaim = "employee_id";
        public const string DoctorIdClaim = "doctor_id";

        /// <summary>Identitas akun pengguna yang sedang masuk, atau <c>Guid.Empty</c> bila tidak terbaca.</summary>
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(UserIdClaim) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        /// <summary>Identitas akun pengguna; melempar <see cref="UnauthorizedAccessException"/> bila tidak valid.</summary>
        public static Guid RequireUserId(this ClaimsPrincipal user)
        {
            var id = user.GetUserId();
            if (id == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Identitas user pada token tidak valid.");
            }

            return id;
        }

        /// <summary>Profil tenaga kerja (<c>MstWorkforceProfile.Id</c>) milik pengguna, atau <c>null</c>.</summary>
        public static Guid? GetWorkforceProfileId(this ClaimsPrincipal user) => ParseOptional(user.FindFirstValue(WorkforceProfileIdClaim));

        /// <summary>Data karyawan (<c>MstEmployee.Id</c>) milik pengguna, atau <c>null</c>.</summary>
        public static Guid? GetEmployeeId(this ClaimsPrincipal user) => ParseOptional(user.FindFirstValue(EmployeeIdClaim));

        /// <summary>Data dokter (<c>MstDoctor.Id</c>) milik pengguna, atau <c>null</c> bila bukan dokter.</summary>
        public static Guid? GetDoctorId(this ClaimsPrincipal user) => ParseOptional(user.FindFirstValue(DoctorIdClaim));

        /// <summary>
        /// Kunci pegawai untuk tabel berkolom <c>WorkforceProfileId</c>:
        /// <c>workforce_profile_id</c> → <c>employee_id</c> → ID akun.
        /// </summary>
        public static Guid ResolveWorkforceProfileId(this ClaimsPrincipal user)
        {
            return user.GetWorkforceProfileId() ?? user.GetEmployeeId() ?? user.GetUserId();
        }

        /// <summary>
        /// Kunci pegawai untuk tabel berkolom <c>EmployeeId</c> yang merujuk <c>MstEmployee</c>:
        /// <c>employee_id</c> → ID akun.
        /// </summary>
        public static Guid ResolveEmployeeId(this ClaimsPrincipal user)
        {
            return user.GetEmployeeId() ?? user.GetUserId();
        }

        private static Guid? ParseOptional(string? value)
        {
            if (Guid.TryParse(value, out var id) && id != Guid.Empty)
            {
                return id;
            }

            return null;
        }
    }
}
