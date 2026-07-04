using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
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

        public Guid? DefaultScannerProfileId { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        [MaxLength(100)]
        public string? IpAddress { get; set; }

        [MaxLength(100)]
        public string? MacAddress { get; set; }

        public bool IsAllowWalkIn { get; set; } = true;

        public bool IsAllowAppointment { get; set; } = true;

        public bool IsAllowInsuranceRegistration { get; set; } = true;

        /// <summary>
        /// Masa aktif session login device dalam menit.
        /// Kosongkan untuk memakai fallback dari konfigurasi Jwt:DeviceDefaultExpireMinutes.
        /// Contoh: 1440 = 1 hari, 43200 = 30 hari, 129600 = 90 hari, 525600 = 365 hari.
        /// </summary>
        public int? SessionExpireMinutes { get; set; }

        public DateTime? LastOnlineAt { get; set; }

        public DateTime? LastOfflineAt { get; set; }

        [MaxLength(250)]
        public string? LastErrorMessage { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstIdentityScannerProfile? DefaultScannerProfile { get; set; }
    }
}