using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

public class CreateOprMaterialUsageRequest
{
    /// <summary>Identitas item milik Inventory/Farmasi; modul operasi tidak menyalin masternya.</summary>
    [Required] public Guid ExternalItemId { get; set; }

    [Required] public OprMaterialItemType ItemType { get; set; }
    public decimal Quantity { get; set; }
    [Required, MaxLength(30)] public string UnitCode { get; set; } = string.Empty;
    [Required] public OprMaterialOutcome Outcome { get; set; }
    [MaxLength(100)] public string? BatchNumber { get; set; }
    [MaxLength(150)] public string? SerialNumber { get; set; }
    public DateTime? OccurredAt { get; set; }

    /// <summary>Wajib saat `Outcome` bernilai `Corrected`: catatan yang dikoreksi.</summary>
    public Guid? CorrectionOfUsageId { get; set; }

    [MaxLength(2000)] public string? CorrectionReason { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class OprMaterialUsageResponse
{
    public Guid Id { get; set; }
    public Guid OprCaseId { get; set; }
    public Guid ExternalItemId { get; set; }

    /// <summary>False bila item belum dapat dikenali master Inventory/Farmasi yang tersedia.</summary>
    public bool IsItemResolved { get; set; }

    public string ItemName { get; set; } = string.Empty;
    public OprMaterialItemType ItemType { get; set; }
    public decimal Quantity { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public OprMaterialOutcome Outcome { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime OccurredAt { get; set; }
    public Guid RecordedBy { get; set; }
    public int Revision { get; set; }
    public string? CorrectionReason { get; set; }
}

public class OprMaterialLedgerResponse
{
    public Guid OprCaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public List<OprMaterialUsageResponse> Entries { get; set; } = [];

    /// <summary>Jumlah entri yang itemnya belum dapat divalidasi; menandai dependency terbuka.</summary>
    public int UnresolvedItemCount { get; set; }
}
