using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("WfpPositionAssignment", Schema = "public")]
    public class WfpPositionAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public Guid? JobFamilyId { get; set; }
        public Guid? JobLevelId { get; set; }
        public Guid? EmployeeGradeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AssignmentType { get; set; } = "Substantive";
        // Substantive, Acting, Functional, Project, Temporary.

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }
        public bool IsPrimary { get; set; } = false;
        public bool IsActing { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstJobFamily? JobFamily { get; set; }
        public MstJobLevel? JobLevel { get; set; }
        public MstEmployeeGrade? EmployeeGrade { get; set; }
    }
}
