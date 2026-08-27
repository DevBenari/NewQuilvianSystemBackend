using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    [Table("InpFinancialClearance", Schema = "public")]
    public class InpFinancialClearance : IdentityModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public int SequenceNumber { get; set; }

        public InpFinancialClearanceStatus ClearanceStatus { get; set; } = InpFinancialClearanceStatus.Pending;

        public DateTime MarkedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid MarkedByUserId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Note { get; set; } = string.Empty;

        // Selalu true selama MVP, dan wajib ditampilkan pada layar dan laporan — RWI-RULE-028.
        public bool IsManualMarking { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public InpEpisode? Episode { get; set; }

        public ApplicationUser? MarkedByUser { get; set; }
    }
}
