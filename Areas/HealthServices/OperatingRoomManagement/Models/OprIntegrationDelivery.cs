using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprIntegrationDelivery : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OprCaseId { get; set; }

    /// <summary>
    /// Identitas kejadian bisnisnya, dipakai consumer untuk membuang kiriman ganda (`BE-OPR-009`).
    /// </summary>
    /// <remarks>
    /// Terpisah dari <see cref="Id"/> karena keduanya menjawab pertanyaan berbeda:
    /// <see cref="Id"/> menunjuk baris outbox, sedangkan ini menunjuk kejadian yang dilaporkan.
    /// Satu kejadian tetap satu <c>EventId</c> walau barisnya diantrekan ulang berkali-kali.
    /// </remarks>
    public Guid EventId { get; set; } = Guid.NewGuid();

    /// <summary>Nama kejadian bisnis, misalnya `operating-room.material-usage.recorded`.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Versi bentuk amplopnya.</summary>
    public string EventVersion { get; set; } = string.Empty;

    /// <summary>Waktu kejadiannya, bukan waktu pengirimannya.</summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// Amplop kejadian yang sudah dibekukan sebagai JSON, sesuai
    /// <see cref="DTOs.OprIntegrationEvent"/>.
    /// </summary>
    /// <remarks>
    /// Disimpan, bukan disusun ulang saat dibaca, supaya pesan yang sudah terlanjur dikirim
    /// tidak ikut berubah ketika jadwal, tim, atau tindakan kasusnya berubah kemudian.
    /// </remarks>
    public string PayloadJson { get; set; } = string.Empty;

    public string Destination { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string PayloadReference { get; set; } = string.Empty;
    public OprDeliveryStatus Status { get; set; } = OprDeliveryStatus.Pending;
    public int RetryCount { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public string? LastErrorCode { get; set; }
    public string? AcceptedReference { get; set; }
    public OprCase? OprCase { get; set; }
}
