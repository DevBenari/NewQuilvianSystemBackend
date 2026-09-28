using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Kredit retur milik supplier (FIN-DES-038, FIN-DEC-047, 02-backend-architecture.md §C.1-C.2,
/// D.1-D.2). Meniru pola struktur BilRefundableCredit (Billing) — arah aliran berlawanan, bukan
/// tabel yang sama. Dapat dipakai lintas invoice/pembelian berikutnya: dicadangkan sebagai
/// sumber dana FinPayment lewat FinSupplierReturnDepositUsage (FIN-DES-045, BE-FIN-036), BUKAN
/// dipakai langsung ke FinPurchasingInvoice.
///
/// AvailableAmount HANYA ditulis FinanceSupplierReturnService — satu-satunya penulis, ditegakkan
/// di level service (ReserveAsync/ReleaseAsync/MarkAppliedAsync, FIN-DES-046).
/// </summary>
[Table("FinSupplierReturnDeposit", Schema = "public")]
public sealed class FinSupplierReturnDeposit : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Milik Administrator (FIN-DEC-014) — Finance MUST NOT membuat master Supplier baru.</summary>
    public Guid SupplierId { get; set; }
    public MstSupplier? Supplier { get; set; }

    /// <summary>Satu retur = satu deposit (UNIQUE).</summary>
    public Guid SourceReturnId { get; set; }
    public FinSupplierReturn? SourceReturn { get; set; }

    public decimal OriginalAmount { get; set; }

    /// <summary>MUST NOT negatif. Berkurang hanya lewat baris FinSupplierReturnDepositUsage.</summary>
    public decimal AvailableAmount { get; set; }

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string Status { get; set; } = FinSupplierReturnDepositStatuses.Available;

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinSupplierReturnDepositUsage> Usages { get; set; } = new List<FinSupplierReturnDepositUsage>();
}

public static class FinSupplierReturnDepositStatuses
{
    public const string Available = "AVAILABLE";
    public const string Exhausted = "EXHAUSTED";
    public const string Cancelled = "CANCELLED";
}
