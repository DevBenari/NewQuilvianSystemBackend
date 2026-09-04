using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums
{
    /// <summary>
    /// Status keutuhan sebuah dokumen klinis: masih boleh diubah atau tidak.
    ///
    /// Status ini BERBEDA dari status alur kerja yang sudah ada pada masing-masing dokumen.
    /// Status alur kerja menjawab "sudah selesai dikerjakan atau belum". Status keutuhan
    /// menjawab "masih boleh diubah atau tidak". Keduanya berjalan berdampingan sesuai
    /// RM-DEC-013, dan layar wajib membedakannya (RM-FE-008).
    /// </summary>
    public enum ClinicalDocumentIntegrityStatus
    {
        [Display(Name = "Draf")]
        Draft = 1,

        [Display(Name = "Ditandatangani")]
        Signed = 2,

        [Display(Name = "Terkunci, Tidak Ditandatangani")]
        LockedUnsigned = 3,

        [Display(Name = "Dibatalkan")]
        Cancelled = 4
    }
}
