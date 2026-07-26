using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("TrxEmploymentCertificateRequest", Schema = "public")]
    public class TrxEmploymentCertificateRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(50)] public string RequestNumber { get; set; } = string.Empty;
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? EmployeeSeparationId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public DateTime RequestDate { get; set; }
        [Required, MaxLength(100)] public string CertificateType { get; set; } = "EmploymentCertificate";
        [MaxLength(30)] public string LanguageCode { get; set; } = "id-ID";
        [MaxLength(500)] public string? Purpose { get; set; }
        [MaxLength(50)] public string DeliveryMethod { get; set; } = "Download";
        public int RequestedCopies { get; set; } = 1;
        public bool IncludeEmploymentPeriod { get; set; } = true;
        public bool IncludePosition { get; set; } = true;
        public bool IncludeSalary { get; set; }
        [MaxLength(30)] public string RequestStatus { get; set; } = "Draft";
        public Guid? RequestedByUserId { get; set; }
        public Guid? IssuedByUserId { get; set; }
        public DateTime? IssuedAt { get; set; }
        [MaxLength(500)] public string? DocumentPath { get; set; }
        [MaxLength(250)] public string? OriginalFileName { get; set; }
        [MaxLength(150)] public string? ContentType { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public TrxEmployeeSeparation? EmployeeSeparation { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? RequestedByUser { get; set; }
        public ApplicationUser? IssuedByUser { get; set; }
    }
}
