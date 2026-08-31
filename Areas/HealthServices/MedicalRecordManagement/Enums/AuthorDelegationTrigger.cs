using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums
{
    /// <summary>
    /// Sebab seorang penulis catatan dianggap berhalangan, sehingga kepala unit atau DPJP boleh
    /// membuat addendum menggantikannya. Ditetapkan pada RM-DEC-020.
    /// </summary>
    public enum AuthorDelegationTrigger
    {
        /// <summary>
        /// Akun pengguna penulis sudah nonaktif. Sistem menyimpulkannya sendiri, tanpa perlu
        /// ada yang mencatat. Keadaan yang dapat disimpulkan otomatis tidak boleh bergantung
        /// pada seseorang mengingat untuk mencatatnya.
        /// </summary>
        [Display(Name = "Akun Nonaktif")]
        InactiveAccount = 1,

        /// <summary>
        /// Kepala unit menetapkan secara manual, disertai alasan dan batas waktu berlaku.
        /// Batas waktu wajib: penetapan tanpa batas waktu adalah pintu belakang permanen.
        /// </summary>
        [Display(Name = "Penetapan Kepala Unit")]
        UnitHeadGrant = 2
    }
}
