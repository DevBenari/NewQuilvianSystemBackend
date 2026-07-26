using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("WfpManagerAssignment", Schema = "public")]
    public class WfpManagerAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid ManagerWorkforceProfileId { get; set; }

        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ManagerPositionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ManagerType { get; set; } = "Direct";
        // Direct, Functional, Project, Acting, DottedLine.

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }
        public bool IsPrimaryManager { get; set; } = true;
        public bool CanApproveRequests { get; set; } = true;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstWorkforceProfile? ManagerWorkforceProfile { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? ManagerPosition { get; set; }
    }
}
