using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    [Table("InpStatusHistory", Schema = "public")]
    public class InpStatusHistory : IdentityModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public int SequenceNumber { get; set; }

        public InpEpisodeStatus? FromStatus { get; set; }

        public InpEpisodeStatus ToStatus { get; set; }

        [Required]
        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty;

        public InpStatusChangeActorType ActorType { get; set; } = InpStatusChangeActorType.User;

        // Kosong bila perpindahan dilakukan sistem, bukan manusia.
        public Guid? ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string? Reason { get; set; }

        public bool IsActive { get; set; } = true;

        public InpEpisode? Episode { get; set; }

        public ApplicationUser? ChangedByUser { get; set; }
    }
}
