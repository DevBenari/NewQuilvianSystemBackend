using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models
{
    [Table("TrxOvertimeRealization", Schema = "public")]
    public class TrxOvertimeRealization : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string RealizationNumber { get; set; } = string.Empty;

        [Required]
        public Guid OvertimeRequestId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? AttendanceDailyId { get; set; }

        public int RealizationVersion { get; set; } = 1;
        public DateOnly ActualStartDate { get; set; }
        public DateOnly ActualEndDate { get; set; }
        public DateTime? ActualStartAt { get; set; }
        public DateTime? ActualEndAt { get; set; }

        public int RequestedMinutesSnapshot { get; set; } = 0;
        public int ApprovedMinutesSnapshot { get; set; } = 0;
        public int ActualMinutes { get; set; } = 0;
        public int ActualBreakMinutes { get; set; } = 0;
        public int EligibleMinutes { get; set; } = 0;
        public int VerifiedMinutes { get; set; } = 0;
        public int PostedMinutes { get; set; } = 0;
        public int VarianceMinutes { get; set; } = 0;

        public decimal CalculatedAmount { get; set; } = 0;
        public decimal VerifiedAmount { get; set; } = 0;
        public decimal PostedAmount { get; set; } = 0;

        [Required, MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [MaxLength(2000)]
        public string? RealizationNotes { get; set; }

        [Column(TypeName = "jsonb")]
        public string? EvidenceSummaryJson { get; set; }

        [Column(TypeName = "jsonb")]
        public string? CalculationResultJson { get; set; }

        [Required, MaxLength(40)]
        public string RealizationStatus { get; set; } = "Draft";
        // Draft, Submitted, WaitingVerification, NeedRevision,
        // Verified, Rejected, PostedToPayroll, Cancelled.

        public DateTime? SubmittedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime? PostedToPayrollAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public Guid? PostedToPayrollByUserId { get; set; }
        public Guid? CancelledByUserId { get; set; }

        public Guid? PayrollPeriodId { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public bool IsPayrollPosted { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public WfpOvertimeRequest? OvertimeRequest { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public HrdAttendanceDaily? AttendanceDaily { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public MstPayrollComponent? PayrollComponent { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
        public ApplicationUser? PostedToPayrollByUser { get; set; }
        public ApplicationUser? CancelledByUser { get; set; }

        public ICollection<TrxOvertimeRealizationDetail> Details { get; set; }
            = new List<TrxOvertimeRealizationDetail>();

        public ICollection<TrxOvertimeVerification> Verifications { get; set; }
            = new List<TrxOvertimeVerification>();

        public ICollection<TrxCompensatoryTimeOff> CompensatoryTimeOffs { get; set; }
            = new List<TrxCompensatoryTimeOff>();
    }
}
