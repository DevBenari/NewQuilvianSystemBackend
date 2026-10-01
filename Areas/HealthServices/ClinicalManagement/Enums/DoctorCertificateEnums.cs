using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>Jenis surat dokter — RJ-DOC-REV-BE-004.</summary>
    public enum DoctorCertificateType
    {
        [Display(Name = "Surat Keterangan Sakit")]
        SickLeave = 1,

        [Display(Name = "Surat Keterangan Sehat")]
        Health = 2,

        [Display(Name = "Surat Rujukan")]
        InpatientReferral = 3
    }

    /// <summary>Status surat dokter. Surat terbit tidak dihapus; ia dibatalkan.</summary>
    public enum DoctorCertificateStatus
    {
        [Display(Name = "Terbit")]
        Issued = 1,

        [Display(Name = "Dibatalkan")]
        Cancelled = 2
    }
}
