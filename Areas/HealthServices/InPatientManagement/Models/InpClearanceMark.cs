using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    [Table("InpClearanceMark", Schema = "public")]
    public class InpClearanceMark : IdentityModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        [Required]
        public Guid ClearanceItemId { get; set; }

        public DateTime MarkedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid MarkedByUserId { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public bool IsActive { get; set; } = true;

        public InpEpisode? Episode { get; set; }

        public MstInpatientClearanceItem? ClearanceItem { get; set; }

        public ApplicationUser? MarkedByUser { get; set; }
    }
}
