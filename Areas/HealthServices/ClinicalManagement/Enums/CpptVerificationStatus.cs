namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Keadaan verifikasi DPJP atas satu catatan pada lembar terpadu (CPPT).
    /// </summary>
    /// <remarks>
    /// Bawaannya <c>NotRequired</c>, bukan <c>Pending</c>. PRD menuliskan "bila verifikasi DPJP
    /// diwajibkan": menyalakan <c>Pending</c> sebagai bawaan membuat setiap catatan perawat
    /// langsung terhitung menunggu verifikasi pada rumah sakit yang tidak mewajibkannya, dan
    /// daftar pantau penuh sejak hari pertama.
    ///
    /// <para>
    /// Nilai <c>Overdue</c> diturunkan dari batas waktu, bukan ditulis manual. Nilai batasnya
    /// sendiri belum disahkan — <c>RWI-RULE-021</c> — sehingga tidak satu angka pun ditanam di
    /// sini maupun di tempat lain.
    /// </para>
    /// </remarks>
    public enum CpptVerificationStatus
    {
        /// <summary>Verifikasi tidak diwajibkan; catatan tidak masuk daftar pantau.</summary>
        NotRequired = 0,

        /// <summary>Menunggu verifikasi DPJP.</summary>
        Pending = 1,

        /// <summary>Sudah diverifikasi DPJP yang aktif saat verifikasi.</summary>
        Verified = 2,

        /// <summary>Melewati batas waktu verifikasi dan muncul pada daftar pantau.</summary>
        Overdue = 3
    }
}
