using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums
{
    /// <summary>
    /// Sebab sebuah dokumen klinis menjadi terkunci.
    ///
    /// Dicatat agar pembaca dapat membedakan dokumen yang dikunci penulisnya sendiri dari
    /// dokumen yang terkunci otomatis karena kunjungan ditutup. Keduanya sama-sama sah, tetapi
    /// yang kedua menandakan catatan belum sempat ditandatangani.
    /// </summary>
    public enum ClinicalDocumentLockTrigger
    {
        [Display(Name = "Ditandatangani Penulis")]
        AuthorSigned = 1,

        [Display(Name = "Kunjungan Ditutup")]
        EncounterClosed = 2,

        [Display(Name = "Pengisian Data Lama")]
        BackfillEncounterClosed = 3,

        [Display(Name = "Dokumen Dibatalkan")]
        DocumentCancelled = 4
    }
}
