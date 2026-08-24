using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    [Table("InpDischargeSummary", Schema = "public")]
    public class InpDischargeSummary : IdentityModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string PrimaryDiagnosisText { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? SecondaryDiagnosisText { get; set; }

        [MaxLength(2000)]
        public string? ProcedureSummary { get; set; }

        [MaxLength(2000)]
        public string? DischargeMedicationNote { get; set; }

        [MaxLength(2000)]
        public string? FollowUpInstruction { get; set; }

        [MaxLength(250)]
        public string? ReferralDestination { get; set; }

        [MaxLength(4000)]
        public string? ClinicalSummary { get; set; }

        public DateTime? SignedAt { get; set; }

        public Guid? SignedByDoctorId { get; set; }

        public bool IsActive { get; set; } = true;

        public InpEpisode? Episode { get; set; }

        public MstDoctor? SignedByDoctor { get; set; }

        public ICollection<InpDischargeSummaryRevision> Revisions { get; set; } = new List<InpDischargeSummaryRevision>();
    }
}
