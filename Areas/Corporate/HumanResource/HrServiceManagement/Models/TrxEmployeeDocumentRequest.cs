using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models
{
    [Table("TrxEmployeeDocumentRequest", Schema = "public")]
    public class TrxEmployeeDocumentRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EmployeeDocumentTypeId { get; set; }
        public Guid RequestedByWorkforceProfileId { get; set; }
        public Guid? RequestedForWorkforceProfileId { get; set; }
        public Guid? RequestedForEmployeeId { get; set; }
        public Guid RequestedByUserId { get; set; }
        public Guid? HrServiceRequestId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public Guid? ProcessedByUserId { get; set; }

        [Required]
        [MaxLength(60)]
        public string DocumentRequestNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string RequestStatus { get; set; } = "Draft";

        [MaxLength(1000)]
        public string? Purpose { get; set; }

        [MaxLength(20)]
        public string LanguageCode { get; set; } = "id-ID";

        public int NumberOfCopies { get; set; } = 1;

        [MaxLength(50)]
        public string DeliveryMethod { get; set; } = "Digital";

        [MaxLength(500)]
        public string? DeliveryAddress { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? NeededByDate { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        [MaxLength(1000)]
        public string? ProcessingNotes { get; set; }

        [MaxLength(1000)]
        public string? CancellationReason { get; set; }

        public string? RequestedDataJson { get; set; }
        public bool IsEmployeeDownloadAllowed { get; set; } = true;
        public bool IsConfidential { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public MstEmployeeDocumentType? EmployeeDocumentType { get; set; }
        public MstWorkforceProfile? RequestedByWorkforceProfile { get; set; }
        public MstWorkforceProfile? RequestedForWorkforceProfile { get; set; }
        public MstEmployee? RequestedForEmployee { get; set; }
        public ApplicationUser? RequestedByUser { get; set; }
        public TrxHrServiceRequest? HrServiceRequest { get; set; }
        public TrxWorkflowInstance? WorkflowInstance { get; set; }
        public ApplicationUser? ProcessedByUser { get; set; }
        public ICollection<TrxEmployeeDocumentIssuance> Issuances { get; set; } = new List<TrxEmployeeDocumentIssuance>();
    }
}
