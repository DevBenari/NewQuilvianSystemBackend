using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    /// <summary>Tugas belajar penuh waktu dari SK (HRD-DEC-201). Selama masanya pegawai diblokir dari roster (B8) dan rekapnya "Tugas Belajar".</summary>
    public class CreateStudyAssignmentRequest
    {
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DecreeNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string InstitutionName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ProgramName { get; set; }

        public DateOnly StartDate { get; set; }

        /// <summary>Tidak boleh sebelum StartDate (VR-PK-19).</summary>
        public DateOnly EndDate { get; set; }
    }

    /// <summary>Mengubah data SK atau tanggal (kembali lebih cepat, diperpanjang). Pegawai tidak dapat diganti.</summary>
    public class UpdateStudyAssignmentRequest
    {
        [Required]
        [MaxLength(100)]
        public string DecreeNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string InstitutionName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ProgramName { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        /// <summary>Wajib (HRD-DEC-201 butir 5, VR-PK-19).</summary>
        [MaxLength(500)]
        public string? ChangeReason { get; set; }
    }

    public class CancelStudyAssignmentRequest
    {
        /// <summary>Wajib (state 30.4, VR-PK-19).</summary>
        [MaxLength(500)]
        public string? CancelReason { get; set; }
    }

    public class StudyAssignmentQueryRequest
    {
        public Guid? WorkforceProfileId { get; set; }

        /// <summary>Hanya tugas belajar Active yang mencakup tanggal ini.</summary>
        public DateOnly? ActiveOn { get; set; }

        /// <summary>Active atau Cancelled.</summary>
        public string? Status { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class StudyAssignmentResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? ProfileCode { get; set; }
        public string? EmployeeName { get; set; }
        public string DecreeNumber { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public string? ProgramName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;

        /// <summary>Alasan perubahan atau pembatalan terakhir.</summary>
        public string? LastChangeReason { get; set; }

        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public string? CreatedByName { get; set; }

        /// <summary>Siapa dan kapan perubahan terakhir (HRD-DEC-201 butir 5).</summary>
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public string? UpdatedByName { get; set; }

        /// <summary>Jumlah rekap harian yang dijadwalkan dihitung ulang oleh tindakan ini (INT-HR-102); 0 pada baca.</summary>
        public int ReprocessScheduled { get; set; }
    }
}
