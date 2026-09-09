using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;

// ===================================================================== permintaan

public class DrugUsageItemInput
{
    [Required] public Guid DrugId { get; set; }
    [Required] public Guid MeasurementId { get; set; }

    [Range(0.001, 1000000)]
    public decimal Quantity { get; set; }

    [MaxLength(500)] public string? Note { get; set; }
}

public class CreateDrugUsageRequest
{
    [Required] public Guid EncounterId { get; set; }
    [Required] public Guid StorageLocationId { get; set; }
    [Required] public Guid RecordedByWorkforceId { get; set; }

    /// <summary>Kapan obatnya dipakai. Kosong berarti saat perintah ini dikirim.</summary>
    public DateTime? UsedAt { get; set; }

    [MaxLength(1000)] public string? Notes { get; set; }

    [Required, MinLength(1)]
    public List<DrugUsageItemInput> Items { get; set; } = [];

    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class UpdateDrugUsageRequest
{
    [Required] public Guid StorageLocationId { get; set; }
    public DateTime? UsedAt { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    [Required, MinLength(1)]
    public List<DrugUsageItemInput> Items { get; set; } = [];

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class DrugUsageCommandRequest
{
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class CancelDrugUsageRequest
{
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class DrugUsagePagedQuery
{
    public DrugUsageStatus? Status { get; set; }
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

public class DrugUsageSummaryResponse
{
    public Guid Id { get; set; }
    public string UsageNumber { get; set; } = string.Empty;

    public Guid EncounterId { get; set; }
    public string EncounterNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string ServiceUnitName { get; set; } = string.Empty;

    public Guid StorageLocationId { get; set; }
    public string StorageLocationName { get; set; } = string.Empty;
    public string RecordedByName { get; set; } = string.Empty;

    public DrugUsageStatus Status { get; set; }
    public DateTime UsedAt { get; set; }
    public DateTime? RecordedAt { get; set; }
    public DateTime? BilledAt { get; set; }

    public int ItemCount { get; set; }
    public int Version { get; set; }

    /// <summary>
    /// Perintah yang sah pada keadaan sekarang, dihitung backend agar layar tidak perlu
    /// menyalin aturan transisinya.
    /// </summary>
    public bool IsEditable { get; set; }
    public bool CanRecord { get; set; }
    public bool CanCancel { get; set; }
}

public class DrugUsageDetailResponse : DrugUsageSummaryResponse
{
    public Guid RecordedByWorkforceId { get; set; }
    public string? Notes { get; set; }
    public string? CancelReason { get; set; }
    public List<DrugUsageItemResponse> Items { get; set; } = [];
}

public class DrugUsageItemResponse
{
    public Guid Id { get; set; }
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public Guid MeasurementId { get; set; }
    public string? MeasurementName { get; set; }
    public decimal Quantity { get; set; }
    public string? Note { get; set; }
    public int LineNumber { get; set; }

    /// <summary>Batch yang benar-benar dipakai; terisi setelah pemakaian dicatat.</summary>
    public List<DrugUsageAllocationResponse> Allocations { get; set; } = [];
}

public class DrugUsageAllocationResponse
{
    public Guid DrugBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
}
