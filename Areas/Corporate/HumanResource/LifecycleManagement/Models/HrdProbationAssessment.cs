using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("HrdProbationAssessment", Schema = "public")]
    public class HrdProbationAssessment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string AssessmentNumber { get; set; } = string.Empty;

        [Required]
        public Guid ProbationId { get; set; }

        public ProbationResult Result { get; set; }

        [Required]
        [MaxLength(2000)]
        public string ReasonText { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? EvidenceStoragePath { get; set; }

        [MaxLength(250)]
        public string? EvidenceFileName { get; set; }

        [MaxLength(150)]
        public string? EvidenceContentType { get; set; }

        public ProbationAssessmentStatus AssessmentStatus { get; set; } = ProbationAssessmentStatus.Submitted;

        [Required]
        public Guid SubmittedByUserId { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public Guid? WorkflowInstanceId { get; set; }

        public Guid? DecidedByUserId { get; set; }

        public DateTime? DecidedAt { get; set; }

        [MaxLength(1000)]
        public string? DecisionNote { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public HrdProbation? Probation { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? DecidedByUser { get; set; }
    }
}
