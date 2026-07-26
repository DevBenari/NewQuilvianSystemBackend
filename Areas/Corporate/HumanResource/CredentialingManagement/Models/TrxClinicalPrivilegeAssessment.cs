using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxClinicalPrivilegeAssessment", Schema = "public")]
    public class TrxClinicalPrivilegeAssessment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ClinicalPrivilegeRequestId { get; set; }

        public Guid? CompetencyId { get; set; }

        public Guid? AssessorUserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AssessmentType { get; set; } = "Competency";

        public DateTime AssessmentDate { get; set; }

        public decimal? AssessmentScore { get; set; }

        [Required]
        [MaxLength(30)]
        public string AssessmentResult { get; set; } = "Pending";

        public string? CompetencyResultJson { get; set; }

        [MaxLength(2000)]
        public string? Recommendation { get; set; }

        [MaxLength(2000)]
        public string? Restrictions { get; set; }

        public bool RequiresSupervision { get; set; } = false;

        public DateTime? ValidUntil { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxClinicalPrivilegeRequest? ClinicalPrivilegeRequest { get; set; }
        public MstCompetency? Competency { get; set; }
        public ApplicationUser? AssessorUser { get; set; }

    }
}
