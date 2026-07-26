using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("WfpTransportAllowance", Schema = "public")]
    public class WfpTransportAllowance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? TransportAllowancePolicyId { get; set; }
        public Guid? PayrollComponentId { get; set; }

        [Required, MaxLength(30)]
        public string AllowanceStatus { get; set; } = "Active";

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal MonthlyAmount { get; set; } = 0m;
        public decimal PerAttendanceAmount { get; set; } = 0m;
        public decimal MaximumMonthlyAmount { get; set; } = 0m;
        public decimal AccruedAmount { get; set; } = 0m;
        public decimal UsedAmount { get; set; } = 0m;
        public decimal PaidAmount { get; set; } = 0m;
        public decimal RemainingAmount { get; set; } = 0m;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public WfpTransportAllowancePolicy? TransportAllowancePolicy { get; set; }
        public MstPayrollComponent? PayrollComponent { get; set; }

        public ICollection<WfpTransportAllowanceTransaction> Transactions { get; set; }
            = new List<WfpTransportAllowanceTransaction>();
    }
}
