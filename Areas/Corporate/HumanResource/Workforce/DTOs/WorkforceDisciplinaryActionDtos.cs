using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceDisciplinaryActionResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public DisciplinaryActionType ActionType { get; set; }

        public DateTime IncidentDate { get; set; }

        public DateTime IssuedDate { get; set; }

        public DisciplinarySeverityLevel SeverityLevel { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid IssuedByUserId { get; set; }

        public string? IssuedByUserName { get; set; }

        public DateTime? EffectiveUntil { get; set; }

        public string? FilePath { get; set; }

        public bool HasFile { get; set; }

        public DisciplinaryActionStatus ActionStatus { get; set; }

        public string? Notes { get; set; }

        public bool IsExpired { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceDisciplinaryActionListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int DraftData { get; set; }

        public int IssuedData { get; set; }

        public int AcknowledgedData { get; set; }

        public int UnderReviewData { get; set; }

        public int ResolvedData { get; set; }

        public int CancelledData { get; set; }

        public int ExpiredData { get; set; }

        public int HighSeverityData { get; set; }

        public int CriticalSeverityData { get; set; }

        public int WithFileData { get; set; }

        public List<WorkforceDisciplinaryActionResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceDisciplinaryActionRequest
    {
        public DisciplinaryActionType ActionType { get; set; } = DisciplinaryActionType.Unknown;

        [Required]
        public DateTime IncidentDate { get; set; }

        [Required]
        public DateTime IssuedDate { get; set; }

        public DisciplinarySeverityLevel SeverityLevel { get; set; } = DisciplinarySeverityLevel.Low;

        [Required]
        [MaxLength(250)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public Guid IssuedByUserId { get; set; }

        public DateTime? EffectiveUntil { get; set; }

        public IFormFile? File { get; set; }

        public DisciplinaryActionStatus ActionStatus { get; set; } = DisciplinaryActionStatus.Draft;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceDisciplinaryActionRequest
    {
        public DisciplinaryActionType ActionType { get; set; } = DisciplinaryActionType.Unknown;

        [Required]
        public DateTime IncidentDate { get; set; }

        [Required]
        public DateTime IssuedDate { get; set; }

        public DisciplinarySeverityLevel SeverityLevel { get; set; } = DisciplinarySeverityLevel.Low;

        [Required]
        [MaxLength(250)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public Guid IssuedByUserId { get; set; }

        public DateTime? EffectiveUntil { get; set; }

        public IFormFile? File { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        public DisciplinaryActionStatus ActionStatus { get; set; } = DisciplinaryActionStatus.Draft;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceDisciplinaryActionStatusRequest
    {
        public DisciplinaryActionStatus ActionStatus { get; set; } = DisciplinaryActionStatus.Issued;

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
