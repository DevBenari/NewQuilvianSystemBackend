using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprMaterialUsage : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OprCaseId { get; set; }
    public Guid ExternalItemId { get; set; }
    public OprMaterialItemType ItemType { get; set; }
    public decimal Quantity { get; set; }
    public string UnitCode { get; set; } = string.Empty;

    /// <summary>
    /// Satuan yang dipakai, merujuk `MstMeasurement` milik Farmasi.
    /// </summary>
    /// <remarks>
    /// Nullable hanya demi baris lama yang tercatat sebelum satuan diwajibkan. Baris tanpa
    /// satuan tidak dapat dibukukan ke stok, dan pembukuannya menolak dengan sebab yang jelas
    /// alih-alih menebak satuannya.
    /// </remarks>
    public Guid? UnitMeasurementId { get; set; }
    public OprMaterialOutcome Outcome { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime OccurredAt { get; set; }
    public Guid RecordedBy { get; set; }
    public int Revision { get; set; }
    /// <summary>
    /// Baris pemakaian yang dikoreksi oleh baris ini.
    /// </summary>
    /// <remarks>
    /// Tanpa tautan ini sebuah koreksi hanya menaikkan nomor revisi tanpa menyebut apa yang
    /// dikoreksi, sehingga selisih stok yang harus dibukukan tidak dapat dihitung dan
    /// rekonsiliasi tidak dapat menjelaskan dirinya sendiri.
    /// </remarks>
    public Guid? CorrectionOfUsageId { get; set; }
    public string? CorrectionReason { get; set; }
    public MstMeasurement? UnitMeasurement { get; set; }
    public OprCase? OprCase { get; set; }
}
