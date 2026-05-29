using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceTaxResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? NpwpNumber { get; set; }

        public string TaxStatus { get; set; } = string.Empty;

        public bool IsTaxed { get; set; }

        public string TaxCalculationMethod { get; set; } = string.Empty;

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceTaxListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int TaxedData { get; set; }

        public List<WorkforceTaxResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceTaxRequest
    {
        [MaxLength(30)]
        public string? NpwpNumber { get; set; }

        [MaxLength(50)]
        public string TaxStatus { get; set; } = "TK0";

        public bool IsTaxed { get; set; } = true;

        [MaxLength(50)]
        public string TaxCalculationMethod { get; set; } = "Gross";

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceTaxRequest
    {
        [MaxLength(30)]
        public string? NpwpNumber { get; set; }

        [MaxLength(50)]
        public string TaxStatus { get; set; } = "TK0";

        public bool IsTaxed { get; set; } = true;

        [MaxLength(50)]
        public string TaxCalculationMethod { get; set; } = "Gross";

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceTaxStatusRequest
    {
        public bool IsActive { get; set; }

        public bool IsTaxed { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
