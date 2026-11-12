using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    public sealed class CreateOnboardingTemplateTaskRequest
    {
        [Required(ErrorMessage = "TaskCode wajib diisi.")]
        [MaxLength(50)]
        public string TaskCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "TaskName wajib diisi.")]
        [MaxLength(250)]
        public string TaskName { get; set; } = string.Empty;

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
    }

    public sealed class CreateOnboardingTemplateRequest
    {
        [Required(ErrorMessage = "TemplateCode wajib diisi.")]
        [MaxLength(50)]
        public string TemplateCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "TemplateName wajib diisi.")]
        [MaxLength(250)]
        public string TemplateName { get; set; } = string.Empty;

        [Required(ErrorMessage = "TemplateLayer wajib ditentukan.")]
        public OnboardingTemplateLayer TemplateLayer { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        [Range(1, 365, ErrorMessage = "TargetCompletionDays harus antara 1 sampai 365 hari.")]
        public int TargetCompletionDays { get; set; } = 30;

        [MaxLength(1500)]
        public string? Description { get; set; }

        public List<CreateOnboardingTemplateTaskRequest> Tasks { get; set; } = new();
    }

    public sealed class UpdateOnboardingTemplateTaskRequest
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "TaskCode wajib diisi.")]
        [MaxLength(50)]
        public string TaskCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "TaskName wajib diisi.")]
        [MaxLength(250)]
        public string TaskName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string TaskCategory { get; set; } = "General";

        [MaxLength(1000)]
        public string? Description { get; set; }

        public OnboardingResponsibleRole ResponsibleRole { get; set; }

        public OnboardingItemKind ItemKind { get; set; }

        public OnboardingAutomaticSource? AutomaticSource { get; set; }

        public Guid? CertificationTypeId { get; set; }

        [MaxLength(40)]
        public string? SystemItemCode { get; set; }

        public bool IsRequired { get; set; } = true;

        public int DueDayOffset { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public sealed class UpdateOnboardingTemplateRequest
    {
        [Required(ErrorMessage = "TemplateName wajib diisi.")]
        [MaxLength(250)]
        public string TemplateName { get; set; } = string.Empty;

        [Range(1, 365, ErrorMessage = "TargetCompletionDays harus antara 1 sampai 365 hari.")]
        public int TargetCompletionDays { get; set; } = 30;

        [MaxLength(1500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public List<UpdateOnboardingTemplateTaskRequest> Tasks { get; set; } = new();
    }

    public sealed class OnboardingTemplateTaskResponse
    {
        public Guid Id { get; set; }
        public Guid OnboardingTemplateId { get; set; }
        public string TaskCode { get; set; } = string.Empty;
        public string TaskName { get; set; } = string.Empty;
        public string TaskCategory { get; set; } = string.Empty;
        public string? Description { get; set; }
        public OnboardingResponsibleRole ResponsibleRole { get; set; }
        public string ResponsibleRoleName => ResponsibleRole.ToString();
        public OnboardingItemKind ItemKind { get; set; }
        public string ItemKindName => ItemKind.ToString();
        public OnboardingAutomaticSource? AutomaticSource { get; set; }
        public string? AutomaticSourceName => AutomaticSource?.ToString();
        public Guid? CertificationTypeId { get; set; }
        public string? CertificationTypeName { get; set; }
        public string? SystemItemCode { get; set; }
        public bool IsSystemItem => !string.IsNullOrWhiteSpace(SystemItemCode);
        public bool IsRequired { get; set; }
        public int DueDayOffset { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class OnboardingTemplateResponse
    {
        public Guid Id { get; set; }
        public string TemplateCode { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public OnboardingTemplateLayer TemplateLayer { get; set; }
        public string TemplateLayerName => TemplateLayer.ToString();
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public int TargetCompletionDays { get; set; }
        public int Version { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<OnboardingTemplateTaskResponse> Tasks { get; set; } = new();
    }

    public sealed class OnboardingTemplateListItemResponse
    {
        public Guid Id { get; set; }
        public string TemplateCode { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public OnboardingTemplateLayer TemplateLayer { get; set; }
        public string TemplateLayerName => TemplateLayer.ToString();
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public int TargetCompletionDays { get; set; }
        public int Version { get; set; }
        public int TaskCount { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class OnboardingTemplateQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public OnboardingTemplateLayer? TemplateLayer { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
    }
}
