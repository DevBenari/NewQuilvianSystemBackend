using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("TrxPrescriptionPreparation", Schema = "public")]
    public class TrxPrescriptionPreparation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PrescriptionId { get; set; }
        public PrescriptionPreparationStatus Status { get; set; } = PrescriptionPreparationStatus.Pending;
        public Guid? PreparedByUserId { get; set; }
        public DateTime? PreparationStartedAt { get; set; }
        public DateTime? PreparationCompletedAt { get; set; }

        [MaxLength(1000)]
        public string? PreparationNote { get; set; }

        public bool IsActive { get; set; } = true;
        public TrxPrescription? Prescription { get; set; }
        public ICollection<TrxPrescriptionPreparationItem> Items { get; set; }
            = new List<TrxPrescriptionPreparationItem>();
    }

    [Table("TrxPrescriptionPreparationItem", Schema = "public")]
    public class TrxPrescriptionPreparationItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PrescriptionPreparationId { get; set; }
        public Guid? PrescriptionItemId { get; set; }
        public Guid? PrescriptionCompoundItemId { get; set; }
        public Guid DrugId { get; set; }
        public decimal TheoreticalQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal WasteQuantity { get; set; }
        public Guid? MeasurementId { get; set; }

        [MaxLength(100)]
        public string? MeasurementNameSnapshot { get; set; }

        [MaxLength(100)]
        public string? BatchNumber { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public TrxPrescriptionPreparation? PrescriptionPreparation { get; set; }
    }
}
