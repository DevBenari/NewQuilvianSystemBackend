using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("WfpEmploymentHistory", Schema = "public")]
    public class WfpEmploymentHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(50)]
        public string HistoryType { get; set; } = "StatusChange";
        // Join, StatusChange, Transfer, Promotion, Demotion, Rotation, ContractChange, Separation.

        [MaxLength(100)]
        public string? OldStatus { get; set; }

        [MaxLength(100)]
        public string? NewStatus { get; set; }

        public Guid? OldEmploymentStatusId { get; set; }
        public Guid? NewEmploymentStatusId { get; set; }
        public Guid? OldEmploymentTypeId { get; set; }
        public Guid? NewEmploymentTypeId { get; set; }
        public Guid? OldDepartmentId { get; set; }
        public Guid? NewDepartmentId { get; set; }
        public Guid? OldPositionId { get; set; }
        public Guid? NewPositionId { get; set; }
        public Guid? OldOrganizationUnitId { get; set; }
        public Guid? NewOrganizationUnitId { get; set; }
        public Guid? OldEmployeeGradeId { get; set; }
        public Guid? NewEmployeeGradeId { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(250)]
        public string? Reason { get; set; }

        [MaxLength(100)]
        public string? ReferenceType { get; set; }

        public Guid? ReferenceId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(150)]
        public string? FileContentType { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmploymentStatus? OldEmploymentStatus { get; set; }
        public MstEmploymentStatus? NewEmploymentStatus { get; set; }
        public MstEmploymentType? OldEmploymentType { get; set; }
        public MstEmploymentType? NewEmploymentType { get; set; }
        public MstDepartment? OldDepartment { get; set; }
        public MstDepartment? NewDepartment { get; set; }
        public MstPosition? OldPosition { get; set; }
        public MstPosition? NewPosition { get; set; }
        public MstOrganizationUnit? OldOrganizationUnit { get; set; }
        public MstOrganizationUnit? NewOrganizationUnit { get; set; }
        public MstEmployeeGrade? OldEmployeeGrade { get; set; }
        public MstEmployeeGrade? NewEmployeeGrade { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
