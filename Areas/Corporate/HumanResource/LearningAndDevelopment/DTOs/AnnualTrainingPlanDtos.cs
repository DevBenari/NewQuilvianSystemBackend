using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs
{
    public class AnnualTrainingPlanDto
    {
        public Guid Id { get; set; }
        public string TrainingPlanNumber { get; set; } = string.Empty;
        public string TrainingPlanName { get; set; } = string.Empty;
        public int PlanYear { get; set; }
        public string PlanType { get; set; } = "Annual";
        public string PlanStatus { get; set; } = "Draft";
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public string? DeliveryMode { get; set; }
        public string? ProviderName { get; set; }
        public string? TargetAudience { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsExternalTraining { get; set; }
        public int PlannedParticipantCount { get; set; }
        public int ApprovedParticipantCount { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal ApprovedBudget { get; set; }
        public string CurrencyCode { get; set; } = "IDR";
        public string? Objectives { get; set; }
        public string? Notes { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? MandatoryTrainingRuleId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? SubmittedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateAnnualTrainingPlanRequestDto
    {
        [Required(ErrorMessage = "Nama rencana pelatihan tahunan wajib diisi.")]
        [MaxLength(250)]
        public string TrainingPlanName { get; set; } = string.Empty;

        [Range(2020, 2050, ErrorMessage = "Tahun rencana kerja tidak valid.")]
        public int PlanYear { get; set; } = DateTime.UtcNow.Year;

        public DateTime PlannedStartDate { get; set; }

        public DateTime PlannedEndDate { get; set; }

        [MaxLength(50)]
        public string? DeliveryMode { get; set; } = "InHouseWorkshop";

        [MaxLength(250)]
        public string? ProviderName { get; set; }

        [MaxLength(1000)]
        public string? TargetAudience { get; set; }

        public bool IsMandatory { get; set; } = false;

        public bool IsExternalTraining { get; set; } = false;

        [Range(1, 10000, ErrorMessage = "Target jumlah peserta minimal 1 orang (VR-TRN-13).")]
        public int PlannedParticipantCount { get; set; }

        [Range(1.0, 10000000000.0, ErrorMessage = "Estimasi anggaran wajib lebih besar dari 0 (VR-TRN-13).")]
        public decimal EstimatedCost { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required(ErrorMessage = "Target tujuan pembelajaran dan kompetensi (Objectives) wajib diisi (VR-TRN-13).")]
        [MaxLength(3000)]
        public string Objectives { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? MandatoryTrainingRuleId { get; set; }
    }

    public class UpdateAnnualTrainingPlanRequestDto
    {
        [Required(ErrorMessage = "Nama rencana pelatihan tahunan wajib diisi.")]
        [MaxLength(250)]
        public string TrainingPlanName { get; set; } = string.Empty;

        public DateTime PlannedStartDate { get; set; }

        public DateTime PlannedEndDate { get; set; }

        [MaxLength(50)]
        public string? DeliveryMode { get; set; }

        [MaxLength(250)]
        public string? ProviderName { get; set; }

        [MaxLength(1000)]
        public string? TargetAudience { get; set; }

        public bool IsMandatory { get; set; }

        public bool IsExternalTraining { get; set; }

        [Range(1, 10000)]
        public int PlannedParticipantCount { get; set; }

        [Range(1.0, 10000000000.0)]
        public decimal EstimatedCost { get; set; }

        [MaxLength(3000)]
        public string? Objectives { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class ApproveTrainingPlanRequestDto
    {
        [Range(0.0, 10000000000.0, ErrorMessage = "Anggaran yang disetujui tidak boleh negatif.")]
        public decimal ApprovedBudget { get; set; }

        public int? ApprovedParticipantCount { get; set; }

        [MaxLength(1000)]
        public string? ApprovalNotes { get; set; }
    }

    public class RejectTrainingPlanRequestDto
    {
        [Required(ErrorMessage = "Alasan penolakan / pengembalian revisi wajib diisi.")]
        [MaxLength(1000)]
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class SubmitTrainingPlanRequestDto
    {
        [MaxLength(1000)]
        public string? SubmissionNotes { get; set; }
    }
}
