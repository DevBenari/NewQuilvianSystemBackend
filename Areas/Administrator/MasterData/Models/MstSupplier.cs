using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstSupplier", Schema = "public")]
    public class MstSupplier : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string SupplierCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string SupplierName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? LegalName { get; set; }

        [Required]
        [MaxLength(50)]
        public string SupplierType { get; set; } = "General";
        // General, Pharmacy, MedicalDevice, Laboratory, Consumable, Distributor, Principal, Manufacturer, Other

        [MaxLength(100)]
        public string? SupplierGroupName { get; set; }

        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        [MaxLength(100)]
        public string? BusinessLicenseNumber { get; set; }

        [MaxLength(100)]
        public string? ContactPersonName { get; set; }

        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        [MaxLength(50)]
        public string? WhatsAppNumber { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(150)]
        public string? Website { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? CityName { get; set; }

        [MaxLength(100)]
        public string? ProvinceName { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(100)]
        public string? CountryName { get; set; }

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(100)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(150)]
        public string? BankAccountName { get; set; }

        public int PaymentTermDays { get; set; } = 0;

        public int LeadTimeDays { get; set; } = 0;

        public decimal MinimumPurchaseAmount { get; set; } = 0;

        public decimal? CreditLimitAmount { get; set; }

        public bool IsTaxable { get; set; } = false;

        public decimal? TaxPercent { get; set; }

        public bool IsPrincipal { get; set; } = false;

        public bool IsDistributor { get; set; } = false;

        public bool IsManufacturer { get; set; } = false;

        public bool IsPharmacySupplier { get; set; } = true;

        public bool IsMedicalDeviceSupplier { get; set; } = false;

        public bool IsLaboratorySupplier { get; set; } = false;

        public bool IsConsumableSupplier { get; set; } = false;

        public bool IsPreferredSupplier { get; set; } = false;

        public bool IsBlacklisted { get; set; } = false;

        [MaxLength(250)]
        public string? BlacklistReason { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<MstDrugSupplier> DrugSuppliers { get; set; } = new List<MstDrugSupplier>();
    }

}
