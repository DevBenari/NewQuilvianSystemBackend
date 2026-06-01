using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDrug", Schema = "public")]
    public class MstDrug : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DrugCategoryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DrugCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DrugName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? GenericName { get; set; }

        [MaxLength(200)]
        public string? BrandName { get; set; }

        [MaxLength(100)]
        public string? ManufacturerName { get; set; }

        [MaxLength(100)]
        public string? DrugForm { get; set; }
        // Tablet, Capsule, Syrup, Injection, Cream, Drop, Inhaler, Other

        [MaxLength(100)]
        public string? Strength { get; set; }
        // Display text: 500 mg, 5 mg/ml, 1 g, etc

        public decimal? StrengthValue { get; set; }
        // Numeric strength, contoh: 500

        public Guid? StrengthMeasurementId { get; set; }
        // contoh: mg, g, ml, mg/ml jika tersedia di MstMeasurement

        [MaxLength(50)]
        public string? BaseUnit { get; set; }
        // Legacy/display: tablet, vial, ampoule, bottle, strip

        [MaxLength(50)]
        public string? DispenseUnit { get; set; }
        // Legacy/display: tablet, pcs, bottle, vial

        public Guid? BaseUnitMeasurementId { get; set; }
        // Satuan terkecil untuk perhitungan stok/racikan.
        // Contoh: tablet, ml, vial, ampul.

        public Guid? DispenseUnitMeasurementId { get; set; }
        // Satuan default saat diberikan ke pasien.
        // Contoh: tablet, kapsul, botol, vial.

        public Guid? PurchaseUnitMeasurementId { get; set; }
        // Satuan pembelian.
        // Contoh: box, strip, bottle.

        public Guid? StockUnitMeasurementId { get; set; }
        // Satuan tampilan stok default.
        // Bisa sama dengan BaseUnitMeasurementId.

        public Guid? DefaultDoseUnitMeasurementId { get; set; }
        // Satuan default dosis resep.
        // Contoh: mg, tablet, ml.

        [MaxLength(50)]
        public string? Route { get; set; }
        // Oral, IV, IM, SC, Topical, Inhalation

        public bool IsFormulary { get; set; } = true;

        public bool IsGeneric { get; set; } = false;

        public bool IsAntibiotic { get; set; } = false;

        public bool IsNarcotic { get; set; } = false;

        public bool IsPsychotropic { get; set; } = false;

        public bool IsHighAlert { get; set; } = false;

        public bool IsChronicDiseaseDrug { get; set; } = false;

        public bool IsVaccine { get; set; } = false;

        public bool IsConsumable { get; set; } = false;

        public bool IsCompoundIngredientAllowed { get; set; } = true;
        // Boleh dipakai sebagai bahan racikan.

        public bool IsStockManaged { get; set; } = true;
        // Jika false, item tidak dihitung stoknya, misalnya jasa/obat tertentu.

        public bool IsBatchTracked { get; set; } = true;
        // Untuk lot/batch obat.

        public bool IsExpiryDateTracked { get; set; } = true;
        // Untuk expired date obat.

        public bool IsAllowFractionalDispense { get; set; } = false;
        // Contoh true untuk syrup ml, false untuk tablet utuh.

        public bool IsNeedPrescription { get; set; } = true;

        public bool IsCoveredByInsuranceDefault { get; set; } = true;

        public bool IsNeedApproval { get; set; } = false;

        public decimal DefaultPrice { get; set; } = 0;

        public decimal? InsurancePrice { get; set; }

        public decimal? MemberPrice { get; set; }

        public decimal? CompanyPrice { get; set; }

        [MaxLength(1000)]
        public string? Indication { get; set; }

        [MaxLength(1000)]
        public string? Contraindication { get; set; }

        [MaxLength(1000)]
        public string? SideEffect { get; set; }

        [MaxLength(1000)]
        public string? WarningPrecaution { get; set; }

        [MaxLength(1000)]
        public string? DosageInformation { get; set; }

        [MaxLength(1000)]
        public string? DrugInteraction { get; set; }

        [MaxLength(500)]
        public string? AdministrationInstruction { get; set; }

        [MaxLength(500)]
        public string? StorageInstruction { get; set; }

        [MaxLength(100)]
        public string? PregnancyCategory { get; set; }

        [MaxLength(250)]
        public string? LactationNote { get; set; }

        [MaxLength(250)]
        public string? PediatricNote { get; set; }

        [MaxLength(250)]
        public string? GeriatricNote { get; set; }

        [MaxLength(50)]
        public string? ExternalDrugCode { get; set; }

        [MaxLength(50)]
        public string? IntegrationCode { get; set; }

        [MaxLength(50)]
        public string? BpomRegistrationNumber { get; set; }

        [MaxLength(50)]
        public string? NationalDrugCode { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstDrugCategory? DrugCategory { get; set; }

        public MstMeasurement? StrengthMeasurement { get; set; }

        public MstMeasurement? BaseUnitMeasurement { get; set; }

        public MstMeasurement? DispenseUnitMeasurement { get; set; }

        public MstMeasurement? PurchaseUnitMeasurement { get; set; }

        public MstMeasurement? StockUnitMeasurement { get; set; }

        public MstMeasurement? DefaultDoseUnitMeasurement { get; set; }
    }
}