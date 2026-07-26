using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models
{
    [Table("MstReimbursementPolicy", Schema = "public")]
    public class MstReimbursementPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ExpenseCategoryId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReimbursementPolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ReimbursementPolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required]
        [MaxLength(50)]
        public string LimitPeriod { get; set; } = "PerTransaction";
        // PerTransaction, Daily, Monthly, Annual.

        public decimal MinimumClaimAmount { get; set; } = 0m;

        public decimal? MaximumAmountPerTransaction { get; set; }

        public decimal? MaximumAmountPerDay { get; set; }

        public decimal? MaximumAmountPerMonth { get; set; }

        public decimal? MaximumAmountPerYear { get; set; }

        public bool RequiresReceipt { get; set; } = true;

        public decimal? ReceiptRequiredAmount { get; set; }

        public bool AllowWithoutReceipt { get; set; } = false;

        public int MaximumSubmissionDays { get; set; } = 30;

        public bool AllowBackdatedSubmission { get; set; } = true;

        public bool RequireCostCenter { get; set; } = true;

        public bool RequireManagerApproval { get; set; } = true;

        public bool RequireHrVerification { get; set; } = false;

        public bool RequireFinanceVerification { get; set; } = true;

        [MaxLength(100)]
        public string? ApprovalWorkflowCode { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstExpenseCategory? ExpenseCategory { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstEmployeeCategory? EmployeeCategory { get; set; }

        public MstEmploymentType? EmploymentType { get; set; }
    }
}
