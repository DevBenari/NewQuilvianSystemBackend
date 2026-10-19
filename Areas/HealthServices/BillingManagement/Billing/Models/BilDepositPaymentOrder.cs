using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilDepositPaymentOrder", Schema = "public")]
public sealed class BilDepositPaymentOrder : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    public Guid EpisodeId { get; set; }
    public Guid EncounterId { get; set; }
    public Guid PatientId { get; set; }

    [Required, MaxLength(30)]
    public string OrderType { get; set; } = DepositOrderTypes.InitialDeposit;

    [Required, MaxLength(50)]
    public string SourceType { get; set; } = DepositOrderSourceTypes.InpatientAdmission;

    public Guid? SourceReferenceId { get; set; }

    public decimal RequestedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = DepositOrderStatuses.Unpaid;

    public DateTimeOffset? DueDate { get; set; }
    public Guid? PolicyId { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    public Guid IdempotencyKey { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public DateTimeOffset? CancelledAt { get; set; }
    public Guid? CancelledBy { get; set; }
    [MaxLength(500)]
    public string? CancellationReason { get; set; }

    public ICollection<BilDepositOrderPayment> Payments { get; set; } = new List<BilDepositOrderPayment>();
}

public static class DepositOrderTypes
{
    public const string InitialDeposit = "INITIAL_DEPOSIT";
    public const string AdditionalTopUp = "ADDITIONAL_TOP_UP";
}

public static class DepositOrderSourceTypes
{
    public const string InpatientAdmission = "INPATIENT_ADMISSION";
    public const string BillingCashier = "BILLING_CASHIER";
    public const string Other = "OTHER";
}

public static class DepositOrderStatuses
{
    public const string Unpaid = "UNPAID";
    public const string PartiallyPaid = "PARTIALLY_PAID";
    public const string Paid = "PAID";
    public const string Cancelled = "CANCELLED";
    public const string Expired = "EXPIRED";
}
