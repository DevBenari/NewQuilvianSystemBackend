using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class ApplyDiscountRequest
{
    public Guid DiscountPolicyId { get; set; }
    public Guid? InvoiceItemId { get; set; }
    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal? RequestedAmount { get; set; }
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public sealed class ApproveDiscountRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public sealed class DiscountResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid? InvoiceItemId { get; set; }
    public Guid DiscountPolicyId { get; set; }
    public string PolicyCode { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public string TargetComponent { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal Amount { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsEffective { get; set; }
    public bool RequiresFinanceApproval { get; set; }
    public Guid InvoiceRowVersion { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }
}

public sealed class DiscountCalculationResponse
{
    public Guid DiscountApplicationId { get; set; }
    public Guid DiscountPolicyId { get; set; }
    public string PolicyCode { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public string TargetComponent { get; set; } = string.Empty;
    public Guid? InvoiceItemId { get; set; }
    public string ValueType { get; set; } = string.Empty;
    public decimal PolicyValue { get; set; }
    public decimal? PolicyLimit { get; set; }
    public decimal BasisAmount { get; set; }
    public decimal AppliedAmount { get; set; }
}
