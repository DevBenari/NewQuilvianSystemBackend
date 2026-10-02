using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Jejak kelengkapan berkas klaim satu piutang. Kelengkapan dokumen MUST NOT menjadi syarat
/// piutang diakui (FIN-DEC-013) — hanya memengaruhi FinReceivable.ClaimStatus, tidak pernah
/// mengubah nilainya.
/// </summary>
[Table("FinReceivableDocument", Schema = "public")]
public sealed class FinReceivableDocument : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReceivableId { get; set; }
    public FinReceivable? Receivable { get; set; }

    [Required, MaxLength(50)] public string DocumentType { get; set; } = string.Empty;
    [MaxLength(100)] public string? DocumentNumber { get; set; }

    public bool IsReceived { get; set; } = false;
    public DateTimeOffset? ReceivedAt { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }
}
