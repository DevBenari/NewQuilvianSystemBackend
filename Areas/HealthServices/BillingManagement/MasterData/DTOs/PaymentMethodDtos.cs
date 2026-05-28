using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.DTOs
{
    public class PaymentMethodSummaryResponse
    {
        public int TotalPaymentMethod { get; set; }
        public int ActivePaymentMethod { get; set; }
        public int InactivePaymentMethod { get; set; }
        public int CashPaymentMethod { get; set; }
        public int BankTransferPaymentMethod { get; set; }
        public int CardPaymentMethod { get; set; }
        public int QrisPaymentMethod { get; set; }
        public int InsurancePaymentMethod { get; set; }
        public int CompanyGuarantorPaymentMethod { get; set; }
        public int MembershipPaymentMethod { get; set; }
        public int NeedReferenceNumberPaymentMethod { get; set; }
        public int NeedApprovalPaymentMethod { get; set; }
        public int NeedAttachmentPaymentMethod { get; set; }
        public int AvailableForRegistrationPaymentMethod { get; set; }
        public int AvailableForBillingPaymentMethod { get; set; }
        public int AvailableForRefundPaymentMethod { get; set; }
    }

    public class PaymentMethodResponse
    {
        public Guid Id { get; set; }

        public string PaymentMethodCode { get; set; } = string.Empty;
        public string PaymentMethodName { get; set; } = string.Empty;
        public string PaymentMethodType { get; set; } = string.Empty;
        public string? PaymentGroupName { get; set; }

        public bool IsCash { get; set; }
        public bool IsBankTransfer { get; set; }
        public bool IsCardPayment { get; set; }
        public bool IsQris { get; set; }
        public bool IsInsurance { get; set; }
        public bool IsCompanyGuarantor { get; set; }
        public bool IsMembership { get; set; }
        public bool IsNeedReferenceNumber { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsNeedAttachment { get; set; }
        public bool IsAvailableForRegistration { get; set; }
        public bool IsAvailableForBilling { get; set; }
        public bool IsAvailableForRefund { get; set; }

        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
        public string? MerchantId { get; set; }
        public string? TerminalId { get; set; }
        public string? ExternalPaymentCode { get; set; }
        public string? IntegrationCode { get; set; }

        public decimal AdminFeeAmount { get; set; }
        public decimal AdminFeePercent { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PaymentMethodDetailResponse : PaymentMethodResponse
    {
        public string? Description { get; set; }
    }

    public class PaymentMethodOptionResponse
    {
        public Guid Id { get; set; }

        public string PaymentMethodCode { get; set; } = string.Empty;
        public string PaymentMethodName { get; set; } = string.Empty;
        public string PaymentMethodType { get; set; } = string.Empty;
        public string? PaymentGroupName { get; set; }

        public bool IsCash { get; set; }
        public bool IsBankTransfer { get; set; }
        public bool IsCardPayment { get; set; }
        public bool IsQris { get; set; }
        public bool IsInsurance { get; set; }
        public bool IsCompanyGuarantor { get; set; }
        public bool IsMembership { get; set; }
        public bool IsNeedReferenceNumber { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsNeedAttachment { get; set; }
        public bool IsAvailableForRegistration { get; set; }
        public bool IsAvailableForBilling { get; set; }
        public bool IsAvailableForRefund { get; set; }

        public decimal AdminFeeAmount { get; set; }
        public decimal AdminFeePercent { get; set; }
    }

    public class PaymentMethodFilterMetadataResponse
    {
        public PaymentMethodDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PaymentMethodSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> PaymentMethodTypes { get; set; } = new();
    }

    public class PaymentMethodDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string? PaymentMethodType { get; set; }
        public string? PaymentGroupName { get; set; }

        public bool? IsCash { get; set; }
        public bool? IsBankTransfer { get; set; }
        public bool? IsCardPayment { get; set; }
        public bool? IsQris { get; set; }
        public bool? IsInsurance { get; set; }
        public bool? IsCompanyGuarantor { get; set; }
        public bool? IsMembership { get; set; }
        public bool? IsNeedReferenceNumber { get; set; }
        public bool? IsNeedApproval { get; set; }
        public bool? IsNeedAttachment { get; set; }
        public bool? IsAvailableForRegistration { get; set; }
        public bool? IsAvailableForBilling { get; set; }
        public bool? IsAvailableForRefund { get; set; }

        public decimal? MinimumAdminFeeAmount { get; set; }
        public decimal? MaximumAdminFeeAmount { get; set; }
        public decimal? MinimumAdminFeePercent { get; set; }
        public decimal? MaximumAdminFeePercent { get; set; }

        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PaymentMethodSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePaymentMethodRequest
    {
        [Required]
        [MaxLength(50)]
        public string PaymentMethodCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PaymentMethodName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string PaymentMethodType { get; set; } = "Cash";

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

        [Range(0, 999999999999.99)]
        public decimal AdminFeeAmount { get; set; } = 0;

        [Range(0, 100)]
        public decimal AdminFeePercent { get; set; } = 0;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdatePaymentMethodRequest
    {
        [Required]
        [MaxLength(50)]
        public string PaymentMethodCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PaymentMethodName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string PaymentMethodType { get; set; } = "Cash";

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

        [Range(0, 999999999999.99)]
        public decimal AdminFeeAmount { get; set; } = 0;

        [Range(0, 100)]
        public decimal AdminFeePercent { get; set; } = 0;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class PaymentMethodCreateResponse
    {
        public Guid Id { get; set; }
        public string PaymentMethodCode { get; set; } = string.Empty;
        public string PaymentMethodName { get; set; } = string.Empty;
    }

    public class PaymentMethodUpdateResponse
    {
        public Guid Id { get; set; }
        public string PaymentMethodCode { get; set; } = string.Empty;
        public string PaymentMethodName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class PaymentMethodDeleteResponse
    {
        public Guid Id { get; set; }
        public string PaymentMethodCode { get; set; } = string.Empty;
        public string PaymentMethodName { get; set; } = string.Empty;
        public bool IsDelete { get; set; }
    }

    public class PaymentMethodStatusResponse
    {
        public Guid Id { get; set; }
        public string PaymentMethodCode { get; set; } = string.Empty;
        public string PaymentMethodName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
