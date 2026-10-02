using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Koreksi nilai piutang, maker-checker dua lapis (FIN-DES-014, FIN-DEC-012). OutstandingAmount
/// piutang hanya berubah saat Status menjadi APPROVED — ditegakkan service, belum dibangun pada
/// task ini (BE-FIN-006 murni entity+configuration).
/// </summary>
[Table("FinReceivableAdjustment", Schema = "public")]
public sealed class FinReceivableAdjustment : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string AdjustmentNumber { get; set; } = string.Empty;

    public Guid ReceivableId { get; set; }
    public FinReceivable? Receivable { get; set; }

    /// <summary>Terisi bila koreksi berasal dari Billing — bukan FK, lintas bounded context.</summary>
    public Guid? SourceHandoffAdjustmentId { get; set; }

    [Required, MaxLength(10)] public string Direction { get; set; } = string.Empty;

    /// <summary>Selalu positif; arah ditentukan Direction.</summary>
    public decimal Amount { get; set; }

    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;

    [Required, MaxLength(30)] public string Status { get; set; } = FinReceivableApprovalStatuses.Requested;

    public Guid RequestedBy { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    [MaxLength(500)] public string? RejectionReason { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

public static class FinReceivableAdjustmentDirections
{
    public const string Debit = "DEBIT";
    public const string Credit = "CREDIT";
}

/// <summary>Dipakai bersama FinReceivableAdjustment dan FinReceivableWriteOff — keduanya memakai matriks status yang identik.</summary>
public static class FinReceivableApprovalStatuses
{
    public const string Requested = "REQUESTED";
    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";
}
