using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpOnboardingChecklist", Schema = "public")]
    public class WfpOnboardingChecklist : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public OnboardingType OnboardingType { get; set; } = OnboardingType.Unknown;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime TargetCompletionDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public OnboardingStatus Status { get; set; } = OnboardingStatus.Draft;

        public Guid? AssignedToUserId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public ApplicationUser? AssignedToUser { get; set; }

        public ICollection<WfpOnboardingTask> Tasks { get; set; } = new List<WfpOnboardingTask>();
    }

    [Table("WfpOnboardingTask", Schema = "public")]
    public class WfpOnboardingTask : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OnboardingChecklistId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TaskCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TaskName { get; set; } = string.Empty;

        public OnboardingTaskCategory TaskCategory { get; set; } = OnboardingTaskCategory.Other;

        public bool IsRequired { get; set; } = true;

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        public Guid? CompletedByUserId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public WfpOnboardingChecklist? OnboardingChecklist { get; set; }

        public ApplicationUser? CompletedByUser { get; set; }
    }
}
