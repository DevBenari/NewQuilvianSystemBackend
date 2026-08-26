using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

public class OprChecklistItemRequest
{
    [Required, MaxLength(50)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(300)] public string Label { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public bool IsChecked { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
}

public class SaveOprChecklistRequest
{
    [Required, MaxLength(50)] public string TemplateVersion { get; set; } = string.Empty;
    [Required, MinLength(1)] public List<OprChecklistItemRequest> Items { get; set; } = [];

    /// <summary>Bila true checklist difinalisasi; bila false disimpan sebagai draft.</summary>
    public bool Complete { get; set; }

    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }
}

public class CreateOprReadinessSignOffRequest
{
    [Required] public OprReadinessRole Role { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }
}

public class CreateOprEmergencyBypassRequest
{
    [Required, MaxLength(2000)] public string Reason { get; set; } = string.Empty;
    [Required] public Guid ResponsibleUserId { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }
}

public class OprChecklistItemResponse
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public bool IsChecked { get; set; }
    public string? Note { get; set; }
}

public class OprChecklistResponse
{
    public Guid Id { get; set; }
    public OprChecklistPhase Phase { get; set; }
    public string TemplateVersion { get; set; } = string.Empty;
    public int Revision { get; set; }
    public OprChecklistStatus Status { get; set; }
    public List<OprChecklistItemResponse> Items { get; set; } = [];
    public Guid? SignedByUserId { get; set; }
    public DateTime? SignedAt { get; set; }
    public bool IsEmergencyBypass { get; set; }
    public string? BypassReason { get; set; }
    public Guid? BypassResponsibleUserId { get; set; }
    public DateTime? CompletedAfterStableAt { get; set; }
}

/// <summary>Ringkasan consent milik ClinicalManagement; isi consent tidak disalin.</summary>
public class OprConsentStatusResponse
{
    public PatientConsentType ConsentType { get; set; }
    public Guid? ConsentId { get; set; }
    public string? ConsentNumber { get; set; }
    public PatientConsentStatus? ConsentStatus { get; set; }
    public bool IsValid { get; set; }
}

public class OprReadinessSignOffResponse
{
    public OprReadinessRole Role { get; set; }
    public Guid ActorUserId { get; set; }
    public DateTime SignedAt { get; set; }
    public string? Notes { get; set; }
}

public class OprPreparationResponse
{
    public Guid OprCaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public OprCaseStatus Status { get; set; }
    public int Version { get; set; }
    public List<OprConsentStatusResponse> Consents { get; set; } = [];
    public List<OprChecklistResponse> Checklists { get; set; } = [];
    public List<OprReadinessSignOffResponse> SignOffs { get; set; } = [];

    /// <summary>Prasyarat yang belum terpenuhi; kosong berarti kasus sudah boleh `Ready`.</summary>
    public List<string> OutstandingRequirements { get; set; } = [];

    public bool IsEmergencyBypassActive { get; set; }
    public List<string> AvailableActions { get; set; } = [];
}
