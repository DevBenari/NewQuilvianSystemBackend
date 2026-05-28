using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDrug", Schema = "public")]
    public class MstDrug : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DrugCategoryId { get; set; }

        public Guid? DefaultTariffId { get; set; }

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
        // 500 mg, 5 mg/ml, 1 g, etc

        [MaxLength(50)]
        public string? BaseUnit { get; set; }
        // tablet, vial, ampoule, bottle, strip

        [MaxLength(50)]
        public string? DispenseUnit { get; set; }
        // tablet, pcs, bottle, vial

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

        public MstTariff? DefaultTariff { get; set; }
    }
}
