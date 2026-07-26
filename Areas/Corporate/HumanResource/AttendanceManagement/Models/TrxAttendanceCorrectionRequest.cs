using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    [Table("TrxAttendanceCorrectionRequest", Schema = "public")]
    public class TrxAttendanceCorrectionRequest : IdentityModel
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
        public string CorrectionType { get; set; } = "AttendanceTime";
        // AttendanceTime, MissingPunch, Location, Schedule, Status, BusinessTrip, RemoteAttendance, Other.

        [Required]
        [MaxLength(30)]
        public string RequestStatus { get; set; } = "Draft";
        // Draft, Submitted, UnderReview, Approved, PartiallyApproved, Rejected, Applied, Cancelled.

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
        public TrxAttendanceDaily? AttendanceDaily { get; set; }
        public TrxAttendance? Attendance { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public MstWorkforceProfile? RequestedByWorkforceProfile { get; set; }
        public ApplicationUser? RequestedByUser { get; set; }
        public ApplicationUser? AppliedByUser { get; set; }

        public ICollection<TrxAttendanceCorrectionDetail> Details { get; set; } = new List<TrxAttendanceCorrectionDetail>();
        public ICollection<TrxAttendanceCorrectionApproval> Approvals { get; set; } = new List<TrxAttendanceCorrectionApproval>();
        public ICollection<TrxAttendanceException> Exceptions { get; set; } = new List<TrxAttendanceException>();
    }
}
