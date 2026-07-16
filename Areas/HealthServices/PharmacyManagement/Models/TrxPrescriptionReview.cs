using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("TrxPrescriptionReview", Schema = "public")]
    public class TrxPrescriptionReview : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PrescriptionId { get; set; }
        public int ReviewVersion { get; set; } = 1;
        public PrescriptionReviewStatus Status { get; set; } = PrescriptionReviewStatus.Pending;
        public Guid? ReviewedByPharmacistId { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool HasAdministrativeProblem { get; set; }
        public bool HasPharmaceuticalProblem { get; set; }
        public bool HasClinicalProblem { get; set; }
        public bool HasCompoundFormulaProblem { get; set; }
        public bool RequiresDoctorClarification { get; set; }

        [MaxLength(1000)]
        public string? GeneralNote { get; set; }

        [MaxLength(500)]
        public string PrescriptionSignatureSnapshot { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public TrxPrescription? Prescription { get; set; }
        public ICollection<TrxPrescriptionReviewItem> Items { get; set; }
            = new List<TrxPrescriptionReviewItem>();
        public ICollection<TrxPrescriptionClarification> Clarifications { get; set; }
            = new List<TrxPrescriptionClarification>();
    }
}
