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
