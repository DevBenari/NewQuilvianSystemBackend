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

        public DateTime? LastOnlineDateTime { get; set; }

        public DateTime? LastOfflineDateTime { get; set; }

        [MaxLength(250)]
        public string? LastErrorMessage { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstNurseStationCluster? NurseStationCluster { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }
    }

}