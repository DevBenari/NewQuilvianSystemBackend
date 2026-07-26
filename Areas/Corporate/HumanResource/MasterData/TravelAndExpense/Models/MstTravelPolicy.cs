using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models
{
    [Table("MstTravelPolicy", Schema = "public")]
    public class MstTravelPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TravelTypeId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TravelPolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string TravelPolicyName { get; set; } = string.Empty;

        public int MinimumServiceMonths { get; set; } = 0;

        public int MinimumAdvanceRequestDays { get; set; } = 0;

        public int? MaximumTravelDays { get; set; }

        public bool AllowWeekendTravel { get; set; } = true;

        public bool AllowHolidayTravel { get; set; } = true;

        public bool AllowCompanion { get; set; } = false;

        public bool AllowCashAdvance { get; set; } = true;

        public decimal MaximumAdvancePercentage { get; set; } = 80m;

        public bool RequireBudgetAvailability { get; set; } = true;

        public bool RequireItinerary { get; set; } = true;

        public bool RequireTravelOrder { get; set; } = true;

        public bool RequireManagerApproval { get; set; } = true;

        public bool RequireHrVerification { get; set; } = true;

        public bool RequireFinanceVerification { get; set; } = true;

        public bool RequireSettlement { get; set; } = true;

        public int SettlementDueDays { get; set; } = 7;

        public decimal? ReceiptRequiredAmount { get; set; }

        [MaxLength(100)]
        public string? ApprovalWorkflowCode { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstTravelType? TravelType { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstEmployeeCategory? EmployeeCategory { get; set; }

        public MstEmploymentType? EmploymentType { get; set; }

        public ICollection<MstTravelAllowanceRate> AllowanceRates { get; set; }
            = new List<MstTravelAllowanceRate>();
    }
}
