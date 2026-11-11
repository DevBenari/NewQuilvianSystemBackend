using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs
{
    public class TrainingBudgetSummaryDto
    {
        public Guid Id { get; set; }
        public Guid TrainingPlanId { get; set; }
        public string? TrainingPlanName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int FiscalYear { get; set; }
        public string BudgetType { get; set; } = "TrainingPlan";
        public string BudgetStatus { get; set; } = "Approved";
        public string CurrencyCode { get; set; } = "IDR";
        public decimal AllocatedAmount { get; set; }
        public decimal CommittedAmount { get; set; }
        public decimal UsedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal AbsorptionPercentage => AllocatedAmount > 0 ? Math.Round((UsedAmount / AllocatedAmount) * 100m, 2) : 0m;
        public decimal CommitmentPercentage => AllocatedAmount > 0 ? Math.Round(((UsedAmount + CommittedAmount) / AllocatedAmount) * 100m, 2) : 0m;
        public DateTime? ApprovedAt { get; set; }
        public string? BudgetNote { get; set; }
        public bool IsActive { get; set; }
    }

    public class AllocateDepartmentBudgetRequestDto
    {
        [Required(ErrorMessage = "TrainingPlanId wajib diisi.")]
        public Guid TrainingPlanId { get; set; }

        [Required(ErrorMessage = "DepartmentId wajib diisi.")]
        public Guid DepartmentId { get; set; }

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? CostCenterId { get; set; }

        [Range(2020, 2050, ErrorMessage = "Tahun fiskal tidak valid.")]
        public int FiscalYear { get; set; } = DateTime.UtcNow.Year;

        [Range(1.0, 10000000000.0, ErrorMessage = "Pagu alokasi anggaran wajib lebih besar dari 0.")]
        public decimal AllocatedAmount { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [MaxLength(2000)]
        public string? BudgetNote { get; set; }
    }

    public class CommitTrainingBudgetRequestDto
    {
        [Required(ErrorMessage = "TrainingPlanId wajib diisi.")]
        public Guid TrainingPlanId { get; set; }

        [Required(ErrorMessage = "DepartmentId wajib diisi.")]
        public Guid DepartmentId { get; set; }

        [Required(ErrorMessage = "EnrollmentRequestId wajib diisi.")]
        public Guid EnrollmentRequestId { get; set; }

        [Range(1.0, 1000000000.0, ErrorMessage = "Jumlah dana komitmen wajib lebih besar dari 0.")]
        public decimal CommitmentAmount { get; set; }

        [MaxLength(500)]
        public string? CommitmentReference { get; set; }
    }

    public class RealizeTrainingBudgetRequestDto
    {
        [Required(ErrorMessage = "BudgetId wajib diisi.")]
        public Guid BudgetId { get; set; }

        [Range(1.0, 1000000000.0, ErrorMessage = "Biaya realisasi aktual wajib lebih besar dari 0.")]
        public decimal RealizedAmount { get; set; }

        public decimal ReleasedCommitmentAmount { get; set; }

        [MaxLength(1000)]
        public string? RealizationNote { get; set; }
    }

    public class DepartmentBudgetValidationResultDto
    {
        public bool IsEligible { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal CommittedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal ProjectedRemainingAmount { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
    }
}
