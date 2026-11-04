using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("HrdOnboardingCaseItem", Schema = "public")]
    public class HrdOnboardingCaseItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OnboardingCaseId { get; set; }

        public OnboardingTemplateLayer TemplateLayer { get; set; }

        public Guid? SourceTemplateTaskId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string ItemName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? TaskCategory { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(40)]
        public string? SystemItemCode { get; set; }

        public OnboardingResponsibleRole ResponsibleRole { get; set; }

        public OnboardingItemKind ItemKind { get; set; } = OnboardingItemKind.Manual;

        public OnboardingAutomaticSource? AutomaticSource { get; set; }

        public Guid? CertificationTypeId { get; set; }

        public bool IsRequired { get; set; } = true;

        public int DueDayOffset { get; set; } = 0;

        public DateOnly DueDate { get; set; }

        public OnboardingItemStatus ItemStatus { get; set; } = OnboardingItemStatus.Pending;

        public DateTime? CompletedAt { get; set; }

        public Guid? CompletedByUserId { get; set; }

        [MaxLength(1000)]
        public string? CompletionNote { get; set; }

        public Guid? SelectedWorkforceProfileId { get; set; }

        [MaxLength(500)]
        public string? EvidenceStoragePath { get; set; }

        [MaxLength(250)]
        public string? EvidenceFileName { get; set; }

        [MaxLength(150)]
        public string? EvidenceContentType { get; set; }

        public long? EvidenceFileSize { get; set; }

        public DateTime? AutomaticCheckedAt { get; set; }

        [MaxLength(250)]
        public string? AutomaticResultNote { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public HrdOnboardingCase? OnboardingCase { get; set; }
        public MstOnboardingTemplateTask? SourceTemplateTask { get; set; }
        public MstCertificationType? CertificationType { get; set; }
        public ApplicationUser? CompletedByUser { get; set; }
        public MstWorkforceProfile? SelectedWorkforceProfile { get; set; }
    }
}
