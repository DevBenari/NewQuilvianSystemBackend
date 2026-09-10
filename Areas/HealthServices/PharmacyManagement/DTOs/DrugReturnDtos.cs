using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;

// ===================================================================== permintaan

public class DrugReturnItemInput
{
    [Required] public Guid DrugId { get; set; }

    /// <summary>Wajib. Stok tidak dapat bertambah tanpa mengetahui batch dan kedaluwarsanya.</summary>
    [Required] public Guid DrugBatchId { get; set; }

    [Required] public Guid MeasurementId { get; set; }

    [Range(0.001, 1000000)]
    public decimal Quantity { get; set; }

    [MaxLength(500)] public string? Note { get; set; }
}

public class CreateDrugReturnRequest
{
    [Required] public Guid EncounterId { get; set; }
    [Required] public Guid StorageLocationId { get; set; }
    [Required] public Guid ReturnedByWorkforceId { get; set; }

    /// <summary>Pemakaian yang dikembalikan, bila diketahui.</summary>
    public Guid? SourceDrugUsageId { get; set; }

    /// <summary>
    /// Pemakaian material kamar operasi yang dikembalikan, bila retur ini berasal dari sana.
    /// </summary>
    public Guid? SourceOprMaterialUsageId { get; set; }

    public DateTime? ReturnedAt { get; set; }
    [MaxLength(1000)] public string? Reason { get; set; }

    [Required, MinLength(1)]
    public List<DrugReturnItemInput> Items { get; set; } = [];

    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class UpdateDrugReturnRequest
{
    [Required] public Guid StorageLocationId { get; set; }
    public Guid? SourceDrugUsageId { get; set; }
    public DateTime? ReturnedAt { get; set; }
    [MaxLength(1000)] public string? Reason { get; set; }

    [Required, MinLength(1)]
    public List<DrugReturnItemInput> Items { get; set; } = [];

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class DrugReturnCommandRequest
{
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class DrugReturnReasonRequest
{
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>Keputusan pemeriksa atas satu baris retur.</summary>
public class VerifyDrugReturnItemInput
{
    [Required] public Guid DrugReturnItemId { get; set; }

    /// <summary>
    /// Jumlah yang diterima kembali. Nol berarti seluruh baris ditolak; selisihnya terhadap
    /// jumlah yang diajukan adalah barang yang tidak layak kembali.
    /// </summary>
    [Range(0, 1000000)]
    public decimal AcceptedQuantity { get; set; }

    /// <summary>
    /// Keadaan barang saat masuk kembali. Siap pakai bila layak, karantina bila masih perlu
    /// diperiksa lebih lanjut, rusak bila jelas tidak dapat dipakai.
    /// </summary>
    public DrugStockStatus AcceptedStatus { get; set; } = DrugStockStatus.Available;

    [MaxLength(500)] public string? Note { get; set; }
}

public class VerifyDrugReturnRequest
{
    [Required] public Guid VerifiedByWorkforceId { get; set; }

    /// <summary>Harus memuat seluruh baris retur, termasuk yang diterima nol.</summary>
    [Required, MinLength(1)]
    public List<VerifyDrugReturnItemInput> Items { get; set; } = [];

    [MaxLength(1000)] public string? Note { get; set; }

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class DrugReturnPagedQuery
{
    public DrugReturnStatus? Status { get; set; }
    public Guid? EncounterId { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? StorageLocationId { get; set; }
    public Guid? DrugId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// ===================================================================== tanggapan

public class DrugReturnSummaryResponse
{
    public Guid Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;

    public Guid EncounterId { get; set; }
    public string EncounterNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;

    public Guid StorageLocationId { get; set; }
    public string StorageLocationName { get; set; } = string.Empty;
    public string ReturnedByName { get; set; } = string.Empty;
    public string? VerifiedByName { get; set; }

    public DrugReturnStatus Status { get; set; }
    public DateTime ReturnedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }

    public int ItemCount { get; set; }
    public int Version { get; set; }

    public bool IsEditable { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanVerify { get; set; }
    public bool CanReject { get; set; }
    public bool CanCancel { get; set; }
}

public class DrugReturnDetailResponse : DrugReturnSummaryResponse
{
    public Guid ReturnedByWorkforceId { get; set; }
    public Guid? VerifiedByWorkforceId { get; set; }
    public Guid? SourceDrugUsageId { get; set; }
    public string? SourceUsageNumber { get; set; }
    public string? Reason { get; set; }
    public string? DecisionReason { get; set; }

    public List<DrugReturnItemResponse> Items { get; set; } = [];
    public List<DrugReturnHistoryResponse> Histories { get; set; } = [];
}

public class DrugReturnItemResponse
{
    public Guid Id { get; set; }
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;

    public Guid DrugBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }

    public Guid MeasurementId { get; set; }
    public string? MeasurementName { get; set; }

    public decimal Quantity { get; set; }
    public decimal? AcceptedQuantity { get; set; }
    public DrugStockStatus AcceptedStatus { get; set; }

    public string? Note { get; set; }
    public int LineNumber { get; set; }
}

public class DrugReturnHistoryResponse
{
    public Guid Id { get; set; }
    public DrugReturnStatus? FromStatus { get; set; }
    public DrugReturnStatus ToStatus { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime OccurredAt { get; set; }
}
