using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Baris pemakaian Deposit Retur sebagai sumber dana pembayaran supplier (FIN-DES-045, FIN-DES-046,
/// FIN-DEC-057, 02-backend-architecture.md §D.1-D.2 AMENDMENT REVISI 5).
///
/// BENTUK INI MENGGANTIKAN rancangan awal AMENDMENT REVISI 4 yang menunjuk PurchasingInvoiceId
/// (erd/data-dictionary.md bagian C.11, DIGANTIKAN). Purchasing Invoice yang sudah APPROVED
/// beku (FIN-STATE-1.2 §B.4), dan hanya FinancePaymentService/FinanceSupplierPayableService yang
/// menulis FinSupplierPayable — sehingga pemakaian deposit dipasangkan ke FinPayment (baris
/// alokasi non-tunai di tingkat pembayaran, bukan ke invoice), memakai ulang kapasitas satu
/// pembayaran melunasi banyak utang sekaligus (FIN-DES-015).
///
/// Reservasi terjadi saat baris ditambahkan ke FinPayment berstatus DRAFT (RESERVED), bukan saat
/// PAID — mencegah dua pembayaran DRAFT memakai saldo deposit yang sama tanpa ketahuan sampai
/// keduanya disetujui (FIN-DES-046). AvailableAmount HANYA ditulis FinanceSupplierReturnService.
/// </summary>
[Table("FinSupplierReturnDepositUsage", Schema = "public")]
public sealed class FinSupplierReturnDepositUsage : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SupplierReturnDepositId { get; set; }
    public FinSupplierReturnDeposit? SupplierReturnDeposit { get; set; }

    /// <summary>FK ke FinPayment — pembayaran yang memakai deposit ini sebagai sumber dana.</summary>
    public Guid PaymentId { get; set; }
    public FinPayment? Payment { get; set; }

    public decimal UsedAmount { get; set; }

    [Required, MaxLength(20)] public string Status { get; set; } = FinSupplierReturnDepositUsageStatuses.Reserved;

    public DateTimeOffset UsedAt { get; set; }

    /// <summary>Terisi hanya saat Status = RELEASED.</summary>
    public DateTimeOffset? ReleasedAt { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

public static class FinSupplierReturnDepositUsageStatuses
{
    public const string Reserved = "RESERVED";
    public const string Applied = "APPLIED";
    public const string Released = "RELEASED";
}
