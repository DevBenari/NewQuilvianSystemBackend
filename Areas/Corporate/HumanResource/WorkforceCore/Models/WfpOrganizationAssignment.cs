using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("WfpOrganizationAssignment", Schema = "public")]
    public class WfpOrganizationAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public Guid? CostCenterId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? EmployeeGradeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AssignmentType { get; set; } = "Primary";
        // Primary, Secondary, Acting, Temporary, Project, Functional.

        public bool IsPrimary { get; set; } = false;
        public bool IsManagerialAssignment { get; set; } = false;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(100)]
        public string? AssignmentNumber { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public MstEmployeeGrade? EmployeeGrade { get; set; }
    }
}
