using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDrugUnitConversion", Schema = "public")]
    public class MstDrugUnitConversion : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DrugId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ConversionCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ConversionName { get; set; } = string.Empty;

        [Required]
        public Guid FromMeasurementId { get; set; }

        [Required]
        public Guid ToMeasurementId { get; set; }

        public decimal FromQuantity { get; set; } = 1;

        public decimal ToQuantity { get; set; } = 1;

        public decimal ConversionFactor { get; set; } = 1;
        // Contoh:
        // 1 strip = 10 tablet
        // FromQuantity = 1
        // ToQuantity = 10
        // ConversionFactor = 10

        [Required]
        [MaxLength(50)]
        public string ConversionType { get; set; } = "General";
        // General, PurchaseToStock, StockToDispense, DispenseToBase, StrengthToBase, DoseToBase, Compound

        public bool IsDefault { get; set; } = false;

        public bool IsBidirectional { get; set; } = true;

        public bool IsForPurchase { get; set; } = false;

        public bool IsForStock { get; set; } = true;

        public bool IsForDispensing { get; set; } = true;

        public bool IsForPrescription { get; set; } = true;

        public bool IsForCompound { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstDrug? Drug { get; set; }

        public MstMeasurement? FromMeasurement { get; set; }

        public MstMeasurement? ToMeasurement { get; set; }
    }
}