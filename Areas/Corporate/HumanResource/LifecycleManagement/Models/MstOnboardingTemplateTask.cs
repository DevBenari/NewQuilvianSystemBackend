using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("MstOnboardingTemplateTask", Schema = "public")]
    public class MstOnboardingTemplateTask : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OnboardingTemplateId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TaskCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string TaskName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TaskCategory { get; set; } = "General";

        [MaxLength(1000)]
        public string? Description { get; set; }

        public OnboardingResponsibleRole ResponsibleRole { get; set; } = OnboardingResponsibleRole.Hrd;

        public OnboardingItemKind ItemKind { get; set; } = OnboardingItemKind.Manual;

        public OnboardingAutomaticSource? AutomaticSource { get; set; }

        public Guid? CertificationTypeId { get; set; }

        [MaxLength(40)]
        public string? SystemItemCode { get; set; }

        public bool IsRequired { get; set; } = true;

        public int DueDayOffset { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstOnboardingTemplate? OnboardingTemplate { get; set; }
        public MstCertificationType? CertificationType { get; set; }
    }
}
