using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstInsuranceTariff", Schema = "public")]
    public class MstInsuranceTariff : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid InsuranceProviderId { get; set; }

        public Guid? TariffId { get; set; }

        public Guid? DrugId { get; set; }

        public Guid? ProcedureId { get; set; }

        public Guid? TariffCategoryId { get; set; }

        public Guid? PatientClassId { get; set; }

        [Required]
        [MaxLength(50)]
        public string InsuranceTariffCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string InsuranceTariffName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ExternalServiceCode { get; set; }

        [MaxLength(50)]
        public string? ExternalClassCode { get; set; }

        [MaxLength(100)]
        public string? BenefitPlanCode { get; set; }

        [MaxLength(150)]
        public string? BenefitPlanName { get; set; }

        [MaxLength(100)]
        public string? PatientClassName { get; set; }

        [MaxLength(100)]
        public string? ProviderName { get; set; }

        public decimal ContractPrice { get; set; } = 0;

        public decimal? HospitalPriceSnapshot { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? DiscountPercent { get; set; }

        public bool IsUsingContractPrice { get; set; } = true;

        public bool IsSurgeryRelated { get; set; } = false;

        public bool IsRoomCharge { get; set; } = false;

        public bool IsDrug { get; set; } = false;

        public bool IsConsumable { get; set; } = false;

        public bool IsProcedure { get; set; } = false;

        public bool IsLaboratory { get; set; } = false;

        public bool IsRadiology { get; set; } = false;

        public bool IsNeedApproval { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? BillingInstruction { get; set; }

        [MaxLength(250)]
        public string? ClaimInstruction { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstInsuranceProvider? InsuranceProvider { get; set; }

        public MstTariff? Tariff { get; set; }

        public MstDrug? Drug { get; set; }

        public MstProcedure? Procedure { get; set; }

        public MstTariffCategory? TariffCategory { get; set; }

        public MstPatientClass? PatientClass { get; set; }
    }
}
