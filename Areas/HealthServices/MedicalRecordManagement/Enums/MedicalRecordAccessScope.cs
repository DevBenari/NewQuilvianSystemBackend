using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums
{
    /// <summary>
    /// Bagian rekam medis yang dibuka. Disimpan pada jejak akses agar pembukaan catatan pribadi
    /// dapat dihitung terpisah saat ditinjau, sesuai RM-DEC-022.
    /// </summary>
    public enum MedicalRecordAccessScope
    {
        [Display(Name = "Ringkasan Berkas")]
        Summary = 1,

        [Display(Name = "Riwayat Lintas Kunjungan")]
        Timeline = 2,

        [Display(Name = "Detail Dokumen")]
        DocumentDetail = 3,

        [Display(Name = "Catatan Pribadi")]
        PrivateNote = 4
    }
}
