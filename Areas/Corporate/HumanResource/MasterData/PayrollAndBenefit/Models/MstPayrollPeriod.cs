using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models
{
    [Table("MstPayrollPeriod", Schema = "public")]
    public class MstPayrollPeriod : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PayrollPeriodCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PayrollPeriodName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string PeriodType { get; set; } = "Monthly";
        // Monthly, BiWeekly, Weekly, Special, Adjustment.

        public int FiscalYear { get; set; }

        public int PeriodNumber { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public DateTime? AttendanceCutoffDate { get; set; }

        public DateTime? VariableInputCutoffDate { get; set; }

        public DateTime? ApprovalDueDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string PayrollPeriodStatus { get; set; } = "Draft";
        // Draft, Open, Processing, Review, Approved, Closed, Posted, Cancelled.

        public bool IsLocked { get; set; } = false;

        public DateTime? LockedAt { get; set; }

        public Guid? LockedByUserId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }
    }
}
