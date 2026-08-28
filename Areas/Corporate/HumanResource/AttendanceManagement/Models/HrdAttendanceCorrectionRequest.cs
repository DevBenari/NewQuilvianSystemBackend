using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    [Table("HrdAttendanceCorrectionRequest", Schema = "public")]
    public class HrdAttendanceCorrectionRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? AttendanceDailyId { get; set; }
        public Guid? AttendanceId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        public Guid? RequestedByWorkforceProfileId { get; set; }
        public Guid? RequestedByUserId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string CorrectionType { get; set; }
            = AttendanceValueConstants.CorrectionType.AttendanceTime;

        [Required]
        [MaxLength(30)]
        public string RequestStatus { get; set; }
            = AttendanceValueConstants.CorrectionRequestStatus.Draft;

        [Required]
        [MaxLength(1500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? EvidenceFilePath { get; set; }

        [MaxLength(255)]
        public string? EvidenceFileName { get; set; }

        [MaxLength(100)]
        public string? EvidenceContentType { get; set; }

        public string? OriginalSummaryJson { get; set; }
        public string? RequestedSummaryJson { get; set; }
        public string? ApprovedSummaryJson { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
        public Guid? AppliedByUserId { get; set; }

        [MaxLength(1000)]
        public string? FinalNote { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public HrdAttendanceDaily? AttendanceDaily { get; set; }
        public HrdAttendance? Attendance { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public TrxWorkflowInstance? WorkflowInstance { get; set; }
        public MstWorkforceProfile? RequestedByWorkforceProfile { get; set; }
        public ApplicationUser? RequestedByUser { get; set; }
        public ApplicationUser? AppliedByUser { get; set; }

        public ICollection<HrdAttendanceCorrectionDetail> Details { get; set; }
            = new List<HrdAttendanceCorrectionDetail>();

        // Legacy compatibility only. Approval baru memakai Generic Workflow Engine.
        public ICollection<HrdAttendanceCorrectionApproval> Approvals { get; set; }
            = new List<HrdAttendanceCorrectionApproval>();

        public ICollection<HrdAttendanceException> Exceptions { get; set; }
            = new List<HrdAttendanceException>();
    }
}
