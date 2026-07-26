using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Enums.HumanResource;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models
{
    [Table("WfpCompetencyAssessment", Schema = "public")]
    public class WfpCompetencyAssessment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public Guid CompetencyId { get; set; }

        public Guid? SourceTrainingAssessmentId { get; set; }

        public Guid? SourceTrainingResultId { get; set; }

        public DateTime AssessmentDate { get; set; }

        public CompetencyLevel CompetencyLevel { get; set; }

        public CompetencyAssessmentResultStatus ResultStatus { get; set; }

        public Guid? AssessedByUserId { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public decimal? Score { get; set; }

        public decimal? MaximumScore { get; set; }

        [MaxLength(1000)]
        public string? FilePath { get; set; }

        [MaxLength(150)]
        public string? FileContentType { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsVerified { get; set; } = false;

        public Guid? VerifiedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstCompetency? Competency { get; set; }
        public TrxTrainingAssessment? SourceTrainingAssessment { get; set; }
        public TrxTrainingResult? SourceTrainingResult { get; set; }
        public ApplicationUser? AssessedByUser { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
