using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Detail baris pengajuan pemutihan tagihan AR V2.
/// Memetakan relasi hierarkis: Proposal -> FinReceivableInvoiceBatch -> FinReceivable.
/// Satu invoice AR dapat terdiri dari beberapa receivable, dan masing-masing receivable dialokasikan nominal pemutihannya.
/// </summary>
[Table("FinReceivableWriteOffProposalItem", Schema = "public")]
public sealed class FinReceivableWriteOffProposalItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProposalId { get; set; }
    public FinReceivableWriteOffProposal? Proposal { get; set; }

    /// <summary>ID Invoice AR (FinReceivableInvoiceBatch.Id).</summary>
    public Guid InvoiceBatchId { get; set; }
    public FinReceivableInvoiceBatch? InvoiceBatch { get; set; }

    /// <summary>ID Piutang AR (FinReceivable.Id).</summary>
    public Guid ReceivableId { get; set; }
    public FinReceivable? Receivable { get; set; }

    /// <summary>Snapshot sisa piutang saat proposal diajukan.</summary>
    public decimal OutstandingSnapshot { get; set; }

    /// <summary>Nominal pemutihan yang diusulkan untuk piutang ini.</summary>
    public decimal ProposedAmount { get; set; }

    /// <summary>Nominal pemutihan yang disetujui (maker-checker).</summary>
    public decimal ApprovedAmount { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    /// <summary>Referensi FinReceivableWriteOff yang dibuat saat proposal di-POSTING secara atomik.</summary>
    public Guid? WriteOffId { get; set; }
    public FinReceivableWriteOff? WriteOff { get; set; }
}
