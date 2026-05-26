using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models
{
    [Table("MstWorkforceRequirement", Schema = "public")]
    public class MstWorkforceRequirement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public UserType UserType { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequirementCategory { get; set; } = string.Empty;
        // Document, Education, Training, Certification, License,
        // BankAccount, TransportAllowance, Payroll, Tax, Insurance

        [Required]
        [MaxLength(100)]
        public string RequirementCode { get; set; } = string.Empty;
        // KTP, NPWP, IJAZAH_TERAKHIR, STR, SIP, BLS,
        // BANK_PRIMARY, PAYROLL_PROFILE, TAX_PROFILE, BPJS_KESEHATAN

        [Required]
        [MaxLength(150)]
        public string RequirementName { get; set; } = string.Empty;

        public bool IsRequired { get; set; } = true;

        public bool IsMultipleAllowed { get; set; } = false;

        public bool IsFileRequired { get; set; } = true;

        public bool IsNumberRequired { get; set; } = false;

        public bool IsIssueDateRequired { get; set; } = false;

        public bool IsExpiredDateRequired { get; set; } = false;

        public bool IsVerificationRequired { get; set; } = true;

        public bool IsProfileRequired { get; set; } = false;

        [MaxLength(100)]
        public string? TargetEntityName { get; set; }
        // WfpDocument, WfpEducation, WfpCertification, WfpCredentialLicense,
        // WfpBankAccount, WfpTransportAllowanceProfile,
        // WfpPayrollProfile, WfpTaxProfile, WfpInsuranceProfile

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}