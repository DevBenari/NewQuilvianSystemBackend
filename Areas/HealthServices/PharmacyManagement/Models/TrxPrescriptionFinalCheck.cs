using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("TrxPrescriptionFinalCheck", Schema = "public")]
    public class TrxPrescriptionFinalCheck : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PrescriptionId { get; set; }
        public PrescriptionFinalCheckStatus Status { get; set; } = PrescriptionFinalCheckStatus.Pending;
        public Guid? CheckedByUserId { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [MaxLength(1000)]
        public string? CheckNote { get; set; }

        public bool IsActive { get; set; } = true;
        public TrxPrescription? Prescription { get; set; }
        public ICollection<TrxPrescriptionFinalCheckItem> Items { get; set; }
            = new List<TrxPrescriptionFinalCheckItem>();
    }

    [Table("TrxPrescriptionFinalCheckItem", Schema = "public")]
    public class TrxPrescriptionFinalCheckItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PrescriptionFinalCheckId { get; set; }

        [Required, MaxLength(80)]
        public string CriterionCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string CriterionName { get; set; } = string.Empty;

        public PrescriptionReviewResult Result { get; set; } = PrescriptionReviewResult.NotReviewed;

        [MaxLength(1000)]
        public string? Finding { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public TrxPrescriptionFinalCheck? PrescriptionFinalCheck { get; set; }
    }
}
