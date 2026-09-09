using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;

/// <summary>
/// Penanda kelengkapan penyerahan sebuah baris resep.
/// </summary>
/// <remarks>
/// Diturunkan dari jumlah sisa setiap kali dibaca, tidak pernah disimpan. Menyimpannya
/// sebagai status tersendiri akan menciptakan kemungkinan penanda dan histori penyerahan
/// menyatakan dua hal yang berbeda.
/// </remarks>
public enum PrescriptionItemDispensingMark
{
    /// <summary>Sisa nol — seluruhnya sudah diserahkan.</summary>
    Det = 1,

    /// <summary>Masih ada sisa yang belum diserahkan.</summary>
    Nedet = 2
}

// ------------------------------------------------------------------- permintaan

public class PreparePrescriptionDispensingRequest
{
    /// <summary>Depo yang stoknya ditahan dan kemudian berkurang.</summary>
    [Required] public Guid StorageLocationId { get; set; }

    [Required] public Guid PreparedByWorkforceId { get; set; }

    [Required, MinLength(1)]
    public List<PrescriptionDispensingItemInput> Items { get; set; } = [];

    [MaxLength(1000)] public string? Notes { get; set; }

    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class PrescriptionDispensingItemInput
{
    [Required] public Guid PrescriptionItemId { get; set; }

    /// <summary>
    /// Jumlah yang disiapkan untuk diserahkan kali ini.
    /// </summary>
    /// <remarks>
    /// Boleh lebih kecil dari jumlah yang diresepkan — itulah penyerahan bertahap. Tidak
    /// boleh melebihi sisa yang belum diserahkan.
    /// </remarks>
    [Range(0.001, 1000000)] public decimal Quantity { get; set; }

    [MaxLength(500)] public string? Note { get; set; }
}

public class PrescriptionDispensingCommandRequest
{
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class CancelPrescriptionDispensingRequest
{
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;

    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

// ---------------------------------------------------------------------- jawaban

/// <summary>
/// Rekapitulasi penyerahan seluruh baris satu resep.
/// </summary>
/// <remarks>
/// Inilah bahan copy resep. Seluruh angkanya dihitung dari histori penyerahan yang benar-benar
/// tercatat, bukan dari status resep, sehingga tidak mungkin menyatakan obat sudah diserahkan
/// padahal stoknya tidak pernah berkurang.
/// </remarks>
public class PrescriptionDispensingSummaryResponse
{
    public Guid PrescriptionId { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public PrescriptionFulfillmentStatus FulfillmentStatus { get; set; }

    /// <summary>True bila seluruh baris resep sudah tidak bersisa.</summary>
    public bool IsFullyDispensed { get; set; }

    public List<PrescriptionItemDispensingResponse> Items { get; set; } = [];

    /// <summary>Penyerahan yang membentuk angka di atas, terbaru lebih dahulu.</summary>
    public List<PrescriptionDispensingEventResponse> History { get; set; } = [];
}

public class PrescriptionItemDispensingResponse
{
    public Guid PrescriptionItemId { get; set; }
    public int LineNumber { get; set; }
    public Guid DrugId { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public string? DispenseUnit { get; set; }

    /// <summary>Jumlah yang diresepkan dokter.</summary>
    public decimal QuantityPrescribed { get; set; }

    /// <summary>Jumlah yang sudah benar-benar diserahkan dan mengurangi stok.</summary>
    public decimal QuantityDispensed { get; set; }

    /// <summary>
    /// Jumlah yang sedang ditahan untuk penyerahan yang masih berupa draft.
    /// </summary>
    /// <remarks>
    /// Belum diserahkan, tetapi sudah tidak dapat dijanjikan kepada pasien lain. Ditampilkan
    /// terpisah supaya petugas tidak menyiapkan jumlah yang sama dua kali.
    /// </remarks>
    public decimal QuantityReserved { get; set; }

    /// <summary>Diresepkan dikurangi yang sudah diserahkan. Tidak pernah negatif.</summary>
    public decimal QuantityRemaining { get; set; }

    /// <summary>Diturunkan dari sisa; tidak disimpan.</summary>
    public PrescriptionItemDispensingMark Mark { get; set; }
}

public class PrescriptionDispensingEventResponse
{
    public Guid DrugUsageId { get; set; }
    public string UsageNumber { get; set; } = string.Empty;
    public DrugUsageStatus Status { get; set; }
    public Guid StorageLocationId { get; set; }
    public string? StorageLocationName { get; set; }
    public DateTime UsedAt { get; set; }
    public DateTime? RecordedAt { get; set; }
    public int Version { get; set; }
    public List<PrescriptionDispensingEventItemResponse> Items { get; set; } = [];
}

public class PrescriptionDispensingEventItemResponse
{
    public Guid DrugUsageItemId { get; set; }
    public Guid? PrescriptionItemId { get; set; }
    public Guid DrugId { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public List<PrescriptionDispensingAllocationResponse> Allocations { get; set; } = [];
}

public class PrescriptionDispensingAllocationResponse
{
    public Guid DrugBatchId { get; set; }
    public string? BatchNumber { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public int SequenceNumber { get; set; }
}
