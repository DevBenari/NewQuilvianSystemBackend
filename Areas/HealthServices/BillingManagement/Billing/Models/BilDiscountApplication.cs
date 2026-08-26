using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilDiscountApplication", Schema = "public")]
public sealed class BilDiscountApplication : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    public Guid? InvoiceItemId { get; set; }
    public Guid DiscountPolicyId { get; set; }
    [Required, MaxLength(30)] public string DiscountType { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal Amount { get; set; }
    [Required, MaxLength(30)] public string ApprovalStatus { get; set; } = BillingDiscountApprovalStatuses.Approved;
    public Guid RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;

    public BilInvoice Invoice { get; set; } = null!;
    public BilInvoiceItem? InvoiceItem { get; set; }
    public MstDiscountPolicy DiscountPolicy { get; set; } = null!;
}

public static class BillingDiscountApprovalStatuses
{
    public const string Approved = "APPROVED";
    public const string PendingDoctor = "PENDING_DOCTOR";
    public const string PendingFinance = "PENDING_FINANCE";
}
