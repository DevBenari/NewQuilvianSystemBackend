using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>
    /// Jenis unit tujuan rujukan (<c>RJ-DOC-DEC-071</c>). Unit tujuan menentukan kunjungan.
    /// </summary>
    public enum ReferralTargetUnitType
    {
        [Display(Name = "Poliklinik")]
        Clinic = 1,

        [Display(Name = "Laboratorium")]
        Laboratory = 2,

        /// <summary>
        /// Disiapkan, tetapi ditolak selama Radiologi belum punya registrasi pasien
        /// (<c>RJ-DOC-OQ-PM-02</c>, <c>RJ-VAL-PM-05</c>).
        /// </summary>
        [Display(Name = "Radiologi")]
        Radiology = 3
    }
}
