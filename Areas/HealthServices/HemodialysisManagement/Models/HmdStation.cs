using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>Kursi atau tempat tidur tempat pasien dicuci darah — master milik modul.</summary>
    [Table("HmdStation", Schema = "public")]
    public class HmdStation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string StationCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string StationName { get; set; } = string.Empty;

        [Required]
        public Guid ServiceUnitId { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public Guid? RoomId { get; set; }

        public MstRoom? Room { get; set; }

        public HmdStationStatus StationStatus { get; set; } = HmdStationStatus.Available;

        /// <summary>Alasan perubahan status terakhir; wajib diisi setiap perubahan.</summary>
        [MaxLength(500)]
        public string? StatusReason { get; set; }

        public DateTime? LastStatusChangedAt { get; set; }

        public bool IsIsolationStation { get; set; }

        public bool IsActive { get; set; } = true;

        public int Version { get; set; }
    }
}
