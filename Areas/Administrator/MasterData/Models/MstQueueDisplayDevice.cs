using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstQueueDisplayDevice", Schema = "public")]
    public class MstQueueDisplayDevice : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid NurseStationClusterId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DisplayCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string DisplayName { get; set; } = string.Empty;

        public QueueDisplayDeviceType DisplayDeviceType { get; set; } = QueueDisplayDeviceType.TvDisplay;

        public QueueDisplayLayoutType LayoutType { get; set; } = QueueDisplayLayoutType.ClusterBoard;

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        [MaxLength(50)]
        public string? RoomName { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(100)]
        public string? MacAddress { get; set; }

        [MaxLength(100)]
        public string? PairingCode { get; set; }

        [MaxLength(200)]
        public string? DeviceToken { get; set; }

        public bool EnableVoiceCalling { get; set; } = true;

        public bool ShowPatientName { get; set; } = false;

        public bool ShowDoctorName { get; set; } = true;

        public bool ShowClinicName { get; set; } = true;

        public int RefreshIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// Masa aktif session login display dalam menit.
        /// Kosongkan untuk memakai fallback dari konfigurasi Jwt:DeviceDefaultExpireMinutes.
        /// Contoh: 1440 = 1 hari, 43200 = 30 hari, 129600 = 90 hari, 525600 = 365 hari.
        /// </summary>
        public int? SessionExpireMinutes { get; set; }

        public DateTime? LastOnlineDateTime { get; set; }

        public DateTime? LastOfflineDateTime { get; set; }

        [MaxLength(250)]
        public string? LastErrorMessage { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstNurseStationCluster? NurseStationCluster { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }
    }

}