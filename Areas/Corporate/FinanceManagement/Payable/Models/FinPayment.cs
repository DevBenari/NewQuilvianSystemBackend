using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

/// <summary>
/// Aggregate root pembayaran keluar (FIN-DES-015, FIN-DEC-019, FIN-DEC-022, 02-backend-architecture.md §4.13).
/// Satu perintah bayar yang boleh melunasi banyak utang sekaligus dalam bentuk rekap pembayaran.
///
/// Invariant:
/// - NetTransferAmount = TotalAmount - DeductionAmount + AdditionAmount (CK_FinPayment_NetTransfer, FR-FIN-050).
/// - NetTransferAmount >= 0 (CK_FinPayment_NetTransferNonNegative).
/// - TotalAmount > 0 (CK_FinPayment_Total).
/// - ApprovedBy IS NULL OR ApprovedBy != RequestedBy (CK_FinPayment_MakerChecker, FIN-VAL-051).
/// - Status != 'PAID' OR AllocatedAmount = TotalAmount (CK_FinPayment_FullyAllocatedWhenPaid, FIN-VAL-050).
/// - ApprovalTier diisi service dari total nominal (FIN-DEC-022; ambang nominal masih FIN-OQ-010).
/// </summary>
[Table("FinPayment", Schema = "public")]
public sealed class FinPayment : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string PaymentNumber { get; set; } = string.Empty;

    /// <summary>SUPPLIER atau MEDICAL_SERVICE (Amendment A.4). Default SUPPLIER.</summary>
    [Required, MaxLength(30)] public string PaymentType { get; set; } = FinPaymentTypes.Supplier;

    /// <summary>Identitas penerima pembayaran (SupplierId atau MedicalStaffId/PractitionerId). Sensitif bila dokter/tenaga medis.</summary>
    public Guid PayeeReferenceId { get; set; }

    /// <summary>Rekening sumber dana (FK ke MstBankAccount).</summary>
    public Guid BankAccountId { get; set; }
    public MstBankAccount? BankAccount { get; set; }

    /// <summary>TRANSFER, CASH, atau CHEQUE.</summary>
    [Required, MaxLength(30)] public string PaymentMethod { get; set; } = FinPaymentMethods.Transfer;

    /// <summary>Total nilai utang yang dilunasi — MUST sama dengan jumlah alokasi sebelum PAID (FR-FIN-050).</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Jumlah seluruh alokasi (FinPaymentAllocation.Amount).</summary>
    public decimal AllocatedAmount { get; set; } = 0m;

    /// <summary>Jumlah seluruh baris potongan berarah DEDUCTION (PPh 21, kasbon, iuran, dll.).</summary>
    public decimal DeductionAmount { get; set; } = 0m;

    /// <summary>Jumlah seluruh baris tambahan berarah ADDITION (sitting fee, dll.).</summary>
    public decimal AdditionAmount { get; set; } = 0m;

    /// <summary>Uang yang benar-benar ditransfer: TotalAmount - DeductionAmount + AdditionAmount (FR-FIN-050). MUST NOT negatif.</summary>
    public decimal NetTransferAmount { get; set; } = 0m;

    [Required, MaxLength(30)] public string Status { get; set; } = FinPaymentStatuses.Draft;

    /// <summary>Diisi service dari total nominal (FIN-DEC-022); ambang nominal masih FIN-OQ-010.</summary>
    [MaxLength(30)] public string? ApprovalTier { get; set; }

    public Guid RequestedBy { get; set; }
    public DateTimeOffset RequestedAt { get; set; }

    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }

    /// <summary>Waktu uang benar-benar keluar (saat Treasury menandai MarkPaid).</summary>
    public DateTimeOffset? PaidAt { get; set; }

    /// <summary>Nomor bukti transfer bank — wajib diisi saat MarkPaid (FIN-VAL-056).</summary>
    [MaxLength(150)] public string? ReferenceNumber { get; set; }

    [MaxLength(500)] public string? RejectionReason { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinPaymentAllocation> Allocations { get; set; } = new List<FinPaymentAllocation>();
    public ICollection<FinPaymentDeduction> Deductions { get; set; } = new List<FinPaymentDeduction>();
}

public static class FinPaymentStatuses
{
    public const string Draft = "DRAFT";
    public const string Submitted = "SUBMITTED";
    public const string Approved = "APPROVED";
    public const string Paid = "PAID";
    public const string Rejected = "REJECTED";
    public const string Cancelled = "CANCELLED";
}

public static class FinPaymentTypes
{
    public const string Supplier = "SUPPLIER";
    public const string MedicalService = "MEDICAL_SERVICE";
}

public static class FinPaymentMethods
{
    public const string Transfer = "TRANSFER";
    public const string Cash = "CASH";
    public const string Cheque = "CHEQUE";
}
