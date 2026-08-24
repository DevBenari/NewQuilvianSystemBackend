using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    [Table("InpCorrectionSession", Schema = "public")]
    public class InpCorrectionSession : IdentityModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public int SequenceNumber { get; set; }

        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid OpenedByUserId { get; set; }

        [Required]
        [MaxLength(500)]
        public string OpenReason { get; set; } = string.Empty;

        public DateTime? ClosedAt { get; set; }

        public Guid? ClosedByUserId { get; set; }

        [MaxLength(4000)]
        public string? ChangedFieldSummary { get; set; }

        public bool IsActive { get; set; } = true;

        public InpEpisode? Episode { get; set; }

        public ApplicationUser? OpenedByUser { get; set; }

        public ApplicationUser? ClosedByUser { get; set; }

        public ICollection<InpDischargeSummaryRevision> SummaryRevisions { get; set; } = new List<InpDischargeSummaryRevision>();
    }
}
