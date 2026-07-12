using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstInsuranceTariff", Schema = "public")]
    public class MstInsuranceTariff : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid InsuranceProviderId { get; set; }

        [Required]
        public Guid TariffId { get; set; }

        public Guid? PatientClassId { get; set; }

        [Required, MaxLength(50)]
        public string InsuranceTariffCode { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string InsuranceTariffName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ExternalServiceCode { get; set; }

        [MaxLength(50)]
        public string? ExternalClassCode { get; set; }

        [MaxLength(100)]
        public string? BenefitPlanCode { get; set; }

        [MaxLength(150)]
        public string? BenefitPlanName { get; set; }

        public decimal ContractPrice { get; set; }
        public decimal? HospitalPriceSnapshot { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountPercent { get; set; }
        public bool IsUsingContractPrice { get; set; } = true;
        public bool IsNeedApproval { get; set; }
        public int Priority { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public int SortOrder { get; set; }

        [MaxLength(250)]
        public string? BillingInstruction { get; set; }

        [MaxLength(250)]
        public string? ClaimInstruction { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstInsuranceProvider? InsuranceProvider { get; set; }
        public MstTariff? Tariff { get; set; }
        public MstPatientClass? PatientClass { get; set; }
    }
}
