using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDrugSupplier", Schema = "public")]
    public class MstDrugSupplier : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DrugId { get; set; }

        [Required]
        public Guid SupplierId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DrugSupplierCode { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SupplierDrugCode { get; set; }

        [MaxLength(200)]
        public string? SupplierDrugName { get; set; }

        public Guid? PurchaseUnitMeasurementId { get; set; }

        public decimal MinimumOrderQuantity { get; set; } = 1;

        public decimal OrderMultipleQuantity { get; set; } = 1;

        public decimal? MaximumOrderQuantity { get; set; }

        public decimal? MinimumPurchaseAmount { get; set; }

        public decimal DefaultPurchasePrice { get; set; } = 0;

        public decimal? LastPurchasePrice { get; set; }

        public decimal? ContractPurchasePrice { get; set; }

        public decimal? DiscountPercent { get; set; }

        public decimal? TaxPercent { get; set; }

        public int LeadTimeDays { get; set; } = 0;

        public bool IsPreferredSupplier { get; set; } = false;

        public bool IsContractSupplier { get; set; } = false;

        public bool IsDefaultForPurchase { get; set; } = false;

        public bool IsAllowPurchase { get; set; } = true;

        public bool IsRequireQuotation { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstDrug? Drug { get; set; }

        public MstSupplier? Supplier { get; set; }

        public MstMeasurement? PurchaseUnitMeasurement { get; set; }
    }

}
