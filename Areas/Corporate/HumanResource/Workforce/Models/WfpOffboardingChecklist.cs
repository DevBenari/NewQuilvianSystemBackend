using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpOffboardingChecklist", Schema = "public")]
    public class WfpOffboardingChecklist : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public OffboardingType OffboardingType { get; set; } = OffboardingType.Unknown;

        [Required]
        public DateTime EffectiveEndDate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public OffboardingStatus Status { get; set; } = OffboardingStatus.Draft;

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public ICollection<WfpOffboardingTask> Tasks { get; set; } = new List<WfpOffboardingTask>();
    }

    [Table("WfpOffboardingTask", Schema = "public")]
    public class WfpOffboardingTask : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OffboardingChecklistId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TaskCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TaskName { get; set; } = string.Empty;

        public OffboardingTaskCategory TaskCategory { get; set; } = OffboardingTaskCategory.Other;

        public bool IsRequired { get; set; } = true;

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        public Guid? CompletedByUserId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public WfpOffboardingChecklist? OffboardingChecklist { get; set; }

        public ApplicationUser? CompletedByUser { get; set; }
    }
}
