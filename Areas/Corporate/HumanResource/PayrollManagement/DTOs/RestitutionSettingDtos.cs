using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class RestitutionSettingResponseDto
    {
        public Guid Id { get; set; }
        public string GradeLevelCode { get; set; } = string.Empty;
        public string BenefitCategory { get; set; } = string.Empty;
        public decimal AnnualLimitAmount { get; set; }
        public int MinimumServiceMonthsRequired { get; set; }
        public decimal MaxDeductionPercentageOfGross { get; set; }
        public bool AllowEmergencyCitoCashier { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateRestitutionSettingRequestDto
    {
        [Required, MaxLength(50)]
        public string GradeLevelCode { get; set; } = "GRADE-ALL";

        [Required, MaxLength(100)]
        public string BenefitCategory { get; set; } = "KebijakanUmumPemotongan";

        [Range(0, 1000000000)]
        public decimal AnnualLimitAmount { get; set; } = 0.00m;

        [Range(0, 120)]
        public int MinimumServiceMonthsRequired { get; set; } = 0;

        [Range(10, 100)]
        public decimal MaxDeductionPercentageOfGross { get; set; } = 50.00m; // Default 50.00% PP 36/2021

        public bool AllowEmergencyCitoCashier { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateRestitutionSettingRequestDto
    {
        [Range(0, 1000000000)]
        public decimal AnnualLimitAmount { get; set; }

        [Range(0, 120)]
        public int MinimumServiceMonthsRequired { get; set; }

        [Range(10, 100)]
        public decimal MaxDeductionPercentageOfGross { get; set; }

        public bool AllowEmergencyCitoCashier { get; set; }

        public bool IsActive { get; set; }
    }

    public class RestitutionSettingFilterDto
    {
        public string? GradeLevelCode { get; set; }
        public string? BenefitCategory { get; set; }
        public bool? IsActive { get; set; }
    }
}
