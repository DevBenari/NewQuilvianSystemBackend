using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models
{
    [Table("MstTrainingCatalog", Schema = "public")]
    public class MstTrainingCatalog : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TrainingCategoryId { get; set; }

        public Guid? CertificationTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TrainingCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TrainingName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TrainingType { get; set; } = "Internal";
        // Internal, External, Orientation, Compliance, Clinical, Technical, Leadership.

        [Required]
        [MaxLength(50)]
        public string DeliveryMethod { get; set; } = "Classroom";
        // Classroom, Online, Hybrid, OnTheJob, Simulation, Workshop, Seminar.

        [MaxLength(200)]
        public string? DefaultProviderName { get; set; }

        public decimal DurationHours { get; set; } = 0;

        public int? ValidityMonths { get; set; }

        public bool IsMandatory { get; set; } = false;

        public bool RequiresAssessment { get; set; } = false;

        public decimal? MinimumPassingScore { get; set; }

        public bool IssuesCertificate { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstTrainingCategory? TrainingCategory { get; set; }

        public MstCertificationType? CertificationType { get; set; }

        public ICollection<MstMandatoryTrainingRule> MandatoryTrainingRules { get; set; }
            = new List<MstMandatoryTrainingRule>();
    }
}
