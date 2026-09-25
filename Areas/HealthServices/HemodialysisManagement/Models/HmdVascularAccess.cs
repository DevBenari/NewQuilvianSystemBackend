using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>Jenis, lokasi, dan kondisi akses pembuluh darah yang dipakai cuci darah.</summary>
    [Table("HmdVascularAccess", Schema = "public")]
    public class HmdVascularAccess : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public HmdEpisode? Episode { get; set; }

        public HmdVascularAccessType AccessType { get; set; }

        /// <summary>Lokasi anatomi, misalnya "lengan kiri" atau "jugular kanan".</summary>
        [Required]
        [MaxLength(200)]
        public string AccessSite { get; set; } = string.Empty;

        public HmdVascularAccessStatus AccessStatus { get; set; } = HmdVascularAccessStatus.Usable;

        public bool IsPrimary { get; set; }

        public DateOnly? EstablishedDate { get; set; }

        public DateTime? LastAssessedAt { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? ConditionNote { get; set; }
    }
}
