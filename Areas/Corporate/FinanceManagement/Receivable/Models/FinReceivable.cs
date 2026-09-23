using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Aggregate root buku piutang Finance (FIN-DES-010..013, FIN-DES-024). Satu-satunya penulis
/// OutstandingAmount adalah FinanceReceivableService (belum dibangun pada task ini, BE-FIN-006
/// murni entity+configuration).
///
/// Invariant (FIN-VAL-011): OriginalAmount = OutstandingAmount + AllocatedAmount + AdjustedAmount
/// + WrittenOffAmount — ditegakkan CK_FinReceivable_Balance di database, dan MUST tetap dijaga
/// oleh service manapun yang kelak menulis kolom-kolom ini.
/// </summary>
[Table("FinReceivable", Schema = "public")]
public sealed class FinReceivable : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string ReceivableNumber { get; set; } = string.Empty;

    /// <summary>Idempotensi terhadap BilArHandoff.HandoffKey (FIN-DES-009).</summary>
    public Guid SourceHandoffKey { get; set; }

    /// <summary>Id baris handoff milik Billing — bukan FK, agar tidak mengunci tabel modul lain.</summary>
    public Guid SourceHandoffId { get; set; }

    /// <summary>Rujukan tagihan asal (BilInvoice) — bukan FK, lintas bounded context.</summary>
    public Guid InvoiceId { get; set; }

    [Required, MaxLength(30)] public string DebtorType { get; set; } = string.Empty;

    public Guid? DebtorReferenceId { get; set; }

    /// <summary>Sensitif — terisi hanya untuk EMPLOYEE_BENEFIT (FIN-DES-024, jalur intake OPEN DECISION).</summary>
    public Guid? BenefitOwnerId { get; set; }

    /// <summary>Sensitif.</summary>
    [MaxLength(30)] public string? BenefitRelationship { get; set; }

    public decimal OriginalAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal AllocatedAmount { get; set; } = 0m;
    public decimal AdjustedAmount { get; set; } = 0m;
    public decimal WrittenOffAmount { get; set; } = 0m;

    public DateOnly DueDate { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = FinReceivableStatuses.Outstanding;
    [Required, MaxLength(30)] public string ClaimStatus { get; set; } = FinReceivableClaimStatuses.NotRequired;

    public DateTimeOffset RecognizedAt { get; set; }

    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinReceivableItem> Items { get; set; } = new List<FinReceivableItem>();
    public ICollection<FinReceivableDocument> Documents { get; set; } = new List<FinReceivableDocument>();
    public ICollection<FinReceivableAdjustment> Adjustments { get; set; } = new List<FinReceivableAdjustment>();
    public ICollection<FinReceivableWriteOff> WriteOffs { get; set; } = new List<FinReceivableWriteOff>();

    /// <summary>BE-FIN-016: sisi lain jembatan FinReceiptAllocation.ReceivableId — boleh kosong bila
    /// piutang ini belum pernah dialokasikan penerimaan apa pun (BE-FIN-017, BLOCKED).</summary>
    public ICollection<FinReceiptAllocation> ReceiptAllocations { get; set; } = new List<FinReceiptAllocation>();
}

public static class FinReceivableStatuses
{
    public const string Outstanding = "OUTSTANDING";
    public const string Partial = "PARTIAL";
    public const string Settled = "SETTLED";
    public const string WrittenOff = "WRITTEN_OFF";
    public const string Cancelled = "CANCELLED";
}

public static class FinReceivableDebtorTypes
{
    public const string Payer = "PAYER";
    public const string PatientGuarantor = "PATIENT_GUARANTOR";
    public const string EmployeeBenefit = "EMPLOYEE_BENEFIT";
}

public static class FinReceivableClaimStatuses
{
    public const string NotRequired = "NOT_REQUIRED";
    public const string Incomplete = "INCOMPLETE";
    public const string Complete = "COMPLETE";
    public const string Submitted = "SUBMITTED";
}
