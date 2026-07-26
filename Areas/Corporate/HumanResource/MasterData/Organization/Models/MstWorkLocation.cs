using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models
{
    [Table("MstWorkLocation", Schema = "public")]
    public class MstWorkLocation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LegalEntityId { get; set; }

        [Required]
        public Guid HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? DepartmentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LocationCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string LocationName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LocationType { get; set; } = "WorkArea";
        // WorkArea, Office, Clinic, Ward, Laboratory, Pharmacy, Warehouse, Remote.

        [MaxLength(150)]
        public string? BuildingName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        [MaxLength(100)]
        public string? RoomName { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstDepartment? Department { get; set; }
    }
}
