using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models
{
    [Table("TrxWorkRestriction", Schema = "public")]
    public class TrxWorkRestriction : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid FitnessToWorkId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }

        [Required]
        [MaxLength(60)]
        public string RestrictionNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(60)]
        public string RestrictionType { get; set; } = "Temporary";

        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [Required]
        [MaxLength(40)]
        public string RestrictionStatus { get; set; } = "Active";

        [MaxLength(1500)]
        public string? AdministrativeInstruction { get; set; }
        [MaxLength(1000)]
        public string? RestrictedActivity { get; set; }
        [MaxLength(1000)]
        public string? AllowedActivity { get; set; }

        public int? MaximumWorkHoursPerDay { get; set; }
        public bool NoNightShift { get; set; } = false;
        public bool NoOnCall { get; set; } = false;
        public bool NoHeavyLifting { get; set; } = false;
        public bool RequiresWorkplaceAdjustment { get; set; } = false;
        public bool IsSchedulingBlocked { get; set; } = false;
        public bool IsClinicalServiceBlocked { get; set; } = false;

        public DateTime? ReviewDate { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public TrxEmployeeFitnessToWork? FitnessToWork { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
