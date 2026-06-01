using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstKioskDevice", Schema = "public")]
    public class MstKioskDevice : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string DeviceCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string DeviceName { get; set; } = string.Empty;

        public KioskDeviceType DeviceType { get; set; } = KioskDeviceType.Unknown;

        public KioskDeviceStatus DeviceStatus { get; set; } = KioskDeviceStatus.Active;

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DefaultScannerProfileId { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        [MaxLength(100)]
        public string? IpAddress { get; set; }

        [MaxLength(100)]
        public string? MacAddress { get; set; }

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [MaxLength(100)]
        public string? DeviceModel { get; set; }

        [MaxLength(100)]
        public string? VendorName { get; set; }

        public bool IsAvailableForRegistration { get; set; } = true;

        public bool IsAvailableForCheckIn { get; set; } = true;

        public bool IsAvailableForPayment { get; set; } = false;

        public bool IsAllowWalkIn { get; set; } = true;

        public bool IsAllowAppointment { get; set; } = true;

        public bool IsAllowInsuranceRegistration { get; set; } = true;

        public DateTime? LastOnlineAt { get; set; }

        public DateTime? LastOfflineAt { get; set; }

        [MaxLength(250)]
        public string? LastErrorMessage { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public MstIdentityScannerProfile? DefaultScannerProfile { get; set; }
    }
}
