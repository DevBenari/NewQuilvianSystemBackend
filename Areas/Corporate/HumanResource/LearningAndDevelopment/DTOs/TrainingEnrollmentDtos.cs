using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs
{
    public class TrainingEnrollmentDto
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public Guid TrainingPlanId { get; set; }
        public string? TrainingPlanName { get; set; }
        public Guid? TrainingSessionId { get; set; }
        public string? SessionCode { get; set; }
        public string? SessionName { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? StaffName { get; set; }
        public string? DepartmentName { get; set; }
        public string EnrollmentType { get; set; } = "Internal";
        public string RequestStatus { get; set; } = "Draft";
        public string? DevelopmentNeed { get; set; }
        public string? RequestReason { get; set; }
        public string? ExternalProviderName { get; set; }
        public string? ExternalTrainingName { get; set; }
        public DateTime? ExternalStartDate { get; set; }
        public DateTime? ExternalEndDate { get; set; }
        public decimal RequestedCost { get; set; }
        public string CurrencyCode { get; set; } = "IDR";
        public string? AttachmentPath { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ManagerActionAt { get; set; }
        public string? ManagerActionNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class SubmitEnrollmentRequestDto
    {
        [Required(ErrorMessage = "TrainingPlanId wajib diisi.")]
        public Guid TrainingPlanId { get; set; }

        public Guid? TrainingSessionId { get; set; }

        [Required(ErrorMessage = "WorkforceProfileId wajib diisi.")]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        [MaxLength(50)]
        public string EnrollmentType { get; set; } = "Internal";

        [Required(ErrorMessage = "Kebutuhan pengembangan kompetensi (DevelopmentNeed) wajib diisi.")]
        [MaxLength(2000)]
        public string DevelopmentNeed { get; set; } = string.Empty;

        [Required(ErrorMessage = "Alasan / justifikasi pendaftaran dinas (RequestReason) wajib diisi.")]
        [MaxLength(2000)]
        public string RequestReason { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? ExternalProviderName { get; set; }

        [MaxLength(250)]
        public string? ExternalTrainingName { get; set; }

        public DateTime? ExternalStartDate { get; set; }

        public DateTime? ExternalEndDate { get; set; }

        [Range(0.0, 1000000000.0, ErrorMessage = "Biaya yang diajukan tidak boleh negatif.")]
        public decimal RequestedCost { get; set; } = 0m;

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [MaxLength(1000)]
        public string? AttachmentPath { get; set; }
    }

    public class ProcessEnrollmentApprovalDto
    {
        [Required(ErrorMessage = "Keputusan persetujuan wajib diisi (Approved / Rejected).")]
        [RegularExpression("^(Approved|Rejected)$", ErrorMessage = "Action harus bernilai 'Approved' atau 'Rejected'.")]
        public string Action { get; set; } = "Approved";

        [Required(ErrorMessage = "Tahap approval wajib diisi (Supervisor / Diklat).")]
        [RegularExpression("^(Supervisor|Diklat)$", ErrorMessage = "Stage harus bernilai 'Supervisor' atau 'Diklat'.")]
        public string Stage { get; set; } = "Supervisor";

        [MaxLength(2000)]
        public string? ActionNote { get; set; }
    }
}
