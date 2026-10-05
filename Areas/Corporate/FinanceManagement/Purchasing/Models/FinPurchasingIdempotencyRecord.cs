using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
public sealed class FinPurchasingIdempotencyRecord : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Kunci dari header <c>Idempotency-Key</c> milik klien. Unik — inilah baris dedup-nya.</summary>
    public Guid IdempotencyKey { get; set; }

    /// <summary>Nama aggregate root, salah satu dari <see cref="FinPurchasingIdempotencyEntityTypes"/>.</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Nama aksi, salah satu dari <see cref="FinPurchasingIdempotencyActions"/>.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Id baris yang terdampak. Null hanya bila Create gagal sebelum entity tersimpan
    /// (baris ledger tidak pernah ditulis untuk permintaan yang gagal — lihat *IdempotencyService.cs,
    /// hanya jalur sukses yang direkam).</summary>
    public Guid EntityId { get; set; }

    public int ResponseStatusCode { get; set; }

    /// <summary>Salinan persis body JSON (`ApiResponse&lt;T&gt;`) yang dikembalikan pertama kali —
    /// dikirim ulang apa adanya saat replay, bukan dihitung ulang.</summary>
    public string ResponseBody { get; set; } = string.Empty;
}

public static class FinPurchasingIdempotencyEntityTypes
{
    public const string PurchaseOrder = "PurchaseOrder";
    public const string GoodsReceipt = "GoodsReceipt";
    public const string InvoiceExchange = "InvoiceExchange";
    public const string PurchasingInvoice = "PurchasingInvoice";
    public const string SupplierReturn = "SupplierReturn";
}

public static class FinPurchasingIdempotencyActions
{
    public const string Create = "Create";
    public const string Submit = "Submit";
    public const string Approve = "Approve";
    public const string Cancel = "Cancel";
    public const string Confirm = "Confirm";
}
