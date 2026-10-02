using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Rincian baris piutang. Jumlah seluruh baris MUST sama dengan FinReceivable.OriginalAmount
/// (FIN-DES-024 area, ditegakkan service — tidak ada check constraint lintas tabel di Postgres).
/// PatientId/EncounterId sensitif — MUST NOT ikut ke payload Accounting.
/// </summary>
[Table("FinReceivableItem", Schema = "public")]
public sealed class FinReceivableItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReceivableId { get; set; }
    public FinReceivable? Receivable { get; set; }

    /// <summary>Sensitif.</summary>
    public Guid? PatientId { get; set; }

    /// <summary>Sensitif.</summary>
    public Guid? EncounterId { get; set; }

    /// <summary>Rujukan tagihan per baris — bukan FK, lintas bounded context.</summary>
    public Guid? InvoiceId { get; set; }

    [Required, MaxLength(300)] public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
