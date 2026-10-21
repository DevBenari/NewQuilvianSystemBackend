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
        InpatientReferral = 3,

        /// <summary>
        /// Surat Pengantar Rawat Inap — satu-satunya sumber pengajuan transfer Poliklinik → Rawat Inap
        /// pada antrean Admisi Transfer (<c>SourceType = Outpatient</c>). Terbit langsung, tidak
        /// menunggu finalisasi konsultasi; tanda tangannya berasal dari akun dokter yang menerbitkan.
        /// </summary>
        [Display(Name = "Surat Pengantar Rawat Inap")]
        InpatientAdmissionLetter = 4
    }

    /// <summary>
    /// Tingkat ketergantungan pasien pada Surat Pengantar Rawat Inap — dipakai unit Rawat Inap
    /// menyiapkan tenaga dan ruang perawatan.
    /// </summary>
    public enum PatientDependencyLevel
    {
        [Display(Name = "Low Dependency")]
        Low = 1,

        [Display(Name = "Moderate Dependency")]
        Moderate = 2,

        [Display(Name = "High Dependency")]
        High = 3
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
