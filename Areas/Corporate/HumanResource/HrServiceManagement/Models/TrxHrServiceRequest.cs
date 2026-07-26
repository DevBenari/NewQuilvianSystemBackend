using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models
{
    [Table("TrxHrServiceRequest", Schema = "public")]
    public class TrxHrServiceRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid HrServiceCategoryId { get; set; }
        public Guid HrServiceTypeId { get; set; }
        public Guid RequestedByWorkforceProfileId { get; set; }
        public Guid? RequestedByEmployeeId { get; set; }
        public Guid RequestedByUserId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public Guid? AssignedToWorkforceProfileId { get; set; }

        [Required]
        [MaxLength(60)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(5000)]
        public string RequestDescription { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string RequestStatus { get; set; } = "Draft";

        [Required]
        [MaxLength(30)]
        public string Priority { get; set; } = "Normal";

        [Required]
        [MaxLength(30)]
        public string SourceChannel { get; set; } = "Web";

        [MaxLength(100)]
        public string? AssignedRoleCode { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedAt { get; set; }
        public DateTime? SlaDueAt { get; set; }
        public DateTime? FirstResponseAt { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public bool IsSlaBreached { get; set; } = false;
        public bool IsEmployeeVisible { get; set; } = true;
        public bool IsConfidential { get; set; } = false;
        public bool RequiresEnhancedAudit { get; set; } = false;

        [MaxLength(100)]
        public string? RequestCorrelationId { get; set; }

        [MaxLength(100)]
        public string? ExternalReferenceNumber { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        [MaxLength(1000)]
        public string? ResolutionSummary { get; set; }

        [MaxLength(1000)]
        public string? CancellationReason { get; set; }

        public string? RequestPayloadJson { get; set; }
        public string? ServiceSnapshotJson { get; set; }
        public bool IsActive { get; set; } = true;

        public MstHrServiceCategory? HrServiceCategory { get; set; }
        public MstHrServiceType? HrServiceType { get; set; }
        public MstWorkforceProfile? RequestedByWorkforceProfile { get; set; }
        public MstEmployee? RequestedByEmployee { get; set; }
        public ApplicationUser? RequestedByUser { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public TrxWorkflowInstance? WorkflowInstance { get; set; }
        public ApplicationUser? AssignedToUser { get; set; }
        public MstWorkforceProfile? AssignedToWorkforceProfile { get; set; }

        public ICollection<TrxHrServiceRequestComment> Comments { get; set; } = new List<TrxHrServiceRequestComment>();
        public ICollection<TrxHrServiceRequestAttachment> Attachments { get; set; } = new List<TrxHrServiceRequestAttachment>();
        public ICollection<TrxEmployeeDocumentRequest> EmployeeDocumentRequests { get; set; } = new List<TrxEmployeeDocumentRequest>();
    }
}
