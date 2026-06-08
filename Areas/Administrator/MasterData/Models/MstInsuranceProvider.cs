using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstInsuranceProvider", Schema = "public")]
    public class MstInsuranceProvider : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string InsuranceProviderCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string InsuranceProviderName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? InsuranceGroupName { get; set; }

        [MaxLength(50)]
        public string ProviderType { get; set; } = "PrivateInsurance";
        // PrivateInsurance, TPA, GovernmentInsurance, CorporateInsurance, Other

        [MaxLength(50)]
        public string ClaimMethod { get; set; } = "Cashless";
        // Cashless, Reimbursement, GuaranteeLetter, Mixed

        [MaxLength(50)]
        public string? ExternalProviderCode { get; set; }

        [MaxLength(50)]
        public string? IntegrationCode { get; set; }

        [MaxLength(100)]
        public string? ContractNumber { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public bool IsUsingInsuranceTariffBook { get; set; } = true;

        public bool IsUsingHospitalTariff { get; set; } = false;

        public bool IsNeedEligibilityCheck { get; set; } = true;

        public bool IsNeedGuaranteeLetter { get; set; } = true;

        public bool IsNeedReferralLetter { get; set; } = false;

        public bool IsNeedApprovalForProcedure { get; set; } = true;

        public bool IsNeedApprovalForDrug { get; set; } = false;

        public bool IsCoverageLimitedByPlan { get; set; } = true;

        public bool IsAllowExcessPaymentByPatient { get; set; } = true;

        [MaxLength(100)]
        public string? PicName { get; set; }

        [MaxLength(30)]
        public string? PicPhoneNumber { get; set; }

        [MaxLength(30)]
        public string? PicWhatsAppNumber { get; set; }

        [MaxLength(200)]
        public string? PicEmail { get; set; }

        [MaxLength(500)]
        public string? OfficeAddress { get; set; }

        [MaxLength(500)]
        public string? LogoPath { get; set; }

        [MaxLength(250)]
        public string? BillingInstruction { get; set; }

        [MaxLength(250)]
        public string? ClaimInstruction { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
