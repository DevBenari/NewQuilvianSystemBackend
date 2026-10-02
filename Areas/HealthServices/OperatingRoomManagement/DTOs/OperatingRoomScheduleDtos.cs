using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

public class OprTeamMemberRequest
{
    [Required] public Guid WorkforceId { get; set; }
    [Required] public OprTeamRole Role { get; set; }
    public bool IsLead { get; set; }
}

public class ScheduleOprCaseRequest
{
    [Required] public Guid RoomId { get; set; }
    [Required] public DateTime StartAt { get; set; }
    [Required] public DateTime EndAt { get; set; }
    [Range(0, 480)] public int? BufferBeforeMinutes { get; set; }
    [Range(0, 480)] public int? BufferAfterMinutes { get; set; }
    [MaxLength(500)] public string? ChangeReason { get; set; }
    [Required, MinLength(1)] public List<OprTeamMemberRequest> TeamMembers { get; set; } = [];
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }
}

public class PostponeOprCaseRequest
{
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    [Required] public Guid ConfirmedByDoctorId { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }
}

public class OprTeamMemberResponse
{
    public Guid WorkforceId { get; set; }
    public string WorkforceName { get; set; } = string.Empty;
    public OprTeamRole Role { get; set; }
    public bool IsLead { get; set; }
    public OprCredentialCheckStatus CredentialCheckStatus { get; set; }
    public DateTime? CredentialCheckedAt { get; set; }
}

public class OprScheduleResponse
{
    public Guid Id { get; set; }
    public Guid OprCaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int BufferBeforeMinutes { get; set; }
    public int BufferAfterMinutes { get; set; }
    public int Revision { get; set; }
    public string? ChangeReason { get; set; }

    /// <summary>Menandai revisi yang sedang berlaku; berguna pada daftar histori.</summary>
    public bool IsCurrent { get; set; }
    public OprCaseStatus Status { get; set; }
    public int Version { get; set; }
    public List<OprTeamMemberResponse> TeamMembers { get; set; } = [];
    public List<string> AvailableActions { get; set; } = [];
}

public class OprCaseStatusResponse
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public OprCaseStatus Status { get; set; }
    public int Version { get; set; }
    public string? Reason { get; set; }
    public List<string> AvailableActions { get; set; } = [];
}
