using System;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs
{
    /// <summary>
    /// Request upload berkas surat keterangan sakit resmi dokter (HRD-DEC-112).
    /// </summary>
    public class UploadMedicalCertificateRequest
    {
        [Required]
        public Guid LeaveRequestId { get; set; }

        [Required(ErrorMessage = "Nama fasilitas kesehatan penerbit surat sakit wajib diisi.")]
        [MaxLength(255)]
        public string FaskesName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama dokter pemeriksa wajib diisi.")]
        [MaxLength(200)]
        public string DoctorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor Surat Izin Praktik (SIP) dokter pemeriksa wajib diisi.")]
        [MaxLength(100)]
        public string DoctorSipNumber { get; set; } = string.Empty;

        /// <summary>
        /// Diagnosa medis ICD-10 yang dilindungi privasi K3RS (LEV-W3, VR-LEV-09).
        /// </summary>
        [MaxLength(100)]
        public string? MedicalDiagnosisIcd10 { get; set; }

        [Required]
        [MaxLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? ContentType { get; set; }

        public long FileSizeBytes { get; set; }
    }

    /// <summary>
    /// Detail berkas surat keterangan sakit dengan dukungan masking diagnosa K3RS (VR-LEV-09).
    /// </summary>
    public class MedicalCertificateDetailDto
    {
        public Guid AttachmentId { get; set; }
        public Guid LeaveRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string FaskesName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSipNumber { get; set; } = string.Empty;

        /// <summary>
        /// Nilai diagnosa ICD-10. Dimaskir untuk atasan langsung, hanya terbaca oleh Tim K3RS.
        /// </summary>
        public string MedicalDiagnosis { get; set; } = string.Empty;

        public bool IsDiagnosisMasked { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public string? VerificationNotes { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedByName { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// Perintah verifikasi berkas medis oleh Petugas K3RS / HR Medis.
    /// </summary>
    public class VerifyMedicalCertificateCommand
    {
        [Required]
        public bool IsApproved { get; set; }

        [MaxLength(1000)]
        public string? VerificationNotes { get; set; }
    }
}
