namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums
{
    /// <summary>
    /// Peran yang dipakai penerbit otorisasi darurat (DEC-BD-040, INV-BD-032).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Nilai ini menjawab <b>dengan wewenang apa</b> seseorang bertindak, bukan <b>siapa</b> dia —
    /// pelakunya sudah tersimpan terpisah pada <c>AuthorizedByUserId</c>.
    /// </para>
    /// <para>
    /// <b>Sistem tidak memeriksa kebenaran penugasannya.</b> <c>contracts/validation-matrix.md</c>
    /// menyatakan secara eksplisit bahwa Quilvian tidak memverifikasi apakah penerbit benar DPJP
    /// pasien yang bersangkutan: Bank Darah bukan pemilik data penugasan DPJP. Yang dijaga adalah
    /// kelengkapan rekam; kebenaran penugasannya terbaca saat ditinjau.
    /// </para>
    /// </remarks>
    public enum BbkEmergencyAuthorizerRole
    {
        /// <summary>Dokter Bank Darah.</summary>
        BloodBankDoctor = 0,

        /// <summary>Dokter penanggung jawab pasien.</summary>
        AttendingPhysician = 1
    }
}
