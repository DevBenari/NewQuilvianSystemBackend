using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;

// ===================================================================== permintaan

/// <summary>Batch yang dituju sebuah perintah stok.</summary>
public class DrugBatchInput
{
    [Required] public Guid DrugId { get; set; }

    [Required, MaxLength(100)]
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>Wajib diisi. Kolomnya boleh kosong di basis data, aturannya tidak.</summary>
    public DateOnly? ExpiryDate { get; set; }

    public Guid? SupplierId { get; set; }
    [MaxLength(200)] public string? PrincipalName { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}

/// <summary>Saldo pembuka saat stok pertama kali dicatat sistem.</summary>
public class RecordOpeningBalanceRequest
{
    [Required] public DrugBatchInput Batch { get; set; } = new();
    [Required] public Guid StorageLocationId { get; set; }

    [Range(0.001, 100000000)]
    public decimal Quantity { get; set; }

    [MaxLength(1000)] public string? Reason { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>
/// Penyesuaian saldo di luar transaksi normal.
/// </summary>
/// <remarks>
/// Penambahan stok dari pembelian tidak boleh lewat sini; pembelian wajib melalui penerimaan
/// barang resmi. Perintah ini untuk hasil stock opname dan koreksi selisih.
/// </remarks>
public class AdjustStockRequest
{
    [Required] public Guid DrugBatchId { get; set; }
    [Required] public Guid StorageLocationId { get; set; }
    public DrugStockStatus Status { get; set; } = DrugStockStatus.Available;

    /// <summary>Selisih bertanda: positif menambah, negatif mengurangi.</summary>
    [Required] public decimal QuantityChange { get; set; }

    /// <summary>Wajib. Penyesuaian tanpa alasan tidak dapat diperiksa siapa pun kelak.</summary>
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>Memindahkan sebagian stok ke status lain, misalnya karantina.</summary>
public class ChangeStockStatusRequest
{
    [Required] public Guid DrugBatchId { get; set; }
    [Required] public Guid StorageLocationId { get; set; }

    public DrugStockStatus FromStatus { get; set; } = DrugStockStatus.Available;
    [Required] public DrugStockStatus ToStatus { get; set; }

    [Range(0.001, 100000000)]
    public decimal Quantity { get; set; }

    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>Mengeluarkan stok satu obat dari satu lokasi menurut FEFO.</summary>
public class IssueStockRequest
{
    [Required] public Guid DrugId { get; set; }
    [Required] public Guid StorageLocationId { get; set; }

    [Range(0.001, 100000000)]
    public decimal Quantity { get; set; }

    [MaxLength(50)] public string? SourceDocumentType { get; set; }
    public Guid? SourceDocumentId { get; set; }
    [MaxLength(1000)] public string? Reason { get; set; }

    /// <summary>
    /// Memakai stok yang sudah direservasi lebih dahulu, bukan menambah pengeluaran baru.
    /// </summary>
    public bool ConsumeReservation { get; set; }

    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>Menahan stok agar tidak diambil proses lain.</summary>
public class ReserveStockRequest
{
    [Required] public Guid DrugId { get; set; }
    [Required] public Guid StorageLocationId { get; set; }

    [Range(0.001, 100000000)]
    public decimal Quantity { get; set; }

    [MaxLength(50)] public string? SourceDocumentType { get; set; }
    public Guid? SourceDocumentId { get; set; }

    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>Mengoreksi satu baris kartu stok yang terlanjur salah.</summary>
public class CorrectMutationRequest
{
    [Required] public Guid MutationId { get; set; }

    /// <summary>Nilai pergerakan yang seharusnya, bertanda.</summary>
    [Required] public decimal CorrectedQuantityChange { get; set; }

    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

// ===================================================================== saringan

public class DrugStockBalanceQuery
{
    public Guid? DrugId { get; set; }
    public Guid? StorageLocationId { get; set; }
    public Guid? DrugBatchId { get; set; }
    public DrugStockStatus? Status { get; set; }

    /// <summary>Hanya menampilkan yang saldonya masih ada.</summary>
    public bool OnlyInStock { get; set; }

    /// <summary>Batch yang kedaluwarsa dalam sekian hari ke depan.</summary>
    public int? ExpiringWithinDays { get; set; }

    /// <summary>Batch yang sudah lewat tanggal kedaluwarsa.</summary>
    public bool? OnlyExpired { get; set; }

    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Saringan ringkasan stok per item pada satu lokasi.</summary>
public class DrugStockSummaryQuery
{
    public Guid? DrugId { get; set; }
    public Guid? StorageLocationId { get; set; }
    public DrugStockStatus? Status { get; set; }
    public bool OnlyInStock { get; set; }
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class DrugBatchQuery
{
    public Guid? DrugId { get; set; }
    public string? Search { get; set; }

    /// <summary>Batch yang kedaluwarsa dalam sekian hari ke depan.</summary>
    public int? ExpiringWithinDays { get; set; }
    public bool? OnlyExpired { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class DrugStockMutationQuery
{
    public Guid? DrugId { get; set; }
    public Guid? DrugBatchId { get; set; }
    public Guid? StorageLocationId { get; set; }
    public DrugStockMutationType? MutationType { get; set; }
    public string? SourceDocumentType { get; set; }
    public Guid? SourceDocumentId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

// ===================================================================== tanggapan

public class DrugBatchResponse
{
    public Guid Id { get; set; }
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? PrincipalName { get; set; }

    /// <summary>Sisa hari menuju kedaluwarsa; negatif berarti sudah lewat.</summary>
    public int? DaysToExpiry { get; set; }
}

public class DrugStockBalanceResponse
{
    public Guid Id { get; set; }
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;

    public Guid DrugBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public int? DaysToExpiry { get; set; }

    public Guid StorageLocationId { get; set; }
    public string StorageLocationName { get; set; } = string.Empty;

    public DrugStockStatus Status { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable { get; set; }
    public int Version { get; set; }
}

/// <summary>
/// Ringkasan stok satu obat pada satu lokasi dalam satu status.
/// </summary>
/// <remarks>
/// Status ikut menjadi kunci, sama seperti pada saldo. Menjumlahkan stok siap pakai dengan
/// stok karantina menghasilkan angka yang tidak dapat dipakai memutuskan apa pun.
/// </remarks>
public class DrugStockSummaryResponse
{
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;

    public Guid StorageLocationId { get; set; }
    public string StorageLocationName { get; set; } = string.Empty;

    public DrugStockStatus Status { get; set; }

    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable { get; set; }

    /// <summary>Banyaknya batch yang menyusun angka di atas.</summary>
    public int BatchCount { get; set; }

    /// <summary>Kedaluwarsa terdekat di antara batch tersebut; itulah yang keluar lebih dahulu.</summary>
    public DateOnly? NearestExpiryDate { get; set; }
    public int? DaysToNearestExpiry { get; set; }
}

public class DrugStockMutationResponse
{
    public Guid Id { get; set; }
    public Guid DrugId { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public Guid DrugBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public Guid StorageLocationId { get; set; }
    public string StorageLocationName { get; set; } = string.Empty;

    public DrugStockStatus Status { get; set; }
    public DrugStockMutationType MutationType { get; set; }
    public decimal QuantityChange { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    public string? Reason { get; set; }
    public string? SourceDocumentType { get; set; }
    public Guid? SourceDocumentId { get; set; }
    public Guid? CorrectionOfMutationId { get; set; }

    /// <summary>Siapa yang melakukan pergerakan ini.</summary>
    public Guid ActorUserId { get; set; }
    public string? ActorName { get; set; }

    public DateTime OccurredAt { get; set; }
}

/// <summary>Satu batch yang ikut dipakai memenuhi sebuah pengeluaran.</summary>
public class StockAllocationResponse
{
    public Guid DrugBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public Guid MutationId { get; set; }
}

/// <summary>Hasil pengeluaran atau reservasi, berikut batch mana saja yang terpakai.</summary>
public class StockOperationResponse
{
    public Guid DrugId { get; set; }
    public Guid StorageLocationId { get; set; }
    public decimal TotalQuantity { get; set; }
    public List<StockAllocationResponse> Allocations { get; set; } = [];
}
