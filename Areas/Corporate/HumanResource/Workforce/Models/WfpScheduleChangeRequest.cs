using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpScheduleChangeRequest", Schema = "public")]
    public class WfpScheduleChangeRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? CurrentWorkScheduleAssignmentId { get; set; }

        [Required]
        public DateOnly RequestedScheduleDate { get; set; }

        public Guid? RequestedWorkScheduleId { get; set; }

        public ScheduleChangeRequestType RequestType { get; set; } = ScheduleChangeRequestType.Unknown;

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public ScheduleRequestApprovalStatus ApprovalStatus { get; set; } = ScheduleRequestApprovalStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? RejectedByUserId { get; set; }

        public DateTime? RejectedAt { get; set; }

        [MaxLength(250)]
        public string? RejectedReason { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public WfpWorkScheduleAssignment? CurrentWorkScheduleAssignment { get; set; }

        public MstWorkSchedule? RequestedWorkSchedule { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }

        public ApplicationUser? RejectedByUser { get; set; }
    }

    [Table("WfpShiftSwapRequest", Schema = "public")]
    public class WfpShiftSwapRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RequesterWorkforceProfileId { get; set; }

        [Required]
        public Guid TargetWorkforceProfileId { get; set; }

        [Required]
        public Guid RequesterScheduleAssignmentId { get; set; }

        [Required]
        public Guid TargetScheduleAssignmentId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public ScheduleRequestApprovalStatus ApprovalStatus { get; set; } = ScheduleRequestApprovalStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? RejectedByUserId { get; set; }

        public DateTime? RejectedAt { get; set; }

        [MaxLength(250)]
        public string? RejectedReason { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? RequesterWorkforceProfile { get; set; }

        public MstWorkforceProfile? TargetWorkforceProfile { get; set; }

        public WfpWorkScheduleAssignment? RequesterScheduleAssignment { get; set; }

        public WfpWorkScheduleAssignment? TargetScheduleAssignment { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }

        public ApplicationUser? RejectedByUser { get; set; }
    }
}
