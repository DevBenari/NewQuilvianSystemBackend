using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceOffboardingTaskResponse
    {
        public Guid Id { get; set; }

        public Guid OffboardingChecklistId { get; set; }

        public string TaskCode { get; set; } = string.Empty;

        public string TaskName { get; set; } = string.Empty;

        public OffboardingTaskCategory TaskCategory { get; set; }

        public bool IsRequired { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }

        public Guid? CompletedByUserId { get; set; }

        public string? CompletedByUserName { get; set; }

        public string? Notes { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class CreateWorkforceOffboardingTaskRequest
    {
        [Required]
        [MaxLength(100)]
        public string TaskCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TaskName { get; set; } = string.Empty;

        public OffboardingTaskCategory TaskCategory { get; set; } = OffboardingTaskCategory.Other;

        public bool IsRequired { get; set; } = true;

        public bool IsCompleted { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceOffboardingTaskRequest
    {
        [Required]
        [MaxLength(100)]
        public string TaskCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TaskName { get; set; } = string.Empty;

        public OffboardingTaskCategory TaskCategory { get; set; } = OffboardingTaskCategory.Other;

        public bool IsRequired { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public class CompleteWorkforceOffboardingTaskRequest
    {
        public bool IsCompleted { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateWorkforceOffboardingTaskStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
