using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    [Table("InpBedReservation", Schema = "public")]
    public class InpBedReservation : IdentityModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        [Required]
        public Guid BedId { get; set; }

        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public InpBedReservationStatus ReservationStatus { get; set; } = InpBedReservationStatus.Active;

        [Required]
        public Guid ReservedByUserId { get; set; }

        public DateTime? ReleasedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public InpEpisode? Episode { get; set; }

        public MstBed? Bed { get; set; }

        public ApplicationUser? ReservedByUser { get; set; }
    }
}
