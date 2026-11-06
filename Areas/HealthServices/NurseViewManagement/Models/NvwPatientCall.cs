using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models
{
    [Table("NvwPatientCall", Schema = "public")]
    public class NvwPatientCall : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BedId { get; set; }

        public MstBed? Bed { get; set; }

        [Required]
        public Guid BedTabletId { get; set; }

        public NvwBedTablet? BedTablet { get; set; }

        [Required]
        public Guid InpBedPlacementId { get; set; }

        public InpBedPlacement? BedPlacement { get; set; }

        [Required]
        public Guid InpEpisodeId { get; set; }

        [Required]
        [MaxLength(20)]
        public string FloorCode { get; set; } = string.Empty;

        public NvwPatientCallPriority Priority { get; set; }

        public NvwPatientCallStatus CallStatus { get; set; } = NvwPatientCallStatus.Waiting;

        public DateTime FirstPressedAt { get; set; }

        public DateTime LastPressedAt { get; set; }

        public int PressCount { get; set; } = 1;

        public DateTime? EscalatedAt { get; set; }

        public Guid? ClaimedByUserId { get; set; }

        public DateTime? ClaimedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public Guid? ClosedByUserId { get; set; }

        public NvwPatientCallCloseReason? CloseReason { get; set; }

        [MaxLength(250)]
        public string? CloseNote { get; set; }

        public bool IsBedsideConfirmed { get; set; }

        public int Version { get; set; }

        public ICollection<NvwPatientCallEvent> Events { get; set; } = new List<NvwPatientCallEvent>();
    }
}
