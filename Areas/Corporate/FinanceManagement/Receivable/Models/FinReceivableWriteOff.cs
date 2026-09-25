using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Penghapusan buku sebagian/seluruh piutang, maker-checker dua lapis (FIN-DES-014, FIN-DEC-012).
/// Bentuknya sama persis dengan FinReceivableAdjustment kecuali tanpa Direction dan
/// SourceHandoffAdjustmentId. Penghapusan buku MUST NOT menghapus baris piutang — hanya
/// memindahkan nilai dari OutstandingAmount ke WrittenOffAmount (ditegakkan service, belum
/// dibangun pada task ini).
/// </summary>
[Table("FinReceivableWriteOff", Schema = "public")]
public sealed class FinReceivableWriteOff : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string WriteOffNumber { get; set; } = string.Empty;

    public Guid ReceivableId { get; set; }
    public FinReceivable? Receivable { get; set; }

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
