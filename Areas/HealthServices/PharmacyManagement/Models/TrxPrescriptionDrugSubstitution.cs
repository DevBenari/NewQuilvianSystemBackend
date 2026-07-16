using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("TrxPrescriptionDrugSubstitution", Schema = "public")]
    public class TrxPrescriptionDrugSubstitution : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PrescriptionId { get; set; }
        public Guid? PrescriptionItemId { get; set; }
        public Guid? PrescriptionCompoundItemId { get; set; }
        public Guid OriginalDrugId { get; set; }
        public Guid SubstituteDrugId { get; set; }

        [Required, MaxLength(100)]
        public string ReasonCode { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? ReasonNote { get; set; }

        public Guid ProposedByPharmacistId { get; set; }
        public DateTime ProposedAt { get; set; }
        public PrescriptionSubstitutionApprovalStatus ApprovalStatus { get; set; }
            = PrescriptionSubstitutionApprovalStatus.Proposed;
        public Guid? ApprovedByDoctorId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        [MaxLength(1000)]
        public string? DoctorApprovalNote { get; set; }

        public bool IsActive { get; set; } = true;
        public TrxPrescription? Prescription { get; set; }
    }
}
