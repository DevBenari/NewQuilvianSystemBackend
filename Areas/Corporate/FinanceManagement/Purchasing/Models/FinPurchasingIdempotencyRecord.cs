namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

/// <summary>
/// Ledger idempotensi bersama untuk rumpun Purchasing (BE-FIN-051, FIN-DES-006, api-contract.md
/// baris 26: "Perintah yang memindahkan uang — Wajib header Idempotency-Key"). Menutup gap yang
/// dicatat FE-FIN-008: kelima controller Purchasing (PO, GR, Tukar Faktur, Purchasing Invoice,
/// Supplier Return) sama sekali tidak membaca header ini.
///
/// Setiap aggregate root Purchasing punya BEBERAPA aksi terpisah yang masing-masing butuh
/// idempotensi sendiri sepanjang siklus hidupnya (Create, Submit, Approve, Cancel, Confirm) — pola
/// `PettyCashBudgetService` (kolom `IdempotencyKey` pada baris "movement") tidak cocok ditiru
/// langsung karena PO/GR/Tukar Faktur/Purchasing Invoice/Supplier Return tidak punya tabel
/// "movement" per aksi. Satu tabel ledger inilah yang menggantikan peran itu: dicek sebelum
/// eksekusi (replay bila kunci sudah pernah dipakai), ditulis SEKALI setelah eksekusi berhasil,
/// dalam SaveChangesAsync yang SAMA dengan mutasi bisnisnya (lihat *IdempotencyService.cs) supaya
/// atomis — bukan transaksi terpisah.
///
/// Sengaja diletakkan di submodul Purchasing (bukan folder "Shared" lintas modul) karena cakupan
/// task ini eksplisit hanya lima entity Purchasing; gap yang sama ada di rumpun Finance lain
/// (Receipt, Receivable, Payment, dst. — seluruhnya masih "Rencana (belum tersedia)" pada
/// api-contract.md), tapi itu di luar wewenang task ini, dicatat sebagai gap lanjutan, bukan
/// digeneralisasi diam-diam di sini.
/// </summary>
public sealed class FinPurchasingIdempotencyRecord
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

    public DateTimeOffset CreateDateTime { get; set; } = DateTimeOffset.UtcNow;

    public Guid CreateBy { get; set; }
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
