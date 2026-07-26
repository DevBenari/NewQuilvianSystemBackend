using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("MstOffboardingTemplateTask", Schema = "public")]
    public class MstOffboardingTemplateTask : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required] public Guid OffboardingTemplateId { get; set; }
        [Required, MaxLength(50)] public string TaskCode { get; set; } = string.Empty;
        [Required, MaxLength(250)] public string TaskName { get; set; } = string.Empty;
        [MaxLength(50)] public string TaskCategory { get; set; } = "General";
        [MaxLength(50)] public string ResponsiblePartyType { get; set; } = "HR";
        public Guid? ResponsibleOrganizationUnitId { get; set; }
        public Guid? ResponsiblePositionId { get; set; }
        public int DueDayOffset { get; set; }
        public bool IsRequired { get; set; } = true;
        public bool RequiresDocument { get; set; }
        public bool RequiresVerification { get; set; }
        [MaxLength(50)] public string CompletionSource { get; set; } = "Manual";
        public int SortOrder { get; set; }
        [MaxLength(1000)] public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public MstOffboardingTemplate? OffboardingTemplate { get; set; }
        public MstOrganizationUnit? ResponsibleOrganizationUnit { get; set; }
        public MstPosition? ResponsiblePosition { get; set; }
    }
}
