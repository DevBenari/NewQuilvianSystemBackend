using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models
{
    [Table("MstAttendanceDevice", Schema = "public")]
    public class MstAttendanceDevice : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? AttendanceLocationId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AttendanceDeviceCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string AttendanceDeviceName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DeviceType { get; set; } = "Fingerprint";
        // Fingerprint, FaceRecognition, CardReader, QR, Mobile, Integration

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        [MaxLength(100)]
        public string? ModelName { get; set; }

        [MaxLength(64)]
        public string? IpAddress { get; set; }

        public int? Port { get; set; }

        [MaxLength(50)]
        public string? MacAddress { get; set; }

        [MaxLength(100)]
        public string? IntegrationProvider { get; set; }

        [MaxLength(100)]
        public string? ExternalDeviceId { get; set; }

        public DateTime? LastSyncAt { get; set; }

        public bool IsOnline { get; set; } = false;

        public bool IsPrimary { get; set; } = false;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstAttendanceLocation? AttendanceLocation { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }
    }
}
