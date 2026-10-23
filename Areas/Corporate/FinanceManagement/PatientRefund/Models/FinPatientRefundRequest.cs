using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PatientRefund.Models;

/// <summary>
/// Pengajuan dan pencairan saldo refund pasien dari kelebihan pembayaran deposit (BilRefundableCredit).
/// Dikelola di area Finance dengan alur Maker-Checker (pengaju tidak boleh menyetujui pengajuannya sendiri).
/// </summary>
[Table("FinPatientRefundRequest", Schema = "public")]
public sealed class FinPatientRefundRequest : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PatientId { get; set; }

    public Guid InvoiceId { get; set; }

    public Guid RefundableCreditId { get; set; }

    public decimal RequestedAmount { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public decimal PaidAmount { get; set; } = 0m;

    [Required, MaxLength(30)]
    public string Status { get; set; } = FinPatientRefundStatuses.Pending;

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public Guid RequestedBy { get; set; }

    public Guid? ApprovedBy { get; set; }

    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ApprovedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    [MaxLength(30)]
    public string? PaymentMethod { get; set; }

    public Guid? BankAccountId { get; set; }

    [MaxLength(100)]
    public string? PayeeBankName { get; set; }

    [MaxLength(50)]
    public string? PayeeAccountNumber { get; set; }

    [MaxLength(150)]
    public string? PayeeAccountName { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(500)]
    public string? ExecutionNotes { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    [MaxLength(500)]
    public string? FailedReason { get; set; }

    public Guid? ExecutedBy { get; set; }

    public Guid IdempotencyKey { get; set; }

    public Guid CorrelationId { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    // Navigations
    public MstPatient? Patient { get; set; }
    public BilInvoice? Invoice { get; set; }
    public BilRefundableCredit? RefundableCredit { get; set; }
}

public static class FinPatientRefundStatuses
{
    public const string Available = "AVAILABLE"; // Status virtual untuk credit yang belum diajukan
    public const string Pending = "PENDING";
    public const string Approved = "APPROVED";
    public const string PartiallyPaid = "PARTIALLY_PAID";
    public const string Paid = "PAID";
    public const string Rejected = "REJECTED";
    public const string Cancelled = "CANCELLED";
    public const string Failed = "FAILED";
}

public static class FinPatientRefundPaymentMethods
{
    public const string Transfer = "TRANSFER";
    public const string Cash = "CASH";
}

