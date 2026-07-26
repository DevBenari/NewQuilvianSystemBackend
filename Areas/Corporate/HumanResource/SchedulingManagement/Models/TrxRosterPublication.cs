using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    [Table("TrxRosterPublication", Schema = "public")]
    public class TrxRosterPublication : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RosterPeriodId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PublicationNumber { get; set; } = string.Empty;

        public int VersionNumber { get; set; } = 1;

        [Required]
        [MaxLength(30)]
        public string PublicationStatus { get; set; } = "Draft";
        // Draft, Published, Superseded, Cancelled

        [MaxLength(30)]
        public string PublicationChannel { get; set; } = "Application";
        // Application, Email, WhatsApp, Print, API

        public DateTime? PublishedAt { get; set; }
        public Guid? PublishedByUserId { get; set; }

        [Column(TypeName = "jsonb")]
        public string? AudienceDefinitionJson { get; set; }

        [Column(TypeName = "jsonb")]
        public string? PublicationSnapshotJson { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public Guid? SupersededByPublicationId { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxRosterPeriod? RosterPeriod { get; set; }
        public ApplicationUser? PublishedByUser { get; set; }
        public TrxRosterPublication? SupersededByPublication { get; set; }
    }
}
