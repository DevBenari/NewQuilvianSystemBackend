using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDrugStockPolicy", Schema = "public")]
    public class MstDrugStockPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DrugId { get; set; }

        public Guid? StorageLocationId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        [Required]
        public Guid StockUnitMeasurementId { get; set; }

        [Required]
        [MaxLength(50)]
        public string StockPolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string StockPolicyName { get; set; } = string.Empty;

        public decimal MinimumStockQuantity { get; set; } = 0;

        public decimal MaximumStockQuantity { get; set; } = 0;

        public decimal ReorderPointQuantity { get; set; } = 0;

        public decimal ReorderQuantity { get; set; } = 0;

        public decimal SafetyStockQuantity { get; set; } = 0;

        public decimal CriticalStockQuantity { get; set; } = 0;

        public int LeadTimeDays { get; set; } = 0;

        public int ExpiryWarningDays { get; set; } = 90;

        public int NearExpiryWarningDays { get; set; } = 30;

        public bool IsAutoReorderEnabled { get; set; } = false;

        public bool IsAllowNegativeStock { get; set; } = false;

        public bool IsBatchRequired { get; set; } = true;

        public bool IsExpiryDateRequired { get; set; } = true;

        public bool IsStockOpnameRequired { get; set; } = true;

        public int StockOpnameIntervalDays { get; set; } = 30;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstDrug? Drug { get; set; }

        public MstDrugStorageLocation? StorageLocation { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public MstMeasurement? StockUnitMeasurement { get; set; }
    }
}