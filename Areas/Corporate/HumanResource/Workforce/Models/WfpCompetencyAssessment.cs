using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpCompetencyAssessment", Schema = "public")]
    public class WfpCompetencyAssessment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid CompetencyId { get; set; }

        [Required]
        public DateTime AssessmentDate { get; set; }

        public CompetencyLevel CompetencyLevel { get; set; } = CompetencyLevel.None;

        public CompetencyAssessmentResultStatus ResultStatus { get; set; } = CompetencyAssessmentResultStatus.NotAssessed;

        public Guid? AssessedByUserId { get; set; }

        public DateTime? ExpiredDate { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public MstCompetency? Competency { get; set; }

        public ApplicationUser? AssessedByUser { get; set; }
    }
}
