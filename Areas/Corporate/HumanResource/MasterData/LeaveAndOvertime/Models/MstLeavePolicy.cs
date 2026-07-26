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

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LeavePolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string LeavePolicyName { get; set; } = string.Empty;

        public int MinimumServiceMonths { get; set; } = 0;

        public int MinimumNoticeDays { get; set; } = 0;

        public int? MaximumRequestDays { get; set; }

        public int? MinimumRequestMinutes { get; set; }

        public bool AllowDuringProbation { get; set; } = false;

        public bool AllowNegativeBalance { get; set; } = false;

        public bool AllowBackdatedRequest { get; set; } = false;

        public int BackdatedLimitDays { get; set; } = 0;

        public bool AllowFutureDatedRequest { get; set; } = true;

        public int? MaximumAdvanceRequestDays { get; set; }

        public bool ExcludeHoliday { get; set; } = true;

        public bool ExcludeWeeklyOff { get; set; } = true;

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

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstLeaveType? LeaveType { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstEmployeeCategory? EmployeeCategory { get; set; }

        public MstEmploymentType? EmploymentType { get; set; }

        public ICollection<MstLeaveEntitlementPolicy> EntitlementPolicies { get; set; }
            = new List<MstLeaveEntitlementPolicy>();
    }
}
