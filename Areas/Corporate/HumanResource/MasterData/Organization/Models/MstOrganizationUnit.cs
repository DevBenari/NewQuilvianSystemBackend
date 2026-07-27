using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models
{
    [Table("MstOrganizationUnit", Schema = "public")]
    public class MstOrganizationUnit : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? ParentOrganizationUnitId { get; set; }

        // Bridge ke master department lama tanpa mengubah struktur entity lama.
        public Guid? DepartmentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string UnitCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string UnitName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string UnitType { get; set; } = "Unit";
        // Directorate, Division, Installation, Department, Section, Team, Unit.

        public int LevelNumber { get; set; } = 1;

        public bool IsOperationalUnit { get; set; } = true;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? ParentOrganizationUnit { get; set; }

        public MstDepartment? Department { get; set; }

        public ICollection<MstOrganizationUnit> ChildOrganizationUnits { get; set; }
            = new List<MstOrganizationUnit>();

        public ICollection<MstCostCenter> CostCenters { get; set; }
            = new List<MstCostCenter>();

        public ICollection<MstWorkLocation> WorkLocations { get; set; }
            = new List<MstWorkLocation>();
    }
}
