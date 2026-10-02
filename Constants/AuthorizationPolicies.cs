namespace QuilvianSystemBackend.Constants
{
    /// <summary>
    /// Nama policy otorisasi bernama yang <b>disetujui</b> sebagai pengganti sah
    /// <c>[AccessPermission]</c>.
    ///
    /// <para>Sebagian endpoint memang tidak boleh melewati matriks Akses Role karena pemanggilnya
    /// bukan pegawai, melainkan perangkat: kiosk pendaftaran dan layar antrean login memakai akun
    /// perangkat yang tidak punya Departemen × Posisi, sehingga <c>HasAccessAsync</c> tidak punya
    /// baris apa pun untuk diperiksa. Untuk endpoint semacam itu, <c>[Authorize(Policy = ...)]</c>
    /// adalah desain yang disengaja, bukan kelalaian.</para>
    ///
    /// <para><b>Kenapa daftarnya ada di sini, bukan di dalam verifier.</b> Invarian naked endpoint
    /// harus bisa membedakan "endpoint yang sengaja memakai policy perangkat" dari "endpoint yang
    /// lupa dipasangi permission". Bila daftarnya ditulis ulang di dalam verifier, maka penambahan
    /// policy baru di <c>Program.cs</c> tidak terlihat oleh verifier dan endpoint-nya dilaporkan
    /// sebagai naked selamanya — atau lebih buruk, daftar verifier dilonggarkan sekali lalu
    /// menutupi endpoint yang benar-benar lalai. Dengan satu konstanta bersama, pendaftaran policy
    /// dan pengecualian invarian membaca sumber yang sama.</para>
    ///
    /// <para>Menambah nama ke sini adalah keputusan keamanan: ia membuat sekumpulan endpoint
    /// berhenti menuntut <c>[AccessPermission]</c>. Tambahkan hanya bersama policy yang benar-benar
    /// terdaftar di <c>Program.cs</c> dan alasannya dicatat pada laporan task.</para>
    /// </summary>
    public static class AuthorizationPolicies
    {
        /// <summary>Akun perangkat kiosk pendaftaran pasien.</summary>
        public const string KioskRead = "KioskRead";

        /// <summary>Akun perangkat layar antrean — endpoint runtime display.</summary>
        public const string QueueDisplayRuntimeRead = "QueueDisplayRuntimeRead";

        /// <summary>Alias umum policy layar antrean.</summary>
        public const string QueueDisplayRead = "QueueDisplayRead";

        /// <summary>
        /// Himpunan policy yang diterima invarian naked endpoint sebagai otorisasi alternatif
        /// yang sah. Endpoint tanpa <c>[AccessPermission]</c> yang memakai salah satu policy ini
        /// tidak dilaporkan sebagai naked.
        /// </summary>
        public static readonly IReadOnlyCollection<string> ApprovedAlternativeAuthorization =
            new HashSet<string>(StringComparer.Ordinal)
            {
                KioskRead,
                QueueDisplayRuntimeRead,
                QueueDisplayRead
            };

        public static bool IsApprovedAlternativeAuthorization(string? policyName) =>
            !string.IsNullOrWhiteSpace(policyName) &&
            ApprovedAlternativeAuthorization.Contains(policyName);
    }
}
