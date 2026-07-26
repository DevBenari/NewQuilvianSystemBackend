using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models
{
    [Table("MstOvertimePolicy", Schema = "public")]
    public class MstOvertimePolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string OvertimePolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string OvertimePolicyName { get; set; } = string.Empty;

        public bool RequirePreApproval { get; set; } = true;

        public bool RequirePostVerification { get; set; } = true;

        public bool RequireAttendanceMatch { get; set; } = true;

        public int MinimumOvertimeMinutes { get; set; } = 30;

        public int? MaximumOvertimeMinutesPerDay { get; set; }

        public int? MaximumOvertimeMinutesPerWeek { get; set; }

        public int? MaximumOvertimeMinutesPerMonth { get; set; }

        public int OvertimeThresholdMinutes { get; set; } = 0;

        public int RoundingIntervalMinutes { get; set; } = 30;

        [Required]
        [MaxLength(50)]
        public string RoundingMethod { get; set; } = "Down";
        // None, Up, Down, Nearest.

        public bool DeductBreakMinutes { get; set; } = false;

        public int BreakDeductionMinutes { get; set; } = 0;

        public bool AllowBeforeShift { get; set; } = false;

        public bool AllowAfterShift { get; set; } = true;

        public bool AllowRestDay { get; set; } = true;

        public bool AllowHoliday { get; set; } = true;

        public bool AllowDuringLeave { get; set; } = false;

        public int AttendanceToleranceMinutes { get; set; } = 15;

        [MaxLength(100)]
        public string? ApprovalWorkflowCode { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstEmployeeCategory? EmployeeCategory { get; set; }

        public MstEmploymentType? EmploymentType { get; set; }

        public ICollection<MstOvertimeRate> OvertimeRates { get; set; }
            = new List<MstOvertimeRate>();
    }
}
