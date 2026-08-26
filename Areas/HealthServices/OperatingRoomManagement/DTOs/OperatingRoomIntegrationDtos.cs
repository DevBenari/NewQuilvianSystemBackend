using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

public class OprIntegrationDeliveryResponse
{
    public Guid Id { get; set; }
    public Guid OprCaseId { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string PayloadReference { get; set; } = string.Empty;
    public OprDeliveryStatus Status { get; set; }
    public int RetryCount { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public string? LastErrorCode { get; set; }
    public string? AcceptedReference { get; set; }
}

public class OprReconciliationResponse
{
    public Guid OprCaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public List<OprIntegrationDeliveryResponse> Deliveries { get; set; } = [];
    public int PendingCount { get; set; }
    public int FailedCount { get; set; }
    public int AcceptedCount { get; set; }

    /// <summary>
    /// Tujuan yang kontrak consumer-nya belum disahkan sehingga pengirimannya belum dapat
    /// dijalankan. Selama daftar ini terisi, rekonsiliasi bersifat manual.
    /// </summary>
    public List<string> BlockedDestinations { get; set; } = [];
}

public class RecordOprDeliveryAttemptRequest
{
    /// <summary>True bila consumer menerima pesan; false bila gagal dan perlu retry.</summary>
    public bool Accepted { get; set; }

    [MaxLength(150)] public string? AcceptedReference { get; set; }
    [MaxLength(100)] public string? ErrorCode { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}
