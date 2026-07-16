using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("TrxPrescriptionClarification", Schema = "public")]
    public class TrxPrescriptionClarification : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PrescriptionId { get; set; }
        public Guid PrescriptionReviewId { get; set; }
        public Guid? PrescriptionReviewItemId { get; set; }
        public Guid? PrescriptionItemId { get; set; }
        public Guid? PrescriptionCompoundId { get; set; }
        public Guid? PrescriptionCompoundItemId { get; set; }

        [Required, MaxLength(100)]
        public string ProblemCode { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string ProblemDescription { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? PharmacistRecommendation { get; set; }

        public PrescriptionIssueSeverity Severity { get; set; } = PrescriptionIssueSeverity.Warning;
        public PrescriptionClarificationStatus Status { get; set; } = PrescriptionClarificationStatus.Open;
        public Guid RequestedByPharmacistId { get; set; }
        public DateTime RequestedAt { get; set; }
        public Guid? RespondedByDoctorId { get; set; }
        public DateTime? RespondedAt { get; set; }

        [MaxLength(2000)]
        public string? DoctorResponse { get; set; }

        public Guid? ClosedByUserId { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxPrescription? Prescription { get; set; }
        public TrxPrescriptionReview? PrescriptionReview { get; set; }
        public TrxPrescriptionReviewItem? PrescriptionReviewItem { get; set; }
    }
}
