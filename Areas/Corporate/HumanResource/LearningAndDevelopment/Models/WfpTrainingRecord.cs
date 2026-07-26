using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models
{
    [Table("WfpTrainingRecord", Schema = "public")]
    public class WfpTrainingRecord : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public Guid? TrainingCatalogId { get; set; }

        public Guid? TrainingCategoryId { get; set; }

        public Guid? MandatoryTrainingRuleId { get; set; }

        public Guid? TrainingParticipantId { get; set; }

        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(50)]
        public string TrainingType { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string TrainingName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Organizer { get; set; }

        [MaxLength(500)]
        public string? Location { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(150)]
        public string? CertificateNumber { get; set; }

        public decimal CreditPoint { get; set; } = 0m;

        [MaxLength(1000)]
        public string? FilePath { get; set; }

        [MaxLength(150)]
        public string? FileContentType { get; set; }

        public bool IsVerified { get; set; } = false;

        public Guid? VerifiedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public bool IsMandatory { get; set; } = false;

        public bool IsExternalTraining { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstTrainingCatalog? TrainingCatalog { get; set; }
        public MstTrainingCategory? TrainingCategory { get; set; }
        public MstMandatoryTrainingRule? MandatoryTrainingRule { get; set; }
        public TrxTrainingParticipant? TrainingParticipant { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
