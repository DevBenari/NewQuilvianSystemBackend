using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceTransportAllowanceResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Guid? TransportAllowancePolicyId { get; set; }

        public string? PolicyCode { get; set; }

        public string? PolicyName { get; set; }

        public bool IsEligible { get; set; }

        public bool IsRegularTransportEligible { get; set; }

        public bool IsNightTransportEligible { get; set; }

        public string AllowanceMode { get; set; } = string.Empty;

        public decimal MonthlyAmount { get; set; }

        public decimal DailyAmount { get; set; }

        public decimal NightAmount { get; set; }

        public bool IsProrated { get; set; }

        public bool IsTaxable { get; set; }

        public bool IsPayrollComponent { get; set; }

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceTransportAllowanceListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int EligibleData { get; set; }

        public int RegularEligibleData { get; set; }

        public int NightEligibleData { get; set; }

        public List<WorkforceTransportAllowanceResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceTransportAllowanceRequest
    {
        public Guid? TransportAllowancePolicyId { get; set; }

        public bool IsEligible { get; set; } = false;

        public bool IsRegularTransportEligible { get; set; } = false;

        public bool IsNightTransportEligible { get; set; } = false;

        [Required]
        [MaxLength(50)]
        public string AllowanceMode { get; set; } = "None";

        public decimal MonthlyAmount { get; set; } = 0;

        public decimal DailyAmount { get; set; } = 0;

        public decimal NightAmount { get; set; } = 0;

        public bool IsProrated { get; set; } = true;

        public bool IsTaxable { get; set; } = true;

        public bool IsPayrollComponent { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceTransportAllowanceRequest
    {
        public Guid? TransportAllowancePolicyId { get; set; }

        public bool IsEligible { get; set; } = false;

        public bool IsRegularTransportEligible { get; set; } = false;

        public bool IsNightTransportEligible { get; set; } = false;

        [Required]
        [MaxLength(50)]
        public string AllowanceMode { get; set; } = "None";

        public decimal MonthlyAmount { get; set; } = 0;

        public decimal DailyAmount { get; set; } = 0;

        public decimal NightAmount { get; set; } = 0;

        public bool IsProrated { get; set; } = true;

        public bool IsTaxable { get; set; } = true;

        public bool IsPayrollComponent { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceTransportAllowanceStatusRequest
    {
        public bool IsActive { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }   
}
