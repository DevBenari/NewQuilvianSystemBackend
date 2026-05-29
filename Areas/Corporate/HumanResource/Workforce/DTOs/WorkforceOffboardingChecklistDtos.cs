using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceOffboardingChecklistResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public OffboardingType OffboardingType { get; set; }

        public DateTime EffectiveEndDate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public OffboardingStatus Status { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public int TotalTask { get; set; }

        public int RequiredTask { get; set; }

        public int CompletedTask { get; set; }

        public int PendingTask { get; set; }

        public int RequiredPendingTask { get; set; }

        public decimal CompletionPercentage { get; set; }

        public DateTime CreateDateTime { get; set; }

        public List<WorkforceOffboardingTaskResponse> Tasks { get; set; } = new();
    }

    public class WorkforceOffboardingChecklistListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int DraftData { get; set; }

        public int InProgressData { get; set; }

        public int CompletedData { get; set; }

        public int CancelledData { get; set; }

        public List<WorkforceOffboardingChecklistResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceOffboardingChecklistRequest
    {
        [Required]
        public OffboardingType OffboardingType { get; set; } = OffboardingType.Unknown;

        [Required]
        public DateTime EffectiveEndDate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public bool GenerateDefaultTasks { get; set; } = true;
    }

    public class UpdateWorkforceOffboardingChecklistRequest
    {
        [Required]
        public OffboardingType OffboardingType { get; set; } = OffboardingType.Unknown;

        [Required]
        public DateTime EffectiveEndDate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public OffboardingStatus Status { get; set; } = OffboardingStatus.InProgress;

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceOffboardingChecklistStatusRequest
    {
        [Required]
        public OffboardingStatus Status { get; set; }

        public DateTime? CompletedDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
