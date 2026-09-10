using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;

// ===================================================================== permintaan

public class StockTransferItemInput
{
    [Required] public Guid DrugId { get; set; }

    [Range(0.001, 1000000)]
    public decimal RequestedQuantity { get; set; }

    [MaxLength(500)] public string? Note { get; set; }
}

public class CreateStockTransferRequest
{
    [Required] public Guid SourceStorageLocationId { get; set; }
    [Required] public Guid DestinationStorageLocationId { get; set; }
    [Required] public Guid RequestedByWorkforceId { get; set; }

    [MaxLength(1000)] public string? Notes { get; set; }

    [Required, MinLength(1)]
    public List<StockTransferItemInput> Items { get; set; } = [];

    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class UpdateStockTransferRequest
{
    [Required] public Guid DestinationStorageLocationId { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    [Required, MinLength(1)]
    public List<StockTransferItemInput> Items { get; set; } = [];

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>Perintah tanpa isian tambahan selain versi dan kunci idempotensi.</summary>
public class StockTransferCommandRequest
{
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>Perintah yang alasannya wajib, yaitu penolakan dan pembatalan.</summary>
public class StockTransferReasonRequest
{
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class ReceiveStockTransferItemInput
{
    [Required] public Guid StockTransferItemId { get; set; }

    /// <summary>
    /// Jumlah yang benar-benar diterima. Boleh lebih kecil daripada yang dikirim; selisihnya
    /// adalah barang yang hilang atau rusak di perjalanan, dan itu memang perlu terbaca.
    /// </summary>
    [Range(0, 1000000)]
    public decimal ReceivedQuantity { get; set; }
}

public class ReceiveStockTransferRequest
{
    /// <summary>Harus memuat seluruh baris yang dikirim, termasuk yang diterima nol.</summary>
    [Required, MinLength(1)]
    public List<ReceiveStockTransferItemInput> Items { get; set; } = [];

    [MaxLength(1000)] public string? Note { get; set; }

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class StockTransferPagedQuery
{
    public StockTransferStatus? Status { get; set; }
    public Guid? SourceStorageLocationId { get; set; }
    public Guid? DestinationStorageLocationId { get; set; }
    public Guid? DrugId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// ===================================================================== tanggapan

public class StockTransferSummaryResponse
{
    public Guid Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;

    public Guid SourceStorageLocationId { get; set; }
    public string SourceStorageLocationName { get; set; } = string.Empty;
    public Guid DestinationStorageLocationId { get; set; }
    public string DestinationStorageLocationName { get; set; } = string.Empty;

    public StockTransferStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? IssuedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }

    public int ItemCount { get; set; }
    public int Version { get; set; }

    /// <summary>
    /// Perintah yang sah pada keadaan sekarang, dihitung backend agar layar tidak perlu
    /// menyalin aturan transisinya dan tidak dapat menyimpang darinya.
    /// </summary>
    public bool IsEditable { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
    public bool CanIssue { get; set; }
    public bool CanReceive { get; set; }
    public bool CanCancel { get; set; }
}

public class StockTransferDetailResponse : StockTransferSummaryResponse
{
    public Guid RequestedByWorkforceId { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? DecisionReason { get; set; }

    public List<StockTransferItemResponse> Items { get; set; } = [];
    public List<StockTransferHistoryResponse> Histories { get; set; } = [];
}

public class StockTransferItemResponse
{
    public Guid Id { get; set; }
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;

    public decimal RequestedQuantity { get; set; }
    public decimal? IssuedQuantity { get; set; }
    public decimal? ReceivedQuantity { get; set; }

    public string? Note { get; set; }
    public int LineNumber { get; set; }

    /// <summary>Batch yang ditahan untuk baris ini, ditentukan saat persetujuan menurut FEFO.</summary>
    public List<StockTransferAllocationResponse> Allocations { get; set; } = [];
}

public class StockTransferAllocationResponse
{
    public Guid DrugBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public bool IsReleased { get; set; }
}

public class StockTransferHistoryResponse
{
    public Guid Id { get; set; }
    public StockTransferStatus? FromStatus { get; set; }
    public StockTransferStatus ToStatus { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime OccurredAt { get; set; }
}
