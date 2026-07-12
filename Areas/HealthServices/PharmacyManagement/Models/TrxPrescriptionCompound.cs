using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("TrxPrescriptionCompound", Schema = "public")]
    public class TrxPrescriptionCompound : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PrescriptionId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CompoundName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CompoundForm { get; set; }

        public decimal TotalPackage { get; set; } = 1;

        public Guid? PackageUnitMeasurementId { get; set; }

        [MaxLength(100)]
        public string? PackageUnitNameSnapshot { get; set; }

        [MaxLength(30)]
        public string? PackageUnitSymbolSnapshot { get; set; }

        public decimal DosePerUse { get; set; } = 1;

        public Guid? DoseUnitMeasurementId { get; set; }

        [MaxLength(100)]
        public string? DoseUnitNameSnapshot { get; set; }

        [MaxLength(30)]
        public string? DoseUnitSymbolSnapshot { get; set; }

        [MaxLength(50)]
        public string? FrequencyCode { get; set; }

        [MaxLength(150)]
        public string? FrequencyText { get; set; }

        public decimal? FrequencyPerDay { get; set; }

        public decimal? DurationValue { get; set; }

        [MaxLength(30)]
        public string? DurationUnit { get; set; }

        public bool IsAsNeeded { get; set; }

        [MaxLength(250)]
        public string? AdministrationTime { get; set; }

        [MaxLength(500)]
        public string? Signa { get; set; }

        [MaxLength(1000)]
        public string? CompoundingInstruction { get; set; }

        [MaxLength(500)]
        public string? AdministrationInstruction { get; set; }

        [MaxLength(500)]
        public string? DoctorNote { get; set; }

        public int IngredientCount { get; set; }

        public decimal TotalPrice { get; set; }

        public decimal CoveredAmount { get; set; }

        public decimal PatientPayAmount { get; set; }

        public bool IsNeedApproval { get; set; }

        public bool IsApproved { get; set; }

        public bool IsNeedGuaranteeLetter { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPrescription? Prescription { get; set; }

        public MstMeasurement? PackageUnitMeasurement { get; set; }

        public MstMeasurement? DoseUnitMeasurement { get; set; }

        public ICollection<TrxPrescriptionCompoundItem> Items { get; set; }
            = new List<TrxPrescriptionCompoundItem>();
    }
}
