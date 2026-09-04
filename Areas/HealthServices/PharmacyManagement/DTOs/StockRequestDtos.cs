using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;

// ===================================================================== permintaan

public class StockRequestItemInput
{
    [Required] public Guid DrugId { get; set; }
    [Required] public Guid MeasurementId { get; set; }

    [Range(0.001, 1000000)]
    public decimal RequestedQuantity { get; set; }

    [MaxLength(500)] public string? Note { get; set; }
}

public class CreateStockRequestRequest
{
    [Required] public Guid RequestingServiceUnitId { get; set; }
    [Required] public Guid StorageLocationId { get; set; }
    [Required] public Guid RequestedByWorkforceId { get; set; }
    [Required] public StockRequestPriority Priority { get; set; } = StockRequestPriority.Routine;

    public DateTime? NeededAt { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    [Required, MinLength(1)]
    public List<StockRequestItemInput> Items { get; set; } = [];

    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class UpdateStockRequestRequest
{
    [Required] public Guid StorageLocationId { get; set; }
    [Required] public StockRequestPriority Priority { get; set; }

    public DateTime? NeededAt { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    /// <summary>
    /// Daftar item pengganti. Yang dikirim menjadi isi permintaan seluruhnya; baris lama
    /// yang tidak lagi disebut akan dihapus.
    /// </summary>
    [Required, MinLength(1)]
    public List<StockRequestItemInput> Items { get; set; } = [];

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class SubmitStockRequestRequest
{
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class CancelStockRequestRequest
{
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

// ------------------------------------------------------------------ sisi gudang

public class FulfillStockRequestItemInput
{
    [Required] public Guid StockRequestItemId { get; set; }

    /// <summary>
    /// Jumlah yang benar-benar diserahkan. Nol sah dan berarti tidak ada yang diserahkan
    /// untuk baris ini — berbeda dari tidak menyebut barisnya sama sekali, yang ditolak.
    /// </summary>
    [Range(0, 1000000)]
    public decimal FulfilledQuantity { get; set; }
}

/// <summary>Penyerahan barang oleh gudang, per baris permintaan.</summary>
public class FulfillStockRequestRequest
{
    /// <summary>
    /// Harus memuat seluruh baris permintaan yang masih berlaku. Baris yang tidak disebut
    /// ditolak, bukan dianggap nol: gudang harus menyatakan setiap baris secara sadar.
    /// </summary>
    [Required, MinLength(1)]
    public List<FulfillStockRequestItemInput> Items { get; set; } = [];

    [MaxLength(1000)] public string? Note { get; set; }

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>Saringan riwayat permintaan obat.</summary>
public class StockRequestPagedQuery
{
    public StockRequestStatus? Status { get; set; }
    public StockRequestPriority? Priority { get; set; }
    public Guid? RequestingServiceUnitId { get; set; }
    public Guid? StorageLocationId { get; set; }

    /// <summary>Menyaring menurut obat yang diminta, bukan hanya nomor permintaan.</summary>
    public Guid? DrugId { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>Mencari pada nomor permintaan, nama unit, atau nama obat di dalamnya.</summary>
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// ===================================================================== tanggapan

public class StockRequestSummaryResponse
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public Guid RequestingServiceUnitId { get; set; }
    public string RequestingServiceUnitName { get; set; } = string.Empty;
    public Guid StorageLocationId { get; set; }
    public string StorageLocationName { get; set; } = string.Empty;
    public string RequestedByName { get; set; } = string.Empty;
    public StockRequestStatus Status { get; set; }
    public StockRequestPriority Priority { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? NeededAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int ItemCount { get; set; }
    public int Version { get; set; }

    /// <summary>
    /// Benar bila permintaan ini masih boleh diubah. Dihitung backend agar layar tidak
    /// perlu menyalin aturannya dan tidak dapat menyimpang darinya.
    /// </summary>
    public bool IsEditable { get; set; }

    /// <summary>
    /// Perintah yang sah pada keadaan sekarang. Sama alasannya dengan <see cref="IsEditable"/>:
    /// aturan transisi hanya ada di backend, layar cukup menuruti.
    /// </summary>
    /// <remarks>
    /// Tidak ada bendera menyetujui maupun menolak. Gudang tidak memutuskan permintaan;
    /// ia melihatnya, lalu mencatat berapa yang benar-benar diserahkan.
    /// </remarks>
    public bool CanFulfill { get; set; }
    public bool CanCancel { get; set; }
}

public class StockRequestDetailResponse : StockRequestSummaryResponse
{
    public Guid RequestedByWorkforceId { get; set; }
    public string? Notes { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionReason { get; set; }
    public List<StockRequestItemResponse> Items { get; set; } = [];
    public List<StockRequestHistoryResponse> Histories { get; set; } = [];
}

public class StockRequestItemResponse
{
    public Guid Id { get; set; }
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public Guid MeasurementId { get; set; }
    public string? MeasurementName { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal? FulfilledQuantity { get; set; }
    public string? Note { get; set; }
    public int LineNumber { get; set; }
}

public class StockRequestHistoryResponse
{
    public Guid Id { get; set; }
    public StockRequestStatus? FromStatus { get; set; }
    public StockRequestStatus ToStatus { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime OccurredAt { get; set; }
}
