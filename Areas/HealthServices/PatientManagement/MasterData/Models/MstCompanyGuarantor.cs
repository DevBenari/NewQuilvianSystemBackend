using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models
{
    [Table("MstCompanyGuarantor", Schema = "public")]
    public class MstCompanyGuarantor : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string CompanyGuarantorCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CompanyGuarantorName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CompanyGroupName { get; set; }

        [MaxLength(50)]
        public string GuarantorType { get; set; } = "Corporate";
        // Corporate, Government, Foundation, School, Other

        [MaxLength(50)]
        public string BillingMethod { get; set; } = "Invoice";
        // Invoice, Deposit, Mixed

        [MaxLength(50)]
        public string? ExternalCompanyCode { get; set; }

        [MaxLength(50)]
        public string? IntegrationCode { get; set; }

        [MaxLength(100)]
        public string? ContractNumber { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public bool IsUsingCompanyTariffBook { get; set; } = true;

        public bool IsUsingHospitalTariff { get; set; } = false;

        public bool IsNeedGuaranteeLetter { get; set; } = true;

        public bool IsNeedEmployeeVerification { get; set; } = true;

        public bool IsNeedApprovalForProcedure { get; set; } = true;

        public bool IsNeedApprovalForDrug { get; set; } = false;

        public bool IsCoverageLimitedByEmployeeGrade { get; set; } = true;

        public bool IsAllowExcessPaymentByPatient { get; set; } = true;

        public decimal? CreditLimitAmount { get; set; }

        public decimal? CurrentOutstandingAmount { get; set; }

        public int PaymentDueDays { get; set; } = 30;

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
