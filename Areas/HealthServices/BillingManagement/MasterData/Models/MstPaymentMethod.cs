using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models
{
    [Table("MstPaymentMethod", Schema = "public")]
    public class MstPaymentMethod : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string PaymentMethodCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PaymentMethodName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string PaymentMethodType { get; set; } = "Cash";
        // Cash, Debit, CreditCard, Transfer, QRIS, Insurance, CompanyGuarantor, Membership, Other

        [MaxLength(100)]
        public string? PaymentGroupName { get; set; }

        public bool IsCash { get; set; } = false;

        public bool IsBankTransfer { get; set; } = false;

        public bool IsCardPayment { get; set; } = false;

        public bool IsQris { get; set; } = false;

        public bool IsInsurance { get; set; } = false;

        public bool IsCompanyGuarantor { get; set; } = false;

        public bool IsMembership { get; set; } = false;

        public bool IsNeedReferenceNumber { get; set; } = false;

        public bool IsNeedApproval { get; set; } = false;

        public bool IsNeedAttachment { get; set; } = false;

        public bool IsAvailableForRegistration { get; set; } = true;

        public bool IsAvailableForBilling { get; set; } = true;

        public bool IsAvailableForRefund { get; set; } = true;

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(100)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(200)]
        public string? BankAccountName { get; set; }

        [MaxLength(100)]
        public string? MerchantId { get; set; }

        [MaxLength(100)]
        public string? TerminalId { get; set; }

        [MaxLength(50)]
        public string? ExternalPaymentCode { get; set; }

        [MaxLength(50)]
        public string? IntegrationCode { get; set; }

        public decimal AdminFeeAmount { get; set; } = 0;

        public decimal AdminFeePercent { get; set; } = 0;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
