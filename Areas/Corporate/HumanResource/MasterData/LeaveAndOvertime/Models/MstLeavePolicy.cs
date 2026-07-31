using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models
{
    [Table("MstLeavePolicy", Schema = "public")]
    public class MstLeavePolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LeaveTypeId { get; set; }

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? WorkLocationId { get; set; }

        public Guid? WorkforceTypeId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? EmploymentStatusId { get; set; }
        public Guid? ContractTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LeavePolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string LeavePolicyName { get; set; } = string.Empty;

        public int Priority { get; set; } = 0;
        public bool IsFallback { get; set; } = false;

        public int MinimumServiceMonths { get; set; } = 0;
        public int MinimumNoticeDays { get; set; } = 0;
        public int? MaximumRequestDays { get; set; }
        public int? MinimumRequestMinutes { get; set; }

        public bool AllowDuringProbation { get; set; } = false;
        public bool AllowNegativeBalance { get; set; } = false;
        public decimal? NegativeBalanceLimitDays { get; set; }

        public bool AllowBackdatedRequest { get; set; } = false;
        public int BackdatedLimitDays { get; set; } = 0;
        public bool AllowFutureDatedRequest { get; set; } = true;
        public int? MaximumAdvanceRequestDays { get; set; }

        [Required]
        [MaxLength(50)]
        public string DayCalculationMethod { get; set; }
            = LeaveValueConstants.DayCalculationMethod.ScheduledWorkDays;

        public bool ExcludeHoliday { get; set; } = true;
        public bool ExcludeWeeklyOff { get; set; } = true;

        [Required]
        [MaxLength(30)]
        public string ReservationTiming { get; set; }
            = LeaveValueConstants.ReservationTiming.OnSubmit;

        [Required]
        [MaxLength(30)]
        public string DeductionTiming { get; set; }
            = LeaveValueConstants.DeductionTiming.OnApproval;

        public bool RequireAttachment { get; set; } = false;
        public int? AttachmentRequiredAfterDays { get; set; }
        public bool RequireReplacementEmployee { get; set; } = false;
        public bool RequireManagerApproval { get; set; } = true;
        public bool RequireHrVerification { get; set; } = false;

        [MaxLength(100)]
        public string? ApprovalWorkflowCode { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public MstLeaveType? LeaveType { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }

        public MstWorkforceType? WorkforceType { get; set; }
        public MstEmployeeCategory? EmployeeCategory { get; set; }
        public MstEmploymentType? EmploymentType { get; set; }
        public MstEmploymentStatus? EmploymentStatus { get; set; }
        public MstContractType? ContractType { get; set; }

        public ICollection<MstLeaveEntitlementPolicy> EntitlementPolicies { get; set; }
            = new List<MstLeaveEntitlementPolicy>();
    }
}
