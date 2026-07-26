using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models
{
    [Table("MstAttendanceLocation", Schema = "public")]
    public class MstAttendanceLocation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? WorkLocationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AttendanceLocationCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string AttendanceLocationName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LocationType { get; set; } = "OnSite";
        // OnSite, Remote, Mobile, Field, Hybrid

        [MaxLength(500)]
        public string? Address { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public int RadiusMeters { get; set; } = 100;

        public bool AllowMobileAttendance { get; set; } = false;

        public bool AllowDeviceAttendance { get; set; } = true;

        public bool RequiresGeolocation { get; set; } = false;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstWorkLocation? WorkLocation { get; set; }

        public ICollection<MstAttendanceDevice> AttendanceDevices { get; set; }
            = new List<MstAttendanceDevice>();
    }
}
