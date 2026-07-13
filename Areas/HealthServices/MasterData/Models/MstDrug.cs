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

        public string? Strength { get; set; }
        // Display text dapat berisi komposisi panjang, misalnya obat kombinasi multivitamin.
        // Dipetakan sebagai PostgreSQL text melalui MstDrugConfiguration.

        public decimal? StrengthValue { get; set; }
        // Numeric strength, contoh: 500

        public Guid? StrengthMeasurementId { get; set; }
        // contoh: mg, g, ml, mg/ml jika tersedia di MstMeasurement

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

        public bool IsPrescribable { get; set; } = true;
        // Menentukan apakah obat dapat dipilih pada transaksi resep dokter.

        public bool IsNeedApproval { get; set; } = false;

        // Narasi klinis dipetakan sebagai PostgreSQL text agar label resmi tidak terpotong.
        public string? Indication { get; set; }

        public string? Contraindication { get; set; }

        public string? SideEffect { get; set; }

        public string? WarningPrecaution { get; set; }

        public string? DosageInformation { get; set; }

        public string? DrugInteraction { get; set; }

        public string? AdministrationInstruction { get; set; }

        public string? StorageInstruction { get; set; }

        // Nama property dipertahankan agar tidak breaking terhadap API/frontend lama.
        // Isinya dapat berupa narasi risiko kehamilan (PLLR), bukan hanya kategori A/B/C/D/X.
        public string? PregnancyCategory { get; set; }

        public string? LactationNote { get; set; }

        public string? PediatricNote { get; set; }

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