using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums
{
    /// <summary>
    /// Jenis akses terhadap berkas rekam medis, ditetapkan pada RM-DEC-005 dan RM-DEC-016.
    /// </summary>
    public enum MedicalRecordAccessType
    {
        /// <summary>
        /// Pasien sedang memiliki kunjungan aktif. Akses berjalan tanpa diminta alasan.
        /// </summary>
        [Display(Name = "Akses Rawatan")]
        RoutineCare = 1,

        /// <summary>
        /// Pasien tidak memiliki kunjungan aktif. Akses tetap diizinkan, tetapi wajib mengisi
        /// keperluan lebih dulu dan ditandai untuk ditinjau unit rekam medis.
        /// </summary>
        [Display(Name = "Akses Beralasan")]
        ReasonedAccess = 2
    }
}
