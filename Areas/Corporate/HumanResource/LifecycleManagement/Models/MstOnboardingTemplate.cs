using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("MstOnboardingTemplate", Schema = "public")]
    public class MstOnboardingTemplate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string TemplateCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        public OnboardingTemplateLayer TemplateLayer { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public int TargetCompletionDays { get; set; } = 30;

        public int Version { get; set; } = 1;

        [MaxLength(1500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public ICollection<MstOnboardingTemplateTask> Tasks { get; set; } = new List<MstOnboardingTemplateTask>();
    }
}
