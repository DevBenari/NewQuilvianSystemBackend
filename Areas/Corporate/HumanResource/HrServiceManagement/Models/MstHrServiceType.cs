using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models
{
    [Table("MstHrServiceType", Schema = "public")]
    public class MstHrServiceType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid HrServiceCategoryId { get; set; }
        public Guid? EmployeeDocumentTypeId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? DefaultAssignedUserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ServiceTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RequestClassification { get; set; } = "General";

        [Required]
        [MaxLength(50)]
        public string DefaultPriority { get; set; } = "Normal";

        public int DefaultSlaHours { get; set; } = 24;
        public int? FirstResponseSlaHours { get; set; }
        public int? ResolutionSlaHours { get; set; }

        [MaxLength(100)]
        public string? DefaultAssignedRoleCode { get; set; }

        [MaxLength(50)]
        public string AssignmentSource { get; set; } = "SiteHr";

        public bool RequiresWorkflow { get; set; } = false;
        public bool RequiresAttachment { get; set; } = false;
        public bool AllowsEmployeeComment { get; set; } = true;
        public bool AllowsInternalComment { get; set; } = true;
        public bool AutoCreateDocumentRequest { get; set; } = false;
        public bool IsEmployeeSelectable { get; set; } = true;
        public bool IsConfidential { get; set; } = false;
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public string? FormSchemaJson { get; set; }
        public string? ValidationRuleJson { get; set; }

        public MstHrServiceCategory? HrServiceCategory { get; set; }
        public MstEmployeeDocumentType? EmployeeDocumentType { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? DefaultAssignedUser { get; set; }
        public ICollection<TrxHrServiceRequest> ServiceRequests { get; set; } = new List<TrxHrServiceRequest>();
    }
}
