using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Baris keanggotaan Batch Tagihan AR (FIN-DES-041, erd/data-dictionary.md §C.14). Satu
/// FinReceivable hanya boleh berada di satu batch yang belum CANCELLED (unique index parsial pada
/// ReceivableId, ditegakkan configuration — bukan FIN-VAL bernomor di sini, murni integritas data).
/// </summary>
[Table("FinReceivableInvoiceBatchItem", Schema = "public")]
public sealed class FinReceivableInvoiceBatchItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BatchId { get; set; }
    public FinReceivableInvoiceBatch? Batch { get; set; }

    public Guid ReceivableId { get; set; }
    public FinReceivable? Receivable { get; set; }
}
