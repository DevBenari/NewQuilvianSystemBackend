using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceOnboardingChecklistResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public OnboardingType OnboardingType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime TargetCompletionDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public OnboardingStatus Status { get; set; }

        public Guid? AssignedToUserId { get; set; }

        public string? AssignedToUserName { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public int TotalTask { get; set; }

        public int RequiredTask { get; set; }

        public int CompletedTask { get; set; }

        public int PendingTask { get; set; }

        public int RequiredPendingTask { get; set; }

        public decimal CompletionPercentage { get; set; }

        public DateTime CreateDateTime { get; set; }

        public List<WorkforceOnboardingTaskResponse> Tasks { get; set; } = new();
    }

    public class WorkforceOnboardingChecklistListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int DraftData { get; set; }

        public int InProgressData { get; set; }

        public int CompletedData { get; set; }

        public List<WorkforceOnboardingChecklistResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceOnboardingChecklistRequest
    {
        [Required]
        public OnboardingType OnboardingType { get; set; } = OnboardingType.Unknown;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime TargetCompletionDate { get; set; }

        public Guid? AssignedToUserId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public bool GenerateDefaultTasks { get; set; } = true;
    }

    public class UpdateWorkforceOnboardingChecklistRequest
    {
        [Required]
        public OnboardingType OnboardingType { get; set; } = OnboardingType.Unknown;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime TargetCompletionDate { get; set; }

        public OnboardingStatus Status { get; set; } = OnboardingStatus.InProgress;

        public Guid? AssignedToUserId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceOnboardingChecklistStatusRequest
    {
        [Required]
        public OnboardingStatus Status { get; set; }

        public DateTime? CompletedDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
