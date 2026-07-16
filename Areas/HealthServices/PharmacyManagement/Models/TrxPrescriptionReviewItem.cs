using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("TrxPrescriptionReviewItem", Schema = "public")]
    public class TrxPrescriptionReviewItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PrescriptionReviewId { get; set; }
        public Guid? CriterionId { get; set; }
        public PrescriptionReviewCategory Category { get; set; }

        [Required, MaxLength(80)]
        public string CriterionCodeSnapshot { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string CriterionNameSnapshot { get; set; } = string.Empty;

        public PrescriptionReviewResult Result { get; set; } = PrescriptionReviewResult.NotReviewed;
        public PrescriptionIssueSeverity Severity { get; set; } = PrescriptionIssueSeverity.Warning;

        [MaxLength(1000)]
        public string? Finding { get; set; }

        [MaxLength(1000)]
        public string? Recommendation { get; set; }

        public bool IsSystemDetected { get; set; }

        [MaxLength(100)]
        public string? SystemRuleCode { get; set; }

        public Guid? PrescriptionItemId { get; set; }
        public Guid? PrescriptionCompoundId { get; set; }
        public Guid? PrescriptionCompoundItemId { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxPrescriptionReview? PrescriptionReview { get; set; }
        public MstPrescriptionReviewCriterion? Criterion { get; set; }
    }
}
