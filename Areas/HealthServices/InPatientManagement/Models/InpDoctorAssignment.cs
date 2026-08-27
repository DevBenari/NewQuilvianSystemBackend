using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    [Table("InpDoctorAssignment", Schema = "public")]
    public class InpDoctorAssignment : IdentityModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        public int SequenceNumber { get; set; }

        public DateTime StartDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? EndDateTime { get; set; }

        [Required]
        public Guid AssignedByUserId { get; set; }

        [MaxLength(500)]
        public string? HandoverReason { get; set; }

        public bool IsActive { get; set; } = true;

        public InpEpisode? Episode { get; set; }

        public MstDoctor? Doctor { get; set; }

        public ApplicationUser? AssignedByUser { get; set; }
    }
}
