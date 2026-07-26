using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("WfpOnboardingTask", Schema = "public")]
    public class WfpOnboardingTask : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required] public Guid OnboardingChecklistId { get; set; }
        public Guid? OnboardingTemplateTaskId { get; set; }
        [Required, MaxLength(50)] public string TaskCode { get; set; } = string.Empty;
        [Required, MaxLength(250)] public string TaskName { get; set; } = string.Empty;
        [MaxLength(50)] public string TaskCategory { get; set; } = "General";
        [MaxLength(1000)] public string? Description { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public Guid? AssignedToWorkforceProfileId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        [MaxLength(30)] public string TaskStatus { get; set; } = "Pending";
        public bool IsRequired { get; set; } = true;
        public bool RequiresDocument { get; set; }
        [MaxLength(500)] public string? DocumentPath { get; set; }
        [MaxLength(250)] public string? OriginalFileName { get; set; }
        [MaxLength(150)] public string? ContentType { get; set; }
        public int SortOrder { get; set; }
        [MaxLength(1000)] public string? CompletionNote { get; set; }
        [MaxLength(1000)] public string? VerificationNote { get; set; }
        public bool IsActive { get; set; } = true;
        public WfpOnboardingChecklist? OnboardingChecklist { get; set; }
        public MstOnboardingTemplateTask? OnboardingTemplateTask { get; set; }
        public ApplicationUser? AssignedToUser { get; set; }
        public MstWorkforceProfile? AssignedToWorkforceProfile { get; set; }
        public ApplicationUser? CompletedByUser { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
