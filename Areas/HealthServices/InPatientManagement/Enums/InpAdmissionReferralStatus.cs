namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Status permintaan admisi dari kamar pulih (<c>BE-RWI-181</c>, kamus data 19.3 dan 19.8).
    /// <c>Completed</c> dan <c>Cancelled</c> final (<c>INP-ADM-REF-002</c>).
    /// </summary>
    public enum InpAdmissionReferralStatus
    {
        Pending = 1,
        Completed = 2,
        Cancelled = 3
    }

    /// <summary>Tingkat perawatan yang diminta kamar pulih (kamus data 19.3).</summary>
    public enum InpRequestedCareLevel
    {
        Inpatient = 1,
        Icu = 2
    }

    /// <summary>
    /// Akibat simpan keputusan kamar pulih pada permintaan admisi, dikembalikan ke OK
    /// (<c>AdmissionReferralState</c>, API 11.5.2).
    /// </summary>
    public enum InpAdmissionReferralState
    {
        /// <summary>Tidak perlu permintaan: keputusan bukan rawat inap/ICU, atau pasien sudah punya episode hadir.</summary>
        NotNeeded = 1,

        /// <summary>Permintaan <c>Pending</c> lahir atau sudah ada untuk kasus ini.</summary>
        Created = 2,

        /// <summary>Permintaan <c>Pending</c> kasus ini dibatalkan karena keputusan berubah.</summary>
        Cancelled = 3
    }
}
