using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;

public class BedVersionExpectation
{
    public Guid BedId { get; set; }
    [Range(1, long.MaxValue)] public long ExpectedVersion { get; set; }
}
public class BedOperationMeta
{
    public Guid ReceiptId { get; set; }
    public Guid ResultEntityId { get; set; }
    public string ResultKind { get; set; } = "";
    public long? CommittedVersion { get; set; }
    public DateTime CommittedAtUtc { get; set; }
    public bool IsReplay { get; set; }
}
public class OperationOutcomeResponse : BedOperationMeta { public string OperationName { get; set; } = ""; }
public class BedMonitoringQuery
{
    public Guid? ServiceUnitId { get; set; }
    public Guid? RoomId { get; set; }
    public string? Status { get; set; }
    [MaxLength(100)] public string? Search { get; set; }
}
public class BedHoldingContext
{
    public Guid? EpisodeId { get; set; }
    public string? EpisodeNumber { get; set; }
    public string? PatientName { get; set; }
    public Guid? ReservationId { get; set; }
    public DateTime? ReservationExpiresAt { get; set; }
}
public class BedOperationalResponse
{
    public Guid BedId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore] public int RawBedStatus { get; set; }
    [System.Text.Json.Serialization.JsonIgnore] public string? RawRoomCode { get; set; }
    [System.Text.Json.Serialization.JsonIgnore] public int RawRoomCapacity { get; set; }
    public string BedCode { get; set; } = "";
    public string BedName { get; set; } = "";
    public Guid RoomId { get; set; }
    public string? RoomName { get; set; }
    public Guid ServiceUnitId { get; set; }
    public Guid? PatientClassId { get; set; }
    public string? PatientClassName { get; set; }
    public string StatusCode { get; set; } = "Unavailable";
    public string StatusLabel { get; set; } = "Tidak tersedia";
    public string? CleaningPhase { get; set; }
    public bool IsBookable { get; set; }
    public long OperationalVersion { get; set; }
    public Guid CycleId { get; set; }
    public string? ConflictCode { get; set; }
    public List<string> BlockReasonCodes { get; set; } = [];
    public List<string> AvailableActions { get; set; } = [];
    public BedHoldingContext? HoldingContext { get; set; }
}
public class BedMonitoringCounts
{
    public int Total { get; set; }
    public int Available { get; set; }
    public int Occupied { get; set; }
    public int Reserved { get; set; }
    public int WaitingCleaning { get; set; }
    public int Cleaning { get; set; }
    public int Unavailable { get; set; }
}
public class BedMonitoringRoom
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public List<BedOperationalResponse> Beds { get; set; } = [];
}
public class BedMonitoringUnit
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public List<BedMonitoringRoom> Rooms { get; set; } = [];
}
public class BedMonitoringResponse
{
    public DateTime AsOfUtc { get; set; }
    public BedMonitoringCounts Counts { get; set; } = new();
    public List<BedMonitoringUnit> ServiceUnits { get; set; } = [];
}
public class BedCleaningRequest
{
    public Guid CycleId { get; set; }
    [Range(1, long.MaxValue)] public long ExpectedBedVersion { get; set; }
}
public class StartBedCleaningRequest : BedCleaningRequest { }
public class CompleteBedCleaningRequest : BedCleaningRequest { }
public class VerifyBedReadinessRequest : BedCleaningRequest
{
    public Guid? AttemptId { get; set; }
    public bool IsReady { get; set; }
    [Required, MaxLength(200)] public string EvidenceReference { get; set; } = "";
    [MaxLength(500)] public string? Reason { get; set; }
}
public class BedOperationResponse
{
    public BedOperationalResponse Bed { get; set; } = new();
    public Guid? AttemptId { get; set; }
    public BedOperationMeta? OperationMeta { get; set; }
}
public class BedCleaningAttemptResponse
{
    public Guid Id { get; set; }
    public Guid CycleId { get; set; }
    public string State { get; set; } = "";
    public DateTime StartedAtUtc { get; set; }
    public Guid StartedByUserId { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public string? RejectionReason { get; set; }
    public string SopReference { get; set; } = "";
}
public class BedTransferContextResponse
{
    public Guid EpisodeId { get; set; }
    public Guid SourcePlacementId { get; set; }
    public int SourcePlacementVersion { get; set; }
    public BedOperationalResponse SourceBed { get; set; } = new();
    public string? LocationSnapshotJson { get; set; }
    public DateTime AsOfUtc { get; set; }
}
public class BedUsageHistoryQuery
{
    public Guid BedId { get; set; }
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
    public Guid? RoomId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
public class BedUsageHistoryResponse
{
    public Guid PlacementId { get; set; }
    public Guid? EpisodeId { get; set; }
    public string? PatientIdentity { get; set; }
    public Guid BedId { get; set; }
    public Guid RoomId { get; set; }
    public Guid ServiceUnitId { get; set; }
    public Guid PatientClassId { get; set; }
    public string? LocationSnapshot { get; set; }
    public string ContextSource { get; set; } = "";
    public int SequenceNumber { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public int? EndReason { get; set; }
    public int? TransferCategory { get; set; }
    public Guid? TransferFromPlacementId { get; set; }
    public int Version { get; set; }
    public Guid? CorrectsPlacementId { get; set; }
    public Guid? SupersededByCorrectionId { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsCorrection { get; set; }
    public bool IsEffectiveForBilling { get; set; }
}

