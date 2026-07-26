using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models
{
    [Table("MstEmployeeDocumentType", Schema = "public")]
    public class MstEmployeeDocumentType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? WorkflowDefinitionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DocumentTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DocumentTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DocumentCategory { get; set; } = "Employment";

        [MaxLength(100)]
        public string? TemplateCode { get; set; }

        [MaxLength(500)]
        public string? TemplatePath { get; set; }

        [MaxLength(20)]
        public string DefaultLanguageCode { get; set; } = "id-ID";

        public int? DefaultValidityDays { get; set; }
        public bool RequiresApproval { get; set; } = false;
        public bool RequiresDigitalSignature { get; set; } = false;
        public bool AllowsEmployeeDownload { get; set; } = true;
        public bool AllowsMultipleIssuance { get; set; } = true;
        public bool IsConfidential { get; set; } = false;
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public string? RequiredDataSchemaJson { get; set; }

        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ICollection<MstHrServiceType> HrServiceTypes { get; set; } = new List<MstHrServiceType>();
        public ICollection<TrxEmployeeDocumentRequest> DocumentRequests { get; set; } = new List<TrxEmployeeDocumentRequest>();
    }
}
